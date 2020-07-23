using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

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
		Smaller = 1 << 17
	}
#pragma warning restore 1591

	/// <summary>
	///     An callable operator
	/// </summary>
	public class MorestachioOperator
	{
		static MorestachioOperator()
		{
			var operators = new Dictionary<OperatorTypes, MorestachioOperator>();
			operators.Add(OperatorTypes.Add, new MorestachioOperator("+", OperatorTypes.Add));
			operators.Add(OperatorTypes.Substract, new MorestachioOperator("-", OperatorTypes.Substract));
			operators.Add(OperatorTypes.Multiply, new MorestachioOperator("*", OperatorTypes.Multiply));
			operators.Add(OperatorTypes.Divide, new MorestachioOperator("/", OperatorTypes.Divide));
			operators.Add(OperatorTypes.Pow, new MorestachioOperator("^", OperatorTypes.Pow));
			operators.Add(OperatorTypes.Remainder, new MorestachioOperator("%", OperatorTypes.Remainder));
			operators.Add(OperatorTypes.ShiftLeft, new MorestachioOperator("<<", OperatorTypes.ShiftLeft));
			operators.Add(OperatorTypes.ShiftRight, new MorestachioOperator(">>", OperatorTypes.ShiftRight));
			operators.Add(OperatorTypes.Equals, new MorestachioOperator("==", OperatorTypes.Equals));
			operators.Add(OperatorTypes.UnEquals, new MorestachioOperator("!=", OperatorTypes.UnEquals));
			operators.Add(OperatorTypes.GreaterThen, new MorestachioOperator("<", OperatorTypes.GreaterThen));
			operators.Add(OperatorTypes.LessThen, new MorestachioOperator(">", OperatorTypes.LessThen));
			operators.Add(OperatorTypes.GreaterOrEquals, new MorestachioOperator("<=", OperatorTypes.GreaterOrEquals));
			operators.Add(OperatorTypes.LessOrEquals, new MorestachioOperator(">=", OperatorTypes.LessOrEquals));
			operators.Add(OperatorTypes.And, new MorestachioOperator("&&", OperatorTypes.And));
			operators.Add(OperatorTypes.Or, new MorestachioOperator("||", OperatorTypes.Or));
			operators.Add(OperatorTypes.Bigger, new MorestachioOperator("<?", OperatorTypes.Bigger));
			operators.Add(OperatorTypes.Smaller, new MorestachioOperator(">?", OperatorTypes.Smaller));
			Operators = new ReadOnlyDictionary<OperatorTypes, MorestachioOperator>(
				operators);
		}

		/// <summary>
		///     Creates a new Operator.
		/// </summary>
		public MorestachioOperator(string operatorText, OperatorTypes operatorType, bool acceptsTwoExpressions = true)
		{
			OperatorText = operatorText;
			OperatorType = operatorType;
			AcceptsTwoExpressions = acceptsTwoExpressions;
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

		///// <summary>
		/////		A Dictionary of custom operators.
		///// </summary>
		//public static IDictionary<string, MorestachioOperator> CustomOperators { get; private set; }

		/// <summary>
		///     The default supported operators
		/// </summary>
		public static IDictionary<OperatorTypes, MorestachioOperator> Operators { get; }

		/// <summary>
		///     Executes the operator
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <param name="contextObject"></param>
		/// <param name="scopeData"></param>
		/// <returns></returns>
		public virtual async Task<object> Execute(
			IMorestachioExpression left,
			IMorestachioExpression right,
			ContextObject contextObject,
			ScopeData scopeData)
		{
			var leftValue = await left.GetValue(contextObject, scopeData);
			return await leftValue.Operator(OperatorType,
				await (right?.GetValue(contextObject, scopeData) ?? Task.FromResult<ContextObject>(null)));
		}

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