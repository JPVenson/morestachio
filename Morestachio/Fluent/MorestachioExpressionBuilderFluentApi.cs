using System.Collections.Generic;
using System.Linq;
using Morestachio.Framework;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Helper;

namespace Morestachio.Fluent
{
	public class MorestachioExpressionBuilderBaseRootApi : MorestachioExpressionBuilderBase
	{
		public IMorestachioExpression Parse(string expression)
		{
			return MorestachioExpression.ParseFrom(expression, TokenzierContext.FromText(expression), out _);
		}

		//public MorestachioExpressionBuilderBase BuildNumber(string number)
		//{
		//	ExpressionParts.Add(new ExpressionNumber(Number.Parse(number), new CharacterLocation(0, Column)));
		//	Column += number.Length;
		//	return this;
		//}
	}

	public class MorestachioExpressionBuilderBase
	{
		public IList<IMorestachioExpression> ExpressionParts { get; set; }
		public int Column { get; set; }

		public MorestachioExpressionBuilderBase()
		{
			ExpressionParts = new List<IMorestachioExpression>();
		}

		//public MorestachioExpressionBuilderBase Access(string property)
		//{
		//	var canAttach = ExpressionParts.LastOrDefault();
		//	if (canAttach != null && (canAttach is MorestachioExpression exp) && exp.FormatterName == null)
		//	{
		//		exp.PathParts.
		//	}
		//}
	}
}