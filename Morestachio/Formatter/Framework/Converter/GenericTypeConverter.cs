using System;
using System.ComponentModel;

namespace Morestachio.Formatter.Framework.Converter
{
	public class GenericTypeConverter : IFormatterValueConverter
	{
		public bool CanConvert(object value, Type requestedType)
		{
			if (value is null)
			{
				return requestedType.IsClass;
			}

			var typeConverter = TypeDescriptor.GetConverter(value.GetType());
			return typeConverter.CanConvertTo(requestedType);
		}

		public object Convert(object value, Type requestedType)
		{
			if (value is null)
			{
				return null;
			}
			var typeConverter = TypeDescriptor.GetConverter(value.GetType());
			return typeConverter.ConvertTo(value, requestedType);
		}
	}
}