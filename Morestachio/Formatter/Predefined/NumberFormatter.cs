

using System;
using Morestachio.Attributes;
using Morestachio.Formatter.Framework;

namespace Morestachio.Formatter.Predefined
{
	public static class NumberFormatter
	{
	 
		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static Boolean BiggerAs([SourceObject]Int16 val1, Int16 val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static Boolean SmallerAs([SourceObject]Int16 val1, Int16 val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static Boolean BiggerOrEqualAs([SourceObject]Int16 val1, Int16 val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static Boolean SmallerOrEqualAs([SourceObject]Int16 val1, Int16 val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static Boolean BiggerAs([SourceObject]Int16 val1, Int32 val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static Boolean SmallerAs([SourceObject]Int16 val1, Int32 val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static Boolean BiggerOrEqualAs([SourceObject]Int16 val1, Int32 val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static Boolean SmallerOrEqualAs([SourceObject]Int16 val1, Int32 val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static Boolean BiggerAs([SourceObject]Int16 val1, Int64 val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static Boolean SmallerAs([SourceObject]Int16 val1, Int64 val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static Boolean BiggerOrEqualAs([SourceObject]Int16 val1, Int64 val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static Boolean SmallerOrEqualAs([SourceObject]Int16 val1, Int64 val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static Boolean BiggerAs([SourceObject]Int16 val1, Decimal val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static Boolean SmallerAs([SourceObject]Int16 val1, Decimal val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static Boolean BiggerOrEqualAs([SourceObject]Int16 val1, Decimal val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static Boolean SmallerOrEqualAs([SourceObject]Int16 val1, Decimal val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static Boolean BiggerAs([SourceObject]Int16 val1, Double val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static Boolean SmallerAs([SourceObject]Int16 val1, Double val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static Boolean BiggerOrEqualAs([SourceObject]Int16 val1, Double val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static Boolean SmallerOrEqualAs([SourceObject]Int16 val1, Double val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static Boolean BiggerAs([SourceObject]Int32 val1, Int16 val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static Boolean SmallerAs([SourceObject]Int32 val1, Int16 val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static Boolean BiggerOrEqualAs([SourceObject]Int32 val1, Int16 val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static Boolean SmallerOrEqualAs([SourceObject]Int32 val1, Int16 val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static Boolean BiggerAs([SourceObject]Int32 val1, Int32 val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static Boolean SmallerAs([SourceObject]Int32 val1, Int32 val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static Boolean BiggerOrEqualAs([SourceObject]Int32 val1, Int32 val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static Boolean SmallerOrEqualAs([SourceObject]Int32 val1, Int32 val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static Boolean BiggerAs([SourceObject]Int32 val1, Int64 val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static Boolean SmallerAs([SourceObject]Int32 val1, Int64 val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static Boolean BiggerOrEqualAs([SourceObject]Int32 val1, Int64 val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static Boolean SmallerOrEqualAs([SourceObject]Int32 val1, Int64 val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static Boolean BiggerAs([SourceObject]Int32 val1, Decimal val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static Boolean SmallerAs([SourceObject]Int32 val1, Decimal val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static Boolean BiggerOrEqualAs([SourceObject]Int32 val1, Decimal val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static Boolean SmallerOrEqualAs([SourceObject]Int32 val1, Decimal val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static Boolean BiggerAs([SourceObject]Int32 val1, Double val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static Boolean SmallerAs([SourceObject]Int32 val1, Double val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static Boolean BiggerOrEqualAs([SourceObject]Int32 val1, Double val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static Boolean SmallerOrEqualAs([SourceObject]Int32 val1, Double val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static Boolean BiggerAs([SourceObject]Int64 val1, Int16 val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static Boolean SmallerAs([SourceObject]Int64 val1, Int16 val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static Boolean BiggerOrEqualAs([SourceObject]Int64 val1, Int16 val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static Boolean SmallerOrEqualAs([SourceObject]Int64 val1, Int16 val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static Boolean BiggerAs([SourceObject]Int64 val1, Int32 val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static Boolean SmallerAs([SourceObject]Int64 val1, Int32 val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static Boolean BiggerOrEqualAs([SourceObject]Int64 val1, Int32 val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static Boolean SmallerOrEqualAs([SourceObject]Int64 val1, Int32 val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static Boolean BiggerAs([SourceObject]Int64 val1, Int64 val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static Boolean SmallerAs([SourceObject]Int64 val1, Int64 val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static Boolean BiggerOrEqualAs([SourceObject]Int64 val1, Int64 val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static Boolean SmallerOrEqualAs([SourceObject]Int64 val1, Int64 val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static Boolean BiggerAs([SourceObject]Int64 val1, Decimal val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static Boolean SmallerAs([SourceObject]Int64 val1, Decimal val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static Boolean BiggerOrEqualAs([SourceObject]Int64 val1, Decimal val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static Boolean SmallerOrEqualAs([SourceObject]Int64 val1, Decimal val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static Boolean BiggerAs([SourceObject]Int64 val1, Double val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static Boolean SmallerAs([SourceObject]Int64 val1, Double val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static Boolean BiggerOrEqualAs([SourceObject]Int64 val1, Double val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static Boolean SmallerOrEqualAs([SourceObject]Int64 val1, Double val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static Boolean BiggerAs([SourceObject]Decimal val1, Decimal val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static Boolean SmallerAs([SourceObject]Decimal val1, Decimal val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static Boolean BiggerOrEqualAs([SourceObject]Decimal val1, Decimal val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static Boolean SmallerOrEqualAs([SourceObject]Decimal val1, Decimal val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static Boolean BiggerAs([SourceObject]Decimal val1, Int16 val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static Boolean SmallerAs([SourceObject]Decimal val1, Int16 val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static Boolean BiggerOrEqualAs([SourceObject]Decimal val1, Int16 val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static Boolean SmallerOrEqualAs([SourceObject]Decimal val1, Int16 val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static Boolean BiggerAs([SourceObject]Decimal val1, Int32 val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static Boolean SmallerAs([SourceObject]Decimal val1, Int32 val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static Boolean BiggerOrEqualAs([SourceObject]Decimal val1, Int32 val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static Boolean SmallerOrEqualAs([SourceObject]Decimal val1, Int32 val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static Boolean BiggerAs([SourceObject]Decimal val1, Int64 val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static Boolean SmallerAs([SourceObject]Decimal val1, Int64 val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static Boolean BiggerOrEqualAs([SourceObject]Decimal val1, Int64 val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static Boolean SmallerOrEqualAs([SourceObject]Decimal val1, Int64 val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static Boolean BiggerAs([SourceObject]Double val1, Double val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static Boolean SmallerAs([SourceObject]Double val1, Double val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static Boolean BiggerOrEqualAs([SourceObject]Double val1, Double val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static Boolean SmallerOrEqualAs([SourceObject]Double val1, Double val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static Boolean BiggerAs([SourceObject]Double val1, Int16 val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static Boolean SmallerAs([SourceObject]Double val1, Int16 val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static Boolean BiggerOrEqualAs([SourceObject]Double val1, Int16 val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static Boolean SmallerOrEqualAs([SourceObject]Double val1, Int16 val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static Boolean BiggerAs([SourceObject]Double val1, Int32 val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static Boolean SmallerAs([SourceObject]Double val1, Int32 val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static Boolean BiggerOrEqualAs([SourceObject]Double val1, Int32 val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static Boolean SmallerOrEqualAs([SourceObject]Double val1, Int32 val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static Boolean BiggerAs([SourceObject]Double val1, Int64 val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static Boolean SmallerAs([SourceObject]Double val1, Int64 val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static Boolean BiggerOrEqualAs([SourceObject]Double val1, Int64 val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static Boolean SmallerOrEqualAs([SourceObject]Double val1, Int64 val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static Boolean BiggerAs([SourceObject]UInt16 val1, UInt16 val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static Boolean SmallerAs([SourceObject]UInt16 val1, UInt16 val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static Boolean BiggerOrEqualAs([SourceObject]UInt16 val1, UInt16 val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static Boolean SmallerOrEqualAs([SourceObject]UInt16 val1, UInt16 val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static Boolean BiggerAs([SourceObject]UInt16 val1, UInt32 val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static Boolean SmallerAs([SourceObject]UInt16 val1, UInt32 val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static Boolean BiggerOrEqualAs([SourceObject]UInt16 val1, UInt32 val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static Boolean SmallerOrEqualAs([SourceObject]UInt16 val1, UInt32 val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static Boolean BiggerAs([SourceObject]UInt16 val1, UInt64 val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static Boolean SmallerAs([SourceObject]UInt16 val1, UInt64 val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static Boolean BiggerOrEqualAs([SourceObject]UInt16 val1, UInt64 val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static Boolean SmallerOrEqualAs([SourceObject]UInt16 val1, UInt64 val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static Boolean BiggerAs([SourceObject]UInt32 val1, UInt16 val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static Boolean SmallerAs([SourceObject]UInt32 val1, UInt16 val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static Boolean BiggerOrEqualAs([SourceObject]UInt32 val1, UInt16 val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static Boolean SmallerOrEqualAs([SourceObject]UInt32 val1, UInt16 val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static Boolean BiggerAs([SourceObject]UInt32 val1, UInt32 val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static Boolean SmallerAs([SourceObject]UInt32 val1, UInt32 val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static Boolean BiggerOrEqualAs([SourceObject]UInt32 val1, UInt32 val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static Boolean SmallerOrEqualAs([SourceObject]UInt32 val1, UInt32 val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static Boolean BiggerAs([SourceObject]UInt32 val1, UInt64 val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static Boolean SmallerAs([SourceObject]UInt32 val1, UInt64 val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static Boolean BiggerOrEqualAs([SourceObject]UInt32 val1, UInt64 val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static Boolean SmallerOrEqualAs([SourceObject]UInt32 val1, UInt64 val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static Boolean BiggerAs([SourceObject]UInt64 val1, UInt16 val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static Boolean SmallerAs([SourceObject]UInt64 val1, UInt16 val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static Boolean BiggerOrEqualAs([SourceObject]UInt64 val1, UInt16 val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static Boolean SmallerOrEqualAs([SourceObject]UInt64 val1, UInt16 val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static Boolean BiggerAs([SourceObject]UInt64 val1, UInt32 val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static Boolean SmallerAs([SourceObject]UInt64 val1, UInt32 val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static Boolean BiggerOrEqualAs([SourceObject]UInt64 val1, UInt32 val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static Boolean SmallerOrEqualAs([SourceObject]UInt64 val1, UInt32 val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static Boolean BiggerAs([SourceObject]UInt64 val1, UInt64 val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static Boolean SmallerAs([SourceObject]UInt64 val1, UInt64 val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static Boolean BiggerOrEqualAs([SourceObject]UInt64 val1, UInt64 val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static Boolean SmallerOrEqualAs([SourceObject]UInt64 val1, UInt64 val2) {
			return val1 <= val2;
		}
	}
}
