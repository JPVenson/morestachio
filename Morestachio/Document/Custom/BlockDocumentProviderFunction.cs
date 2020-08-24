using System.Collections.Generic;
using Morestachio.Document.Contracts;
using Morestachio.Framework.Context;
using Morestachio.Framework.IO;
#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using Promise = System.Threading.Tasks.ValueTask;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using Promise = System.Threading.Tasks.Task;
#endif
namespace Morestachio.Document.Custom
{
	/// <summary>
	/// 
	/// </summary>
	public delegate ItemExecutionPromise BlockDocumentProviderFunction(
		IByteCounterStream outputStream,
		ContextObject context,
		ScopeData scopeData,
		string value,
		IEnumerable<IDocumentItem> children);
}