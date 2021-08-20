namespace Morestachio.Framework.Expression.Parser
{
	internal readonly struct LambdaExpressionToken : IExpressionToken
	{
		public LambdaExpressionToken(CharacterLocation location)
		{
			Location = location;
			TokenType = ExpressionTokenType.LambdaOperator;
		}

		public ExpressionTokenType TokenType { get; }
		public CharacterLocation Location { get; }
	}

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