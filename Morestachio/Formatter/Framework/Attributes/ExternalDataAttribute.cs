using System;

namespace Morestachio.Formatter.Framework.Attributes;

/// <summary>
///		Marks an parameter to be injected with an external value from <see cref="ServiceCollection"/>
/// </summary>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class ExternalDataAttribute : Attribute
{
}