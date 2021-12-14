using System;
using System.Collections.Generic;
using Morestachio.Document.Contracts;
using Morestachio.Framework.Tokenizing;
using Morestachio.Parsing;

namespace Morestachio.Document.Custom;

/// <summary>
///		The Standard Block that is enclosed with an opening tag <code>{{#Anything}}</code> and closed with an closing tag <code>{{/Anything}}</code>
/// </summary>
public abstract class BlockDocumentItemProviderBase : CustomDocumentItemProvider
{
	/// <summary>
	///		Defines the opening tag
	/// </summary>
	protected readonly string TagOpen;

		
	/// <summary>
	///		Defines the closing tag
	/// </summary>
	protected readonly string TagClose;

	/// <summary>
	///		Creates a new Block
	/// </summary>
	/// <param name="tagOpen">Should contain full tag like <code>#Anything</code> excluding the brackets and any parameter</param>
	/// <param name="tagClose">Should contain full tag like <code>/Anything</code> excluding the brackets and any parameter</param>
	protected BlockDocumentItemProviderBase(string tagOpen, string tagClose)
	{
		TagOpen = tagOpen;
		TagClose = tagClose;
	}

	/// <summary>
	///		Will be called to produce an Document item that must be executed
	/// </summary>
	public abstract IDocumentItem CreateDocumentItem(string tag, string value, TokenPair token,
													ParserOptions options, IEnumerable<ITokenOption> tagCreationOptions);

	/// <inheritdoc />
	public override IEnumerable<TokenPair> Tokenize(TokenInfo token, ParserOptions options)
	{
		var trim = token.Token;
		if (trim == TagOpen)
		{
			yield return new TokenPair(TagOpen, trim, token.TokenizerContext.CurrentLocation);
		}
		if (trim == TagClose)
		{
			yield return new TokenPair(TagClose, trim, token.TokenizerContext.CurrentLocation);
		}
	}

	/// <inheritdoc />
	public override bool ShouldParse(TokenPair token, ParserOptions options, IEnumerable<ITokenOption> tagCreationOptions)
	{
		return token.Type.Equals(TagOpen.Trim()) || token.Type.Equals(TagClose);
	}

	/// <inheritdoc />
	public override IDocumentItem Parse(TokenPair token, ParserOptions options, Stack<DocumentScope> buildStack,
										Func<int> getScope, IEnumerable<ITokenOption> tagCreationOptions)
	{
		if (Equals(token.Type, TagOpen.Trim()))
		{
			var tagDocumentItem = CreateDocumentItem(TagOpen, 
				token.Value?.Remove(0, TagOpen.Length).Trim(),
				token, options, tagCreationOptions);
			buildStack.Push(new DocumentScope(tagDocumentItem, getScope));
			return tagDocumentItem;
		}
		else if (Equals(token.Type, TagClose))
		{
			buildStack.Pop();
		}
		return null;
	}

	/// <inheritdoc />
	public override bool ShouldTokenize(string token)
	{
		return token.StartsWith(TagOpen, StringComparison.OrdinalIgnoreCase)
			|| token.StartsWith(TagClose, StringComparison.OrdinalIgnoreCase);
	}
}