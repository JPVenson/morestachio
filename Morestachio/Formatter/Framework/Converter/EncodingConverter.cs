using System;
using System.Collections.Generic;
using System.Text;

namespace Morestachio.Formatter.Framework.Converter
{
	/// <summary>
	///		Parses an string to an encoding object
	/// </summary>
	public class EncodingConverter : IFormatterValueConverter
	{
		/// <inheritdoc />
		public bool CanConvert(Type sourceType, Type requestedType)
		{
			return (sourceType == typeof(string) || sourceType == typeof(Encoding))
			       && requestedType == typeof(Encoding);
		}
		
		/// <inheritdoc />
		public object Convert(object value, Type requestedType)
		{
			if (value is Encoding)
			{
				return value;
			}

			return Encoding.GetEncoding(value.ToString());
		}
	}
}
