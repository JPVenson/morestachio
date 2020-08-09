using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
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
		[TestCase(590, 60, MoneyChargeRate.PerHour, 19, "¤702.10")]
		[TestCase(590, 60, MoneyChargeRate.PerMinute, 19, "¤42,126.00")]
		[TestCase(590, 60, MoneyChargeRate.PerQuarterHour, 19, "¤2,808.40")]
		[TestCase(590, 60, MoneyChargeRate.PerHalfHour, 19, "¤1,404.20")]
		[TestCase(590, 60, MoneyChargeRate.PerDay, 19, "¤29.25")]
		[TestCase(590, 60, MoneyChargeRate.PerStartedHour, 19, "¤714.00")]
		public void TestMoneyCanAddAndFormatAsTime(double worktime, double value, MoneyChargeRate chargeRate, double tax, string expected)
		{
			var wt = new Worktime(worktime, WorktimePrecision.Minutes);
			var money = Money.Get(wt, value, chargeRate);
			var taxValue = money.GetTax(tax);
			money = money.Add(taxValue);
			Assert.That(money.ToString(null, CultureInfo.InvariantCulture), Is.EqualTo(expected));
		}
	}
}
