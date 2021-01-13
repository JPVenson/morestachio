using System.Threading.Tasks;

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
		/// <param name="name"></param>
		Task<MorestachioDocumentInfo> GetPartialAsync(string name, ParserOptions parserOptions);

		/// <summary>
		///		Gets the list of all known partials
		/// </summary>
		Task<string[]> GetNamesAsync(ParserOptions parserOptions);
	}
}