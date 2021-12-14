using Morestachio.Document.Contracts;
using Morestachio.Framework.Context;
using Morestachio.Framework.IO;

namespace Morestachio.Document.Custom;

/// <summary>
/// 
/// </summary>
public delegate ItemExecutionPromise BlockDocumentProviderFunction(
	IByteCounterStream outputStream,
	ContextObject context,
	ScopeData scopeData,
	string value,
	IEnumerable<IDocumentItem> children);