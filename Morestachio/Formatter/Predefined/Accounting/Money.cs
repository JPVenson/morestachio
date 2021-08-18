using System;
using System.Globalization;
using System.Linq;
using System.Xml;
using Morestachio.Formatter.Framework;
using Morestachio.Formatter.Framework.Attributes;
using Morestachio.Framework.Expression;
using Morestachio.Helper;

namespace Morestachio.Formatter.Predefined.Accounting
{
	/// <summary>
	///		Represents an amount of money
	/// </summary>
	public readonly struct Money : IFormattable, IEquatable<Money>
	{
		/// <summary>
		///		The monetary value
		/// </summary>
		public Number Value { get; }

		/// <summary>
		///		The values currency
		/// </summary>
		public Currency Currency { get; }

		/// <summary>
		///		Creates a new Money type
		/// </summary>
		public Money(Number val, Currency currency)
		{
			Value = val;
			Currency = currency;
		}

		/// <summary>
		///		Creates a new Money type
		/// </summary>
		public Money(Number val) : this(val, Currency.UnknownCurrency)
		{
		}

#pragma warning disable CS1591
		[MorestachioGlobalFormatter("Money", "Creates a new Money Object")]
		public static Money MoneyFactory(Number value)
		{
			return new Money(value);
		}

		[MorestachioGlobalFormatter("Money", "Creates a new Money Object")]
		public static Money MoneyFactory(Number value, [FormatterValueConverter(typeof(CurrencyTypeConverter))]Currency currency)
		{
			return new Money(value);
		}

		[MorestachioFormatter("GetMoney", "Calculates the value of the worktime by taking the rate and chargerate")]
		public static Money GetMoney([SourceObject] Worktime worktime, double rate, MoneyChargeRate chargeRate)
		{
			return Get(worktime, rate, chargeRate);
		}

		[MorestachioFormatter("GetMoney", "Calculates the value of the worktime by taking the rate and chargerate")]
		public static Money GetMoney([SourceObject] Worktime worktime, double rate, MoneyChargeRate chargeRate, [FormatterValueConverter(typeof(CurrencyTypeConverter))]Currency currency)
		{
			return Get(worktime, rate, chargeRate, currency);
		}

		[MorestachioFormatter("GetTax", "Gets the amount of Tax for this money object")]
		public static Money GetTax([SourceObject] Money worktime, double value)
		{
			return worktime.GetTax(value);
		}

		[MorestachioFormatter("Add", "Adds the value to a new money object and returns it")]
		[MorestachioOperator(OperatorTypes.Add, "Adds the value to a new money object and returns it")]
		public static Money Add([SourceObject] Money sourceValue, Money value)
		{
			return sourceValue.Add(value);
		}

		[MorestachioFormatter("Substract", "Substracts the value to a new money object and returns it")]
		[MorestachioOperator(OperatorTypes.Substract, "Substracts the value to a new money object and returns it")]
		public static Money Subtract([SourceObject] Money sourceValue, Money value)
		{
			return sourceValue.Subtract(value);
		}

		[MorestachioFormatter("Round", "Rounds the value using common commercial rules IEEE 754")]
		public static Money Round([SourceObject] Money worktime)
		{
			return worktime.CommercialRound();
		}
#pragma warning restore CS1591

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
			if (value.Currency.Equals(value.Currency))
			{
				return new Money(Value + value.Value);
			}

			throw new InvalidOperationException(
				$"Cannot add two different currencies. Please convert one of them to the other using the {typeof(CurrencyHandler)}");
		}

		/// <summary>
		///		Subtracts the value to a new money object and returns it
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public Money Subtract(Money value)
		{
			if (value.Currency.Equals(value.Currency))
			{
				return new Money(Value - value.Value);
			}

			throw new InvalidOperationException(
				$"Cannot add two different currencies. Please convert one of them to the other using the {typeof(CurrencyHandler)}");
		}

		/// <summary>
		///		Adds the value to a new money object and returns it
		/// </summary>
		public Money Round(Number by, MidpointRounding mode)
		{
			return new Money(Value.Round(by, mode));
		}

		/// <summary>
		///		Rounds the value using common commercial rules IEEE 754
		/// </summary>
		public Money CommercialRound()
		{
			return new Money(Value.Round(2));
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
			return Get(worktime, value, chargeRate, Currency.UnknownCurrency);
		}
		
		/// <summary>
		///		Calculates money based on the <see cref="Worktime"/> and the <see cref="MoneyChargeRate"/>
		/// </summary>
		/// <param name="worktime"></param>
		/// <param name="value"></param>
		/// <param name="chargeRate"></param>
		/// <returns></returns>
		public static Money Get(Worktime worktime, double value, MoneyChargeRate chargeRate, Currency currency)
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
			return new Money(val, currency);
		}

		/// <inheritdoc />
		public string ToString(string format, IFormatProvider formatProvider)
		{
			return Currency.ToString(this.Value.ToString(formatProvider), formatProvider);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return ToString("", CultureInfo.CurrentCulture);
		}

		/// <summary>
		///		Parses a text formatted as an currency value e.g. €100 or 100€ or EUR100 or 100EUR
		/// </summary>
		[MorestachioGlobalFormatter("ParseMoney", "Parses a string to a money object")]
		public static Money Parse(string text, CurrencyHandler handler = null)
		{
			var currencies = handler?.Currencies ?? CurrencyHandler.DefaultHandler.Currencies;
			var currencyByChar = currencies.FirstOrDefault(e => text.StartsWith(e.Value.IsoName) || text.StartsWith(e.Value.DisplayValue)).Value;
			if (currencyByChar.Equals(default))
			{
				currencyByChar = currencies.FirstOrDefault(e => text.EndsWith(e.Value.IsoName) || text.EndsWith(e.Value.DisplayValue)).Value;
				
				if (currencyByChar.Equals(default))
				{
					return new Money(Number.Parse(text));
				}
			}

			text = text.Replace(currencyByChar.DisplayValue, "").Replace(currencyByChar.IsoName, "");
			return new Money(Number.Parse(text), currencyByChar);
		}

		/// <inheritdoc />
		public bool Equals(Money other)
		{
			return Value.Equals(other.Value) && Currency.Equals(other.Currency);
		}

		/// <summary>
		///		Checks if both values are the same but maybe not of equal type and if the currency matches exactly
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Same(Money other)
		{
			return Value.Same(other.Value) && Currency.Equals(other.Currency);
		}
		
		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			return obj is Money other && Equals(other);
		}
		
		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				return (Value.GetHashCode() * 397) ^ Currency.GetHashCode();
			}
		}
	}
}