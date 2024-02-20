using System.ComponentModel;
using System.Globalization;
using Morestachio.Formatter.Framework;

namespace Morestachio.Formatter.Predefined.Accounting;

/// <summary>
///		A list of common used Currencies
/// </summary>
public static class WellKnownCurrencies
{
	/// <summary>
	///		US Dollar
	/// </summary>
	[Description("Gets the US-Doller Currency type denoted in $")]
	public static Currency USD => new Currency("$", "USD");

	/// <summary>
	///		EURO
	/// </summary>
	[Description("Gets the European Currency type denoted in €")]
	public static Currency EUR => new Currency("€", "EUR");

	/// <summary>
	///		Russian Ruble
	/// </summary>
	[Description("Gets the Russian Federations Currency type denoted in RUB")]
	public static Currency RUB => new Currency("RUB", "RUB");

	/// <summary>
	///		Indian Rupee
	/// </summary>
	[Description("Gets the Indian Currency type denoted in ₹")]
	public static Currency INR => new Currency("₹", "INR");

	/// <summary>
	///		Chinese Yuan
	/// </summary>
	[Description("Gets the People's Republic of China Currency type denoted in ¥")]
	public static Currency CNY => new Currency("¥", "CNY");

	/// <summary>
	///		Great British Pound
	/// </summary>
	[Description("Gets the Great British Currency type denoted in £")]
	public static Currency GBP => new Currency("£", "GBP");

	/// <summary>
	///		Japanese Yen
	/// </summary>
	[Description("Gets the Japanese Currency type denoted in ￥")]
	public static Currency JPY => new Currency("￥", "JPY");

	/// <summary>
	///		Gets the current regions currency info
	/// </summary>
	/// <returns></returns>
	[MorestachioGlobalFormatter("GetCurrentCurrency", "Gets the current currency")]
	public static Currency GetCurrentCurrency()
	{
		return new Currency(RegionInfo.CurrentRegion.CurrencySymbol, RegionInfo.CurrentRegion.ISOCurrencySymbol);
	}
}