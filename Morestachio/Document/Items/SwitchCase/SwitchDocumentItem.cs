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
using Morestachio.Framework.Expression;
using Morestachio.Framework.IO;

namespace Morestachio.Document.Items.SwitchCase
{
	/// <summary>
	///		The document item for a switch block
	/// </summary>
	[Serializable]
	public class SwitchDocumentItem : ExpressionDocumentItemBase
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal SwitchDocumentItem()
		{

		}

		/// <inheritdoc />
		public SwitchDocumentItem(IMorestachioExpression value, bool shouldScopeToValue)
		{
			MorestachioExpression = value;
			ScopeToValue = shouldScopeToValue;
		}
		
		/// <inheritdoc />
		[UsedImplicitly]
		protected SwitchDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
			ScopeToValue = info.GetBoolean(nameof(ScopeToValue));
		}

		/// <summary>
		///		Indicates that the case statement should also scope to the value given in switch
		/// </summary>
		public bool ScopeToValue { get; private set; }

		/// <inheritdoc />
		public override async ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			var value = await MorestachioExpression.GetValue(context, scopeData);
			var switchCases = Children.OfType<SwitchCaseDocumentItem>();
			IDocumentItem matchingCase = null;
			foreach (var switchCaseDocumentItem in switchCases)
			{
				var contextObject = await switchCaseDocumentItem.MorestachioExpression.GetValue(context, scopeData);
				if (Equals(contextObject.Value, value.Value))
				{
					matchingCase = switchCaseDocumentItem;
					break;
				}
			}

			if (ScopeToValue)
			{
				context = value;
			}

			if (matchingCase != null)
			{
				return await matchingCase.Render(outputStream, context, scopeData);
			}

			matchingCase = Children.FirstOrDefault(e => e is SwitchDefaultDocumentItem);
			
			if (matchingCase != null)
			{
				return await matchingCase.Render(outputStream, context, scopeData);
			}

			return Enumerable.Empty<DocumentItemExecution>();
		}

		protected override void DeSerializeXml(XmlReader reader)
		{
			ScopeToValue = reader.GetAttribute(nameof(ScopeToValue)) == bool.TrueString;
			base.DeSerializeXml(reader);
		}

		protected override void SerializeXml(XmlWriter writer)
		{
			writer.WriteAttributeString(nameof(ScopeToValue), ScopeToValue.ToString());
			base.SerializeXml(writer);
		}

		protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(nameof(ScopeToValue), ScopeToValue);
			base.SerializeBinaryCore(info, context);
		}

		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
