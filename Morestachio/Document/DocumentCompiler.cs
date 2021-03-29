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
		Compilation Compile(IDocumentItem document);

		/// <summary>
		///		Compile a number of <see cref="IDocumentItem"/> into a delegate 
		/// </summary>
		/// <returns></returns>
		Compilation Compile(IEnumerable<IDocumentItem> documents);
	}

	/// <summary>
	///		Compiles a tree of <see cref="IDocumentItem"/> into an chain of delegates
	/// </summary>
	public class DocumentCompiler : IDocumentCompiler
	{
		/// <inheritdoc />
		public Compilation Compile(IDocumentItem document)
		{
			return CompileItemsAndChildren(new[] { document });
		}

		/// <inheritdoc />
		public Compilation Compile(IEnumerable<IDocumentItem> documents)
		{
			return CompileItemsAndChildren(documents);
		}

		/// <summary>
		///		Compiles all <see cref="IDocumentItem"/> and their children. If the <see cref="IDocumentItem"/> supports the <see cref="ISupportCustomCompilation"/> it is used otherwise
		///		the items <see cref="IDocumentItem.Render"/> method is wrapped
		/// </summary>
		/// <param name="documentItems"></param>
		/// <returns></returns>
		public Compilation CompileItemsAndChildren(IEnumerable<IDocumentItem> documentItems)
		{
			var docs = documentItems.ToArray();
			var actions = new Compilation[docs.Length];

			for (var index = 0; index < docs.Length; index++)
			{
				var documentItem = docs[index];
				var document = documentItem;
				if (document is ISupportCustomCompilation customCompilation)
				{
					actions[index] = (customCompilation.Compile(this));
				}
				else
				{
					actions[index] = (async (outputStream,
						context,
						scopeData) =>
					{
						var children = await document.Render(outputStream, context, scopeData);

						foreach (var documentItemExecution in children)
						{
							await MorestachioDocument.ProcessItemsAndChildren(new []
							{
								documentItemExecution.DocumentItem
							}, outputStream, documentItemExecution.ContextObject, scopeData);
						}
					});
				}
			}

			return FastExecuteItems(actions);
		}

		private static Compilation FastExecuteItems(Compilation[] actions)
		{
			async Promise ExecuteTenItems(IByteCounterStream stream, ContextObject context, ScopeData data)
			{
				if (!DocumentItemBase.ContinueBuilding(stream, data))
				{
					return;
				}

				await actions[0](stream, context, data);
				if (!DocumentItemBase.ContinueBuilding(stream, data))
				{
					return;
				}

				await actions[1](stream, context, data);
				if (!DocumentItemBase.ContinueBuilding(stream, data))
				{
					return;
				}

				await actions[2](stream, context, data);
				if (!DocumentItemBase.ContinueBuilding(stream, data))
				{
					return;
				}

				await actions[3](stream, context, data);
				if (!DocumentItemBase.ContinueBuilding(stream, data))
				{
					return;
				}

				await actions[4](stream, context, data);
				if (!DocumentItemBase.ContinueBuilding(stream, data))
				{
					return;
				}

				await actions[5](stream, context, data);
				if (!DocumentItemBase.ContinueBuilding(stream, data))
				{
					return;
				}

				await actions[6](stream, context, data);
				if (!DocumentItemBase.ContinueBuilding(stream, data))
				{
					return;
				}

				await actions[7](stream, context, data);
				if (!DocumentItemBase.ContinueBuilding(stream, data))
				{
					return;
				}

				await actions[8](stream, context, data);
				if (!DocumentItemBase.ContinueBuilding(stream, data))
				{
					return;
				}

				await actions[9](stream, context, data);
			}

			if (actions.Length == 0)
			{
				return NopAction;
			}
			else if (actions.Length == 1)
			{
				return async (stream, context, data) =>
				{
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[0](stream, context, data);
				};
			}
			else if (actions.Length == 2)
			{
				return async (stream, context, data) =>
				{
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[0](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[1](stream, context, data);
				};
			}
			else if (actions.Length == 3)
			{
				return async (stream, context, data) =>
				{
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[0](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[1](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[2](stream, context, data);
				};
			}
			else if (actions.Length == 4)
			{
				return async (stream, context, data) =>
				{
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[0](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[1](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[2](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[3](stream, context, data);
				};
			}
			else if (actions.Length == 5)
			{
				return async (stream, context, data) =>
				{
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[0](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[1](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[2](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[3](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[4](stream, context, data);
				};
			}
			else if (actions.Length == 6)
			{
				return async (stream, context, data) =>
				{
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[0](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[1](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[2](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[3](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[4](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[5](stream, context, data);
				};
			}
			else if (actions.Length == 7)
			{
				return async (stream, context, data) =>
				{
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[0](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[1](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[2](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[3](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[4](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[5](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[6](stream, context, data);
				};
			}
			else if (actions.Length == 8)
			{
				return async (stream, context, data) =>
				{
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[0](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[1](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[2](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[3](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[4](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[5](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[6](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[7](stream, context, data);
				};
			}
			else if (actions.Length == 9)
			{
				return async (stream, context, data) =>
				{
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[0](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[1](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[2](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[3](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[4](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[5](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[6](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[7](stream, context, data);
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[8](stream, context, data);
				};
			}
			else
			{
				if (actions.Length == 10)
				{
					return ExecuteTenItems;
				}
			}

			return async (stream, context, data) =>
			{
				await ExecuteTenItems(stream, context, data);
				for (int i = 10; i < actions.Length; i++)
				{
					if (!DocumentItemBase.ContinueBuilding(stream, data))
					{
						return;
					}
					await actions[i](stream, context, data);	
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
