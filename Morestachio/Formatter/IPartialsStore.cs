namespace Morestachio.Formatter
{
	/// <summary>
	///		Allows to store Partials for multiple Runs
	/// </summary>
	public interface IPartialsStore
	{
		/// <summary>
		///		Adds the Parsed Partial to the store
		/// </summary>
		void AddParsedPartial(MorestachioDocumentInfo document, string name);

		/// <summary>
		///		Removes the Partial from the List of Known Partials
		/// </summary>
		/// <param name="name"></param>
		void RemovePartial(string name);

		/// <summary>
		///		Obtains the Partial if known
		/// </summary>
		/// <param name="name"></param>
		MorestachioDocumentInfo GetPartial(string name);

		/// <summary>
		///		Obtains the Partial if known
		/// </summary>
		/// <param name="name"></param>
		string[] GetNames();
	}
}