using System;
using System.Collections.Generic;
using Morestachio.Document.Contracts;
using Morestachio.Framework.Tokenizing;
using Morestachio.Parsing;

namespace Morestachio.Document.Custom
{
	/// <summary>
	///		Can be used to create a single statement Tag
	/// </summary>
	public abstract class TagDocumentItemProviderBase : CustomDocumentItemProvider
	{
		private readonly string _tag;

		/// <summary>
		///		
		/// </summary>
		/// <param name="tag">Should contain full tag like <code>#Anything</code> excluding the brackets and any parameter</param>
		protected TagDocumentItemProviderBase(string tag)
		{
			_tag = tag;
		}

		/// <summary>
		///		Will be called to produce an Document item that must be executed
		/// </summary>
		public abstract IDocumentItem CreateDocumentItem(string tag, string value, TokenPair token, ParserOptions options);
		
		/// <inheritdoc />
		public override IEnumerable<TokenPair> Tokenize(TokenInfo token, ParserOptions options)
		{
			yield return new TokenPair(_tag, token.Token, token.TokenizerContext.CurrentLocation);
		}
		
		/// <inheritdoc />
		public override bool ShouldParse(TokenPair token, ParserOptions options)
		{
			return token.Type.Equals(_tag.Trim());
		}
		
		/// <inheritdoc />
		public override IDocumentItem Parse(TokenPair token, ParserOptions options, Stack<DocumentScope> buildStack,
			Func<int> getScope)
		{
			return CreateDocumentItem(_tag, token.Value?.Trim('{', '}').Remove(0, _tag.Length).Trim(), token, options);
		}
		
		/// <inheritdoc />
		public override bool ShouldTokenize(string token)
		{
			return token.StartsWith(_tag, StringComparison.InvariantCultureIgnoreCase);
		}
	}
}