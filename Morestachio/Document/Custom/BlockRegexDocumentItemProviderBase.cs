using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Morestachio.Document.Contracts;
using Morestachio.Framework.Tokenizing;
using Morestachio.Parsing;

namespace Morestachio.Document.Custom;

/// <summary>
///		The Standard Block that is enclosed with an opening tag <code>{{#Anything}}</code> and closed with an closing tag <code>{{/Anything}}</code>
/// </summary>
public abstract class BlockRegexDocumentItemProviderBase : CustomDocumentItemProvider
{
	/// <summary>
	///		Defines the opening tag
	/// </summary>
	protected readonly Regex TagOpen;


	/// <summary>
	///		Defines the closing tag
	/// </summary>
	protected readonly Regex TagClose;

	/// <summary>
	///		Creates a new Block
	/// </summary>
	/// <param name="tagOpen">Should contain full tag like <code>#Anything</code> excluding the brackets and any parameter</param>
	/// <param name="tagClose">Should contain full tag like <code>/Anything</code> excluding the brackets and any parameter</param>
	protected BlockRegexDocumentItemProviderBase(Regex tagOpen, Regex tagClose)
	{
		TagOpen = tagOpen;
		TagClose = tagClose;
	}

	/// <summary>
	///		Will be called to produce an Document item that must be executed
	/// </summary>
	public abstract IDocumentItem CreateDocumentItem(string tag,
													string value,
													TokenPair token,
													ParserOptions options,
													IEnumerable<ITokenOption> tagCreationOptions);

	/// <inheritdoc />
	public override IEnumerable<TokenPair> Tokenize(TokenInfo token, ParserOptions options)
	{
		var trim = token.Token;

		if (TagOpen.IsMatch(trim))
		{
			var match = TagOpen.Match(trim);
			yield return new TokenPair(match.Value, trim, token.Location);
		}

		if (TagClose.IsMatch(trim))
		{
			var match = TagClose.Match(trim);
			yield return new TokenPair(match.Value, trim, token.Location);
		}
	}

	/// <inheritdoc />
	public override bool ShouldParse(TokenPair token,
									ParserOptions options,
									IEnumerable<ITokenOption> tagCreationOptions)
	{
		if (!(token.Type is string blockType))
		{
			return false;
		}

		return TagOpen.IsMatch(blockType) || TagClose.IsMatch(blockType);
	}

	/// <inheritdoc />
	public override IDocumentItem Parse(TokenPair token,
										ParserOptions options,
										Stack<DocumentScope> buildStack,
										Func<int> getScope,
										IEnumerable<ITokenOption> tagCreationOptions)
	{
		if (TagOpen.IsMatch((string)token.Type))
		{
			var tagDocumentItem = CreateDocumentItem((string)token.Type,
				token.Value?.Remove(0, ((string)token.Type).Length).Trim(),
				token, options, tagCreationOptions);
			buildStack.Push(new DocumentScope(tagDocumentItem, getScope));
			return tagDocumentItem;
		}

		if (TagClose.IsMatch((string)token.Type))
		{
			buildStack.Pop();
		}

		return null;
	}

	/// <inheritdoc />
	public override bool ShouldTokenize(string token)
	{
		return TagOpen.IsMatch(token) || TagClose.IsMatch(token);
	}
}