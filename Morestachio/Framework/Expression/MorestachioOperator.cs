using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Morestachio.Formatter.Framework;
#if ValueTask
using ObjectPromise = System.Threading.Tasks.ValueTask<object>;
#else
using ObjectPromise = System.Threading.Tasks.Task<object>;
#endif

namespace Morestachio.Framework.Expression
{
	internal class OperatorList : IReadOnlyDictionary<OperatorTypes, MorestachioOperator>
	{
		public OperatorList()
		{
			Operators = new Dictionary<OperatorTypes, MorestachioOperator>();
		}

		private IDictionary<OperatorTypes, MorestachioOperator> Operators { get; }

		public void Add(MorestachioOperator mOperator)
		{
			Operators[mOperator.OperatorType] = mOperator;
		}

		public IEnumerator<KeyValuePair<OperatorTypes, MorestachioOperator>> GetEnumerator()
		{
			return Operators.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)Operators).GetEnumerator();
		}

		public int Count
		{
			get
			{
				return Operators.Count;
			}
		}

		public bool ContainsKey(OperatorTypes key)
		{
			return Operators.ContainsKey(key);
		}

		public bool TryGetValue(OperatorTypes key, out MorestachioOperator value)
		{
			return Operators.TryGetValue(key, out value);
		}

		public MorestachioOperator this[OperatorTypes key]
		{
			get { return Operators[key]; }
		}

		public IEnumerable<OperatorTypes> Keys
		{
			get
			{
				return Operators.Keys;
			}
		}

		public IEnumerable<MorestachioOperator> Values
		{
			get
			{
				return Operators.Values;
			}
		}
	}
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
			operators.Add(BinaryOperator("<", OperatorTypes.GreaterThen));
			operators.Add(BinaryOperator(">", OperatorTypes.LessThen));
			operators.Add(BinaryOperator("<=", OperatorTypes.GreaterOrEquals));
			operators.Add(BinaryOperator(">=", OperatorTypes.LessOrEquals));
			operators.Add(BinaryOperator("&&", OperatorTypes.And));
			operators.Add(BinaryOperator("||", OperatorTypes.Or));
			operators.Add(BinaryOperator("<?", OperatorTypes.Bigger));
			operators.Add(BinaryOperator(">?", OperatorTypes.Smaller));
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