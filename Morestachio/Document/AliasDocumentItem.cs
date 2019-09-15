using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Framework;

namespace Morestachio.Document
{
	/// <summary>
	///		Creates an alias 
	/// </summary>
	[System.Serializable]
	public class AliasDocumentItem : ValueDocumentItemBase
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public AliasDocumentItem(string value)
		{
			Value = value;
		}

		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal AliasDocumentItem()
		{
			
		}

		[UsedImplicitly]
		protected AliasDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
		}

		/// <inheritdoc />
		public override async Task<IEnumerable<DocumentItemExecution>> Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			scopeData.Alias[Value] = context.Clone();

			await Task.CompletedTask;
			return Children.WithScope(context);
		}

		/// <inheritdoc />
		public override string Kind { get; } = "Alias";
	}
}