using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;

#if ValueTask
using BoolPromise = System.Threading.Tasks.ValueTask<bool>;

#else
using BoolPromise = System.Threading.Tasks.Task<bool>;
#endif

namespace Morestachio.Helper.Localization
{
	/// <summary>
	///		Class for loading translations from an <see cref="ResourceManager"/>
	/// </summary>
	public class ResourceManagerTranslationResource : ITranslationResource
	{
		private readonly ResourceManager _resourceManager;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="resourceManager"></param>
		public ResourceManagerTranslationResource(ResourceManager resourceManager)
		{
			_resourceManager = resourceManager;
		}

		/// <inheritdoc />
		public KeyValuePair<string, object>[] Get(CultureInfo culture)
		{
			return _resourceManager.GetResourceSet(culture, false, true)
				.OfType<DictionaryEntry>()
				.Select(f => new KeyValuePair<string, object>(f.Key.ToString(), f.Value))
				.ToArray();
		}

		/// <inheritdoc />
		public BoolPromise GetTranslation(string key, CultureInfo culture, out object translation)
		{
			var firstOrDefault = _resourceManager.GetResourceSet(culture, false, false)
				.OfType<DictionaryEntry>()
				.FirstOrDefault(e => e.Key.Equals(key));
			translation = firstOrDefault
				.Value;

			if (Equals(firstOrDefault.Key, key))
			{
				return true.ToPromise();
			}

			return false.ToPromise();
		}
	}
}