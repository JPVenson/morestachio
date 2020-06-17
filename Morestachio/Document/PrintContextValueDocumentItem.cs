using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Document.Visitor;
using Morestachio.Framework;

namespace Morestachio.Document
{
	/// <summary>
	///		If a formatter ends without using its value it is printed
	/// </summary>
	[System.Serializable]
	public class PrintContextValueDocumentItem : DocumentItemBase, IValueDocumentItem
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal PrintContextValueDocumentItem()
		{

		}

		/// <inheritdoc />
		[UsedImplicitly]
		protected PrintContextValueDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
		}

		/// <inheritdoc />
		public override string Kind { get; } = "PrintExpression";

		/// <inheritdoc />
		public override async Task<IEnumerable<DocumentItemExecution>> Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			if (context != null)
			{
				string value = null;
				await context.EnsureValue();
				if (context.Value != null)
				{
					value = await context.RenderToString();
				}

				outputStream.Write(value);
			}
			
			return Children.WithScope(context);
		}

		/// <inheritdoc />
		public async Task<ContextObject> GetValue(ContextObject context, ScopeData scopeData)
		{
			await Task.CompletedTask;
			return context;
		}
		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			throw new NotImplementedException();
		}
	}
}