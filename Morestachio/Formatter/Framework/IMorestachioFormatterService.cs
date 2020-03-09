using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Morestachio.Formatter.Framework.Converter;

namespace Morestachio.Formatter.Framework
{
	/// <summary>
	///		Interface for Resolving formatters
	/// </summary>
	public interface IMorestachioFormatterService
	{		

		/// <summary>
		///		Defines how Formatters names are compared
		/// </summary>
		StringComparison FormatterNameCompareMode { get; set; }

		/// <summary>
		///		Filteres the list of Formatters
		/// </summary>
		/// <param name="filter"></param>
		/// <returns></returns>
		IEnumerable<MorestachioFormatterModel> Filter(Func<MorestachioFormatterModel, bool> filter);

		/// <summary>
		///		Defines the list of all formatter value converters
		/// </summary>
		ICollection<IFormatterValueConverter> ValueConverter { get; }

		/// <summary>
		///		Searches for the formatter that matches the type and the given values
		/// </summary>
		/// <param name="type"></param>
		/// <param name="values"></param>
		/// <param name="sourceValue"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		Task<object> CallMostMatchingFormatter(
			[NotNull]Type type,
			[NotNull]KeyValuePair<string, object>[] values,
			object sourceValue,
			[CanBeNull]string name);

		/// <summary>
		///		Adds a new Formatter
		/// </summary>
		/// <param name="method"></param>
		/// <param name="morestachioFormatterAttribute"></param>
		/// <returns></returns>
		MorestachioFormatterModel Add(MethodInfo method, MorestachioFormatterAttribute morestachioFormatterAttribute);
	}
}