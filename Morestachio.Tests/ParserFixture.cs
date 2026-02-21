using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Morestachio.Document.Contracts;
using Morestachio.Document.Custom;
using Morestachio.Document.Items;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Formatter.Framework.Attributes;
using Morestachio.Formatter.Predefined;
using Morestachio.Framework;
using Morestachio.Framework.Context.Resolver;
using Morestachio.Framework.Error;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Expression.Parser;
using Morestachio.Framework.Expression.Visitors;
using Morestachio.Framework.IO;
using Morestachio.Helper;
using Morestachio.Helper.Logging;
using Morestachio.Linq;
using Morestachio.Parsing.ParserErrors;
using Morestachio.Rendering;
using Morestachio.Tests.SerilalizerTests;
using NUnit.Framework;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace Morestachio.Tests
{
	[TestFixture(ParserOptionTypes.UseOnDemandCompile)]
	[TestFixture(ParserOptionTypes.Precompile)]
	[Parallelizable(ParallelScope.All)]
	public class ParserFixture
	{
		private readonly ParserOptionTypes _options;

		public ParserFixture(ParserOptionTypes options)
		{
			_options = options;
		}

		public static IParserOptionsBuilder TestBuilder()
		{
			return ParserOptionsBuilder.New()
				.WithEncoding(DefaultEncoding)
				.WithCultureInfo(DefaultCulture);
		}

		public static Encoding DefaultEncoding { get; set; } = new UnicodeEncoding(true, false, false);
		public static CultureInfo DefaultCulture { get; set; } = CultureInfo.InvariantCulture;

		public static async Task<string> CreateAndParseWithOptions(
			string template,
			object data,
			ParserOptionTypes opt,
			Func<IParserOptionsBuilder, IParserOptionsBuilder> option = null,
			Action<MorestachioDocumentInfo> documentCallback = null,
			Action<MorestachioDocumentResult> documentResultCallback = null,
			CancellationTokenSource cancellationTokenSource = null
		)
		{
			return (await CreateAndParseWithOptionsStream(template, data, opt, option, documentCallback,
				documentResultCallback, cancellationTokenSource))?.Stringify(true, DefaultEncoding);
		}

		private class TestLogger : ILogger
		{
			public bool Enabled { get; set; }

			public void Log(
				string logLevel,
				string eventId,
				string message,
				IDictionary<string, object> data
			)
			{
				if (eventId == LoggingFormatter.TokenizerEventId)
				{
					Assert.Fail($"{logLevel}:{eventId}:{message}");
				}
			}

			/// <inheritdoc />
			public void Log(string logLevel, string eventId, string message)
			{
				if (eventId == LoggingFormatter.TokenizerEventId)
				{
					Assert.Fail($"{logLevel}:{eventId}:{message}");
				}
			}
		}

		public static async Task<IByteCounterStream> CreateAndParseWithOptionsStream(
			string template,
			object data,
			ParserOptionTypes opt,
			Func<IParserOptionsBuilder, IParserOptionsBuilder> option = null,
			Action<MorestachioDocumentInfo> documentCallback = null,
			Action<MorestachioDocumentResult> documentResultCallback = null,
			CancellationTokenSource cancellationTokenSource = null
		)
		{
			var document = await CreateWithOptionsStream(template, opt, option, documentCallback);

			if (document == null)
			{
				return null;
			}

			MorestachioDocumentResult docInfo = null;

			if (documentResultCallback != null)
			{
				docInfo = await document.CreateRenderer().RenderAsync(data, CancellationToken.None);
				documentResultCallback(docInfo);
			}

			var cToken = cancellationTokenSource?.Token ?? CancellationToken.None;

			if (opt.HasFlag(ParserOptionTypes.UseOnDemandCompile))
			{
				docInfo = docInfo ?? await document.CreateRenderer().RenderAsync(data, cToken);

				return docInfo.Stream;
			}

			if (opt.HasFlag(ParserOptionTypes.Precompile))
			{
				var compilation = document.CreateCompiledRenderer();

				return (await compilation.RenderAsync(data, cToken)).Stream;
			}

			return null;
		}

		public static async Task<MorestachioDocumentInfo> CreateWithOptionsStream(
			string template,
			ParserOptionTypes opt,
			Func<IParserOptionsBuilder, IParserOptionsBuilder> option = null,
			Action<MorestachioDocumentInfo> documentCallback = null
		)
		{
			var parserOptionsBuilder = TestBuilder()
				.WithTemplate(template)
				.WithLogger(() => !opt.HasFlag(ParserOptionTypes.NoLoggingTest) ? new TestLogger() : null);
			parserOptionsBuilder = option?.Invoke(parserOptionsBuilder) ?? parserOptionsBuilder;

			var parsingOptions = parserOptionsBuilder
				.Build();
			var document = await Parser.ParseWithOptionsAsync(parsingOptions);

			if (!opt.HasFlag(ParserOptionTypes.NoRerenderingTest) && document.Document != null)
			{
				SerializerTest.AssertDocumentItemIsSameAsTemplate(template, document.Document, parsingOptions);
				//var xml = new SerializerTest(typeof(DocumentSerializerXmlStrategy));
				//xml.SerilalizeAndDeserialize(document.Document);
				TestLocationsInOrder(document);
			}

			documentCallback?.Invoke(document);

			if (document.Document == null)
			{
				foreach (var morestachioError in document.Errors)
				{
					var sb = new StringBuilder();
					morestachioError.Format(sb);
					TestContext.Error.WriteLine(sb.ToString());
				}

				return null;
			}

			return document;
		}

		/// <summary>
		///		Tests all children of the document to be correctly ordered
		/// </summary>
		/// <param name="documentInfo"></param>
		public static void TestLocationsInOrder(MorestachioDocumentInfo documentInfo)
		{
			var api = documentInfo
				.Fluent()
				.SearchForward(f => !(f is MorestachioDocument), true);
			var lastLocation = TextIndex.Unknown;
			var visitor = new ToParsableStringDocumentVisitor(documentInfo.ParserOptions);

			while (api.Context.OperationStatus)
			{
				//TextEdit Documents are virtual because they are translated from multiple sources
				if (api.Context.CurrentNode.Item is TextEditDocumentItem txtEdit)
				{
					api.SearchForward(f => true);
					continue;
				}

				//RemoveAlias Documents must be handled separately as they are virtually added to the end of a block when LET variables are used
				if (api.Context.CurrentNode.Item is RemoveAliasDocumentItem removeAlias)
				{
					var ancestorItem = api.Context.CurrentNode.Ancestor.Item as BlockDocumentItemBase;
					Assert.That(removeAlias.Location.RangeStart, Is.EqualTo(ancestorItem.BlockLocation.RangeStart),
						() => "");
				}
				else if (api.Context.CurrentNode.Item is AliasDocumentItem)
				{
					Assert.That(api.Context.CurrentNode.Item.Location.RangeStart, Is.LessThan(lastLocation), () => "");
				}
				else
				{
					Assert.That(api.Context.CurrentNode.Item.Location.RangeStart, Is.GreaterThan(lastLocation),
						() =>
							$"Positions dont match. Expected last known '{lastLocation}' to be greater then '{api.Context.CurrentNode.Item.Location.RangeEnd}'");
					Assert.That(api.Context.CurrentNode.Ancestor.Item, Is.InstanceOf<IBlockDocumentItem>());
					var parentBlock = api.Context.CurrentNode.Ancestor.Item as IBlockDocumentItem;
					Assert.That(new TextRange(parentBlock.Location.RangeStart, parentBlock.BlockLocation.RangeEnd)
							.Includes(parentBlock.Location),
						Is.True,
						() => $"The item {api.Context.CurrentNode.Ancestor.Item} " +
							$"with its location of {api.Context.CurrentNode.Ancestor.Item.Location} does not seem to be wholly located within its parent" +
							$"of {parentBlock} and location {new TextRange(parentBlock.Location.RangeStart, parentBlock.BlockLocation.RangeEnd)}");
					lastLocation = api.Context.CurrentNode.Item.Location.RangeEnd;
				}

				visitor.StringBuilder.Clear();
				api.SearchForward(f => true);
			}
		}

		private static TextRange TestExpressionLocationsInOrderInternal(
			BlockExpressionDocumentItemBase expDoc,
			TextRange lastLocation
		)
		{
			var exp = expDoc.MorestachioExpression;
			var expStack = new Queue<IMorestachioExpression>();
			expStack.Enqueue(exp);

			while (expStack.Any())
			{
				var nextExpression = expStack.Dequeue();

				if (nextExpression is MorestachioExpressionListBase list)
				{
					foreach (var listExpression in list.Expressions)
					{
						expStack.Enqueue(listExpression);
					}
				}
				else
				{
					if (nextExpression is MorestachioExpression normalExp)
					{
						foreach (var expressionArgument in normalExp.Formats)
						{
							expStack.Enqueue(expressionArgument);
						}
					}

					Assert.That(nextExpression.Location, Is.GreaterThan(lastLocation).Or.EqualTo(lastLocation));
					lastLocation = nextExpression.Location;
				}
			}

			return lastLocation;
		}

		[Test]
		public async Task TestUnpackCanUnpackTask()
		{
			var fromResult = Task.FromResult("string");
			var unpackFormatterTask = await fromResult.UnpackFormatterTask();
			Assert.That(unpackFormatterTask, Is.EqualTo(fromResult.Result));
		}

		[Test]
		[TestCase("d.f", 1)]
		[TestCase("d.f;T", 2, Ignore = "Currently unsupported")] //TODO FIXME
		[TestCase("d.f;T()", 2)]
		[TestCase("d.f;T(); O", 3)]
		[TestCase("d.f; T() ; O", 3)]
		[TestCase("d.f; T() ; O; 'TT'", 4)]
		[TestCase("d.f; T() ; 't'", 3)]
		[TestCase("d.f; T() ; 't'; a", 4)]
		[TestCase("'d'; T() ; 't'; a", 4)]
		[TestCase("''; T() ; 't'; a", 4)]
		public void ExpressionParserCanParseMany(string expression, int expected)
		{
			var context = TokenzierContext.FromText(expression);
			var expr = new List<IMorestachioExpression>();
			var textIndex = TextRange.RangeIndex(context, 0, 0);
			var endIndex = TextIndex.End(expression);

			while (!textIndex.Includes(endIndex))
			{
				var expressionParserResult = ExpressionParser
					.ParseExpression(expression, context, new TextRange(textIndex.RangeEnd, endIndex));
				textIndex = expressionParserResult.SourceBoundary;
				expr.Add(expressionParserResult.Expression);
			}

			Assert.That(context.Errors, Is.Empty);
			Assert.That(expr.Count, Is.EqualTo(expected));
			var ses = expression.Split(';');

			for (var index = 0; index < ses.Length; index++)
			{
				var s = ses[index];
				var expPart = s.Trim();
				var exp = expr[index];

				var visitor = new ToParsableStringExpressionVisitor
				{
					TrimEoexDelimiters = true
				};
				visitor.Visit(exp);
				Assert.That(visitor.StringBuilder.ToString(), Is.EqualTo(expPart));
			}
		}

		[Test]
		[TestCase("(('').A())")]
		[TestCase("a(b(c().d()))")]
		[TestCase("f((A + B), C)")]
		[TestCase("f((), C)")]
		[TestCase("f((''), C)")]
		[TestCase("(f('')).Test")]
		[TestCase("((A.F()).T)")]
		[TestCase("f(fe().P).Fg()")]
		[TestCase("f(fe().P).Fg().DE()")]
		[TestCase("f(fe().P.Add(' ')).Fg().DE()")]
		[TestCase("f(fe().P.Add(' ')).Fg().DE()")]
		[TestCase("d.f")]
		[TestCase("d.f()")]
		[TestCase("d.f().Test")]
		[TestCase("d.f(fA)")]
		[TestCase("d.f(fA).Test")]
		[TestCase("d.f(fA, fb()).Test")]
		[TestCase("d.f(fA, fb.TS()).Test")]
		[TestCase("d.f(fA.TS())")]
		[TestCase("d.f(fA(''))")]
		[TestCase("d.f(fA('').Test)")]
		[TestCase("d.f(fA('').A().D())")]
		[TestCase("d.f(fA('').fB.A())")]
		[TestCase("d.f(fA('').fB.A()).Test")]
		[TestCase("d.f(fA('').fB.A()).Test.Data")]
		[TestCase("d.f(fA('').fB.A()).Test.fC()")]
		[TestCase("d.f(fA.TS('', e))")]
		[TestCase("d.f(fA.TS('', e), (A + B))")]
		[TestCase("d.f(fA.TS('', e), (A))")]
		[TestCase("d.f(fA.TS('', e), ())")]
		[TestCase("d.f(fA.TS('', e), (A + (D)))")]
		[TestCase("d.f(fA.TS('', e), (A + (D) + d))")]
		[TestCase("d.f(fA.TS('', e), (A + (D + 4) + d))")]
		[TestCase("d.f(fA.TS('', e), (A + Delta(D) + d))")]
		[TestCase("d.f(fA.TS('', e), (A + Delta(D + 4) + d))")]
		[TestCase("d.f(fA.TS('', e), (A + Delta((D + 4), A) + d))")]
		[TestCase("d.f(fA.TS('', e()))")]
		[TestCase("d.f(fA.TS('', e('')))")]
		[TestCase("d.f(fA.TS('d'))")]
		[TestCase("d.f(fA.TS('d').Test)")]
		[TestCase("d.f(fA.TS('d').fB())")]
		[TestCase("d.f(fA.TS('d').fB()).Test")]
		[TestCase("d.f(fA.TS('d').fB()).Test.Data")]
		[TestCase("d.f(fA.TS('d').fB()).Test.fC()")]
		[TestCase("d.f(fA.TS('d').fB('')).Test.fC()")]
		[TestCase("d.f(fA.TS('d').fB('')).Test.fC('')")]
		[TestCase("d.f(fA.TS('d').fB('d')).Test.fC('')")]
		[TestCase("d.f(fA.TS('d').fB('d')).Test.fC('d')")]
		[TestCase("d.f(fA.TS('d', f).fB('d')).Test.fC('d')")]
		[TestCase("d.f(fA.TS('d', f).fB('d', f)).Test.fC('d')")]
		[TestCase("d.f(fA.TS('d', f).fB('d', f)).Test.fC('d', f)")]
		[TestCase("d.f(fA.TS('d', f.TS()).fB('d', f)).Test.fC('d', f)")]
		[TestCase("d.f(fA.TS('d', f.TS()).fB('d', f.TS())).Test.fC('d', f.TS())")]
		[TestCase("(d.f(fA.TS('d', f.TS()).fB('d', f.TS())).Test.fC('d', f.TS()))")]
		[TestCase("(d.f((fA.TS('d', f.TS())).fB('d', f.TS())).Test.fC('d', f.TS()))")]
		public void TestExpressionParser(string query)
		{
			var context = TokenzierContext.FromText(query);
			var expressions = ExpressionParser.ParseExpression(query, context).Expression;
			Assert.That(expressions, Is.Not.Null, () => context.Errors.GetErrorText());
			var visitor = new ToParsableStringExpressionVisitor();
			expressions.Accept(visitor);
			Assert.That(visitor.StringBuilder.ToString(), Is.EqualTo(query));
			Assert.That(context.Errors, Is.Empty, () => context.Errors.GetErrorText());
		}

		[Test]
		[TestCase("'te' + 'st'")]
		public void TestExpressionParserDbg(string query)
		{
			var context = TokenzierContext.FromText(query);
			var expressions = ExpressionParser.ParseExpression(query, context).Expression;
			Assert.That(expressions, Is.Not.Null);
			var visitor = new ToParsableStringExpressionVisitor();
			expressions.Accept(visitor);
			Assert.That(visitor.StringBuilder.ToString(), Is.EqualTo(query));
			Assert.That(context.Errors, Is.Empty, () => context.Errors.GetErrorText());
		}

		[Test]
		[TestCase("A + B", 5 + 10, 5, 10)]
		[TestCase("'te' + 'st'", "test")]
		[TestCase("(A + B)", 5 + 10, 5, 10)]
		[TestCase("((A + B))", 5 + 10, 5, 10)]
		[TestCase("A - B", 5 - 10, 5, 10)]
		[TestCase("A / B", 5 / 10, 5, 10)]
		[TestCase("A * B", 5 * 10, 5, 10)]
		[TestCase("A % B", 5 % 10, 5, 10)]
		[TestCase("A << B", 5 << 10, 5, 10)]
		[TestCase("A >> B", 5 >> 10, 5, 10)]
		[TestCase("A < B", 5 < 10, 5, 10)]
		[TestCase("A > B", 5 > 10, 5, 10)]
		[TestCase("A <= B", 5 <= 10, 5, 10)]
		[TestCase("A >= B", 5 >= 10, 5, 10)]
		[TestCase("A == B", 5 == 10, 5, 10)]
		[TestCase("A != B", 5 != 10, 5, 10)]
		[TestCase("(A) + (B)", 5 + 10, 5, 10)]
		[TestCase("((A) + (B))", 5 + 10, 5, 10)]
		[TestCase("(A) - (B)", 5 - 10, 5, 10)]
		[TestCase("(A) / (B)", 5 / 10, 5, 10)]
		[TestCase("(A) * (B)", 5 * 10, 5, 10)]
		[TestCase("(A) % (B)", 5 % 10, 5, 10)]
		[TestCase("(A) << (B)", 5 << 10, 5, 10)]
		[TestCase("(A) >> (B)", 5 >> 10, 5, 10)]
		[TestCase("(A) < (B)", 5 < 10, 5, 10)]
		[TestCase("(A) > (B)", 5 > 10, 5, 10)]
		[TestCase("(A) <= (B)", 5 <= 10, 5, 10)]
		[TestCase("(A) >= (B)", 5 >= 10, 5, 10)]
		[TestCase("(A) == (B)", 5 == 10, 5, 10)]
		[TestCase("(A) != (B)", 5 != 10, 5, 10)]
		[TestCase("A + B + C", 5 + 10 + 15, 5, 10, 15)]
		[TestCase("A - B - C", 5 - 10 - 15, 5, 10, 15)]
		[TestCase("A / B / C", 5 / 10 / 15, 5, 10, 15)]
		[TestCase("A * B * C", 5 * 10 * 15, 5, 10, 15)]
		[TestCase("A % B % C", 5 % 10 % 15, 5, 10, 15)]
		[TestCase("A << B << C", 5 << 10 << 15, 5, 10, 15)]
		[TestCase("A >> B >> C", 5 >> 10 >> 15, 5, 10, 15)]
		[TestCase("(A + B) + C", 5 + 10 + 15, 5, 10, 15)]
		[TestCase("(A - B) - C", 5 - 10 - 15, 5, 10, 15)]
		[TestCase("(A / B) / C", 5 / 10 / 15, 5, 10, 15)]
		[TestCase("(A * B) * C", 5 * 10 * 15, 5, 10, 15)]
		[TestCase("(A % B) % C", 5 % 10 % 15, 5, 10, 15)]
		[TestCase("(A << B) << C", 5 << 10 << 15, 5, 10, 15)]
		[TestCase("(A >> B) >> C", 5 >> 10 >> 15, 5, 10, 15)]
		[TestCase("A + (B + C)", 5 + 10 + 15, 5, 10, 15)]
		[TestCase("(A + (B + C))", 5 + 10 + 15, 5, 10, 15)]
		[TestCase("(A + (B) + C)", 5 + 10 + 15, 5, 10, 15)]
		[TestCase("A - (B - C)", 5 - (10 - 15), 5, 10, 15)]
		[TestCase("A / (B / C)", 5 / (22 / 15), 5, 22, 15)]
		[TestCase("A * (B * C)", 5 * 10 * 15, 5, 10, 15)]
		[TestCase("A % (B % C)", 5 % (10 % 15), 5, 10, 15)]
		[TestCase("A << (B << C)", 5 << (10 << 15), 5, 10, 15)]
		[TestCase("A >> (B >> C)", 5 >> (10 >> 15), 5, 10, 15)]
		[TestCase("(A + (B + C))", 5 + 10 + 15, 5, 10, 15)]
		[TestCase("(A - (B - C))", 5 - (10 - 15), 5, 10, 15)]
		[TestCase("(A / (B / C))", 5 / (22 / 15), 5, 22, 15)]
		[TestCase("(A * (B * C))", 5 * 10 * 15, 5, 10, 15)]
		[TestCase("(A % (B % C))", 5 % (10 % 15), 5, 10, 15)]
		[TestCase("(A << (B << C))", 5 << (10 << 15), 5, 10, 15)]
		[TestCase("(A >> (B >> C))", 5 >> (10 >> 15), 5, 10, 15)]
		[TestCase("(((A) >> ((B) >> C)))", 5 >> (10 >> 15), 5, 10, 15)]
		public async Task TestExpressionCanParseOperators(string query, object valExp, params object[] args)
		{
			var context = TokenzierContext.FromText(query);
			var expressions = ExpressionParser.ParseExpression(query, context).Expression;
			Assert.That(expressions, Is.Not.Null, () => context.Errors.GetErrorText());
			Assert.That(context.Errors, Is.Empty, () => context.Errors.GetErrorText());
			var visitor = new ToParsableStringExpressionVisitor();
			expressions.Accept(visitor);
			var actual = visitor.StringBuilder.ToString();
			Assert.That(actual, Is.EqualTo(query));
			var template = "{{" + query + "}}";
			var data = new Dictionary<string, object>();

			for (var index = 0; index < args.Length; index++)
			{
				var arg = args[index];
				data.Add(((char)('A' + index)).ToString(), arg);
			}

			var result = await CreateAndParseWithOptions(template, data, _options, options =>
			{
				return options;
				//options.WithFormatterGlobal<object, object>(f =>
				//{
				//	return f;
				//}, "Self");
			});
			Assert.That(result, Is.EqualTo(valExp.ToString()));
		}

		[Test]
		[TestCase("d")]
		[TestCase("D")]
		[TestCase("f")]
		[TestCase("F")]
		[TestCase("dd,,MM,,YYY")]
		public async Task ParserCanFormat(string dtFormat)
		{
			var template = "{{data.ToString(\"" + dtFormat + "\")}},{{data}}";
			var dataValue = DateTime.UtcNow;
			var data = new Dictionary<string, object> { { "data", dataValue } };
			var result = await CreateAndParseWithOptions(template, data, _options);
			Assert.That(result, Is.EqualTo(dataValue.ToString(dtFormat, DefaultCulture) + "," + dataValue));
		}

		[Test]
		public async Task ParserCanParseJsonLikeText()
		{
			var template = @"{
	{{data}}
}";

			var data = new Dictionary<string, object>
			{
				{ "data", "test" }
			};
			var result = await CreateAndParseWithOptions(template, data, _options);

			Assert.That(result, Is.EqualTo(@"{
	test
}"));
		}

		[Test]
		public async Task ParserCanVariableStaticExpression()
		{
			var template = "{{#var f = data}}|{{f}}";

			var data = new Dictionary<string, object>
			{
				{ "data", "test" }
			};
			var result = await CreateAndParseWithOptions(template, data, _options);
			Assert.That(result, Is.EqualTo("|test"));
		}

		[Test]
		public async Task ParserCanVariableExpression()
		{
			var template = "{{#var f = data.ToString('G')}}|{{f}}";
			var dataValue = DateTime.Now;

			var data = new Dictionary<string, object>
			{
				{ "data", dataValue }
			};
			var result = await CreateAndParseWithOptions(template, data, _options);
			Assert.That(result, Is.EqualTo("|" + dataValue.ToString("G", DefaultCulture)));
		}

		[Test]
		public async Task ParserCanVariableExpressionWithFormats()
		{
			var template = "{{#VAR f = data.ToString('G').PadLeft(123)}}|{{f}}";
			var dataValue = DateTime.Now;

			var data = new Dictionary<string, object>
			{
				{ "data", dataValue }
			};
			var result = await CreateAndParseWithOptions(template, data, _options,
				options =>
				{
					return options.WithFormatter((string value, int nr) => { return value.PadLeft(nr); }, "PadLeft");
				});
			Assert.That(result, Is.EqualTo("|" + dataValue.ToString("G", DefaultCulture).PadLeft(123)));
		}

		[Test]
		public async Task ParserCanVariableSetToOtherVariable()
		{
			var template = "{{#var f = data}}" +
				"{{#var e = f.ToString('G')}}" +
				"{{e.PadLeft(123)}}";
			var dataValue = DateTime.Now;

			var data = new Dictionary<string, object>
			{
				{ "data", dataValue }
			};

			var result = await CreateAndParseWithOptions(template, data, _options, options =>
			{
				return options.WithFormatter((string value, int nr) => { return value.PadLeft(nr); }, "PadLeft");
				;
			});
			Assert.That(result, Is.EqualTo(dataValue.ToString("G", DefaultCulture).PadLeft(123)));
		}

		[Test]
		public async Task ParserCanVariableScope()
		{
			var template =
				@"{{#VAR global = data |--}}
{{#SCOPE data |--}}
		{{global |--}}
		{{#LET global = 'Burns ' |--}}
		{{global |--}}
		{{#LET global = 'Likes ' |--}}
		{{global |--}}
		{{#LET local = 'Likes ' |--}}
		{{#VAR global = 'Miss ' |--}}
{{/SCOPE |--}}
{{local |--}}
{{global |--}}
{{#VAR global = 'Money ' |--}}
{{global |--}}
{{#LET global = 'Alot' |--}}
{{global |--}}";

			var data = new Dictionary<string, object>
			{
				{ "data", "Mr " }
			};
			var result = await CreateAndParseWithOptions(template, data, _options | ParserOptionTypes.NoLoggingTest);
			Assert.That(result, Is.EqualTo("Mr Burns Likes Miss Money Alot"));
		}

		[Test]
		public async Task ParserCanVariableScopeIsolation()
		{
			var template =
				@"{{#VAR global = data |--}}
{{#ISOLATE #VARIABLES |--}}
{{global |--}}
{{#VAR global = 'Bu-erns ' |--}}
{{global |--}}
{{/ISOLATE |--}}
{{global.Trim()}}";

			var data = new Dictionary<string, object>
			{
				{ "data", "Mr " }
			};
			var result = await CreateAndParseWithOptions(template, data, _options | ParserOptionTypes.NoLoggingTest);
			Assert.That(result, Is.EqualTo("Mr Bu-erns Mr"));
		}

		[Test]
		public async Task ParserCanVariableSetToNull()
		{
			var template = "{{#var f = data}}" +
				"{{#var f = null}}" +
				"{{f.PadLeft(123)}}";
			var dataValue = DateTime.Now;

			var data = new Dictionary<string, object>
			{
				{ "data", dataValue }
			};
			var result = await CreateAndParseWithOptions(template, data, _options,
				options =>
				{
					return options.WithFormatter((string value, int nr) => { return value.PadLeft(nr); }, "PadLeft")
						.WithNull("{NULL}");
				});
			Assert.That(result, Is.EqualTo("{NULL}"));
		}

		[Test]
		public async Task ParserCanSetVariableNull()
		{
			var template = "{{dataA}}" +
				"{{#var $null = 'DD'}}" +
				"{{aataA}}";

			var data = new Dictionary<string, object>
			{
				{ "data", "dataValue" }
			};
			var result = await CreateAndParseWithOptions(template, data, _options,
				options => { return options.WithNull("{NULL}"); });
			Assert.That(result, Is.EqualTo("{NULL}DD"));
		}

		[Test]
		public async Task ParserCanVariableSetToString()
		{
			var template = "{{#var f = data}}" +
				"{{#var f = 'Test'}}" +
				"{{f.PadLeft(123)}}";
			var dataValue = DateTime.Now;

			var data = new Dictionary<string, object>
			{
				{ "data", dataValue }
			};
			var result = await CreateAndParseWithOptions(template, data, _options,
				options =>
				{
					return options.WithFormatter((string value, int nr) => { return value.PadLeft(nr); }, "PadLeft");
				});
			Assert.That(result, Is.EqualTo("Test".PadLeft(123)));
		}

		[Test]
		public async Task ParserCanVariableSetToStringWithEscaptedValues()
		{
			var template = "{{#var f = data}}" +
				"{{#var f = 'Te\\'st'}}" +
				"{{f.PadLeft(123)}}";
			var dataValue = DateTime.Now;

			var data = new Dictionary<string, object>
			{
				{ "data", dataValue }
			};
			var result = await CreateAndParseWithOptions(template, data, _options,
				options =>
				{
					return options.WithFormatter((string value, int nr) => { return value.PadLeft(nr); }, "PadLeft")
						.WithDisableContentEscaping(true);
				});
			Assert.That(result, Is.EqualTo("Te'st".PadLeft(123)));
		}

		[Test]
		public async Task ParserCanVariableSetToEmptyString()
		{
			var template = "{{#var f = ''}}" +
				"{{f.PadLeft(123)}}";
			var dataValue = DateTime.Now;

			var data = new Dictionary<string, object>
			{
				{ "data", dataValue }
			};
			var result = await CreateAndParseWithOptions(template, data, _options,
				options =>
				{
					return options.WithFormatter((string value, int nr) => { return value.PadLeft(nr); }, "PadLeft");
				});
			Assert.That(result, Is.EqualTo("".PadLeft(123)));
		}

		[Test]
		[Ignore("Support for this feature is retracted")]
		public async Task ParserCanSetOption()
		{
			var template = "{{valueA}}," +
				"{{#SET OPTION TokenPrefix = '<%'}}" +
				"<%#SET OPTION TokenSuffix = '%>'}}" +
				"{{valueA}}" +
				"<%valueB%>," +
				"<%#SET OPTION TokenPrefix = '{{'%>" +
				"{{#SET OPTION TokenSuffix = '}}'%>" +
				"{{valueC}}";

			var data = new Dictionary<string, object>
			{
				{ "valueA", "Hello" },
				{ "valueB", "_" },
				{ "valueC", "World" }
			};
			var result = await CreateAndParseWithOptions(template, data,
				_options | ParserOptionTypes.NoRerenderingTest);
			Assert.That(result, Is.EqualTo("Hello,{{valueA}}_,World"));
		}

		[Test]
		public async Task ParserCanNullableFormatTest()
		{
			var template = "ShouldBe: {{data}}, ButNot: {{extData}}";

			var data = new Dictionary<string, object>
			{
				{ "data", null },
				{ "extData", "Test" }
			};
			var result = await CreateAndParseWithOptions(template, data, _options,
				options => { return options.WithNull("IAmTheLaw!"); });
			Assert.That(result, Is.EqualTo("ShouldBe: IAmTheLaw!, ButNot: Test"));
		}

		[Test]
		public async Task ParserCanParseIntegerNumber()
		{
			var template = "{{1111}}";
			var data = new Dictionary<string, object>();
			var result = await CreateAndParseWithOptions(template, data, _options);
			Assert.That(result, Is.EqualTo("1111"));
		}

		[Test]
		public async Task ParserCanParseNumberAsFormatterArg()
		{
			var template = "{{f.Format(123)}}";

			var data = new Dictionary<string, object>
			{
				{ "f", "F5" }
			};
			var result = await CreateAndParseWithOptions(template, data, _options,
				options =>
				{
					return options.WithFormatter(new Func<string, int, string>((e, f) => f.ToString(e)), "Format");
				});
			Assert.That(result, Is.EqualTo(123.ToString("F5")));
		}

		[Test]
		public async Task ParserCanParseFloatingNumber()
		{
			var template = "{{1.123.ToString('F5')}}";
			var data = new Dictionary<string, object>();
			var result = await CreateAndParseWithOptions(template, data,
				_options | ParserOptionTypes.NoRerenderingTest);
			Assert.That(result, Is.EqualTo(1.123.ToString("F5", DefaultCulture)));
		}

		[Test]
		public async Task TestRootAccess()
		{
			var template = "{{#EACH data.testList.Select()}}" +
				"{{#VAR eachValue = ~data.testInt}}" +
				"{{/EACH}}" +
				"{{eachValue}} = {{~data.testInt}} = {{data.testInt}}";

			var data = new Dictionary<string, object>
			{
				{
					"data", new Dictionary<string, object>
					{
						{
							"testList",
							new string[1]
							{
								string.Empty
							}
						},
						{
							"testInt",
							2
						}
					}
				}
			};
			var result = await CreateAndParseWithOptions(template, data, _options,
				options =>
				{
					return options.WithFormatters(typeof(DynamicLinq))
						.WithFormatters(typeof(FormatterTests.NumberFormatter));
				});
			Assert.That(result, Is.EqualTo("2 = 2 = 2"));
		}

		[Test]
		public async Task TestMethodAsArgumentAccess()
		{
			var template = "{{data.Multiply(data.Multiply(data))}}";

			var data = new Dictionary<string, object>
			{
				{ "data", 2 }
			};
			var result = await CreateAndParseWithOptions(template, data, _options,
				options => { return options.WithFormatters(typeof(NumberFormatter)); });
			Assert.That(result, Is.EqualTo("8"));
		}

		public static class NumberFormatter
		{
			[MorestachioFormatter("Multiply", "XXX")]
			public static decimal Multiply(object value, object value2)
			{
				decimal a = 0;
				decimal.TryParse(value.ToString(), out a);
				decimal b = 0;
				decimal.TryParse(value2.ToString(), out b);

				return a * b;
			}
		}

		[Test]
		[TestCase("d")]
		[TestCase("D")]
		[TestCase("f")]
		[TestCase("F")]
		[TestCase("dd,,MM,,YYY")]
		public async Task ParserCanSelfFormat(string dtFormat)
		{
			var template = "{{#SCOPE data}}{{ToString(\"" + dtFormat + "\")}}{{/SCOPE}},{{data}}";
			var dataValue = DateTime.UtcNow;
			var data = new Dictionary<string, object> { { "data", dataValue } };
			var result = await CreateAndParseWithOptions(template, data, _options,
				options => { return options.WithFormatters(typeof(NumberFormatter)); });
			Assert.That(result, Is.EqualTo(dataValue.ToString(dtFormat, DefaultCulture) + "," + dataValue));
		}

		[Test]
		[TestCase("{{data.F(d))}}", 1)]
		[TestCase("{{data.F((d)}}", 1)]
		[TestCase("{{data)}}", 1)]
		[TestCase("{{data.F(}}", 1)]
		[TestCase("{{data.F(arg}}", 1)]
		[TestCase("{{data.F(arg, 'test'}}", 1)]
		[TestCase("{{data.F(arg, 'test)}}", 1)]
		[TestCase("{{data.F(arg, )}}", 1,
			Ignore = "This is currently auto corrected as the missing argument is interpreted as not existing")]
		public void ParserThrowsAnExceptionWhenFormatIsMismatched(string invalidTemplate, int expectedNoOfExceptions)
		{
			IEnumerable<IMorestachioError> errors;

			Assert.That(errors = Parser.ParseWithOptions(TestBuilder().WithTemplate(invalidTemplate).Build()).Errors,
				Is.Not.Empty,
				() => errors.Select(e => e.HelpText).DefaultIfEmpty("")
					.Aggregate((e, f) => e + Environment.NewLine + f));
		}

		[Test]
		[TestCase("{{#ACollection}}{{.}}{{/each}}")]
		[TestCase("{{#ACollection}}{{.}}{{/ACollection}}{{/each}}")]
		[TestCase("{{#IF ACollection}}{{.}}{{/IF}}{{/each}}")]
		[TestCase("{{/each}}")]
		public void ParserThrowsAnExceptionWhenEachIsMismatched(string invalidTemplate)
		{
			Assert.That(Parser.ParseWithOptions(TestBuilder().WithTemplate(invalidTemplate).Build()).Errors,
				Is.Not.Empty);
		}

		[Test]
		[TestCase("{{Mike", "{{{{name}}")]
		[TestCase("{Mike", "{{{name}}")]
		[TestCase("Mike}", "{{name}}}")]
		[TestCase("Mike}}", "{{name}}}}")]
		public void ParserHandlesPartialOpenAndPartialClose(string expected, string template)
		{
			var model = new Dictionary<string, object>();
			model["name"] = "Mike";

			Assert.That(
				Parser.ParseWithOptions(TestBuilder().WithTemplate(template).Build())
					.CreateRenderer()
					.Render(model)
					.Stream
					.Stringify(true, DefaultEncoding),
				Is.EqualTo(expected));
		}

		[Test]
		[TestCase("{{#each element}}{{name}}")]
		[TestCase("{{#element}}{{name}}")]
		[TestCase("{{^element}}{{name}}")]
		[TestCase("{{#IF element}}{{name}}")]
		public void ParserThrowsParserExceptionForUnclosedGroups(string template)
		{
			Assert.That(Parser.ParseWithOptions(TestBuilder().WithTemplate(template).Build()).Errors,
				Is.Not.Empty.And.Count.EqualTo(1));
		}

		[Test]
		[TestCase("{{..../asdf.content}}")]
		[TestCase("{{asdf.content.~}}")]
		[TestCase("{{asdf.content../}}")]
		[TestCase("{{/}}")]
		[TestCase("{{./}}")]
		[TestCase("{{.. }}")]
		[TestCase("{{..}}")]
		[TestCase("{{...}}")]
		[TestCase("{{//}}")]
		[TestCase("{{@}}")]
		[TestCase("{{[}}")]
		[TestCase("{{]}}")]
		[TestCase("{{)}}")]
		[TestCase("{{(}}")]
		[TestCase("{{%}}")]
		public void ParserShouldThrowForInvalidPaths(string template, int noOfErrors = 1)
		{
			Assert.That(Parser.ParseWithOptions(TestBuilder().WithTemplate(template).Build()).Errors,
				Is.Not.Empty);
		}

		[Test]
		[TestCase("{{first_name}}")]
		[TestCase("{{company.name}}")]
		[TestCase("{{company.address_line_1}}")]
		[TestCase("{{name}}")]
		public void ParserShouldNotThrowForValidPath(string template)
		{
			Parser.ParseWithOptions(TestBuilder().WithTemplate(template).Build());
		}

		//[Test]
		//[TestCase("1{{first name}}", 1)]
		//[TestCase("ss{{#each company.name}}\nasdf", 1)]
		//[TestCase("xzyhj{{#company.address_line_1}}\nasdf{{dsadskl-sasa@}}\n{{/each}}", 2)]
		//[TestCase("fff{{#each company.address_line_1}}\n{{dsadskl-sasa@}}\n{{/each}}", 1)]
		//[TestCase("a{{name}}dd\ndd{{/each}}dd", 1)]
		//public void ParserShouldThrowWithTextRangeInformation(string template, int expectedErrorCount)
		//{
		//	Assert.That(Parser.ParseWithOptions(new ParserOptions(template)).Errors,
		//		Is.Not.Empty.And.Count.EqualTo(expectedErrorCount));
		//}

		[Test]
		[TestCase("<wbr>", "{{content}}", "&lt;wbr&gt;")]
		[TestCase("<wbr>", "{{&content}}", "<wbr>")]
		public async Task ValueEscapingIsActivatedBasedOnValueInterpolationMustacheSyntax(
			string content,
			string template,
			string expected
		)
		{
			var data = new Dictionary<string, object>
			{
				{ "content", content }
			};
			var result = await CreateAndParseWithOptions(template, data, _options,
				options => { return options.WithFormatters(typeof(NumberFormatter)); });
			Assert.That(result, Is.EqualTo(expected));
		}

		[Test]
		[TestCase("<wbr>", "{{content}}", "<wbr>")]
		[TestCase("<wbr>", "{{&content}}", "<wbr>")]
		public async Task ValueEscapingIsDisabledWhenRequested(string content, string template, string expected)
		{
			var data = new Dictionary<string, object>
			{
				{ "content", content }
			};
			var result = await CreateAndParseWithOptions(template, data, _options,
				options =>
				{
					return options.WithFormatters(typeof(NumberFormatter)).WithDisableContentEscaping(true);
				});
			Assert.That(result, Is.EqualTo(expected));
		}

		[Test]
		public async Task ParserCanParseEmailAcidTest()
		{
			#region Email ACID Test Body:

			var emailACIDTest = @"
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"">
<head>
<title></title>
<meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"" />
<meta http-equiv=""Content-Language"" content=""en-us"" />
<style type=""text/css"" media=""screen"">

	/* common
	--------------------------------------------------*/

	body {
		margin: 0px;
		padding: 0px;
		color: #fff;
		background: #930;
	}
	#BodyImposter {
		color: #fff;
		background: #930 url(""img/bgBody.gif"") repeat-x;
		background-color: #930;
		font-family: Arial, Helvetica, sans-serif;
		width: 100%;
		margin: 0px;
		padding: 0px;
		text-align: center;
	}
	#BodyImposter dt {
		font-size: 14px;
		line-height: 1.5em;
		font-weight: bold;
	}
	#BodyImposter dd,
	#BodyImposter li,
	#BodyImposter p,
	#WidthHeight span {
		font-size: 12px;
		line-height: 1.5em;
	}
	#BodyImposter dd,
	#BodyImposter dt {
		margin: 0px;
		padding: 0px;
	}
	#BodyImposter dl,
	#BodyImposter ol,
	#BodyImposter p,
	#BodyImposter ul {
		margin: 0px 0px 4px 0px;
		padding: 10px;
		color: #fff;
		background: #ad5c33;
	}
	#BodyImposter small {
		font-size: 11px;
		font-style: italic;
	}
	#BodyImposter ol li {
		margin: 0px 0px 0px 20px;
		padding: 0px;
	}
	#BodyImposter ul#BulletBg li {
		background: url(""img/bullet.gif"") no-repeat 0em 0.2em;
		padding: 0px 0px 0px 20px;
		margin: 0px;
		list-style: none;
	}
	#BodyImposter ul#BulletListStyle li {
		margin: 0px 0px 0px 22px;
		padding: 0px;
		list-style: url(""img/bullet.gif"");
	}

	/* links
	--------------------------------------------------*/

	#BodyImposter a {
		text-decoration: underline;
	}
	#BodyImposter a:link,
	#BodyImposter a:visited {
		color: #dfb8a4;
		background: #ad5c33;
	}
	#ButtonBorders a:link,
	#ButtonBorders a:visited {
		color: #fff;
		background: #892e00;
	}
	#BodyImposter a:hover {
		text-decoration: none;
	}
	#BodyImposter a:active {
		color: #000;
		background: #ad5c33;
		text-decoration: none;
	}

	/* heads
	--------------------------------------------------*/

	#BodyImposter h1,
	#BodyImposter h2,
	#BodyImposter h3 {
		color: #fff;
		background: #ad5c33;
		font-weight: bold;
		line-height: 1em;
		margin: 0px 0px 4px 0px;
		padding: 10px;
	}
	#BodyImposter h1 {
		font-size: 34px;
	}
	#BodyImposter h2 {
		font-size: 22px;
	}
	#BodyImposter h3 {
		font-size: 16px;
	}
	#BodyImposter h1:hover,
	#BodyImposter h2:hover,
	#BodyImposter h3:hover,
	#BodyImposter dl:hover,
	#BodyImposter ol:hover,
	#BodyImposter p:hover,
	#BodyImposter ul:hover {
		color: #fff;
		background: #892e00;
	}

	/* boxes
	--------------------------------------------------*/

	#Box {
		width: 470px;
		margin: 0px auto;
		padding: 40px 20px;
		text-align: left;
	}
	p#ButtonBorders {
		clear: both;
		color: #fff;
		background: #892e00;
		border-top: 10px solid #ad5c33;
		border-right: 1px dotted #ad5c33;
		border-bottom: 1px dashed #ad5c33;
		border-left: 1px dotted #ad5c33;
	}
	p#ButtonBorders a#Arrow {
		padding-right: 20px;
		background: url(""img/arrow.gif"") no-repeat right 2px;
	}
	p#ButtonBorders a {
		color: #fff;
		background-color: #892e00;
	}
	p#ButtonBorders a#Arrow:hover {
		background-position: right -38px;
	}
	#Floater {
		width: 470px;
	}
	#Floater #Left {
		float: left;
		width: 279px;
		height: 280px;
		color: #fff;
		background: #892e00;
		margin-bottom: 4px;
	}
	#Floater #Right {
		float: right;
		width: 187px;
		height: 280px;
		color: #fff;
		background: #892e00 url(""img/ornament.gif"") no-repeat right bottom;
		margin-bottom: 4px;
	}
	#Floater #Right p {
		color: #fff;
		background: transparent;
	}
	#FontInheritance {
		font-family: Georgia, Times, serif;
	}
	#MarginPaddingOut {
		padding: 20px;
	}
	#MarginPaddingOut #MarginPaddingIn {
		padding: 15px;
		color: #fff;
		background: #ad5c33;
	}
	#MarginPaddingOut #MarginPaddingIn img {
		background: url(""img/bgPhoto.gif"") no-repeat;
		padding: 15px;
	}
	span#SerifFont {
		font-family: Georgia, Times, serif;
	}
	p#QuotedFontFamily {
		font-family: ""Trebuchet MS"", serif;
	}
	#WidthHeight {
		width: 470px;
		height: 200px;
		color: #fff;
		background: #892e00;
	}
	#WidthHeight span {
		display: block;
		padding: 10px;
	}

