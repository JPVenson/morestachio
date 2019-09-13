using Morestachio.Attributes;

namespace Morestachio.Formatter.Framework
{
	/// <summary>
	///     Delegate for mapping formatter function of the Morestachio framework to the params argument
	/// </summary>
	/// <param name="originalObject">The original object.</param>
	/// <param name="name">The name.</param>
	/// <param name="arguments">The arguments.</param>
	/// <returns></returns>
	public delegate object MorstachioFormatter([SourceObject] object originalObject,
		[FormatterArgumentName("Name")] string name,
		[RestParameter] params object[] arguments);
}