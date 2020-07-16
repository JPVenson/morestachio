using System.Linq;
using Morestachio.Attributes;
using Morestachio.Formatter.Framework;

namespace Morestachio.Formatter.Predefined
{
	public static class BooleanFormatter
	{
		/// <summary>
		///		Negates a boolean value
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[MorestachioFormatter("Negate", "Negates a Boolean value")]
		public static bool Negate([SourceObject]bool value)
		{
			return !value;
		}

		[MorestachioFormatter("And", "Returns true if all values are true")]
		public static bool And([SourceObject]bool value, bool other)
		{
			return value && other;
		}

		[MorestachioFormatter("And", "Returns true if all values are true")]
		public static bool And([SourceObject]bool value, [RestParameter]params object[] values)
		{
			return value && values.OfType<bool>().All(f => f);
		}

		[MorestachioFormatter("Or", "Returns true any value is true")]
		public static bool Or([SourceObject]bool value, bool other)
		{
			return value || other;
		}

		[MorestachioFormatter("Or", "Returns true any value is true")]
		public static bool Or([SourceObject]bool value, [RestParameter]params object[] values)
		{
			return value || values.OfType<bool>().Any(f => f);
		}
	}
}