namespace Morestachio.Framework.Expression.Framework;

/// <summary>
///     Defines a Match of Arguments for a Formatter
/// </summary>
internal class HeaderTokenMatch
{
	/// <summary>
	///     Initializes a new instance of the <see cref="HeaderTokenMatch" /> class.
	/// </summary>
	public HeaderTokenMatch()
	{
	}

	public string ArgumentName { get; set; }

	public IMorestachioExpression Value { get; set; }
	public CharacterLocation TokenLocation { get; set; }

	internal TokenState State { get; set; }
	internal int BracketsCounter { get; set; }
	public bool Evaluated { get; set; }

	public HeaderTokenMatch Parent { get; set; }

	///// <inheritdoc />
	//public override string ToString()
	//{
	//	if (TokenType == HeaderArgumentType.Expression)
	//	{
	//		if (Arguments.Any())
	//		{
	//			return Value + $"({Arguments.Select(f => f.ToString()).Aggregate((e, f) => e + "," + f)})";
	//		}

	//		return Value.ToString();
	//	}

	//	return $"\"{Value}\"";
	//}
}