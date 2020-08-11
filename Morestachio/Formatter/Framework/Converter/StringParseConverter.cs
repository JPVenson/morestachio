using System;
using System.ComponentModel;

namespace Morestachio.Formatter.Framework.Converter
{
	/// <summary>
	/// Provides a type converter to convert string objects to and from various other
	/// representations.
	/// </summary>
	public class StringParseConverter : IFormatterValueConverter
	{
		/// <summary>
		/// 
		/// </summary>
		public StringParseConverter()
		{
			StringConverter = new StringConverter();
		}

		/// <summary>
		///		Reusable Converter instance
		/// </summary>
		public StringConverter StringConverter { get; set; }

		/// <inheritdoc />
		public bool CanConvert(Type sourceType, Type requestedType)
		{
			return sourceType == typeof(string);
		}

		/// <inheritdoc />
		public object Convert(object value, Type requestedType)
		{
			return StringConverter.ConvertTo(value, requestedType);
		}
	}
}