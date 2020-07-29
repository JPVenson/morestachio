using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace Morestachio.Formatter.Framework
{
	/// <summary>
	/// </summary>
	public class PrepareFormatterComposingResult
	{
		public PrepareFormatterComposingResult(
			[NotNull] Func<object[], MethodInfo> methodInfo, 
			[NotNull] IDictionary<MultiFormatterInfo, FormatterArgumentMap> arguments)
		{
			MethodInfo = methodInfo;
			Arguments = arguments;
		}

		/// <summary>
		///     The Result Method of the Composing operation. It can be different from the original.
		/// </summary>
		[NotNull]
		public Func<object[], MethodInfo> MethodInfo { get; }

		private MethodInfo _methodInfo;

		public MethodInfo PrepareInvoke(object[] arguments)
		{
			return _methodInfo ?? (_methodInfo = MethodInfo(arguments));
		}
		
		/// <summary>
		///     The list of arguments for the <see cref="MethodInfo" />
		/// </summary>
		[NotNull]
		public IDictionary<MultiFormatterInfo, FormatterArgumentMap> Arguments { get; }
	}

	public class FormatterArgumentMap
	{
		public FormatterArgumentMap(int codeIndex, int? argumentIndex)
		{
			CodeParameterIndex = codeIndex;
			ParameterIndex = argumentIndex;
		}

		public int CodeParameterIndex { get; }
		public int? ParameterIndex { get; }
		public Func<object, object> ConverterFunc { get; set; }
		public ObtainValue ObtainValue { get; set; }
	}

	public delegate object ObtainValue(object sourceObject, FormatterArgumentType[] arguments);
}