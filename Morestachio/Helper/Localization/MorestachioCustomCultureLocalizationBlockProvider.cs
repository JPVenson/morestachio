using System.Collections.Generic;
using Morestachio.Document.Contracts;
using Morestachio.Document.Custom;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Tokenizing;

namespace Morestachio.Helper.Localization
{
	/// <summary>
	///		Provides access to the <see cref="MorestachioCustomCultureLocalizationDocumentItem"/>
	/// </summary>
	public class MorestachioCustomCultureLocalizationBlockProvider : BlockDocumentItemProviderBase
	{
		/// <inheritdoc />
		public MorestachioCustomCultureLocalizationBlockProvider() : base("#LocCulture ", "/LocCulture")
		{
		}

		public override IEnumerable<TokenPair> Tokenize(TokenInfo token, ParserOptions options)
		{
			var trim = token.Token.Trim('{', '}');
			if (trim.StartsWith(TagOpen))
			{
				yield return new TokenPair(TagOpen.Trim(), 
					ExpressionParser.ParseExpression(trim.Remove(0, "#LocCulture".Length).Trim(), token.TokenizerContext), 
					token.TokenizerContext.CurrentLocation);
			}
			if (trim == TagClose)
			{
				yield return new TokenPair(TagClose, trim, token.TokenizerContext.CurrentLocation);
			}
		}

		/// <inheritdoc />
		public override IDocumentItem CreateDocumentItem(string tag, string value, TokenPair token, ParserOptions options)
		{
			return new MorestachioCustomCultureLocalizationDocumentItem(token.MorestachioExpression);
		}
	}
}