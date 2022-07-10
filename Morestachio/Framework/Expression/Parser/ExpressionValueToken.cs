using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Framework.Expression.Parser;

internal readonly struct ExpressionValueToken : IExpressionToken
{
	public ExpressionValueToken(ExpressionTokenType tokenType, string value, TextRange location)
	{
		TokenType = tokenType;
		Value = value;
		Location = location;
	}

	public ExpressionTokenType TokenType { get; }
	public string Value { get; }
	public TextRange Location { get; }
}