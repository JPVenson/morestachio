using System;
using Morestachio.Formatter.Framework;
using Morestachio.Helper.Localization.Documents.CustomCultureDocument;
using Morestachio.Helper.Localization.Documents.LocDocument;
using Morestachio.Helper.Localization.Documents.LocPDocument;

namespace Morestachio.Helper.Localization
{
	/// <summary>
	///		Helper for Registering all necessary components for the Translation
	/// </summary>
	public static class MorestachioLocalizationServiceExtensions
	{
		/// <summary>
		///		Registers everything for using the {{#loc "key"}}, {{Loc("key")}} and {{#LOCP "key"}} {{#LOCPARAM "argA"}} {{#loc "keyB"}} {{/LOCP}}
		/// </summary>
		/// <param name="options"></param>
		/// <param name="getService"></param>
		/// <returns></returns>
		public static ParserOptions RegisterLocalizationService(this ParserOptions options,
			Func<IMorestachioLocalizationService> getService)
		{
			var service = getService();
			options.Formatters.Services.AddService(service);
			options.Formatters.AddFromType(typeof(IMorestachioLocalizationService));
			options.Formatters.AddFromType(service.GetType());
			options.Formatters.AddFromType(typeof(LocalizationFormatter));
			options.CustomDocumentItemProviders.Add(new MorestachioLocalizationTagProvider());
			options.CustomDocumentItemProviders.Add(new MorestachioCustomCultureLocalizationBlockProvider());
			options.CustomDocumentItemProviders.Add(new MorestachioLocalizationBlockProvider());
			options.CustomDocumentItemProviders.Add(new MorestachioLocalizationParamTagProvider());
			return options;
		}
	}
}