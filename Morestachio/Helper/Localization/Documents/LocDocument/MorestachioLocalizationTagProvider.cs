using System;
using System.Collections.Generic;
using System.Globalization;
using Morestachio.Document.Contracts;
using Morestachio.Document.Custom;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Parser;
using Morestachio.Framework.Tokenizing;

namespace Morestachio.Helper.Localization.Documents.LocDocument;

/// <summary>
///		Provides access to <see cref="MorestachioLocalizationDocumentItem"/>
/// </summary>
public class MorestachioLocalizationTagProvider : TagDocumentItemProviderBase
{
	/// <inheritdoc />
	public MorestachioLocalizationTagProvider() : base(OpenTag)
	{
	}

	/// <summary>
	/// The opening tag for a loc document item
	/// </summary>
	public const string OpenTag = "#LOC ";

	/// <inheritdoc />
	public override IEnumerable<TokenPair> Tokenize(TokenInfo token, ParserOptions options)
	{
		var locToken = token.Token.Remove(0, OpenTag.Length).Trim(Tokenizer.GetWhitespaceDelimiters());
		var locExpression
			= ExpressionParser.ParseExpression(locToken, token.TokenizerContext, token.Location.RangeStart);
		var tokenOptions = new List<ITokenOption>();

		locToken = locToken.Substring(locExpression.SourceBoundary.RangeEnd.Index)
			.Trim(Tokenizer.GetWhitespaceDelimiters());

		if (locToken.StartsWith("#CULTURE ", StringComparison.OrdinalIgnoreCase))
		{
			locToken = locToken.Substring("#CULTURE ".Length);
			tokenOptions.Add(new TokenOption("Culture", ExpressionParser
				.ParseExpression(locToken, token.TokenizerContext, token.Location.RangeEnd)
				.Expression));
		}

		yield return new TokenPair(OpenTag.Trim(),
			token.Location, locExpression.Expression, tokenOptions);
	}

	/// <inheritdoc />
	public override IDocumentItem CreateDocumentItem(string tagKeyword,
													string value,
													TokenPair token,
													ParserOptions options,
													IEnumerable<ITokenOption> tagCreationOptions)
	{
		return new MorestachioLocalizationDocumentItem(token.TokenRange,
			token.MorestachioExpression,
			token.FindOption<IMorestachioExpression>("Culture"),
			tagCreationOptions);
	}
}