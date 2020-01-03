using System;
using System.Globalization;

namespace Morestachio.Helper
{
	/// <summary>
	///		Encapsulates a late bound number
	/// </summary>
	public struct Number : IConvertible, IFormattable
	{
		/// <summary>
		///		Contains the numeric value
		/// </summary>
		public IConvertible Value { get; }

		/// <summary>
		///		Creates an new Integral number
		/// </summary>
		/// <param name="fullNumber"></param>
		public Number(long fullNumber)
		{
			Value = fullNumber;
		}

		/// <summary>
		///		Creates an new Integral number
		/// </summary>
		/// <param name="fullNumber"></param>
		public Number(ulong fullNumber)
		{
			Value = fullNumber;
		}

		/// <summary>
		///		Creates an new Integral number
		/// </summary>
		/// <param name="fullNumber"></param>
		public Number(int fullNumber)
		{
			Value = fullNumber;
		}

		/// <summary>
		///		Creates an new Integral number
		/// </summary>
		/// <param name="fullNumber"></param>
		public Number(uint fullNumber)
		{
			Value = fullNumber;
		}

		/// <summary>
		///		Creates an new Integral number
		/// </summary>
		/// <param name="fullNumber"></param>
		public Number(byte fullNumber)
		{
			Value = fullNumber;
		}

		/// <summary>
		///		Creates an new Integral number
		/// </summary>
		/// <param name="fullNumber"></param>
		public Number(sbyte fullNumber)
		{
			Value = fullNumber;
		}

		/// <summary>
		///		Creates an new Integral number
		/// </summary>
		/// <param name="fullNumber"></param>
		public Number(short fullNumber)
		{
			Value = fullNumber;
		}

		/// <summary>
		///		Creates an new Integral number
		/// </summary>
		/// <param name="fullNumber"></param>
		public Number(ushort fullNumber)
		{
			Value = fullNumber;
		}

		/// <summary>
		///		Creates an new floating number
		/// </summary>
		/// <param name="fullNumber"></param>
		public Number(float fullNumber)
		{
			Value = fullNumber;
		}

		/// <summary>
		///		Creates an new floating number
		/// </summary>
		/// <param name="fullNumber"></param>
		public Number(double fullNumber)
		{
			Value = fullNumber;
		}

		/// <summary>
		///		Creates an new floating number
		/// </summary>
		/// <param name="fullNumber"></param>
		public Number(decimal fullNumber)
		{
			Value = fullNumber;
		}

