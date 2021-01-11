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
	///		Delegate for the Tagdocument 
	/// </summary>
	/// <param name="outputStream"></param>
	/// <param name="context"></param>
	/// <param name="scopeData"></param>
	/// <param name="value"></param>
	/// <returns></returns>
	public delegate Promise TagDocumentProviderFunction(
		IByteCounterStream outputStream,
		ContextObject context,
		ScopeData scopeData,
		string value,
		string tag);
}