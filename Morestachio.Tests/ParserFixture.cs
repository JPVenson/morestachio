using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core.Parser;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Morestachio.Attributes;
using Morestachio.Formatter;
using Morestachio.Framework;
using Morestachio.Formatter.Framework;
using Morestachio.Formatter.Predefined;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Visitors;
using Morestachio.Helper;
using Morestachio.Linq;
using Morestachio.ParserErrors;
using Newtonsoft.Json;
using NUnit.Framework;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace Morestachio.Tests
{
	[TestFixture]
	public class ParserFixture
	{
		public static Encoding DefaultEncoding { get; set; } = new UnicodeEncoding(true, false, false);

		[Test]
		[TestCase(".(.('').())")]
		[TestCase(".a(.b(.c().d()))")]
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
			var expressions = ExpressionTokenizer.ParseExpressionOrString(query, context);
			Assert.That(expressions, Is.Not.Null);

			var visitor = new ToParsableStringExpressionVisitor();
			expressions.Visit(visitor);

			Assert.That(visitor.StringBuilder.ToString(), Is.EqualTo(query));
			Assert.That(context.Errors, Is.Empty, () => context.Errors.GetErrorText());
		}
		
		[Test]
		[TestCase("d.f(fA.('', e.('')))")]
		public void TestExpressionParserDbg(string query)
		{
			var context = TokenzierContext.FromText(query);
			var expressions = ExpressionTokenizer.ParseExpressionOrString(query, context);
			Assert.That(expressions, Is.Not.Null);
			Assert.That(context.Errors, Is.Empty, () => context.Errors.GetErrorText());
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
			var result = results.CreateAndStringify(new Dictionary<string, object>
			{
				{ "data", date },
			});
			Assert.That(result, Is.EqualTo("|" + date.ToString("G")));
		}

		[Test]
		public void ParserCanVariableExpressionWithFormats()
		{
			var parsingOptions = new ParserOptions("{{#var f = data.('G').PadLeft(123)}}|{{f}}", null,
				DefaultEncoding);
			parsingOptions.Formatters.AddSingle((string value, int nr) =>
			{
				return value.PadLeft(nr);
			}, "PadLeft");
			var results =
				Parser.ParseWithOptions(parsingOptions);
			var date = DateTime.Now;
			var result = results.CreateAndStringify(new Dictionary<string, object>
			{
				{ "data", date },
			});
			Assert.That(result, Is.EqualTo("|" + date.ToString("G").PadLeft(123)));
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
			var result = results.CreateAndStringify(new Dictionary<string, object>
			{
				{ "data", date },
			});
			Assert.That(result, Is.EqualTo(date.ToString("G").PadLeft(123)));
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
			var result = results.CreateAndStringify(new Dictionary<string, object>
			{
				{ "data", date },
			});
			Assert.That(result, Is.EqualTo("".PadLeft(123)));
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
			var result = results.CreateAndStringify(new Dictionary<string, object>
			{
			});
			Assert.That(result, Is.EqualTo(1.123.ToString("F5")));
		}

		[Test]
		public void TestRootAccess()
		{
			var templateWorking = "{{#EACH data.testList.Select()}}" +
								  "{{#var eachValue = ~data.testInt}}" +
								  "{{/EACH}}" +
								  "{{eachValue}} = {{~data.testInt}} = {{data.testInt}}";

			var parsingOptionsWorking = new ParserOptions(templateWorking, null, DefaultEncoding);
			parsingOptionsWorking.Formatters.AddFromType(typeof(DynamicLinq));
			parsingOptionsWorking.Formatters.AddFromType(typeof(FormatterTests.NumberFormatter));
			var parsedTemplateWorking = Parser.ParseWithOptions(parsingOptionsWorking);

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
	
			var result = parsedTemplateWorking.Create(modelWorking).Stream.Stringify(true, DefaultEncoding);
			Assert.AreEqual("2 = 2 = 2", result);
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
			var results = Parser.ParseWithOptions(new ParserOptions(
				"{{#data}}{{.(\"" + dtFormat + "\")}}{{/data}},{{data}}",
				null, DefaultEncoding));
			var result = results.CreateAndStringify(new Dictionary<string, object> { { "data", data } });
			Assert.That(result, Is.EqualTo(data.ToString(dtFormat) + "," + data));
		}

		[Test]
		[TestCase("{{data.(d))}}", 1, Ignore = "Currently its not possible to evaluate this info")]
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
				Is.Not.Empty.And.Count.EqualTo(expectedNoOfExceptions),
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
		[TestCase("<wbr>", "{{{content}}}", "<wbr>")]
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
		[TestCase("<wbr>", "{{{content}}}", "<wbr>")]
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
				@"{{#declare TestPartial}}{{self.Test}}{{/declare}}{{#each Data}}{{#include TestPartial}}{{/each}}";

			var parsed = Parser.ParseWithOptions(new ParserOptions(template, null, DefaultEncoding));
			var andStringify = await parsed.CreateAndStringifyAsync(data);
			Assert.That(andStringify, Is.EqualTo("123"));
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
				@"{{#declare TestPartial}}{{../../DataOneUp.self.Test}}{{self.Test}}{{/declare}}{{#each Data}}{{#include TestPartial}}{{/each}}";

			var parsed = Parser.ParseWithOptions(new ParserOptions(template, null, DefaultEncoding));
			var andStringify = await parsed.CreateAndStringifyAsync(data);
			Assert.That(andStringify, Is.EqualTo("Is:1Is:2"));
		}

		[Test]
		public void ParserThrowsOnInfiniteNestedCalls()
		{
			var data = new Dictionary<string, object>();
			var template = @"{{#declare TestPartial}}{{#include TestPartial}}{{/declare}}{{#include TestPartial}}";

			var parsed = Parser.ParseWithOptions(new ParserOptions(template, null, DefaultEncoding));
			Assert.That(async () => await parsed.CreateAndStringifyAsync(data),
				Throws.Exception.TypeOf<MustachioStackOverflowException>());
		}

		[Test]
		public async Task ParserCanCreateNestedPartials()
		{
			var data = new Dictionary<string, object>();
			var template =
				@"{{#declare TestPartial}}{{#declare InnerPartial}}1{{/declare}}2{{/declare}}{{#include TestPartial}}{{#include InnerPartial}}";

			var parsed = Parser.ParseWithOptions(new ParserOptions(template, null, DefaultEncoding));
			var result = await parsed.CreateAndStringifyAsync(data);
			Assert.That(result, Is.EqualTo("21"));
		}

		[Test]
		public async Task ParserCanPrintNested()
		{
			var data = new Dictionary<string, object>();
			//declare TestPartial -> Print Recursion -> If Recursion is smaller then 10 -> Print TestPartial
			//Print TestPartial
			var template =
				@"{{#declare TestPartial}}{{$recursion}}{{#$recursion.() as rec}}{{#include TestPartial}}{{/rec}}{{/declare}}{{#include TestPartial}}";

			var parsingOptions = new ParserOptions(template, null, DefaultEncoding);
			parsingOptions.Formatters.AddSingle<int, bool>(e => { return e < 9; });
			var parsed = Parser.ParseWithOptions(parsingOptions);
			var result = await parsed.CreateAndStringifyAsync(data);
			Assert.That(result, Is.EqualTo("123456789"));
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
			var extendedParseInformation = Parser.ParseWithOptions(
				new ParserOptions("{{data.ValueA}}{{data.ValueCancel}}{{data.ValueB}}", null, DefaultEncoding));
			var template = extendedParseInformation.CreateAndStringify(new Dictionary<string, object>
			{
				{"data", model}
			}, token.Token);
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

			var extendedParseInformation = Parser.ParseWithOptions(
				new ParserOptions(template, null, DefaultEncoding));
			var andStringify = extendedParseInformation.CreateAndStringify(data);
			Assert.That(andStringify, Is.EqualTo("Success"));
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

			var parsedTemplate = Parser.ParseWithOptions(new ParserOptions(template, null, DefaultEncoding));
			var genTemplate = parsedTemplate.CreateAndStringify(new Dictionary<string, object> { { "data", elementdata } });
			var realData = elementdata.Select(e => e.ToString()).Aggregate((e, f) => e + f);
			Assert.That(genTemplate, Is.EqualTo(realData));
		}

		[Test]
		public void TestWhileLoopContext()
		{
			var template = "{{#VAR condition = true}}" +
			               "{{#WHILE condition}}" +
			               "{{$index}}," +
			               "{{#IF $index.fnc_Equals(5)}}{{#VAR condition = false}}{{/IF}}" +
			               "{{/WHILE}}";

			var parsingOptions = new ParserOptions(template, null, DefaultEncoding)
			{
				//Timeout = TimeSpan.FromSeconds(5)
			};
			parsingOptions.Formatters.AddFromType(typeof(EqualityFormatter));
			var parsedTemplate = Parser.ParseWithOptions(parsingOptions);
			var genTemplate = parsedTemplate.CreateAndStringify(new object());
			Assert.That(genTemplate, Is.EqualTo("0,1,2,3,4,5,"));
		}

		[Test]
		public void TestDoLoopContext()
		{
			var template = "{{#VAR condition = true}}" +
			               "{{#DO condition}}" +
			               "{{$index}}," +
			               "{{#IF $index.fnc_Equals(5)}}{{#VAR condition = false}}{{/IF}}" +
			               "{{/DO}}";

			var parsingOptions = new ParserOptions(template, null, DefaultEncoding)
			{
				//Timeout = TimeSpan.FromSeconds(5)
			};
			parsingOptions.Formatters.AddFromType(typeof(EqualityFormatter));
			var parsedTemplate = Parser.ParseWithOptions(parsingOptions);
			var genTemplate = parsedTemplate.CreateAndStringify(new object());
			Assert.That(genTemplate, Is.EqualTo("0,1,2,3,4,5,"));
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