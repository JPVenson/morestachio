using System;
using Morestachio.Formatter.Framework;

namespace Morestachio.Attributes
{
	/// <summary>
	///		Marks an Parameter as the source object. That object is the source from where the formatter was called.
	/// </summary>
	/// <seealso cref="System.Attribute" />
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
	public sealed class SourceObjectAttribute : Attribute
	{
	}

	/// <summary>
	///		Marks an parameter to be injected with an external value from <see cref="MorestachioFormatterService.ServiceCollection"/>
	/// </summary>
	/// <seealso cref="System.Attribute" />
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
	public sealed class ExternalDataAttribute : Attribute
	{
	}
}
