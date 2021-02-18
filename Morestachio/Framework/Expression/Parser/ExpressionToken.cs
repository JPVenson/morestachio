using Morestachio.Framework.Expression.Framework;

namespace Morestachio.Framework.Expression.Parser
{
	internal readonly struct ExpressionToken : IExpressionToken
	{
		public ExpressionToken(PathTokenizer value, CharacterLocation location)
		{
			TokenType = ExpressionTokenType.Path;
			Value = value;
			Location = location;
		}

		public ExpressionTokenType TokenType { get; }
		public PathTokenizer Value { get; }
		public CharacterLocation Location { get; }
	}
}