using System.Xml;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;
using Morestachio.Helper;

namespace Morestachio.Document.Items
{
	/// <summary>
	///		Contains the Declaration of a Partial item
	/// </summary>
	[Serializable]
	public class PartialDocumentItem : BlockDocumentItemBase, IEquatable<PartialDocumentItem>, ISupportCustomAsyncCompilation
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal PartialDocumentItem()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PartialDocumentItem"/> class.
		/// </summary>
		/// <param name="partialName">The partial name.</param>
		public PartialDocumentItem(CharacterLocation location,  
			string partialName,  
			IEnumerable<ITokenOption> tagCreationOptions) 
			: base(location, tagCreationOptions)
		{
			PartialName = partialName;
		}

		/// <summary>
		///		The name of this partial
		/// </summary>
		public string PartialName { get; private set; }
		
		/// <inheritdoc />
		protected PartialDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
			PartialName = info.GetString(nameof(PartialName));
		}

		/// <inheritdoc />
		protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(nameof(PartialName), PartialName);
			base.SerializeBinaryCore(info, context);
		}
		
		/// <inheritdoc />
		protected override void SerializeXml(XmlWriter writer)
		{
			writer.WriteAttributeString(nameof(PartialName), PartialName);
			base.SerializeXml(writer);
		}
		
		/// <inheritdoc />
		protected override void DeSerializeXml(XmlReader reader)
		{
			PartialName = reader.GetAttribute(nameof(PartialName));
			base.DeSerializeXml(reader);
		}

		/// <param name="compiler"></param>
		/// <param name="parserOptions"></param>
		/// <inheritdoc />
		public CompilationAsync Compile(IDocumentCompiler compiler, ParserOptions parserOptions)
		{
			var children = compiler.Compile(Children, parserOptions);
			return async (stream, context, scopeData) =>
			{
				scopeData.CompiledPartials[PartialName] = children;
				await AsyncHelper.FakePromise();
			};
		}

		/// <inheritdoc />
		public override ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context,
			ScopeData scopeData)
		{
			scopeData.Partials[PartialName] = new MorestachioDocument(ExpressionStart, Enumerable.Empty<ITokenOption>())
			{
				Children = Children
			};
			return Enumerable.Empty<DocumentItemExecution>().ToPromise();
		}

		/// <inheritdoc />
		public bool Equals(PartialDocumentItem other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return base.Equals(other);
		}

		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}