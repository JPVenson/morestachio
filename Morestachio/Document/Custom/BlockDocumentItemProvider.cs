using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;

namespace Morestachio.Document.Custom;

/// <summary>
///		The Standard Block that is enclosed with an opening tag <code>{{#Anything}}</code> and closed with an closing tag <code>{{/Anything}}</code>
/// </summary>
public class BlockDocumentItemProvider : BlockDocumentItemProviderBase
{
	private readonly BlockDocumentProviderFunction _action;

	/// <summary>
	///		Creates a new Block
	/// </summary>
	/// <param name="tagOpen">Should contain full tag like <code>#Anything</code> excluding the brackets and any parameter</param>
	/// <param name="tagClose">Should contain full tag like <code>/Anything</code> excluding the brackets and any parameter</param>
	/// <param name="action"></param>
	public BlockDocumentItemProvider(string tagOpen, string tagClose, BlockDocumentProviderFunction action)
		: base(tagOpen, tagClose)
	{
		_action = action;
	}

	/// <summary>
	///		The General purpose block
	/// </summary>
	public class BlockDocumentItem : ValueDocumentItemBase
	{
		private readonly BlockDocumentProviderFunction _action;

		/// <summary>
		/// 
		/// </summary>
		public BlockDocumentItem()
		{

		}

		/// <inheritdoc />
		public BlockDocumentItem(CharacterLocation location, 
								BlockDocumentProviderFunction action, 
								string value,
								IEnumerable<ITokenOption> tagCreationOptions) : base(location, value, tagCreationOptions)
		{
			_action = action;
		}
			
		/// <inheritdoc />
		public override async ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			return await _action(outputStream, context, scopeData, Value, Children).ConfigureAwait(false);
			//return Array.Empty<DocumentItemExecution>();
		}

		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}
	}

	/// <inheritdoc />
	public override IDocumentItem CreateDocumentItem(string tag, 
													 string value, 
													 TokenPair token,
													 ParserOptions options, 
													 IEnumerable<ITokenOption> tagCreationOptions)
	{
		return new BlockDocumentItem(token.TokenLocation, _action, value, tagCreationOptions);
	}
}