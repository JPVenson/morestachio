using System;
using System.Collections.Generic;
using System.Linq;

namespace Morestachio.Framework.Expression.Framework;

/// <summary>
/// 
/// </summary>
public static class IsolationOptionsExtensions
{
	/// <summary>
	/// 
	/// </summary>
	public static bool HasFlagFast(this IsolationOptions value, IsolationOptions flag)
	{
		return (value & flag) != 0;
	}
	/// <summary>
	/// 
	/// </summary>
	public static IEnumerable<T> GetFlags<T>(this T en) where T : struct, Enum
	{
		return Enum.GetValues(typeof(T)).Cast<T>().Where(member => en.HasFlag(member));
	}
}