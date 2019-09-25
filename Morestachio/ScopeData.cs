using System.Collections.Generic;
using Morestachio.Document;
using Morestachio.Document.Contracts;
using Morestachio.Framework;

namespace Morestachio
{
	/// <summary>
	///		Hosts all infos about the current execution path of a part in the Template.
	///		Can be used for future parallel execution
	/// </summary>
	public class ScopeData
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ScopeData"/> class.
		/// </summary>
		public ScopeData()
		{
			Partials = new Dictionary<string, IDocumentItem>();
			PartialDepth = new Stack<string>();
			Alias = new Dictionary<string, ContextObject>();
		}

		/// <summary>
		///		List of all Partials
		/// </summary>
		public IDictionary<string, IDocumentItem> Partials { get; private set; }

		/// <summary>
		///		The Depth of current Partial usage
		/// </summary>
		public Stack<string> PartialDepth { get; private set; }

		/// <summary>
		///		Lists all Alias objects
		/// </summary>
		public IDictionary<string, ContextObject> Alias { get; set; }

		/// <summary>
		///		Will be set by any preceding If statement if the expression was not rendered to true
		/// </summary>
		public bool ExecuteElse { get; set; }
	}
}