using System.Collections.Generic;
using System.Text.RegularExpressions;
using Morestachio.Document.Contracts;
using Morestachio.Framework.Tokenizing;

namespace Morestachio.Document.Custom;

/// <summary>
///		The Standard Block that is enclosed with an opening tag <code>{{#Anything}}</code> and closed with an closing tag <code>{{/Anything}}</code>
/// </summary>
public class BlockRegexDocumentItemProvider : BlockRegexDocumentItemProviderBase
{
	private readonly BlockDocumentProviderFunction _action;

	/// <inheritdoc />
	public BlockRegexDocumentItemProvider(Regex tagOpen, Regex tagClose, BlockDocumentProviderFunction action) : base(tagOpen, tagClose)
	{
		_action = action;
	}
		
	/// <inheritdoc />
	public override IDocumentItem CreateDocumentItem(string tag, string value, TokenPair token, ParserOptions options,
													IEnumerable<ITokenOption> tagCreationOptions)
	{
		return new BlockDocumentItemProvider.BlockDocumentItem(token.TokenLocation, _action, value,
			tagCreationOptions);
	}
}