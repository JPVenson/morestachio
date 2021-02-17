using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Morestachio.Formatter.Framework;
using Morestachio.Formatter.Framework.Attributes;
using Morestachio.Helper;
using Morestachio.Linq;
using NUnit.Framework;

namespace Morestachio.Tests
{
	[TestFixture(ParserOptionTypes.UseOnDemandCompile)]
	[TestFixture(ParserOptionTypes.Precompile)]
	[Parallelizable(ParallelScope.All)]
	public class FormatterTests
	{
		private readonly ParserOptionTypes _opts;

		public FormatterTests(ParserOptionTypes opts)
		{
			_opts = opts;
		}

		public class MainTestClass
		{
			public TestClass Entity { get; set; }
		}

		public class TestClass
		{
			public decimal SomeValue2 { get; set; }
			public decimal SomeValue3 { get; set; }
		}

		[Test]
		[TestCase("", false)]
		[TestCase(null, false)]
		[TestCase("Test", false)]
		[TestCase("Data", false)]
		[TestCase("69Nice", true)]
		[TestCase("Nice69", false)]
		[TestCase("d d", true)]
		[TestCase("Data_Das", true)]
		[TestCase("Test", false)]
		[TestCase("?", true)]
		[TestCase("Any thing else", true)]
		public void TestFormatterNames(string name, bool expectTobeValid)
		{
			Assert.That(() => new MorestachioFormatterAttribute(name, "").ValidateFormatterName(),
				Is.EqualTo(!expectTobeValid));
		}

		[Test]
		public async Task TestSelect()
		{
			var template = "{{#SCOPE data}}" +
						   "{{#each someList.Select('Entity')}}" +
						   "{{SomeValue2}}*{{SomeValue3}}={{SomeValue2.Multiply(SomeValue3)}}" +
						   "{{/each}}" +
						   "{{/SCOPE}}";

			var data = new Dictionary<string, object>()
			{
				{
					"data", new Dictionary<string, object>()
					{
						{
							"someList",
							new MainTestClass[1]
							{
								new MainTestClass()
								{
									Entity = new TestClass
									{
										SomeValue2 = 2,
										SomeValue3 = 3
									}
								}
							}
						}
					}
				}
			};

			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddFromType(typeof(DynamicLinq));
				options.Formatters.AddFromType(typeof(NumberFormatter));
			});
			Assert.That(result, Is.EqualTo("2*3=6"));
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

		public static class ExternalDataFormatter
		{
			public class ExternalDataService
			{
				public string Text { get; set; } = "B6B747D4-02E4-4CBE-8CD2-013B64C1399A";
			}

			[MorestachioFormatter("Formatter", "")]
			public static string Formatter(object source, [ExternalData] ExternalDataService value2)
			{
				return source + "|" + value2.Text;
			}
		}

		[Test]
		public async Task TestCanAcceptExternalService()
		{
			//var options = new ParserOptions(, null, DefaultEncoding);
			//options.Formatters.AddFromType(typeof(ExternalDataFormatter));
			//options.Formatters.AddService(new ExternalDataFormatter.ExternalDataService());

			//var template = Parser.ParseWithOptions(options);
			//var result = template.CreateAndStringify();

			var template = "{{data.Formatter()}}";
			var data = new Dictionary<string, object>()
			{
				{"data", 123}
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddFromType(typeof(ExternalDataFormatter));
				options.Formatters.AddService(new ExternalDataFormatter.ExternalDataService());
			});

