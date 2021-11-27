using System.Reflection;
using Morestachio.Document;
using Morestachio.Formatter.Framework.Attributes;
using Morestachio.Formatter.Framework.Converter;
using Morestachio.Util.Sealing;

namespace Morestachio.Formatter.Framework
{
	/// <summary>
	///     Interface for Resolving formatters
	/// </summary>
	[ServiceName("Formatters")]
	public interface IMorestachioFormatterService : ISealed
	{
		/// <summary>
		///		This object is available everywhere and acts like root #var objects
		/// </summary>
		IDictionary<string, object> Constants { get; }

		/// <summary>
		///		Gets access to the injectable services
		/// </summary>
		ServiceCollection Services { get; }

		/// <summary>
		///     Experimental
		///     <para />
		///     Allows all parameters that are objects to be null
		/// </summary>
		bool AllParametersAllDefaultValue { get; set; }
		
		/// <summary>
		///     Defines the list of all formatter value converters
		/// </summary>
		ICollection<IFormatterValueConverter> ValueConverter { get; }

		/// <summary>
		///     Filteres the list of Formatters
		/// </summary>
		/// <param name="filter"></param>
		/// <returns></returns>
		IEnumerable<MorestachioFormatterModel> Filter(Func<MorestachioFormatterModel, bool> filter);

		/// <summary>
		///     Searches for the formatter that matches the type and the given values and returns an struct that can be used to
		///     call an optimized formatter multiple times
		/// </summary>
		FormatterCache PrepareCallMostMatchingFormatter(
			Type type,
			FormatterArgumentType[] arguments,
			string name,
			ParserOptions options,
			ScopeData scope);

		/// <summary>
		///     Executes an formatter that must match the definition that it was created with
		/// </summary>
		ObjectPromise Execute(
			FormatterCache formatter,
			object sourceValue,
			ParserOptions parserOptions,
			FormatterArgumentType[] args);

		/// <summary>
		///     Adds a new Formatter
		/// </summary>
		/// <param name="method"></param>
		/// <param name="morestachioFormatterAttribute"></param>
		/// <returns></returns>
		MorestachioFormatterModel Add(MethodInfo method, IMorestachioFormatterDescriptor morestachioFormatterAttribute);
	}
}