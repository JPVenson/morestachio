using System;
using System.Reflection;

namespace Morestachio.Formatter.Framework.Attributes;

/// <summary>
///		Contains meta information for a formatter
/// </summary>
public interface IMorestachioFormatterDescriptor
{
	/// <summary>
	///		Gets or Sets whoever an Formatter should apply the <see cref="SourceObjectAttribute"/> to its first argument if not anywhere else present
	/// <para>If its set to true and no argument has an <see cref="SourceObjectAttribute"/>, the first argument will be used to determinate the source value</para>
	/// <para>If its set to false the formatter can be called globally without specifying and object first. This ignores the <see cref="SourceObjectAttribute"/></para>
	/// </summary>
	/// <value>Default true</value>
	bool IsSourceObjectAware { get; }

	/// <summary>
	///		What is the "header" of the function in morestachio.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Gets the description.
	/// </summary>
	/// <value>
	/// The description.
	/// </value>
	string Description { get; }

	/// <summary>
	/// Gets or sets the return hint.
	/// </summary>
	/// <value>
	/// The return hint.
	/// </value>
	string ReturnHint { get; }

	/// <summary>
	/// Gets or sets the type of the output.
	/// </summary>
	/// <value>
	/// The type of the output.
	/// </value>
	Type OutputType { get; }

	/// <summary>
	///		When enabled the Method info is invoked on the given object
	/// </summary>
	bool LinkFunctionTarget { get; }

	/// <summary>
	///		Replaces alias or variables in the formatter name
	/// </summary>
	/// <param name="method"></param>
	/// <returns></returns>
	string GetFormatterName(MethodInfo method);

	///  <summary>
	/// 		Validates the name of the formatter
	///  </summary>
	///  <param name="method"></param>
	///  <returns></returns>
	bool ValidateFormatterName(MethodInfo method);

	///  <summary>
	/// 		Validates the formatter
	///  </summary>
	///  <param name="method"></param>
	///  <returns></returns>
	void ValidateFormatter(MethodInfo method);

	/// <summary>
	///		Gets all parameters from an method info
	/// </summary>
	/// <param name="method"></param>
	/// <returns></returns>
	MultiFormatterInfo[] GetParameters(MethodInfo method);
}