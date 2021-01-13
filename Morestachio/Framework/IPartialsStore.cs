using Morestachio.Util.Sealing;

#if ValueTask
using DocumentInfoPromise = System.Threading.Tasks.ValueTask<Morestachio.MorestachioDocumentInfo>;
using StringArrayPromise = System.Threading.Tasks.ValueTask<string[]>;
#else

#endif

namespace Morestachio.Framework
{
	/// <summary>
	///		Allows to store Partials for multiple Runs
	/// </summary>
	public interface IPartialsStore : ISealed
	{
		/// <summary>
		///		Obtains the Partial if known
		/// </summary>
		MorestachioDocumentInfo GetPartial(string name, ParserOptions parserOptions);

		/// <summary>
		///		Gets the list of all known partials
		/// </summary>
		string[] GetNames(ParserOptions parserOptions);
	}
}