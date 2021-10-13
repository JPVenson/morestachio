using System;
using System.Globalization;
using System.Linq;
using Morestachio.Formatter.Predefined.Accounting;
using NUnit.Framework;

namespace Morestachio.Tests
{
	[TestFixture]
	public class AccountingTests
	{
		[Test]
		[TestCase(5, 5, WorktimePrecision.Minutes, "00:10", "00.17")]
		[TestCase(5, 59, WorktimePrecision.Minutes, "01:04", "01.07")]
		[TestCase(5, 1, WorktimePrecision.Days, "24:05", "24.08")]
		[TestCase(5, 2, WorktimePrecision.Days, "48:05", "48.08")]
		public void TestWorktimeCanAddAndFormatAsTime(double value, double toAdd, WorktimePrecision withPrecision, string expectedTime, string expectedDecimal)
		{
			var wt = new Worktime(value, WorktimePrecision.Minutes);
			wt = wt.Add(toAdd, withPrecision);
			Assert.That(wt.ToString(null, CultureInfo.InvariantCulture), Is.EqualTo(expectedTime));
			Assert.That(wt.ToString("d", CultureInfo.InvariantCulture), Is.EqualTo(expectedDecimal));
		}

		[Test]
		[TestCase(590, 60, MoneyChargeRate.PerHour, 19, "702.10¤")]
		[TestCase(590, 60, MoneyChargeRate.PerMinute, 19, "42126.00¤")]
		[TestCase(590, 60, MoneyChargeRate.PerQuarterHour, 19, "2808.40¤")]
		[TestCase(590, 60, MoneyChargeRate.PerHalfHour, 19, "1404.20¤")]
		[TestCase(590, 60, MoneyChargeRate.PerDay, 19, "29.25¤")]
		[TestCase(590, 60, MoneyChargeRate.PerStartedHour, 19, "714.00¤")]
		public void TestMoneyCanAddAndFormatAsTime(double worktime, double value, MoneyChargeRate chargeRate, double tax, string expected)
		{
			var wt = new Worktime(worktime, WorktimePrecision.Minutes);
			var money = Money.Get(wt, value, chargeRate);
			var taxValue = money.GetTax(tax);
			money = money.Add(taxValue).CommercialRound();
			Assert.That(money.ToString(null, CultureInfo.InvariantCulture), Is.EqualTo(expected));
		}

		[Test]
		[TestCase("100€", "100€", 1)]
		[TestCase("100€", "50$", 0.5)]
		public void TestMoneyConversion(string currencyLeft, string expectedCurrency, double conversion)
		{
			var handler = CurrencyHandler.DefaultHandler.Clone();

			var moneyLeft = Money.Parse(currencyLeft, handler);
			Assert.That(moneyLeft, Is.Not.EqualTo(new Money()));
			var expected = Money.Parse(expectedCurrency, handler);
			Assert.That(expected, Is.Not.EqualTo(new Money()));

			handler.ConversionFactors.Add(new CurrencyConversion(moneyLeft.Currency, expected.Currency, conversion));

			var actual = handler.Convert(moneyLeft, expected.Currency);
			Assert.That(actual.Same(expected), Is.True);
		}
	}
}
