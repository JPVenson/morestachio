using System;
using System.Collections.Generic;
using Morestachio.Document.Contracts;
using Morestachio.Framework;

namespace Morestachio.Document.Custom
{
	/// <summary>
	///		The Standard Block that is enclosed with an opening tag <code>{{#Anything}}</code> and closed with an closing tag <code>{{/Anything}}</code>
	/// </summary>
	public abstract class BlockDocumentItemProviderBase : CustomDocumentItemProvider
	{
		private readonly string _tagOpen;
		private readonly string _tagClose;

		/// <summary>
		///		Creates a new Block
		/// </summary>
		/// <param name="tagOpen">Should contain full tag like <code>#Anything</code> excluding the brackets and any parameter</param>
		/// <param name="tagClose">Should contain full tag like <code>/Anything</code> excluding the brackets and any parameter</param>
		public BlockDocumentItemProviderBase(string tagOpen, string tagClose)
		{
			_tagOpen = tagOpen;
			_tagClose = tagClose;
		}

		/// <summary>
		///		Will be called to produce an Document item that must be executed
		/// </summary>
		public abstract IDocumentItem CreateDocumentItem(string tag, string value, TokenPair token, ParserOptions options);

		/// <inheritdoc />
		public override IEnumerable<TokenPair> Tokenize(TokenInfo token, ParserOptions options)
		{
			var trim = token.Token.Trim('{', '}');
			if (trim == _tagOpen)
			{
				yield return new TokenPair(_tagOpen, trim, token.TokenizerContext.CurrentLocation);
			}
			if (trim == _tagClose)
			{
				yield return new TokenPair(_tagClose, trim, token.TokenizerContext.CurrentLocation);
			}
		}

		/// <inheritdoc />
		public override bool ShouldParse(TokenPair token, ParserOptions options)
		{
			return token.Type.Equals(_tagOpen) || token.Type.Equals(_tagClose);
		}

		/// <inheritdoc />
		public override IDocumentItem Parse(TokenPair token, ParserOptions options, Stack<DocumentScope> buildStack,
			Func<int> getScope)
		{
			if (token.Value == _tagOpen)
			{
				var tagDocumentItem = CreateDocumentItem(_tagOpen, 
					token.Value?.Remove(0, _tagOpen.Length).Trim(),
					token, options);
				buildStack.Push(new DocumentScope(tagDocumentItem, getScope));
				return tagDocumentItem;
			}
			else if (token.Value == _tagClose)
			{
				buildStack.Pop();
			}
			return null;
		}

		/// <inheritdoc />
		public override bool ShouldTokenize(string token)
		{
			return token.StartsWith("{{" + _tagOpen, StringComparison.InvariantCultureIgnoreCase)
			       || token.StartsWith("{{" + _tagClose, StringComparison.InvariantCultureIgnoreCase);
		}
	}
}