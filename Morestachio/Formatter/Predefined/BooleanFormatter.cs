using System;
using System.Linq;
using Morestachio.Formatter.Framework.Attributes;
using Morestachio.Framework.Expression;

namespace Morestachio.Formatter.Predefined;
#pragma warning disable CS1591
public static class BooleanFormatter
{
	/// <summary>
	///		Negates a boolean value
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[MorestachioFormatter("Negate", "Negates a Boolean value")]
	[MorestachioFormatter("Not", "Negates a Boolean value")]
	[MorestachioOperator(OperatorTypes.Invert, "Inverts a boolean value")]
	public static bool Negate([SourceObject]bool value)
	{
		return !value;
	}

	[MorestachioFormatter("And", "Returns true if all values are true")]
	[MorestachioOperator(OperatorTypes.And, "Returns true if all values are true")]
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
	[MorestachioOperator(OperatorTypes.Or, "Returns true any value is true")]
	public static bool Or([SourceObject]bool value, bool other)
	{
		return value || other;
	}

	[MorestachioFormatter("Or", "Returns true any value is true")]
	public static bool Or([SourceObject]bool value, [RestParameter]params object[] values)
	{
		return value || values.OfType<bool>().Any(f => f);
	}

	[MorestachioFormatter("ParseBool", "Parses a boolean from string by checking for the equality of ether '1', 'yes', 'true', 'valid'")]
	public static bool ParseBool([SourceObject]string value)
	{
		return value.Equals("true", StringComparison.OrdinalIgnoreCase)
			|| value.Equals("yes", StringComparison.OrdinalIgnoreCase)
			|| value.Equals("valid", StringComparison.OrdinalIgnoreCase)
			|| value.Equals("1", StringComparison.OrdinalIgnoreCase);
	}
}