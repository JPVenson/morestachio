using JetBrains.Annotations;
using Morestachio.Util.Sealing;

namespace Morestachio.Framework
{
	/// <summary>
	///		Allows to store Partials for multiple Runs
	/// </summary>
	public interface IPartialsStore : ISealed
	{
		/// <summary>
		///		Adds the Parsed Partial to the store
		/// </summary>
		void AddParsedPartial([NotNull]MorestachioDocumentInfo document, [NotNull]string name);

		/// <summary>
		///		Removes the Partial from the List of Known Partials
		/// </summary>
		/// <param name="name"></param>
		void RemovePartial([NotNull]string name);

		/// <summary>
		///		Obtains the Partial if known
		/// </summary>
		/// <param name="name"></param>
		[CanBeNull] MorestachioDocumentInfo GetPartial([NotNull]string name);

		/// <summary>
		///		Gets the list of all known partials
		/// </summary>
		[NotNull, ItemNotNull] string[] GetNames();
	}
}