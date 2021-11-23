using Morestachio.Framework.Context;
using Morestachio.Framework.IO;

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