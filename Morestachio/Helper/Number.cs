using System;
using System.Globalization;
using System.Numerics;
using Morestachio.Formatter.Framework;

namespace Morestachio.Helper
{
	/// <summary>
	///		Encapsulates a late bound number
	/// </summary>
	public readonly struct Number : IComparable, IComparable<Number>, IConvertible, IFormattable, IEquatable<Number>
	{
		/// <summary>
		///		Contains the numeric value
		/// </summary>
		public IConvertible Value
		{
			get { return _value; }
		}

		internal Number(IConvertible fullNumber)
		{
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
		
		private static bool IsIntegral(Number number)
		{
			return !(number._value is decimal) && !(number._value is double) && !(number._value is float);
		}

		/// <summary>
		///		Contains the list of all .net Integral Types
		/// </summary>
		public static readonly Type[] CsFrameworkIntegralTypes = new[]
		{
			typeof(ulong),
			typeof(long),

			typeof(uint),
			typeof(int),

			typeof(ushort),
			typeof(short),

			typeof(byte),
			typeof(sbyte)
		};

		/// <summary>
		///		Contains the list of all .net Floating Point numbers and the Decimal type
		/// </summary>
		public static readonly Type[] CsFrameworkFloatingPointNumberTypes = new[]
		{
			typeof(decimal),
			typeof(double),
			typeof(float)
		};

		private readonly IConvertible _value;

		private static Type GetOperationTargetType(Number numberLeft, Number numberRight)
		{
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

			//if (!IsIntegral(numberLeft) || !IsIntegral(numberRight))
			//{
			//	throw new InvalidOperationException("Cannot determinate the numbers type");
			//}
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

		[MorestachioFormatter("Add", "Adds two numbers")]
		public static Number Add(Number left, Number right)
		{
			return left.Add(right);
		}

		[MorestachioFormatter("Subtract", "Subtracts two numbers")]
		public static Number Subtract(Number left, Number right)
		{
			return left.Subtract(right);
		}

		[MorestachioFormatter("Multiply", "Multiplies two numbers")]
		public static Number Multiply(Number left, Number right)
		{
			return left.Multiply(right);
		}

		[MorestachioFormatter("Divide", "Divides two numbers")]
		public static Number Divide(Number left, Number right)
		{
			return left.Divide(right);
		}

		[MorestachioFormatter("Modulo", "Modulo two numbers")]
		public static Number Modulo(Number left, Number right)
		{
			return left.Divide(right);
		}

		[MorestachioFormatter("ShiftLeft", "Shift two numbers")]
		public static Number ShiftLeft(Number left, Number right)
		{
			return left.ShiftLeft(right);
		}

		[MorestachioFormatter("ShiftRight", "Shift two numbers")]
		public static Number ShiftRight(Number left, Number right)
		{
			return left.ShiftRight(right);
		}

		[MorestachioFormatter("BiggerAs", "Checks if the source number is bigger as the other number")]
		public static bool BiggerAs(Number left, Number right)
		{
			return left.BiggerAs(right);
		}

		[MorestachioFormatter("SmallerAs", "Checks if the source number is smaller as the other number")]
		public static bool SmallerAs(Number left, Number right)
		{
			return left.SmallerAs(right);
		}

		[MorestachioFormatter("Equals", "Checks if the two numbers are equal to each other")]
		public static bool Equals(Number left, Number right)
		{
			return left.Equals(right);
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
		public static Number Max(Number left, Number right)
		{
			return left.Max(right);
		}

		[MorestachioFormatter("Min", "Returns the smaller of the two numbers")]
		public static Number Min(Number left, Number right)
		{
			return left.Max(right);
		}

		[MorestachioFormatter("Pow", "Gets this number power the times of the other number")]
		public static Number Pow(Number left, Number right)
		{
			return left.Pow(right);
		}

		#endregion

		#region Number Operations

		/// <summary>
		///		Gets the Absolute value
		/// </summary>
		/// <returns></returns>
		public Number Max(Number other)
		{
			return this > other ? this : other;
		}

		/// <summary>
		///		Gets the Absolute value
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
			return new Number(Math.Pow(ToDouble(null), other.ToDouble(null)));
		}

		/// <summary>
		///		Gets the Absolute value
		/// </summary>
		/// <returns></returns>
		public Number Log(Number other)
		{
			return new Number(Math.Log(ToDouble(null), other.ToDouble(null)));
		}

		/// <summary>
		///		Gets the Absolute value
		/// </summary>
		/// <returns></returns>
		public Number Log()
		{
			return Log(new Number(Math.E));
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
		///		Adds the two numbers together
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public Number Round(Number other)
		{
			if (_value is decimal)
			{
				return Math.Round(_value.ToDecimal(null), other.ToInt32(null));
			}
			return Math.Round(_value.ToDouble(null), other.ToInt32(null));
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
		public bool BiggerAs(Number other)
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
		public bool SmallerAs(Number other)
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
			if (other._value.GetType() != _value.GetType())
			{
				return false;
			}

			return Same(other);
		}

		/// <summary>
		///		Checks if both numbers are the same type and also the same value
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Same(Number other)
		{
			if (other._value == _value)
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
			if (targetType == typeof(ushort))
			{
				return ToInt16(null) == other.ToInt16(null);
			}
			if (targetType == typeof(ushort))
			{
				return ToByte(null) == other.ToByte(null);
			}
			if (targetType == typeof(sbyte))
			{
				return ToSByte(null) == other.ToSByte(null);
			}
			throw new InvalidCastException($"Cannot convert {other._value} ({other._value.GetType()}) or {_value} ({_value.GetType()}) to a numeric type");
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

			if (input.EndsWith("l", StringComparison.InvariantCultureIgnoreCase))
			{
				//its an long
				if (input.EndsWith("ul", StringComparison.InvariantCultureIgnoreCase))
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

			if (input.EndsWith("f", StringComparison.InvariantCultureIgnoreCase))
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

			if (input.EndsWith("d", StringComparison.InvariantCultureIgnoreCase))
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

			if (input.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
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

			number = default;
			return false;
		}

		#endregion

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			return obj is Number other && Equals(other);
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

		public static Number operator +(Number a) => a;
		public static Number operator ++(Number a) => a.Add(1);
		public static Number operator -(Number a) => a * -1;
		public static Number operator --(Number a) => a.Subtract(1);
		public static Number operator <<(Number a, int b) => a.ShiftLeft(b);
		public static Number operator >>(Number a, int b) => a.ShiftRight(b);
		public static bool operator ==(Number a, Number b) => a.Equals(b);
		public static bool operator !=(Number a, Number b) => !a.Equals(b);
		public static bool operator <(Number a, Number b) => a.SmallerAs(b);
		public static bool operator >(Number a, Number b) => a.BiggerAs(b);
		public static bool operator <=(Number a, Number b) => a.Equals(b) || a.SmallerAs(b);
		public static bool operator >=(Number a, Number b) => a.Equals(b) || a.BiggerAs(b);

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

		#endregion
	}
}
