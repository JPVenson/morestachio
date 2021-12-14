using Morestachio.Framework.Expression.Framework;

namespace Morestachio.TemplateContainers;

/// <summary>
///		Defines a Tokenizer Match
/// </summary>
public readonly struct TokenMatch
{
	/// <summary>
	///		Creates a new Match
	/// </summary>
	/// <param name="index"></param>
	/// <param name="value"></param>
	/// <param name="preText"></param>
	/// <param name="length"></param>
	/// <param name="contentToken"></param>
	public TokenMatch(int index, string value, string preText, int length, bool contentToken)
	{
		Index = index;
		Value = value;
		PreText = preText;
		ContentToken = contentToken;
		Length = length;
	}

	/// <summary>
	///		The index within the template where this token occurs
	/// </summary>
	public int Index { get; }
	/// <summary>
	///		The length of the Value. Should differ as value omits the <see cref="TokenzierContext.PrefixToken"/> and <see cref="TokenzierContext.SuffixToken"/>
	/// </summary>
	public int Length { get; }

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