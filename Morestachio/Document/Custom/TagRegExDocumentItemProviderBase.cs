using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Morestachio.Document.Contracts;
using Morestachio.Framework.Tokenizing;
using Morestachio.Parsing;

namespace Morestachio.Document.Custom;

/// <summary>
///		Can be used to create a single statement Tag
/// </summary>
public abstract class TagRegexDocumentItemProviderBase : CustomDocumentItemProvider
{
	private readonly Regex _tagRegex;

	/// <summary>
	///		
	/// </summary>
	protected TagRegexDocumentItemProviderBase(Regex tagRegex)
	{
		_tagRegex = tagRegex;
	}

	/// <summary>
	///		Will be called to produce an Document item that must be executed
	/// </summary>
	public abstract IDocumentItem CreateDocumentItem(string tagKeyword, string value, TokenPair token,
													ParserOptions options, IEnumerable<ITokenOption> tagCreationOptions);
		
	/// <inheritdoc />
	public override IEnumerable<TokenPair> Tokenize(TokenInfo token, ParserOptions options)
	{
		var tag = _tagRegex.Match(token.Token).Value;
		yield return new TokenPair(tag, token.Token, token.Location);
	}
		
	/// <inheritdoc />
	public override bool ShouldParse(TokenPair token, ParserOptions options, IEnumerable<ITokenOption> tokenOptions)
	{
		return _tagRegex.IsMatch(token.Value);
	}
		
	/// <inheritdoc />
	public override IDocumentItem Parse(TokenPair token, ParserOptions options, Stack<DocumentScope> buildStack,
										Func<int> getScope, IEnumerable<ITokenOption> tokenOptions)
	{
		var tagKeyword = _tagRegex.Match(token.Value).Value;
		var value = token.Value?.Trim('{', '}').Remove(0, tagKeyword.Length).Trim();
		return CreateDocumentItem(tagKeyword, value, token, options, tokenOptions);
	}
		
	/// <inheritdoc />
	public override bool ShouldTokenize(string token)
	{
		return _tagRegex.IsMatch(token);
	}
}