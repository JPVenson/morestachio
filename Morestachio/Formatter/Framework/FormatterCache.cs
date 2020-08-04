namespace Morestachio.Formatter.Framework
{
	public readonly struct FormatterCache
	{
		public FormatterCache(MorestachioFormatterModel model, PrepareFormatterComposingResult testedTypes)
		{
			Model = model;
			TestedTypes = testedTypes;
		}

		public MorestachioFormatterModel Model { get; }
		public PrepareFormatterComposingResult TestedTypes { get; }
	}
}