using System;
using System.Collections.Generic;
using System.Text;
using Morestachio.TemplateContainers;

namespace Morestachio.Util
{
	internal static class StringExtensions
	{
		public static bool StartsWith(this string value, char check)
		{
			return value is { Length: > 1 } && value[0] == check;
		}

		public static bool StartsWithIgnoreCase(this string value, char check)
		{
			return value is { Length: > 1 } && OrdinalCharComparer.ComparerIgnoreCase.Equals(value[0], check);
		}
		public static bool IsEquals(this string value, char check)
		{
			return value.Length == 1 && value[0] == check;
		}

		public static bool IsEqualsIgnoreCase(this string value, char check)
		{
			return value.Length == 1 && OrdinalCharComparer.ComparerIgnoreCase.Equals(value[0], check);
		}
	}
}
