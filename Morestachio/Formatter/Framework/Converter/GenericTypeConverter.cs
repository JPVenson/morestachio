using System;
using System.ComponentModel;
using System.Linq;

namespace Morestachio.Formatter.Framework.Converter
{
	/// <summary>
	///		Uses the TypeDescriptor to convert native cs types
	/// </summary>
	public class GenericTypeConverter : IFormatterValueConverter
	{
		public static readonly IFormatterValueConverter Instance = new GenericTypeConverter();

		/// <inheritdoc />
		public bool CanConvert(object value, Type requestedType)
		{
			if (value is null)
			{
				return requestedType.IsClass;
			}

			var typeConverter = TypeDescriptor.GetConverter(value.GetType());
			return typeConverter.CanConvertTo(requestedType);
		}

		/// <inheritdoc />
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