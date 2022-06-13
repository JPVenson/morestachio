using Morestachio.Framework.Expression.Framework;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.TemplateContainers;

/// <summary>
///		Defines a Tokenizer Match
/// </summary>
public readonly struct TokenMatch
{
	/// <summary>
	///		Creates a new Match
	/// </summary>
	public TokenMatch(TextRange range, string value, string preText, bool contentToken)
	{
		Range = range;
		Value = value;
		PreText = preText;
		ContentToken = contentToken;
	}

	/// <summary>
	///		The index within the template where this token occurs
	/// </summary>
	public TextRange Range { get; }

	/// <summary>
	///		The Tokens value excluding <see cref="TokenzierContext.PrefixToken"/> and <see cref="TokenzierContext.SuffixToken"/>
	/// </summary>
	public string Value { get; }

	/// <summary>
	///		If present, any preciding text
	/// </summary>
	public string PreText { get; }

	/// <summary>
	///		<value>true</value> if this is a dedicated content token
	/// </summary>
	public bool ContentToken { get; }
}