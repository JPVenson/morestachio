using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Expression;

namespace Morestachio.Document
{
	/// <summary>
	///		Evaluates a variable expression and then stores it into the set alias
	/// </summary>
	[System.Serializable]
	public class EvaluateVariableDocumentItem : ExpressionDocumentItemBase
	{
		/// <inheritdoc />
		[UsedImplicitly]
		protected EvaluateVariableDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
			Value = info.GetString(nameof(Value));
		}

		internal EvaluateVariableDocumentItem()
		{
			
		}

		/// <inheritdoc />
		public EvaluateVariableDocumentItem(string value, IMorestachioExpression morestachioExpression)
		{
			MorestachioExpression = morestachioExpression;
			Value = value;
		}
		
		/// <inheritdoc />
		protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
		{
			base.SerializeBinaryCore(info, context);
			info.AddValue(nameof(Value), Value);
		}
		
		/// <inheritdoc />
		protected override void SerializeXml(XmlWriter writer)
		{
			writer.WriteAttributeString(nameof(Value), Value);
			base.SerializeXml(writer);
		}
		
		/// <inheritdoc />
		protected override void DeSerializeXml(XmlReader reader)
		{
			Value = reader.GetAttribute(nameof(Value));
			base.DeSerializeXml(reader);
		}
		
		/// <inheritdoc />
		public override async Task<IEnumerable<DocumentItemExecution>> Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			await Task.CompletedTask;
			context = await MorestachioExpression.GetValue(context, scopeData);
			scopeData.Alias[Value] = context;
			return new DocumentItemExecution[0];
		}

		/// <summary>
		///		The name of the Variable
		/// </summary>
		public string Value { get; set; }
		/// <inheritdoc />
		public override string Kind { get; } = "EvaluateVariableDocumentItem";

		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}