#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#endif
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;
using Morestachio.Helper;

namespace Morestachio.Document.Items
{
	/// <summary>
	///		Document item for Isolation
	/// </summary>
	[Serializable]
	public class IsolationScopeDocumentItem : BlockDocumentItemBase, ISupportCustomCompilation
	{
		/// <inheritdoc />
		public IsolationScopeDocumentItem()
		{
			
		}
		
		/// <inheritdoc />
		public IsolationScopeDocumentItem(CharacterLocation location, IsolationOptions isolationOptions,
			IEnumerable<ITokenOption> tagCreationOptions) : base(location, tagCreationOptions)
		{
			Isolation = isolationOptions;
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
		}
		
		/// <inheritdoc />
		protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(nameof(Isolation), Isolation);
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
		}

		/// <summary>
		///		The type of isolation enforced
		/// </summary>
		public IsolationOptions Isolation { get; private set; }
		
		/// <inheritdoc />
		public override ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			return Children.WithScope(context).ToPromise();
		}
		
		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}

		/// <inheritdoc />
		public Compilation Compile()
		{
			return MorestachioDocument.CompileItemsAndChildren(Children);
		}
	}
}
