using System;
using System.Collections.Generic;
using System.Text;
using Morestachio.Formatter.Framework;
using Morestachio.Formatter.Framework.Attributes;
using Morestachio.Framework.Expression;

namespace Morestachio.Formatter.Predefined
{
	/// <summary>
	///		Defines helper method for single line conditional output
	/// </summary>
	public static class ConditionalFormatter
	{
		/// <summary>
		/// </summary>
		[MorestachioFormatter("When", "Prints the argument when the source value is true. Otherwise null.")]
		[MorestachioGlobalFormatter("When", "Prints the argument when the source value is true. Otherwise null.")]
		public static object When(bool condition, object outputTrue)
		{
			return When(condition, outputTrue, null);
		}

		/// <summary>
		/// </summary>
		[MorestachioFormatter("When", "Prints the argument when the source value is true. Otherwise null.")]
		[MorestachioGlobalFormatter("When", "Prints the argument when the source value is true. Otherwise null.")]
		public static object When(bool condition, object outputTrue, object outputFalse)
		{
			return condition ? outputTrue : outputFalse;
		}
	}
}