using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Framework;

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
}