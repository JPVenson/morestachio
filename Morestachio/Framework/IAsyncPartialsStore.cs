using System.Threading.Tasks;
#if ValueTask
using MorestachioDocumentInfoPromise = System.Threading.Tasks.ValueTask<Morestachio.MorestachioDocumentInfo>;
using StringArrayPromise = System.Threading.Tasks.ValueTask<string[]>;
#else
using MorestachioDocumentInfoPromise = System.Threading.Tasks.Task<Morestachio.MorestachioDocumentInfo>;
using StringArrayPromise = System.Threading.Tasks.Task<string[]>;
#endif

namespace Morestachio.Framework
{
	/// <summary>
	///		Allows to store Partials for multiple Runs
	/// </summary>
	public interface IAsyncPartialsStore : IPartialsStore
	{
		/// <summary>
		///		Obtains the Partial if known
		/// </summary>
		MorestachioDocumentInfoPromise GetPartialAsync(string name, ParserOptions parserOptions);

		/// <summary>
		///		Gets the list of all known partials
		/// </summary>
		StringArrayPromise GetNamesAsync(ParserOptions parserOptions);
	}
}