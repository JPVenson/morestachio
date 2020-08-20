#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#endif
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework.Context;
using Morestachio.Framework.IO;
using Morestachio.Helper;

namespace Morestachio.Document.Items
{
	/// <summary>
	///		Removes the alias from the scope
	/// </summary>
	[System.Serializable]
	public class RemoveAliasDocumentItem : ValueDocumentItemBase
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal RemoveAliasDocumentItem()
		{

		}

		[UsedImplicitly]
		protected RemoveAliasDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
			IdVariableScope = info.GetInt32(nameof(IdVariableScope));
		}

		/// <inheritdoc />
		protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
		{
			base.SerializeBinaryCore(info, context);
			info.AddValue(nameof(IdVariableScope), IdVariableScope);
		}
		
		/// <inheritdoc />
		protected override void SerializeXml(XmlWriter writer)
		{
			writer.WriteAttributeString(nameof(IdVariableScope), IdVariableScope.ToString());
			base.SerializeXml(writer);
		}
		
		/// <inheritdoc />
		protected override void DeSerializeXml(XmlReader reader)
		{
			IdVariableScope = int.Parse(reader.GetAttribute(nameof(IdVariableScope)));
			base.DeSerializeXml(reader);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="aliasName"></param>
		/// <param name="scopeVariableScopeNumber"></param>
		public RemoveAliasDocumentItem([NotNull] string aliasName, int scopeVariableScopeNumber)
		{
			Value = aliasName ?? throw new ArgumentNullException(nameof(aliasName));
			IdVariableScope = scopeVariableScopeNumber;
		}

		/// <inheritdoc />
		public override ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			scopeData.RemoveVariable(Value, IdVariableScope);
			return Enumerable.Empty<DocumentItemExecution>().ToPromise();
		}
		
		/// <summary>
		///		Gets or Sets the Scope of the variable that should be removed
		/// </summary>
		public int IdVariableScope { get; private set; }
		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}