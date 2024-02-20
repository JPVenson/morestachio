using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Morestachio.Document;
using Morestachio.Formatter.Framework.Attributes;

namespace Morestachio.Formatter.Framework
{
	/// <summary>
	///		Marks an object to handle its own formatter invocation.
	/// </summary>
	public interface IDynamicFormatterProvider
	{
		/// <summary>
		///		Will be invoked when the <see cref="MorestachioFormatterService"/> should format this type.
		/// </summary>
		/// <param name="type">The type of the object to format.</param>
		/// <param name="arguments">The list of arguments handed by the template.</param>
		/// <param name="name">The name of the formatter to invoke.</param>
		/// <param name="parserOptions">The current ParserOptions</param>
		/// <param name="scopeData">The current runs Scope data</param>
		/// <returns>A <see cref="FormatterCache"/> that will be invoked and cached for subsequent calls that matches the argument description or when null the calling <see cref="MorestachioFormatterService"/> will try to obtain a formatter from its list.</returns>
		FormatterCache GetFormatter(
			IMorestachioFormatterService formatterService,
			Type type,
			FormatterArgumentType[] arguments,
			string name,
			ParserOptions parserOptions,
			ScopeData scopeData);
	}

	/// <summary>
	///		Provides methods for providing formatter delegates at runtime.
	/// </summary>
	public abstract class DynamicFormatterProviderBase : IDynamicFormatterProvider
	{
		/// <inheritdoc />
		public FormatterCache GetFormatter(
			IMorestachioFormatterService formatterService,
			Type type,
			FormatterArgumentType[] arguments,
			string name,
			ParserOptions parserOptions,
			ScopeData scopeData)
		{
			var formatter = GetDynamicFormatterUnsafe(type,
				arguments,
				name,
				parserOptions,
				scopeData);

			if (formatter is null)
			{
				return null;
			}

			return new FormatterCache(MorestachioFormatterService.ModelFromMethodInfo(formatter.Method,
					new MorestachioFormatterAttribute(name, "")
					{
						IsSourceObjectAware = false,
						LinkFunctionTarget = false,
						OutputType = typeof(void),
					}),
				new AnonFormatterComposingResult(formatter));
		}

		private class AnonFormatterComposingResult : IPrepareFormatterComposingResult
		{
			private readonly Func<object> _method;

			public AnonFormatterComposingResult(Func<object> method)
			{
				_method = method;
				Arguments = new Dictionary<MultiFormatterInfo, FormatterArgumentMap>();
			}

			/// <inheritdoc />
			public IDictionary<MultiFormatterInfo, FormatterArgumentMap> Arguments { get; }

			/// <inheritdoc />
			public (Func<object, object[], object>, MethodInfo) PrepareInvoke(object[] arguments)
			{
				return ((source, arguments) => _method(), _method.Method);
			}
		}

		/// <summary>
		///		Should provide a delegate to be invoked for the matching criteria.
		///		Does not perform a argument matching.
		/// </summary>
		/// <param name="type">The requested type to be formatted.</param>
		/// <param name="arguments">The arguments that were written in the template.</param>
		/// <param name="name">The name of the formatter to be invoked.</param>
		/// <param name="parserOptions"></param>
		/// <param name="scopeData"></param>
		/// <returns></returns>
		public abstract Func<object> GetDynamicFormatterUnsafe(Type type,
																FormatterArgumentType[] arguments,
																string name,
																ParserOptions parserOptions,
																ScopeData scopeData);
	}
}