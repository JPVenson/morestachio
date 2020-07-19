using System.Threading.Tasks;
using Morestachio.Framework;

namespace Morestachio.Document.Custom
{
	public delegate Task TagDocumentProviderFunction(
		IByteCounterStream outputStream,
		ContextObject context,
		ScopeData scopeData,
		string value);
}