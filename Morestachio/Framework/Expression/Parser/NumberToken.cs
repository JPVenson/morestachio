using Morestachio.Helper;

namespace Morestachio.Framework.Expression.Parser
{
	internal readonly struct NumberToken : IExpressionToken
	{
		public NumberToken(Number number, CharacterLocation location)
		{
			TokenType = ExpressionTokenType.Number;
			Number = number;
			Location = location;
		}

		public ExpressionTokenType TokenType { get; }
		public Number Number { get; }
		public CharacterLocation Location { get; }
	}
}