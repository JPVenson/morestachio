using System.Globalization;
using Morestachio.Formatter.Framework;

namespace Morestachio.Formatter.Predefined.Accounting
{
	/// <summary>
	///		A list of common used Currencies
	/// </summary>
	public static class WellKnownCurrencies
	{
		/// <summary>
		///		US Dollar
		/// </summary>
		public static Currency USD => new Currency("$", "USD");
		/// <summary>
		///		EURO
		/// </summary>
		public static Currency EUR => new Currency("€", "EUR");
		/// <summary>
		///		Russian Ruble
		/// </summary>
		public static Currency RUB => new Currency("RUB", "RUB");

		/// <summary>
		///		Indian Rupee
		/// </summary>
		public static Currency INR => new Currency("₹", "INR");
		/// <summary>
		///		Chinese Yuan
		/// </summary>
		public static Currency CNY => new Currency("¥", "CNY");
		/// <summary>
		///		Great British Pound
		/// </summary>
		public static Currency GBP => new Currency("£", "GBP");

		/// <summary>
		///		Japanese Yen
		/// </summary>
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
}