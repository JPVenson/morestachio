using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;

namespace Morestachio.Document.Items.SwitchCase
{
	/// <summary>
	///		Defines an default case to be used within a switch statement if all other <see cref="SwitchCaseDocumentItem"/> do not match.
	///		If used outside a <see cref="SwitchDocumentItem"/>, it will unconditionally render its items
	/// </summary>
	[Serializable]
	public class SwitchDefaultDocumentItem : BlockDocumentItemBase
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal SwitchDefaultDocumentItem()
		{

		}

		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		public SwitchDefaultDocumentItem(CharacterLocation location,
			IEnumerable<ITokenOption> tagCreationOptions) : base(location, tagCreationOptions)
		{

		}
		
		/// <inheritdoc />
		protected SwitchDefaultDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
		}
		
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
	}
}