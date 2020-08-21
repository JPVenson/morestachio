using Morestachio.Formatter.Framework;
using Morestachio.Formatter.Framework.Attributes;

namespace Morestachio.Helper.Localization
{
	/// <summary>
	///		Contains the Localization Formatter to use with the <see cref="IMorestachioLocalizationService"/>
	/// </summary>
	public static class LocalizationFormatter
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="LocalizationService"></param>
		/// <param name="parserOptions"></param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		[MorestachioGlobalFormatter("Loc", "Translates the given key in the current Culture")]
		[MorestachioFormatter("Loc", "Translates the given key in the current Culture")]
		public static object Translate(string key, 
			[ExternalData] IMorestachioLocalizationService LocalizationService,
			[ExternalData] ParserOptions parserOptions,
			[RestParameter]params object[] arguments)
		{
			return LocalizationService.GetTranslationOrNull(key, parserOptions.CultureInfo, arguments);
		}
	}
}