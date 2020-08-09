﻿using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Expression;
using Morestachio.Helper;
#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using Promise = System.Threading.Tasks.ValueTask;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using Promise = System.Threading.Tasks.Task;
#endif

namespace Morestachio.Document
{
	/// <summary>
	///		Creates an alias 
	/// </summary>
	[System.Serializable]
	public class AliasDocumentItem : ValueDocumentItemBase
	{
		/// <summary>
		///		Creates a new Alias DocumentItem
		/// </summary>
		/// <param name="value">The name of the Alias</param>
		/// <param name="variableScope">The Scope id generated by the parser to determinate when to clean this variable</param>
		public AliasDocumentItem(string value, int variableScope)
		{
			Value = value;
			IdVariableScope = variableScope;
		}

		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal AliasDocumentItem()
		{

		}

		/// <inheritdoc />
		[UsedImplicitly]
		protected AliasDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
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

		/// <inheritdoc />
		public override ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			scopeData.AddVariable(Value, context, IdVariableScope);
			return Children.WithScope(context).ToPromise();
		}

		/// <inheritdoc />
		public override string Kind { get; } = "Alias";

		/// <summary>
		///		Gets or Sets the Scope of the variable
		/// </summary>
		public int IdVariableScope { get; private set; }

		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}