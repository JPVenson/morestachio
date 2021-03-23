using Morestachio.Document;

namespace Morestachio
{
	/// <summary>
	///		Declares an IDocumentItem to support Delegate generation
	/// </summary>
	public interface ISupportCustomCompilation
	{
		///  <summary>
		/// 		Should return a delegate for performing the main rendering task
		///  </summary>
		///  <param name="compiler"></param>
		///  <returns></returns>
		Compilation Compile(IDocumentCompiler compiler);
	}
}