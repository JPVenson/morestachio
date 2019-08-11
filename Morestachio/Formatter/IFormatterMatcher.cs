using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Morestachio.Formatter
{
	/// <summary>
	///		Can be used to Preprovide Partials in Templates. A template here will always be overwritten by a partial provided in the template
	/// </summary>
	public interface IPartialTemplateProvider
	{
		/// <summary>
		///		Obtains the Template from the store
		/// </summary>
		/// <returns></returns>
		ExternalPartialDeclaration[] GetTemplates();
	}

	/// <summary>
	///		Defines the contents of a Predifined Partial
	/// </summary>
	public class ExternalPartialDeclaration
	{
		/// <summary>
		///		The name of that partial
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		///		The Template Contents
		/// </summary>
		public string Template { get; set; }
	}

	/// <summary>
	///		Defines the Tools for Enumerating formatter based on the input type
	/// </summary>
	public interface IFormatterMatcher
	{
		/// <summary>
		///     Adds the formatter.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="formatterDelegate">The formatter delegate.</param>
		FormatTemplateElement AddFormatter<T>([NotNull] Delegate formatterDelegate);

		/// <summary>
		///     Adds the formatter.
		/// </summary>
		/// <param name="formatter">The formatter.</param>
		FormatTemplateElement AddFormatter([NotNull] FormatTemplateElement formatter);

		/// <summary>
		///     Adds the formatter.
		/// </summary>
		/// <param name="forType">For type.</param>
		/// <param name="formatterDelegate">The formatter delegate.</param>
		FormatTemplateElement AddFormatter([NotNull] Type forType, [NotNull] Delegate formatterDelegate);

		/// <summary>
		///     Executes the specified formatter.
		/// </summary>
		/// <param name="formatter">The formatter.</param>
		/// <param name="sourceObject">The source object.</param>
		/// <param name="templateArguments">The template arguments.</param>
		/// <returns></returns>
		Task<object> Execute([NotNull] FormatTemplateElement formatter,
			[CanBeNull] object sourceObject,
			params KeyValuePair<string, object>[] templateArguments);

		/// <summary>
		///     Gets the Formatter that matches the type or is assignable to that type. If null it will search for a object
		///     formatter
		/// </summary>
		/// <param name="type"></param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		IEnumerable<FormatTemplateElement> GetMostMatchingFormatter([CanBeNull] Type type,
			KeyValuePair<string, object>[] arguments);

		/// <summary>
		///     Searches for the first formatter does not reject the value.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="arguments">The arguments.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		Task<object> CallMostMatchingFormatter(Type type, KeyValuePair<string, object>[] arguments, object value);
	}
}