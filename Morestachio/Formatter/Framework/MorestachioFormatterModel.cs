using System;
using System.Reflection;
using Morestachio.Formatter.Framework.Attributes;

namespace Morestachio.Formatter.Framework;

/// <summary>
///		Wrapper class for a function call
/// </summary>
public class MorestachioFormatterModel
{
	/// <summary>
	/// Initializes a new instance of the <see cref="MorestachioFormatterModel"/> class.
	/// </summary>
	public MorestachioFormatterModel(
		string name,
		string description,
		Type inputType,
		InputDescription[] inputDescription,
		string output,
		MethodInfo function,
		MultiFormatterInfoCollection metaData,
		bool isGlobalFormatter,
		bool linkFunctionTarget)
	{
		Name = name;
		Description = description;
		InputDescription = inputDescription;
		Output = output;
		Function = function;
		InputType = inputType;
		MetaData = metaData;
		IsGlobalFormatter = isGlobalFormatter;
		LinkFunctionTarget = linkFunctionTarget;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="MorestachioFormatterModel"/> class.
	/// </summary>
	public MorestachioFormatterModel(string name,
									string description,
									Type inputType,
									Type outputType,
									InputDescription[] inputDescription,
									string output,
									MethodInfo function,
									MultiFormatterInfoCollection metaData,
									bool isGlobalFormatter,
									bool linkFunctionTarget)
		: this(name, description, inputType, inputDescription, output, function, metaData, isGlobalFormatter,
			linkFunctionTarget)
	{
		OutputType = outputType;
	}

	/// <summary>
	///		Gets if this is an SourceObject less formatter
	/// </summary>
	public bool IsGlobalFormatter { get; }

	/// <summary>
	/// Gets the name.
	/// </summary>
	/// <value>
	/// The name.
	/// </value>
	public string Name { get; private set; }

	/// <summary>
	/// Gets the description.
	/// </summary>
	/// <value>
	/// The description.
	/// </value>
	public string Description { get; private set; }

	/// <summary>
	/// Gets the input description.
	/// </summary>
	/// <value>
	/// The input description.
	/// </value>
	public InputDescription[] InputDescription { get; private set; }

	/// <summary>
	/// Gets the type of the input.
	/// </summary>
	/// <value>
	/// The type of the input.
	/// </value>
	public Type InputType { get; private set; }

	/// <summary>
	/// Gets the output.
	/// </summary>
	/// <value>
	/// The output.
	/// </value>
	public string Output { get; private set; }

	/// <summary>
	/// Gets the type of the output.
	/// </summary>
	/// <value>
	/// The type of the output.
	/// </value>
	public Type OutputType { get; private set; }

	/// <summary>
	/// Gets the function.
	/// </summary>
	/// <value>
	/// The function.
	/// </value>
	public MethodInfo Function { get; private set; }

	/// <summary>
	///		When set links the <see cref="Function"/>s target to the invoking object if the <see cref="Function"/> is an instance function
	/// </summary>
	public bool LinkFunctionTarget { get; private set; }

	internal object FunctionTarget { get; set; }


	/// <summary>
	///     Gets the Meta data for the Arguments
	/// </summary>


	public MultiFormatterInfoCollection MetaData { get; }
}