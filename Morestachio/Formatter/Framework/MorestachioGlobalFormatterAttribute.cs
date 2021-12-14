using Morestachio.Formatter.Framework.Attributes;

namespace Morestachio.Formatter.Framework;

/// <summary>
///		Defines an Global Formatter that can be called without the need for specifing an source object
/// </summary>
public class MorestachioGlobalFormatterAttribute : MorestachioFormatterAttribute
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="name"></param>
	/// <param name="description"></param>
	public MorestachioGlobalFormatterAttribute(string name, string description) : base(name, description)
	{
		IsSourceObjectAware = false;
	}
}