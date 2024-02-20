using System.Collections.Generic;
using System.Globalization;
#if ValueTask
using BoolPromise = System.Threading.Tasks.ValueTask<bool>;

#else
using BoolPromise = System.Threading.Tasks.Task<bool>;
#endif
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
		BoolPromise GetTranslation(string key, CultureInfo culture, out object translation);
	}
}