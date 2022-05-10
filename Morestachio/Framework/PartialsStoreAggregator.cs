using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;


#if ValueTask
using StringArrayPromise = System.Threading.Tasks.ValueTask<string[]>;
#else
using StringArrayPromise = System.Threading.Tasks.Task<string[]>;
#endif

namespace Morestachio.Framework
{
	/// <summary>
	///		A Partials store that can aggregate multiple partial stores
	/// </summary>
	public class PartialsStoreAggregator : IAsyncPartialsStore
	{
		/// <summary>
		/// 
		/// </summary>
		public PartialsStoreAggregator()
		{
			PartialsStores = new List<IPartialsStore>();
		}

		/// <summary>
		///		The list of <see cref="IPartialsStore"/> or <see cref="IAsyncPartialsStore"/>
		/// </summary>
		public IList<IPartialsStore> PartialsStores { get; private set; }

		private IDictionary<string, IPartialsStore> _knownPartials;

		/// <inheritdoc />
		public bool IsSealed { get; private set; }
		
		/// <inheritdoc />
		public void Seal()
		{
			IsSealed = true;
			PartialsStores = new ReadOnlyCollection<IPartialsStore>(PartialsStores);
			foreach (var partialsStore in PartialsStores)
			{
				partialsStore.Seal();
			}
		}

		/// <inheritdoc />
		public MorestachioDocumentInfo GetPartial(string name, ParserOptions parserOptions)
		{
			throw new System.NotImplementedException();
		}
		
		/// <inheritdoc />
		public string[] GetNames(ParserOptions parserOptions)
		{
			throw new System.NotImplementedException();
		}
		
		/// <inheritdoc />
		public async MorestachioDocumentInfoPromise GetPartialAsync(string name, ParserOptions parserOptions)
		{
			if (_knownPartials.TryGetValue(name, out var store))
			{
				if (store is IAsyncPartialsStore asyncPartialsStore)
				{
					return await asyncPartialsStore.GetPartialAsync(name, parserOptions);
				}

				return store.GetPartial(name, parserOptions);
			}

			return null;
		}
		
		/// <inheritdoc />
		public async StringArrayPromise GetNamesAsync(ParserOptions parserOptions)
		{
			if (_knownPartials != null)
			{
				return _knownPartials.Keys.ToArray();
			}

			_knownPartials = new Dictionary<string, IPartialsStore>();
			foreach (var partialsStore in PartialsStores)
			{
				string[] names = null;
				if (partialsStore is IAsyncPartialsStore asyncPartialsStore)
				{
					names = await asyncPartialsStore.GetNamesAsync(parserOptions);
				}
				else
				{
					names = partialsStore.GetNames(parserOptions);
				}

				foreach (var name in names)
				{
					_knownPartials[name] = partialsStore;
				}
			}
			return _knownPartials.Keys.ToArray();
		}
	}
}