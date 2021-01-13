namespace Morestachio
{
	/// <summary>
	///		Declares an IDocumentItem to support Delegate generation
	/// </summary>
	public interface ISupportCustomCompilation
	{
		/// <summary>
		///		Should return a delegate for performing the main rendering task
		/// </summary>
		/// <returns></returns>
		Compilation Compile();
	}
}