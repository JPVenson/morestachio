using System.Collections.Generic;
#if ValueTask
using ObjectPromise = System.Threading.Tasks.ValueTask<object>;
#else
using ObjectPromise = System.Threading.Tasks.Task<object>;
#endif

namespace Morestachio.Framework.Expression
{
	/// <summary>
	///     An callable operator
	/// </summary>
	public class MorestachioOperator
	{
		static MorestachioOperator()
		{
			var operators = new OperatorList();
			operators.Add(BinaryOperator("+", OperatorTypes.Add));
			operators.Add(BinaryOperator("-", OperatorTypes.Substract));
			operators.Add(BinaryOperator("*", OperatorTypes.Multiply));
			operators.Add(BinaryOperator("/", OperatorTypes.Divide));
			operators.Add(BinaryOperator("^", OperatorTypes.Pow));
			operators.Add(BinaryOperator("%", OperatorTypes.Remainder));
			operators.Add(BinaryOperator("<<", OperatorTypes.ShiftLeft));
			operators.Add(BinaryOperator(">>", OperatorTypes.ShiftRight));
			operators.Add(BinaryOperator("==", OperatorTypes.Equals));
			operators.Add(BinaryOperator("!=", OperatorTypes.UnEquals));
			operators.Add(BinaryOperator("<", OperatorTypes.LessThen));
			operators.Add(BinaryOperator("<=", OperatorTypes.LessOrEquals));
			operators.Add(BinaryOperator(">", OperatorTypes.GreaterThen));
			operators.Add(BinaryOperator(">=", OperatorTypes.GreaterOrEquals));
			operators.Add(BinaryOperator("&&", OperatorTypes.And));
			operators.Add(BinaryOperator("||", OperatorTypes.Or));
			operators.Add(BinaryOperator("<?", OperatorTypes.Bigger));
			operators.Add(BinaryOperator(">?", OperatorTypes.Smaller));
			operators.Add(BinaryOperator("??", OperatorTypes.NullCoalescing));
			Operators = operators;
		}

		/// <summary>
		///     Creates a new Operator.
		/// </summary>
		private MorestachioOperator(string operatorText,
			OperatorTypes operatorType,
			bool acceptsTwoExpressions,
			bool isPrefixOperator)
		{
			OperatorText = operatorText;
			OperatorType = operatorType;
			AcceptsTwoExpressions = acceptsTwoExpressions;
			IsPrefixOperator = isPrefixOperator;
		}

		private static MorestachioOperator BinaryOperator(string operatorText, OperatorTypes type)
		{
			return new MorestachioOperator(operatorText, type, true, false);
		}

		private static MorestachioOperator UnaryOperator(string operatorText, OperatorTypes type, bool leftHandOperator)
		{
			return new MorestachioOperator(operatorText, type, false, leftHandOperator);
		}

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
		public bool AcceptsTwoExpressions { get; }

		/// <summary>
		/// 
		/// </summary>
		public bool IsPrefixOperator { get; set; }

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
}