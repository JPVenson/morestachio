using System.Threading;
using Morestachio.Document;
using Morestachio.Document.Contracts;
using Morestachio.Framework.IO;

namespace Morestachio.Rendering;

/// <summary>
///		Uses a <see cref="DocumentCompiler"/> to compile a DocumentTree into delegates
/// </summary>
public class CompiledRenderer : Renderer
{
	private readonly IDocumentCompiler _compiler;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="document"></param>
	/// <param name="parserOptions"></param>
	/// <param name="captureVariables"></param>
	/// <param name="compiler"></param>
	public CompiledRenderer(IDocumentItem document,
							ParserOptions parserOptions,
							bool captureVariables,
							IDocumentCompiler compiler)
		: base(document, parserOptions, captureVariables)
	{
		_compiler = compiler;
	}

	private CompilationAsync CompiledDocument { get; set; }

	/// <inheritdoc />
	public override void PreRender()
	{
		if (CompiledDocument == null)
		{
			PreCompile();
		}
	}

	/// <summary>
	///		Compiles the <see cref="Renderer.Document"/> and stores the result
	/// </summary>
	public void PreCompile()
	{
		CompiledDocument = _compiler.Compile(Document, ParserOptions);
	}

	/// <inheritdoc />
	public override async MorestachioDocumentResultPromise RenderAsync(object data,
																		CancellationToken cancellationToken,
																		IByteCounterStream targetStream = null)
	{
		return await Render(data, cancellationToken,
			async (stream, context, scopeData) =>
			{
				await CompiledDocument(stream, context, scopeData).ConfigureAwait(false);
			}, targetStream).ConfigureAwait(false);
	}
}