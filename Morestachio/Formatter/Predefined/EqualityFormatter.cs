using System;
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

			//if (source is Number srcNumb)
			//{
			//	if (target is Number tarNumb)
			//	{
			//		return srcNumb.Value == tarNumb.Value;
			//	}

			//	return IsEquals(srcNumb.Value, target);
			//}

			//if (target is Number tarNumbA)
			//{
			//	return IsEquals(source, tarNumbA.Value);
			//}
			

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
