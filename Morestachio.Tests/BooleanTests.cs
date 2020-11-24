using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Morestachio.Tests
{
	[Flags]
	public enum ParserOptionTypes
	{
		UseOnDemandCompile = 1 << 0,
		Precompile = 1 << 1,
		NoRerenderingTest = 1 << 2
	}

	[TestFixture(ParserOptionTypes.UseOnDemandCompile)]
	[TestFixture(ParserOptionTypes.Precompile)]
	[Parallelizable(ParallelScope.All)]
	public class BooleanTests
	{
		private readonly ParserOptionTypes _opts;

		public BooleanTests(ParserOptionTypes opts)
		{
			_opts = opts;
		}
		
		[Test]	
		[TestCase(true, true, "&&", true && true)]
		[TestCase(true, true, "||", true || true)]
		[TestCase(true, false, "||", true || false)]
		[TestCase(true, false, "&&", true && false)]
		public async Task CanUseBooleanOperationsOnData(object realData, object templateData, string operation, object expected)
		{
			var template = $"{{{{data {operation} templateData}}}}";
			var data = new
			{
				data = realData,
				templateData = templateData
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _opts);
			//var parsingOptions = new ParserOptions(template, null, ParserFixture.DefaultEncoding);
			//var result = await Parser.ParseWithOptions(parsingOptions).CreateAndStringifyAsync(data);
			Assert.That(result, Is.EqualTo(expected.ToString()));
		}
	}
}