using System;
using System.Collections.Generic;
using System.Linq;

namespace Morestachio.Framework.Expression.Framework
{
	public static class IsolationOptionsExtensions
	{
		public static bool HasFlagFast(this IsolationOptions value, IsolationOptions flag)
		{
			return (value & flag) != 0;
		}

		public static IEnumerable<T> GetFlags<T>(this T en) where T : struct, Enum
		{
			return Enum.GetValues(typeof(T)).Cast<T>().Where(member => en.HasFlag(member));
		}
	}
}