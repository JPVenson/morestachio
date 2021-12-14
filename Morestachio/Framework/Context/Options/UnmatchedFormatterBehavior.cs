using System;

namespace Morestachio.Framework.Context.Options;

/// <summary>
///		Defines the behavior how to react to a call of a formatter that does not exist
/// </summary>
public enum UnmatchedFormatterBehavior
{
	/// <summary>
	///		If the Option is set to null and no formatter that matches is found the result of that operation is null
	/// </summary>
	Null,

	/// <summary>
	///		If the option is set to ParentValue and no formatter that matches is found the result of that operation is the last non-null value
	/// </summary>
	ParentValue
}

/// <summary>
///		Defines the behavior how to react to an #tag that is not recognized by the tokenizer
/// </summary>
[Flags]
public enum UnmatchedTagBehavior
{
	/// <summary>
	///		An error should be put in the <see cref="MorestachioDocumentInfo.Errors"/> collection.
	/// <para>This can only be used with <see cref="LogWarning"/></para>
	/// </summary>
	ThrowError = 1 << 0,

	/// <summary>
	///		An warning should be logged within the <see cref="ParserOptions.Logger"/>
	/// </summary>
	LogWarning = 1 << 1,

	/// <summary>
	///		No nothing
	/// </summary>
	Ignore = 1 << 2,

	/// <summary>
	///		Output the whole tag as content
	/// </summary>
	Output = 1 << 3
}

/// <summary>
/// 
/// </summary>
public static class UnmatchedTagBehaviorExtensions
{
	/// <summary>
	///		Checks for the presence of an flag within the enumeration
	/// </summary>
	/// <param name="value"></param>
	/// <param name="flag"></param>
	/// <returns></returns>
	public static bool HasFlagFast(this UnmatchedTagBehavior value, UnmatchedTagBehavior flag)
	{
		return (value & flag) != 0;
	}
}