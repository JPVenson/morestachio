using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Morestachio.Util.Sealing;

#if ValueTask
using DocumentInfoPromise = System.Threading.Tasks.ValueTask<Morestachio.MorestachioDocumentInfo>;
using StringArrayPromise = System.Threading.Tasks.ValueTask<string[]>;
#else
using DocumentInfoPromise = System.Threading.Tasks.Task<Morestachio.MorestachioDocumentInfo>;
using StringArrayPromise = System.Threading.Tasks.Task<string[]>;
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
	
	/// <inheritdoc />
	public class ListPartialsStore : IPartialsStore
	{
		/// <summary>
		///		Creates a new Instance that can hold multiple partials
		/// </summary>
		public ListPartialsStore()
		{
			Partials = new Dictionary<string, MorestachioDocumentInfo>();
		}

		/// <summary>
		///		The Partials that are accessible from this Store
		/// </summary>
		public IDictionary<string, MorestachioDocumentInfo> Partials { get; private set; }

		/// <inheritdoc />
		public bool IsSealed { get; private set; }
		
		/// <inheritdoc />
		public void Seal()
		{
			IsSealed = true;
			Partials = new ReadOnlyDictionary<string, MorestachioDocumentInfo>(Partials);
		}
		
		/// <inheritdoc />
		public MorestachioDocumentInfo GetPartial(string name, ParserOptions parserOptions)
		{
			if (Partials.TryGetValue(name, out var documentInfo))
			{
				return documentInfo;
			}

			return null;
		}
		
		/// <inheritdoc />
		public string[] GetNames(ParserOptions parserOptions)
		{
			return Partials.Keys.ToArray();
		}
	}

	/// <summary>
	///		Allows to store Partials for multiple Runs
	/// </summary>
	public interface IAsyncPartialsStore : IPartialsStore
	{
		/// <summary>
		///		Obtains the Partial if known
		/// </summary>
		/// <param name="name"></param>
		DocumentInfoPromise GetPartialAsync(string name, ParserOptions parserOptions);

		/// <summary>
		///		Gets the list of all known partials
		/// </summary>
		StringArrayPromise GetNamesAsync(ParserOptions parserOptions);
	}
}