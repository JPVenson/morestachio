using System.Text.RegularExpressions;
using Morestachio.Formatter.Framework;
using Morestachio.Formatter.Framework.Attributes;

namespace Morestachio.Formatter.Predefined;
#pragma warning disable CS1591
public static class RegexFormatter
{
	[MorestachioFormatter("Regex", "Creates a new Regex with the given pattern")]
	[MorestachioGlobalFormatter("Regex", "Creates a new Regex with the given pattern")]
	public static Regex Regex(string pattern)
	{
		return new Regex(pattern);
	}

	[MorestachioFormatter("IsMatch", "Searches the input string for one or more occurrences of the text supplied in the given pattern.")]
	public static bool IsMatch(Regex pattern, string input)
	{
		return pattern.IsMatch(input);
	}

	[MorestachioFormatter("Match", "Matches a regular expression with a string and returns the precise result as a RegexMatch object.")]
	public static Match Match(Regex pattern, string input)
	{
		return pattern.Match(input);
	}

	[MorestachioFormatter("Replace", "Replaces all occurrences of the previously defined pattern with the replacement pattern, starting at the first character in the input string.")]
	public static string Replace(Regex pattern, string input, string replacement)
	{
		return pattern.Replace(input, replacement);
	}

	[MorestachioFormatter("Split", "Splits the input string at the position defined by a previous pattern.")]
	public static string[] Split(Regex pattern, string input)
	{
		return pattern.Split(input);
	}

	[MorestachioFormatter("Matches", "Returns all the successful matches as if Match was called iteratively numerous times.")]
	public static MatchCollection Matches(Regex pattern, string input)
	{
		return pattern.Matches(input);
	}

	[MorestachioFormatter("Matches", "Returns the expansion of the passed replacement pattern. For example, if the replacement pattern is ?$1$2?, Result returns the concatenation of Group(1).ToString() and Group(2).ToString().")]
	public static string Matches(Match match, string replacement)
	{
		return match.Result(replacement);
	}
}