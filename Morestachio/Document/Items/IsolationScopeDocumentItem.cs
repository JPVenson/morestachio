using System.Security.Permissions;
using System.Xml;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Expression.Parser;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;

namespace Morestachio.Document.Items
{
	/// <summary>
	///		Document item for Isolation
	/// </summary>
	[Serializable]
	public class IsolationScopeDocumentItem : BlockDocumentItemBase, ISupportCustomAsyncCompilation
	{
		/// <inheritdoc />
		public IsolationScopeDocumentItem()
		{
			
		}
		
		/// <inheritdoc />
		public IsolationScopeDocumentItem(CharacterLocation location, IsolationOptions isolationOptions,
			IMorestachioExpression morestachioExpression,
			IEnumerable<ITokenOption> tagCreationOptions) : base(location, tagCreationOptions)
		{
			Isolation = isolationOptions;
			ScopeIsolationExpression = morestachioExpression;
		}

		/// <summary>
		///		Creates a new DocumentItemBase from a Serialization context
		/// </summary>
		/// <param name="info"></param>
		/// <param name="c"></param>
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		protected IsolationScopeDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
			Isolation = (IsolationOptions) info.GetValue(nameof(Isolation), typeof(IsolationOptions));
			ScopeIsolationExpression = (IMorestachioExpression) info.GetValue(nameof(ScopeIsolationExpression), typeof(IMorestachioExpression));
		}
		
		/// <inheritdoc />
		protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(nameof(Isolation), Isolation);
			info.AddValue(nameof(ScopeIsolationExpression), ScopeIsolationExpression);
			base.SerializeBinaryCore(info, context);
		}
		
		/// <inheritdoc />
		protected override void SerializeXml(XmlWriter writer)
		{
			foreach (var flag in Isolation.GetFlags())
			{
				writer.WriteAttributeString(flag.ToString(), "true");
			}
			base.SerializeXml(writer);

			if (ScopeIsolationExpression != null)
			{
				writer.WriteStartElement(nameof(ScopeIsolationExpression));
				writer.WriteExpressionToXml(ScopeIsolationExpression);
				writer.WriteEndElement();
			}
		}
		
		/// <inheritdoc />
		protected override void DeSerializeXml(XmlReader reader)
		{
			foreach (IsolationOptions option in Enum.GetValues(typeof(IsolationOptions)))
			{
				if (reader.GetAttribute(option.ToString()) == "true")
				{
					Isolation |= option;
				}
			}
			base.DeSerializeXml(reader);

			if (reader.Name == nameof(ScopeIsolationExpression))
			{
				reader.ReadStartElement();
				var subtree = reader.ReadSubtree();
				subtree.Read();
				ScopeIsolationExpression = subtree.ParseExpressionFromKind();
				reader.Skip();
				reader.ReadEndElement();
			}
		}

		/// <summary>
		///		The type of isolation enforced
		/// </summary>
		public IsolationOptions Isolation { get; private set; }

		/// <summary>
		///		Is set, defines the path that should be isolated to
		/// </summary>
		public IMorestachioExpression ScopeIsolationExpression { get; private set; }

		/// <inheritdoc />
		public override async ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			if (ScopeIsolationExpression != null)
			{
				context = await ScopeIsolationExpression.GetValue(context, scopeData);
				context = new ContextObject(context.Key, null, context.Value);
			}

			return Children.WithScope(context);
		}
		
		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}

		/// <param name="compiler"></param>
		/// <param name="parserOptions"></param>
		/// <inheritdoc />
		public CompilationAsync Compile(IDocumentCompiler compiler, ParserOptions parserOptions)
		{
			var children = compiler.Compile(Children, parserOptions);
			if (ScopeIsolationExpression != null)
			{
				var compiledExpression = ScopeIsolationExpression.Compile(parserOptions);
				return async (stream, context, data) =>
				{
					context = await compiledExpression(context, data);
					context = new ContextObject(context.Key, null, context.Value);
					await children(stream, context, data);
				};
			}

			
			return children;
		}
	}
}
