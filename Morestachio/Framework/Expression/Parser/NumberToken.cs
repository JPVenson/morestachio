using Morestachio.Helper;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Framework.Expression.Parser;

internal readonly struct NumberToken : IExpressionToken
{
	public NumberToken(Number number, TextRange location)
	{
		TokenType = ExpressionTokenType.Number;
		Number = number;
		Location = location;
	}

	public ExpressionTokenType TokenType { get; }
	public Number Number { get; }
	public TextRange Location { get; }
}