using System.Collections.Generic;
using System.IO;

namespace Morestachio
{
	/// <summary>
	///		The Compiled template
	/// </summary>
	public class MorestachioDocumentResult
	{
		internal MorestachioDocumentResult(Stream stream, 
			PerformanceProfiler profiler,
			IDictionary<string, object> variables)
		{
			Stream = stream;
			Profiler = profiler;
			CapturedVariables = variables;
		}

		internal MorestachioDocumentResult(Stream stream, 
			PerformanceProfiler profiler)
		{
			Stream = stream;
			Profiler = profiler;
		}

		/// <summary>
		///		The Result of the CreateAsync call
		/// </summary>
		public Stream Stream { get; }

		/// <summary>
		///		[Experimental]
		/// </summary>
		public PerformanceProfiler Profiler { get; }

		/// <summary>
		///		If enabled by <see cref="MorestachioDocumentInfo.CaptureVariables"/> will contain all global variables set by the template
		/// </summary>
		public IDictionary<string, object> CapturedVariables { get; }
	}
}