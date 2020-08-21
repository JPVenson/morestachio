using System;
using Morestachio.Formatter.Framework;

namespace Morestachio.Helper.Localization
{
	/// <summary>
	///		Helper for Registering all necessary components for the Translation
	/// </summary>
	public static class MorestachioLocalizationServiceExtensions
	{
		/// <summary>
		///		Registers everything for using the {{#loc "key"}} and {{Loc("key")}}
		/// </summary>
		/// <param name="options"></param>
		/// <param name="getService"></param>
		/// <returns></returns>
		public static ParserOptions RegisterLocalizationService(this ParserOptions options,
			Func<IMorestachioLocalizationService> getService)
		{
			var service = getService();
			options.Formatters.AddService<IMorestachioLocalizationService>(service);
			options.Formatters.AddFromType(typeof(LocalizationFormatter));
			options.CustomDocumentItemProviders.Add(new MorestachioLocalizationTagProvider());
			options.CustomDocumentItemProviders.Add(new MorestachioCustomCultureLocalizationBlockProvider());
			return options;
		}
	}
}