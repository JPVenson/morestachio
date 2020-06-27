using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Morestachio.Helper;
using NUnit.Framework;

namespace Morestachio.Tests
{
	[TestFixture]
	[Parallelizable(ParallelScope.All)]
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
		[TestCase("0xA", 0xA)]
		public void CanParseNumber(string text, object expected)
		{
			Assert.That(Number.TryParse(text, CultureInfo.InvariantCulture, out var number), Is.True);
			Assert.That(number.Value, Is.EqualTo(expected).And.TypeOf(expected.GetType()));
		}

		[Test]
		[TestCase("10", "10", 10 + 10)]
		[TestCase("10D", "10", 10D + 10)]
		[TestCase("10F", "10", 10F + 10)]
		[TestCase("10U", "10", 10U + 10)]
		[TestCase("10.1", "10", 10.1 + 10)]
		[TestCase("0xA", "10", 0xA + 10)]
		[TestCase("0xA", "10D", 0xA + 10D)]
		public void CanAddNumbers(string left, string right, object expected)
		{
			Assert.That(Number.TryParse(left, CultureInfo.InvariantCulture, out var numberLeft), Is.True);
			Assert.That(Number.TryParse(right, CultureInfo.InvariantCulture, out var numberRight), Is.True);
			Assert.That(numberLeft.Add(numberRight).Value, Is.EqualTo(expected).And.TypeOf(expected.GetType()));
		}

		[Test]
		[TestCase("30", "10", 30 - 10)]
		[TestCase("30D", "10", 30D - 10)]
		[TestCase("30F", "10", 30F - 10)]
		[TestCase("30U", "10", 30U - 10)]
		[TestCase("30.1", "10", 30.1 - 10)]
		[TestCase("0x1E", "10", 0x1E - 10)]
		[TestCase("0x1E", "10D", 0x1E - 10D)]
		public void CanSubtractNumbers(string left, string right, object expected)
		{
			Assert.That(Number.TryParse(left, CultureInfo.InvariantCulture, out var numberLeft), Is.True);
			Assert.That(Number.TryParse(right, CultureInfo.InvariantCulture, out var numberRight), Is.True);
			Assert.That(numberLeft.Subtract(numberRight).Value, Is.EqualTo(expected).And.TypeOf(expected.GetType()));
		}

		[Test]
		[TestCase("5", "4", 5 * 4)]
		[TestCase("5D", "4", 5D * 4)]
		[TestCase("5F", "4", 5F * 4)]
		[TestCase("5U", "4", 5U * 4)]
		[TestCase("5.1", "4", 5.1 * 4)]
		[TestCase("0x5", "4", 0x5 * 4)]
		[TestCase("0x5", "4D", 0x5 * 4D)]
		public void CanMultiplyNumbers(string left, string right, object expected)
		{
			Assert.That(Number.TryParse(left, CultureInfo.InvariantCulture, out var numberLeft), Is.True);
			Assert.That(Number.TryParse(right, CultureInfo.InvariantCulture, out var numberRight), Is.True);
			Assert.That(numberLeft.Multiply(numberRight).Value, Is.EqualTo(expected).And.TypeOf(expected.GetType()));
		}

		[Test]
		[TestCase("30", "5", 30 / 5)]
		[TestCase("30D", "5", 30D / 5)]
		[TestCase("30F", "5", 30F / 5)]
		[TestCase("30U", "5", 30U / 5)]
		[TestCase("30.5", "5", 30.5 / 5)]
		[TestCase("0x1E", "5", 0x1E / 5)]
		[TestCase("0x1E", "5D", 0x1E / 5D)]
		public void CanDivideNumbers(string left, string right, object expected)
		{
			Assert.That(Number.TryParse(left, CultureInfo.InvariantCulture, out var numberLeft), Is.True);
			Assert.That(Number.TryParse(right, CultureInfo.InvariantCulture, out var numberRight), Is.True);
			Assert.That(numberLeft.Divide(numberRight).Value, Is.EqualTo(expected).And.TypeOf(expected.GetType()));
		}

		[Test]
		[TestCase("30", "5", 30 % 5)]
		[TestCase("30D", "5", 30D % 5)]
		[TestCase("30F", "5", 30F % 5)]
		[TestCase("30U", "5", 30U % 5)]
		[TestCase("30.5", "5", 30.5 % 5)]
		[TestCase("0x1E", "5", 0x1E % 5)]
		[TestCase("0x1E", "5D", 0x1E % 5D)]
		public void CanModuloNumbers(string left, string right, object expected)
		{
			Assert.That(Number.TryParse(left, CultureInfo.InvariantCulture, out var numberLeft), Is.True);
			Assert.That(Number.TryParse(right, CultureInfo.InvariantCulture, out var numberRight), Is.True);
			Assert.That(numberLeft.Modulo(numberRight).Value, Is.EqualTo(expected).And.TypeOf(expected.GetType()));
		}

		[Test]
		[TestCase(5L, "5", "Add", "10")]
		[TestCase(5UL, "5", "Add", "10")]
		[TestCase(5, "5", "Add", "10")]
		[TestCase(5U, "5", "Add", "10")]
		[TestCase(0x5, "5", "Add", "10")]
		[TestCase(5F, "5", "Add", "10")]
		[TestCase(5D, "5", "Add", "10")]
		public async Task CanUseNumberFunctionsOnData(object realData, string templateData, string operation, string expected)
		{
			var template = $"{{{{data.{operation}({templateData})}}}}";
			var data = new
			{
				data = realData
			};
			var parsingOptions = new ParserOptions(template, null, ParserFixture.DefaultEncoding);
			var result = await Parser.ParseWithOptions(parsingOptions).CreateAndStringifyAsync(data);
			Assert.That(result, Is.EqualTo(expected));
		}
	}
}
