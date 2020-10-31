#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#endif
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.IO;
using Morestachio.Helper;

namespace Morestachio.Document.Items
{
	/// <summary>
	///		Removes the alias from the scope
	/// </summary>
	[Serializable]
	public class RemoveAliasDocumentItem : ValueDocumentItemBase, ISupportCustomCompilation
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal RemoveAliasDocumentItem() : base(CharacterLocation.Unknown, null)
		{

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="aliasName"></param>
		/// <param name="scopeVariableScopeNumber"></param>
		public RemoveAliasDocumentItem(CharacterLocation location, [NotNull] string aliasName, int scopeVariableScopeNumber) 
			: base(location, aliasName)
		{
			IdVariableScope = scopeVariableScopeNumber;
		}
		
		/// <inheritdoc />
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
			var varScope = reader.GetAttribute(nameof(IdVariableScope));
			if (!int.TryParse(varScope, out var intVarScope))
			{
				throw new XmlException($"Error while serializing '{nameof(AliasDocumentItem)}'. " +
				                       $"The value for '{nameof(IdVariableScope)}' is expected to be an integer.");
			}
			IdVariableScope = intVarScope;
			base.DeSerializeXml(reader);
		}

		public Compilation Compile()
		{
			return async (stream, context, scopeData) =>
			{
				scopeData.RemoveVariable(Value, IdVariableScope);
			};
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