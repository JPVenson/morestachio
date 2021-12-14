using Morestachio.Document;

namespace Morestachio;

/// <summary>
///		Declares an IDocumentItem to support Delegate generation
/// </summary>
public interface ISupportCustomAsyncCompilation
{
	///  <summary>
	/// 		Should return a delegate for performing the main rendering task
	///  </summary>
	///  <param name="compiler"></param>
	///  <param name="parserOptions"></param>
	///  <returns></returns>
	CompilationAsync Compile(IDocumentCompiler compiler, ParserOptions parserOptions);
}

/// <summary>
///		Declares an IDocumentItem to support Delegate generation
/// </summary>
public interface ISupportCustomCompilation
{
	///  <summary>
	/// 		Should return a delegate for performing the main rendering task
	///  </summary>
	///  <param name="compiler"></param>
	///  <param name="parserOptions"></param>
	///  <returns></returns>
	Compilation Compile(IDocumentCompiler compiler, ParserOptions parserOptions);
}