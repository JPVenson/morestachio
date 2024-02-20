﻿using System.Threading;
using Morestachio.Document.Contracts;
using Morestachio.Framework.IO;

namespace Morestachio.Rendering;

/// <summary>
///		Contains all logic for invoking the Document items
/// </summary>
public interface IRenderer
{
	/// <summary>
	///     The parser Options object that was used to create the Template Delegate
	/// </summary>
	ParserOptions ParserOptions { get; }


	/// <summary>
	///		The Morestachio Document generated by the <see cref="Parser"/>
	/// </summary>
	IDocumentItem Document { get; }

	/// <summary>
	///		Will be invoked before the rendering step
	/// </summary>
	void PreRender();

	/// <summary>
	///		Will be invoked after the rendering
	/// </summary>
	void PostRender();

	/// <summary>
	///		Renders a document with the given object
	/// </summary>
	MorestachioDocumentResultPromise RenderAsync(object data,
												CancellationToken cancellationToken,
												IByteCounterStream targetStream = null);
}

/// <summary>
///		Extends the <see cref="IRenderer"/> for common use cases
/// </summary>
public static class RendererExtensions
{
	/// <summary>
	///		Renders the <see cref="IRenderer.Document"/> and awaits synchronously 
	/// </summary>
	public static MorestachioDocumentResult Render(this IRenderer renderer, object data, CancellationToken token)
	{
		return renderer.Render(data, token, null);
	}

	/// <summary>
	///		Renders the <see cref="IRenderer.Document"/> and awaits synchronously 
	/// </summary>
	public static MorestachioDocumentResult Render(this IRenderer renderer,
													object data,
													CancellationToken token,
													IByteCounterStream targetStream)
	{
		return renderer.RenderAsync(data, token, targetStream).Await();
	}

	/// <summary>
	///		Renders the <see cref="IRenderer.Document"/> and awaits synchronously 
	/// </summary>
	public static MorestachioDocumentResult Render(this IRenderer renderer, object data)
	{
		return renderer.Render(data, CancellationToken.None);
	}

	/// <summary>
	///		Renders the <see cref="IRenderer.Document"/> and awaits synchronously 
	/// </summary>
	public static MorestachioDocumentResult Render(this IRenderer renderer,
													object data,
													IByteCounterStream targetStream)
	{
		return renderer.Render(data, CancellationToken.None, targetStream);
	}

	/// <summary>
	///		Renders the <see cref="IRenderer.Document"/>
	/// </summary>
	public static MorestachioDocumentResultPromise RenderAsync(this IRenderer renderer, object data)
	{
		return renderer.RenderAsync(data, CancellationToken.None);
	}

	/// <summary>
	///		Renders the <see cref="IRenderer.Document"/>
	/// </summary>
	public static MorestachioDocumentResultPromise RenderAsync(this IRenderer renderer,
																object data,
																IByteCounterStream targetStream)
	{
		return renderer.RenderAsync(data, CancellationToken.None, targetStream);
	}

	/// <summary>
	///		Renders the <see cref="IRenderer.Document"/> and converts the output stream to a string by using the <see cref="ParserOptions.Encoding"/>
	/// </summary>
	public static async StringPromise RenderAndStringifyAsync(this IRenderer renderer,
															object data,
															CancellationToken token)
	{
		using (var stream = (await renderer.RenderAsync(data, token).ConfigureAwait(false)).Stream)
		{
			return stream.Stringify(true, renderer.ParserOptions.Encoding);
		}
	}

	/// <summary>
	///		Renders the <see cref="IRenderer.Document"/> and converts the output stream to a string by using the <see cref="ParserOptions.Encoding"/>
	/// </summary>
	public static StringPromise RenderAndStringifyAsync(this IRenderer renderer, object data)
	{
		return renderer.RenderAndStringifyAsync(data, CancellationToken.None);
	}

	/// <summary>
	///		Renders the <see cref="IRenderer.Document"/> and converts the output stream to a string by using the <see cref="ParserOptions.Encoding"/>
	/// </summary>
	public static string RenderAndStringify(this IRenderer renderer, object data, CancellationToken token)
	{
		using (var stream = (renderer.Render(data, token)).Stream)
		{
			return stream.Stringify(true, renderer.ParserOptions.Encoding);
		}
	}

	/// <summary>
	///		Renders the <see cref="IRenderer.Document"/> and converts the output stream to a string by using the <see cref="ParserOptions.Encoding"/>
	/// </summary>
	public static string RenderAndStringify(this IRenderer renderer, object data)
	{
		return renderer.RenderAndStringify(data, CancellationToken.None);
	}
}