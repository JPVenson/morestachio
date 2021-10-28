using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Morestachio.Tests.FormatterFunctionTests
{
	[TestFixture]
	public partial class ListExtensionsTests
	{
		[Test]
		public async Task TestAdd()
		{
			var list = new List<string>() { };
			var callFormatter = await CallFormatter<IList<string>>("this.Add('A', 'B')", list);
			Assert.That(callFormatter, Is.Not.Null);
			Assert.That(callFormatter, Has.Count.EqualTo(2));
			Assert.That(callFormatter, Contains.Item("A").And.Contain("B"));
		}

		[Test]
		public async Task TestRemove()
		{
			var list = new List<string>() { "A", "B", "Z", "@" };
			var callFormatter = await CallFormatter<IList<string>>("this.Remove('Z', '@')", list);
			Assert.That(callFormatter, Is.Not.Null);
			Assert.That(callFormatter, Has.Count.EqualTo(2));
			Assert.That(callFormatter, Contains.Item("A").And.Contain("B"));
		}

		[Test]
		public async Task TestInsert()
		{
			var list = new List<string>() { "A", "C", };
			var callFormatter = await CallFormatter<IList<string>>("this.Insert(1, 'B')", list);
			Assert.That(callFormatter, Is.Not.Null);
			Assert.That(callFormatter, Has.Count.EqualTo(3));
			Assert.That(callFormatter, Contains.Item("A").And.Contain("B").And.Contain("C"));
		}

		[Test]
		public async Task TestRemoveAt()
		{
			var list = new List<string>() { "A", "Z", "B" };
			var callFormatter = await CallFormatter<IList<string>>("this.RemoveAt(1)", list);
			Assert.That(callFormatter, Is.Not.Null);
			Assert.That(callFormatter, Has.Count.EqualTo(2));
			Assert.That(callFormatter, Contains.Item("A").And.Contain("B"));
		}
	}
}
