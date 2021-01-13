using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Morestachio.Framework
{
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
}