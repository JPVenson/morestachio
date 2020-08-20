#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using Promise = System.Threading.Tasks.ValueTask;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#endif
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

namespace Morestachio.Document.Items
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
			IdVariableScope = info.GetInt32(nameof(IdVariableScope));
		}

		internal EvaluateVariableDocumentItem()
		{
			
		}

		/// <summary>
		///		Creates an new Variable that expires when its enclosing scope (<see cref="IdVariableScope"/>) is closed
		/// </summary>
		/// <param name="value"></param>
		/// <param name="morestachioExpression"></param>
		/// <param name="idVariableScope"></param>
		public EvaluateVariableDocumentItem(string value, IMorestachioExpression morestachioExpression, int idVariableScope)
		{
			MorestachioExpression = morestachioExpression;
			Value = value;
			IdVariableScope = idVariableScope;
		}

		/// <summary>
		///		Creates a new global Variable
		/// </summary>
		/// <param name="value"></param>
		/// <param name="morestachioExpression"></param>
		public EvaluateVariableDocumentItem(string value, IMorestachioExpression morestachioExpression)
		{
			MorestachioExpression = morestachioExpression;
			Value = value;
			IdVariableScope = 0;
		}
		
		/// <inheritdoc />
		protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
		{
			base.SerializeBinaryCore(info, context);
			info.AddValue(nameof(Value), Value);
			info.AddValue(nameof(IdVariableScope), IdVariableScope);
		}
		
		/// <inheritdoc />
		protected override void SerializeXml(XmlWriter writer)
		{
			writer.WriteAttributeString(nameof(Value), Value);
			writer.WriteAttributeString(nameof(IdVariableScope), IdVariableScope.ToString());
			base.SerializeXml(writer);
		}
		
		/// <inheritdoc />
		protected override void DeSerializeXml(XmlReader reader)
		{
			Value = reader.GetAttribute(nameof(Value));
			IdVariableScope = int.Parse(reader.GetAttribute(nameof(IdVariableScope)));
			base.DeSerializeXml(reader);
		}
		
		/// <inheritdoc />
		public override async ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			context = await MorestachioExpression.GetValue(context, scopeData);
			scopeData.AddVariable(Value, context, IdVariableScope);
			return Enumerable.Empty<DocumentItemExecution>();
		}

		/// <summary>
		///		The name of the Variable
		/// </summary>
		public string Value { get; private set; }
		
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