using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Framework.Expression.Parser;

internal readonly struct StringToken : IExpressionToken
{
	public StringToken(string value, char delimiter, TextRange location)
	{
		TokenType = ExpressionTokenType.String;
		Value = value;
		Delimiter = delimiter;
		Location = location;
	}

	public ExpressionTokenType TokenType { get; }
	public string Value { get; }
	public TextRange Location { get; }
	public char Delimiter { get; }
}