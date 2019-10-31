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
		[TestCase("-1", -1L)]
		[TestCase("1", 1L)]
		[TestCase("0", 0L)]
		[TestCase("1000000", 1000000L)]
		public void CanParseFullNumber(string text, long expected)
		{
			Assert.That(Number.TryParse(text, out var number), Is.True);
			Assert.That(number.Value, Is.EqualTo(expected));
		}

		[Test]
		[TestCase("-1.1", -1.1)]
		[TestCase("1.3", 1.3)]
		[TestCase("0.5", .5)]
		[TestCase("1000000.4564", 1000000.4564)]
		public void CanParseFrictionNumber(string text, decimal expected)
		{
			var parts = text.Split('.');

			Assert.That(Number.TryParse(parts[0], out var number), Is.True);
			Assert.That(Number.TryParse(parts[1], out var fraction), Is.True);

			var floatingPoint = number.WithFraction(fraction);
			Assert.That(floatingPoint.Value, Is.EqualTo(expected));
		}
	}
}
