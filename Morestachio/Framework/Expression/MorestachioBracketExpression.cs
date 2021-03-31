using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Morestachio.Document;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression.Visitors;
#if ValueTask
using ContextObjectPromise = System.Threading.Tasks.ValueTask<Morestachio.Framework.Context.ContextObject>;
#else
using ContextObjectPromise = System.Threading.Tasks.Task<Morestachio.Framework.Context.ContextObject>;
#endif

namespace Morestachio.Framework.Expression
{
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

		internal MorestachioBracketExpression(CharacterLocation location) : base(location)
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
				get { return _exp.ToString(); }
			}

			public CharacterLocation Location
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
}