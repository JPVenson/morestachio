using System;
using System.Collections.Generic;
using System.Reflection;

namespace Morestachio.Formatter.Framework
{
	/// <summary>
	/// </summary>
	public class PrepareFormatterComposingResult
	{
		/// <summary>
		/// 
		/// </summary>
		public PrepareFormatterComposingResult(
			 Func<object[], MethodInfo> methodInfo, 
			 IDictionary<MultiFormatterInfo, FormatterArgumentMap> arguments)
		{
			MethodInfo = methodInfo;
			Arguments = arguments;
		}

		/// <summary>
		///     The Result Method of the Composing operation. It can be different from the original.
		/// </summary>
		
		public Func<object[], MethodInfo> MethodInfo { get; }

		private MethodInfo _methodInfo;

		/// <summary>
		///		Gets an compiled Method info
		/// </summary>
		/// <param name="arguments"></param>
		/// <returns></returns>
		public MethodInfo PrepareInvoke(object[] arguments)
		{
			return _methodInfo ?? (_methodInfo = MethodInfo(arguments));
		}
		
		/// <summary>
		///     The list of arguments for the <see cref="MethodInfo" />
		/// </summary>
		
		public IDictionary<MultiFormatterInfo, FormatterArgumentMap> Arguments { get; }
	}
}