using System.Globalization;
using System.Threading.Tasks;

namespace Morestachio.Helper.Localization
{
	/// <summary>
	/// 
	/// </summary>
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