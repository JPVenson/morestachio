using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Morestachio.Formatter.Framework.Converter;

namespace Morestachio.Helper
{
	/// <summary>
	///		A Floating Number
	/// </summary>
	public struct FloatingNumber
	{
		public decimal Value { get; }

		/// <summary>
		///		Creates a new Floating number
		/// </summary>
		/// <param name="floatingNumber"></param>
		public FloatingNumber(decimal floatingNumber)
		{
			Value = floatingNumber;
		}

		/// <summary>
		///		Creates a new Floating number
		/// </summary>
		/// <param name="floatingNumber"></param>
		public FloatingNumber(double floatingNumber)
		{
			Value = (decimal) floatingNumber;
		}
	}

	/// <summary>
	///		Encapsulates a late bound number
	/// </summary>
	public struct Number
	{
		public object Value { get; }

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
		///		Adds an Fraction to the Number
		/// </summary>
		/// <param name="floatingPoint"></param>
		/// <returns></returns>
		public FloatingNumber WithFraction(Number floatingPoint)
		{
			var decimalFloating = floatingPoint.As<double>();

			var noFloating = (byte)(Math.Floor(Math.Log10(decimalFloating)) + 1);
			var pow = Math.Pow(10, noFloating);

			var floating = (decimalFloating / pow);
			if (Value is long lval)
			{
				if (lval < 0)
				{
					floating *= -1;
				}
				return new FloatingNumber(lval + floating);
			}
			if (Value is ulong ulval)
			{
				return new FloatingNumber(ulval + floating);
			}
			throw new InvalidOperationException();
		}

		public T As<T>() where T : IConvertible
		{
			var typeConverter = TypeDescriptor.GetConverter(typeof(T));
			return (T)typeConverter.ConvertTo(Value, typeof(T));
		}

		public static bool TryParse(string input, out Number number)
		{
			if (long.TryParse(input, out var integerNumber))
			{
				number = new Number(integerNumber);
				return true;
			}
			if (ulong.TryParse(input, out var uIntegerNumber))
			{
				number = new Number(uIntegerNumber);
				return true;
			}
			number = default;
			return false;
		}
	}
}
