using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Morestachio.Formatter.Framework;
using Morestachio.Linq;
using NUnit.Framework;

namespace Morestachio.Tests
{
	[TestFixture]
	public class LinqTests
	{
		private async Task<TE> CreateAndExecute<TE, T>(string template, T data)
			where T : class, IEnumerable
			where TE : class, IEnumerable
		{
			var parserOptions = new ParserOptions(template, null, ParserFixture.DefaultEncoding);
			parserOptions.Formatters.AddFromType(typeof(DynamicLinq));
			var document = (await Parser.ParseWithOptionsAsync(parserOptions));
			document.CaptureVariables = true;
			var andStringifyAsync = await document.CreateAsync(new Dictionary<string, object>()
			{
				{ "data", data}
			});
			return andStringifyAsync.CapturedVariables["result"] as TE;
		}

		[Test]
		public async Task TestWhere()
		{
			var sl = new List<string>()
			{
				"ABC",
				"ACB",
				"CBA"
			};
			var simple = await CreateAndExecute<IEnumerable<string>, IEnumerable<string>>("{{#var result = data.Where('it.StartsWith(\"A\")')}}", sl);
			Assert.That(simple, Contains.Item("ABC"));
			Assert.That(simple, Contains.Item("ACB"));
			Assert.That(simple, Is.Not.Contain("CBA"));

			var withArg = await CreateAndExecute<IEnumerable<string>, IEnumerable<string>>("{{#var result = data.Where('it.StartsWith(@0)', 'A')}}", sl);
			Assert.That(withArg, Contains.Item("ABC"));
			Assert.That(withArg, Contains.Item("ACB"));
			Assert.That(withArg, Is.Not.Contain("CBA"));
		}

		[Test]
		public async Task TestSelect()
		{
			var sl = new List<string>()
			{
				"ABC",
				"ACB",
				"CBA"
			};
			var simple = await CreateAndExecute<IEnumerable<string>, IEnumerable<string>>("{{#var result = data.Select('it + \"Bla\"')}}", sl);
			Assert.That(simple, Contains.Item("ABCBla"));
			Assert.That(simple, Contains.Item("ACBBla"));
			Assert.That(simple, Contains.Item("CBABla"));

			var withArg = await CreateAndExecute<IEnumerable<string>, IEnumerable<string>>("{{#var result = data.Where('it + @0', 'Bla')}}", sl);
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
