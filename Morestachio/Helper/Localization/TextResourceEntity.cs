using System.Globalization;

namespace Morestachio.Helper.Localization
{
	/// <summary>
	///		Internal storage for an Translation
	/// </summary>
	public readonly struct TextResourceEntity
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="lang"></param>
		/// <param name="key"></param>
		/// <param name="text"></param>
		/// <param name="page"></param>
		public TextResourceEntity(CultureInfo lang, string key, object text, string page)
		{
			Lang = lang;
			Key = key;
			Text = text;
			Page = page;
		}

		/// <summary>
		///		The Culture that this translation is in
		/// </summary>
		public CultureInfo Lang { get; }

		/// <summary>
		///		The key to access this translation
		/// </summary>
		public string Key { get; }

		/// <summary>
		///		The Translation
		/// </summary>
		public object Text { get; }

		/// <summary>
		///		The optional part before an / in the key
		/// </summary>
		public string Page { get; }
	}
}