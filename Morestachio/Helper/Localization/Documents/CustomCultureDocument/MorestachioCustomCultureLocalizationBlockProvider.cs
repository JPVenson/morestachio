using System;
using System.Collections.Generic;
using System.Globalization;
using Morestachio.Document.Contracts;
using Morestachio.Document.Custom;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Parser;
using Morestachio.Framework.Tokenizing;

namespace Morestachio.Helper.Localization.Documents.CustomCultureDocument;

/// <summary>
///		Provides access to the <see cref="MorestachioCustomCultureLocalizationDocumentItem"/>
/// </summary>
public class MorestachioCustomCultureLocalizationBlockProvider : BlockDocumentItemProviderBase
{
	/// <inheritdoc />
	public MorestachioCustomCultureLocalizationBlockProvider() : base(OpenTag, CloseTag)
	{
	}

	/// <summary>
	///		The opening tag of <see cref="MorestachioCustomCultureLocalizationDocumentItem"/>
	/// </summary>
	public const string OpenTag = "#LocCulture ";


	/// <summary>
	///		The closing tag of <see cref="MorestachioCustomCultureLocalizationDocumentItem"/>
	/// </summary>
	public const string CloseTag = "/LocCulture";

	/// <inheritdoc />
	public override IEnumerable<TokenPair> Tokenize(TokenInfo token, ParserOptions options)
	{
		var trim = token.Token;

		if (trim.StartsWith(TagOpen, StringComparison.OrdinalIgnoreCase))
		{
			yield return new TokenPair(TagOpen.Trim(), token.Location,
				ExpressionParser.ParseExpression(trim.Remove(0, OpenTag.Length).Trim(), token.TokenizerContext)
					.Expression);
		}

		if (string.Equals(trim, TagClose, StringComparison.OrdinalIgnoreCase))
		{
			yield return new TokenPair(TagClose, trim, token.Location);
		}
	}

	/// <inheritdoc />
	public override IBlockDocumentItem CreateDocumentItem(string tag,
														string value,
														TokenPair token,
														ParserOptions options,
														IEnumerable<ITokenOption> tagCreationOptions)
	{
		return new MorestachioCustomCultureLocalizationDocumentItem(token.TokenRange,
			token.MorestachioExpression,
			tagCreationOptions);
	}
}