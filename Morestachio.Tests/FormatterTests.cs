using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Morestachio.Attributes;
using Morestachio.Helper;
using Morestachio.Linq;
using Morestachio.Tests;
using NUnit.Framework;

namespace Morestachio.Formatter.Framework.Tests
{
	[TestFixture]
	public class FormatterTests
	{
		public static Encoding DefaultEncoding { get; set; } = new UnicodeEncoding(true, false, false);

		[Test]
		public void TestStringConversion()
		{
			var options = new ParserOptions("{{data.ExpectInt()}}", null, DefaultEncoding);
			options.Formatters.AddFromType(typeof(StringFormatter));
			var template = Parser.ParseWithOptions(options);

			var andStringify = template.CreateAndStringify(new Dictionary<string, object>() { { "data", 123 } });
			Assert.That(andStringify, Is.EqualTo(123.ToString("X2")));
		}

		[Test]
		public void FormatterCanFormatObjectTwice()
		{
			var options = new ParserOptions("{{Plus(NumberB, NumberB)}}", null, DefaultEncoding);
			options.Formatters.AddFromType(typeof(StringFormatter));
			var template = Parser.ParseWithOptions(options);

			var andStringify = template.CreateAndStringify(new Dictionary<string, object>()
			{
				{ "NumberA", 5 },
				{ "NumberB", 6 }
			});
			Assert.That(andStringify, Is.EqualTo("12"));
		}


		[Test]
		public void TestSingleNamed()
		{
			var options = new ParserOptions("{{data.reverse()}}", null, DefaultEncoding);
			options.Formatters.AddFromType(typeof(StringFormatter));
			var template = Parser.ParseWithOptions(options);

			var andStringify = template.CreateAndStringify(new Dictionary<string, object>() { { "data", "Test" } });
			Assert.That(andStringify, Is.EqualTo("tseT"));
		}


		[Test]
		public void TestRest()
		{
			var options = new ParserOptions("{{data.rest('other', 'and', 'more')}}", null, DefaultEncoding);
			options.Formatters.AddFromType(typeof(StringFormatter));
			var template = Parser.ParseWithOptions(options);

			var andStringify = template.CreateAndStringify(new Dictionary<string, object>() { { "data", "Test" } });
			Assert.That(andStringify, Is.EqualTo("ORIGINAL: Test REST:andmore"));
		}

		[Test]
		public void TestAsync()
		{
			var options = new ParserOptions("{{data.reverseAsync()}}", null, DefaultEncoding);
			options.Formatters.AddFromType(typeof(StringFormatter));
			var template = Parser.ParseWithOptions(options);

			var andStringify = template.CreateAndStringify(new Dictionary<string, object>() { { "data", "Test" } });
			Assert.That(andStringify, Is.EqualTo("tseT"));
		}

		[Test]
		public void TestOptionalArgument()
		{
			var options = new ParserOptions("{{data.optional()}}", null, DefaultEncoding);
			options.Formatters.AddFromType(typeof(StringFormatter));
			var template = Parser.ParseWithOptions(options);

			var andStringify = template.CreateAndStringify(new Dictionary<string, object>() { { "data", "Test" } });
			Assert.That(andStringify, Is.EqualTo("OPTIONAL Test"));
		}

		[Test]
		public void TestDefaultArgument()
		{
			var options = new ParserOptions("{{data.defaultValue()}}", null, DefaultEncoding);
			options.Formatters.AddFromType(typeof(StringFormatter));
			var template = Parser.ParseWithOptions(options);

			var andStringify = template.CreateAndStringify(new Dictionary<string, object>() { { "data", "Test" } });
			Assert.That(andStringify, Is.EqualTo("DEFAULT Test"));
		}

		[Test]
		public void TestNamed()
		{
			var options = new ParserOptions("{{data.reverse-arg('TEST')}}", null, DefaultEncoding);
			options.Formatters.AddFromType(typeof(StringFormatter));
			var template = Parser.ParseWithOptions(options);

			var andStringify = template.CreateAndStringify(new Dictionary<string, object>() { { "data", "Test" } });
			Assert.That(andStringify, Is.EqualTo("TEST"));
		}

		[Test]
		public void GenericsTest()
		{
			var options = new ParserOptions("{{data.fod()}}", null, DefaultEncoding);
			options.Formatters.AddFromType(typeof(StringFormatter));
			options.Formatters.AddFromType(typeof(ListFormatter));
			var template = Parser.ParseWithOptions(options);

			var andStringify = template.CreateAndStringify(new Dictionary<string, object>() { { "data", new[] { "TEST", "test" } } });
			Assert.That(andStringify, Is.EqualTo("TEST"));
		}


