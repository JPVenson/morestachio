using System.Collections.Generic;
using System.Threading.Tasks;
using Morestachio.Document.Contracts;
using Morestachio.Framework;

namespace Morestachio.Document.Custom
{
	public delegate Task<IEnumerable<DocumentItemExecution>> BlockDocumentProviderFunction(
		IByteCounterStream outputStream,
		ContextObject context,
		ScopeData scopeData,
		string value,
		IEnumerable<IDocumentItem> children);
}