			Assert.That(result, Is.EqualTo("123|B6B747D4-02E4-4CBE-8CD2-013B64C1399A"));
		}

		[Test]
		public async Task TestCanAcceptExternalServiceFactory()
		{
			var template = "{{data.Formatter()}}";
			var data = new Dictionary<string, object>()
			{
				{"data", 123}
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddFromType(typeof(ExternalDataFormatter));
				options.Formatters.AddService(() => new ExternalDataFormatter.ExternalDataService());
			});

			Assert.That(result, Is.EqualTo("123|B6B747D4-02E4-4CBE-8CD2-013B64C1399A"));
		}

		[Test]
		public async Task TestCanFormatObject()
		{
			var template = "{{.(data)}}";
			var data = new Dictionary<string, object>()
			{
				{"data", 123}
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddSingle(new Func<object, object, int>((e, f) => (int)f));
			});

			Assert.That(result, Is.EqualTo("123"));
		}

		//[Test]
		//public void TestCanFormatStringFromTemplate()
		//{
		//	var options = new ParserOptions("{{'TEST'.Length}}", null, DefaultEncoding);
		//	var template = Parser.ParseWithOptions(options);

		//	var result = template.CreateAndStringify(new Dictionary<string, object>()
		//	{

		//	});
		//	Assert.That(result, Is.EqualTo("TEST".Length));
		//}

		[Test]
		public async Task TestCanFormatObjectSubWithFormatter()
		{
			var template = "{{Value.Format(SubValue)}}";

			var data = new
			{
				Value = 123,
				SubValue = 2
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddSingle((int left, int right) =>
				{
					return left * right;
				}, "Format");

				options.Formatters.AddSingle((int left, string arg) =>
				{
					return left;
				}, "Self");
			});
			Assert.That(result, Is.EqualTo("246"));
		}

		[Test]
		public async Task TestCanFormatObjectSubWithFormatterAndConst()
		{
			var template = "{{v.add(sv.r('tt'))}}";

			var data = new
			{
				v = 123,
				sv = 2
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddSingle((int left, int right) =>
				{
					return left * right;
				}, "add");

				options.Formatters.AddSingle((int left, string arg) =>
				{
					return left;
				}, "r");
			});

			Assert.That(result, Is.EqualTo("246"));
		}

		[Test]
		public async Task TestCanTransformValue()
		{
			var template = "{{data.ReturnValue()}}";

			var data = new Dictionary<string, object>()
			{
				{ "data", new CustomConverterFormatter.TestObject(){No = 123} }
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddFromType(typeof(CustomConverterFormatter));
				options.Formatters.ValueConverter.Add(new CustomConverterFormatter.TestToExpectedObjectConverter());
			});
			Assert.That(result, Is.EqualTo("123"));
		}

		[Test]
		public async Task TestCanTransformValueWithAttribute()
		{
			var template = "{{data.ReturnValueExplicitConverter()}}";

			var data = new Dictionary<string, object>()
			{
				{ "data", new CustomConverterFormatter.TestObject(){No = 123} }
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddFromType(typeof(CustomConverterFormatter));
			});

			Assert.That(result, Is.EqualTo("123"));
		}

		[Test]
		public async Task TestStringConversion()
		{
			var template = "{{data.ExpectInt()}}";

			var data = new Dictionary<string, object>()
			{
				{ "data", 123 }
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddFromType(typeof(StringFormatter));
			});

			Assert.That(result, Is.EqualTo(123.ToString("X2")));
		}

		[Test]
		public async Task FormatterCanFormatObjectTwice()
		{
			var template = "{{Plus(B, B)}}";

			var data = new Dictionary<string, object>()
			{
				{ "A", 5 },
				{ "B", 6 }
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddFromType(typeof(StringFormatter));
			});

			Assert.That(result, Is.EqualTo("12"));
		}


		[Test]
		public async Task TestSingleNamed()
		{
			var template = "{{data.reverse()}}";

			var data = new Dictionary<string, object>() { { "data", "Test" } };
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddFromType(typeof(StringFormatter));
			});
			Assert.That(result, Is.EqualTo("tseT"));
		}


		[Test]
		public async Task TestCanFormatSourceObjectLessFormatter()
		{
			var template = "{{DateTimeNow().ToString('D')}}";

			var data = new Dictionary<string, object>();
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts);
			Assert.That(result, Is.EqualTo(DateTime.Now.ToString("D")));
		}


		[Test]
		public async Task TestCanFormatSourceObjectLessFormatterAsArgumentAsync()
		{
			var template = "{{TimeSpanFromDays(DateTimeNow().Day).ToString('g')}}";

			var data = new Dictionary<string, object>();
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts);

			Assert.That(result, Is.EqualTo(TimeSpan.FromDays(DateTime.Now.Day).ToString("g")));
		}


		[Test]
		public async Task TestRest()
		{
			var template = "{{data.rest('other', 'and', 'more')}}";
			var data = new Dictionary<string, object>() { { "data", "Test" } };
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddFromType(typeof(StringFormatter));
			});

			Assert.That(result, Is.EqualTo("ORIGINAL: Test REST:otherandmore"));
		}

		[Test]
		public async Task TestAsync()
		{
			var template = "{{data.reverseAsync()}}";
			var data = new Dictionary<string, object>() { { "data", "Test" } };
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddFromType(typeof(StringFormatter));
			});
			Assert.That(result, Is.EqualTo("tseT"));
		}

		[Test]
		public async Task TestOptionalArgument()
		{
			var template = "{{data.optional()}}";
			var data = new Dictionary<string, object>() { { "data", "Test" } };
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddFromType(typeof(StringFormatter));
			});
			Assert.That(result, Is.EqualTo("OPTIONAL Test"));
		}

		[Test]
		public async Task TestDefaultArgument()
		{
			var template = "{{data.defaultValue()}}";
			var data = new Dictionary<string, object>() { { "data", "Test" } };
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddFromType(typeof(StringFormatter));
			});
			Assert.That(result, Is.EqualTo("DEFAULT Test"));
		}

		[Test]
		public async Task TestNamed()
		{
			var template = "{{data.ReverseArg('TEST')}}";
			var data = new Dictionary<string, object>() { { "data", "Test" } };
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddFromType(typeof(StringFormatter));
			});
			Assert.That(result, Is.EqualTo("TEST"));
		}

		[Test]
		public async Task GenericsTest()
		{
			var template = "{{data.fod()}}";
			var data = new Dictionary<string, object>() { { "data", new string[] { "TEST", "test" } } };
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddFromType(typeof(StringFormatter));
			});
			Assert.That(result, Is.EqualTo("TEST"));
		}


		[Test]
		public async Task ParserCanChainFormat()
		{
			var template = "{{#SCOPE data}}{{.('d').fnc()}}{{/SCOPE}}";
			var data = new Dictionary<string, object>() { { "data", DateTime.UtcNow } };
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddSingle(new Func<string, string>(s => "TEST"), "fnc");
			});
			Assert.That(result, Is.EqualTo("TEST"));
		}

		[Test]
		public async Task ParserCanChainFormatSubExpression()
		{
			var template = "{{#SCOPE data}}{{.('V').('V', r.d)}}{{/SCOPE}}";
			var referenceDataValue = "reference data value";
			var data = new Dictionary<string, object>
			{
				{"data", new
				{
					field = "field value",
					r = new
					{
						d = referenceDataValue
					}
				}},
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddSingle(
					new Func<object, object, object, object>((source, tempValue, reference) => reference));

				options.Formatters.AddSingle(
					new Func<object, object, object>((source, tempValue) => source));
			});
			Assert.That(result, Is.EqualTo(referenceDataValue));
		}

		[Test]
		public async Task ParserCanChainFormatSubExpressionFromEach()
		{
			var template = "{{#each d.r}}{{d.Format('t').Format('t', f)}}{{/each}}";
			var expectedValue = "formatter data value";
			var data = new Dictionary<string, object>
			{
				{"d", new Dictionary<string, object>()
				{
					{"field", "field value"},
					{
						"r", new List<Dictionary<string, object>>()
						{
							new Dictionary<string, object>
							{
								{"d", "display data value"},
								{"f", expectedValue}
							}
						}
					}
				}},
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddSingle(
					new Func<object, object, object, object>((source, tempValue, reference) => reference), "Format");

				options.Formatters.AddSingle(
					new Func<object, object, object>((source, tempValue) => source), "Format");
			});
			Assert.That(expectedValue, Is.EqualTo(result));
		}

		[Test]
		public async Task ParserCanChainFormatWithLineBreak()
		{
			var template = @"{{#SCOPE data}}{{
	.  (   'd'  )
																.()
}}{{/SCOPE}}";
			var data = new Dictionary<string, object>() { { "data", DateTime.UtcNow } };
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts | ParserOptionTypes.NoRerenderingTest, options =>
			{
				options.Formatters.AddSingle(new Func<string, string>(s => "TEST"));
			});

			Assert.That(result, Is.EqualTo("TEST"));
		}

		[Test]
		public async Task ParserCanTransferChains()
		{
			var template = @"{{#SCOPE data}}{{.('(d(a))')}}{{/SCOPE}}";
			var data = new Dictionary<string, object>() { { "data", "d" } };
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddSingle(new Func<string, string, string>((s, s1) => s1));
			});

			Assert.That(result, Is.EqualTo("(d(a))"));
		}

		[Test]
		public async Task ParserCanFormatMultipleUnnamedWithoutResult()
		{
			var formatterResult = "";
			var template = "{{#SCOPE data}}{{.('test', 'arg', 'arg, arg', ' spaced ', ' spaced with quote \\' ' , . )}}{{/SCOPE}}";
			var data = new Dictionary<string, object>() { { "data", 123123123 } };
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts| ParserOptionTypes.NoRerenderingTest, options =>
			{
				options.DisableContentEscaping = true;
				options.Formatters.AddSingle(new Action<int, string[]>(
					(self, test) => { Assert.Fail("Should not be invoked"); }));

				options.Formatters.AddSingle(new Action<int, string, string, string, string, string, int>(
					(self, test, arg, argarg, spacedArg, spacedWithQuote, refSelf) =>
					{
						formatterResult = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}", self, test, arg, argarg, spacedArg,
							spacedWithQuote,
							refSelf);
					}));

			});

			Assert.That(result, Is.Empty);
			Assert.That(formatterResult,
				Is.EqualTo(@"123123123|test|arg|arg, arg| spaced | spaced with quote ' |123123123"));
		}

		[Test]
		public async Task ParserCanFormatMultipleUnnamed()
		{
			var template = "{{#SCOPE data}}{{.('test', 'arg', 'arg, arg', ' spaced ', ' spaced with quote \\' ' , .)}}{{/SCOPE}}";
			var data = new Dictionary<string, object>() { { "data", 123123123 } };
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts | ParserOptionTypes.NoRerenderingTest, options =>
			{
				options.DisableContentEscaping = true;
				options.Formatters.AddSingle(
					new Func<int, string, string, string, string, string, int, string>(
						(self, test, arg, argarg, spacedArg, spacedWithQuote, refSelf) =>
						{
							return string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}", self, test, arg, argarg, spacedArg,
								spacedWithQuote,
								refSelf);
						}));
			});
			Assert.That(result, Is.EqualTo("123123123|test|arg|arg, arg| spaced | spaced with quote ' |123123123"));
		}

		[Test]
		public async Task ParserCanFormatMultipleUnnamedParams()
		{
			var template = "{{#SCOPE data}}{{.( 'arg', 'arg, arg', ' spaced ', [testArgument]'test', ' spaced with quote \\' ' , .)}}{{/SCOPE}}";
			var data = new Dictionary<string, object>() { { "data", 123123123 } };
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts| ParserOptionTypes.NoRerenderingTest, options =>
			{
				options.DisableContentEscaping = true;
				options.Formatters.AddSingle(
					new Func<int, string, object[], string>(UnnamedParamsFormatter));
			});
			Assert.That(result, Is.EqualTo("123123123|test|arg|arg, arg| spaced | spaced with quote ' |123123123"));
		}

		private string UnnamedParamsFormatter(int self, string testArgument, [RestParameter] params object[] other)
		{
			return string.Format("{0}|{1}|{2}", self, testArgument, other.Aggregate((e, f) => e + "|" + f));
		}

		[Test]
		public async Task ParserCanFormatMultipleNamed()
		{
			var template = "{{#SCOPE data}}{{Formatter([refSelf] ., 'arg',[Fob]'test', [twoArgs]'arg, arg', [anySpaceArg]' spaced ')}}{{/SCOPE}}";
			var data = new Dictionary<string, object>() { { "data", 123123123 } };
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts | ParserOptionTypes.NoRerenderingTest, options =>
			{
				options.Formatters.AddSingle(
					new Func<int, string, string, string, string, int, string>(NamedFormatter), "Formatter");
			});
			Assert.That(result, Is.EqualTo("123123123|test|arg|arg, arg| spaced |123123123"));
		}

		private string NamedFormatter(int self, [FormatterArgumentName("Fob")] string testArgument, string arg,
			string twoArgs, string anySpaceArg, int refSelf)
		{
			return string.Format("{0}|{1}|{2}|{3}|{4}|{5}", self, testArgument, arg, twoArgs, anySpaceArg, refSelf);
		}

		[Test]
		public async Task ParserCanCheckCanFormat()
		{
			var template = "{{#SCOPE data}}{{.('(d(a))')}}{{/SCOPE}}";
			var data = new Dictionary<string, object>() { { "data", "d" } };
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddSingle(
					new Func<string, string, string, string>((s, inv, inva) => throw new Exception("A")));

				options.Formatters.AddSingle(new Func<string, string>(s =>
					throw new Exception("Wrong Ordering")));
				options.Formatters.AddSingle(new Action<string>(s =>
					throw new Exception("Wrong Return Ordering")));
				options.Formatters.AddSingle(new Func<string, string, string>((s, inv) => inv));
			});

			Assert.That(result, Is.EqualTo("(d(a))"));
		}

		[Test]
		public async Task ParserCanChainWithAndWithoutFormat()
		{
			var dataValue = DateTime.UtcNow;
			var template = "{{data.().TimeOfDay.Ticks.().()}}";
			var data = new Dictionary<string, object>() { { "data", dataValue } };
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddSingle<string, string>(s => s);
				options.Formatters.AddSingle<DateTime, DateTime>(s => s);
				options.Formatters.AddSingle<long, TimeSpan>(s => new TimeSpan(s));
			});
			Assert.That(result, Is.EqualTo(new TimeSpan(dataValue.TimeOfDay.Ticks).ToString()));
		}

		[Test]
		public async Task ParserCanFormatAndCombine()
		{
			//this should compile as its valid but not work as the Default
			//settings for DateTime are ToString(Arg) so it should return a string and not an object

			var dataValue = DateTime.UtcNow;
			var template = "{{data.ToString('d').Year}},{{data}}";
			var data = new Dictionary<string, object>() { { "data", dataValue } };
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts);
			Assert.That(result,
				Is.EqualTo(string.Empty + "," + dataValue));
		}

		[Test]
		public async Task ParserCanFormatArgumentWithExpression()
		{
			var format = "yyyy.mm";
			var dataValue = DateTime.UtcNow;
			var template = "{{data.ToString(testFormat)}}";
			var data = new Dictionary<string, object>
			{
				{"data", dataValue},
				{"testFormat", format}
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts);
			Assert.That(result, Is.EqualTo(dataValue.ToString(format)));
		}

		[Test]
		public async Task ParserCanFormatArgumentWithNestedExpression()
		{
			var dataValue = DateTime.Now;
			var format = new Dictionary<string, object>
			{
				{"inner", "yyyy.mm"}
			};
			var template = "{{data.ToString(testFormat.inner)}}";
			var data = new Dictionary<string, object>
			{
				{"data", dataValue},
				{"testFormat", format}
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts);
			Assert.That(result, Is.EqualTo(dataValue.ToString(format["inner"].ToString())));
		}

		[Test]
		public async Task ParserCanFormatArgumentWithSubExpression()
		{
			var formatterCalled = false;
			var formatter2Called = false;

			var dataValue = DateTime.Now;
			var format = "yyyy.mm";
			var template = "{{d.Format(t.Format('d'), 'tt')}}";
			var data = new Dictionary<string, object>
			{
				{"d", dataValue},
				{"t", 19191919}
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts,
				options =>
				{
					options.Formatters.AddSingle<int, string, string>((sourceValue, testString) =>
					{
						Assert.That(testString, Is.EqualTo("d"));
						formatterCalled = true;
						return format;
					}, "Format");
					options.Formatters.AddSingle(new Func<DateTime, string, string, string>(
						(sourceValue, testString2, shouldBed) =>
						{
							Assert.That(shouldBed, Is.EqualTo("tt"));
							Assert.That(testString2, Is.EqualTo(format));
							formatter2Called = true;
							return sourceValue.ToString(testString2);
						}), "Format");
				});

			Assert.That(formatterCalled, Is.True, "The  formatter was not called");
			Assert.That(formatter2Called, Is.True, "The Date formatter was not called");
			Assert.That(result, Is.EqualTo(dataValue.ToString(format)));
		}

		[Test]
		public async Task ParserCanFormatArgumentWithSubExpressionMultiple()
		{
			var formatterCalled = false;
			var formatter2Called = false;
			var formatter3Called = false;

			var format = "yyyy.mm";
			var dataValue = DateTime.UtcNow;
			var template = "{{d.(f.Format('d'), \"t\").('pl', by.(by, 'f'))}}";
			var data = new Dictionary<string, object>
			{
				{"d", dataValue},
				{"f", 19191919},
				{"by", 10L}
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts,
				options =>
				{
					options.Formatters.AddSingle<int, string, string>((sourceValue, testString) =>
					{
						Assert.That(testString, Is.EqualTo("d"));
						formatterCalled = true;
						return format;
					}, "Format");
					options.Formatters.AddSingle(new Func<long, long, string, int>(
						(sourceValue, testString, f) =>
						{
							Assert.That(testString, Is.EqualTo(sourceValue));
							Assert.That(f, Is.EqualTo("f"));
							formatterCalled = true;
							return (int)sourceValue;
						}));
					options.Formatters.AddSingle(new Func<DateTime, string, string, string>(
						(sourceValue, testString2, shouldBed) =>
						{
							Assert.That(shouldBed, Is.EqualTo("t"));
							Assert.That(testString2, Is.EqualTo(format));
							formatter2Called = true;
							return sourceValue.ToString(testString2);
						}));
					options.Formatters.AddSingle(new Func<string, string, int, string>(
						(sourceValue, name, number) =>
						{
							Assert.That(sourceValue, Is.EqualTo(dataValue.ToString(format)));
							Assert.That(name, Is.EqualTo("pl"));
							Assert.That(number, Is.EqualTo(data["by"]));

							formatter3Called = true;
							return sourceValue.PadLeft(number);
						}));
				});

			Assert.That(formatterCalled, Is.True, "The formatter was not called");
			Assert.That(formatter2Called, Is.True, "The Date formatter was not called");
			Assert.That(formatter3Called, Is.True, "The Pad formatter was not called");
			Assert.That(result, Is.EqualTo(dataValue.ToString(format).PadLeft(int.Parse(data["by"].ToString()))));
		}

		[Test]
		public async Task TemplateIfDoesNotScopeWithFormatter()
		{
			var template =
				@"{{#IF data.()}}{{.}}{{/IF}}";
			var data = new Dictionary<string, object>()
			{
				{"data", "test" },
				{"root", "tset" }
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddSingle(new Func<string, bool>(f => f == "test"));
			});
			Assert.That(data.ToString(), Is.EqualTo(result));
		}


		[Test]
		public async Task TemplateIfDoesNotScopeToRootWithFormatter()
		{
			var template =
				@"{{#SCOPE data}}{{#IF data2.()}}{{data3.dataSet}}{{/IF}}{{/SCOPE}}";
			var data = new Dictionary<string, object>()
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
								{"dataSet", "TEST"}
							}
						}
					}
				},
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddSingle(new Func<string, bool>(f => f == "test"));
			});

			Assert.AreEqual("TEST", result);
		}

		[Test]
		public async Task TemplateIfDoesNotScopeToRootWithFormatterCustomized()
		{
			var template =
				@"{{#SCOPE data}}{{#EACH data3.dataList.()}}{{#IF .()}}{{.}}{{/IF}}{{/EACH}}{{/SCOPE}}";
			var data = new Dictionary<string, object>()
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
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddSingle(new Func<IEnumerable<string>, IEnumerable<string>>(f => f));
				options.Formatters.AddSingle(new Func<string, bool>(f => f == "TE"));
			});

			Assert.AreEqual("TE", result);
		}

		[Test]
		public async Task TemplateInvertedIfDoesNotScopeWithFormatter()
		{
			var template =
				@"{{^IF data.()}}{{.}}{{/IF}}";
			var data = new Dictionary<string, object>()
			{
				{"data", "test" },
				{"root", "tset" }
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddSingle(new Func<string, bool>(f => f != "test"));
			});


			Assert.AreEqual(data.ToString(), result);
		}

		[Test]
		public async Task FormatterCanHandleNullArgument()
		{
			var template =
				@"{{data.TEST(root, null, any.Any)}}";
			var data = new Dictionary<string, object>()
			{
				{"data", "test" },
				{"root", "tset" }
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AllParametersAllDefaultValue = true;
				options.Formatters.AddSingle(
					new Func<string, string, string, string, string>((source, rootArg, argNullConst, argNullValue) =>
					{
						Assert.That(argNullConst, Is.Null);
						Assert.That(argNullValue, Is.Null);
						return rootArg.PadLeft(123);
					}), "TEST");
			});

			Assert.AreEqual("tset".PadLeft(123), result);
		}

		[Test]
		public async Task FormatterCanHandleEnumInputAsString()
		{
			var template =
				@"{{data.TEST('Friday')}}";
			var data = new Dictionary<string, object>()
			{
				{"data", "Day: " },
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddSingle(
					new Func<string, DayOfWeek, string>((pre, source) =>
					{
						return pre + source.ToString();
					}), "TEST");
			});

			Assert.That(result, Is.EqualTo("Day: Friday"));
		}

		[Test]
		public async Task FormatterCanHandleNumberOperator()
		{
			var template =
				@"{{ToString(1 + 3)}}";

			var data = new Dictionary<string, object>()
			{
				{"dataA", 1 },
				{"dataB", 3 }
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddSingleGlobal(new Func<int, string>(i => i.ToString("x8")), "ToString");
			});

			Assert.That(result, Is.EqualTo((1 + 3).ToString("X8")));
		}

		[Test]
		public async Task FormatterCanHandleNumberOperatorMultipleArgument()
		{
			var template =
				@"{{ToString(1 + 3, 'X8')}}";

			var data = new Dictionary<string, object>()
			{
				{"dataA", 1 },
				{"dataB", 3 }
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddSingleGlobal(new Func<int, string, string>((nr, txt) => nr.ToString(txt)), "ToString");
			});


			Assert.That(result, Is.EqualTo((1 + 3).ToString("X8")));
		}

		[Test]
		public async Task FormatterCanHandleNumberOperatorAsArgument()
		{
			var template =
				@"{{ToString(3 + AsInt(1), 'X8')}}{{ToString(AsInt(1) + 3, 'X4')}}";

			var data = new Dictionary<string, object>()
			{
				{"dataA", 1 },
				{"dataB", 3 }
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddSingleGlobal(new Func<int, string, string>((nr, txt) => nr.ToString(txt)), "ToString");
				options.Formatters.AddSingleGlobal(new Func<int, int>((nr) => nr), "AsInt");
			});

			Assert.That(result, Is.EqualTo((1 + 3).ToString("X8") + (1 + 3).ToString("X4")));
		}

		[Test]
		public async Task FormatterCanHandleStringOperator()
		{
			var template =
				@"{{ToString('*' + 'test')}}";

			var data = new Dictionary<string, object>()
			{
				{"dataA", 1 },
				{"dataB", 3 }
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddSingleGlobal(new Func<string, string>(i => i), "ToString");
			});
			Assert.That(result, Is.EqualTo("*test"));
		}

		[Test]
		public async Task FormatterCanHandleStringOperatorWithTextAtLast()
		{
			var template =
				@"{{dataA == 'test'}}";

			var data = new Dictionary<string, object>()
			{
				{"dataA", "test" },
				{"dataB", 3 }
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddSingleGlobal(new Func<string, string>(i => i), "ToString");
			});
			Assert.That(result, Is.EqualTo("True"));
		}

		[Test]
		public async Task FormatterCanHandleStringOperatorMultipleArgument()
		{
			var template =
				@"{{Pad('te' + 'st', 'X8')}}";
			var data = new Dictionary<string, object>()
			{
				{"dataA", 1 },
				{"dataB", 3 }
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddSingleGlobal(new Func<string, string, string>((nr, txt) => nr + txt), "Pad");
			});

			Assert.That(result, Is.EqualTo("testX8"));
		}

		[Test]
		//[Ignore("This behavior is currently desired. Implicit null calls are expected to fail")]
		public async Task FormatterCanHandleStringOperatorAsArgument()
		{
			var template =
				@"{{ToString('Te' + AsString('1'), 'X8')}} {{ToString(AsString('8') + 'st', 'X4')}}";

			var data = new Dictionary<string, object>()
			{
				{"dataA", 1 },
				{"dataB", 3 }
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddSingleGlobal(new Func<string, string, string>((nr, txt) => nr + txt), "ToString");
				options.Formatters.AddSingleGlobal(new Func<string, string>((nr) => nr), "AsString");
			});

			Assert.That(result, Is.EqualTo("Te1X8 8stX4"));
		}

		[Test]
		public async Task FormatterCanHandleStringOperatorCarryOver()
		{
			var template =
				@"{{ToString('*' + 'test' + '*')}}";
			var data = new Dictionary<string, object>()
			{
				{"dataA", 1 },
				{"dataB", 3 }
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddSingleGlobal(new Func<string, string>(i => i), "ToString");
			});
			Assert.That(result, Is.EqualTo("*test*"));
		}

		[Test]
		public async Task FormatterCanHandleDataOperatorCarryOver()
		{
			var template =
				@"{{ToString(dataA + dataB + dataC)}}";
			var data = new Dictionary<string, object>()
			{
				{"dataA", "*" },
				{"dataB", "test" },
				{"dataC", "!*" },
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddSingleGlobal(new Func<string, string>(i => i), "ToString");
			});

			Assert.That(result, Is.EqualTo("*test!*"));
		}

		[Test]
		public async Task FormatterCanHandleDataDevideOperatorCarryOver()
		{
			var template =
				@"{{ToString(dataA / dataB + dataC)}}";
			var data = new Dictionary<string, object>()
			{
				{"dataA", 50 },
				{"dataB", 60 },
				{"dataC", 10 },
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddSingleGlobal(new Func<string, string>(i => i), "ToString");
			});

			Assert.That(result, Is.EqualTo((50 / 60 + 10).ToString()));
		}

		[Test]
		public async Task FormatterCanHandleDataDevideOperatorCarryOverFromOtherMethod()
		{
			var template =
				@"{{ToString(dataA.Add(2) / dataB + dataC)}}";
			var data = new Dictionary<string, object>()
			{
				{"dataA", 50 },
				{"dataB", 60 },
				{"dataC", 10 },
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddSingleGlobal(new Func<string, string>(i => i), "ToString");
			});

			Assert.That(result, Is.EqualTo(((50 + 2) / 60 + 10).ToString()));
		}

		[Test]
		public async Task FormatterCanHandleDataDevideOperatorCarryOverFromOtherRightHandOperatorMethod()
		{
			var template =
				@"{{ToString(dataB / dataA.Add(2) + dataC)}}";
			var data = new Dictionary<string, object>()
			{
				{"dataA", 50 },
				{"dataB", 60 },
				{"dataC", 10 },
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts, options =>
			{
				options.Formatters.AddSingleGlobal(new Func<string, string>(i => i), "ToString");
			});
			Assert.That(result, Is.EqualTo((60 / (50 + 2) + 10).ToString()));
		}
	}
}
