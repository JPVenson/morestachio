using System;
using System.ComponentModel;
using System.Globalization;
using Morestachio.Formatter.Framework;
using Morestachio.Formatter.Framework.Attributes;
using Morestachio.Framework.Expression;

namespace Morestachio.Helper
{
	/// <summary>
	///		Encapsulates a late bound number
	/// </summary>
	public readonly struct Number :
		IComparable,
		IComparable<Number>,
		IComparable<ulong>,
		IComparable<long>,
		IComparable<uint>,
		IComparable<int>,
		IComparable<ushort>,
		IComparable<short>,
		IComparable<byte>,
		IComparable<sbyte>,
		IComparable<decimal>,
		IComparable<double>,
		IComparable<float>,
		IConvertible,
		IFormattable,
		IEquatable<Number>,
		IEquatable<ulong>,
		IEquatable<long>,
		IEquatable<uint>,
		IEquatable<int>,
		IEquatable<ushort>,
		IEquatable<short>,
		IEquatable<byte>,
		IEquatable<sbyte>,
		IEquatable<decimal>,
		IEquatable<double>,
		IEquatable<float>
	{
		/// <summary>
		///		Contains the numeric value
		/// </summary>
		[Description("Contains the numeric value")]
		public IConvertible Value
		{
			get { return _value; }
		}

		private readonly IConvertible _value;

		internal Number(IConvertible fullNumber)
		{
			if (fullNumber is Number nr)
			{
				fullNumber = nr._value;
			}
			_value = fullNumber;
		}

		/// <summary>
		///		Creates an new Integral number
		/// </summary>
		/// <param name="fullNumber"></param>
		public Number(long fullNumber)
		{
			_value = fullNumber;
		}

		/// <summary>
		///		Creates an new Integral number
		/// </summary>
		/// <param name="fullNumber"></param>
		public Number(ulong fullNumber)
		{
			_value = fullNumber;
		}

		/// <summary>
		///		Creates an new Integral number
		/// </summary>
		/// <param name="fullNumber"></param>
		public Number(int fullNumber)
		{
			_value = fullNumber;
		}

		/// <summary>
		///		Creates an new Integral number
		/// </summary>
		/// <param name="fullNumber"></param>
		public Number(uint fullNumber)
		{
			_value = fullNumber;
		}

		/// <summary>
		///		Creates an new Integral number
		/// </summary>
		/// <param name="fullNumber"></param>
		public Number(byte fullNumber)
		{
			_value = fullNumber;
		}

		/// <summary>
		///		Creates an new Integral number
		/// </summary>
		/// <param name="fullNumber"></param>
		public Number(sbyte fullNumber)
		{
			_value = fullNumber;
		}

		/// <summary>
		///		Creates an new Integral number
		/// </summary>
		/// <param name="fullNumber"></param>
		public Number(short fullNumber)
		{
			_value = fullNumber;
		}

		/// <summary>
		///		Creates an new Integral number
		/// </summary>
		/// <param name="fullNumber"></param>
		public Number(ushort fullNumber)
		{
			_value = fullNumber;
		}

		/// <summary>
		///		Creates an new floating number
		/// </summary>
		/// <param name="fullNumber"></param>
		public Number(float fullNumber)
		{
			_value = fullNumber;
		}

		/// <summary>
		///		Creates an new floating number
		/// </summary>
		/// <param name="fullNumber"></param>
		public Number(double fullNumber)
		{
			_value = fullNumber;
		}

		/// <summary>
		///		Creates an new floating number
		/// </summary>
		/// <param name="fullNumber"></param>
		public Number(decimal fullNumber)
		{
			_value = fullNumber;
		}

		/// <summary>
		///		checks if the object is ether an instance of <see cref="Number"/> or an .net build in floating point number
		/// </summary>
		/// <param name="number"></param>
		/// <returns></returns>
		public static bool IsFloatingPointNumber(object number)
		{
			if (number is Number nr)
			{
				number = nr._value;
			}

			if (number is Type nrType)
			{
				return nrType == typeof(decimal) ||
					   nrType == typeof(double) ||
					   nrType == typeof(float);
			}
			return number is decimal ||
				   number is double ||
				   number is float;
		}

		/// <summary>
		///		checks if the <see cref="Number"/> represents an floating point number
		/// </summary>
		/// <param name="number"></param>
		/// <returns></returns>
		public static bool IsFloatingPointNumber(Number number)
		{
			return IsFloatingPointNumber(number._value);
		}

		/// <summary>
		///		checks if the object is ether an instance of <see cref="Number"/> or an .net build in integral number
		/// </summary>
		/// <param name="number"></param>
		/// <returns></returns>
		public static bool IsIntegralNumber(object number)
		{
			if (number is Number nr)
			{
				number = nr._value;
			}

			if (number is Type nrType)
			{
				return nrType == typeof(ulong) ||
					   nrType == typeof(long) ||
					   nrType == typeof(uint) ||
					   nrType == typeof(int) ||
					   nrType == typeof(ushort) ||
					   nrType == typeof(short) ||
					   nrType == typeof(byte) ||
					   nrType == typeof(sbyte);
			}

			return number is ulong ||
				   number is long ||
				   number is uint ||
				   number is int ||
				   number is ushort ||
				   number is short ||
				   number is byte ||
				   number is sbyte;
		}

		/// <summary>
		///		checks if the <see cref="Number"/> represents an integral number
		/// </summary>
		/// <param name="number"></param>
		/// <returns></returns>
		public static bool IsIntegralNumber(Number number)
		{
			return IsIntegralNumber(number._value);
		}

		/// <summary>
		///		Contains the list of all .net Integral Types
		/// </summary>
		public static readonly Type[] CsFrameworkIntegralTypes = new[]
		{
			typeof(ulong    ),
			typeof(long     ),
			typeof(uint     ),
			typeof(int      ),
			typeof(ushort   ),
			typeof(short        ),
			typeof(byte     ),
			typeof(sbyte        )
		};

		/// <summary>
		///		Contains the list of all .net Floating Point numbers and the Decimal type
		/// </summary>
		public static readonly Type[] CsFrameworkFloatingPointNumberTypes = new[]
		{
			typeof(decimal  ),
			typeof(double   ),
			typeof(float    )
		};

		/// <summary>
		///		Should return the type of any number operations order of return value as some types take prevalence over others
		/// </summary>
		/// <param name="numberLeft"></param>
		/// <param name="numberRight"></param>
		/// <returns></returns>
		private static Type GetOperationTargetType(Number numberLeft, Number numberRight)
		{
			//if any value is a floating point number the result must always be a floating point number
			if (numberLeft._value is decimal || numberRight._value is decimal)
			{
				return typeof(decimal);
			}
			if (numberLeft._value is double || numberRight._value is double)
			{
				return typeof(double);
			}
			if (numberLeft._value is float || numberRight._value is float)
			{
				return typeof(float);
			}

			//if non of the types are floating point numbers check in order of most to least precision
			if (numberLeft._value is ulong || numberRight._value is ulong)
			{
				return typeof(ulong);
			}
			if (numberLeft._value is long || numberRight._value is long)
			{
				return typeof(long);
			}
			if (numberLeft._value is uint || numberRight._value is uint)
			{
				return typeof(uint);
			}
			if (numberLeft._value is int || numberRight._value is int)
			{
				return typeof(int);
			}
			if (numberLeft._value is ushort || numberRight._value is ushort)
			{
				return typeof(ushort);
			}
			if (numberLeft._value is short || numberRight._value is short)
			{
				return typeof(short);
			}
			if (numberLeft._value is byte || numberRight._value is byte)
			{
				return typeof(byte);
			}
			if (numberLeft._value is sbyte || numberRight._value is sbyte)
			{
				return typeof(sbyte);
			}
			throw new InvalidOperationException("Cannot determinate the numbers type");
		}

		#region MorestachioFormatter
#pragma warning disable CS1591

		[MorestachioFormatter("Add", "Adds two numbers")]
		[MorestachioFormatter("Plus", "Adds two numbers")]
		[MorestachioOperator(OperatorTypes.Add, "Adds two numbers")]
		public static Number Add(Number left, Number right)
		{
			return left.Add(right);
		}

		[MorestachioFormatter("Subtract", "Subtracts two numbers")]
		[MorestachioFormatter("Minus", "Subtracts two numbers")]
		[MorestachioOperator(OperatorTypes.Substract, "Subtracts two numbers")]
		public static Number Subtract(Number left, Number right)
		{
			return left.Subtract(right);
		}

		[MorestachioFormatter("Multiply", "Multiplies two numbers")]
		[MorestachioOperator(OperatorTypes.Multiply, "Multiplies two numbers")]
		public static Number Multiply(Number left, Number right)
		{
			return left.Multiply(right);
		}

		[MorestachioFormatter("Divide", "Divides two numbers")]
		[MorestachioOperator(OperatorTypes.Divide, "Divides two numbers")]
		public static Number Divide(Number left, Number right)
		{
			return left.Divide(right);
		}

		[MorestachioFormatter("Modulo", "Modulo two numbers")]
		[MorestachioOperator(OperatorTypes.Remainder, "Modulo two numbers")]
		public static Number Modulo(Number left, Number right)
		{
			return left.Modulo(right);
		}

		[MorestachioFormatter("ShiftLeft", "Shift two numbers")]
		[MorestachioOperator(OperatorTypes.ShiftLeft, "Shift two numbers")]
		public static Number ShiftLeft(Number left, Number right)
		{
			return left.ShiftLeft(right);
		}

		[MorestachioFormatter("ShiftRight", "Shift two numbers")]
		[MorestachioOperator(OperatorTypes.ShiftRight, "Shift two numbers")]
		public static Number ShiftRight(Number left, Number right)
		{
			return left.ShiftRight(right);
		}
		[MorestachioFormatter("SmallerAs", "Checks if the source number is smaller as the other number")]
		[MorestachioFormatter("SmallerThan", "Checks if the source number is smaller as the other number")]
		[MorestachioFormatter("LessThen", "Checks if the source number is smaller as the other number")]
		[MorestachioOperator(OperatorTypes.LessThen, "Checks if the source number is smaller as the other number")]
		public static bool LessThen(Number left, Number right)
		{
			return left.LessThen(right);
		}

		[MorestachioFormatter("SmallerOrEquals", "Checks if the source number is smaller as the other number")]
		[MorestachioOperator(OperatorTypes.LessOrEquals, "Checks if the source number is smaller as the other number")]
		public static bool SmallerOrEquals(Number left, Number right)
		{
			return left.LessThen(right) || left.Same(right);
		}

		[MorestachioFormatter("BiggerAs", "Checks if the source number is bigger as the other number")]
		[MorestachioFormatter("GreaterThen", "Checks if the source number is bigger as the other number")]
		[MorestachioOperator(OperatorTypes.GreaterThen, "Checks if the source number is bigger as the other number")]
		public static bool GreaterThen(Number left, Number right)
		{
			return left.GreaterThen(right);
		}

		[MorestachioFormatter("GreaterOrEquals", "Checks if the source number is bigger as the other number")]
		[MorestachioOperator(OperatorTypes.GreaterOrEquals, "Checks if the source number is bigger as the other number")]
		public static bool GreaterOrEquals(Number left, Number right)
		{
			return left.GreaterThen(right) || left.Same(right);
		}

		[MorestachioFormatter("Equals", "Checks if the two numbers are equal to each other")]
		[MorestachioOperator(OperatorTypes.Equals, "Checks if the two numbers are equal to each other")]
		public static bool Equals(Number left, Number right)
		{
			return left.Equals(right);
		}

		[MorestachioFormatter("UnEquals", "Checks if the two numbers are equal to each other")]
		[MorestachioOperator(OperatorTypes.UnEquals, "Checks if the two numbers are equal to each other")]
		public static bool UnEquals(Number left, Number right)
		{
			return !left.Equals(right);
		}

		[MorestachioFormatter("Same", "Checks if the two numbers are the same")]
		public static bool Same(Number left, Number right)
		{
			return left.Same(right);
		}

		[MorestachioFormatter("Abs", "Gets the Absolute value")]
		public static Number Abs(Number left)
		{
			return left.Abs();
		}

		[MorestachioFormatter("Round", "Rounds a double-precision floating-point value to a specified number of fractional digits.")]
		public static Number Round(Number left, Number right)
		{
			return left.Round(right);
		}

		[MorestachioFormatter("Negate", "Negates the current value")]
		public static Number Negate(Number left)
		{
			return left.Negate();
		}

		[MorestachioFormatter("Log", "")]
		public static Number Log(Number left, Number right)
		{
			return left.Log(right);
		}

		[MorestachioFormatter("Max", "Returns the bigger of the two numbers")]
		[MorestachioOperator(OperatorTypes.Bigger, "Returns the bigger of the two numbers")]
		public static Number Max(Number left, Number right)
		{
			return left.Max(right);
		}

		[MorestachioFormatter("Min", "Returns the smaller of the two numbers")]
		[MorestachioOperator(OperatorTypes.Smaller, "Returns the smaller of the two numbers")]
		public static Number Min(Number left, Number right)
		{
			return left.Max(right);
		}

		[MorestachioFormatter("Pow", "Gets this number power the times of the other number")]
		[MorestachioOperator(OperatorTypes.Pow, "Gets this number power the times of the other number")]
		public static Number Pow(Number left, Number right)
		{
			return left.Pow(right);
		}

		[MorestachioFormatter("IsNaN", "Gets if the current number object is not a number")]
		public static bool IsNaN(Number left)
		{
			return left.IsNaN();
		}

		private static readonly string[] SizeSuffixes =
			{"b", "kB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"};

		[MorestachioFormatter("ToBytes", "Format a number to it's equivalent in bytes.")]
		public static string ToBytes(Number value, Number? decimalPlaces = null)
		{
			if (value == NaN || value.Equals(0L))
			{
				return "0 b";
			}

			if (value < 0)
			{
				return "-" + ToBytes(value.Negate(), decimalPlaces);
			}

			var precisionValue = (decimalPlaces ?? 0).ToInt32(null);
			var valueLong = value.ToInt64(null);

			// mag is 0 for bytes, 1 for KB, 2, for MB, etc.
			var mag = (int)Math.Log(valueLong, 1024);
			//(int)Math.Log(value, 1024);

			// 1L << (mag * 10) == 2 ^ (10 * mag) 
			// [i.e. the number of bytes in the unit corresponding to mag]

			var adjustedSize = (decimal)valueLong / (1L << (mag * 10));

			// make adjustment when the value is large enough that
			// it would round up to 1000 or more
			if (Math.Round(adjustedSize, precisionValue) >= 1000)
			{
				mag += 1;
				adjustedSize /= 1024;
			}

			return string.Format("{0:n" + precisionValue + "} {1}",
				adjustedSize,
				SizeSuffixes[mag]);

		}

		[MorestachioGlobalFormatter("ParseNumber", "Parses a string into a number. Returns NaN if invalid")]
		public static Number Parse(string text)
		{
			if (TryParse(text, null, out var nr))
			{
				return nr;
			}

			return NaN;
		}

#pragma warning restore CS1591
		#endregion

		/// <summary>
		///		Zero Int
		/// </summary>
		public static readonly Number Zero = new Number(0);

		/// <summary>
		///		Minus one int
		/// </summary>
		public static readonly Number MinusOne = new Number(-1);

		/// <summary>
		///		Represents a value that is NaN (Not a number)
		/// </summary>
		public static readonly Number NaN = new Number(double.NaN);

		#region Number Operations

		/// <summary>
		///		Returns if this number represents not a number
		/// </summary>
		/// <returns></returns>
		public bool IsNaN()
		{
			return double.IsNaN(ToDouble(null));
		}

		/// <summary>
		///		Returns the bigger of ether this value or the other value
		/// </summary>
		/// <returns></returns>
		public Number Max(Number other)
		{
			return this > other ? this : other;
		}

		/// <summary>
		///		Returns the smaller of ether this value or the other value
		/// </summary>
		/// <returns></returns>
		public Number Min(Number other)
		{
			return this < other ? this : other;
		}

		/// <summary>
		///		Gets this number power the times of the other number
		/// </summary>
		/// <returns></returns>
		public Number Pow(Number other)
		{
			return Math.Pow(ToDouble(null), other.ToDouble(null));
		}

		/// <summary>
		///		Gets the Absolute value
		/// </summary>
		/// <returns></returns>
		public Number Log(Number other)
		{
			return Math.Log(ToDouble(null), other.ToDouble(null));
		}

		/// <summary>
		///		Gets the Absolute value
		/// </summary>
		/// <returns></returns>
		public Number Log()
		{
			return Log(Math.E);
		}

		/// <summary>
		///		Gets the Absolute value
		/// </summary>
		/// <returns></returns>
		public Number Log10()
		{
			return Log(10D);
		}

		/// <summary>
		///		Returns the sine of the current angle.
		/// </summary>
		/// <returns></returns>
		public Number Sin()
		{
			return Math.Sin(ToDouble(null));
		}

		/// <summary>
		///		Returns the hyperbolic sine of the current angle.
		/// </summary>
		/// <returns></returns>
		public Number Sinh()
		{
			return Math.Sinh(ToDouble(null));
		}

		/// <summary>
		///		Returns the square root of the current value
		/// </summary>
		/// <returns></returns>
		public Number Sqrt()
		{
			return Math.Sqrt(ToDouble(null));
		}

		/// <summary>
		///		Returns the hyperbolic tangent of the specified angle.
		/// </summary>
		/// <returns></returns>
		public Number Tanh()
		{
			return Math.Tanh(ToDouble(null));
		}

		/// <summary>
		///		Returns the cosine of the specified angle.
		/// </summary>
		/// <returns></returns>
		public Number Cos()
		{
			return Math.Cos(ToDouble(null));
		}

		/// <summary>
		///		Returns the hyperbolic cosine of the specified angle.
		/// </summary>
		/// <returns></returns>
		public Number Cosh()
		{
			return Math.Cosh(ToDouble(null));
		}

		/// <summary>
		///		Returns the angle whose cosine is the specified number
		/// </summary>
		/// <returns></returns>
		public Number Acos()
		{
			return Math.Acos(ToDouble(null));
		}

		/// <summary>
		///		Returns the angle whose sine is the specified number.
		/// </summary>
		/// <returns></returns>
		public Number Asin()
		{
			return Math.Asin(ToDouble(null));
		}

		/// <summary>
		///		Returns the hyperbolic cosine of the specified angle.
		/// </summary>
		/// <returns></returns>
		public Number Atan()
		{
			return Math.Atan(ToDouble(null));
		}

		/// <summary>
		///		Returns the hyperbolic cosine of the specified angle.
		/// </summary>
		/// <returns></returns>
		public Number Atan2(Number x)
		{
			return Math.Atan2(ToDouble(null), x.ToDouble(null));
		}

		/// <summary>
		///		Calculates the integral part of a specified number.
		/// </summary>
		/// <returns></returns>
		public Number Truncate()
		{
			if (_value is decimal)
			{
				return Math.Truncate(ToDecimal(null));
			}
			return Math.Truncate(ToDouble(null));
		}

		/// <summary>
		///		Returns the smallest integral value that is greater than or equal to the specified decimal number.
		/// </summary>
		/// <returns></returns>
		public Number Ceiling()
		{
			if (_value is decimal)
			{
				return Math.Ceiling(ToDecimal(null));
			}
			return Math.Ceiling(ToDouble(null));
		}

		/// <summary>
		///		Returns the smallest integral value that is greater than or equal to the specified decimal number.
		/// </summary>
		/// <returns></returns>
		public Number Floor()
		{
			if (_value is decimal)
			{
				return Math.Floor(ToDecimal(null));
			}
			return Math.Floor(ToDouble(null));
		}

		/// <summary>
		///		Gets the Absolute value
		/// </summary>
		/// <returns></returns>
		public Number Negate()
		{
			var targetType = _value.GetType();
			if (targetType == typeof(decimal))
			{
				return new Number(-ToDecimal(null));
			}
			if (targetType == typeof(double))
			{
				return new Number(-ToDouble(null));
			}
			if (targetType == typeof(float))
			{
				return new Number(-ToSingle(null));
			}
			if (targetType == typeof(long))
			{
				return new Number(-ToInt64(null));
			}
			if (targetType == typeof(uint))
			{
				return new Number(-ToDouble(null));
			}
			if (targetType == typeof(int))
			{
				return new Number(-ToInt32(null));
			}
			if (targetType == typeof(ushort))
			{
				return new Number(-ToUInt16(null));
			}
			if (targetType == typeof(short))
			{
				return new Number(-ToInt16(null));
			}
			if (targetType == typeof(byte))
			{
				return new Number(-ToByte(null));
			}
			if (targetType == typeof(sbyte))
			{
				return new Number(-ToSByte(null));
			}
			throw new InvalidCastException($"Cannot get the absolute value for {_value} ({_value.GetType()})");
		}

		/// <summary>
		///		Gets the Absolute value
		/// </summary>
		/// <returns></returns>
		public Number Abs()
		{
			var targetType = _value.GetType();
			if (targetType == typeof(decimal))
			{
				return new Number(Math.Abs(ToDecimal(null)));
			}
			if (targetType == typeof(double))
			{
				return new Number(Math.Abs(ToDouble(null)));
			}
			if (targetType == typeof(float))
			{
				return new Number(Math.Abs(ToSingle(null)));
			}
			if (targetType == typeof(long))
			{
				return new Number(Math.Abs(ToInt64(null)));
			}
			if (targetType == typeof(uint))
			{
				return new Number(Math.Abs(ToDouble(null)));
			}
			if (targetType == typeof(int))
			{
				return new Number(Math.Abs(ToInt32(null)));
			}
			if (targetType == typeof(ushort))
			{
				return new Number(Math.Abs(ToUInt16(null)));
			}
			if (targetType == typeof(short))
			{
				return new Number(Math.Abs(ToInt16(null)));
			}
			if (targetType == typeof(byte))
			{
				return new Number(Math.Abs(ToByte(null)));
			}
			if (targetType == typeof(sbyte))
			{
				return new Number(Math.Abs(ToSByte(null)));
			}
			throw new InvalidCastException($"Cannot get the absolute value for {_value} ({_value.GetType()})");
		}

		/// <summary>
		///		Rounds a decimal value to a specified number of fractional digits.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public Number Round(Number other)
		{
			return Round(other, MidpointRounding.ToEven);
		}
		
		/// <summary>
		///		Rounds a decimal value to a specified number of fractional digits.
		/// </summary>
		public Number Round(Number other, MidpointRounding mode)
		{
			if (_value is decimal)
			{
				return Math.Round(_value.ToDecimal(null), other.ToInt32(null), mode);
			}
			return Math.Round(_value.ToDouble(null), other.ToInt32(null), mode);
		}

		/// <summary>
		///		Adds the two numbers together
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public Number Add(Number other)
		{
			var targetType = GetOperationTargetType(this, other);
			if (targetType == typeof(decimal))
			{
				return new Number(ToDecimal(null) + other.ToDecimal(null));
			}
			if (targetType == typeof(double))
			{
				return new Number(ToDouble(null) + other.ToDouble(null));
			}
			if (targetType == typeof(float))
			{
				return new Number(ToSingle(null) + other.ToSingle(null));
			}
			if (targetType == typeof(ulong))
			{
				return new Number(ToUInt64(null) + other.ToUInt64(null));
			}
			if (targetType == typeof(long))
			{
				return new Number(ToInt64(null) + other.ToInt64(null));
			}
			if (targetType == typeof(uint))
			{
				return new Number(ToUInt32(null) + other.ToUInt32(null));
			}
			if (targetType == typeof(int))
			{
				return new Number(ToInt32(null) + other.ToInt32(null));
			}
			if (targetType == typeof(ushort))
			{
				return new Number(ToUInt16(null) + other.ToUInt16(null));
			}
			if (targetType == typeof(short))
			{
				return new Number(ToInt16(null) + other.ToInt16(null));
			}
			if (targetType == typeof(byte))
			{
				return new Number(ToByte(null) + other.ToByte(null));
			}
			if (targetType == typeof(sbyte))
			{
				return new Number(ToSByte(null) + other.ToSByte(null));
			}
			throw new InvalidCastException($"Cannot convert {other._value} ({other._value.GetType()}) or {_value} ({_value.GetType()}) to a numeric type");
		}

		/// <summary>
		///		Subtracts the other number from this number
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public Number Subtract(Number other)
		{
			var targetType = GetOperationTargetType(this, other);
			if (targetType == typeof(decimal))
			{
				return new Number(ToDecimal(null) - other.ToDecimal(null));
			}
			if (targetType == typeof(double))
			{
				return new Number(ToDouble(null) - other.ToDouble(null));
			}
			if (targetType == typeof(float))
			{
				return new Number(ToSingle(null) - other.ToSingle(null));
			}
			if (targetType == typeof(ulong))
			{
				return new Number(ToUInt64(null) - other.ToUInt64(null));
			}
			if (targetType == typeof(long))
			{
				return new Number(ToInt64(null) - other.ToInt64(null));
			}
			if (targetType == typeof(uint))
			{
				return new Number(ToUInt32(null) - other.ToUInt32(null));
			}
			if (targetType == typeof(int))
			{
				return new Number(ToInt32(null) - other.ToInt32(null));
			}
			if (targetType == typeof(ushort))
			{
				return new Number(ToUInt16(null) - other.ToUInt16(null));
			}
			if (targetType == typeof(ushort))
			{
				return new Number(ToInt16(null) - other.ToInt16(null));
			}
			if (targetType == typeof(ushort))
			{
				return new Number(ToByte(null) - other.ToByte(null));
			}
			if (targetType == typeof(sbyte))
			{
				return new Number(ToSByte(null) - other.ToSByte(null));
			}
			throw new InvalidCastException($"Cannot convert {other._value} ({other._value.GetType()}) or {_value} ({_value.GetType()}) to a numeric type");
		}

		/// <summary>
		///		Multiplies the two numbers
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public Number Multiply(Number other)
		{
			var targetType = GetOperationTargetType(this, other);
			if (targetType == typeof(decimal))
			{
				return new Number(ToDecimal(null) * other.ToDecimal(null));
			}
			if (targetType == typeof(double))
			{
				return new Number(ToDouble(null) * other.ToDouble(null));
			}
			if (targetType == typeof(float))
			{
				return new Number(ToSingle(null) * other.ToSingle(null));
			}
			if (targetType == typeof(ulong))
			{
				return new Number(ToUInt64(null) * other.ToUInt64(null));
			}
			if (targetType == typeof(long))
			{
				return new Number(ToInt64(null) * other.ToInt64(null));
			}
			if (targetType == typeof(uint))
			{
				return new Number(ToUInt32(null) * other.ToUInt32(null));
			}
			if (targetType == typeof(int))
			{
				return new Number(ToInt32(null) * other.ToInt32(null));
			}
			if (targetType == typeof(ushort))
			{
				return new Number(ToUInt16(null) * other.ToUInt16(null));
			}
			if (targetType == typeof(ushort))
			{
				return new Number(ToInt16(null) * other.ToInt16(null));
			}
			if (targetType == typeof(ushort))
			{
				return new Number(ToByte(null) * other.ToByte(null));
			}
			if (targetType == typeof(sbyte))
			{
				return new Number(ToSByte(null) * other.ToSByte(null));
			}
			throw new InvalidCastException($"Cannot convert {other._value} ({other._value.GetType()}) or {_value} ({_value.GetType()}) to a numeric type");
		}

		/// <summary>
		///		Divides the other number from this number
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public Number Divide(Number other)
		{
			var targetType = GetOperationTargetType(this, other);
			if (targetType == typeof(decimal))
			{
				return new Number(ToDecimal(null) / other.ToDecimal(null));
			}
			if (targetType == typeof(double))
			{
				return new Number(ToDouble(null) / other.ToDouble(null));
			}
			if (targetType == typeof(float))
			{
				return new Number(ToSingle(null) / other.ToSingle(null));
			}
			if (targetType == typeof(ulong))
			{
				return new Number(ToUInt64(null) / other.ToUInt64(null));
			}
			if (targetType == typeof(long))
			{
				return new Number(ToInt64(null) / other.ToInt64(null));
			}
			if (targetType == typeof(uint))
			{
				return new Number(ToUInt32(null) / other.ToUInt32(null));
			}
			if (targetType == typeof(int))
			{
				return new Number(ToInt32(null) / other.ToInt32(null));
			}
			if (targetType == typeof(ushort))
			{
				return new Number(ToUInt16(null) / other.ToUInt16(null));
			}
			if (targetType == typeof(ushort))
			{
				return new Number(ToInt16(null) / other.ToInt16(null));
			}
			if (targetType == typeof(ushort))
			{
				return new Number(ToByte(null) / other.ToByte(null));
			}
			if (targetType == typeof(sbyte))
			{
				return new Number(ToSByte(null) / other.ToSByte(null));
			}
			throw new InvalidCastException($"Cannot convert {other._value} ({other._value.GetType()}) or {_value} ({_value.GetType()}) to a numeric type");
		}

		/// <summary>
		///		Gets the reminder from the diversion of the other number
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public Number Modulo(Number other)
		{
			var targetType = GetOperationTargetType(this, other);
			if (targetType == typeof(decimal))
			{
				return new Number(ToDecimal(null) % other.ToDecimal(null));
			}
			if (targetType == typeof(double))
			{
				return new Number(ToDouble(null) % other.ToDouble(null));
			}
			if (targetType == typeof(float))
			{
				return new Number(ToSingle(null) % other.ToSingle(null));
			}
			if (targetType == typeof(ulong))
			{
				return new Number(ToUInt64(null) % other.ToUInt64(null));
			}
			if (targetType == typeof(long))
			{
				return new Number(ToInt64(null) % other.ToInt64(null));
			}
			if (targetType == typeof(uint))
			{
				return new Number(ToUInt32(null) % other.ToUInt32(null));
			}
			if (targetType == typeof(int))
			{
				return new Number(ToInt32(null) % other.ToInt32(null));
			}
			if (targetType == typeof(ushort))
			{
				return new Number(ToUInt16(null) % other.ToUInt16(null));
			}
			if (targetType == typeof(ushort))
			{
				return new Number(ToInt16(null) % other.ToInt16(null));
			}
			if (targetType == typeof(ushort))
			{
				return new Number(ToByte(null) % other.ToByte(null));
			}
			if (targetType == typeof(sbyte))
			{
				return new Number(ToSByte(null) % other.ToSByte(null));
			}
			throw new InvalidCastException($"Cannot convert {other._value} ({other._value.GetType()}) or {_value} ({_value.GetType()}) to a numeric type");
		}

		/// <summary>
		///		Shifts its left-hand operand right by the number of bits defined by its right-hand operand.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public Number ShiftRight(Number other)
		{
			var targetType = GetOperationTargetType(this, other);
			if (targetType == typeof(ulong))
			{
				return new Number(ToUInt64(null) >> other.ToInt32(null));
			}
			if (targetType == typeof(long))
			{
				return new Number(ToInt64(null) >> other.ToInt32(null));
			}
			if (targetType == typeof(uint))
			{
				return new Number(ToUInt32(null) >> other.ToInt32(null));
			}
			if (targetType == typeof(int))
			{
				return new Number(ToInt32(null) >> other.ToInt32(null));
			}
			if (targetType == typeof(ushort))
			{
				return new Number(ToUInt16(null) >> other.ToInt32(null));
			}
			if (targetType == typeof(ushort))
			{
				return new Number(ToInt16(null) >> other.ToInt32(null));
			}
			if (targetType == typeof(ushort))
			{
				return new Number(ToByte(null) >> other.ToInt32(null));
			}
			if (targetType == typeof(sbyte))
			{
				return new Number(ToSByte(null) >> other.ToInt32(null));
			}
			throw new InvalidCastException($"Cannot convert {other._value} ({other._value.GetType()}) or {_value} ({_value.GetType()}) to a numeric type");
		}

		/// <summary>
		///		Shifts its left-hand operand left by the number of bits defined by its right-hand operand.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public Number ShiftLeft(Number other)
		{
			var targetType = GetOperationTargetType(this, other);
			if (targetType == typeof(ulong))
			{
				return new Number(ToUInt64(null) << other.ToInt32(null));
			}
			if (targetType == typeof(long))
			{
				return new Number(ToInt64(null) << other.ToInt32(null));
			}
			if (targetType == typeof(uint))
			{
				return new Number(ToUInt32(null) << other.ToInt32(null));
			}
			if (targetType == typeof(int))
			{
				return new Number(ToInt32(null) << other.ToInt32(null));
			}
			if (targetType == typeof(ushort))
			{
				return new Number(ToUInt16(null) << other.ToInt32(null));
			}
			if (targetType == typeof(ushort))
			{
				return new Number(ToInt16(null) << other.ToInt32(null));
			}
			if (targetType == typeof(ushort))
			{
				return new Number(ToByte(null) << other.ToInt32(null));
			}
			if (targetType == typeof(sbyte))
			{
				return new Number(ToSByte(null) << other.ToInt32(null));
			}
			throw new InvalidCastException($"Cannot convert {other._value} ({other._value.GetType()}) or {_value} ({_value.GetType()}) to a numeric type");
		}

		/// <summary>
		///		Checks if this number is bigger as the other number
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool GreaterThen(Number other)
		{
			var targetType = GetOperationTargetType(this, other);
			if (targetType == typeof(decimal))
			{
				return ToDecimal(null) > other.ToDecimal(null);
			}
			if (targetType == typeof(double))
			{
				return ToDouble(null) > other.ToDouble(null);
			}
			if (targetType == typeof(float))
			{
				return ToSingle(null) > other.ToSingle(null);
			}
			if (targetType == typeof(ulong))
			{
				return ToUInt64(null) > other.ToUInt64(null);
			}
			if (targetType == typeof(long))
			{
				return ToInt64(null) > other.ToInt64(null);
			}
			if (targetType == typeof(uint))
			{
				return ToUInt32(null) > other.ToUInt32(null);
			}
			if (targetType == typeof(int))
			{
				return ToInt32(null) > other.ToInt32(null);
			}
			if (targetType == typeof(ushort))
			{
				return ToUInt16(null) > other.ToUInt16(null);
			}
			if (targetType == typeof(ushort))
			{
				return ToInt16(null) > other.ToInt16(null);
			}
			if (targetType == typeof(ushort))
			{
				return ToByte(null) > other.ToByte(null);
			}
			if (targetType == typeof(sbyte))
			{
				return ToSByte(null) > other.ToSByte(null);
			}
			throw new InvalidCastException($"Cannot convert {other._value} ({other._value.GetType()}) or {_value} ({_value.GetType()}) to a numeric type");
		}

		/// <summary>
		///		Checks if this number is smaller as the other number
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool LessThen(Number other)
		{
			var targetType = GetOperationTargetType(this, other);
			if (targetType == typeof(decimal))
			{
				return ToDecimal(null) < other.ToDecimal(null);
			}
			if (targetType == typeof(double))
			{
				return ToDouble(null) < other.ToDouble(null);
			}
			if (targetType == typeof(float))
			{
				return ToSingle(null) < other.ToSingle(null);
			}
			if (targetType == typeof(ulong))
			{
				return ToUInt64(null) < other.ToUInt64(null);
			}
			if (targetType == typeof(long))
			{
				return ToInt64(null) < other.ToInt64(null);
			}
			if (targetType == typeof(uint))
			{
				return ToUInt32(null) < other.ToUInt32(null);
			}
			if (targetType == typeof(int))
			{
				return ToInt32(null) < other.ToInt32(null);
			}
			if (targetType == typeof(ushort))
			{
				return ToUInt16(null) < other.ToUInt16(null);
			}
			if (targetType == typeof(ushort))
			{
				return ToInt16(null) < other.ToInt16(null);
			}
			if (targetType == typeof(ushort))
			{
				return ToByte(null) < other.ToByte(null);
			}
			if (targetType == typeof(sbyte))
			{
				return ToSByte(null) < other.ToSByte(null);
			}
			throw new InvalidCastException($"Cannot convert {other._value} ({other._value.GetType()}) or {_value} ({_value.GetType()}) to a numeric type");
		}

		/// <summary>
		///		Checks if both numbers are the same type and also the same value
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(Number other)
		{
			if (other._value?.GetType() != _value?.GetType())
			{
				return false;
			}

			return Same(other);
		}

		/// <summary>
		///		Checks if both numbers are the same value
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Same(Number other)
		{
			if (Equals(other._value, _value))
			{
				return true;
			}

			var targetType = _value.GetType();
			if (targetType == typeof(decimal))
			{
				return ToDecimal(null) == other.ToDecimal(null);
			}
			if (targetType == typeof(double))
			{
				return ToDouble(null) == other.ToDouble(null);
			}
			if (targetType == typeof(float))
			{
				return ToSingle(null) == other.ToSingle(null);
			}
			if (targetType == typeof(ulong))
			{
				return ToUInt64(null) == other.ToUInt64(null);
			}
			if (targetType == typeof(long))
			{
				return ToInt64(null) == other.ToInt64(null);
			}
			if (targetType == typeof(uint))
			{
				return ToUInt32(null) == other.ToUInt32(null);
			}
			if (targetType == typeof(int))
			{
				return ToInt32(null) == other.ToInt32(null);
			}
			if (targetType == typeof(ushort))
			{
				return ToUInt16(null) == other.ToUInt16(null);
			}
			if (targetType == typeof(short))
			{
				return ToInt16(null) == other.ToInt16(null);
			}
			if (targetType == typeof(byte))
			{
				return ToByte(null) == other.ToByte(null);
			}
			if (targetType == typeof(sbyte))
			{
				return ToSByte(null) == other.ToSByte(null);
			}

			return false;
			throw new InvalidCastException($"Cannot convert {other._value} ({other._value.GetType()}) or {_value} ({_value.GetType()}) to a numeric type");
		}

		/// <summary>
		/// Tries to cast the <see cref="IConvertible"/> object into any number type and outputs a <see cref="Number"/>
		/// </summary>
		/// <param name="convertable"></param>
		/// <param name="number"></param>
		/// <returns></returns>
		public static bool TryCast(IConvertible convertable, out Number? number)
		{
			switch (convertable)
			{
				case ulong noulong: number = new Number(noulong); return true;
				case long nolong: number = new Number(nolong); return true;
				case uint nouint: number = new Number(nouint); return true;
				case int noint: number = new Number(noint); return true;
				case ushort noushort: number = new Number(noushort); return true;
				case short noshort: number = new Number(noshort); return true;
				case byte nobyte: number = new Number(nobyte); return true;
				case sbyte nosbyte: number = new Number(nosbyte); return true;
				case decimal nodecimal: number = new Number(nodecimal); return true;
				case double nodouble: number = new Number(nodouble); return true;
				case float nofloat: number = new Number(nofloat); return true;
			}

			number = null;
			return false;
		}

		///  <summary>
		/// 		Tries to parse the input to any number, following roughly the rules of msbuild.
		/// 		Like:
		/// 		Has Suffix? (u,m,l,f,d)
		/// 		Has Prefix? (0x)
		/// 		Is int?
		/// 		Is long?
		/// 		Is Double?
		/// 		Is sbyte?
		/// 		Is ushort?
		/// 		Is decimal?
		///  </summary>
		///  <param name="input"></param>
		///  <param name="culture"></param>
		///  <param name="number"></param>
		///  <returns></returns>
		public static bool TryParse(string input, CultureInfo culture, out Number number)
		{
			if (input.Length == 0)
			{
				number = NaN;
				return false;
			}

			if (char.IsLetter(input[input.Length - 1]))
			{
				//according to MSDN folloring literals are allowed

				if (input.EndsWith("u", StringComparison.CurrentCultureIgnoreCase))
				{
					input = input.TrimEnd('u', 'U');
					//its an unsigned number
					//evaluate of which type it is
					if (uint.TryParse(input, NumberStyles.Integer, culture, out var uIntVal))
					{
						number = new Number(uIntVal);
						return true;
					}
					if (ushort.TryParse(input, NumberStyles.Integer, culture, out var ushortVal))
					{
						number = new Number(ushortVal);
						return true;
					}
					if (ulong.TryParse(input, NumberStyles.Integer, culture, out var uLongVal))
					{
						number = new Number(uLongVal);
						return true;
					}
					if (byte.TryParse(input, NumberStyles.Integer, culture, out var byteVal))
					{
						number = new Number(byteVal);
						return true;
					}

					number = default;
					return false;
				}

				if (input.EndsWith("m", StringComparison.CurrentCultureIgnoreCase))
				{
					input = input.TrimEnd('m', 'M');
					//its an unsigned number
					//evaluate of which type it is
					if (decimal.TryParse(input, NumberStyles.Number, culture, out var uIntVal))
					{
						number = new Number(uIntVal);
						return true;
					}

					number = default;
					return false;
				}

				if (input.EndsWith("l", StringComparison.OrdinalIgnoreCase))
				{
					//its an long
					if (input.EndsWith("ul", StringComparison.OrdinalIgnoreCase))
					{
						input = input.TrimEnd('u', 'U', 'l', 'L');
						//its unsigned
						if (ulong.TryParse(input, NumberStyles.Integer, culture, out var uLongVal))
						{
							number = new Number(uLongVal);
							return true;
						}

						number = default;
						return false;
					}
					input = input.TrimEnd('l', 'L');
					//its signed
					if (long.TryParse(input, NumberStyles.Integer, culture, out var explLongVal))
					{
						number = new Number(explLongVal);
						return true;
					}

					number = default;
					return false;
				}

				if (input.EndsWith("f", StringComparison.OrdinalIgnoreCase))
				{
					//its an float
					input = input.TrimEnd('f', 'F');
					if (float.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, culture, out var floatVal))
					{
						number = new Number(floatVal);
						return true;
					}

					number = default;
					return false;
				}

				if (input.EndsWith("d", StringComparison.OrdinalIgnoreCase))
				{
					//its an float
					input = input.TrimEnd('d', 'D');
					if (double.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, culture, out var doubleVal))
					{
						number = new Number(doubleVal);
						return true;
					}

					number = default;
					return false;
				}
			}

