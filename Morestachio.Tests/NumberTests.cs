using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Morestachio.Helper;
using NUnit.Framework;

namespace Morestachio.Tests
{
	[TestFixture]
	public class NumberTests
	{
		[Test]
		//default cs number test: int
		[TestCase("-1", -1)]
		[TestCase("1", 1)]
		[TestCase("0", 0)]
		[TestCase("1000000", 1000000)]
		//should be printed as long
		[TestCase("10000000000", 10000000000)]

		//explicit long test
		[TestCase("-1l", -1L)]
		[TestCase("1l", 1L)]
		[TestCase("0l", 0L)]
		[TestCase("1000000l", 1000000L)]

		//unsigned default cs number test
		[TestCase("1u", 1u)]
		[TestCase("0u", 0u)]
		[TestCase("1000000u", 1000000u)]
		[TestCase("100000000000u", 100000000000u)]

		//default cs floating number test
		[TestCase("-1.1", -1.1)]
		[TestCase("1.3", 1.3)]
		[TestCase("0.5", .5)]
		[TestCase("1000000.4564", 1000000.4564)]
		[TestCase("1000000.45e64", 1000000.45e64)]

		//float number test
		[TestCase("-1.1f", -1.1f)]
		[TestCase("1.3f", 1.3f)]
		[TestCase("0.5f", .5f)]
		[TestCase("1000000.4564f", 1000000.4564f)]

		//double number test
		[TestCase("-1.1d", -1.1d)]
		[TestCase("1.3d", 1.3d)]
		[TestCase("0.5d", .5d)]
		[TestCase("1000000.4564d", 1000000.4564d)]
		
		//hexdecimal test
		[TestCase("0x123", 0x123)]
		public void CanParseNumber(string text, object expected)
		{
			Assert.That(Number.TryParse(text, out var number), Is.True);
			Assert.That(number.Value, Is.EqualTo(expected).And.TypeOf(expected.GetType()));
		}
	}
}
