using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Morestachio.Tests
{
	[TestFixture(ParserOptionTypes.UseOnDemandCompile)]
	[TestFixture(ParserOptionTypes.Precompile)]
	[Parallelizable(ParallelScope.All)]
	public class OperatorTests
	{
		private readonly ParserOptionTypes _opts;

		public OperatorTests(ParserOptionTypes opts)
		{
			_opts = opts;
		}

		[Test]
		public async Task TestNullCoalescingOperatorSingleValues()
		{
			var template = "{{A ?? B}}";
			var data = new Dictionary<string, object>()
			{
				{"A", "VALA"},
				{"B", "VALB"},
			};
			Assert.That(await ParserFixture.CreateAndParseWithOptions(template, data, _opts), Is.EqualTo(data["A"]));
			data = new Dictionary<string, object>()
			{
				{"A", null},
				{"B", "VALB"},
			};
			Assert.That(await ParserFixture.CreateAndParseWithOptions(template, data, _opts), Is.EqualTo(data["B"]));
		}
	}
}
