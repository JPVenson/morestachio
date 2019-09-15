using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Framework;

namespace Morestachio.Document
{
	/// <summary>
	///		Defines a document that can be rendered. Does only store its Children
	/// </summary>
	[System.Serializable]
	public class MorestachioDocument : DocumentItemBase
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal MorestachioDocument()
		{

		}

		[UsedImplicitly]
		protected MorestachioDocument(SerializationInfo info, StreamingContext c) : base(info, c)
		{
		}

		/// <inheritdoc />
		public override string Kind { get; } = "Document";

		/// <inheritdoc />
		public override async Task<IEnumerable<DocumentItemExecution>> Render(IByteCounterStream outputStream, ContextObject context,
			ScopeData scopeData)
		{
			await Task.CompletedTask;
			return Children.WithScope(context);
		}

		/// <summary>
		/// Processes the items and children.
		/// </summary>
		/// <param name="documentItems">The document items.</param>
		/// <param name="outputStream">The output stream.</param>
		/// <param name="context">The context.</param>
		/// <param name="scopeData">The scope data.</param>
		/// <returns></returns>
		public static async Task ProcessItemsAndChildren(IEnumerable<IDocumentItem> documentItems, IByteCounterStream outputStream, ContextObject context,
			ScopeData scopeData)
		{
			//we do NOT use a recursive loop to avoid stack overflows. 

			var processStack = new Stack<DocumentItemExecution>(); //deep search. create a stack to go deeper into the tree without loosing work left on other branches

			foreach (var documentItem in documentItems.TakeWhile(e => ContinueBuilding(outputStream, context))) //abort as soon as the cancellation is requested OR the template size is reached
			{
				processStack.Push(new DocumentItemExecution(documentItem, context));
				while (processStack.Any() && ContinueBuilding(outputStream, context))
				{
					var currentDocumentItem = processStack.Pop();//take the current branch
					var next = await currentDocumentItem.DocumentItem.Render(outputStream, currentDocumentItem.ContextObject, scopeData);
					foreach (var item in next.Reverse()) //we have to reverse the list as the logical first item returned must be the last inserted to be the next that pops out
					{
						processStack.Push(item);
					}
				}
			}
		}
	}
}