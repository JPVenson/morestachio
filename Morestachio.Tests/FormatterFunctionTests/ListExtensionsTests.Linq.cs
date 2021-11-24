using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Morestachio.Tests.FormatterFunctionTests
{
	public partial class ListExtensionsTests
	{
		[Test]
        //[Ignore("")]
		public async Task TestWhere()
		{
			var list = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
			var callFormatter = await CallFormatter<IEnumerable<int>>("this.Where(e => (e > 5) && (e < 8))", list);
			Assert.That(callFormatter, Is.Not.Null);
			Assert.That(callFormatter, Has.Length.EqualTo(2));
			Assert.That(callFormatter, Contains.Item(6).And.Contain(7));
		}

		[Test]
		public async Task TestSelect()
		{
			var list = new List<(int, string)>() { (1, "A"), (2, "B"), (3, "C") };

			var callFormatter = await CallFormatter<IEnumerable<object>>("this.Select(e => e.Item2)", list);
			Assert.That(callFormatter, Is.Not.Null);
			Assert.That(callFormatter, Has.Length.EqualTo(3));
			Assert.That(callFormatter, Contains.Item("A").And.Contain("B"));
		}

		[Test]
		[Ignore("")]
		public async Task TestSelectMany()
		{
			var list = new List<(int, char[])>() { (1, new char[] { 'A', 'B' }), (2, new char[] { 'C', 'D' }) };
			var callFormatter = await CallFormatter<IEnumerable<char>>("this.SelectMany(e => e.Item2)", list);
			Assert.That(callFormatter, Is.Not.Null);
			Assert.That(callFormatter, Has.Length.EqualTo(4));
			Assert.That(callFormatter, Contains.Item("A").And.Contain("B").And.Contain("C").And.Contain("D"));
		}

		[Test]
		public async Task TestTakeWhile()
		{
			var list = new List<(int, string)>() { (1, "A"), (2, "B"), (3, "C") };
			var callFormatter = await CallFormatter<IEnumerable<(int, string)>>("this.TakeWhile(e => e.Item1 < 2)", list);
			Assert.That(callFormatter, Is.Not.Null);
			Assert.That(callFormatter, Has.Length.EqualTo(1));
			Assert.That(callFormatter, Contains.Item(list[0]));
		}

		[Test]
		public async Task TestSkipWhile()
		{
			var list = new List<(int, string)>() { (1, "A"), (2, "B"), (3, "C") };
			var callFormatter = await CallFormatter<IEnumerable<(int, string)>>("this.SkipWhile(e => e.Item1 < 2)", list);
			Assert.That(callFormatter, Is.Not.Null);
			Assert.That(callFormatter, Has.Length.EqualTo(2));
			Assert.That(callFormatter, Contains.Item(list[1]).And.Contain(list[2]));
		}

		[Test]
		public async Task TestOrderBy()
		{
			var list = new List<(int, string)>() { (3, "A"), (2, "B"), (1, "C") };
			var callFormatter = (await CallFormatter<IEnumerable<(int, string)>>("this.OrderBy(e => e.Item1).ToArray()", list)).ToArray();
			Assert.That(callFormatter, Is.Not.Null);
			Assert.That(callFormatter, Has.Length.EqualTo(list.Count));
			var items = list.OrderBy(e => e.Item1).ToArray();
			for (var index = 0; index < items.Length; index++)
			{
				var orderedItem = items[index];
				var item = callFormatter[index];
				Assert.That(orderedItem, Is.EqualTo(item));
			}
		}

		[Test]
		public async Task TestOrderByThenBy()
		{
			var list = new List<(int, string)>() { (3, "A"), (2, "B"), (2, "C") };
			var callFormatter = (await CallFormatter<IEnumerable<(int, string)>>("this.OrderBy(e => e.Item1).ThenBy(e => e.Item2)", list)).ToArray();
			Assert.That(callFormatter, Is.Not.Null);
			Assert.That(callFormatter, Has.Length.EqualTo(list.Count));
			var items = list.OrderBy(e => e.Item1).ThenBy(e => e.Item2).ToArray();
			for (var index = 0; index < items.Length; index++)
			{
				var orderedItem = items[index];
				var item = callFormatter[index];
				Assert.That(orderedItem, Is.EqualTo(item));
			}
		}
	}
}
