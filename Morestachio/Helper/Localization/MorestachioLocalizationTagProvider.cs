using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;
using Morestachio.Document.Contracts;
using Morestachio.Document.Custom;
using Morestachio.Document.Items;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Tokenizing;


namespace Morestachio.Helper.Localization
{
	/// <summary>
	///		Provides access to <see cref="MorestachioLocalizationDocumentItem"/>
	/// </summary>
	public class MorestachioLocalizationTagProvider : TagDocumentItemProviderBase
	{
		/// <inheritdoc />
		public MorestachioLocalizationTagProvider() : base("#loc ")
		{
		}
		
		/// <inheritdoc />
		public override IEnumerable<TokenPair> Tokenize(TokenInfo token, ParserOptions options)
		{
			yield return new TokenPair("#loc", token.Token,
				ExpressionParser.ParseExpression(token.Token.Trim('{', '}').Remove(0, "#loc".Length).Trim(),
					token.TokenizerContext),
				token.TokenizerContext.CurrentLocation);
		}
		
		/// <inheritdoc />
		public override IDocumentItem CreateDocumentItem(string tag, string value, TokenPair token, ParserOptions options)
		{
			return new MorestachioLocalizationDocumentItem(token.MorestachioExpression);
		}
	}
}
