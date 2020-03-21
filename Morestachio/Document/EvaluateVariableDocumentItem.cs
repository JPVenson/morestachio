using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
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
			
		}

		internal EvaluateVariableDocumentItem()
		{
			
		}

		public EvaluateVariableDocumentItem(string value, IExpression expression)
		{
			Expression = expression;
			Value = value;
		}

		protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
		{
			base.SerializeBinaryCore(info, context);
			info.AddValue(nameof(Value), Value);
		}

		protected override void SerializeXml(XmlWriter writer)
		{
			writer.WriteAttributeString(nameof(Value), Value);
			base.SerializeXml(writer);
		}

		protected override void DeSerializeXml(XmlReader reader)
		{
			Value = reader.GetAttribute(nameof(Value));
			if (reader.Name == nameof(Expression))
			{
				if (reader.IsEmptyElement)
				{
					return;
				}
				//reader.ReadToFollowing(nameof(Value));
				Expression = reader.ReadContentAs(typeof(IExpression), null) as IExpression;
			}
		}
		
		/// <inheritdoc />
		public override async Task<IEnumerable<DocumentItemExecution>> Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			await Task.CompletedTask;
			context = await Expression.GetValue(context, scopeData);
			scopeData.Alias[Value] = context;
			return new DocumentItemExecution[0];
		}

		public string Value { get; set; }
		/// <inheritdoc />
		public override string Kind { get; } = "EvaluateVariableDocumentItem";
	}
}