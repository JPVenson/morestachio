using Morestachio.Attributes;
using Morestachio.Formatter.Framework;

namespace Morestachio.Formatter.Predefined
{
	public static class EqualityFormatter
	{
		[MorestachioFormatter("==", "Checks if two objects are equals")]
		public static bool IsEquals([SourceObject]object source, object target)
		{
			return Equals(source, target);
		}

		[MorestachioFormatter("!", "Negates a Boolean value")]
		public static bool Negate([SourceObject]bool value)
		{
			return !value;
		}

		[MorestachioFormatter("===", "Checks if two objects are the same")]
		public static bool IsReferenceEquals([SourceObject]object source, object target)
		{
			return ReferenceEquals(source, target);
		}
	}
}
