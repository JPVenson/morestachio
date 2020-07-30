using System.Threading.Tasks;
using Morestachio.Document.Contracts;
using Morestachio.Document.Visitor;
using Morestachio.Framework;

#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using Promise = System.Threading.Tasks.ValueTask;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using Promise = System.Threading.Tasks.Task;
#endif
namespace Morestachio.Document.Custom
{
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
			public BlockDocumentItem(string kind, BlockDocumentProviderFunction action, string value)
			{
				_action = action;
				Kind = kind;
				Value = value;
			}
			
			/// <inheritdoc />
			public override async ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
			{
				return await _action(outputStream, context, scopeData, Value, Children);
				//return Array.Empty<DocumentItemExecution>();
			}

			/// <inheritdoc />
			public override string Kind { get; }
			/// <inheritdoc />
			public override void Accept(IDocumentItemVisitor visitor)
			{
				visitor.Visit(this);
			}
		}

		/// <inheritdoc />
		public override IDocumentItem CreateDocumentItem(string tag, string value, TokenPair token, ParserOptions options)
		{
			return new BlockDocumentItem(tag, _action, value);
		}
	}
}