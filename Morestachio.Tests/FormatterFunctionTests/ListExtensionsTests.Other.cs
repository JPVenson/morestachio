using System.Collections.Generic;
using System.Threading.Tasks;
using Morestachio.Framework.Context.Resolver;
using NUnit.Framework;

namespace Morestachio.Tests.FormatterFunctionTests
{
	[TestFixture]
	public partial class ListExtensionsTests
	{
		[Test]
		public async Task TestWith()
		{
			var callFormatter = await CallFormatter<IEnumerable<string>>("With('A', 'B')", null);
			Assert.That(callFormatter, Is.Not.Null);
			Assert.That(callFormatter, Has.Count.EqualTo(2));
		}

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