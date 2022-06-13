using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Document.Custom;

/// <summary>
///		Can be used to create a single statement Tag
/// </summary>
public class TagDocumentItemProvider : TagDocumentItemProviderBase
{
	private readonly TagDocumentProviderFunction _action;

	/// <summary>
	///		
	/// </summary>
	/// <param name="tagKeyword">Should contain full tag like <code>#Anything</code> excluding the brackets and any parameter</param>
	/// <param name="action"></param>
	public TagDocumentItemProvider(string tagKeyword, TagDocumentProviderFunction action) : base(tagKeyword)
	{
		_action = action;
	}

	/// <summary>
	/// 
	/// </summary>
	public class TagDocumentItem : ValueDocumentItemBase, ToParsableStringDocumentVisitor.IStringVisitor
	{
		private readonly TagDocumentProviderFunction _action;

		/// <inheritdoc />
		public TagDocumentItem()
		{

		}

		/// <inheritdoc />
		public TagDocumentItem(TextRange location,
								TagDocumentProviderFunction action,
								string tagKeyword,
								string value,
								IEnumerable<ITokenOption> tagCreationOptions) : base(location, value, tagCreationOptions)
		{
			_action = action;
			TagKeyword = tagKeyword;
		}

		/// <summary>
		///		The keyword this DocumentItem was used to create
		/// </summary>
		public string TagKeyword { get; private set; }

		/// <inheritdoc />
		public override async ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			await _action(outputStream, context, scopeData, Value, TagKeyword).ConfigureAwait(false);
			return Array.Empty<DocumentItemExecution>();
		}

		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}

		/// <inheritdoc />
		public void Render(ToParsableStringDocumentVisitor visitor)
		{
			visitor.StringBuilder.Append("{{");
			visitor.CheckForInlineTagLineBreakAtStart(this);
			visitor.StringBuilder.Append(TagKeyword);
			if (!string.IsNullOrWhiteSpace(Value))
			{
				visitor.StringBuilder.Append(" ");
				visitor.StringBuilder.Append(Value);
			}
			visitor.CheckForInlineTagLineBreakAtEnd(this);
			visitor.StringBuilder.Append("}}");
		}
	}

	/// <inheritdoc />
	public override IDocumentItem CreateDocumentItem(string tagKeyword, string value, TokenPair token,
													ParserOptions options, IEnumerable<ITokenOption> tagCreationOptions)
	{
		return new TagDocumentItem(token.TokenRange, _action, tagKeyword, value, tagCreationOptions);
	}
}