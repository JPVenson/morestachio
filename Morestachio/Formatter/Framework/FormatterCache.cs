namespace Morestachio.Formatter.Framework
{
	/// <summary>
	///		The Cached result to call a formatter
	/// </summary>
	public readonly struct FormatterCache
	{
		/// <summary>
		/// 
		/// </summary>
		public FormatterCache(MorestachioFormatterModel model, PrepareFormatterComposingResult testedTypes)
		{
			Model = model;
			TestedTypes = testedTypes;
		}

		/// <summary>
		///		The Formatter model this cache is for
		/// </summary>
		public MorestachioFormatterModel Model { get; }

		/// <summary>
		///		The cs function and arguments map
		/// </summary>
		public PrepareFormatterComposingResult TestedTypes { get; }
	}
}