</style>

</head>

<body>
<div id=""BodyImposter"">
	<div id=""Box"">
		<div id=""FontInheritance"">
			<h1>H1 headline (34px/1em)</h1>
			<h2>H2 headline (22px/1em)</h2>
			<h3>H3 headline (16px/1em)</h3>
		</div>
		<p>Paragraph (12px/1.5em) Lorem ipsum dolor sit amet, <a href=""http://www.email-standards.org/"">consectetuer adipiscing</a> elit, sed diam nonummy nibh euismod tincidunt ut laoreet dolore magna aliquam erat volutpat. Ut wisi enim ad minim veniam, quis nostrud exerci tation ullamcorper suscipit lobortis nisl ut aliquip ex ea commodo consequat. <span id=""SerifFont"">(This is a serif font inside of a paragraph styled with a sans-serif font.)</span> <small>(This is <code>small</code> text.)</small></p>
		<p id=""QuotedFontFamily"">This is a font (Trebuchet MS) which needs quotes because its label comprises two words.</p>
		<ul id=""BulletBg"">
			<li>background bullet eum iriure dolor in hendrerit in</li>
			<li>vulputate velit esse molestie consequat, vel illum dolore eu</li>
			<li>feugiat nulla facilisis at vero eros et accumsan et iusto odio</li>
		</ul>
		<ul id=""BulletListStyle"">
			<li>list-style bullet eum iriure dolor in hendrerit in</li>
			<li>vulputate velit esse molestie consequat, vel illum dolore eu</li>
			<li>feugiat nulla facilisis at vero eros et accumsan et iusto odio</li>
		</ul>
		<ol>
			<li>ordered list sit amet, consectetuer adipiscing elit</li>
			<li>sed diam nonummy nibh euismod tincidunt ut laoreet</li>
			<li>dolore magna aliquam erat volutpat. Ut wisi enim ad minim</li>
		</ol>
		<dl>
			<dt>Definition list</dt>
			<dd>lorem ipsum dolor sit amet, consectetuer adipiscing elit</dd>
			<dd>sed diam nonummy nibh euismod tincidunt ut laoreet</dd>
			<dd>dolore magna aliquam erat volutpat. Ut wisi enim ad minim</dd>
		</dl>
		<div id=""Floater"">
			<div id=""Left"">
				<div id=""MarginPaddingOut"">
					<div id=""MarginPaddingIn"">
						<img src=""img/photo.jpg"" width=""180"" height=""180"" alt=""[photo: root canal]"" />
					</div>
				</div>
			</div>
			<div id=""Right"">
				<p>Right float with a positioned background</p>
			</div>
		</div>
		<p id=""ButtonBorders""><a href=""http://www.email-standards.org/"" id=""Arrow"">Borders and an a:hover background image</a></p>
		<div id=""WidthHeight"">
			<span>Width = 470, height = 200</span>
		</div>
	</div>
