using System;
using Morestachio.Formatter.Framework;
using Morestachio.Formatter.Framework.Converter;

namespace Morestachio.Attributes
{
	/// <summary>
	///		Marks the Parameter as an Rest parameter. All non specify parameter will given here. 
	/// </summary>
	/// <seealso cref="System.Attribute" />
	[AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
	public sealed class RestParameterAttribute : Attribute
	{
	}

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
				throw new InvalidOperationException($"The given formatter '{ConverterType}' does not implement {nameof(IFormatterValueConverter)}");
			}
		}

		/// <summary>
		///		The Formatter for this parameter
		/// </summary>
		public Type ConverterType { get; set; }
	}
}