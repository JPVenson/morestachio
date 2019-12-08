using System;
using System.Collections.Generic;
using System.Linq;
using Morestachio.Document;
using Morestachio.Document.Contracts;
using Morestachio.Framework;

namespace Morestachio
{
	/// <summary>
	///		Hosts all infos about the current execution path of a part in the Template.
	///		Can be used for future parallel execution
	/// </summary>
	public class ScopeData : IDisposable
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ScopeData"/> class.
		/// </summary>
		public ScopeData()
		{
			Partials = new Dictionary<string, IDocumentItem>();
			PartialDepth = new Stack<string>();
			Alias = new Dictionary<string, ContextObject>();
			CustomData = new Dictionary<string, object>();
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
		public IDictionary<string, ContextObject> Alias { get; private  set; }

		/// <summary>
		///		Can be used by 3rd party document items to store data.
		///		If it contains an IDisposable it will be disposed after the execution is finished.
		/// </summary>
		public IDictionary<string, object> CustomData { get; set; }

		/// <summary>
		///		Will be set by any preceding If statement if the expression was not rendered to true
		/// </summary>
		public bool ExecuteElse { get; set; }

		/// <inheritdoc />
		public void Dispose()
		{
			foreach (var disposable in CustomData.OfType<IDisposable>())
			{
				disposable.Dispose();
			}
		}
	}
}