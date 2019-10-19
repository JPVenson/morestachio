using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Morestachio.Formatter.Framework.Converter
{
	/// <summary>
	///		Can be used to Convert formatter parameters
	/// </summary>
	public interface IFormatterValueConverter
	{
		///  <summary>
		/// 		Used to check if this type can be converted
		///  </summary>
		///  <param name="value"></param>
		///  <param name="requestedType"></param>
		///  <returns></returns>
		bool CanConvert(object value, Type requestedType);

		///  <summary>
		/// 		Should convert the given value to the requestedType
		///  </summary>
		///  <param name="value"></param>
		///  <param name="requestedType"></param>
		///  <returns></returns>
		object Convert(object value, Type requestedType);
	}

	public class GenericTypeConverter : IFormatterValueConverter
	{
		public bool CanConvert(object value, Type requestedType)
		{
			var typeConverter = TypeDescriptor.GetConverter(requestedType);
			return typeConverter.CanConvertTo(requestedType);
		}

		public object Convert(object value, Type requestedType)
		{
			var typeConverter = TypeDescriptor.GetConverter(value.GetType());
			return typeConverter.ConvertTo(value, requestedType);
		}
	}

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