using System;
using System.Collections.Generic;
using Morestachio.Document.Contracts;
using Morestachio.Framework.Tokenizing;
using Morestachio.Parsing;

namespace Morestachio.Document.Custom;

/// <summary>
///		Can be used to create a single statement Tag
/// </summary>
public abstract class TagDocumentItemProviderBase : CustomDocumentItemProvider
{
	private readonly string _tagKeyword;

	/// <summary>
	///		
	/// </summary>
	/// <param name="tagKeyword">Should contain full tag like <code>#Anything</code> excluding the brackets and any parameter</param>
	protected TagDocumentItemProviderBase(string tagKeyword)
	{
		_tagKeyword = tagKeyword;
	}

	/// <summary>
	///		Will be called to produce an Document item that must be executed
	/// </summary>
	public abstract IDocumentItem CreateDocumentItem(string tagKeyword,
													string value,
													TokenPair token,
													ParserOptions options,
													IEnumerable<ITokenOption> tagCreationOptions);

	/// <inheritdoc />
	public override IEnumerable<TokenPair> Tokenize(TokenInfo token, ParserOptions options)
	{
		yield return new TokenPair(_tagKeyword, token.Token, token.Location);
	}

	/// <inheritdoc />
	public override bool ShouldParse(TokenPair token, ParserOptions options, IEnumerable<ITokenOption> tokenOptions)
	{
		return token.Type.Equals(_tagKeyword.Trim());
	}

	/// <inheritdoc />
	public override IDocumentItem Parse(TokenPair token,
										ParserOptions options,
										Stack<DocumentScope> buildStack,
										Func<int> getScope,
										IEnumerable<ITokenOption> tokenOptions)
	{
		return CreateDocumentItem(_tagKeyword, token.Value?.Trim('{', '}').Remove(0, _tagKeyword.Length).Trim(), token,
			options, tokenOptions);
	}

	/// <inheritdoc />
	public override bool ShouldTokenize(string token)
	{
		return token.StartsWith(_tagKeyword, StringComparison.OrdinalIgnoreCase);
	}
}