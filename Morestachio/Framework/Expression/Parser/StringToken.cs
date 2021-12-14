namespace Morestachio.Framework.Expression.Parser;

internal readonly struct StringToken : IExpressionToken
{
	public StringToken(string value, char delimiter, CharacterLocation location)
	{
		TokenType = ExpressionTokenType.String;
		Value = value;
		Delimiter = delimiter;
		Location = location;
	}

	public ExpressionTokenType TokenType { get; }
	public string Value { get; }
	public CharacterLocation Location { get; }
	public char Delimiter { get; }
}