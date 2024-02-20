using System;
using System.ComponentModel;

namespace Morestachio.Formatter.Predefined.Accounting;

/// <summary>
///		A Bidirectional conversion factor
/// </summary>
public readonly struct CurrencyConversion : IComparable<CurrencyConversion>, IEquatable<CurrencyConversion>
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="fromCurrency"></param>
	/// <param name="toCurrency"></param>
	/// <param name="factor"></param>
	public CurrencyConversion(Currency fromCurrency, Currency toCurrency, double factor)
	{
		FromCurrency = fromCurrency;
		ToCurrency = toCurrency;
		Factor = factor;
	}

	/// <summary>
	///		One part of currency
	/// </summary>
	[Description("One part of currency")]
	public Currency FromCurrency { get; }

	/// <summary>
	///		One part of currency
	/// </summary>
	[Description("One part of currency")]
	public Currency ToCurrency { get; }

	/// <summary>
	///		The factor of the conversion. Can be inverted to use in <see cref="ToCurrency"/> -> <see cref="FromCurrency"/>
	/// </summary>
	[Description("The factor of the conversion. Can be inverted to use in ToCurrency -> FromCurrency")]
	public double Factor { get; }

	/// <inheritdoc />
	public int CompareTo(CurrencyConversion other)
	{
		var fromCurrencyComparison = FromCurrency.CompareTo(other.FromCurrency);

		if (fromCurrencyComparison != 0)
		{
			return fromCurrencyComparison;
		}

		var toCurrencyComparison = ToCurrency.CompareTo(other.ToCurrency);

		if (toCurrencyComparison != 0)
		{
			return toCurrencyComparison;
		}

		return Factor.CompareTo(other.Factor);
	}

	/// <summary>
	///		Checks if the currencies are convertible
	/// </summary>
	public bool ConversionEquals(Currency fromCurrency, Currency toCurrency)
	{
		return ((FromCurrency.Equals(fromCurrency) && ToCurrency.Equals(toCurrency))
			|| (FromCurrency.Equals(toCurrency) && ToCurrency.Equals(fromCurrency)));
	}

	/// <inheritdoc />
	public bool Equals(CurrencyConversion other)
	{
		return ConversionEquals(FromCurrency, ToCurrency)
			&& Factor.Equals(other.Factor);
	}

	/// <inheritdoc />
	public override bool Equals(object obj)
	{
		return obj is CurrencyConversion other && Equals(other);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		unchecked
		{
			var hashCode = FromCurrency.GetHashCode();
			hashCode = (hashCode * 397) ^ ToCurrency.GetHashCode();
			hashCode = (hashCode * 397) ^ Factor.GetHashCode();
			return hashCode;
		}
	}
}