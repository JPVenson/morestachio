using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Morestachio.Framework.Expression
{
	/// <summary>
	///		An callable operator
	/// </summary>
	public class MorestachioOperator
	{
		static MorestachioOperator()
		{
			Operators = new ReadOnlyDictionary<string, MorestachioOperator>(new Dictionary<string, MorestachioOperator>()
			{
				{ "+" , new MorestachioOperator("+" )},
				{ "-" , new MorestachioOperator("-" )},
				{ "*" , new MorestachioOperator("*" )},
				{ "/" , new MorestachioOperator("/" )},
				{ "^" , new MorestachioOperator("^" )},
				{ "%" , new MorestachioOperator("%" )},
				{ "<<", new MorestachioOperator("<<")},
				{ ">>", new MorestachioOperator(">>")},
				{ "==", new MorestachioOperator("==")},
				{ "!=", new MorestachioOperator("!=")},
				{ "<" , new MorestachioOperator("<" )},
				{ ">" , new MorestachioOperator(">" )},
				{ "<=", new MorestachioOperator("<=")},
				{ ">=", new MorestachioOperator(">=")},
				{ "&&", new MorestachioOperator("&&")},
				{ "||", new MorestachioOperator("||")},
				{ "<?", new MorestachioOperator("<?")},
				{ ">?", new MorestachioOperator(">?")},
			});
			//CustomOperators = new Dictionary<string, MorestachioOperator>();
		}

		/// <summary>
		///		Creates a new Operator.
		/// </summary>
		/// <param name="operatorText"></param>
		/// <param name="acceptsTwoExpressions"></param>
		public MorestachioOperator(string operatorText, bool acceptsTwoExpressions = true)
		{
			OperatorText = operatorText;
			AcceptsTwoExpressions = acceptsTwoExpressions;
		}

		/// <summary>
		///		The string representation of the operator.
		/// </summary>
		public string OperatorText { get; private set; }

		/// <summary>
		///		[Experimental. false is not supported]
		/// </summary>
		public bool AcceptsTwoExpressions { get; private set; }

		/// <summary>
		///		Executes the operator
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
			var leftValue = (await left.GetValue(contextObject, scopeData));
			return await leftValue.Operator(OperatorText,
				(await (right?.GetValue(contextObject, scopeData) ?? Task.FromResult<ContextObject>(null))));
		}

		///// <summary>
		/////		A Dictionary of custom operators.
		///// </summary>
		//public static IDictionary<string, MorestachioOperator> CustomOperators { get; private set; }

		/// <summary>
		///		The default supported operators
		/// </summary>
		public static IDictionary<string, MorestachioOperator> Operators { get; private set; }

		/// <summary>
		///		Gets all operators
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<MorestachioOperator> Yield()
		{
			return Operators.Values;
		}
	}
}