		/// <summary>
		///		Tries to parse the input to any number folloring roughly the rules of msbuild.
		///		Like:
		///		Has Suffix? (u,l,f,d)
		///		Has Prefix? (0x)
		///		Is int?
		///		Is long?
		///		Is Double?
		///		Is sbyte?
		///		Is ushort?
		///		Is decimal?
		/// </summary>
		/// <param name="input"></param>
		/// <param name="number"></param>
		/// <returns></returns>
		public static bool TryParse(string input, out Number number)
		{
			//according to MSDN folloring literals are allowed

			if (input.EndsWith("U", StringComparison.CurrentCultureIgnoreCase))
			{
				input = input.TrimEnd('u', 'U');
				//its an unsigned number
				//evaluate of which type it is
				if (uint.TryParse(input, out var uIntVal))
				{
					number = new Number(uIntVal);
					return true;
				}
				if (ushort.TryParse(input, out var ushortVal))
				{
					number = new Number(ushortVal);
					return true;
				}
				if (ulong.TryParse(input, out var uLongVal))
				{
					number = new Number(uLongVal);
					return true;
				}
				if (byte.TryParse(input, out var byteVal))
				{
					number = new Number(byteVal);
					return true;
				}

				number = default;
				return false;
			}

			if (input.EndsWith("l", StringComparison.InvariantCultureIgnoreCase))
			{
				//its an long
				if (input.EndsWith("ul", StringComparison.InvariantCultureIgnoreCase))
				{
					input = input.TrimEnd('u', 'U', 'l', 'L');
					//its unsigned
					if (ulong.TryParse(input, out var uLongVal))
					{
						number = new Number(uLongVal);
						return true;
					}

					number = default;
					return false;
				}
				input = input.TrimEnd('l', 'L');
				//its signed
				if (long.TryParse(input, out var explLongVal))
				{
					number = new Number(explLongVal);
					return true;
				}

				number = default;
				return false;
			}

			if (input.EndsWith("f", StringComparison.InvariantCultureIgnoreCase))
			{
				//its an float
				input = input.TrimEnd('f', 'F');
				if (float.TryParse(input, out var floatVal))
				{
					number = new Number(floatVal);
					return true;
				}

				number = default;
				return false;
			}

			if (input.EndsWith("d", StringComparison.InvariantCultureIgnoreCase))
			{
				//its an float
				input = input.TrimEnd('d', 'D');
				if (double.TryParse(input, out var doubleVal))
				{
					number = new Number(doubleVal);
					return true;
				}

				number = default;
				return false;
			}

			if (input.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
			{
				input = input.TrimStart('0', 'x', 'X');
				if (int.TryParse(input, NumberStyles.AllowHexSpecifier, CultureInfo.CurrentCulture, out var hexIntVal))
				{
					number = new Number(hexIntVal);
					return true;
				}

				number = default;
				return false;
			}
			
			//we start with parsing an int as its the default for any number in msbuild
			if (int.TryParse(input, out var intVal))
			{
				number = new Number(intVal);
				return true;
			}
			//if its bigger then an int it is most likely an long
			if (long.TryParse(input, out var longVal))
			{
				number = new Number(longVal);
				return true;
			}
			//if (uint.TryParse(input, out var impliUIntVal))
			//{
			//	number = new Number(impliUIntVal);
			//	return true;
			//}
			//if (ulong.TryParse(input, out var impliULongVal))
			//{
			//	number = new Number(impliULongVal);
			//	return true;
			//}
			
			if (double.TryParse(input, out var impliDoubleVal))
			{
				number = new Number(impliDoubleVal);
				return true;
			}
			if (sbyte.TryParse(input, out var sByteVal))
			{
				number = new Number(sByteVal);
				return true;
			}
			if (ushort.TryParse(input, out var shortVal))
			{
				number = new Number(shortVal);
				return true;
			}
			if (decimal.TryParse(input, out var decimalVal))
			{
				number = new Number(decimalVal);
				return true;
			}

			number = default;
			return false;
		}

		/// <inheritdoc />
		public TypeCode GetTypeCode()
		{
			return Value.GetTypeCode();
		}

		/// <inheritdoc />
		public bool ToBoolean(IFormatProvider provider)
		{
			return Value.ToBoolean(provider);
		}

		/// <inheritdoc />
		public char ToChar(IFormatProvider provider)
		{
			return Value.ToChar(provider);
		}

		/// <inheritdoc />
		public sbyte ToSByte(IFormatProvider provider)
		{
			return Value.ToSByte(provider);
		}

		/// <inheritdoc />
		public byte ToByte(IFormatProvider provider)
		{
			return Value.ToByte(provider);
		}

		/// <inheritdoc />
		public short ToInt16(IFormatProvider provider)
		{
			return Value.ToInt16(provider);
		}

		/// <inheritdoc />
		public ushort ToUInt16(IFormatProvider provider)
		{
			return Value.ToUInt16(provider);
		}

		/// <inheritdoc />
		public int ToInt32(IFormatProvider provider)
		{
			return Value.ToInt32(provider);
		}

		/// <inheritdoc />
		public uint ToUInt32(IFormatProvider provider)
		{
			return Value.ToUInt32(provider);
		}

		/// <inheritdoc />
		public long ToInt64(IFormatProvider provider)
		{
			return Value.ToInt64(provider);
		}

		/// <inheritdoc />
		public ulong ToUInt64(IFormatProvider provider)
		{
			return Value.ToUInt64(provider);
		}

		/// <inheritdoc />
		public float ToSingle(IFormatProvider provider)
		{
			return Value.ToSingle(provider);
		}

		/// <inheritdoc />
		public double ToDouble(IFormatProvider provider)
		{
			return Value.ToDouble(provider);
		}

		/// <inheritdoc />
		public decimal ToDecimal(IFormatProvider provider)
		{
			return Value.ToDecimal(provider);
		}

		/// <inheritdoc />
		public DateTime ToDateTime(IFormatProvider provider)
		{
			return Value.ToDateTime(provider);
		}

		/// <inheritdoc />
		public string ToString(IFormatProvider provider)
		{
			return Value.ToString(provider);
		}

		/// <inheritdoc />
		public object ToType(Type conversionType, IFormatProvider provider)
		{
			return Value.ToType(conversionType, provider);
		}

		/// <inheritdoc />
		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (Value is IFormattable formattable)
			{
				return formattable.ToString(format, formatProvider);
			}

			return Value.ToString(formatProvider);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return Value?.ToString();
		}
	}
}
