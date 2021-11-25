using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Morestachio.Framework.Context.Resolver;
using NUnit.Framework;

namespace Morestachio.Tests.FormatterFunctionTests
{
	[TestFixture]
	internal class ObjectFormatterTests : FormatterBaseTest
	{
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
			var result = await CallFormatter<IDictionary<string, object>>("new([Name]\"Test\", [Age] 57, [Gender] Genders.Male)", data);
			Assert.IsNotNull(result);
			Assert.That(result["Name"], Is.EqualTo("Test"));
			Assert.That(result["Age"], Is.EqualTo(57));
			Assert.That(result["Gender"], Is.EqualTo(data.Genders.Male));
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
				ParserOptionTypes.NoRerenderingTest | ParserOptionTypes.UseOnDemandCompile, e =>
				{
					e.ValueResolver = new FieldValueResolver();
				},
				e =>
				{
					e.CaptureVariables = true;
				},
				e =>
				{
					result = e.CapturedVariables["result"];
					Assert.That(result, Is.AssignableTo<T>());
				});
			return result is T ? (T)result : default;
		}
	}
}
