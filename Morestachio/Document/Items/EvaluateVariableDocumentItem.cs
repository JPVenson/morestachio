#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using Promise = System.Threading.Tasks.ValueTask;
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
using Morestachio.Framework.Expression;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;

namespace Morestachio.Document.Items
{
	/// <summary>
	///		Evaluates a variable expression and then stores it into the set alias
	/// </summary>
	[Serializable]
	public class EvaluateVariableDocumentItem : ExpressionDocumentItemBase, ISupportCustomAsyncCompilation
	{

		internal EvaluateVariableDocumentItem()
		{

		}

		/// <summary>
		///		Creates an new Variable that expires when its enclosing scope (<see cref="IdVariableScope"/>) is closed
		/// </summary>
		/// <param name="value"></param>
		/// <param name="morestachioExpression"></param>
		/// <param name="idVariableScope"></param>
		public EvaluateVariableDocumentItem(CharacterLocation location,
			string value,
			IMorestachioExpression morestachioExpression,
			int idVariableScope,
			IEnumerable<ITokenOption> tagCreationOptions)
			: base(location, morestachioExpression, tagCreationOptions)
		{
			Value = value;
			IdVariableScope = idVariableScope;
		}

		/// <summary>
		///		Creates a new global Variable
		/// </summary>
		/// <param name="value"></param>
		/// <param name="morestachioExpression"></param>
		public EvaluateVariableDocumentItem(CharacterLocation location, string value, IMorestachioExpression morestachioExpression,
			IEnumerable<ITokenOption> tagCreationOptions)
			: base(location, morestachioExpression, tagCreationOptions)
		{
			Value = value;
			IdVariableScope = 0;
		}

		/// <inheritdoc />

		protected EvaluateVariableDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
			Value = info.GetString(nameof(Value));
			IdVariableScope = info.GetInt32(nameof(IdVariableScope));
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
			var varScope = reader.GetAttribute(nameof(IdVariableScope));
			if (!int.TryParse(varScope, out var intVarScope))
			{
				throw new XmlException($"Error while serializing '{nameof(EvaluateVariableDocumentItem)}'." +
									   $" The value for '{nameof(IdVariableScope)}' is expected to be an integer.");
			}
			IdVariableScope = intVarScope;
			base.DeSerializeXml(reader);
		}

		/// <param name="compiler"></param>
		/// <inheritdoc />
		public CompilationAsync Compile(IDocumentCompiler compiler)
		{
			var expression = MorestachioExpression.Compile();
			return async (stream, context, scopeData) =>
			{
				context = await expression(context, scopeData);
				scopeData.AddVariable(Value, context, IdVariableScope);
			};
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

	/// <summary>
	///		Evaluates a variable expression and then stores it into the set alias
	/// </summary>
	[Serializable]
	public class EvaluateLetVariableDocumentItem : EvaluateVariableDocumentItem
	{
		internal EvaluateLetVariableDocumentItem()
		{

		}

		/// <summary>
		///		Creates an new Variable that expires when its enclosing scope (<see cref="IdVariableScope"/>) is closed
		/// </summary>
		/// <param name="value"></param>
		/// <param name="morestachioExpression"></param>
		/// <param name="idVariableScope"></param>
		public EvaluateLetVariableDocumentItem(CharacterLocation location,
			string value,
			IMorestachioExpression morestachioExpression,
			int idVariableScope,
			IEnumerable<ITokenOption> tagCreationOptions)
			: base(location, value, morestachioExpression, idVariableScope, tagCreationOptions)
		{
		}
		
		/// <inheritdoc />
		protected EvaluateLetVariableDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{

		}

		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}