using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items;
using Morestachio.Document.Items.Base;
using Morestachio.Framework.Context;
using Morestachio.Framework.IO;
using Morestachio.Helper;
using Morestachio.Rendering;
#if ValueTask
using Promise = System.Threading.Tasks.ValueTask;
#else
using Promise = System.Threading.Tasks.Task;
#endif

namespace Morestachio.Document
{
	/// <summary>
	///		Compiles a tree of <see cref="IDocumentItem"/> into an chain of delegates
	/// </summary>
	public interface IDocumentCompiler
	{
		/// <summary>
		///		Compile a <see cref="IDocumentItem"/> into a delegate
		/// </summary>
		/// <returns></returns>
		CompilationAsync Compile(IDocumentItem document);

		/// <summary>
		///		Compile a number of <see cref="IDocumentItem"/> into a delegate 
		/// </summary>
		/// <returns></returns>
		CompilationAsync Compile(IEnumerable<IDocumentItem> documents);
	}

	/// <summary>
	///		Compiles a tree of <see cref="IDocumentItem"/> into an chain of delegates
	/// </summary>
	public class DocumentCompiler : IDocumentCompiler
	{
		/// <inheritdoc />
		public CompilationAsync Compile(IDocumentItem document)
		{
			return CompileItemsAndChildren(new[] { document });
		}

		/// <inheritdoc />
		public CompilationAsync Compile(IEnumerable<IDocumentItem> documents)
		{
			return CompileItemsAndChildren(documents);
		}

		/// <summary>
		///		Compiles all <see cref="IDocumentItem"/> and their children. If the <see cref="IDocumentItem"/> supports the <see cref="ISupportCustomAsyncCompilation"/> it is used otherwise
		///		the items <see cref="IDocumentItem.Render"/> method is wrapped
		/// </summary>
		/// <param name="documentItems"></param>
		/// <returns></returns>
		public CompilationAsync CompileItemsAndChildren(IEnumerable<IDocumentItem> documentItems)
		{
			var docs = documentItems.ToArray();
			var actions = new Delegate[docs.Length];

			for (var index = 0; index < docs.Length; index++)
			{
				var documentItem = docs[index];
				var document = documentItem;
				if (document is ISupportCustomAsyncCompilation customAsyncCompilation)
				{
					actions[index] = (customAsyncCompilation.Compile(this));
				}
				else if (document is ISupportCustomCompilation customCompilation)
				{
					actions[index] = (customCompilation.Compile(this));
				}
				else
				{
					actions[index] = new CompilationAsync((async (outputStream,
						context,
						scopeData) =>
					{
						var children = await document.Render(outputStream, context, scopeData);

						foreach (var documentItemExecution in children)
						{
							await MorestachioDocument.ProcessItemsAndChildren(new[]
							{
								documentItemExecution.DocumentItem
							}, outputStream, documentItemExecution.ContextObject, scopeData);
						}
					}));
				}
			}

			return async (stream, context, data) =>
			{
				if (!data.IsOutputLimited)
				{
					for (int i = 0; i < docs.Length; i++)
					{
						var action = actions[i];
						if (action is CompilationAsync ca)
						{
							await ca(stream, context, data);
						}
						else if (action is Compilation c)
						{
							c(stream, context, data);
						}
					}
				}
				else
				{
					for (int i = 0; i < docs.Length; i++)
					{
						if (DocumentItemBase.ContinueBuilding(stream, data))
						{
							return;
						}
						var action = actions[i];
						if (action is CompilationAsync ca)
						{
							await ca(stream, context, data);
						}
						else if (action is Compilation c)
						{
							c(stream, context, data);
						}
					}	
				}
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
}
