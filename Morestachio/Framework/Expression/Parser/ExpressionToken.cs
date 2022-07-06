using Morestachio.Framework.Expression.Framework;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Framework.Expression.Parser;

internal readonly struct ExpressionToken : IExpressionToken
{
	public ExpressionToken(PathTokenizer value, in TextRange location)
	{
		TokenType = ExpressionTokenType.Path;
		Value = value;
		Location = location;
	}

	public ExpressionTokenType TokenType { get; }
	public PathTokenizer Value { get; }
	public TextRange Location { get; }
}