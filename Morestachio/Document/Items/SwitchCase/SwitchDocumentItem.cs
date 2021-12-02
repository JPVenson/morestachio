using System.Threading.Tasks;
using System.Xml;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;

namespace Morestachio.Document.Items.SwitchCase
{
	/// <summary>
	///		The document item for a switch block
	/// </summary>
	[Serializable]
	public class SwitchDocumentItem : ExpressionDocumentItemBase, ISupportCustomAsyncCompilation
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal SwitchDocumentItem()
		{

		}

		/// <inheritdoc />
		public SwitchDocumentItem(CharacterLocation location,
			IMorestachioExpression value,
			bool shouldScopeToValue,
			IEnumerable<ITokenOption> tagCreationOptions)
			: base(location, value, tagCreationOptions)
		{
			ScopeToValue = shouldScopeToValue;
		}

		/// <inheritdoc />
		
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
			var children = Children.OfType<SwitchCaseDocumentItem>().Cast<IDocumentItem>()
				.Concat(Children.OfType<SwitchDefaultDocumentItem>())
				.Select(e =>
				{
					var item = new SwitchExecutionContainerDocumentItem()
					{
						Document = e
					};
					if (e is SwitchCaseDocumentItem switchCaseItem)
					{
						item.Expression = (contextObject, data) =>
							switchCaseItem.MorestachioExpression.GetValue(contextObject, data);
					}
					return item;
				}).ToArray();

			var value = await MorestachioExpression.GetValue(context, scopeData);
			if (ScopeToValue)
			{
				context = value;
			}
			var toBeExecuted = await CoreAction(outputStream, value, scopeData,
				children);

			if (toBeExecuted != null)
			{
				return await toBeExecuted.Document.Render(outputStream, context, scopeData);
			}

			return Enumerable.Empty<DocumentItemExecution>();
		}

		/// <param name="compiler"></param>
		/// <param name="parserOptions"></param>
		/// <inheritdoc />
		public CompilationAsync Compile(IDocumentCompiler compiler, ParserOptions parserOptions)
		{
			var children = Children.Where(e => e is SwitchCaseDocumentItem || e is SwitchDefaultDocumentItem)
				.Cast<BlockDocumentItemBase>()
				.Select(e => new SwitchExecutionContainerCompiledAction()
			{
				Callback = compiler.Compile(e.Children, parserOptions),
				Expression = (e as SwitchCaseDocumentItem)?.MorestachioExpression.Compile(parserOptions)
			}).ToArray();
			var expression = MorestachioExpression.Compile(parserOptions);

			return async (outputStream, context, scopeData) =>
			{
				var value = await expression(context, scopeData);
				if (ScopeToValue)
				{
					context = value;
				}
				var toBeExecuted = await CoreAction(outputStream, value, scopeData,
					children);

				if (toBeExecuted != null)
				{
					await toBeExecuted.Callback(outputStream, context, scopeData);
				}
			};
		}

		internal class SwitchExecutionContainer
		{
			public CompiledExpression Expression { get; set; }
		}

		internal class SwitchExecutionContainerCompiledAction : SwitchExecutionContainer
		{
			public CompilationAsync Callback { get; set; }
		}

		internal class SwitchExecutionContainerDocumentItem : SwitchExecutionContainer
		{
			public IDocumentItem Document { get; set; }
		}

		internal static async Task<T> CoreAction<T>(
			IByteCounterStream outputStream,
			ContextObject context,
			ScopeData scopeData,
			T[] containers) where T : SwitchExecutionContainer
		{
			T matchingCase = null;
			foreach (var switchCaseDocumentItem in containers.Where(e => e.Expression != null))
			{
				var contextObject = await switchCaseDocumentItem.Expression(context, scopeData);
				if (Equals(contextObject.Value, context.Value))
				{
					matchingCase = switchCaseDocumentItem;
					break;
				}
			}

			if (matchingCase != null)
			{
				return matchingCase;
			}

			return containers.FirstOrDefault(e => e.Expression is null);
		}

		/// <inheritdoc />
		protected override void DeSerializeXml(XmlReader reader)
		{
			ScopeToValue = reader.GetAttribute(nameof(ScopeToValue)) == bool.TrueString;
			base.DeSerializeXml(reader);
		}

		/// <inheritdoc />
		protected override void SerializeXml(XmlWriter writer)
		{
			writer.WriteAttributeString(nameof(ScopeToValue), ScopeToValue.ToString());
			base.SerializeXml(writer);
		}

		/// <inheritdoc />
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
