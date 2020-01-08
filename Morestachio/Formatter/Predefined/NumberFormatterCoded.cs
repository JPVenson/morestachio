using System;
using System.Collections.Generic;
using System.Text;
using Morestachio.Attributes;
using Morestachio.Formatter.Framework;

namespace Morestachio.Formatter.Predefined
{
	public static partial class NumberFormatter
	{
		[MorestachioFormatter("fnc_+", "Adds two numbers")]
		public static int Add([SourceObject]short val1, short val2) {
			return val1 + val2;
		}

		[MorestachioFormatter("fnc_-", "Substracts the other number from the current number")]
		public static int Substract([SourceObject]short val1, short val2) {
			return val1 - val2;
		}

		[MorestachioFormatter("fnc_/", "Devides the other number from the current number")]
		public static int Divide([SourceObject]short val1, short val2) {
			return val1 / val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static bool BiggerAs([SourceObject]short val1, short val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static bool SmallerAs([SourceObject]short val1, short val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static bool BiggerOrEqualAs([SourceObject]short val1, short val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static bool SmallerOrEqualAs([SourceObject]short val1, short val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_+", "Adds two numbers")]
		public static int Add([SourceObject]short val1, int val2) {
			return val1 + val2;
		}

		[MorestachioFormatter("fnc_-", "Substracts the other number from the current number")]
		public static int Substract([SourceObject]short val1, int val2) {
			return val1 - val2;
		}

		[MorestachioFormatter("fnc_/", "Devides the other number from the current number")]
		public static int Divide([SourceObject]short val1, int val2) {
			return val1 / val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static bool BiggerAs([SourceObject]short val1, int val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static bool SmallerAs([SourceObject]short val1, int val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static bool BiggerOrEqualAs([SourceObject]short val1, int val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static bool SmallerOrEqualAs([SourceObject]short val1, int val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_+", "Adds two numbers")]
		public static long Add([SourceObject]short val1, long val2) {
			return val1 + val2;
		}

		[MorestachioFormatter("fnc_-", "Substracts the other number from the current number")]
		public static long Substract([SourceObject]short val1, long val2) {
			return val1 - val2;
		}

		[MorestachioFormatter("fnc_/", "Devides the other number from the current number")]
		public static long Divide([SourceObject]short val1, long val2) {
			return val1 / val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static bool BiggerAs([SourceObject]short val1, long val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static bool SmallerAs([SourceObject]short val1, long val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static bool BiggerOrEqualAs([SourceObject]short val1, long val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static bool SmallerOrEqualAs([SourceObject]short val1, long val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_+", "Adds two numbers")]
		public static decimal Add([SourceObject]short val1, decimal val2) {
			return val1 + val2;
		}

		[MorestachioFormatter("fnc_-", "Substracts the other number from the current number")]
		public static decimal Substract([SourceObject]short val1, decimal val2) {
			return val1 - val2;
		}

		[MorestachioFormatter("fnc_/", "Devides the other number from the current number")]
		public static decimal Divide([SourceObject]short val1, decimal val2) {
			return val1 / val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static bool BiggerAs([SourceObject]short val1, decimal val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static bool SmallerAs([SourceObject]short val1, decimal val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static bool BiggerOrEqualAs([SourceObject]short val1, decimal val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static bool SmallerOrEqualAs([SourceObject]short val1, decimal val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_+", "Adds two numbers")]
		public static double Add([SourceObject]short val1, double val2) {
			return val1 + val2;
		}

		[MorestachioFormatter("fnc_-", "Substracts the other number from the current number")]
		public static double Substract([SourceObject]short val1, double val2) {
			return val1 - val2;
		}

		[MorestachioFormatter("fnc_/", "Devides the other number from the current number")]
		public static double Divide([SourceObject]short val1, double val2) {
			return val1 / val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static bool BiggerAs([SourceObject]short val1, double val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static bool SmallerAs([SourceObject]short val1, double val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static bool BiggerOrEqualAs([SourceObject]short val1, double val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static bool SmallerOrEqualAs([SourceObject]short val1, double val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_+", "Adds two numbers")]
		public static int Add([SourceObject]int val1, short val2) {
			return val1 + val2;
		}

		[MorestachioFormatter("fnc_-", "Substracts the other number from the current number")]
		public static int Substract([SourceObject]int val1, short val2) {
			return val1 - val2;
		}

		[MorestachioFormatter("fnc_/", "Devides the other number from the current number")]
		public static int Divide([SourceObject]int val1, short val2) {
			return val1 / val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static bool BiggerAs([SourceObject]int val1, short val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static bool SmallerAs([SourceObject]int val1, short val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static bool BiggerOrEqualAs([SourceObject]int val1, short val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static bool SmallerOrEqualAs([SourceObject]int val1, short val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_+", "Adds two numbers")]
		public static int Add([SourceObject]int val1, int val2) {
			return val1 + val2;
		}

		[MorestachioFormatter("fnc_-", "Substracts the other number from the current number")]
		public static int Substract([SourceObject]int val1, int val2) {
			return val1 - val2;
		}

		[MorestachioFormatter("fnc_/", "Devides the other number from the current number")]
		public static int Divide([SourceObject]int val1, int val2) {
			return val1 / val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static bool BiggerAs([SourceObject]int val1, int val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static bool SmallerAs([SourceObject]int val1, int val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static bool BiggerOrEqualAs([SourceObject]int val1, int val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static bool SmallerOrEqualAs([SourceObject]int val1, int val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_+", "Adds two numbers")]
		public static long Add([SourceObject]int val1, long val2) {
			return val1 + val2;
		}

		[MorestachioFormatter("fnc_-", "Substracts the other number from the current number")]
		public static long Substract([SourceObject]int val1, long val2) {
			return val1 - val2;
		}

		[MorestachioFormatter("fnc_/", "Devides the other number from the current number")]
		public static long Divide([SourceObject]int val1, long val2) {
			return val1 / val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static bool BiggerAs([SourceObject]int val1, long val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static bool SmallerAs([SourceObject]int val1, long val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static bool BiggerOrEqualAs([SourceObject]int val1, long val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static bool SmallerOrEqualAs([SourceObject]int val1, long val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_+", "Adds two numbers")]
		public static decimal Add([SourceObject]int val1, decimal val2) {
			return val1 + val2;
		}

		[MorestachioFormatter("fnc_-", "Substracts the other number from the current number")]
		public static decimal Substract([SourceObject]int val1, decimal val2) {
			return val1 - val2;
		}

		[MorestachioFormatter("fnc_/", "Devides the other number from the current number")]
		public static decimal Divide([SourceObject]int val1, decimal val2) {
			return val1 / val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static bool BiggerAs([SourceObject]int val1, decimal val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static bool SmallerAs([SourceObject]int val1, decimal val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static bool BiggerOrEqualAs([SourceObject]int val1, decimal val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static bool SmallerOrEqualAs([SourceObject]int val1, decimal val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_+", "Adds two numbers")]
		public static double Add([SourceObject]int val1, double val2) {
			return val1 + val2;
		}

		[MorestachioFormatter("fnc_-", "Substracts the other number from the current number")]
		public static double Substract([SourceObject]int val1, double val2) {
			return val1 - val2;
		}

		[MorestachioFormatter("fnc_/", "Devides the other number from the current number")]
		public static double Divide([SourceObject]int val1, double val2) {
			return val1 / val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static bool BiggerAs([SourceObject]int val1, double val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static bool SmallerAs([SourceObject]int val1, double val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static bool BiggerOrEqualAs([SourceObject]int val1, double val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static bool SmallerOrEqualAs([SourceObject]int val1, double val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_+", "Adds two numbers")]
		public static long Add([SourceObject]long val1, short val2) {
			return val1 + val2;
		}

		[MorestachioFormatter("fnc_-", "Substracts the other number from the current number")]
		public static long Substract([SourceObject]long val1, short val2) {
			return val1 - val2;
		}

		[MorestachioFormatter("fnc_/", "Devides the other number from the current number")]
		public static long Divide([SourceObject]long val1, short val2) {
			return val1 / val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static bool BiggerAs([SourceObject]long val1, short val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static bool SmallerAs([SourceObject]long val1, short val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static bool BiggerOrEqualAs([SourceObject]long val1, short val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static bool SmallerOrEqualAs([SourceObject]long val1, short val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_+", "Adds two numbers")]
		public static long Add([SourceObject]long val1, int val2) {
			return val1 + val2;
		}

		[MorestachioFormatter("fnc_-", "Substracts the other number from the current number")]
		public static long Substract([SourceObject]long val1, int val2) {
			return val1 - val2;
		}

		[MorestachioFormatter("fnc_/", "Devides the other number from the current number")]
		public static long Divide([SourceObject]long val1, int val2) {
			return val1 / val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static bool BiggerAs([SourceObject]long val1, int val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static bool SmallerAs([SourceObject]long val1, int val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static bool BiggerOrEqualAs([SourceObject]long val1, int val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static bool SmallerOrEqualAs([SourceObject]long val1, int val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_+", "Adds two numbers")]
		public static long Add([SourceObject]long val1, long val2) {
			return val1 + val2;
		}

		[MorestachioFormatter("fnc_-", "Substracts the other number from the current number")]
		public static long Substract([SourceObject]long val1, long val2) {
			return val1 - val2;
		}

		[MorestachioFormatter("fnc_/", "Devides the other number from the current number")]
		public static long Divide([SourceObject]long val1, long val2) {
			return val1 / val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static bool BiggerAs([SourceObject]long val1, long val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static bool SmallerAs([SourceObject]long val1, long val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static bool BiggerOrEqualAs([SourceObject]long val1, long val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static bool SmallerOrEqualAs([SourceObject]long val1, long val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_+", "Adds two numbers")]
		public static decimal Add([SourceObject]long val1, decimal val2) {
			return val1 + val2;
		}

		[MorestachioFormatter("fnc_-", "Substracts the other number from the current number")]
		public static decimal Substract([SourceObject]long val1, decimal val2) {
			return val1 - val2;
		}

		[MorestachioFormatter("fnc_/", "Devides the other number from the current number")]
		public static decimal Divide([SourceObject]long val1, decimal val2) {
			return val1 / val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static bool BiggerAs([SourceObject]long val1, decimal val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static bool SmallerAs([SourceObject]long val1, decimal val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static bool BiggerOrEqualAs([SourceObject]long val1, decimal val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static bool SmallerOrEqualAs([SourceObject]long val1, decimal val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_+", "Adds two numbers")]
		public static double Add([SourceObject]long val1, double val2) {
			return val1 + val2;
		}

		[MorestachioFormatter("fnc_-", "Substracts the other number from the current number")]
		public static double Substract([SourceObject]long val1, double val2) {
			return val1 - val2;
		}

		[MorestachioFormatter("fnc_/", "Devides the other number from the current number")]
		public static double Divide([SourceObject]long val1, double val2) {
			return val1 / val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static bool BiggerAs([SourceObject]long val1, double val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static bool SmallerAs([SourceObject]long val1, double val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static bool BiggerOrEqualAs([SourceObject]long val1, double val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static bool SmallerOrEqualAs([SourceObject]long val1, double val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_+", "Adds two numbers")]
		public static decimal Add([SourceObject]decimal val1, decimal val2) {
			return val1 + val2;
		}

		[MorestachioFormatter("fnc_-", "Substracts the other number from the current number")]
		public static decimal Substract([SourceObject]decimal val1, decimal val2) {
			return val1 - val2;
		}

		[MorestachioFormatter("fnc_/", "Devides the other number from the current number")]
		public static decimal Divide([SourceObject]decimal val1, decimal val2) {
			return val1 / val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static bool BiggerAs([SourceObject]decimal val1, decimal val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static bool SmallerAs([SourceObject]decimal val1, decimal val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static bool BiggerOrEqualAs([SourceObject]decimal val1, decimal val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static bool SmallerOrEqualAs([SourceObject]decimal val1, decimal val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_+", "Adds two numbers")]
		public static decimal Add([SourceObject]decimal val1, short val2) {
			return val1 + val2;
		}

		[MorestachioFormatter("fnc_-", "Substracts the other number from the current number")]
		public static decimal Substract([SourceObject]decimal val1, short val2) {
			return val1 - val2;
		}

		[MorestachioFormatter("fnc_/", "Devides the other number from the current number")]
		public static decimal Divide([SourceObject]decimal val1, short val2) {
			return val1 / val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static bool BiggerAs([SourceObject]decimal val1, short val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static bool SmallerAs([SourceObject]decimal val1, short val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static bool BiggerOrEqualAs([SourceObject]decimal val1, short val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static bool SmallerOrEqualAs([SourceObject]decimal val1, short val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_+", "Adds two numbers")]
		public static decimal Add([SourceObject]decimal val1, int val2) {
			return val1 + val2;
		}

		[MorestachioFormatter("fnc_-", "Substracts the other number from the current number")]
		public static decimal Substract([SourceObject]decimal val1, int val2) {
			return val1 - val2;
		}

		[MorestachioFormatter("fnc_/", "Devides the other number from the current number")]
		public static decimal Divide([SourceObject]decimal val1, int val2) {
			return val1 / val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static bool BiggerAs([SourceObject]decimal val1, int val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static bool SmallerAs([SourceObject]decimal val1, int val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static bool BiggerOrEqualAs([SourceObject]decimal val1, int val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static bool SmallerOrEqualAs([SourceObject]decimal val1, int val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_+", "Adds two numbers")]
		public static decimal Add([SourceObject]decimal val1, long val2) {
			return val1 + val2;
		}

		[MorestachioFormatter("fnc_-", "Substracts the other number from the current number")]
		public static decimal Substract([SourceObject]decimal val1, long val2) {
			return val1 - val2;
		}

		[MorestachioFormatter("fnc_/", "Devides the other number from the current number")]
		public static decimal Divide([SourceObject]decimal val1, long val2) {
			return val1 / val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static bool BiggerAs([SourceObject]decimal val1, long val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static bool SmallerAs([SourceObject]decimal val1, long val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static bool BiggerOrEqualAs([SourceObject]decimal val1, long val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static bool SmallerOrEqualAs([SourceObject]decimal val1, long val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_+", "Adds two numbers")]
		public static double Add([SourceObject]double val1, double val2) {
			return val1 + val2;
		}

		[MorestachioFormatter("fnc_-", "Substracts the other number from the current number")]
		public static double Substract([SourceObject]double val1, double val2) {
			return val1 - val2;
		}

		[MorestachioFormatter("fnc_/", "Devides the other number from the current number")]
		public static double Divide([SourceObject]double val1, double val2) {
			return val1 / val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static bool BiggerAs([SourceObject]double val1, double val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static bool SmallerAs([SourceObject]double val1, double val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static bool BiggerOrEqualAs([SourceObject]double val1, double val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static bool SmallerOrEqualAs([SourceObject]double val1, double val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_+", "Adds two numbers")]
		public static double Add([SourceObject]double val1, short val2) {
			return val1 + val2;
		}

		[MorestachioFormatter("fnc_-", "Substracts the other number from the current number")]
		public static double Substract([SourceObject]double val1, short val2) {
			return val1 - val2;
		}

		[MorestachioFormatter("fnc_/", "Devides the other number from the current number")]
		public static double Divide([SourceObject]double val1, short val2) {
			return val1 / val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static bool BiggerAs([SourceObject]double val1, short val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static bool SmallerAs([SourceObject]double val1, short val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static bool BiggerOrEqualAs([SourceObject]double val1, short val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static bool SmallerOrEqualAs([SourceObject]double val1, short val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_+", "Adds two numbers")]
		public static double Add([SourceObject]double val1, int val2) {
			return val1 + val2;
		}

		[MorestachioFormatter("fnc_-", "Substracts the other number from the current number")]
		public static double Substract([SourceObject]double val1, int val2) {
			return val1 - val2;
		}

		[MorestachioFormatter("fnc_/", "Devides the other number from the current number")]
		public static double Divide([SourceObject]double val1, int val2) {
			return val1 / val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static bool BiggerAs([SourceObject]double val1, int val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static bool SmallerAs([SourceObject]double val1, int val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static bool BiggerOrEqualAs([SourceObject]double val1, int val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static bool SmallerOrEqualAs([SourceObject]double val1, int val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_+", "Adds two numbers")]
		public static double Add([SourceObject]double val1, long val2) {
			return val1 + val2;
		}

		[MorestachioFormatter("fnc_-", "Substracts the other number from the current number")]
		public static double Substract([SourceObject]double val1, long val2) {
			return val1 - val2;
		}

		[MorestachioFormatter("fnc_/", "Devides the other number from the current number")]
		public static double Divide([SourceObject]double val1, long val2) {
			return val1 / val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static bool BiggerAs([SourceObject]double val1, long val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static bool SmallerAs([SourceObject]double val1, long val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static bool BiggerOrEqualAs([SourceObject]double val1, long val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static bool SmallerOrEqualAs([SourceObject]double val1, long val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static bool BiggerAs([SourceObject]ushort val1, ushort val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static bool SmallerAs([SourceObject]ushort val1, ushort val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static bool BiggerOrEqualAs([SourceObject]ushort val1, ushort val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static bool SmallerOrEqualAs([SourceObject]ushort val1, ushort val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static bool BiggerAs([SourceObject]ushort val1, uint val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static bool SmallerAs([SourceObject]ushort val1, uint val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static bool BiggerOrEqualAs([SourceObject]ushort val1, uint val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static bool SmallerOrEqualAs([SourceObject]ushort val1, uint val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static bool BiggerAs([SourceObject]ushort val1, ulong val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static bool SmallerAs([SourceObject]ushort val1, ulong val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static bool BiggerOrEqualAs([SourceObject]ushort val1, ulong val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static bool SmallerOrEqualAs([SourceObject]ushort val1, ulong val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static bool BiggerAs([SourceObject]uint val1, ushort val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static bool SmallerAs([SourceObject]uint val1, ushort val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static bool BiggerOrEqualAs([SourceObject]uint val1, ushort val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static bool SmallerOrEqualAs([SourceObject]uint val1, ushort val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static bool BiggerAs([SourceObject]uint val1, uint val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static bool SmallerAs([SourceObject]uint val1, uint val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static bool BiggerOrEqualAs([SourceObject]uint val1, uint val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static bool SmallerOrEqualAs([SourceObject]uint val1, uint val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static bool BiggerAs([SourceObject]uint val1, ulong val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static bool SmallerAs([SourceObject]uint val1, ulong val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static bool BiggerOrEqualAs([SourceObject]uint val1, ulong val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static bool SmallerOrEqualAs([SourceObject]uint val1, ulong val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static bool BiggerAs([SourceObject]ulong val1, ushort val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static bool SmallerAs([SourceObject]ulong val1, ushort val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static bool BiggerOrEqualAs([SourceObject]ulong val1, ushort val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static bool SmallerOrEqualAs([SourceObject]ulong val1, ushort val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static bool BiggerAs([SourceObject]ulong val1, uint val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static bool SmallerAs([SourceObject]ulong val1, uint val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static bool BiggerOrEqualAs([SourceObject]ulong val1, uint val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static bool SmallerOrEqualAs([SourceObject]ulong val1, uint val2) {
			return val1 <= val2;
		}

		[MorestachioFormatter("fnc_>", "Returns if ether the value is bigger as the other")]
		public static bool BiggerAs([SourceObject]ulong val1, ulong val2) {
			return val1 > val2;
		}

		[MorestachioFormatter("fnc_<", "Returns if ether the value is smaller as the other")]
		public static bool SmallerAs([SourceObject]ulong val1, ulong val2) {
			return val1 < val2;
		}

		[MorestachioFormatter("fnc_>=", "Returns if ether the value is bigger or equal as the other")]
		public static bool BiggerOrEqualAs([SourceObject]ulong val1, ulong val2) {
			return val1 >= val2;
		}

		[MorestachioFormatter("fnc_<=", "Returns if ether the value is smaller or equal as the other")]
		public static bool SmallerOrEqualAs([SourceObject]ulong val1, ulong val2) {
			return val1 <= val2;
		}
	}
}