		[Test]
		public void ParserCanChainFormat()
		{
			var data = DateTime.UtcNow;
			var parsingOptions = new ParserOptions("{{#data}}{{.('d').fnc()}}{{/data}}", null, DefaultEncoding);
			parsingOptions.Formatters.AddSingle(new Func<string, string>(s => "TEST"), "fnc");
			var results = Parser.ParseWithOptions(parsingOptions);
			var result = results.CreateAndStringify(new Dictionary<string, object> { { "data", data } });
			Assert.That(result, Is.EqualTo("TEST"));
		}

		[Test]
		public void ParserCanChainFormatSubExpression()
		{
			var data = new
			{
				field = "field value",
				reference = new
				{
					data = "reference data value"
				}
			};
			var parsingOptions = new ParserOptions("{{#data}}{{.('template value').('template value', reference.data)}}{{/data}}", null, DefaultEncoding);
			parsingOptions.Formatters.AddSingle(
				new Func<object, object, object, object>((source, tempValue, reference) => reference));

			parsingOptions.Formatters.AddSingle(
				new Func<object, object, object>((source, tempValue) => source));
			var results = Parser.ParseWithOptions(parsingOptions);
			var result = results.CreateAndStringify(new Dictionary<string, object>
			{
				{"data", data},
			});
			Assert.That(result, Is.EqualTo(data.reference.data));
		}

		[Test]
		public void ParserCanChainFormatSubExpressionFromEach()
		{
			var data = new
			{
				field = "field value",
				reference = new List<dynamic>()
				{
					{
						new
						{
							displayValue = "display data value",
							formatterValue = "formatter data value",
						}
					},
				}
			};
			var parsingOptions = new ParserOptions("{{#each data.reference}}{{displayValue('template value').('template value', formatterValue)}}{{/each}}", null, DefaultEncoding);
			parsingOptions.Formatters.AddSingle(
				new Func<object, object, object, object>((source, tempValue, reference) => reference));

			parsingOptions.Formatters.AddSingle(
				new Func<object, object, object>((source, tempValue) => source));
			var results = Parser.ParseWithOptions(parsingOptions);
			var result = results.CreateAndStringify(new Dictionary<string, object>
			{
				{"data", data},
			});
			Assert.That(result, Is.EqualTo(data.reference[0].formatterValue));
		}

		[Test]
		public void ParserCanChainFormatWithLineBreak()
		{
			var data = DateTime.UtcNow;
			var parsingOptions = new ParserOptions(@"{{#data}}{{
	.  (   'd'  )
																.()
}}{{/data}}", null, DefaultEncoding);
			parsingOptions.Formatters.AddSingle(new Func<string, string>(s => "TEST"));
			var results = Parser.ParseWithOptions(parsingOptions);
			var result = results.CreateAndStringify(new Dictionary<string, object> { { "data", data } });
			Assert.That(result, Is.EqualTo("TEST"));
		}

		[Test]
		public void ParserCanTransferChains()
		{
			var data = "d";
			var parsingOptions = new ParserOptions("{{#data}}{{.('(d(a))')}}{{/data}}", null, DefaultEncoding);
			parsingOptions.Formatters.AddSingle(new Func<string, string, string>((s, s1) => s1));

			var results = Parser.ParseWithOptions(parsingOptions);
			var result = results.CreateAndStringify(new Dictionary<string, object> { { "data", data } });
			Assert.That(result, Is.EqualTo("(d(a))"));
		}

