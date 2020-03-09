using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Framework;

namespace Morestachio.Document
{
	/// <summary>
	///		Evaluates a variable expression and then stores it into the set alias
	/// </summary>
	[System.Serializable]
	public class EvaluateVariableDocumentItem : ValueDocumentItemBase
	{
		/// <inheritdoc />
		[UsedImplicitly]
		protected EvaluateVariableDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
			
		}

		internal EvaluateVariableDocumentItem()
		{
			
		}

		public EvaluateVariableDocumentItem(string value)
		{
			Value = value;
		}
		
		/// <inheritdoc />
		public override async Task<IEnumerable<DocumentItemExecution>> Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			await Task.CompletedTask;
			context = context.CloneForEdit();
			foreach (var expression in Children.OfType<IValueDocumentItem>())
			{
				context = await expression.GetValue(context, scopeData);
			}
			scopeData.Alias[Value] = context;
			return new DocumentItemExecution[0];
		}

		/// <inheritdoc />
		public override string Kind { get; } = "EvaluateVariableDocumentItem";
	}
}