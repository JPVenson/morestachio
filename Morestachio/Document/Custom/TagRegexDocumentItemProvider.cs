using System.Collections.Generic;
using System.Text.RegularExpressions;
using Morestachio.Document.Contracts;
using Morestachio.Framework.Tokenizing;

namespace Morestachio.Document.Custom;

/// <summary>
///		Can be used to create a single statement Tag
/// </summary>
public class TagRegexDocumentItemProvider : TagRegexDocumentItemProviderBase
{
	private readonly TagDocumentProviderFunction _action;

	/// <summary>
	///		
	/// </summary>
	/// <param name="tag">Should contain full tag like <code>#Anything</code> excluding the brackets and any parameter</param>
	/// <param name="action"></param>
	public TagRegexDocumentItemProvider(Regex tag, TagDocumentProviderFunction action) : base(tag)
	{
		_action = action;
	}

	/// <inheritdoc />
	public override IDocumentItem CreateDocumentItem(string tagKeyword, string value, TokenPair token,
													ParserOptions options, IEnumerable<ITokenOption> tagCreationOptions)
	{
		return new TagDocumentItemProvider.TagDocumentItem(token.TokenLocation, _action, tagKeyword, value, tagCreationOptions);
	}
}