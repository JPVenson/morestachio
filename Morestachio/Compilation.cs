using System.Threading.Tasks;
using Morestachio.Document;
using Morestachio.Document.Contracts;
using Morestachio.Framework.Context;
using Morestachio.Framework.IO;
namespace Morestachio
{
	/// <summary>
	///		Delegate for the result of an Compile() call of an <see cref="IDocumentItem"/> that implements <see cref="ISupportCustomAsyncCompilation"/>
	/// </summary>
	/// <param name="outputStream"></param>
	/// <param name="context"></param>
	/// <param name="scopeData"></param>
	/// <returns></returns>
	public delegate Promise CompilationAsync(IByteCounterStream outputStream,
		ContextObject context,
		ScopeData scopeData);

	/// <summary>
	///		Delegate for the result of an Compile() call of an <see cref="IDocumentItem"/> that implements <see cref="ISupportCustomCompilation"/>
	/// </summary>
	/// <param name="outputStream"></param>
	/// <param name="context"></param>
	/// <param name="scopeData"></param>
	/// <returns></returns>
	public delegate void Compilation(IByteCounterStream outputStream,
		ContextObject context,
		ScopeData scopeData);
}