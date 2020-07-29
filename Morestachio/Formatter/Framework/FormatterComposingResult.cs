using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace Morestachio.Formatter.Framework
{
	/// <summary>
	/// </summary>
	public struct FormatterComposingResult
	{
		/// <summary>
		///     The Result Method of the Composing operation. It can be different from the original.
		/// </summary>
		[NotNull]
		public MethodInfo MethodInfo { get; set; }

		/// <summary>
		///     The list of arguments for the <see cref="MethodInfo" />
		/// </summary>
		[NotNull]
		public IDictionary<MultiFormatterInfo, object> Arguments { get; set; }
	}
}