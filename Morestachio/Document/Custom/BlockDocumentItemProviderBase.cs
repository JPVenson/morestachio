using System;
using System.Collections.Generic;
using Morestachio.Document.Contracts;
using Morestachio.Framework.Tokenizing;
using Morestachio.Parsing;

namespace Morestachio.Document.Custom
{
	/// <summary>
	///		The Standard Block that is enclosed with an opening tag <code>{{#Anything}}</code> and closed with an closing tag <code>{{/Anything}}</code>
	/// </summary>
	public abstract class BlockDocumentItemProviderBase : CustomDocumentItemProvider
	{
		protected readonly string TagOpen;
		protected readonly string TagClose;

		/// <summary>
		///		Creates a new Block
		/// </summary>
		/// <param name="tagOpen">Should contain full tag like <code>#Anything</code> excluding the brackets and any parameter</param>
		/// <param name="tagClose">Should contain full tag like <code>/Anything</code> excluding the brackets and any parameter</param>
		public BlockDocumentItemProviderBase(string tagOpen, string tagClose)
		{
			TagOpen = tagOpen;
			TagClose = tagClose;
		}

		/// <summary>
		///		Will be called to produce an Document item that must be executed
		/// </summary>
		public abstract IDocumentItem CreateDocumentItem(string tag, string value, TokenPair token, ParserOptions options);

		/// <inheritdoc />
		public override IEnumerable<TokenPair> Tokenize(TokenInfo token, ParserOptions options)
		{
			var trim = token.Token.Trim('{', '}');
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
		public override bool ShouldParse(TokenPair token, ParserOptions options)
		{
			return token.Type.Equals(TagOpen.Trim()) || token.Type.Equals(TagClose);
		}

		/// <inheritdoc />
		public override IDocumentItem Parse(TokenPair token, ParserOptions options, Stack<DocumentScope> buildStack,
			Func<int> getScope)
		{
			if (Equals(token.Type, TagOpen.Trim()))
			{
				var tagDocumentItem = CreateDocumentItem(TagOpen, 
					token.Value?.Remove(0, TagOpen.Length).Trim(),
					token, options);
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
			return token.StartsWith("{{" + TagOpen, StringComparison.InvariantCultureIgnoreCase)
			       || token.StartsWith("{{" + TagClose, StringComparison.InvariantCultureIgnoreCase);
		}
	}
}