using System.Collections.Generic;
using Morestachio.Framework.Expression;

namespace Morestachio.Fluent
{
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