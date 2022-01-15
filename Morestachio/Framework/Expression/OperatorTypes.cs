using System;

namespace Morestachio.Framework.Expression;

/// <summary>
///     Defines all possible operators
/// </summary>
[Flags]
public enum OperatorTypes
{
	/// <summary>
	///		Mapped to the
	/// <code>val + val</code>
	///		Operator
	/// </summary>
	Add = 1 << 0,
	/// <summary>
	///		Mapped to the
	/// <code>val - val</code>
	///		Operator
	/// </summary>
	Substract = 1 << 1,
	/// <summary>
	///		Mapped to the
	/// <code>val * val</code>
	///		Operator
	/// </summary>
	Multiply = 1 << 2,
	/// <summary>
	///		Mapped to the
	/// <code>val / val</code>
	///		Operator
	/// </summary>
	Divide = 1 << 3,
	/// <summary>
	///		Mapped to the
	/// <code>val ^ val</code>
	///		Operator
	/// </summary>
	Pow = 1 << 4,
	/// <summary>
	///		Mapped to the
	/// <code>val % val</code>
	///		Operator
	/// </summary>
	Remainder = 1 << 5,
	/// <summary>
	///		Mapped to the
	/// <code>val &lt;&lt; val</code>
	///		Operator
	/// </summary>
	ShiftLeft = 1 << 6,
		
	/// <summary>
	///		Mapped to the
	/// <code>val &gt;&gt; val</code>
	///		Operator
	/// </summary>
	ShiftRight = 1 << 7,
		
	/// <summary>
	///		Mapped to the
	/// <code>val == val</code>
	///		Operator
	/// </summary>
	Equals = 1 << 8,
		
	/// <summary>
	///		Mapped to the
	/// <code>val != val</code>
	///		Operator
	/// </summary>
	UnEquals = 1 << 9,
		
	/// <summary>
	///		Mapped to the
	/// <code>val &gt; val</code>
	///		Operator
	/// </summary>
	GreaterThen = 1 << 10,
		
	/// <summary>
	///		Mapped to the
	/// <code>val &lt; val</code>
	///		Operator
	/// </summary>
	LessThen = 1 << 11,
		
	/// <summary>
	///		Mapped to the
	/// <code>val &gt;= val</code>
	///		Operator
	/// </summary>
	GreaterOrEquals = 1 << 12,
		
	/// <summary>
	///		Mapped to the
	/// <code>val &lt;= val</code>
	///		Operator
	/// </summary>
	LessOrEquals = 1 << 13,
		
	/// <summary>
	///		Mapped to the
	/// <code>val &amp;&amp; val</code>
	///		Operator
	/// </summary>
	And = 1 << 14,

	/// <summary>
	///		Mapped to the
	/// <code>val || val</code>
	///		Operator
	/// </summary>
	Or = 1 << 15,

	/// <summary>
	///		Mapped to the
	/// <code>val &lt;? val</code>
	///		Operator
	/// </summary>
	/// <remarks>Gets the greater of two numbers</remarks>
	Bigger = 1 << 16,

	/// <summary>
	///		Mapped to the
	/// <code>val &gt;? val</code>
	///		Operator
	/// </summary>
	/// <remarks>Gets the lesser of two numbers</remarks>
	Smaller = 1 << 17,

	/// <summary>
	///		Mapped to the
	/// <code>val ?? val</code>
	///		Operator
	/// </summary>
	/// <remarks>Gets the left value if not null otherwise the right one</remarks>
	NullCoalescing = 1 << 18,

	/// <summary>
	///		Mapped to the
	/// <code>!val</code>
	///		Operator
	/// </summary>
	/// <remarks>Inverts an boolean value</remarks>
	Invert = 1 << 19
}