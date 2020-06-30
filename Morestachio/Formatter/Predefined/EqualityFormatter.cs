using System;
using System.Linq;
using Morestachio.Attributes;
using Morestachio.Formatter.Framework;
using Morestachio.Helper;

namespace Morestachio.Formatter.Predefined
{
	/// <summary>
	///		A list of predefined Morestachio Formatters
	/// </summary>
	public static class EqualityFormatter
	{
		/// <summary>
		///		Checks two objects for equality
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		[MorestachioFormatter("Equals", "Checks if two objects are equal")]
		public static bool IsEquals([SourceObject]object source, object target)
		{
			if ((source == null && target == null))
			{
				return true;
			}
			if (source == null || target == null)
			{
				return false;
			}

			if (source is IConvertible conv)
			{
				return conv.ToType(target.GetType(), null)?.Equals(target) == true;
			}

			if (target is IConvertible conv2)
			{
				return conv2.ToType(source.GetType(), null)?.Equals(source) == true;
			}

			if (source is IComparable comp)
			{
				return comp.CompareTo(target) == 0;
			}

			return source == target || source.Equals(target) || Equals(source, target);
		}

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

		/// <summary>
		///		Checks if two objects are the same
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		[MorestachioFormatter("ReferenceEquals", "Checks if two objects are the same")]
		public static bool IsReferenceEquals([SourceObject]object source, object target)
		{
			return ReferenceEquals(source, target);
		}
	}
}
