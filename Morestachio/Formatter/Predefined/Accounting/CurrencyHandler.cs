using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Morestachio.Formatter.Framework.Attributes;

namespace Morestachio.Formatter.Predefined.Accounting;

/// <summary>
///		The handler that contains a list of currencies and conversion factors
/// </summary>
public class CurrencyHandler
{
	static CurrencyHandler()
	{
		DefaultHandler = new CurrencyHandler();
		DefaultHandler.Currencies = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
			.GroupBy(e => e.LCID)
			.Select(f =>
			{
				//this has to be tried because in some systems we can get invalid regions
				try
				{
					return new RegionInfo(f.Key);
				}
				catch (Exception e)
				{
					return null;
				}
			})
			.Where(e => e != null)
			.GroupBy(e => e.CurrencyEnglishName)
			.Select(f => f.First()).ToDictionary(e => e.ISOCurrencySymbol, e => new Currency(e.CurrencySymbol, e.ISOCurrencySymbol));
	}

	/// <summary>
	/// 
	/// </summary>
	public CurrencyHandler()
	{
		Currencies = new Dictionary<string, Currency>();
		ConversionFactors = new HashSet<CurrencyConversion>();
	}

	/// <summary>
	///		The Default handler that contains all system wide known currencies
	/// </summary>
	[Description("The Default handler that contains all system wide known currencies")]
	public static CurrencyHandler DefaultHandler { get; }

	/// <summary>
	///		The list of all known currencies
	/// </summary>
	[Description("The list of all known currencies")]
	public IDictionary<string, Currency> Currencies { get; private set; }

	/// <summary>
	///		A list of known conversions for Currencies
	/// </summary>
	[Description("A list of known conversions for Currencies")]
	public HashSet<CurrencyConversion> ConversionFactors { get; set; }

	/// <summary>
	///		Creates a new Copy of this Currency Handler
	/// </summary>
	/// <returns></returns>
	public CurrencyHandler Clone()
	{
		return new CurrencyHandler()
		{
			Currencies = new Dictionary<string, Currency>(Currencies),
			ConversionFactors = new HashSet<CurrencyConversion>(ConversionFactors)
		};
	}

	/// <summary>
	///		Converts a money value from one currency to another
	/// </summary>
	/// <param name="one"></param>
	/// <param name="toCurrency"></param>
	/// <returns></returns>
	[MorestachioFormatter("Convert", "Converts a money object using an currency and the current known conversion factors")]
	public Money Convert(Money one, Currency toCurrency)
	{
		if (one.Currency.Equals(Currency.UnknownCurrency))
		{
			throw new InvalidOperationException("Cannot convert an Unknown currency to an known one");
		}

		if (toCurrency.Equals(Currency.UnknownCurrency))
		{
			throw new InvalidOperationException("Cannot convert an known currency to an unknown one");
		}
			
		var factorInfo = ConversionFactors.FirstOrDefault(e => e.ConversionEquals(one.Currency, toCurrency));
			
		if (factorInfo.Equals(default))
		{
			throw new InvalidOperationException("Cannot convert an known currency to an unknown one");
		}

		var factor = factorInfo.Factor;
		if (!factorInfo.FromCurrency.Equals(one.Currency))
		{
			factor *= -1;//invert the factor if the conversion is switched
		}
		return new Money(one.Value * factor, toCurrency);
	}
}