		[Test]
		public void ParserCanFormatMultipleUnnamedWithoutResult()
		{
			var data = 123123123;
			var formatterResult = "";
			var parsingOptions =
				new ParserOptions(
					"{{#data}}{{.('test', 'arg', 'arg, arg', ' spaced ', ' spaced with quote \\' ' , . )}}{{/data}}",
					null, DefaultEncoding);

			parsingOptions.Formatters.AddSingle(new Action<int, string[]>(
				(self, test) => { Assert.Fail("Should not be invoked"); }));

			parsingOptions.Formatters.AddSingle(new Action<int, string, string, string, string, string, int>(
				(self, test, arg, argarg, spacedArg, spacedWithQuote, refSelf) =>
				{
					formatterResult = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}", self, test, arg, argarg, spacedArg,
						spacedWithQuote,
						refSelf);
				}));

			var results = Parser.ParseWithOptions(parsingOptions);
			var result = results.CreateAndStringify(new Dictionary<string, object> { { "data", data } });
			Assert.That(result, Is.Empty);
			Assert.That(formatterResult,
				Is.EqualTo(@"123123123|test|arg|arg, arg| spaced | spaced with quote ' |123123123"));
		}

		[Test]
		public void ParserCanFormatMultipleUnnamed()
		{
			var data = 123123123;
			var parsingOptions =
				new ParserOptions(
					"{{#data}}{{.('test', 'arg', 'arg, arg', ' spaced ', ' spaced with quote \\' ' , .)}}{{/data}}",
					null, DefaultEncoding);


			parsingOptions.Formatters.AddSingle(
				new Func<int, string, string, string, string, string, int, string>(
					(self, test, arg, argarg, spacedArg, spacedWithQuote, refSelf) =>
					{
						return string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}", self, test, arg, argarg, spacedArg,
							spacedWithQuote,
							refSelf);
					}));

			var results = Parser.ParseWithOptions(parsingOptions);
			var result = results.CreateAndStringify(new Dictionary<string, object> { { "data", data } });
			Assert.That(result, Is.EqualTo("123123123|test|arg|arg, arg| spaced | spaced with quote ' |123123123"));
		}

		[Test]
		public void ParserCanFormatMultipleUnnamedParams()
		{
			var data = 123123123;
			var parsingOptions =
				new ParserOptions(
					"{{#data}}{{.( 'arg', 'arg, arg', ' spaced ', [testArgument]'test', ' spaced with quote \\' ' , .)}}{{/data}}",
					null, DefaultEncoding);
			parsingOptions.Formatters.AddSingle(
				new Func<int, string, object[], string>(UnnamedParamsFormatter));

			var results = Parser.ParseWithOptions(parsingOptions);
			var result = results.CreateAndStringify(new Dictionary<string, object> { { "data", data } });
			Assert.That(result, Is.EqualTo("123123123|test|arg|arg, arg| spaced | spaced with quote ' |123123123"));
		}

		private string UnnamedParamsFormatter(int self, string testArgument, params object[] other)
		{
			return string.Format("{0}|{1}|{2}", self, testArgument, other.Aggregate((e, f) => e + "|" + f));
		}

		[Test]
		public void ParserCanFormatMultipleNamed()
		{
			var data = 123123123;
			var parsingOptions =
				new ParserOptions(
					"{{#data}}{{.([refSelf] ., 'arg',[Fob]'test', [twoArgs]'arg, arg', [anySpaceArg]' spaced ')}}{{/data}}",
					null, DefaultEncoding);
			parsingOptions.Formatters.AddSingle(
				new Func<int, string, string, string, string, int, string>(NamedFormatter));

			var results = Parser.ParseWithOptions(parsingOptions);
			var result = results.CreateAndStringify(new Dictionary<string, object> { { "data", data } });
			Assert.That(result, Is.EqualTo("123123123|test|arg|arg, arg| spaced |123123123"));
		}

		private string NamedFormatter(int self, [FormatterArgumentName("Fob")] string testArgument, string arg,
			string twoArgs, string anySpaceArg, int refSelf)
		{
			return string.Format("{0}|{1}|{2}|{3}|{4}|{5}", self, testArgument, arg, twoArgs, anySpaceArg, refSelf);
		}

		[Test]
		public void ParserCanCheckCanFormat()
		{
			var data = "d";
			var parsingOptions = new ParserOptions("{{#data}}{{.('(d(a))')}}{{/data}}", null, DefaultEncoding);
			parsingOptions.Formatters.AddSingle(
				new Func<string, string, string, string>((s, inv, inva) => throw new Exception("A")));

			parsingOptions.Formatters.AddSingle(new Func<string, string>(s =>
				throw new Exception("Wrong Ordering")));
			parsingOptions.Formatters.AddSingle(new Action<string>(s =>
				throw new Exception("Wrong Return Ordering")));
			parsingOptions.Formatters.AddSingle(new Func<string, string, string>((s, inv) => inv));

			var results = Parser.ParseWithOptions(parsingOptions);
			var result = results.CreateAndStringify(new Dictionary<string, object> { { "data", data } });
			Assert.That(result, Is.EqualTo("(d(a))"));
		}

		[Test]
		public void ParserCanChainWithAndWithoutFormat()
		{
			var data = DateTime.UtcNow;
			var parsingOptions = new ParserOptions("{{data.().TimeOfDay.Ticks().()}}", null, DefaultEncoding);
			parsingOptions.Formatters.AddSingle<string, string>(s => s);
			parsingOptions.Formatters.AddSingle<DateTime, DateTime>(s => s);
			parsingOptions.Formatters.AddSingle<long, TimeSpan>(s => new TimeSpan(s));
			var results = Parser.ParseWithOptions(parsingOptions);
			var result = results.CreateAndStringify(new Dictionary<string, object> { { "data", data } });
			Assert.That(result, Is.EqualTo(new TimeSpan(data.TimeOfDay.Ticks).ToString()));
		}

		[Test]
		public void ParserCanFormatAndCombine()
		{
			var data = DateTime.UtcNow;
			var results =
				Parser.ParseWithOptions(new ParserOptions("{{data('d').Year}},{{data}}", null, DefaultEncoding));
			//this should compile as its valid but not work as the Default
			//settings for DateTime are ToString(Arg) so it should return a string and not an object
			Assert.That(results
					.CreateAndStringify(new Dictionary<string, object> { { "data", data } }),
				Is.EqualTo(string.Empty + "," + data));
		}

		[Test]
		public void ParserCanFormatArgumentWithExpression()
		{
			var dt = DateTime.Now;
			var extendedParseInformation =
				Parser.ParseWithOptions(new ParserOptions("{{data.(testFormat)}}", null, DefaultEncoding));

			var format = "yyyy.mm";
			var andStringify = extendedParseInformation.CreateAndStringify(new Dictionary<string, object>
			{
				{"data", dt},
				{"testFormat", format}
			});

			Assert.That(andStringify, Is.EqualTo(dt.ToString(format)));
		}

		[Test]
		public void ParserCanFormatArgumentWithNestedExpression()
		{
			var dt = DateTime.Now;
			var extendedParseInformation =
				Parser.ParseWithOptions(new ParserOptions("{{data.(testFormat.inner)}}", null, DefaultEncoding));

			var format = new Dictionary<string, object>
			{
				{"inner", "yyyy.mm"}
			};
			var andStringify = extendedParseInformation.CreateAndStringify(new Dictionary<string, object>
			{
				{"data", dt},
				{"testFormat", format}
			});

			Assert.That(andStringify, Is.EqualTo(dt.ToString(format["inner"].ToString())));
		}

		[Test]
		public void ParserCanFormatArgumentWithSubExpression()
		{
			var dt = DateTime.Now;
			var parsingOptions = new ParserOptions("{{data.(tt.('d'), \"test\")}}", null, DefaultEncoding);
			var format = "yyyy.mm";
			var formatterCalled = false;
			var formatter2Called = false;
			parsingOptions.Formatters.AddSingle<int, string, string>((sourceValue, testString) =>
			{
				Assert.That(testString, Is.EqualTo("d"));
				formatterCalled = true;
				return format;
			});
			parsingOptions.Formatters.AddSingle(new Func<DateTime, string, string, string>(
				(sourceValue, testString2, shouldBed) =>
				{
					Assert.That(shouldBed, Is.EqualTo("test"));
					Assert.That(testString2, Is.EqualTo(format));
					formatter2Called = true;
					return sourceValue.ToString(testString2);
				}));

			var extendedParseInformation =
				Parser.ParseWithOptions(parsingOptions);

			var andStringify = extendedParseInformation.CreateAndStringify(new Dictionary<string, object>
			{
				{"data", dt},
				{"tt", 19191919}
			});
			Assert.That(formatterCalled, Is.True, "The  formatter was not called");
			Assert.That(formatter2Called, Is.True, "The Date formatter was not called");
			Assert.That(andStringify, Is.EqualTo(dt.ToString(format)));
		}

		[Test]
		public void ParserCanFormatArgumentWithSubExpressionMultiple()
		{
			var dt = DateTime.Now;
			var dictionary = new Dictionary<string, object>
			{
				{"data", dt},
				{"testFormat", 19191919},
				{"by", 10L}
			};
			var parsingOptions = new ParserOptions("{{data.(testFormat.('d'), \"test\").('pad-left', by.(by, 'f'))}}",
				null, DefaultEncoding);
			var format = "yyyy.mm";
			var formatterCalled = false;
			var formatter2Called = false;
			var formatter3Called = false;
			parsingOptions.Formatters.AddSingle<int, string, string>((sourceValue, testString) =>
			{
				Assert.That(testString, Is.EqualTo("d"));
				formatterCalled = true;
				return format;
			});
			parsingOptions.Formatters.AddSingle(new Func<long, long, string, int>(
				(sourceValue, testString, f) =>
				{
					Assert.That(testString, Is.EqualTo(sourceValue));
					Assert.That(f, Is.EqualTo("f"));
					formatterCalled = true;
					return (int)sourceValue;
				}));
			parsingOptions.Formatters.AddSingle(new Func<DateTime, string, string, string>(
				(sourceValue, testString2, shouldBed) =>
				{
					Assert.That(shouldBed, Is.EqualTo("test"));
					Assert.That(testString2, Is.EqualTo(format));
					formatter2Called = true;
					return sourceValue.ToString(testString2);
				}));
			parsingOptions.Formatters.AddSingle(new Func<string, string, int, string>(
				(sourceValue, name, number) =>
				{
					Assert.That(sourceValue, Is.EqualTo(dt.ToString(format)));
					Assert.That(name, Is.EqualTo("pad-left"));
					Assert.That(number, Is.EqualTo(dictionary["by"]));

					formatter3Called = true;
					return sourceValue.PadLeft(number);
				}));

			var extendedParseInformation =
				Parser.ParseWithOptions(parsingOptions);
			var andStringify = extendedParseInformation.CreateAndStringify(dictionary);
			Assert.That(formatterCalled, Is.True, "The formatter was not called");
			Assert.That(formatter2Called, Is.True, "The Date formatter was not called");
			Assert.That(formatter3Called, Is.True, "The Pad formatter was not called");
			Assert.That(andStringify, Is.EqualTo(dt.ToString(format).PadLeft(int.Parse(dictionary["by"].ToString()))));
		}

		[Test]
		public void TemplateIfDoesNotScopeWithFormatter()
		{
			var template =
				@"{{#IF data.()}}{{.}}{{/IF}}";

			var parsingOptions = new ParserOptions(template, null, ParserFixture.DefaultEncoding);
			parsingOptions.Formatters.AddSingle(new Func<string, bool>(f => f == "test"));
			var parsedTemplate =
				Parser.ParseWithOptions(parsingOptions);

			var model = new Dictionary<string, object>()
			{
				{"data", "test" },
				{"root", "tset" }
			};

			var result = parsedTemplate.Create(model).Stream.Stringify(true, ParserFixture.DefaultEncoding);

			Assert.AreEqual(model.ToString(), result);
		}



		[Test]
		public void TemplateIfDoesNotScopeToRootWithFormatter()
		{
			var template =
				@"{{#data}}{{#each data4.dataList}}{{#IF data2.()}}{{.}}{{/IF}}{{/each}}{{/data}}";

            var parsingOptions = new ParserOptions(template, null, ParserFixture.DefaultEncoding);
			parsingOptions.Formatters.AddSingle(new Func<string, bool>(f => f == "test"));
			var parsedTemplate =
				Parser.ParseWithOptions(parsingOptions);

            var model = new Dictionary<string, object>()
            {
                {
                    "data", new Dictionary<string, object>()
                    {
                        {
                            "data2", new Dictionary<string, object>()
                            {
                                {"condition", "true"}
                            }
                        },
                        {
                            "data3", new Dictionary<string, object>()
                            {
                                {"dataList", new List<string>{"TE","ST"}}
                            }
                        }
                    }
                },
			};

			var result = parsedTemplate.Create(model).Stream.Stringify(true, ParserFixture.DefaultEncoding);

			Assert.AreEqual("TEST", result);
		}

		[Test]
		public void TemplateInvertedIfDoesNotScopeWithFormatter()
		{
			var template =
				@"{{^IF data.()}}{{.}}{{/IF}}";

			var parsingOptions = new ParserOptions(template, null, ParserFixture.DefaultEncoding);
			parsingOptions.Formatters.AddSingle(new Func<string, bool>(f => f != "test"));
			var parsedTemplate =
				Parser.ParseWithOptions(parsingOptions);

			var model = new Dictionary<string, object>()
			{
				{"data", "test" },
				{"root", "tset" }
			};

			var result = parsedTemplate.Create(model).Stream.Stringify(true, ParserFixture.DefaultEncoding);

			Assert.AreEqual(model.ToString(), result);
		}
	}
}
