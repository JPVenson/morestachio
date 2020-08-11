using System;
using Morestachio.Formatter.Framework;

namespace Morestachio.Attributes
{
	/// <summary>
	///		Marks an parameter to be injected with an external value from <see cref="MorestachioFormatterService.ServiceCollection"/>
	/// </summary>
	/// <seealso cref="System.Attribute" />
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
	public sealed class ExternalDataAttribute : Attribute
	{
	}
}