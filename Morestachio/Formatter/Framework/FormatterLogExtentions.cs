using System;

namespace Morestachio.Formatter.Framework
{
	internal static class FormatterLogExtentions
	{
		public static void Write(this IFormatterMatcher matcher, Func<string> log)
		{
			if (matcher is FormatterMatcher defaultMatcher)
			{
				defaultMatcher.Log(log);
			}
		}
	}
}