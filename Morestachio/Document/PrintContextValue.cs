using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Framework;

namespace Morestachio.Document
{
	/// <summary>
	///		If a formatter ends without using its value it is printed
	/// </summary>
	[System.Serializable]
	public class PrintContextValue : DocumentItemBase, IValueDocumentItem
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal PrintContextValue()
		{

		}

		[UsedImplicitly]
		protected PrintContextValue(SerializationInfo info, StreamingContext c) : base(info, c)
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

				ContentDocumentItem.WriteContent(outputStream, value, context);
			}
			
			return Children.WithScope(context);
		}

		public async Task<ContextObject> GetValue(ContextObject context, ScopeData scopeData)
		{
			await Task.CompletedTask;
			return context;
		}
	}
}