			if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
			{
				input = input.TrimStart('0', 'x', 'X');
				if (int.TryParse(input, NumberStyles.HexNumber, culture, out var hexIntVal))
				{
					number = new Number(hexIntVal);
					return true;
				}

				number = default;
				return false;
			}
			//we start with parsing an int as its the default for any number in msbuild
			if (int.TryParse(input, NumberStyles.Integer, culture, out var intVal))
			{
				number = new Number(intVal);
				return true;
			}
			//if its bigger then an int it is most likely an long
			if (long.TryParse(input, NumberStyles.Integer, culture, out var longVal))
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

			if (double.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, culture, out var impliDoubleVal))
			{
				number = new Number(impliDoubleVal);
				return true;
			}
			if (sbyte.TryParse(input, NumberStyles.Integer, culture, out var sByteVal))
			{
				number = new Number(sByteVal);
				return true;
			}
			if (ushort.TryParse(input, NumberStyles.Integer, culture, out var shortVal))
			{
				number = new Number(shortVal);
				return true;
			}
			if (decimal.TryParse(input, NumberStyles.Number, culture, out var decimalVal))
			{
				number = new Number(decimalVal);
				return true;
			}

			number = NaN;
			return false;
		}

		/// <summary>
		///		Renderes the current value of <see cref="Number"/>
		/// </summary>
		/// <returns></returns>
		public string AsParsableString()
		{
			var targetType = _value.GetType();
			if (targetType == typeof(decimal))
			{
				return _value + "M";
			}
			if (targetType == typeof(double))
			{
				return _value + "D";
			}
			if (targetType == typeof(float))
			{
				return _value + "F";
			}
			if (targetType == typeof(ulong))
			{
				return _value + "UL";
			}
			if (targetType == typeof(long))
			{
				return _value + "L";
			}
			if (targetType == typeof(uint))
			{
				return _value + "U";
			}
			if (targetType == typeof(int))
			{
				return _value.ToString(null);
			}
			if (targetType == typeof(ushort))
			{
				return _value.ToString(null) + "U";
			}
			if (targetType == typeof(short))
			{
				return _value.ToString(null);
			}
			if (targetType == typeof(byte))
			{
				return "0x" + _value;
			}
			if (targetType == typeof(sbyte))
			{
				return "0x" + _value + "u";
			}

			return null;
		}

		#endregion

		/// <inheritdoc />
		public int CompareTo(ulong other)
		{
			return CompareTo((object)other);
		}

		/// <inheritdoc />
		public int CompareTo(long other)
		{
			return CompareTo((object)other);
		}

		/// <inheritdoc />
		public int CompareTo(uint other)
		{
			return CompareTo((object)other);
		}

		/// <inheritdoc />
		public int CompareTo(int other)
		{
			return CompareTo((object)other);
		}

		/// <inheritdoc />
		public int CompareTo(ushort other)
		{
			return CompareTo((object)other);
		}

		/// <inheritdoc />
		public int CompareTo(short other)
		{
			return CompareTo((object)other);
		}

		/// <inheritdoc />
		public int CompareTo(byte other)
		{
			return CompareTo((object)other);
		}

		/// <inheritdoc />
		public int CompareTo(sbyte other)
		{
			return CompareTo((object)other);
		}

		/// <inheritdoc />
		public int CompareTo(decimal other)
		{
			return CompareTo((object)other);
		}

		/// <inheritdoc />
		public int CompareTo(double other)
		{
			return CompareTo((object)other);
		}

		/// <inheritdoc />
		public int CompareTo(float other)
		{
			return CompareTo((object)other);
		}

		/// <inheritdoc />
		public bool Equals(ulong other)
		{
			return Equals((object)other);
		}

		/// <inheritdoc />
		public bool Equals(long other)
		{
			return Equals((object)other);
		}

		/// <inheritdoc />
		public bool Equals(uint other)
		{
			return Equals((object)other);
		}

		/// <inheritdoc />
		public bool Equals(int other)
		{
			return Equals((object)other);
		}

		/// <inheritdoc />
		public bool Equals(ushort other)
		{
			return Equals((object)other);
		}

		/// <inheritdoc />
		public bool Equals(short other)
		{
			return Equals((object)other);
		}

		/// <inheritdoc />
		public bool Equals(byte other)
		{
			return Equals((object)other);
		}

		/// <inheritdoc />
		public bool Equals(sbyte other)
		{
			return Equals((object)other);
		}

		/// <inheritdoc />
		public bool Equals(decimal other)
		{
			return Equals((object)other);
		}

		/// <inheritdoc />
		public bool Equals(double other)
		{
			return Equals((object)other);
		}

		/// <inheritdoc />
		public bool Equals(float other)
		{
			return Equals((object)other);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			return (obj is Number other && Equals(other))
				   || ((IsIntegralNumber(obj) || IsFloatingPointNumber(obj)) &&
					   obj is IConvertible objCom && new Number(objCom).Same(this));
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return (_value != null ? _value.GetHashCode() : 0);
		}

		#region IConvertable

		/// <inheritdoc />
		public TypeCode GetTypeCode()
		{
			return _value.GetTypeCode();
		}

		/// <inheritdoc />
		public bool ToBoolean(IFormatProvider provider)
		{
			if (_value is bool realVal)
			{
				return realVal;
			}

			return _value.ToBoolean(provider);
		}

		/// <inheritdoc />
		public char ToChar(IFormatProvider provider)
		{
			if (_value is char realVal)
			{
				return realVal;
			}
			return _value.ToChar(provider);
		}

		/// <inheritdoc />
		public sbyte ToSByte(IFormatProvider provider)
		{
			if (_value is sbyte realVal)
			{
				return realVal;
			}
			return _value.ToSByte(provider);
		}

		/// <inheritdoc />
		public byte ToByte(IFormatProvider provider)
		{
			if (_value is byte realVal)
			{
				return realVal;
			}
			return _value.ToByte(provider);
		}

		/// <inheritdoc />
		public short ToInt16(IFormatProvider provider)
		{
			if (_value is short realVal)
			{
				return realVal;
			}
			return _value.ToInt16(provider);
		}

		/// <inheritdoc />
		public ushort ToUInt16(IFormatProvider provider)
		{
			if (_value is ushort realVal)
			{
				return realVal;
			}
			return _value.ToUInt16(provider);
		}

		/// <inheritdoc />
		public int ToInt32(IFormatProvider provider)
		{
			if (_value is int realVal)
			{
				return realVal;
			}
			return _value.ToInt32(provider);
		}

		/// <inheritdoc />
		public uint ToUInt32(IFormatProvider provider)
		{
			if (_value is uint realVal)
			{
				return realVal;
			}
			return _value.ToUInt32(provider);
		}

		/// <inheritdoc />
		public long ToInt64(IFormatProvider provider)
		{
			if (_value is long realVal)
			{
				return realVal;
			}
			return _value.ToInt64(provider);
		}

		/// <inheritdoc />
		public ulong ToUInt64(IFormatProvider provider)
		{
			if (_value is ulong realVal)
			{
				return realVal;
			}
			return _value.ToUInt64(provider);
		}

		/// <inheritdoc />
		public float ToSingle(IFormatProvider provider)
		{
			if (_value is float realVal)
			{
				return realVal;
			}
			return _value.ToSingle(provider);
		}

		/// <inheritdoc />
		public double ToDouble(IFormatProvider provider)
		{
			if (_value is double realVal)
			{
				return realVal;
			}
			return _value.ToDouble(provider);
		}

		/// <inheritdoc />
		public decimal ToDecimal(IFormatProvider provider)
		{
			if (_value is decimal realVal)
			{
				return realVal;
			}
			return _value.ToDecimal(provider);
		}

		/// <inheritdoc />
		public DateTime ToDateTime(IFormatProvider provider)
		{
			if (_value is DateTime realVal)
			{
				return realVal;
			}
			return _value.ToDateTime(provider);
		}

		/// <inheritdoc />
		public string ToString(IFormatProvider provider)
		{
			return _value.ToString(provider);
		}

		/// <inheritdoc />
		public object ToType(Type conversionType, IFormatProvider provider)
		{
			if (_value.GetType() == conversionType)
			{
				return _value;
			}

			return _value.ToType(conversionType, provider);
		}

		/// <inheritdoc />
		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (_value is IFormattable formattable)
			{
				return formattable.ToString(format, formatProvider);
			}

			return _value.ToString(formatProvider);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return _value?.ToString();
		}

		/// <inheritdoc />
		public int CompareTo(object obj)
		{
			if (!(_value is IComparable comparable))
			{
				throw new ArgumentException($"Cannot compare '{obj}' with '{_value}'");
			}

			return comparable.CompareTo(obj);
		}

		/// <inheritdoc />
		public int CompareTo(Number obj)
		{
			if (!(_value is IComparable comparable))
			{
				throw new ArgumentException($"Cannot compare '{obj}' with '{_value}'");
			}

			return comparable.CompareTo(obj._value);
		}

		#endregion

		#region Operator Overloading

