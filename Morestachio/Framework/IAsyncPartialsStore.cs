using System.Threading.Tasks;
#if ValueTask
using StringArrayPromise = System.Threading.Tasks.ValueTask<string[]>;
#else
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