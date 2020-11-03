using System;

namespace Morestachio.Framework.Expression
{
	/// <summary>
	///     Defines all possible operators
	/// </summary>
	[Flags]
	public enum OperatorTypes
	{
#pragma warning disable 1591
		Add = 1 << 0,
		Substract = 1 << 1,
		Multiply = 1 << 2,
		Divide = 1 << 3,
		Pow = 1 << 4,
		Remainder = 1 << 5,
		ShiftLeft = 1 << 6,
		ShiftRight = 1 << 7,
		Equals = 1 << 8,
		UnEquals = 1 << 9,
		GreaterThen = 1 << 10,
		LessThen = 1 << 11,
		GreaterOrEquals = 1 << 12,
		LessOrEquals = 1 << 13,
		And = 1 << 14,
		Or = 1 << 15,
		Bigger = 1 << 16,
		Smaller = 1 << 17,
		NullCoalescing = 1 << 18
	}
#pragma warning restore 1591
}