using System;
using System.Linq;
using System.Reflection;

namespace Morestachio.Formatter.Framework.Attributes;

/// <summary>
///		When decorated by a function, it can be used to format in morestachio
/// </summary>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public class MorestachioFormatterAttribute : Attribute, IMorestachioFormatterDescriptor
{
	/// <summary>
	/// Initializes a new instance of the <see cref="MorestachioFormatterAttribute"/> class.
	/// </summary>
	/// <param name="name">The name or [MethodName] as a placeholder for the method name</param>
	/// <param name="description">The description.</param>
	public MorestachioFormatterAttribute(string name, string description)
	{
		Name = name;
		Description = description;
		IsSourceObjectAware = true;
	}

	/// <inheritdoc />
	public bool IsSourceObjectAware { get; set; }

	/// <inheritdoc />
	public string Name { get; private set; }

	/// <inheritdoc />
	public string Description { get; private set; }

	/// <inheritdoc />
	public string ReturnHint { get; set; }

	/// <inheritdoc />
	public Type OutputType { get; set; }

	/// <inheritdoc />
	public bool LinkFunctionTarget { get; set; }

	/// <inheritdoc />
	public virtual string GetFormatterName(MethodInfo method)
	{
		return Name?
			.Replace("[MethodName]", method?.Name);
	}

	/// <inheritdoc />
	public virtual bool ValidateFormatterName(MethodInfo method)
	{
		var name = GetFormatterName(method);

		if (string.IsNullOrWhiteSpace(name))
		{
			return false;
		}

		return MorestachioFormatterService.ValidateFormatterNameRegEx.IsMatch(name);
	}

	/// <inheritdoc />
	public virtual void ValidateFormatter(MethodInfo method)
	{
		if (!ValidateFormatterName(method))
		{
			throw new InvalidOperationException(
				$"The name '{Name}' is invalid. An Formatter may only contain letters and cannot start with an digit");
		}
	}

	/// <inheritdoc />
	public virtual MultiFormatterInfo[] GetParameters(MethodInfo method)
	{
		var arguments = method.GetParameters().Select((e, index) =>
			new MultiFormatterInfo(
				e.ParameterType,
				e.GetCustomAttribute<FormatterArgumentNameAttribute>()?.Name ?? e.Name,
				e.IsOptional,
				index,
				e.GetCustomAttribute<ParamArrayAttribute>() != null ||
				e.GetCustomAttribute<RestParameterAttribute>() != null)
				{
					IsSourceObject = IsSourceObjectAware &&
						e.GetCustomAttribute<SourceObjectAttribute>() != null,
					FormatterValueConverterAttribute =
						e.GetCustomAttributes<FormatterValueConverterAttribute>().ToArray(),
					IsInjected = e.GetCustomAttribute<ExternalDataAttribute>() != null
				}).ToArray();

		//if there is no declared SourceObject then check if the first object is of type what we are formatting and use this one.
		if (!arguments.Any(e => e.IsSourceObject) && arguments.Any() && IsSourceObjectAware)
		{
			arguments[0].IsSourceObject = true;
		}

		var sourceValue = arguments.FirstOrDefault(e => e.IsSourceObject);

		if (sourceValue != null)
		{
			//if we have a source value in the arguments reduce the index of all following 
			//this is important as the source value is never in the formatter string so we will not "count" it 
			for (var i = sourceValue.Index; i < arguments.Length; i++)
			{
				arguments[i].Index--;
			}

			sourceValue.Index = -1;
		}

		return arguments;
	}
}