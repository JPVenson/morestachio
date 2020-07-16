using System;
using Morestachio.Attributes;
using Morestachio.Formatter.Framework;

namespace Morestachio.Formatter.Predefined
{
	/// <summary>
	///		Contains the basic Formatting operations
	/// </summary>
	public static class ObjectStringFormatter
	{
		[MorestachioFormatter("ToString", null)]
		[MorestachioFormatter(null, null)]
		public static string Formattable(IFormattable source, string argument, [ExternalData]ParserOptions options)
		{
			return source.ToString(argument, options.CultureInfo);
		}

		[MorestachioFormatter("ToString", null)]
		[MorestachioFormatter(null, null)]
		public static string Formattable(IFormattable source)
		{
			return source.ToString();
		}
	}
}