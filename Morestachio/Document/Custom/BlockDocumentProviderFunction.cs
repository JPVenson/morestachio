using System.Collections.Generic;
using System.Threading.Tasks;
using Morestachio.Document.Contracts;
using Morestachio.Framework;

#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using Promise = System.Threading.Tasks.ValueTask;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using Promise = System.Threading.Tasks.Task;
#endif
namespace Morestachio.Document.Custom
{
	public delegate ItemExecutionPromise BlockDocumentProviderFunction(
		IByteCounterStream outputStream,
		ContextObject context,
		ScopeData scopeData,
		string value,
		IEnumerable<IDocumentItem> children);
}