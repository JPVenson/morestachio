namespace Morestachio.Framework.Expression.Parser
{
	internal readonly struct OperatorToken : IExpressionToken
	{
		public OperatorToken(OperatorTypes value, CharacterLocation location)
		{
			TokenType = ExpressionTokenType.Operator;
			Value = value;
			Location = location;
		}

		public ExpressionTokenType TokenType { get; }
		public OperatorTypes Value { get; }
		public CharacterLocation Location { get; }
	}
}