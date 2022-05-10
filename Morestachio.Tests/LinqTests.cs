using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Morestachio.Linq;
using NUnit.Framework;

namespace Morestachio.Tests
{
	[TestFixture(ParserOptionTypes.UseOnDemandCompile)]
	[TestFixture(ParserOptionTypes.Precompile)]
	[Parallelizable(ParallelScope.All)]
	public class LinqTests
	{
		private readonly ParserOptionTypes _options;

		private async Task<TE> CreateAndExecute<TE, T>(string template, T data, ParserOptionTypes opt)
			where T : class, IEnumerable
			where TE : class, IEnumerable
		{
			IDictionary<string, object> variables = null;

			var result = await ParserFixture.CreateAndParseWithOptions(template, new Dictionary<string, object>
			{
				{ "data", data }
			}, opt, options => { return	options.WithFormatters(typeof(DynamicLinq)); }, info => { info.CaptureVariables = true; }, documentResult => { variables = documentResult.CapturedVariables; });

			return variables["result"] as TE;
		}

		public LinqTests(ParserOptionTypes options)
		{
			_options = options;
		}

		[Test]
		public async Task TestWhere()
		{
			var sl = new List<string>
			{
				"ABC",
				"ACB",
				"CBA"
			};
			var simple = await CreateAndExecute<IEnumerable<string>, IEnumerable<string>>("{{#var result = data.Where('it.StartsWith(\"A\")')}}", sl, _options);
			Assert.That(simple, Contains.Item("ABC"));
			Assert.That(simple, Contains.Item("ACB"));
			Assert.That(simple, Is.Not.Contain("CBA"));
			var withArg = await CreateAndExecute<IEnumerable<string>, IEnumerable<string>>("{{#var result = data.Where('it.StartsWith(@0)', 'A')}}", sl, _options);
			Assert.That(withArg, Contains.Item("ABC"));
			Assert.That(withArg, Contains.Item("ACB"));
			Assert.That(withArg, Is.Not.Contain("CBA"));
		}

		[Test]
		public async Task TestSelect()
		{
			var sl = new List<string>
			{
				"ABC",
				"ACB",
				"CBA"
			};
			var simple = await CreateAndExecute<IEnumerable<string>, IEnumerable<string>>("{{#var result = data.Select('it + \"Bla\"')}}", sl, _options);
			Assert.That(simple, Contains.Item("ABCBla"));
			Assert.That(simple, Contains.Item("ACBBla"));
			Assert.That(simple, Contains.Item("CBABla"));
			var withArg = await CreateAndExecute<IEnumerable<string>, IEnumerable<string>>("{{#var result = data.Select('it + @0', 'Bla')}}", sl, _options);
			Assert.That(withArg, Contains.Item("ABCBla"));
			Assert.That(withArg, Contains.Item("ACBBla"));
			Assert.That(withArg, Contains.Item("CBABla"));
		}

		//[Test]
		//public async Task TestSelectMany()
		//{
		//	var sl = new List<List<char>>()
		//	{
		//		new List<char>(){'A','B','C'},
		//		new List<char>(){'A','C','B'},
		//		new List<char>(){'C','B','A'},
		//	};
		//	var simple = await CreateAndExecute<IEnumerable<string>, IEnumerable<string>>("{{#var result = data.SelectMany('it')}}", sl);
		//	Assert.That(simple, Contains.Item("ABCBla"));
		//	Assert.That(simple, Contains.Item("ACBBla"));
		//	Assert.That(simple, Contains.Item("CBABla"));

		//	var withArg = await CreateAndExecute<IEnumerable<string>, IEnumerable<string>>("{{#var result = data.SelectMany('it')}}", sl);
		//	Assert.That(withArg, Contains.Item("ABCBla"));
		//	Assert.That(withArg, Contains.Item("ACBBla"));
		//	Assert.That(withArg, Contains.Item("CBABla"));
		//}
	}
}