</div>
<!-- <unsubscribe>Hidden for testing</unsubscribe> -->
</body>
</html>";

			#endregion

			var result = await CreateAndParseWithOptions(emailACIDTest, new object(), _options);
			Assert.That(result, Is.Not.Null);
		}

		[Test]
		public void ParserCanProcessComplexValuePath()
		{
			var template = "{{#content}}Hello {{../Person.Name}}!{{/content}}";

			Assert.That(() =>
					Parser.ParseWithOptions(TestBuilder().WithTemplate(template).Build()),
				Throws.Nothing);
		}

		[Test]
		public void ParserCanProcessComplexFormattedValuePath()
		{
			var template = "{{../Data.Data(\"e\")}}";

			var morestachioDocumentInfo = Parser.ParseWithOptions(
				TestBuilder().WithTemplate(template).Build());
			Assert.That(morestachioDocumentInfo.Errors, Is.Empty,
				() => morestachioDocumentInfo.Errors.Select(e => e.HelpText).Aggregate((e, f) => e + f));
			template = "{{~Data.Data(\"e\")}}";

			morestachioDocumentInfo = Parser.ParseWithOptions(
				TestBuilder().WithTemplate(template).Build());
			Assert.That(morestachioDocumentInfo.Errors, Is.Empty,
				() => morestachioDocumentInfo.Errors.Select(e => e.HelpText).Aggregate((e, f) => e + f));
		}

		//[Test]
		//public void ParserCanProcessRootValuePath()
		//{
		//	Parser.ParseWithOptions(new ParserOptions("{{#content}}Hello {{.../Person.Name}}!{{/content}}"));
		//}

		[Test]
		public void ParserCanProcessCompoundConditionalGroup()
		{
			Assert.That(() =>
			{
				var template
					= "{{#Collection}}Collection has elements{{/Collection}}{{^Collection}}Collection doesn't have elements{{/Collection}}";
				Parser.ParseWithOptions(TestBuilder().WithTemplate(template).Build());
				template
					= "{{^Collection}}Collection doesn't have elements{{/Collection}}{{#Collection}}Collection has elements{{/Collection}}";
				Parser.ParseWithOptions(TestBuilder().WithTemplate(template).Build());
			}, Throws.Nothing);
		}

		[Test]
		public void ParserCanProcessEachConstruct()
		{
			var template = "{{#each ACollection}}{{.}}{{/each}}";

			Assert.That(() => { Parser.ParseWithOptions(TestBuilder().WithTemplate(template).Build()); },
				Throws.Nothing);
		}

		[Test]
		public void ParserCanProcessHandleMultilineTemplates()
		{
			var template = @"{{^Collection}}Collection doesn't have
							elements{{#Collection}}Collection has
						elements{{/Collection}}{{/Collection}}";
			Assert.That(() => { Parser.ParseWithOptions(TestBuilder().WithTemplate(template).Build()); },
				Throws.Nothing);
		}

		[Test]
		public void ParserCanProcessSimpleConditionalGroup()
		{
			var template = "{{#Collection}}Collection has elements{{/Collection}}";

			Assert.That(() => { Parser.ParseWithOptions(TestBuilder().WithTemplate(template).Build()); },
				Throws.Nothing);
		}

		[Test]
		public void ParserCanProcessSimpleNegatedCondionalGroup()
		{
			var template = "{{^Collection}}Collection has no elements{{/Collection}}";

			Assert.That(() => { Parser.ParseWithOptions(TestBuilder().WithTemplate(template).Build()); },
				Throws.Nothing);
		}

		[Test]
		public void ParserCanProcessSimpleValuePath()
		{
			var template = "Hello {{Name}}!";

			Assert.That(() => { Parser.ParseWithOptions(TestBuilder().WithTemplate(template).Build()); },
				Throws.Nothing);
		}

		[Test]
		public async Task ParserChangeDefaultFormatter()
		{
			var dateTime = DateTime.UtcNow;
			//this should not work as the Default settings for DateTime are ToString(Arg) so it should return a string and not an object
			var template = "{{d.Self().a}},{{d}}";

			var data = new Dictionary<string, object>
			{
				{
					"d", dateTime
				}
			};

			var result = await CreateAndParseWithOptions(template, data, _options, options =>
			{
				return options.WithFormatter<DateTime, object>(dt => new
				{
					Dt = dt,
					a = 2
				}, "Self");
			});
			Assert.That(result, Is.EqualTo("2," + dateTime));
		}

		[Test]
		public async Task ParserCanParseCustomTag()
		{
			var template = "{{#PI 3}}";
			var data = new Dictionary<string, object>();

			var result = await CreateAndParseWithOptions(template, data, _options | ParserOptionTypes.NoRerenderingTest,
				options =>
				{
					return options.AddCustomDocument(new TagDocumentItemProvider("#PI", async (
						outputStream,
						context,
						scopeData,
						value,
						tag
					) =>
					{
						outputStream.Write((Math.PI * int.Parse(value)).ToString());
						await Task.CompletedTask;
					}));
				});
			Assert.That(result, Is.EqualTo((Math.PI * 3).ToString()));
		}

		[Test]
		public async Task ParserCanParseCustomBlock()
		{
			var data = new Dictionary<string, object>();
			var template = @"{{#PI}}3{{/PI}}";

			var result = await CreateAndParseWithOptions(template, data, _options | ParserOptionTypes.NoRerenderingTest,
				options =>
				{
					return options.AddCustomDocument(new BlockDocumentItemProvider("#PI", "/PI",
						(
							outputStream,
							context,
							scopeData,
							value,
							children
						) =>
						{
							var firstOrDefault = children.OfType<ContentDocumentItem>().FirstOrDefault();
							outputStream.Write((Math.PI * int.Parse(firstOrDefault.Value)).ToString());

							return Enumerable.Empty<DocumentItemExecution>().ToPromise();
						}));
				});
			Assert.That(result, Is.EqualTo((Math.PI * 3).ToString()));
		}

		[Test]
		public void ParserThrowsParserExceptionForEachWithoutPath()
		{
			var template = "{{#eachs}}{{name}}{{/each}}";
			Assert.That(Parser.ParseWithOptions(TestBuilder().WithTemplate(template).Build()).Errors, Is.Not.Empty);
		}

		[Test]
		public void ParserThrowsParserExceptionForEmptyEach()
		{
			var template = "{{#eachs}}{{name}}{{/each}}";
			Assert.That(Parser.ParseWithOptions(TestBuilder().WithTemplate(template).Build()).Errors, Is.Not.Empty);
		}

		[Test]
		public void ParsingThrowsAnExceptionWhenConditionalGroupsAreMismatched()
		{
			var template = "{{#Collection}}Collection has elements{{/AnotherCollection}}";

			Assert.That(
				Parser.ParseWithOptions(TestBuilder().WithTemplate(template).Build()).Errors,
				Is.Not.Empty.And.Count.EqualTo(2));
		}

		[Test]
		public async Task TestCancelation()
		{
			var token = new CancellationTokenSource();
			var data = new ParserCancellationional(token);
			var template = @"{{ValueA}}{{ValueCancel}}{{ValueB}}";

			var result = await CreateAndParseWithOptions(template, data, _options, null, null,
				null, token);
			Assert.That(result, Is.EqualTo(data.ValueA + data.ValueCancel));
		}

		[Test]
		public async Task TestScopeOfIfInScoping()
		{
			var template = "{{#SCOPE Data}}{{#SCOPE Failed}}{{#IF ~Value}}{{this}}{{/IF}}{{/SCOPE}}{{/SCOPE}}";

			var data = new
			{
				Data = new
				{
					Failed = "Success"
				},
				Value = "FAILED"
			};
			var result = await CreateAndParseWithOptions(template, data, _options);
			Assert.That(result, Is.EqualTo("Success"));
		}

		[Test]
		public async Task TestScopeIsolation()
		{
			var template = "{{Data.Success}} - " +
				"{{#ISOLATE #SCOPE Data}}" +
				"{{this.Value}} == " +
				"{{~RootValue}} && " +
				"{{../RootValue}} != " +
				"{{Success}}" +
				"{{/ISOLATE}}";

			var data = new
			{
				Data = new
				{
					Success = "Success",
					Value = "SUCCESS"
				},
				RootValue = "FAILED"
			};
			var result = await CreateAndParseWithOptions(template, data, _options,
				options => { return options.WithNull("NULL"); });
			Assert.That(result, Is.EqualTo("Success - SUCCESS == NULL && NULL != Success"));
		}

		[Test]
		public async Task TestCollectionContext()
		{
			var template = "{{#each data}}{{$index}},{{$first}},{{$middel}},{{$last}},{{$odd}},{{$even}}.{{/each}}";

			var elementdata = new List<CollectionContextInfo>
			{
				new CollectionContextInfo
				{
					IndexProp = 0,
					EvenProp = true,
					OddProp = false,
					LastProp = false,
					FirstProp = true,
					MiddelProp = false
				},
				new CollectionContextInfo
				{
					IndexProp = 1,
					EvenProp = false,
					OddProp = true,
					LastProp = false,
					FirstProp = false,
					MiddelProp = true
				},
				new CollectionContextInfo
				{
					IndexProp = 2,
					EvenProp = true,
					OddProp = false,
					LastProp = true,
					FirstProp = false,
					MiddelProp = false
				}
			};
			var data = new Dictionary<string, object> { { "data", elementdata } };

			var result = await CreateAndParseWithOptions(template, data, _options, options =>
			{
				return options.AddCustomDocument(new BlockDocumentItemProvider("#PI", "/PI",
					(
						outputStream,
						context,
						scopeData,
						value,
						children
					) =>
					{
						var firstOrDefault = children.OfType<ContentDocumentItem>().FirstOrDefault();
						outputStream.Write((Math.PI * int.Parse(firstOrDefault.Value)).ToString());

						return Enumerable.Empty<DocumentItemExecution>().ToPromise();
					}));
			});
			Assert.That(result, Is.EqualTo(elementdata.Select(e => e.ToString()).Aggregate((e, f) => e + f)));
		}

		[Test]
		public async Task TestStoredCollectionContext()
		{
			var template
				= "{{#FOREACH item IN data}}{{item.$index}},{{item.$first}},{{item.$middel}},{{item.$last}},{{item.$odd}},{{item.$even}}.{{/FOREACH}}";

			var elementdata = new List<CollectionContextInfo>
			{
				new CollectionContextInfo
				{
					IndexProp = 0,
					EvenProp = true,
					OddProp = false,
					LastProp = false,
					FirstProp = true,
					MiddelProp = false
				},
				new CollectionContextInfo
				{
					IndexProp = 1,
					EvenProp = false,
					OddProp = true,
					LastProp = false,
					FirstProp = false,
					MiddelProp = true
				},
				new CollectionContextInfo
				{
					IndexProp = 2,
					EvenProp = true,
					OddProp = false,
					LastProp = true,
					FirstProp = false,
					MiddelProp = false
				}
			};
			var data = new Dictionary<string, object> { { "data", elementdata } };

			var result = await CreateAndParseWithOptions(template, data, _options, options =>
			{
				return options.AddCustomDocument(new BlockDocumentItemProvider("#PI", "/PI",
					(
						outputStream,
						context,
						scopeData,
						value,
						children
					) =>
					{
						var firstOrDefault = children.OfType<ContentDocumentItem>().FirstOrDefault();
						outputStream.Write((Math.PI * int.Parse(firstOrDefault.Value)).ToString());

						return Enumerable.Empty<DocumentItemExecution>().ToPromise();
					}));
			});
			Assert.That(result, Is.EqualTo(elementdata.Select(e => e.ToString()).Aggregate((e, f) => e + f)));
		}

		[Test]
		public async Task TestRepeatContext()
		{
			var template
				= "{{#repeat data.Count}}{{$index}},{{$first}},{{$middel}},{{$last}},{{$odd}},{{$even}}.{{/repeat}}";

			var elementdata = new List<CollectionContextInfo>
			{
				new CollectionContextInfo
				{
					IndexProp = 0,
					EvenProp = true,
					OddProp = false,
					LastProp = false,
					FirstProp = true,
					MiddelProp = false
				},
				new CollectionContextInfo
				{
					IndexProp = 1,
					EvenProp = false,
					OddProp = true,
					LastProp = false,
					FirstProp = false,
					MiddelProp = true
				},
				new CollectionContextInfo
				{
					IndexProp = 2,
					EvenProp = true,
					OddProp = false,
					LastProp = true,
					FirstProp = false,
					MiddelProp = false
				}
			};
			var data = new Dictionary<string, object> { { "data", elementdata } };

			var result = await CreateAndParseWithOptions(template, data, _options, options =>
			{
				return options.AddCustomDocument(new BlockDocumentItemProvider("#PI", "/PI",
					(
						outputStream,
						context,
						scopeData,
						value,
						children
					) =>
					{
						var firstOrDefault = children.OfType<ContentDocumentItem>().FirstOrDefault();
						outputStream.Write((Math.PI * int.Parse(firstOrDefault.Value)).ToString());

						return Enumerable.Empty<DocumentItemExecution>().ToPromise();
					}));
			});
			Assert.That(result, Is.EqualTo(elementdata.Select(e => e.ToString()).Aggregate((e, f) => e + f)));
		}

		[Test]
		public async Task TestRepeatLoopContext()
		{
			var template = "{{#REPEAT 5}}" +
				"{{$index}}," +
				"{{/REPEAT}}";
			var data = new Dictionary<string, object>();
			var result = await CreateAndParseWithOptions(template, data, _options,
				options => { return options.WithFormatters(typeof(EqualityFormatter)); });
			Assert.That(result, Is.EqualTo("0,1,2,3,4,"));
		}

		[Test]
		public async Task TestRepeatInnerLoopContext()
		{
			var template =
				@"{{#REPEAT 2}}
{{--| $index}}(
{{--| #REPEAT 3}}
{{--| $index}},
{{--| /REPEAT}})
{{--| /REPEAT}}";
			var data = new Dictionary<string, object>();
			var result = await CreateAndParseWithOptions(template, data, _options,
				options => { return options.WithFormatters(typeof(EqualityFormatter)); });
			Assert.That(result, Is.EqualTo("0(0,1,2,)1(0,1,2,)"));
		}

		[Test]
		public async Task TestWhileLoopContext()
		{
			var template = @"{{#WHILE $index.SmallerAs(5)}}
						   {{-| $index}},
						   {{-| /WHILE}}";
			var data = new Dictionary<string, object>();
			var result = await CreateAndParseWithOptions(template, data, _options,
				options => { return options.WithFormatters(typeof(EqualityFormatter)); });
			Assert.That(result, Is.EqualTo("0,1,2,3,4,"));
		}

		[Test]
		public async Task TestDoLoopContext()
		{
			var template = @"{{#DO $index.SmallerAs(5)}}
							 {{-| $index}},
							 {{-| /DO}}";
			var data = new Dictionary<string, object>();
			var result = await CreateAndParseWithOptions(template, data, _options,
				options => { return options.WithFormatters(typeof(EqualityFormatter)); });
			Assert.That(result, Is.EqualTo("0,1,2,3,4,5,"));
		}

		[Test]
		public async Task TestCanRemoveLineBreaks()
		{
			var template = @"{{#TNLS}}

Test{{#NL}}";
			var data = new Dictionary<string, object>();
			var result = await CreateAndParseWithOptions(template, data, _options,
				options => { return options.WithFormatters(typeof(EqualityFormatter)); });

			Assert.That(result, Is.EqualTo($"Test{Environment.NewLine}"));
		}

		[Test]
		public async Task TestCanRemoveLineBreaksWithinKeyword()
		{
			var template = @"{{data |--}}
Static

{{--| data}}";

			var data = new
			{
				data = "World"
			};
			var result = await CreateAndParseWithOptions(template, data, _options,
				options => { return options.WithFormatters(typeof(EqualityFormatter)); });
			Assert.That(result, Is.EqualTo(@"WorldStaticWorld"));
		}

		[Test]
		public async Task TestCanRemoveLineBreaksWhereNothingIsToRemove()
		{
			var template = @"{{#IF other}} Other exists{{/IF |-}} 
{{^IF other}} Other does not exists{{/IF |-}},";

			var data = new
			{
				other = "World"
			};
			var result = await CreateAndParseWithOptions(template, data, _options,
				options => { return options.WithFormatters(typeof(EqualityFormatter)); });
			Assert.That(result, Is.EqualTo(@" Other exists,"));
		}

		[Test]
		public async Task TestCanRemoveLineBreaksWithOption()
		{
			var template = @"{{#SET OPTION TrimTailing = true}}{{#SET OPTION TrimLeading = true}}
{{data}}
Static
{{data}}";

			var data = new
			{
				data = "World"
			};
			var result = await CreateAndParseWithOptions(template, data, _options | ParserOptionTypes.NoRerenderingTest,
				options => { return options.WithFormatters(typeof(EqualityFormatter)); });
			Assert.That(result, Is.EqualTo(@"WorldStaticWorld"));
		}

		[Test]
		public async Task TestParserNotRenderingUnknownTagInstruction()
		{
			var template = @"PreText {{#UnknownTag}} Subtext";

			var data = new
			{
				data = "World"
			};

			var result = await CreateAndParseWithOptions(template, data, _options, options =>
			{
				return options.AddCustomDocument(new TagRegexDocumentItemProvider(new Regex("#(.*)"),
					async (
						stream,
						context,
						scopeData,
						value,
						keyword
					) =>
					{
						stream.Write($"[Unknown Tag '{keyword}{value}']");
						await Task.CompletedTask;
					}));
			});
			Assert.That(result, Is.EqualTo(@"PreText [Unknown Tag '#UnknownTag'] Subtext"));
		}

		[Test]
		public async Task TestParserNotRenderingUnknownTagWithValueInstruction()
		{
			var template = @"PreText {{#UnknownTag #AnyValue BLA}} Subtext";

			var data = new
			{
				data = "WorldA"
			};

			var result = await CreateAndParseWithOptions(template, data, _options, options =>
			{
				TagDocumentProviderFunction tagDocumentProviderFunction = async (
					stream,
					context,
					scopeData,
					value,
					keyword
				) =>
				{
					stream.Write($"[Unknown Tag '{keyword} {value}']");
					await Task.CompletedTask;
				};

				return options.AddCustomDocument(new TagRegexDocumentItemProvider(new Regex("#([^\\s]*)"),
					tagDocumentProviderFunction));
			});
			Assert.That(result, Is.EqualTo(@"PreText [Unknown Tag '#UnknownTag #AnyValue BLA'] Subtext"));
		}

		[Test]
		public async Task TestInstanceMethods()
		{
			var template = @"V: {{data.Value}}{{data.Increment()}}|{{data.Value}}{{data.Decrement()}}|{{data.Value}}";

			var data = new
			{
				data = new DirectLinkedFormatter()
			};
			var result = await CreateAndParseWithOptions(template, data, _options,
				options => { return options.WithFormatters<DirectLinkedFormatter>(); });
			Assert.That(result, Is.EqualTo(@"V: 0|1|0"));
		}

		private class DirectLinkedFormatter
		{
			public int Value { get; set; }

			[MorestachioFormatter("Increment", "XXX")]
			public void Increment()
			{
				Value++;
			}

			[MorestachioFormatter("Decrement", "XXX")]
			public void Decrement()
			{
				Value--;
			}
		}

		[Test]
		public async Task TestParserCanBindExpression()
		{
			var template = @"Data: {{this.RenderExpression(123, data.value.test)}}";

			var data = new
			{
				data = 777
			};
			var result = await CreateAndParseWithOptions(template, data, _options,
				options => { return options.WithFormatters<BoundExpressionFormatter>(); });
			Assert.That(result, Is.EqualTo(@"Data: 123|data.value.test"));
		}

		[Test]
		public async Task TestParserCanProcessNoPrintBlock()
		{
			var template = @"{{#NOPRINT}}{{data.data}}{{this.TestFormatter()}}{{/NOPRINT}}";

			var data = new
			{
				data = 777,
				Called = false
			};
			var result = await CreateAndParseWithOptions(template, data, _options, options =>
			{
				return options.WithValueResolver(new FieldValueResolver()).WithFormatter<object, string>((obj) =>
				{
					Assert.That(data, Is.EqualTo(obj));
					data = new
					{
						data = 777,
						Called = true
					};

					return "NOOOOOOOOOOO!";
				}, "TestFormatter");
			});
			Assert.That(result, Is.EqualTo(@""));
			Assert.That(data.Called, Is.True);
		}

		[Test]
		public async Task TestStrictMode()
		{
			var template = @"{{data.InvalidPath}}";
			var data = new
			{
				data = new
				{
					validPath = true
				}
			};

			Assert.That(
				async () => await CreateAndParseWithOptions(template, data, _options,
					builder => builder.WithStrictExecution(true)),
				Throws.Exception.TypeOf<UnresolvedPathException>());
		}

		private class BoundExpressionFormatter
		{
			[MorestachioFormatter("[MethodName]", "")]
			public static string RenderExpression(object source, int constValue, IMorestachioExpression expression)
			{
				var visitor = new ToParsableStringExpressionVisitor();
				expression.Accept(visitor);

				return constValue + "|" + visitor.StringBuilder;
			}
		}

		private class CollectionContextInfo
		{
			public int IndexProp { private get; set; }
			public bool FirstProp { private get; set; }
			public bool MiddelProp { private get; set; }
			public bool LastProp { private get; set; }

			public bool OddProp { private get; set; }
			public bool EvenProp { private get; set; }

			public override string ToString()
			{
				return $"{IndexProp},{FirstProp},{MiddelProp},{LastProp},{OddProp},{EvenProp}.";
			}
		}
	}
}