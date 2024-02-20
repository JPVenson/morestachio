using System.Diagnostics;
using Morestachio.Framework.Expression.Visitors;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Framework.Expression;

/// <summary>
///		Acts as a expression that only encases another expression within a bracket
/// </summary>
[DebuggerTypeProxy(typeof(ExpressionDebuggerDisplay))]
[Serializable]
public class MorestachioBracketExpression : MorestachioMultiPartExpressionList
{
	internal MorestachioBracketExpression()
	{
	}

	internal MorestachioBracketExpression(TextRange location) : base(location)
	{
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="info"></param>
	/// <param name="context"></param>
	protected MorestachioBracketExpression(SerializationInfo info, StreamingContext context) : base(info, context)
	{
	}

	private class ExpressionDebuggerDisplay
	{
		private readonly MorestachioBracketExpression _exp;

		public ExpressionDebuggerDisplay(MorestachioBracketExpression exp)
		{
			_exp = exp;
		}

		public string Expression
		{
			get { return _exp.AsStringExpression(); }
		}

		public string DbgView
		{
			get { return _exp.AsDebugExpression(); }
		}

		public TextRange Location
		{
			get { return _exp.Location; }
		}

		/// <inheritdoc />
		public override string ToString()
		{
			var visitor = new DebuggerViewExpressionVisitor();
			_exp.Accept(visitor);
			return visitor.StringBuilder.ToString();
		}
	}
}