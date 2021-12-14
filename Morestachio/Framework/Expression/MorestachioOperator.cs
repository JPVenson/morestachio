
namespace Morestachio.Framework.Expression;

/// <summary>
///     An callable operator
/// </summary>
public class MorestachioOperator
{
	static MorestachioOperator()
	{
		var operators = new OperatorList();
		//If you add any new operator or remove one you MUST!
		//update the Tokenizer.IsOperationString and Tokenizer.IsOperationChar method!
		operators.Add(BinaryOperator("+" , OperatorTypes.Add));
		operators.Add(BinaryOperator("-" , OperatorTypes.Substract));
		operators.Add(BinaryOperator("*" , OperatorTypes.Multiply));
		operators.Add(BinaryOperator("/" , OperatorTypes.Divide));
		operators.Add(BinaryOperator("^" , OperatorTypes.Pow));
		operators.Add(BinaryOperator("%" , OperatorTypes.Remainder));
		operators.Add(BinaryOperator("<<", OperatorTypes.ShiftLeft));
		operators.Add(BinaryOperator(">>", OperatorTypes.ShiftRight));
		operators.Add(BinaryOperator("==", OperatorTypes.Equals));
		operators.Add(BinaryOperator("!=", OperatorTypes.UnEquals));
		operators.Add(BinaryOperator("<" , OperatorTypes.LessThen));
		operators.Add(BinaryOperator("<=", OperatorTypes.LessOrEquals));
		operators.Add(BinaryOperator(">" , OperatorTypes.GreaterThen));
		operators.Add(BinaryOperator(">=", OperatorTypes.GreaterOrEquals));
		operators.Add(BinaryOperator("&&", OperatorTypes.And));
		operators.Add(BinaryOperator("||", OperatorTypes.Or));
		operators.Add(BinaryOperator("<?", OperatorTypes.Bigger));
		operators.Add(BinaryOperator(">?", OperatorTypes.Smaller));
		operators.Add(BinaryOperator("??", OperatorTypes.NullCoalescing));
		operators.Add(UnaryOperator("!", OperatorTypes.Invert, OperatorPlacement.Left));
		Operators = operators;
	}

	/// <summary>
	///     Creates a new Operator.
	/// </summary>
	private MorestachioOperator(string operatorText,
								OperatorTypes operatorType,
								bool isBinaryOperator,
								OperatorPlacement placement)
	{
		OperatorText = operatorText;
		OperatorType = operatorType;
		IsBinaryOperator = isBinaryOperator;
		Placement = placement;
	}

	private static MorestachioOperator BinaryOperator(string operatorText, OperatorTypes type, OperatorPlacement placement = OperatorPlacement.Right)
	{
		return new MorestachioOperator(operatorText, type, true, placement);
	}

	private static MorestachioOperator UnaryOperator(string operatorText, OperatorTypes type, OperatorPlacement placement = OperatorPlacement.Right)
	{
		return new MorestachioOperator(operatorText, type, false, placement);
	}

	/// <summary>
	///		Defines where the operator is placed
	/// </summary>
	public OperatorPlacement Placement { get; }

	/// <summary>
	///		The Enum operator type
	/// </summary>
	public OperatorTypes OperatorType { get; }

	/// <summary>
	///     The string representation of the operator.
	/// </summary>
	public string OperatorText { get; }

	/// <summary>
	///     [Experimental. false is not supported]
	/// </summary>
	public bool IsBinaryOperator { get; }

	/// <summary>
	///     The default supported operators
	/// </summary>
	public static IReadOnlyDictionary<OperatorTypes, MorestachioOperator> Operators { get; }
		
	/// <summary>
	///     Gets all operators
	/// </summary>
	/// <returns></returns>
	public static IEnumerable<MorestachioOperator> Yield()
	{
		return Operators.Values;
	}
}

/// <summary>
///		Defines where an operator is placed in relation to ether the single or first operand
/// </summary>
public enum OperatorPlacement
{
	/// <summary>
	///		The operand is placed left to the expression
	/// </summary>
	Left,
	/// <summary>
	///		The operand is placed right to the expression
	/// </summary>
	Right
}