namespace Morestachio.Formatter.Framework;

/// <summary>
///		The Cached result to call a formatter
/// </summary>
public class FormatterCache
{
	/// <summary>
	/// 
	/// </summary>
	public FormatterCache(MorestachioFormatterModel model, PrepareFormatterComposingResult testedTypes)
	{
		Model = model;
		TestedTypes = testedTypes;
		ValueBuffer = new object[TestedTypes.Arguments.Count];
	}

	internal readonly object[] ValueBuffer;

	/// <summary>
	///		The Formatter model this cache is for
	/// </summary>
	public MorestachioFormatterModel Model { get; }

	/// <summary>
	///		The cs function and arguments map
	/// </summary>
	public PrepareFormatterComposingResult TestedTypes { get; }
}