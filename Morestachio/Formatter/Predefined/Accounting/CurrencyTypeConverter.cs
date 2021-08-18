using System;
using Morestachio.Formatter.Framework.Converter;

namespace Morestachio.Formatter.Predefined.Accounting
{
	/// <summary>
	///		Can convert a string ISO4217 to an currency using the <see cref="CurrencyHandler.DefaultHandler"/>
	/// </summary>
	public class CurrencyTypeConverter : IFormatterValueConverter
	{
		/// <inheritdoc />
		public bool CanConvert(Type sourceType, Type requestedType)
		{
			return sourceType == typeof(string) && requestedType == typeof(Currency);
		}
		
		/// <inheritdoc />
		public object Convert(object value, Type requestedType)
		{
			if (CurrencyHandler.DefaultHandler.Currencies.TryGetValue(value.ToString(), out var currency))
			{
				return currency;
			}
			return Currency.UnknownCurrency;
		}
	}
}