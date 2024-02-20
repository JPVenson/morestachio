using System.Collections.Generic;
using System.IO;
using Morestachio.Framework.IO;
using Morestachio.Profiler;

namespace Morestachio;

/// <summary>
///		The Compiled template
/// </summary>
public class MorestachioDocumentResult
{
	internal MorestachioDocumentResult(IByteCounterStream stream,
										PerformanceProfiler profiler,
										IDictionary<string, object> variables)
	{
		Stream = stream;
		Profiler = profiler;
		CapturedVariables = variables;
	}

	internal MorestachioDocumentResult(IByteCounterStream stream,
										PerformanceProfiler profiler)
	{
		Stream = stream;
		Profiler = profiler;
	}

	/// <summary>
	///		The Result of the CreateAsync call
	/// </summary>
	public IByteCounterStream Stream { get; }

	/// <summary>
	///		[Experimental]
	/// </summary>
	public PerformanceProfiler Profiler { get; }

	/// <summary>
	///		If enabled by <see cref="MorestachioDocumentInfo.CaptureVariables"/> will contain all global variables set by the template
	/// </summary>
	public IDictionary<string, object> CapturedVariables { get; }
}