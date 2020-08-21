using System.Collections.Generic;
using System.Globalization;

namespace Morestachio.Helper.Localization
{
	/// <summary>
	///		Defines an resource where Translations can be loaded from
	/// </summary>
	public interface ITranslationResource
	{
		/// <summary>
		///		Gets all Translations from an culture
		/// </summary>
		/// <param name="culture"></param>
		/// <returns></returns>
		KeyValuePair<string, object>[] Get(CultureInfo culture);

		/// <summary>
		///		Gets the specific translation for that key
		/// </summary>
		/// <param name="key"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		object GetTranslation(string key, CultureInfo culture);
	}
}