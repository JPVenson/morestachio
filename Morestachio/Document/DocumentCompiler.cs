using Morestachio.Document.Contracts;
using Morestachio.Document.Items;
using Morestachio.Document.Items.Base;
using Morestachio.Framework.Context;
using Morestachio.Framework.IO;

namespace Morestachio.Document;

/// <summary>
///		Compiles a tree of <see cref="IDocumentItem"/> into an chain of delegates
/// </summary>
public interface IDocumentCompiler
{
	/// <summary>
	///		Compile a <see cref="IDocumentItem"/> into a delegate
	/// </summary>
	/// <returns></returns>
	CompilationAsync Compile(IDocumentItem document, ParserOptions parserOptions);

	/// <summary>
	///		Compile a number of <see cref="IDocumentItem"/> into a delegate 
	/// </summary>
	/// <returns></returns>
	CompilationAsync Compile(IList<IDocumentItem> documents, ParserOptions parserOptions);
}

/// <summary>
///		Compiles a tree of <see cref="IDocumentItem"/> into an chain of delegates
/// </summary>
public class DocumentCompiler : IDocumentCompiler
{
	/// <inheritdoc />
	public CompilationAsync Compile(IDocumentItem document, ParserOptions parserOptions)
	{
		var action = CompileSingle(parserOptions, document);

		return action switch
		{
			CompilationAsync ca => async (stream, context, data) =>
			{
				await ca(stream, context, data).ConfigureAwait(false);
			},
			Compilation c => (stream, context, data) =>
			{
				c(stream, context, data);

				return AsyncHelper.FakePromise();
			},
			_ => throw new NotImplementedException()
		};
	}

	/// <inheritdoc />
	public CompilationAsync Compile(IList<IDocumentItem> documents, ParserOptions parserOptions)
	{
		return documents.Count switch
		{
			0 => NopAction,
			1 => Compile(documents[0], parserOptions),
			_ => CompileItemsAndChildren(documents, parserOptions)
		};
	}

	/// <summary>
	///		Compiles all <see cref="IDocumentItem"/> and their children. If the <see cref="IDocumentItem"/> supports the <see cref="ISupportCustomAsyncCompilation"/> it is used otherwise
	///		the items <see cref="IDocumentItem.Render"/> method is wrapped
	/// </summary>
	public CompilationAsync CompileItemsAndChildren(IEnumerable<IDocumentItem> documentItems,
													ParserOptions parserOptions)
	{
		var docs = documentItems.ToArray();
		var actions = new Delegate[docs.Length];

		for (var index = 0; index < docs.Length; index++)
		{
			var documentItem = docs[index];
			actions[index] = CompileSingle(parserOptions, documentItem);
		}

		if (actions.All(e => e is CompilationAsync))
		{
			return async (stream, context, data) =>
			{
				if (!data.IsOutputLimited)
				{
					for (int i = 0; i < actions.Length; i++)
					{
						var action = (CompilationAsync)actions[i];
						await action(stream, context, data).ConfigureAwait(false);
					}
				}
				else
				{
					for (int i = 0; i < actions.Length; i++)
					{
						if (!DocumentItemBase.ContinueBuilding(stream, data))
						{
							return;
						}

						var action = (CompilationAsync)actions[i];
						await action(stream, context, data).ConfigureAwait(false);
					}
				}
			};
		}

		return async (stream, context, data) =>
		{
			if (!data.IsOutputLimited)
			{
				for (int i = 0; i < actions.Length; i++)
				{
					var action = actions[i];

					if (action is CompilationAsync ca)
					{
						await ca(stream, context, data).ConfigureAwait(false);
					}
					else if (action is Compilation c)
					{
						c(stream, context, data);
					}
				}
			}
			else
			{
				for (int i = 0; i < actions.Length; i++)
				{
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}

					var action = actions[i];

					if (action is CompilationAsync ca)
					{
						await ca(stream, context, data).ConfigureAwait(false);
					}
					else if (action is Compilation c)
					{
						c(stream, context, data);
					}
				}
			}
		};
	}

	private Delegate CompileSingle(ParserOptions parserOptions, IDocumentItem document)
	{
		return document switch
		{
			ISupportCustomAsyncCompilation customAsyncCompilation => (customAsyncCompilation.Compile(this,
				parserOptions)),
			ISupportCustomCompilation customCompilation => (customCompilation.Compile(this, parserOptions)),
			_ => new CompilationAsync((async (outputStream, context, scopeData) =>
			{
				var children = await document.Render(outputStream, context, scopeData).ConfigureAwait(false);

				foreach (var documentItemExecution in children)
				{
					await MorestachioDocument.ProcessItemsAndChildren(new[] { documentItemExecution.DocumentItem },
						outputStream, documentItemExecution.ContextObject, scopeData).ConfigureAwait(false);
				}
			}))
		};
	}

	/// <summary>
	///		Defines an option that does nothing
	/// </summary>
	/// <param name="outputStream"></param>
	/// <param name="context"></param>
	/// <param name="scopeData"></param>
	/// <returns></returns>
	public static Promise NopAction(IByteCounterStream outputStream,
									ContextObject context,
									ScopeData scopeData)
	{
		return AsyncHelper.FakePromise();
	}
}