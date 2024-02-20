using System;
using Morestachio.Formatter.Framework;
using Morestachio.Formatter.Framework.Attributes;

namespace Morestachio.Helper.Localization;

/// <summary>
///		Contains the Localization Formatter to use with the <see cref="IMorestachioLocalizationService"/>
/// </summary>
[MorestachioExtensionSetup("Must be Setup using the IMorestachioLocalizationService service")]
public static class LocalizationFormatter
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="key"></param>
	/// <param name="localizationService"></param>
	/// <param name="parserOptions"></param>
	/// <param name="arguments"></param>
	/// <returns></returns>
	[MorestachioGlobalFormatter("Loc", "Translates the given key in the current Culture")]
	[MorestachioFormatter("Loc", "Translates the given key in the current Culture")]
	public static object Translate(string key,
									[ExternalData] IMorestachioLocalizationService localizationService,
									[ExternalData] ParserOptions parserOptions,
									[RestParameter] params object[] arguments)
	{
		return localizationService.GetTranslationOrNull(key, parserOptions.CultureInfo, arguments);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="count"></param>
	/// <param name="singular"></param>
	/// <param name="plural"></param>
	/// <param name="includeCount"></param>
	/// <returns></returns>
	[MorestachioGlobalFormatter("Inflect",
		"Returns either the singular or plural inflection of a word based on the given count")]
	public static string Inflect(Number count,
								string singular,
								string plural,
								bool includeCount)
	{
		var word = (count > 1 || count == 0) ? plural : singular;

		if (includeCount)
		{
			return count + " " + word;
		}

		return word;
	}
}