using System.IO;

namespace Morestachio
{
	/// <summary>
	///		The Compiled template
	/// </summary>
	public class MorestachioDocumentResult
	{
		/// <summary>
		///		The Result of the CreateAsync call
		/// </summary>
		public Stream Stream { get; set; }

		/// <summary>
		///		[Experimental]
		/// </summary>
		public PerformanceProfiler Profiler { get; set; }
	}
}