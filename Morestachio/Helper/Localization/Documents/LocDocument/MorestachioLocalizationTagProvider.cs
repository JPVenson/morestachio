using System.Collections.Generic;
using System.Globalization;
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
			var locToken = token.Token.Remove(0, OpenTag.Length).Trim(Tokenizer.GetWhitespaceDelimiters());
			var pre = token.TokenizerContext.Character;
			var locExpression = ExpressionParser.ParseExpression(locToken, token.TokenizerContext);
			var tokenOptions = new List<ITokenOption>();

			locToken = locToken.Substring(token.TokenizerContext.Character - pre).Trim(Tokenizer.GetWhitespaceDelimiters());
			if (locToken.StartsWith("#CULTURE ", true, CultureInfo.InvariantCulture))
			{
				locToken = locToken.Substring("#CULTURE ".Length);
				tokenOptions.Add(new TokenOption("Culture", ExpressionParser.ParseExpression(locToken, token.TokenizerContext)));
			}

			yield return new TokenPair(OpenTag.Trim(), 
				token.TokenizerContext.CurrentLocation, locExpression, tokenOptions);
		}

		/// <inheritdoc />
		public override IDocumentItem CreateDocumentItem(string tagKeyword, string value, TokenPair token,
			ParserOptions options, IEnumerable<ITokenOption> tagCreationOptions)
		{
			return new MorestachioLocalizationDocumentItem(token.TokenLocation, 
				token.MorestachioExpression, 
				token.FindOption<IMorestachioExpression>("Culture"),
				tagCreationOptions);
		}
	}
}
