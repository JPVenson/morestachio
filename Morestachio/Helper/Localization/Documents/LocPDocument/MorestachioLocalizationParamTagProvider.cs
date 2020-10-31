using System.Collections.Generic;
using Morestachio.Document.Contracts;
using Morestachio.Document.Custom;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Tokenizing;

namespace Morestachio.Helper.Localization.Documents.LocPDocument
{
	/// <summary>
	///		Provides access to <see cref="MorestachioLocalizationParameterDocumentItem"/>
	/// </summary>
	public class MorestachioLocalizationParamTagProvider : TagDocumentItemProviderBase
	{
		/// <inheritdoc />
		public MorestachioLocalizationParamTagProvider() : base(OpenTag)
		{
		}

		public const string OpenTag = "#LOCPARAM ";
		
		/// <inheritdoc />
		public override IEnumerable<TokenPair> Tokenize(TokenInfo token, ParserOptions options)
		{
			yield return new TokenPair(OpenTag.Trim(), token.Token, ExpressionParser.ParseExpression(token.Token.Remove(0, OpenTag.Length).Trim(),
				token.TokenizerContext), token.TokenizerContext.CurrentLocation);
		}
		
		/// <inheritdoc />
		public override IDocumentItem CreateDocumentItem(string tag, string value, TokenPair token, ParserOptions options)
		{
			return new MorestachioLocalizationParameterDocumentItem(token.TokenLocation, token.MorestachioExpression);
		}
	}
}