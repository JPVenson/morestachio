using System;
using System.Collections.Generic;
using System.Text;

namespace Morestachio.Formatter.Framework
{
	/// <summary>
	///		Marks an object to handle its own formatter invocation.
	/// </summary>
	public interface IFormatterInvoker
	{
		///  <summary>
		/// 		Checks the arguments for this formatter call to be handled by the <see cref="InvokeAsync"/> method.
		///  </summary>
		///  <param name="arguments">The list of arguments the formatter should be invoked with.</param>
		///  <param name="name">The name of the formatter.</param>
		///  <param name="parserOptions"></param>
		///  <returns>When returning true, the object will handle the next formatter call via the <see cref="InvokeAsync"/> method. When false the call is skipped and nothing happens.</returns>
		bool CanInvokeAsync(FormatterArgumentType[] arguments, string name, ParserOptions parserOptions);

		///  <summary>
		/// 		Should handle the tested call by <see cref="CanInvokeAsync"/>.
		///  </summary>
		///  <param name="arguments">The list of arguments the formatter for this invocation.</param>
		///  <param name="name">The name of the formatter to invoke</param>
		///  <param name="parserOptions"></param>
		///  <returns>Can return an value or null.</returns>
		ObjectPromise InvokeAsync(FormatterArgumentType[] arguments, string name, ParserOptions parserOptions);
	}
}
