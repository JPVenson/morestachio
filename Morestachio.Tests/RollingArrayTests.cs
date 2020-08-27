using Morestachio.Framework.Tokenizing;
using NUnit.Framework;

namespace Morestachio.Tests
{
	[TestFixture]
	public class RollingArrayTests
	{
		public RollingArrayTests()
		{
			
		}

		[Test]
		public void TestIndexer()
		{
			var rollingArray = new Tokenizer.RollingArray(4);
			rollingArray.Add('A');
			Assert.That(rollingArray.Pos(), Is.EqualTo(0));
			Assert.That(rollingArray[0], Is.EqualTo('A'));
			rollingArray.Add('B');
			Assert.That(rollingArray.Pos(), Is.EqualTo(1));
			Assert.That(rollingArray[0], Is.EqualTo('A'));
			rollingArray.Add('C');
			Assert.That(rollingArray.Pos(), Is.EqualTo(2));
			Assert.That(rollingArray[0], Is.EqualTo('A'));
			rollingArray.Add('D');
			Assert.That(rollingArray.Pos(), Is.EqualTo(3));
			Assert.That(rollingArray[0], Is.EqualTo('A'));
			rollingArray.Add('E');
			Assert.That(rollingArray.Pos(), Is.EqualTo(0));
			Assert.That(rollingArray[0], Is.EqualTo('B'));
			Assert.That(rollingArray[1], Is.EqualTo('C'));
			Assert.That(rollingArray[2], Is.EqualTo('D'));
			Assert.That(rollingArray[3], Is.EqualTo('E'));
			rollingArray.Add('F');
			Assert.That(rollingArray.Pos(), Is.EqualTo(1));
			Assert.That(rollingArray[0], Is.EqualTo('C'));
			Assert.That(rollingArray[1], Is.EqualTo('D'));
			Assert.That(rollingArray[2], Is.EqualTo('E'));
			Assert.That(rollingArray[3], Is.EqualTo('F'));
			rollingArray.Add('G');
			Assert.That(rollingArray.Pos(), Is.EqualTo(2));
			Assert.That(rollingArray[0], Is.EqualTo('D'));
			Assert.That(rollingArray[1], Is.EqualTo('E'));
			Assert.That(rollingArray[2], Is.EqualTo('F'));
			Assert.That(rollingArray[3], Is.EqualTo('G'));
			rollingArray.Add('H');
			Assert.That(rollingArray.Pos(), Is.EqualTo(3));
			Assert.That(rollingArray[0], Is.EqualTo('E'));
			Assert.That(rollingArray[1], Is.EqualTo('F'));
			Assert.That(rollingArray[2], Is.EqualTo('G'));
			Assert.That(rollingArray[3], Is.EqualTo('H'));
			rollingArray.Add('I');
			Assert.That(rollingArray.Pos(), Is.EqualTo(0));
			Assert.That(rollingArray[0], Is.EqualTo('F'));
			Assert.That(rollingArray[1], Is.EqualTo('G'));
			Assert.That(rollingArray[2], Is.EqualTo('H'));
			Assert.That(rollingArray[3], Is.EqualTo('I'));
		}

		[Test]
		public void TestContents()
		{
			var rollingArray = new Tokenizer.RollingArray(3);
			rollingArray.Add('H');
			Assert.That(new string(rollingArray.ToArray()), Is.EqualTo("H\0\0"));
			rollingArray.Add('e');
			Assert.That(new string(rollingArray.ToArray()), Is.EqualTo("He\0"));
			rollingArray.Add('l');
			Assert.That(new string(rollingArray.ToArray()), Is.EqualTo("Hel"));
			rollingArray.Add('l');
			Assert.That(new string(rollingArray.ToArray()), Is.EqualTo("ell"));
			rollingArray.Add('o');
			Assert.That(new string(rollingArray.ToArray()), Is.EqualTo("llo"));
		}
		
		[Test]
		public void TestCanCheckForStartToken()
		{
			var rollingArray = new Tokenizer.RollingArray(3);
			rollingArray.Add('A');
			Assert.That(rollingArray.StartToken(), Is.False);
			rollingArray.Add('B');
			Assert.That(rollingArray.StartToken(), Is.False);
			rollingArray.Add('{');
			Assert.That(rollingArray.StartToken(), Is.False);
			rollingArray.Add('{');
			Assert.That(rollingArray.StartToken(), Is.True);
			rollingArray.Add('C');
			Assert.That(rollingArray.StartToken(), Is.False);
		}
		
		[Test]
		public void TestCanCheckForEndToken()
		{
			var rollingArray = new Tokenizer.RollingArray(4);
			rollingArray.Add('A');
			Assert.That(rollingArray.EndToken(), Is.False);
			rollingArray.Add('B');
			Assert.That(rollingArray.EndToken(), Is.False);
			rollingArray.Add('C');
			Assert.That(rollingArray.EndToken(), Is.False);
			rollingArray.Add('}');
			Assert.That(rollingArray.EndToken(), Is.False);
			rollingArray.Add('}');
			Assert.That(rollingArray.EndToken(), Is.True);
		}
	}
}