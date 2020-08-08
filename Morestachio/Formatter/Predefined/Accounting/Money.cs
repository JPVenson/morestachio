using System;
using Morestachio.Attributes;
using Morestachio.Formatter.Framework;
using Morestachio.Framework.Expression;

namespace Morestachio.Formatter.Predefined.Accounting
{
	/// <summary>
	///		Represents an amount of money
	/// </summary>
	public readonly struct Money : IFormattable
	{
		/// <summary>
		///		The value
		/// </summary>
		public double Value { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="val"></param>
		public Money(double val)
		{
			Value = val;
		}

		[MorestachioGlobalFormatter("Money", "Creates a new Money Object")]
		public static Money MoneyFactory(double value)
		{
			return new Money(value);
		}

		[MorestachioFormatter("GetMoney", "Calculates the value of the worktime by taking the rate and chargerate")]
		public static Money GetMoney([SourceObject]Worktime worktime, double rate, MoneyChargeRate chargeRate)
		{
			return Get(worktime, rate, chargeRate);
		}

		[MorestachioFormatter("GetTax", "Gets the amount of Tax for this money object")]
		public static Money GetTax([SourceObject]Money worktime, double value)
		{
			return worktime.GetTax(value);
		}

		[MorestachioFormatter("Add", "Adds the value to a new money object and returns it")]
		[MorestachioOperator(OperatorTypes.Add, "Adds the value to a new money object and returns it")]
		public static Money Add([SourceObject]Money worktime, Money value)
		{
			return worktime.Add(value);
		}

		[MorestachioFormatter("Round", "Rounds the value using common commercial rules IEEE 754")]
		public static Money Round([SourceObject]Money worktime)
		{
			return worktime.CommercialRound();
		}

		/// <summary>
		///		Gets the amount of Tax for this money object
		/// </summary>
		/// <param name="rate"></param>
		/// <returns></returns>
		public Money GetTax(double rate)
		{
			return new Money(Value / 100 * rate);
		}

		/// <summary>
		///		Adds the value to a new money object and returns it
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public Money Add(Money value)
		{
			return new Money(Value + value.Value);
		}

		/// <summary>
		///		Adds the value to a new money object and returns it
		/// </summary>
		public Money Round(int by, MidpointRounding mode)
		{
			return new Money(Math.Round(Value, by, mode));
		}

		/// <summary>
		///		Rounds the value using common commercial rules IEEE 754
		/// </summary>
		public Money CommercialRound()
		{
			return new Money(Math.Round(Value, 2));
		}

		/// <summary>
		///		Calculates money based on the <see cref="Worktime"/> and the <see cref="MoneyChargeRate"/>
		/// </summary>
		/// <param name="worktime"></param>
		/// <param name="value"></param>
		/// <param name="chargeRate"></param>
		/// <returns></returns>
		public static Money Get(Worktime worktime, double value, MoneyChargeRate chargeRate)
		{
			var minuteWorktime =
				Worktime.ConvertValue(worktime.TimeWorked, worktime.Precision, WorktimePrecision.Minutes);
			double val;
			switch (chargeRate)
			{
				case MoneyChargeRate.PerMinute:
					val = minuteWorktime * value;
					break;
				case MoneyChargeRate.PerHour:
					val = (minuteWorktime / 60) * value;
					break;
				case MoneyChargeRate.PerQuarterHour:
					val = (minuteWorktime / 15) * value;
					break;
				case MoneyChargeRate.PerHalfHour:
					val = (minuteWorktime / 30) * value;
					break;
				case MoneyChargeRate.PerDay:
					val = (minuteWorktime / 1440) * value;
					break;
				case MoneyChargeRate.PerStartedHour:
					var hours = minuteWorktime / 60;
					var fraction = hours % 60;
					if (fraction != 0)
					{
						hours -= hours % 1;
						hours += 1;
					}
					val = hours * value;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(chargeRate), chargeRate, null);
			}
			return new Money(val);
		}

		/// <inheritdoc />
		public string ToString(string format, IFormatProvider formatProvider)
		{
			return Value.ToString("C", formatProvider);
		}
	}
}