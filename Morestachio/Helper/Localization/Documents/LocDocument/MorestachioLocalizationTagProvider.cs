using System.Collections.Generic;
using Morestachio.Document.Contracts;
using Morestachio.Document.Custom;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Tokenizing;

namespace Morestachio.Helper.Localization.Documents.LocDocument
{
	/// <summary>
	///		Provides access to <see cref="MorestachioLocalizationDocumentItem"/>
	/// </summary>
	public class MorestachioLocalizationTagProvider : TagDocumentItemProviderBase
	{
		/// <inheritdoc />
		public MorestachioLocalizationTagProvider() : base(OpenTag)
		{
		}
		public const string OpenTag = "#LOC ";
		
		/// <inheritdoc />
		public override IEnumerable<TokenPair> Tokenize(TokenInfo token, ParserOptions options)
		{
			yield return new TokenPair(OpenTag.Trim(), token.Token,
				token.TokenizerContext.CurrentLocation, ExpressionParser.ParseExpression(token.Token.Remove(0, OpenTag.Length).Trim(),
					token.TokenizerContext));
		}
		
		/// <inheritdoc />
		public override IDocumentItem CreateDocumentItem(string tag, string value, TokenPair token, ParserOptions options)
		{
			return new MorestachioLocalizationDocumentItem(token.MorestachioExpression);
		}
	}
}
