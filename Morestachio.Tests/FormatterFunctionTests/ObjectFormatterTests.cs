using System.Collections.Generic;
using System.Threading.Tasks;
using Morestachio.Formatter.Predefined;
using Morestachio.Framework.Context.Resolver;
using NUnit.Framework;

namespace Morestachio.Tests.FormatterFunctionTests
{
	[TestFixture]
	public class ObjectFormatterTests : FormatterBaseTest
	{
		public class XmlTestData
		{
			public GenderData[] GenderDatas { get; set; }

			public class GenderData
			{
				public string Type { get; set; }
				public string Id { get; set; }
			}
		}

		[Test]
		public async Task TestToXml()
		{
			var data = new XmlTestData
			{
				GenderDatas = new[]
				{
					new XmlTestData.GenderData { Id = "MALE", Type = "male" },
					new XmlTestData.GenderData { Id = "FEMALE", Type = "female" }
				}
			};
			var result = await CallFormatter<string>("this.ToXml()", data);
			Assert.That(result, Is.Not.Null);
			var expected = ObjectFormatter.ToXml(data, new ParserOptions { Encoding = ParserFixture.DefaultEncoding });
			Assert.That(result, Is.EqualTo(expected));
		}

		[Test]
		public async Task TestToObject()
		{
			var data = new Dictionary<string, object>
			{
				{
					"Genders", new
					{
						Male = "MALE",
						Female = "FEMALE"
					}
				}
			};
			var result = await CallFormatter<object>("this.AsObject().Genders.Male", data);
			Assert.That(result, Is.Not.Null);
			Assert.That("MALE", Is.EqualTo(result));
		}

		[Test]
		public async Task TestNew()
		{
			var data = new
			{
				Genders = new
				{
					Male = "MALE",
					Female = "FEMALE"
				}
			};
			var result = await CallFormatter<IDictionary<string, object>>(
				"new([Name]\"Test\", [Age] 57, [Gender] Genders.Male)", data);
			Assert.That(result, Is.Not.Null);
			Assert.That(result["Name"], Is.EqualTo("Test"));
			Assert.That(result["Age"], Is.EqualTo(57));
			Assert.That(result["Gender"], Is.EqualTo(data.Genders.Male));
		}

		[Test]
		public async Task TestDynamicCall()
		{
			var data = new XmlTestData
			{
				GenderDatas = new[]
				{
					new XmlTestData.GenderData { Id = "MALE", Type = "male" },
					new XmlTestData.GenderData { Id = "FEMALE", Type = "female" }
				}
			};
			var result = await CallFormatter<string>("this.Call('ToXml')", data);
			Assert.That(result, Is.Not.Null);
			var expected = ObjectFormatter.ToXml(data, new ParserOptions { Encoding = ParserFixture.DefaultEncoding });
			Assert.That(result, Is.EqualTo(expected));
		}

		[Test]
		public async Task TestDynamicGet()
		{
			var data = new XmlTestData
			{
				GenderDatas = new[]
				{
					new XmlTestData.GenderData { Id = "MALE", Type = "male" },
					new XmlTestData.GenderData { Id = "FEMALE", Type = "female" }
				}
			};
			var result = await CallFormatter<string>("this.Get('GenderDatas').First().Get('Id')", data);
			Assert.That(result, Is.Not.Null);
			var expected = ObjectFormatter.ToXml(data, new ParserOptions { Encoding = ParserFixture.DefaultEncoding });
			Assert.That(result, Is.EqualTo("MALE"));
		}

		[Test]
		public async Task TestCombine()
		{
			var leftObject = new Dictionary<string, object>
			{
				{ "ValueA", "A" }
			};

			var rightObject = new Dictionary<string, object>
			{
				{ "ValueB", "B" }
			};

			var result = await CallFormatter<IDictionary<string, object>>("left.Combine(right)", new
			{
				left = leftObject,
				right = rightObject
			});
			Assert.That(result, Is.Not.Null);

			var expected = ObjectFormatter.Combine(leftObject, rightObject);

			foreach (var item in result)
			{
				Assert.That(item.Value, Is.EqualTo(expected[item.Key]));
			}
		}
	}

	public class FormatterBaseTest
	{
		public async Task CallFormatter(string expression, object source)
		{
			await CallFormatter<object>(expression, source);
		}

		public async Task<T> CallFormatter<T>(string expression, object source)
		{
			object result = null;
			var template = @"{{#VAR result = " + expression + "}}";

			await ParserFixture.CreateAndParseWithOptions(template, source,
				ParserOptionTypes.NoRerenderingTest | ParserOptionTypes.UseOnDemandCompile,
				e => { return e.WithValueResolver(new FieldValueResolver()); },
				e => { e.CaptureVariables = true; },
				e =>
				{
					result = e.CapturedVariables["result"];
					Assert.That(result, Is.AssignableTo<T>());
				});

			return result is T ? (T)result : default;
		}
	}
}