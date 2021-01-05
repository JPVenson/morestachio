using System;
using System.Collections.Generic;
using System.Reflection;
using Morestachio.Document;
using Morestachio.Formatter.Framework.Attributes;
using Morestachio.Formatter.Framework.Converter;
using Morestachio.Util.Sealing;
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
	public interface IMorestachioFormatterService : IServiceProvider, ISealed
	{
		/// <summary>
		///		Contains a set of objects that can be injected into a formatter
		/// </summary>
		IReadOnlyDictionary<Type, object> ServiceCollection { get; }

		/// <summary>
		///		Adds a new Service to the Service collection
		/// </summary>
		void AddService<T, TE>(TE service) where TE : T;

		/// <summary>
		///		Adds a new Service to the Service collection
		/// </summary>
		void AddService<T>(T service);
		
		/// <summary>
		///		Adds a new Service factory to the Service collection
		/// </summary>
		void AddService<T, TE>(Func<TE> serviceFactory) where TE : T;

		/// <summary>
		///		Adds a new Service factory to the Service collection
		/// </summary>
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
		/// 		Searches for the formatter that matches the type and the given values and returns an struct that can be used to call an optimized formatter multiple times
		///  </summary>
		FormatterCache? PrepareCallMostMatchingFormatter(
			Type type,
			FormatterArgumentType[] arguments,
			string name,
			ParserOptions options,
			ScopeData scope);

		/// <summary>
		///		Executes an formatter that must match the definition that it was created with
		/// </summary>
		/// <param name="formatter"></param>
		/// <param name="sourceType"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		ObjectPromise Execute(
			 FormatterCache formatter,
			object sourceType,
			FormatterArgumentType[] args);

		///// <summary>
		/////		Searches for an cached and prepared formatter call and executes it
		///// </summary>
		///// <param name="args"></param>
		///// <param name="sourceValue"></param>
		///// <param name="name"></param>
		///// <param name="options"></param>
		///// <param name="scope"></param>
		///// <returns></returns>
		//ObjectPromise Execute(
		//	 FormatterArgumentType[] args,
		//	object sourceValue,
		//	 string name,
		//	ParserOptions options,
		//	ScopeData scope);

		/// <summary>
		///		Adds a new Formatter
		/// </summary>
		/// <param name="method"></param>
		/// <param name="morestachioFormatterAttribute"></param>
		/// <returns></returns>
		MorestachioFormatterModel Add(MethodInfo method, MorestachioFormatterAttribute morestachioFormatterAttribute);
	}
}