#pragma warning disable CS1591
		public static Number operator +(Number a) => a;
		public static Number operator ++(Number a) => a.Add(1);
		public static Number operator -(Number a) => a * -1;
		public static Number operator --(Number a) => a.Subtract(1);
		public static Number operator <<(Number a, int b) => a.ShiftLeft(b);
		public static Number operator >>(Number a, int b) => a.ShiftRight(b);
		public static bool operator ==(Number a, Number b) => a.Equals(b);
		public static bool operator !=(Number a, Number b) => !a.Equals(b);
		public static bool operator <(Number a, Number b) => a.LessThen(b);
		public static bool operator >(Number a, Number b) => a.GreaterThen(b);
		public static bool operator <=(Number a, Number b) => a.Equals(b) || a.LessThen(b);
		public static bool operator >=(Number a, Number b) => a.Equals(b) || a.GreaterThen(b);

		public static Number operator +(Number a, Number b) => a.Add(b);
		public static Number operator -(Number a, Number b) => a.Subtract(b);
		public static Number operator *(Number a, Number b) => a.Multiply(b);
		public static Number operator /(Number a, Number b) => a.Divide(b);
		public static Number operator %(Number a, Number b) => a.Modulo(b);

		public static Number operator +(Number a, decimal b) => a.Add(b);
		public static Number operator -(Number a, decimal b) => a.Subtract(b);
		public static Number operator *(Number a, decimal b) => a.Multiply(b);
		public static Number operator /(Number a, decimal b) => a.Divide(b);
		public static Number operator %(Number a, decimal b) => a.Modulo(b);

		public static Number operator +(Number a, double b) => a.Add(b);
		public static Number operator -(Number a, double b) => a.Subtract(b);
		public static Number operator *(Number a, double b) => a.Multiply(b);
		public static Number operator /(Number a, double b) => a.Divide(b);
		public static Number operator %(Number a, double b) => a.Modulo(b);

		public static Number operator +(Number a, float b) => a.Add(b);
		public static Number operator -(Number a, float b) => a.Subtract(b);
		public static Number operator *(Number a, float b) => a.Multiply(b);
		public static Number operator /(Number a, float b) => a.Divide(b);
		public static Number operator %(Number a, float b) => a.Modulo(b);

		public static Number operator +(Number a, ulong b) => a.Add(b);
		public static Number operator -(Number a, ulong b) => a.Subtract(b);
		public static Number operator *(Number a, ulong b) => a.Multiply(b);
		public static Number operator /(Number a, ulong b) => a.Divide(b);
		public static Number operator %(Number a, ulong b) => a.Modulo(b);

		public static Number operator +(Number a, long b) => a.Add(b);
		public static Number operator -(Number a, long b) => a.Subtract(b);
		public static Number operator *(Number a, long b) => a.Multiply(b);
		public static Number operator /(Number a, long b) => a.Divide(b);
		public static Number operator %(Number a, long b) => a.Modulo(b);

		public static Number operator +(Number a, uint b) => a.Add(b);
		public static Number operator -(Number a, uint b) => a.Subtract(b);
		public static Number operator *(Number a, uint b) => a.Multiply(b);
		public static Number operator /(Number a, uint b) => a.Divide(b);
		public static Number operator %(Number a, uint b) => a.Modulo(b);

		public static Number operator +(Number a, int b) => a.Add(b);
		public static Number operator -(Number a, int b) => a.Subtract(b);
		public static Number operator *(Number a, int b) => a.Multiply(b);
		public static Number operator /(Number a, int b) => a.Divide(b);
		public static Number operator %(Number a, int b) => a.Modulo(b);

		public static Number operator +(Number a, ushort b) => a.Add(b);
		public static Number operator -(Number a, ushort b) => a.Subtract(b);
		public static Number operator *(Number a, ushort b) => a.Multiply(b);
		public static Number operator /(Number a, ushort b) => a.Divide(b);
		public static Number operator %(Number a, ushort b) => a.Modulo(b);

		public static Number operator +(Number a, short b) => a.Add(b);
		public static Number operator -(Number a, short b) => a.Subtract(b);
		public static Number operator *(Number a, short b) => a.Multiply(b);
		public static Number operator /(Number a, short b) => a.Divide(b);
		public static Number operator %(Number a, short b) => a.Modulo(b);

		public static Number operator +(Number a, byte b) => a.Add(b);
		public static Number operator -(Number a, byte b) => a.Subtract(b);
		public static Number operator *(Number a, byte b) => a.Multiply(b);
		public static Number operator /(Number a, byte b) => a.Divide(b);
		public static Number operator %(Number a, byte b) => a.Modulo(b);

		public static Number operator +(Number a, sbyte b) => a.Add(b);
		public static Number operator -(Number a, sbyte b) => a.Subtract(b);
		public static Number operator *(Number a, sbyte b) => a.Multiply(b);
		public static Number operator /(Number a, sbyte b) => a.Divide(b);
		public static Number operator %(Number a, sbyte b) => a.Modulo(b);

		public static implicit operator Number(decimal d) => new Number(d);
		public static implicit operator Number(double d) => new Number(d);
		public static implicit operator Number(float d) => new Number(d);
		public static implicit operator Number(ulong d) => new Number(d);
		public static implicit operator Number(long d) => new Number(d);
		public static implicit operator Number(uint d) => new Number(d);
		public static implicit operator Number(int d) => new Number(d);
		public static implicit operator Number(ushort d) => new Number(d);
		public static implicit operator Number(short d) => new Number(d);
		public static implicit operator Number(byte d) => new Number(d);
		public static implicit operator Number(sbyte d) => new Number(d);

		public static implicit operator decimal(Number d) => d.ToDecimal(null);
		public static implicit operator double(Number d) => d.ToDouble(null);
		public static implicit operator float(Number d) => d.ToSingle(null);
		public static implicit operator ulong(Number d) => d.ToUInt64(null);
		public static implicit operator long(Number d) => d.ToInt64(null);
		public static implicit operator uint(Number d) => d.ToUInt32(null);
		public static implicit operator int(Number d) => d.ToInt32(null);
		public static implicit operator ushort(Number d) => d.ToUInt16(null);
		public static implicit operator short(Number d) => d.ToInt16(null);
		public static implicit operator byte(Number d) => d.ToByte(null);
		public static implicit operator sbyte(Number d) => d.ToSByte(null);

#pragma warning restore CS1591

		#endregion
	}
}
