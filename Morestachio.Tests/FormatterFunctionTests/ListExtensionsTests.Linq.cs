using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Morestachio.Tests.FormatterFunctionTests
{
	public partial class ListExtensionsTests
	{
		[Test]
		public async Task TestWhere()
		{
			var list = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
			var callFormatter = await CallFormatter<IList<string>>("this.Where(e => e > 5 && e < 8)", list);
			Assert.That(callFormatter, Is.Not.Null);
			Assert.That(callFormatter, Has.Count.EqualTo(2));
			Assert.That(callFormatter, Contains.Item(6).And.Contain(7));
		}

		[Test]
		public async Task TestSelect()
		{
			var list = new List<(int, string)>() { (1, "A"), (2, "B"), (3, "C") };
			var callFormatter = await CallFormatter<IList<string>>("this.Select(e => e.Item2)", list);
			Assert.That(callFormatter, Is.Not.Null);
			Assert.That(callFormatter, Has.Count.EqualTo(2));
			Assert.That(callFormatter, Contains.Item("A").And.Contain("B"));
		}

		[Test]
		public async Task TestSelectMany()
		{
			var list = new List<(int, string)>() { (1, "AB"), (2, "CD") };
			var callFormatter = await CallFormatter<IList<char>>("this.SelectMany(e => e.Item2)", list);
			Assert.That(callFormatter, Is.Not.Null);
			Assert.That(callFormatter, Has.Count.EqualTo(4));
			Assert.That(callFormatter, Contains.Item("A").And.Contain("B").And.Contain("C").And.Contain("D"));
		}
	}
}
