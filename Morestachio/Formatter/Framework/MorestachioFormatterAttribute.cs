using System;
using Morestachio.Attributes;

namespace Morestachio.Formatter.Framework
{
	/// <summary>
	///		When decorated by a function, it can be used to format in morestachio
	/// </summary>
	/// <seealso cref="System.Attribute" />
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	public class MorestachioFormatterAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MorestachioFormatterAttribute"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="description">The description.</param>
		public MorestachioFormatterAttribute(string name, string description)
		{
			Name = name;
			Description = description;
			IsSourceObjectAware = true;
		}

		/// <summary>
		///		Gets or Sets whoever an Formatter should apply the <see cref="SourceObjectAttribute"/> to its first argument if not anywhere else present
		/// <para>If its set to true and no argument has an <see cref="SourceObjectAttribute"/>, the first argument will be used to determinate the source value</para>
		/// <para>If its set to false the formatter can be called globally without specifying and object first. This ignores the <see cref="SourceObjectAttribute"/></para>
		/// </summary>
		/// <value>Default true</value>
		public bool IsSourceObjectAware { get; set; }

		/// <summary>
		///		What is the "header" of the function in morestachio.
		/// </summary>
		public string Name { get; private set; }
		/// <summary>
		/// Gets the description.
		/// </summary>
		/// <value>
		/// The description.
		/// </value>
		public string Description { get; private set; }
		/// <summary>
		/// Gets or sets the return hint.
		/// </summary>
		/// <value>
		/// The return hint.
		/// </value>
		public string ReturnHint { get; set; }
		/// <summary>
		/// Gets or sets the type of the output.
		/// </summary>
		/// <value>
		/// The type of the output.
		/// </value>
		public Type OutputType { get; set; }
	}

	/// <summary>
	///		Defines an Global Formatter that can be called without the need for specifing an source object
	/// </summary>
	public class MorestachioGlobalFormatterAttribute : MorestachioFormatterAttribute
	{
		public MorestachioGlobalFormatterAttribute(string name, string description) : base(name, description)
		{
			IsSourceObjectAware = false;
		}
	}
}