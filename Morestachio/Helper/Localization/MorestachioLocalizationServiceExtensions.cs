using System;
using Morestachio.Formatter.Framework;
using Morestachio.Helper.Localization.Documents.CustomCultureDocument;
using Morestachio.Helper.Localization.Documents.LocDocument;
using Morestachio.Helper.Localization.Documents.LocPDocument;

namespace Morestachio.Helper.Localization;

/// <summary>
///		Helper for Registering all necessary components for the Translation
/// </summary>
public static class MorestachioLocalizationServiceExtensions
{
	/// <summary>
	///		Registers everything for using the {{#loc "key"}}, {{Loc("key")}} and {{#LOCP "key"}} {{#LOCPARAM "argA"}} {{#loc "keyB"}} {{/LOCP}}
	/// </summary>
	/// <param name="builder"></param>
	/// <param name="getService"></param>
	/// <returns></returns>
	public static IParserOptionsBuilder WithLocalizationService(this IParserOptionsBuilder builder,
																Func<IMorestachioLocalizationService> getService)
	{
		var service = getService();

		return builder
				.AddCustomDocument(new MorestachioLocalizationTagProvider())
				.AddCustomDocument(new MorestachioCustomCultureLocalizationBlockProvider())
				.AddCustomDocument(new MorestachioLocalizationBlockProvider())
				.AddCustomDocument(new MorestachioLocalizationParamTagProvider())
				.WithService(service)
				.WithFormatters<IMorestachioLocalizationService>()
				.WithFormatters(typeof(LocalizationFormatter))
				.WithFormatters(service.GetType());
	}
}