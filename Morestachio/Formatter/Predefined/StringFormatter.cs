using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Morestachio.Formatter.Framework.Attributes;
using Morestachio.Framework.Expression;

namespace Morestachio.Formatter.Predefined
{
#pragma warning disable CS1591
	public static class StringFormatter
	{
		[MorestachioFormatter("Append", "Concatenates two strings")]
		[MorestachioOperator(OperatorTypes.Add, "Concatenates two strings")]
		public static string Append(string source, string target)
		{
			return source + target;
		}
		
		[MorestachioOperator(OperatorTypes.Add, "Concatenates two strings")]
		public static string Append(object source, string target)
		{
			return source + target;
		}

		[MorestachioOperator(OperatorTypes.Add, "Concatenates two strings")]
		public static string Append(string source, object target)
		{
			return source + target;
		}

		[MorestachioFormatter("Capitalize", "Converts the first character of the passed string to a upper case character.")]
		public static string Capitalize(string source)
		{
			if (source.Length > 0)
			{
				return (char.ToUpper(source[0]) + source.Substring(1));
			}

			return source;
		}

		[MorestachioFormatter("CapitalizeWords", "Converts the first character of each word in the passed string to a upper case character.")]
		public static string CapitalizeWords(string source)
		{
			return source.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries)
				.Select(Capitalize)
				.Aggregate((e, f) => e + " " + f);
		}

		[MorestachioFormatter("Contains", "Returns a boolean indicating whether the input string contains the specified string value.")]
		public static bool Contains(string source, string target)
		{
			return source.Contains(target);
		}

		[MorestachioFormatter("ToLower", "Converts the string to lower case.")]
		public static string ToLower(string source, [ExternalData]ParserOptions options)
		{
			return source.ToLower(options.CultureInfo);
		}

		[MorestachioFormatter("ToUpper", "Converts the string to upper case.")]
		public static string ToUpper(string source, [ExternalData]ParserOptions options)
		{
			return source.ToUpper(options.CultureInfo);
		}

		[MorestachioFormatter("EndsWith", "Returns a boolean indicating whether the input string ends with the specified string value.")]
		public static bool EndsWith(string source, string target)
		{
			return source.EndsWith(target);
		}

		[MorestachioFormatter("StartsWith", "Returns a boolean indicating whether the input string starts with the specified string value.")]
		public static bool StartsWith(string source, string target)
		{
			return source.StartsWith(target);
		}

		[MorestachioFormatter("Trim", "Trims all leading and tailing Whitespaces")]
		public static string Trim(string source)
		{
			return source.Trim();
		}

		[MorestachioFormatter("TrimStart", "Trims all leading Whitespaces")]
		public static string TrimStart(string source)
		{
			return source.TrimStart();
		}

		[MorestachioFormatter("TrimEnd", "Trims all tailing Whitespaces")]
		public static string TrimEnd(string source)
		{
			return source.TrimEnd();
		}

		[MorestachioFormatter("Remove", "Removes the range of substring from the string")]
		public static string Remove(string source, int start, int count)
		{
			return source.Remove(start, count);
		}

		[MorestachioFormatter("Remove", "Removes all occurrences of a substring from a string.")]
		public static string Remove(string source, string search)
		{
			return source.Replace(search, "");
		}

		[MorestachioFormatter("Replace", "Replaces all occurrences of a substring from a string.")]
		public static string Replace(string source, string search, string with)
		{
			return source.Replace(search, with);
		}

		[MorestachioFormatter("Substring", "The slice returns a substring, starting at the specified index. An optional second parameter can be passed to specify the length of the substring. If no second parameter is given, a substring with the remaining characters will be returned.")]
		public static string Substring(string source, int start, int count)
		{
			if (start > source.Length)
			{
				return string.Empty;
			}

			if (start + count > source.Length)
			{
				count = source.Length - start;
			}
			return source.Substring(start, count);
		}

		[MorestachioFormatter("Substring", "The slice returns a substring, starting at the specified index. An optional second parameter can be passed to specify the length of the substring. If no second parameter is given, a substring with the remaining characters will be returned.")]
		public static string Substring(string source, int start)
		{
			if (start > source.Length)
			{
				return string.Empty;
			}
			
			return source.Substring(start);
		}

		[MorestachioFormatter("Split", "Splits the string each time any delimiter is found")]
		public static string[] Split(string source, [RestParameter] params object[] delimiters)
		{
			var separator = delimiters.Select(f => f.ToString()).ToArray();
			return source.Split(separator, StringSplitOptions.None);
		}

		[MorestachioFormatter("Join", "Joins an Array of string together by using a seperator")]
		public static string Join(IEnumerable<string> source, string seperator)
		{
			return string.Join(seperator, source);
		}

		[MorestachioFormatter("Truncate", "Truncates a string down to the number of characters passed as the first parameter. An ellipsis (...) is appended to the truncated string and is included in the character count")]
		public static string Truncate(string source, int length, string ellipsis = "...")
		{
			ellipsis = ellipsis ?? "...";
			if (string.IsNullOrEmpty(source))
			{
				return string.Empty;
			}
			int lMinusTruncate = length - ellipsis.Length;
			if (source.Length > length)
			{
				var builder = new StringBuilder();
				builder.Append(source, 0, lMinusTruncate < 0 ? 0 : lMinusTruncate);
				builder.Append(ellipsis);
				source = builder.ToString();
			}
			return source;
		}

		[MorestachioFormatter("PadLeft", "Pads a string with leading spaces to a specified total length.")]
		public static string PadLeft(string source, int width)
		{
			return source.PadLeft(width);
		}

		[MorestachioFormatter("PadRight", "Pads a string with leading spaces to a specified total length.")]
		public static string PadRight(string source, int width)
		{
			return source.PadRight(width);
		}

		[MorestachioFormatter("ToBase64", "Encodes a string to its Base64 representation the encoding will be the same as the template")]
		public static string ToBase64(string source, [ExternalData]ParserOptions options)
		{
			return Convert.ToBase64String(options.Encoding.GetBytes(source ?? string.Empty));
		}

		[MorestachioFormatter("FromBase64", "Decodes a string from its Base64 representation the decoding is expected be the same as the template")]
		public static string FromBase64(string source, [ExternalData]ParserOptions options)
		{
			return options.Encoding.GetString(Convert.FromBase64String(source));
		}
	}
}