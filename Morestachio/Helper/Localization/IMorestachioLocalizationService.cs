using System.Globalization;
using System.Threading.Tasks;
using Morestachio.Formatter.Framework;

namespace Morestachio.Helper.Localization
{
	/// <summary>
	/// 
	/// </summary>
	/// 
	/// <remarks>
	///		This service allows localization of text within your template by adding one or more <see cref="ITranslationResource"/>
	///		It is not available in the standard configuration and must be first enabled via <see cref="MorestachioLocalizationServiceExtensions.RegisterLocalizationService"/>
	/// </remarks>
	[ServiceName("Localization")]
	public interface IMorestachioLocalizationService
	{
		/// <summary>
		///		Gets the stored <see cref="TextResourceEntity"/> that matches the key and the culture (or <see cref="CultureInfo.CurrentUICulture"/> if null)
		/// </summary>
		/// <param name="key"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		Task<TextResourceEntity?> GetEntryOrLoad(string key, CultureInfo culture = null);

		/// <summary>
		///		Get the Translation for the culture (or <see cref="CultureInfo.CurrentUICulture"/> if null)
		/// </summary>
		/// <param name="key"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		object GetTranslationOrNull(string key, CultureInfo culture = null, params object[] arguments);
	}
}