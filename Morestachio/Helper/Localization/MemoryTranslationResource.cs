using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
#if ValueTask
using BoolPromise = System.Threading.Tasks.ValueTask<bool>;
#else
using BoolPromise = System.Threading.Tasks.Task<bool>;
#endif

namespace Morestachio.Helper.Localization
{
	/// <summary>
	///		Provides translations from memory
	/// </summary>
	public class MemoryTranslationResource : ITranslationResource
	{
		/// <summary>
		/// 
		/// </summary>
		public MemoryTranslationResource()
		{
			Translations = new List<Translation>();
		}

		/// <summary>
		///		Adds an Translation to the memory collection
		/// </summary>
		/// <param name="key"></param>
		/// <param name="culture"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public MemoryTranslationResource Add(string key, CultureInfo culture, object value)
		{
			Translations.Add(new Translation(key, value, culture));
			return this;
		}

		private readonly struct Translation
		{
			public Translation(string key, object value, CultureInfo culture)
			{
				Key = key;
				Value = value;
				Culture = culture;
			}

			public string Key { get; }
			public object Value { get; }
			public CultureInfo Culture { get; }
		}

		IList<Translation> Translations { get; set; }
		
		/// <inheritdoc />
		public KeyValuePair<string, object>[] Get(CultureInfo culture)
		{
			return Translations.Where(e => Equals(e.Culture, culture))
				.Select(f => new KeyValuePair<string, object>(f.Key, f.Value))
				.ToArray();
		}
		
		/// <inheritdoc />
		public BoolPromise GetTranslation(string key, CultureInfo culture, out object translation)
		{
			var firstOrDefault = Translations.FirstOrDefault(e => e.Key == key && Equals(e.Culture, culture));
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