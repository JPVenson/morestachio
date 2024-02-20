using System;
using Morestachio.Formatter.Framework.Converter;

namespace Morestachio.Formatter.Framework.Attributes;

/// <summary>
///		Defines one or more Value Converter for this parameter
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = true)]
public sealed class FormatterValueConverterAttribute : Attribute
{
	/// <summary>
	///		
	/// </summary>
	/// <param name="converterType">Must implement <see cref="IFormatterValueConverter"/></param>
	public FormatterValueConverterAttribute(Type converterType)
	{
		ConverterType = converterType;

		if (!typeof(IFormatterValueConverter).IsAssignableFrom(converterType))
		{
			throw new InvalidOperationException(
				$"The given formatter '{ConverterType}' does not implement {nameof(IFormatterValueConverter)}");
		}
	}

	/// <summary>
	///		The Formatter for this parameter
	/// </summary>
	public Type ConverterType { get; set; }

	private IFormatterValueConverter _converter;

	/// <summary>
	///		Creates an instance of the given converter type
	/// </summary>
	/// <returns></returns>
	public IFormatterValueConverter CreateInstance()
	{
		return _converter ?? (_converter = (IFormatterValueConverter)Activator.CreateInstance(ConverterType));
	}
}