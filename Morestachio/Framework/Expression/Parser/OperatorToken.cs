using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Framework.Expression.Parser;

internal readonly struct LambdaExpressionToken : IExpressionToken
{
	public LambdaExpressionToken(TextRange location)
	{
		Location = location;
		TokenType = ExpressionTokenType.LambdaOperator;
	}

	public ExpressionTokenType TokenType { get; }
	public TextRange Location { get; }
}

internal readonly struct OperatorToken : IExpressionToken
{
	public OperatorToken(OperatorTypes value, TextRange location)
	{
		TokenType = ExpressionTokenType.Operator;
		Value = value;
		Location = location;
	}

	public ExpressionTokenType TokenType { get; }
	public OperatorTypes Value { get; }
	public TextRange Location { get; }
}