using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Morestachio.Document.Custom;
using Morestachio.Formatter.Framework;
using Morestachio.Formatter.Predefined;
using Morestachio.Framework.Expression.Visitors;
using Morestachio.Helper;
using Morestachio.Linq;
using NUnit.Framework;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Formatter.Framework.Attributes;
using Morestachio.Framework;
using Morestachio.Framework.Error;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Parsing.ParserErrors;
using ExpressionParser = Morestachio.Framework.Expression.ExpressionParser;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace Morestachio.Tests
{
	[TestFixture]
	[Parallelizable(ParallelScope.All)]
	public class ParserFixture
	{
		public static Encoding DefaultEncoding { get; set; } = new UnicodeEncoding(true, false, false);

		public static void TestLocationsInOrder(MorestachioDocumentInfo documentInfo)
		{
			var api = documentInfo
				.Fluent()
				.SearchForward(f => !(f is MorestachioDocument), false);
			var lastLocation = new CharacterLocation(0, -1);
			var visitor = new ToParsableStringDocumentVisitor();

			while (api.Context.OperationStatus)
			{
				if (api.Context.CurrentNode.Item is TextEditDocumentItem || api.Context.CurrentNode.Item is RemoveAliasDocumentItem)
				{
					Assert.That(api.Context.CurrentNode.Item.ExpressionStart, Is.GreaterThan(lastLocation).Or.EqualTo(lastLocation));
				}
				else
				{
					Assert.That(api.Context.CurrentNode.Item.ExpressionStart, Is.GreaterThan(lastLocation));
					lastLocation = api.Context.CurrentNode.Item.ExpressionStart;
				}

				//if (api.Context.CurrentNode.Item is ExpressionDocumentItemBase expDoc)
				//{
				//	lastLocation = TestExpressionLocationsInOrderInternal(expDoc, lastLocation);
				//}
				//else
				//{
				//}
				visitor.StringBuilder.Clear();
				api.SearchForward(f => true, false);
			}
		}

		private static CharacterLocation TestExpressionLocationsInOrderInternal(ExpressionDocumentItemBase expDoc,
			CharacterLocation lastLocation)
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
			var unpackFormatterTask = await AsyncHelper.UnpackFormatterTask(fromResult);
			Assert.That(unpackFormatterTask, Is.EqualTo(fromResult.Result));
		}

		[Test]
		[TestCase("d.f", 1)]
		[TestCase("d.f;Test", 2)]
		[TestCase("d.f;.Test()", 2)]
		[TestCase("d.f;.Test(); Other", 3)]
		[TestCase("d.f; .Test() ; Other", 3)]
		[TestCase("d.f; .Test() ; Other; 'TEST'", 4)]
		[TestCase("d.f; .Test() ; 'test'", 3)]
		[TestCase("d.f; .Test() ; 'test'; acd", 4)]
		[TestCase("'d'; .Test() ; 'test'; acd", 4)]
		[TestCase("''; .Test() ; 'test'; acd", 4)]
		public void ExpressionParserCanParseMany(string expression, int expected)
		{
			var context = TokenzierContext.FromText(expression);
			var expr = new List<IMorestachioExpression>();
			int parsedBy = 0;
			var location = 0;
			while ((location = context.CurrentLocation.ToPosition(context)) < expression.Length)
			{
				expr.Add(ExpressionParser.ParseExpression(expression, context, out parsedBy, parsedBy));
			}
			Assert.That(context.Errors, Is.Empty);
			Assert.That(expr.Count, Is.EqualTo(expected));

			var ses = expression.Split(';');
			for (var index = 0; index < ses.Length; index++)
			{
				var s = ses[index];
				var expPart = s.Trim();
				var exp = expr[index];
				var visitor = new ToParsableStringExpressionVisitor()
				{
					TrimEoexDelimiters = true
				};
				visitor.Visit(exp);
				Assert.That(visitor.StringBuilder.ToString(), Is.EqualTo(expPart));
			}
		}

		[Test]
		[TestCase(".(.('').())")]
		[TestCase(".a(.b(.c().d()))")]
		[TestCase(".f(.fe().P).Fg()")]
		[TestCase(".f(.fe().P).Fg().DE()")]
		[TestCase(".f(.fe().P.Add(' ')).Fg().DE()")]
		[TestCase(".f(.fe().P.Add(' ')).Fg().DE()")]
		[TestCase("d.f")]
		[TestCase("d.f()")]
		[TestCase("d.f().Test")]
		[TestCase("d.f(fA)")]
		[TestCase("d.f(fA).Test")]
		[TestCase("d.f(fA, .fb()).Test")]
		[TestCase("d.f(fA, fb.()).Test")]
		[TestCase("d.f(fA.())")]
		[TestCase("d.f(fA.(''))")]
		[TestCase("d.f(fA.('').Test)")]
		[TestCase("d.f(fA.('').().())")]
		[TestCase("d.f(fA.('').fB.())")]
		[TestCase("d.f(fA.('').fB.()).Test")]
		[TestCase("d.f(fA.('').fB.()).Test.Data")]
		[TestCase("d.f(fA.('').fB.()).Test.fC()")]
		[TestCase("d.f(fA.('', e))")]
		[TestCase("d.f(fA.('', e.()))")]
		[TestCase("d.f(fA.('', e.('')))")]
		[TestCase("d.f(fA.('d'))")]
		[TestCase("d.f(fA.('d').Test)")]
		[TestCase("d.f(fA.('d').())")]
		[TestCase("d.f(fA.('d').fB.())")]
		[TestCase("d.f(fA.('d').fB.()).Test")]
		[TestCase("d.f(fA.('d').fB.()).Test.Data")]
		[TestCase("d.f(fA.('d').fB.()).Test.fC()")]
		[TestCase("d.f(fA.('d').fB.('')).Test.fC()")]
		[TestCase("d.f(fA.('d').fB.('')).Test.fC('')")]
		[TestCase("d.f(fA.('d').fB.('d')).Test.fC('')")]
		[TestCase("d.f(fA.('d').fB.('d')).Test.fC('d')")]
		[TestCase("d.f(fA.('d', f).fB.('d')).Test.fC('d')")]
		[TestCase("d.f(fA.('d', f).fB.('d', f)).Test.fC('d')")]
		[TestCase("d.f(fA.('d', f).fB.('d', f)).Test.fC('d', f)")]
		[TestCase("d.f(fA.('d', f.()).fB.('d', f)).Test.fC('d', f)")]
		[TestCase("d.f(fA.('d', f.()).fB.('d', f.())).Test.fC('d', f.())")]
		public void TestExpressionParser(string query)
		{
			var context = TokenzierContext.FromText(query);
			var expressions = ExpressionParser.ParseExpression(query, context);
			Assert.That(expressions, Is.Not.Null, () => context.Errors.GetErrorText());

			var visitor = new ToParsableStringExpressionVisitor();
			expressions.Accept(visitor);

			Assert.That(visitor.StringBuilder.ToString(), Is.EqualTo(query));
			Assert.That(context.Errors, Is.Empty, () => context.Errors.GetErrorText());
		}

		[Test]
		[TestCase("a.Add(2).Buffer == 'test'")]
		public void TestExpressionParserDbg(string query)
		{
			var context = TokenzierContext.FromText(query);
			var expressions = ExpressionParser.ParseExpression(query, context);
			Assert.That(expressions, Is.Not.Null);

			var visitor = new ToParsableStringExpressionVisitor();
			expressions.Accept(visitor);

			Assert.That(visitor.StringBuilder.ToString(), Is.EqualTo(query));
			Assert.That(context.Errors, Is.Empty, () => context.Errors.GetErrorText());
		}

		[Test]
		[TestCase("A + B", 5 + 10, 5, 10)]
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

		[TestCase("A + B + C", 5 + 10 + 15, 5, 10, 15)]
		[TestCase("A - B - C", 5 - 10 - 15, 5, 10, 15)]
		[TestCase("A / B / C", 5 / 10 / 15, 5, 10, 15)]
		[TestCase("A * B * C", 5 * 10 * 15, 5, 10, 15)]
		[TestCase("A % B % C", 5 % 10 % 15, 5, 10, 15)]
		[TestCase("A << B << C", 5 << 10 << 15, 5, 10, 15)]
		[TestCase("A >> B >> C", 5 >> 10 >> 15, 5, 10, 15)]

		[TestCase(".Self(A + B) + C", (5 + 10) + 15, 5, 10, 15)]
		[TestCase(".Self(A - B) - C", (5 - 10) - 15, 5, 10, 15)]
		[TestCase(".Self(A / B) / C", (5 / 10) / 15, 5, 10, 15)]
		[TestCase(".Self(A * B) * C", (5 * 10) * 15, 5, 10, 15)]
		[TestCase(".Self(A % B) % C", (5 % 10) % 15, 5, 10, 15)]
		[TestCase(".Self(A << B) << C", (5 << 10) << 15, 5, 10, 15)]
		[TestCase(".Self(A >> B) >> C", (5 >> 10) >> 15, 5, 10, 15)]

		[TestCase("A + .Self(B + C)", 5 + (10 + 15), 5, 10, 15)]
		[TestCase("A - .Self(B - C)", 5 - (10 - 15), 5, 10, 15)]
		[TestCase("A / .Self(B / C)", 5 / (22 / 15), 5, 22, 15)]
		[TestCase("A * .Self(B * C)", 5 * (10 * 15), 5, 10, 15)]
		[TestCase("A % .Self(B % C)", 5 % (10 % 15), 5, 10, 15)]
		[TestCase("A << .Self(B << C)", 5 << (10 << 15), 5, 10, 15)]
		[TestCase("A >> .Self(B >> C)", 5 >> (10 >> 15), 5, 10, 15)]

		[TestCase(".Self(A + .Self(B + C))", 5 + (10 + 15), 5, 10, 15)]
		[TestCase(".Self(A - .Self(B - C))", 5 - (10 - 15), 5, 10, 15)]
		[TestCase(".Self(A / .Self(B / C))", 5 / (22 / 15), 5, 22, 15)]
		[TestCase(".Self(A * .Self(B * C))", 5 * (10 * 15), 5, 10, 15)]
		[TestCase(".Self(A % .Self(B % C))", 5 % (10 % 15), 5, 10, 15)]
		[TestCase(".Self(A << .Self(B << C))", 5 << (10 << 15), 5, 10, 15)]
		[TestCase(".Self(A >> .Self(B >> C))", 5 >> (10 >> 15), 5, 10, 15)]

		public async Task TestExpressionCanParseOperators(string query, object valExp, params object[] args)
		{
			var context = TokenzierContext.FromText(query);
			var expressions = ExpressionParser.ParseExpression(query, context);
			Assert.That(expressions, Is.Not.Null);

			var visitor = new ToParsableStringExpressionVisitor();
			expressions.Accept(visitor);

			Assert.That(visitor.StringBuilder.ToString(), Is.EqualTo(query));
			Assert.That(context.Errors, Is.Empty, () => context.Errors.GetErrorText());

			var parsingOptions = new ParserOptions("{{" + query + "}}", null,
				DefaultEncoding);
			parsingOptions.Formatters.AddSingleGlobal<object, object>(f =>
			{
				return f;
			}, "Self");

			var results = Parser.ParseWithOptions(parsingOptions);
			var dict = new Dictionary<string, object>();
			for (var index = 0; index < args.Length; index++)
			{
				var arg = args[index];
				dict.Add(((char)('A' + index)).ToString(), arg);
			}
			TestLocationsInOrder(results);

			var andStringifyAsync = await results.CreateAndStringifyAsync(dict);
			Assert.That(andStringifyAsync, Is.EqualTo((valExp).ToString()));
		}

		[Test]
		[TestCase("d")]
		[TestCase("D")]
		[TestCase("f")]
		[TestCase("F")]
		[TestCase("dd,,MM,,YYY")]
		public void ParserCanFormat(string dtFormat)
		{
			var data = DateTime.UtcNow;
			var results =
				Parser.ParseWithOptions(new ParserOptions("{{data.(\"" + dtFormat + "\")}},{{data}}", null,
					DefaultEncoding));
			TestLocationsInOrder(results);
			var result = results.CreateAndStringify(new Dictionary<string, object> { { "data", data } });
			Assert.That(result, Is.EqualTo(data.ToString(dtFormat) + "," + data));
		}

		[Test]
		public void ParserCanVariableStaticExpression()
		{
			var parsingOptions = new ParserOptions("{{#var f = data}}|{{f}}", null,
				DefaultEncoding);
			var results =
				Parser.ParseWithOptions(parsingOptions);
			TestLocationsInOrder(results);
			var result = results.CreateAndStringify(new Dictionary<string, object>
			{
				{ "data", "test" },
			});
			Assert.That(result, Is.EqualTo("|test"));
		}

		[Test]
		public void ParserCanVariableExpression()
		{
			var parsingOptions = new ParserOptions("{{#var f = data.('G')}}|{{f}}", null,
				DefaultEncoding);
			var results =
				Parser.ParseWithOptions(parsingOptions);
			var date = DateTime.Now;
			TestLocationsInOrder(results);
			var result = results.CreateAndStringify(new Dictionary<string, object>
			{
				{ "data", date },
			});
			Assert.That(result, Is.EqualTo("|" + date.ToString("G")));
		}

		[Test]
		public void ParserCanVariableExpressionWithFormats()
		{
			var parsingOptions = new ParserOptions("{{#VAR f = data.('G').PadLeft(123)}}|{{f}}", null,
				DefaultEncoding);
			parsingOptions.Formatters.AddSingle((string value, int nr) =>
			{
				return value.PadLeft(nr);
			}, "PadLeft");
			var parsedTemplate =
				Parser.ParseWithOptions(parsingOptions);
			var date = DateTime.Now;
			var result = parsedTemplate.CreateAndStringify(new Dictionary<string, object>
			{
				{ "data", date },
			});
			TestLocationsInOrder(parsedTemplate);
			Assert.That(result, Is.EqualTo("|" + date.ToString("G").PadLeft(123)));
			SerilalizerTests.SerializerTest.AssertDocumentItemIsSameAsTemplate(parsingOptions.Template, parsedTemplate.Document);
		}

		[Test]
		public void ParserCanVariableSetToOtherVariable()
		{
			var parsingOptions = new ParserOptions("{{#var f = data}}" +
												   "{{#var e = f.('G')}}" +
												   "{{e.PadLeft(123)}}", null,
				DefaultEncoding);
			parsingOptions.Formatters.AddSingle((string value, int nr) =>
			{
				return value.PadLeft(nr);
			}, "PadLeft");
			var results =
				Parser.ParseWithOptions(parsingOptions);
			var date = DateTime.Now;
			TestLocationsInOrder(results);
			var result = results.CreateAndStringify(new Dictionary<string, object>
			{
				{ "data", date },
			});
			Assert.That(result, Is.EqualTo(date.ToString("G").PadLeft(123)));
		}

		[Test]
		public void ParserCanVariableScope()
		{
			var template =
				@"{{#var global = data |-}}
{{#data|-}}
		{{global|-}}
		{{#let global = 'Burns '|-}}
		{{global|-}}
		{{#let global = 'Likes '|-}}
		{{global|-}}
		{{#let local = 'Likes '|-}}
		{{#var global = 'Miss '|-}}
{{/data|-}}
{{local|-}}
{{global|-}}
{{#var global = 'Money '|-}}
{{global|-}}
{{#let global = 'Alot'|-}}
{{global|-}}";
			var parsingOptions = new ParserOptions(template, null,
				DefaultEncoding);

			var results =
				Parser.ParseWithOptions(parsingOptions);
			TestLocationsInOrder(results);
			var result = results.CreateAndStringify(new Dictionary<string, object>
			{
				{ "data", "Mr " },
			});
			Assert.That(result, Is.EqualTo("Mr Burns Likes Miss Money Alot"));
		}

		[Test]
		public void ParserCanVariableSetToNull()
		{
			var parsingOptions = new ParserOptions("{{#var f = data}}" +
												   "{{#var f = null}}" +
												   "{{f.PadLeft(123)}}", null,
				DefaultEncoding);
			parsingOptions.Formatters.AddSingle((string value, int nr) =>
			{
				return value.PadLeft(nr);
			}, "PadLeft");
			parsingOptions.Null = "{NULL}";
			var results =
				Parser.ParseWithOptions(parsingOptions);
			var date = DateTime.Now;
			TestLocationsInOrder(results);
			var result = results.CreateAndStringify(new Dictionary<string, object>
			{
				{ "data", date },
			});
			Assert.That(result, Is.EqualTo(parsingOptions.Null));
		}

		[Test]
		public void ParserCanVariableSetToString()
		{
			var parsingOptions = new ParserOptions("{{#var f = data}}" +
												   "{{#var f = 'Test'}}" +
												   "{{f.PadLeft(123)}}", null,
				DefaultEncoding);
			parsingOptions.Formatters.AddSingle((string value, int nr) =>
			{
				return value.PadLeft(nr);
			}, "PadLeft");
			var results =
				Parser.ParseWithOptions(parsingOptions);
			var date = DateTime.Now;
			TestLocationsInOrder(results);
			var result = results.CreateAndStringify(new Dictionary<string, object>
			{
				{ "data", date },
			});
			Assert.That(result, Is.EqualTo("Test".PadLeft(123)));
		}

		[Test]
		public void ParserCanVariableSetToStringWithEscaptedValues()
		{
			var parsingOptions = new ParserOptions("{{#var f = data}}" +
												   "{{#var f = 'Te\\'st'}}" +
												   "{{f.PadLeft(123)}}", null,
				DefaultEncoding, true);
			parsingOptions.Formatters.AddSingle((string value, int nr) =>
			{
				return value.PadLeft(nr);
			}, "PadLeft");
			var results =
				Parser.ParseWithOptions(parsingOptions);
			var date = DateTime.Now;
			TestLocationsInOrder(results);
			var result = results.CreateAndStringify(new Dictionary<string, object>
			{
				{ "data", date },
			});
			Assert.That(result, Is.EqualTo("Te'st".PadLeft(123)));
		}

		[Test]
		public void ParserCanVariableSetToEmptyString()
		{
			var parsingOptions = new ParserOptions("{{#var f = ''}}" +
												   "{{f.PadLeft(123)}}", null,
				DefaultEncoding);
			parsingOptions.Formatters.AddSingle((string value, int nr) =>
			{
				return value.PadLeft(nr);
			}, "PadLeft");
			var results =
				Parser.ParseWithOptions(parsingOptions);
			var date = DateTime.Now;
			TestLocationsInOrder(results);
			var result = results.CreateAndStringify(new Dictionary<string, object>
			{
				{ "data", date },
			});
			Assert.That(result, Is.EqualTo("".PadLeft(123)));
		}

		[Test]
		public void ParserCanSetOption()
		{
			var template = "{{valueA}}," +
						   "{{#SET OPTION TokenPrefix = '<%'}}" +
						   "<%#SET OPTION TokenSuffix = '%>'}}" +
						   "{{valueA}}" +
						   "<%valueB%>," +
						   "<%#SET OPTION TokenPrefix = '{{'%>" +
						   "{{#SET OPTION TokenSuffix = '}}'%>" +
						   "{{valueC}}";
			var data = new Dictionary<string, object>()
			{
				{"valueA", "Hello" },
				{"valueB", "_" },
				{"valueC", "World" },
			};

			var parserOptions = new ParserOptions(template, null, DefaultEncoding);
			var results = Parser.ParseWithOptions(parserOptions);
			TestLocationsInOrder(results);
			Assert.That(results.CreateAndStringify(data), Is.EqualTo("Hello,{{valueA}}_,World"));
		}

		[Test]
		public void ParserCanNullableFormatTest()
		{
			var parsingOptions = new ParserOptions("ShouldBe: {{data}}, ButNot: {{extData}}", null,
				DefaultEncoding)
			{
				Null = "IAmTheLaw!"
			};
			var results =
				Parser.ParseWithOptions(parsingOptions);
			TestLocationsInOrder(results);
			var result = results.CreateAndStringify(new Dictionary<string, object>
			{
				{ "data", (string)null },
				{ "extData", "Test" }
			});
			Assert.That(result, Is.EqualTo($"ShouldBe: {parsingOptions.Null}, ButNot: Test"));
		}

		[Test]
		public void ParserCanParseIntegerNumber()
		{
			var parsingOptions = new ParserOptions("{{1111}}", null,
				DefaultEncoding);
			var results =
				Parser.ParseWithOptions(parsingOptions);
			TestLocationsInOrder(results);
			var result = results.CreateAndStringify(new Dictionary<string, object>
			{
			});
			Assert.That(result, Is.EqualTo("1111"));
		}

		[Test]
		public void ParserCanParseNumberAsFormatterArg()
		{
			var parsingOptions = new ParserOptions("{{f.(123)}}", null,
				DefaultEncoding);
			parsingOptions.Formatters.AddSingle(new Func<string, int, string>((e, f) => f.ToString(e)));
			var results =
				Parser.ParseWithOptions(parsingOptions);
			TestLocationsInOrder(results);
			var result = results.CreateAndStringify(new Dictionary<string, object>
			{
				{"f", "F5" }
			});
			Assert.That(result, Is.EqualTo(123.ToString("F5")));
		}

		[Test]
		public void ParserCanParseFloatingNumber()
		{
			var parsingOptions = new ParserOptions("{{1.123.('F5')}}", null,
				DefaultEncoding);
			var results =
				Parser.ParseWithOptions(parsingOptions);
			TestLocationsInOrder(results);
			var result = results.CreateAndStringify(new Dictionary<string, object>
			{
			});
			Assert.That(result, Is.EqualTo(1.123.ToString("F5", parsingOptions.CultureInfo)));
		}

		[Test]
		public async Task TestRootAccess()
		{
			var template = "{{#EACH data.testList.Select()}}" +
								  "{{#VAR eachValue = ~data.testInt}}" +
								  "{{/EACH}}" +
								  "{{eachValue}} = {{~data.testInt}} = {{data.testInt}}";

			var parsingOptions = new ParserOptions(template, null, DefaultEncoding);
			parsingOptions.Formatters.AddFromType(typeof(DynamicLinq));
			parsingOptions.Formatters.AddFromType(typeof(FormatterTests.NumberFormatter));
			var results = await Parser.ParseWithOptionsAsync(parsingOptions);

			var modelWorking = new Dictionary<string, object>()
			{
				{
					"data", new Dictionary<string, object>()
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

			var result = await results.CreateAndStringifyAsync(modelWorking);
			TestLocationsInOrder(results);
			Assert.AreEqual("2 = 2 = 2", result);
			SerilalizerTests.SerializerTest.AssertDocumentItemIsSameAsTemplate(parsingOptions.Template, results.Document);
		}

		[Test]
		public void TestMethodAsArgumentAccess()
		{
			var options = new ParserOptions("{{data.Multiply(data.Multiply(data))}}", null, DefaultEncoding);
			options.Formatters.AddFromType(typeof(NumberFormatter));
			var template = Parser.ParseWithOptions(options);
			var andStringify = template.CreateAndStringify(new Dictionary<string, object>()
			{
				{"data", 2}
			});

			Assert.That(andStringify, Is.EqualTo("8"));
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
		public void ParserCanSelfFormat(string dtFormat)
		{
			var data = DateTime.UtcNow;
			var parsingOptions = new ParserOptions(
				"{{#data}}{{.(\"" + dtFormat + "\")}}{{/data}},{{data}}",
				null, DefaultEncoding);
			var results = Parser.ParseWithOptions(parsingOptions);
			var result = results.CreateAndStringify(new Dictionary<string, object> { { "data", data } });
			Assert.That(result, Is.EqualTo(data.ToString(dtFormat, parsingOptions.CultureInfo) + "," + data));
		}

		[Test]
		[TestCase("{{data.(d))}}", 1, Ignore = "Currently its not possible to evaluate this info")]//
		[TestCase("{{data.((d)}}", 1)]
		[TestCase("{{data)}}", 1)]
		[TestCase("{{data.(}}", 1)]
		[TestCase("{{data.(arg}}", 1)]
		[TestCase("{{data.(arg, 'test'}}", 1)]
		[TestCase("{{data.(arg, 'test)}}", 1)]
		[TestCase("{{data.(arg, )}}", 1)]
		public void ParserThrowsAnExceptionWhenFormatIsMismatched(string invalidTemplate, int expectedNoOfExceptions)
		{
			IEnumerable<IMorestachioError> errors;
			Assert.That(errors = Parser.ParseWithOptions(new ParserOptions(invalidTemplate)).Errors,
				Is.Not.Empty,
				() => errors.Select(e => e.HelpText).DefaultIfEmpty("").Aggregate((e, f) => e + Environment.NewLine + f));
		}

		[Test]
		[TestCase("{{#ACollection}}{{.}}{{/each}}")]
		[TestCase("{{#ACollection}}{{.}}{{/ACollection}}{{/each}}")]
		[TestCase("{{#IF ACollection}}{{.}}{{/IF}}{{/each}}")]
		[TestCase("{{/each}}")]
		public void ParserThrowsAnExceptionWhenEachIsMismatched(string invalidTemplate)
		{
			Assert.That(Parser.ParseWithOptions(new ParserOptions(invalidTemplate)).Errors, Is.Not.Empty);
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
				Parser.ParseWithOptions(new ParserOptions(template, null, DefaultEncoding)).CreateAndStringify(model),
				Is.EqualTo(expected));
		}


		[Test]
		[TestCase("{{#each element}}{{name}}")]
		[TestCase("{{#element}}{{name}}")]
		[TestCase("{{^element}}{{name}}")]
		[TestCase("{{#IF element}}{{name}}")]
		public void ParserThrowsParserExceptionForUnclosedGroups(string invalidTemplate)
		{
			Assert.That(Parser.ParseWithOptions(new ParserOptions(invalidTemplate)).Errors,
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
			Assert.That(Parser.ParseWithOptions(new ParserOptions(template)).Errors,
				Is.Not.Empty);
		}

		[Test]
		[TestCase("{{first_name}}")]
		[TestCase("{{company.name}}")]
		[TestCase("{{company.address_line_1}}")]
		[TestCase("{{name}}")]
		public void ParserShouldNotThrowForValidPath(string template)
		{
			Parser.ParseWithOptions(new ParserOptions(template));
		}


		//[Test]
		//[TestCase("1{{first name}}", 1)]
		//[TestCase("ss{{#each company.name}}\nasdf", 1)]
		//[TestCase("xzyhj{{#company.address_line_1}}\nasdf{{dsadskl-sasa@}}\n{{/each}}", 2)]
		//[TestCase("fff{{#each company.address_line_1}}\n{{dsadskl-sasa@}}\n{{/each}}", 1)]
		//[TestCase("a{{name}}dd\ndd{{/each}}dd", 1)]
		//public void ParserShouldThrowWithCharacterLocationInformation(string template, int expectedErrorCount)
		//{
		//	Assert.That(Parser.ParseWithOptions(new ParserOptions(template)).Errors,
		//		Is.Not.Empty.And.Count.EqualTo(expectedErrorCount));
		//}

		[Test]
		[TestCase("<wbr>", "{{content}}", "&lt;wbr&gt;")]
		[TestCase("<wbr>", "{{&content}}", "<wbr>")]
		public void ValueEscapingIsActivatedBasedOnValueInterpolationMustacheSyntax(string content, string template,
			string expected)
		{
			var model = new Dictionary<string, object>
			{
				{"content", content}
			};
			var value = Parser.ParseWithOptions(new ParserOptions(template, null, DefaultEncoding))
				.CreateAndStringify(model);

			Assert.That(value, Is.EqualTo(expected));
		}

		[Test]
		[TestCase("<wbr>", "{{content}}", "<wbr>")]
		[TestCase("<wbr>", "{{&content}}", "<wbr>")]
		public void ValueEscapingIsDisabledWhenRequested(string content, string template, string expected)
		{
			var model = new Dictionary<string, object>
			{
				{"content", content}
			};
			Assert.That(Parser.ParseWithOptions(new ParserOptions(template, null, DefaultEncoding, 0, true))
				.CreateAndStringify(model), Is.EqualTo(expected));
		}



		[Test]
		public async Task ParserCanPartials()
		{
			var data = new Dictionary<string, object>();
			data["Data"] = new List<object>
			{
				new Dictionary<string, object>
				{
					{
						"self", new Dictionary<string, object>
						{
							{"Test", 1}
						}
					}
				},
				new Dictionary<string, object>
				{
					{
						"self", new Dictionary<string, object>
						{
							{"Test", 2}
						}
					}
				},
				new Dictionary<string, object>
				{
					{
						"self", new Dictionary<string, object>
						{
							{"Test", 3}
						}
					}
				}
			};

			var template =
				@"{{#DECLARE TestPartial}}{{self.Test}}{{/DECLARE}}{{#EACH Data}}{{#IMPORT 'TestPartial'}}{{/EACH}}";

			var parsingOptions = new ParserOptions(template, null, DefaultEncoding);
			var parsedTemplate = await Parser.ParseWithOptionsAsync(parsingOptions);
			TestLocationsInOrder(parsedTemplate);
			var andStringify = await parsedTemplate.CreateAndStringifyAsync(data);
			Assert.That(andStringify, Is.EqualTo("123"));
			SerilalizerTests.SerializerTest.AssertDocumentItemIsSameAsTemplate(parsingOptions.Template, parsedTemplate.Document);
		}

		[Test]
		public async Task ParserCanIncludePartialsWithExplicitScope()
		{
			var data = new Dictionary<string, object>();
			data["Data"] = new List<object>
			{
				new Dictionary<string, object>
				{
					{
						"self", new Dictionary<string, object>
						{
							{"Test", 1}
						}
					}
				},
				new Dictionary<string, object>
				{
					{
						"self", new Dictionary<string, object>
						{
							{"Test", 2}
						}
					}
				},
			};

			var template =
				@"{{#DECLARE TestPartial}}{{self.Test}}{{/DECLARE}}{{#IMPORT 'TestPartial' #WITH Data.ElementAt(1)}}";

			var parsingOptions = new ParserOptions(template, null, DefaultEncoding);
			var parsedTemplate = await Parser.ParseWithOptionsAsync(parsingOptions);
			TestLocationsInOrder(parsedTemplate);
			var andStringify = await parsedTemplate.CreateAndStringifyAsync(data);
			Assert.That(andStringify, Is.EqualTo("2"));
			SerilalizerTests.SerializerTest.AssertDocumentItemIsSameAsTemplate(parsingOptions.Template, parsedTemplate.Document);
		}

		[Test]
		public async Task ParserCanIncludePartialsWithExplicitScopeFromFormatter()
		{
			var data = new Dictionary<string, object>();
			data["Data"] = new List<object>
			{
				new Dictionary<string, object>
				{
					{
						"self", new Dictionary<string, object>
						{
							{"Test", 1}
						}
					}
				},
				new Dictionary<string, object>
				{
					{
						"self", new Dictionary<string, object>
						{
							{"Test", 2}
						}
					}
				},
			};

			var template =
				@"{{#DECLARE TestPartial}}{{ExportedValue.ElementAt(1).self.Test}}{{/DECLARE}}{{#IMPORT 'TestPartial' #WITH Data.Self($name)}}";

			var parsingOptions = new ParserOptions(template, null, DefaultEncoding);
			parsingOptions.Formatters.AddSingle(new Func<object, string, object>((sourceObject, name) =>
			{
				return new Dictionary<string, object>()
				{
					{"ExportedValue", sourceObject},
					{"XNAME", name}
				};
			}), "Self");
			parsingOptions.Formatters.AddFromType(typeof(DynamicLinq));

			var parsedTemplate = await Parser.ParseWithOptionsAsync(parsingOptions);
			TestLocationsInOrder(parsedTemplate);
			var andStringify = await parsedTemplate.CreateAndStringifyAsync(data);
			Assert.That(andStringify, Is.EqualTo("2"));
			SerilalizerTests.SerializerTest.AssertDocumentItemIsSameAsTemplate(parsingOptions.Template, parsedTemplate.Document);
		}

		[Test]
		public async Task ParserCanIncludePartialsWithExplicitScopeAndDynamicImport()
		{
			var data = new Dictionary<string, object>();
			data["Data"] = new List<object>
			{
				new Dictionary<string, object>
				{
					{
						"self", new Dictionary<string, object>
						{
							{"Test", 1}
						}
					}
				},
				new Dictionary<string, object>
				{
					{
						"self", new Dictionary<string, object>
						{
							{"Test", 2}
						}
					}
				},
			};

			var template =
				"{{#DECLARE TestPartial}}" +
				"{{self.Test}}" +
				"{{/DECLARE}}" +
				"{{#VAR partialName = 'TestPartial'}}" +
				"{{#IMPORT partialName #WITH Data.ElementAt(1)}}";

			var parsingOptions = new ParserOptions(template, null, DefaultEncoding);
			var parsedTemplate = await Parser.ParseWithOptionsAsync(parsingOptions);
			TestLocationsInOrder(parsedTemplate);
			var andStringify = await parsedTemplate.CreateAndStringifyAsync(data);
			Assert.That(andStringify, Is.EqualTo("2"));
			SerilalizerTests.SerializerTest.AssertDocumentItemIsSameAsTemplate(parsingOptions.Template, parsedTemplate.Document);
		}

		[Test]
		public async Task ParserCanPartialsOneUp()
		{
			var data = new Dictionary<string, object>();
			data["Data"] = new List<object>
			{
				new Dictionary<string, object>
				{
					{
						"self", new Dictionary<string, object>
						{
							{"Test", 1}
						}
					}
				},
				new Dictionary<string, object>
				{
					{
						"self", new Dictionary<string, object>
						{
							{"Test", 2}
						}
					}
				}
			};

			data["DataOneUp"] =
				new Dictionary<string, object>
				{
					{
						"self", new Dictionary<string, object>
						{
							{"Test", "Is:"}
						}
					}
				};

			var template =
				"{{#DECLARE TestPartial}}{{../../DataOneUp.self.Test}}{{self.Test}}{{/DECLARE}}" +
				"{{#EACH Data}}" +
				"	{{#IMPORT 'TestPartial'}}" +
				"{{/EACH}}";

			var parsingOptions = new ParserOptions(template, null, DefaultEncoding);
			var parsedTemplate = await Parser.ParseWithOptionsAsync(parsingOptions);
			TestLocationsInOrder(parsedTemplate);
			var andStringify = await parsedTemplate.CreateAndStringifyAsync(data);
			Assert.That(andStringify, Is.EqualTo("	Is:1	Is:2"));
			SerilalizerTests.SerializerTest.AssertDocumentItemIsSameAsTemplate(parsingOptions.Template, parsedTemplate.Document);
		}

		[Test]
		public void ParserThrowsOnInfiniteNestedCalls()
		{
			var data = new Dictionary<string, object>();
			var template = @"{{#declare TestPartial}}{{#IMPORT 'TestPartial'}}{{/declare}}{{#IMPORT 'TestPartial'}}";

			var parsingOptions = new ParserOptions(template, null, DefaultEncoding);
			var parsedTemplate = Parser.ParseWithOptions(parsingOptions);
			TestLocationsInOrder(parsedTemplate);
			Assert.That(async () => await parsedTemplate.CreateAndStringifyAsync(data),
				Throws.Exception.TypeOf<MustachioStackOverflowException>());
			SerilalizerTests.SerializerTest.AssertDocumentItemIsSameAsTemplate(parsingOptions.Template, parsedTemplate.Document);
		}

		[Test]
		public async Task ParserCanCreateNestedPartials()
		{
			var data = new Dictionary<string, object>();
			var template =
				@"{{#declare TestPartial}}{{#declare InnerPartial}}1{{/declare}}2{{/declare}}{{#IMPORT 'TestPartial'}}{{#IMPORT 'InnerPartial'}}";

			var parsingOptions = new ParserOptions(template, null, DefaultEncoding);
			var parsedTemplate = await Parser.ParseWithOptionsAsync(parsingOptions);
			TestLocationsInOrder(parsedTemplate);
			var result = await parsedTemplate.CreateAndStringifyAsync(data);
			Assert.That(result, Is.EqualTo("21"));
			SerilalizerTests.SerializerTest.AssertDocumentItemIsSameAsTemplate(parsingOptions.Template, parsedTemplate.Document);
		}

		[Test]
		public async Task ParserCanPrintNested()
		{
			var data = new Dictionary<string, object>();
			//declare TestPartial -> Print Recursion -> If Recursion is smaller then 10 -> Print TestPartial
			//Print TestPartial
			var template =
				@"{{#declare TestPartial}}{{$recursion}}{{#$recursion.() as rec}}{{#IMPORT 'TestPartial'}}{{/rec}}{{/declare}}{{#IMPORT 'TestPartial'}}";

			var parsingOptions = new ParserOptions(template, null, DefaultEncoding);
			parsingOptions.Formatters.AddSingle<int, bool>(e => { return e < 9; });
			var parsedTemplate = await Parser.ParseWithOptionsAsync(parsingOptions);
			TestLocationsInOrder(parsedTemplate);
			var result = await parsedTemplate.CreateAndStringifyAsync(data);
			Assert.That(result, Is.EqualTo("123456789"));

			SerilalizerTests.SerializerTest.AssertDocumentItemIsSameAsTemplate(parsingOptions.Template, parsedTemplate.Document);
		}

		[Test]
		public void ParserCanParseEmailAcidTest()
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

			Assert.That(() => Parser.ParseWithOptions(new ParserOptions(emailACIDTest)), Throws.Nothing);
		}

		[Test]
		public void ParserCanProcessComplexValuePath()
		{
			Assert.That(() =>
					Parser.ParseWithOptions(new ParserOptions("{{#content}}Hello {{../Person.Name}}!{{/content}}")),
				Throws.Nothing);
		}

		[Test]
		public void ParserCanProcessComplexFormattedValuePath()
		{
			var morestachioDocumentInfo = Parser.ParseWithOptions(
				new ParserOptions("{{../Data.Data(\"e\")}}"));
			Assert.That(morestachioDocumentInfo.Errors, Is.Empty, () => morestachioDocumentInfo.Errors.Select(e => e.HelpText).Aggregate((e, f) => e + f));
			morestachioDocumentInfo = Parser.ParseWithOptions(
				new ParserOptions("{{~Data.Data(\"e\")}}"));
			Assert.That(morestachioDocumentInfo.Errors, Is.Empty, () => morestachioDocumentInfo.Errors.Select(e => e.HelpText).Aggregate((e, f) => e + f));
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
				Parser.ParseWithOptions(new ParserOptions(
					"{{#Collection}}Collection has elements{{/Collection}}{{^Collection}}Collection doesn't have elements{{/Collection}}"));
				Parser.ParseWithOptions(new ParserOptions(
					"{{^Collection}}Collection doesn't have elements{{/Collection}}{{#Collection}}Collection has elements{{/Collection}}"));
			}, Throws.Nothing);
		}

		[Test]
		public void ParserCanProcessEachConstruct()
		{
			Assert.That(() => { Parser.ParseWithOptions(new ParserOptions("{{#each ACollection}}{{.}}{{/each}}")); },
				Throws.Nothing);
		}

		[Test]
		public void ParserCanProcessHandleMultilineTemplates()
		{
			Assert.That(() => Parser.ParseWithOptions(new ParserOptions(@"{{^Collection}}Collection doesn't have
							elements{{#Collection}}Collection has
						elements{{/Collection}}{{/Collection}}")), Throws.Nothing);
		}

		[Test]
		public void ParserCanProcessSimpleConditionalGroup()
		{
			Assert.That(() =>
					Parser.ParseWithOptions(new ParserOptions("{{#Collection}}Collection has elements{{/Collection}}")),
				Throws.Nothing);
		}

		[Test]
		public void ParserCanProcessSimpleNegatedCondionalGroup()
		{
			Assert.That(() =>
					Parser.ParseWithOptions(
						new ParserOptions("{{^Collection}}Collection has no elements{{/Collection}}")),
				Throws.Nothing);
		}

		[Test]
		public void ParserCanProcessSimpleValuePath()
		{
			Assert.That(() =>
				Parser.ParseWithOptions(new ParserOptions("Hello {{Name}}!")), Throws.Nothing);
		}

		[Test]
		public void ParserChangeDefaultFormatter()
		{
			var dateTime = DateTime.UtcNow;
			var parsingOptions = new ParserOptions("{{d.().a}},{{d}}", null, DefaultEncoding);
			parsingOptions.Formatters.AddSingle<DateTime, object>(dt => new
			{
				Dt = dt,
				a = 2
			});
			var results = Parser.ParseWithOptions(parsingOptions);
			//this should not work as the Default settings for DateTime are ToString(Arg) so it should return a string and not an object
			Assert.That(results.CreateAndStringify(new Dictionary<string, object>
			{
				{
					"d", dateTime
				}
			}), Is.EqualTo("2," + dateTime));
		}

		[Test]
		public void ParserCanParseCustomTag()
		{
			var parsingOptions = new ParserOptions("{{#PI 3}}", null, DefaultEncoding);

			parsingOptions.CustomDocumentItemProviders.Add(new TagDocumentItemProvider("#PI", async
			(outputStream,
				context,
				scopeData,
				value) =>
			{
				outputStream.Write((Math.PI * int.Parse(value)).ToString());
				await Task.CompletedTask;
			}));

			var results = Parser.ParseWithOptions(parsingOptions);
			TestLocationsInOrder(results);
			Assert.That(results.CreateAndStringify(new object()), Is.EqualTo((Math.PI * 3).ToString()));
		}

		[Test]
		public void ParserCanParseCustomBlock()
		{
			var parsingOptions = new ParserOptions("{{#PI}}3{{/PI}}", null, DefaultEncoding);

			parsingOptions.CustomDocumentItemProviders.Add(new BlockDocumentItemProvider("#PI", "/PI",
			 (outputStream,
				 context,
				 scopeData,
				 value,
				 children) =>
			{
				var firstOrDefault = children.OfType<ContentDocumentItem>().FirstOrDefault();
				outputStream.Write((Math.PI * int.Parse(firstOrDefault.Value)).ToString());
				return Array.Empty<DocumentItemExecution>().ToPromise();
			}));

			var results = Parser.ParseWithOptions(parsingOptions);
			TestLocationsInOrder(results);
			Assert.That(results.CreateAndStringify(new object()), Is.EqualTo((Math.PI * 3).ToString()));
		}

		[Test]
		public void ParserThrowsParserExceptionForEachWithoutPath()
		{
			Assert.That(Parser.ParseWithOptions(new ParserOptions("{{#eachs}}{{name}}{{/each}}")).Errors, Is.Not.Empty);
		}

		[Test]
		public void ParserThrowsParserExceptionForEmptyEach()
		{
			Assert.That(Parser.ParseWithOptions(new ParserOptions("{{#each}}")).Errors, Is.Not.Empty);
		}

		[Test]
		public void ParsingThrowsAnExceptionWhenConditionalGroupsAreMismatched()
		{
			Assert.That(
				Parser.ParseWithOptions(
					new ParserOptions("{{#Collection}}Collection has elements{{/AnotherCollection}}")).Errors,
				Is.Not.Empty.And.Count.EqualTo(2));
		}

		[Test]
		public void TestCancelation()
		{
			var token = new CancellationTokenSource();
			var model = new ParserCancellationional(token);
			var results = Parser.ParseWithOptions(
				new ParserOptions("{{data.ValueA}}{{data.ValueCancel}}{{data.ValueB}}", null, DefaultEncoding));
			var template = results.CreateAndStringify(new Dictionary<string, object>
			{
				{"data", model}
			}, token.Token);
			TestLocationsInOrder(results);
			Assert.That(template, Is.EqualTo(model.ValueA + model.ValueCancel));
		}

		[Test]
		public void TestScopeOfIfInScoping()
		{
			var template = "{{#Data}}{{#Failed}}{{#IF ~Value}}{{.}}{{/IF}}{{/Failed}}{{/Data}}";
			var data = new
			{
				Data = new
				{
					Failed = "Success"
				},
				Value = "FAILED",
			};

			var parsingOptions = new ParserOptions(template, null, DefaultEncoding);
			var results = Parser.ParseWithOptions(
				parsingOptions);
			TestLocationsInOrder(results);
			var andStringify = results.CreateAndStringify(data);
			Assert.That(andStringify, Is.EqualTo("Success"));
			SerilalizerTests.SerializerTest.AssertDocumentItemIsSameAsTemplate(parsingOptions.Template, results.Document);
		}

		[Test]
		public void TestCollectionContext()
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

			var parsingOptions = new ParserOptions(template, null, DefaultEncoding);
			var results = Parser.ParseWithOptions(parsingOptions);
			TestLocationsInOrder(results);
			var genTemplate = results.CreateAndStringify(new Dictionary<string, object> { { "data", elementdata } });
			var realData = elementdata.Select(e => e.ToString()).Aggregate((e, f) => e + f);
			Assert.That(genTemplate, Is.EqualTo(realData));
			SerilalizerTests.SerializerTest.AssertDocumentItemIsSameAsTemplate(parsingOptions.Template, results.Document);
		}

		[Test]
		public void TestRepeatContext()
		{
			var template = "{{#repeat data.Count}}{{$index}},{{$first}},{{$middel}},{{$last}},{{$odd}},{{$even}}.{{/repeat}}";

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

			var parsingOptions = new ParserOptions(template, null, DefaultEncoding);
			var results = Parser.ParseWithOptions(parsingOptions);
			TestLocationsInOrder(results);

			var genTemplate = results.CreateAndStringify(new Dictionary<string, object> { { "data", elementdata } });
			var realData = elementdata.Select(e => e.ToString()).Aggregate((e, f) => e + f);
			Assert.That(genTemplate, Is.EqualTo(realData));
			SerilalizerTests.SerializerTest.AssertDocumentItemIsSameAsTemplate(parsingOptions.Template, results.Document);
		}

		[Test]
		public void TestRepeatLoopContext()
		{
			var template = "{{#REPEAT 5}}" +
						   "{{$index}}," +
						   "{{/REPEAT}}";

			var parsingOptions = new ParserOptions(template, null, DefaultEncoding)
			{
				//Timeout = TimeSpan.FromSeconds(5)
			};
			parsingOptions.Formatters.AddFromType(typeof(EqualityFormatter));
			var results = Parser.ParseWithOptions(parsingOptions);
			TestLocationsInOrder(results);
			var genTemplate = results.CreateAndStringify(new object());
			Assert.That(genTemplate, Is.EqualTo("0,1,2,3,4,"));
			SerilalizerTests.SerializerTest.AssertDocumentItemIsSameAsTemplate(parsingOptions.Template, results.Document);
		}


		[Test]
		public void TestWhileLoopContext()
		{
			var template = @"{{#WHILE $index.SmallerAs(5) | -}}
						   {{$index}},
						   {{- | /WHILE}}";

			var parsingOptions = new ParserOptions(template, null, DefaultEncoding)
			{
				//Timeout = TimeSpan.FromSeconds(5)
			};
			parsingOptions.Formatters.AddFromType(typeof(EqualityFormatter));
			var results = Parser.ParseWithOptions(parsingOptions);
			TestLocationsInOrder(results);

			var genTemplate = results.CreateAndStringify(new object());
			Assert.That(genTemplate, Is.EqualTo("0,1,2,3,4,"));
			SerilalizerTests.SerializerTest.AssertDocumentItemIsSameAsTemplate(parsingOptions.Template, results.Document);
		}

		[Test]
		public void TestDoLoopContext()
		{
			var template = @"{{#DO $index.SmallerAs(5)}}
							 {{- | $index}},
							 {{- | /DO}}";

			var parsingOptions = new ParserOptions(template, null, DefaultEncoding)
			{
				//Timeout = TimeSpan.FromSeconds(5)
			};
			parsingOptions.Formatters.AddFromType(typeof(EqualityFormatter));
			var parsedTemplate = Parser.ParseWithOptions(parsingOptions);
			var genTemplate = parsedTemplate.CreateAndStringify(new object());
			Assert.That(genTemplate, Is.EqualTo("0,1,2,3,4,5,"));
			SerilalizerTests.SerializerTest.AssertDocumentItemIsSameAsTemplate(parsingOptions.Template, parsedTemplate.Document);
		}

		[Test]
		public void TestCanRemoveLineBreaks()
		{
			var template = @"{{#TNLS}}

Test{{#NL}}";

			var parsingOptions = new ParserOptions(template, null, DefaultEncoding)
			{
				//Timeout = TimeSpan.FromSeconds(5)
			};
			parsingOptions.Formatters.AddFromType(typeof(EqualityFormatter));
			var results = Parser.ParseWithOptions(parsingOptions);
			TestLocationsInOrder(results);
			var genTemplate = results.CreateAndStringify(new object());
			Assert.That(genTemplate, Is.EqualTo(@"Test
"));
		}

		[Test]
		public void TestCanRemoveLineBreaksWithinKeyword()
		{
			var template = @"{{data|-}}
Static

{{-|data}}";

			var parsingOptions = new ParserOptions(template, null, DefaultEncoding)
			{
				//Timeout = TimeSpan.FromSeconds(5)
			};
			parsingOptions.Formatters.AddFromType(typeof(EqualityFormatter));
			var results = Parser.ParseWithOptions(parsingOptions);
			TestLocationsInOrder(results);
			var genTemplate = results.CreateAndStringify(new
			{
				data = "World"
			});
			Assert.That(genTemplate, Is.EqualTo(@"WorldStaticWorld"));
		}

		[Test]
		public void TestCanRemoveLineBreaksWithOption()
		{
			var template = @"{{#SET OPTION TrimTailing = true}}{{#SET OPTION TrimLeading = true}}
{{data}}
Static

{{data}}";

			var parsingOptions = new ParserOptions(template, null, DefaultEncoding)
			{
				//Timeout = TimeSpan.FromSeconds(5)
			};
			parsingOptions.Formatters.AddFromType(typeof(EqualityFormatter));
			var results = Parser.ParseWithOptions(parsingOptions);
			TestLocationsInOrder(results);
			var genTemplate = results.CreateAndStringify(new
			{
				data = "World"
			});
			Assert.That(genTemplate, Is.EqualTo(@"WorldStaticWorld"));
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