using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Morestachio.Formatter.Framework.Converter;
#if ValueTask
using ObjectPromise = System.Threading.Tasks.ValueTask<object>;
#else
using ObjectPromise = System.Threading.Tasks.Task<object>;
#endif

namespace Morestachio.Formatter.Framework
{
	/// <summary>
	///		Interface for Resolving formatters
	/// </summary>
	public interface IMorestachioFormatterService
	{
		/// <summary>
		///		Contains a set of objects that can be injected into a formatter
		/// </summary>
		IReadOnlyDictionary<Type, object> ServiceCollection { get; }

		void AddService<T, TE>(TE service) where TE : T;
		void AddService<T>(T service);
		void AddService<T, TE>(Func<TE> serviceFactory) where TE : T;
		void AddService<T>(Func<T> serviceFactory);

		/// <summary>
		///		Experimental <para/>
		///		Allows all parameters that are objects to be null
		/// </summary>
		bool AllParametersAllDefaultValue { get; set; }

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

		///  <summary>
		/// 		Searches for the formatter that matches the type and the given values
		///  </summary>
		///  <param name="type"></param>
		///  <param name="values"></param>
		///  <param name="sourceValue"></param>
		///  <param name="name"></param>
		///  <param name="options"></param>
		///  <param name="scope"></param>
		///  <returns></returns>
		ObjectPromise CallMostMatchingFormatter(
			[NotNull]Type type,
			[NotNull]List<Tuple<string, object>> values,
			object sourceValue,
			[CanBeNull]string name,
			ParserOptions options,
			ScopeData scope);

		/// <summary>
		///		Adds a new Formatter
		/// </summary>
		/// <param name="method"></param>
		/// <param name="morestachioFormatterAttribute"></param>
		/// <returns></returns>
		MorestachioFormatterModel Add(MethodInfo method, MorestachioFormatterAttribute morestachioFormatterAttribute);
	}
}