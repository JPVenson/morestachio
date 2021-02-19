#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;
using Morestachio.Helper;

namespace Morestachio.Document.Items
{
	/// <summary>
	///		Contains the Declaration of a Partial item
	/// </summary>
	[Serializable]
	public class PartialDocumentItem : ValueDocumentItemBase, IEquatable<PartialDocumentItem>, ISupportCustomCompilation
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
		public PartialDocumentItem(CharacterLocation location,  
			string partialName,  
			IEnumerable<ITokenOption> tagCreationOptions) 
			: base(location, partialName, tagCreationOptions)
		{
		}

		/// <inheritdoc />
		public Compilation Compile()
		{
			var children = MorestachioDocument.CompileItemsAndChildren(Children);
			return async (stream, context, scopeData) =>
			{
				scopeData.CompiledPartials[Value] = children;
				await AsyncHelper.FakePromise();
			};
		}

		/// <inheritdoc />
		public override ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context,
			ScopeData scopeData)
		{
			scopeData.Partials[Value] = new MorestachioDocument(ExpressionStart, Enumerable.Empty<ITokenOption>())
			{
				Children = Children
			};
			return Enumerable.Empty<DocumentItemExecution>().ToPromise();
		}

		/// <inheritdoc />
		public bool Equals(PartialDocumentItem other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return base.Equals(other);
		}

		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}