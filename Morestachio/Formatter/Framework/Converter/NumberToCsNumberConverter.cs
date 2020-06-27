using System;
using Morestachio.Helper;

namespace Morestachio.Formatter.Framework.Converter
{
	public class NumberToCsNumberConverter : IFormatterValueConverter
	{
		public static readonly IFormatterValueConverter Instance = new NumberToCsNumberConverter();

		public bool CanConvert(object value, Type requestedType)
		{
			if (value is Number && requestedType == typeof(Number))
			{
				return true;
			}

			return value is Number numb && requestedType.IsInstanceOfType(numb.Value);
		}

		public object Convert(object value, Type requestedType)
		{
			if (requestedType == typeof(Number))
			{
				return value;
			}

			return ((Number)value).Value;
		}
	}
}