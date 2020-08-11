using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Morestachio.Helper;

namespace Morestachio.Formatter.Framework.Converter
{
	/// <summary>
	///		Allows the usage of Number class in Morestachio Formatters
	/// </summary>
	public class NumberConverter : IFormatterValueConverter
	{
		/// <summary>
		///		The Instance of this Converter
		/// </summary>
		public static readonly IFormatterValueConverter Instance = new NumberConverter();
		
		/// <inheritdoc />
		public bool CanConvert(Type sourceType, Type requestedType)
		{
			if (sourceType == typeof(Number))
			{
				if (Number.CsFrameworkFloatingPointNumberTypes.Contains(requestedType) ||
				    Number.CsFrameworkIntegralTypes.Contains(requestedType))
				{
					return true;
				}
			}
			if (requestedType == typeof(Number))
			{
				if (Number.CsFrameworkFloatingPointNumberTypes.Contains(sourceType) ||
				    Number.CsFrameworkIntegralTypes.Contains(sourceType))
				{
					return true;
				}
			}

			return false;
		}

		/// <inheritdoc />
		public object Convert(object value, Type requestedType)
		{
			if (requestedType == typeof(Number))
			{
				if (value is Number)
				{
					return value;
				}

				return new Number(value as IConvertible);
			}

			return ((Number)value).Value;
		}
	}
}