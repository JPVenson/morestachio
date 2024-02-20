using System;
using System.Net;
using System.Text.RegularExpressions;
using Morestachio.Formatter.Framework;
using Morestachio.Formatter.Framework.Attributes;

namespace Morestachio.Formatter.Predefined;
#pragma warning disable 1591
public static class HtmlFormatter
#pragma warning restore 1591
{
	private const string RegexMatchHtml
		= @"<script.*?</script>|<!--.*?-->|<style.*?</style>|<(?:[^>=]|='[^']*'|=""[^""]*""|=[^'""][^\s>]*)*>";

	private static readonly Regex HtmlTagRegEx
		= new Regex(RegexMatchHtml, RegexOptions.IgnoreCase | RegexOptions.Singleline);

	/// <summary>
	///		Removes any HTML tags from the input string
	/// </summary>
	[MorestachioFormatter("HtmlStrip", "Removes any HTML tags from the input string")]
	[MorestachioGlobalFormatter("HtmlStrip", "Removes any HTML tags from the input string")]
	public static string Strip(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}

		return HtmlTagRegEx.Replace(text, string.Empty);
	}

	/// <summary>
	///		Converts a string to an HTML-encoded string.
	/// </summary>
	/// <param name="text"></param>
	/// <returns></returns>
	[MorestachioFormatter("HtmlEncode", "Converts a string to an HTML-encoded string.")]
	[MorestachioGlobalFormatter("HtmlEncode", "Converts a string to an HTML-encoded string.")]
	public static string HtmlEncode(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}

		return WebUtility.HtmlEncode(text);
	}

	/// <summary>
	///		Converts a string that has been HTML-encoded for HTTP transmission into a decoded string.
	/// </summary>
	/// <param name="text"></param>
	/// <returns></returns>
	[MorestachioFormatter("HtmlDecode",
		"Converts a string that has been HTML-encoded for HTTP transmission into a decoded string.")]
	[MorestachioGlobalFormatter("HtmlDecode",
		"Converts a string that has been HTML-encoded for HTTP transmission into a decoded string.")]
	public static string HtmlDecode(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}

		return WebUtility.HtmlDecode(text);
	}

	/// <summary>
	///		Converts a string to its escaped representation.
	/// </summary>
	/// <param name="text"></param>
	/// <returns></returns>
	[MorestachioFormatter("UrlEncode", "Converts a string to its escaped representation.")]
	[MorestachioGlobalFormatter("UrlEncode", "Converts a string to its escaped representation.")]
	public static string UrlEncode(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}

		return Uri.EscapeDataString(text);
	}

	/// <summary>
	///		Converts a URI string to its escaped representation.
	/// </summary>
	/// <param name="text"></param>
	/// <returns></returns>
	[MorestachioFormatter("UrlEscape", "Converts a URI string to its escaped representation.")]
	[MorestachioGlobalFormatter("UrlEscape", "Converts a URI string to its escaped representation.")]
	public static string UrlEscape(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}

		return Uri.EscapeDataString(text);
	}
}