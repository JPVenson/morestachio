using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Morestachio.Framework;

namespace Morestachio.Document
{
	/// <summary>
	///		Contains the Declaration of a Partial item
	/// </summary>
	public class PartialDocumentItem : DocumentItemBase
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal PartialDocumentItem()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PartialDocumentItem"/> class.
		/// </summary>
		/// <param name="partialName">The partial name.</param>
		/// <param name="partial">The partial.</param>
		public PartialDocumentItem(string partialName, IDocumentItem partial)
		{
			Value = partialName;
			Partial = partial;
		}

		[UsedImplicitly]
		protected PartialDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
			Value = info.GetString(nameof(Value));
			Partial = info.GetValue(nameof(Partial), typeof(IDocumentItem)) as IDocumentItem;
		}

		protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
		{
			base.SerializeBinaryCore(info, context);
			info.AddValue(nameof(Value), Value);
			info.AddValue(nameof(Partial), Partial, Partial.GetType());
		}

		/// <inheritdoc />
		public override string Kind { get; } = "Partial";

		/// <summary>
		///		The name of the Partial
		/// </summary>
		public string Value { get; private set; }

		/// <summary>
		///		The partial Document
		/// </summary>
		public IDocumentItem Partial { get; }

		/// <inheritdoc />
		public override async Task<IEnumerable<DocumentItemExecution>> Render(IByteCounterStream outputStream, ContextObject context,
			ScopeData scopeData)
		{
			scopeData.Partials[Value] = Partial;
			await Task.CompletedTask;
			return new DocumentItemExecution[0];
		}
	}
}