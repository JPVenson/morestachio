using System;
using System.ComponentModel;

namespace Morestachio.Formatter.Framework.Converter
{
	public class StringParseConverter : IFormatterValueConverter
	{
		public StringParseConverter()
		{
			StringConverter = new StringConverter();
		}

		public StringConverter StringConverter { get; set; }

		public bool CanConvert(object value, Type requestedType)
		{
			return StringConverter.CanConvertTo(requestedType);
		}

		public object Convert(object value, Type requestedType)
		{
			return StringConverter.ConvertTo(value, requestedType);
		}
	}
}