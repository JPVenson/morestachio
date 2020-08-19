using Morestachio.Framework.Context;

namespace Morestachio.Document.Contracts
{
	/// <summary>
	///		Combines a Document info that should be rendered with a <see cref="ContextObject"/>
	/// </summary>
	public readonly struct DocumentItemExecution
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DocumentItemExecution"/> struct.
		/// </summary>
		/// <param name="documentItem">The document item.</param>
		/// <param name="contextObject">The context object.</param>
		public DocumentItemExecution(IDocumentItem documentItem, ContextObject contextObject)
		{
			DocumentItem = documentItem;
			ContextObject = contextObject;
		}
		/// <summary>
		/// Gets the document item.
		/// </summary>
		/// <value>
		/// The document item.
		/// </value>
		public IDocumentItem DocumentItem { get; }
		/// <summary>
		/// Gets the context object.
		/// </summary>
		/// <value>
		/// The context object.
		/// </value>
		public ContextObject ContextObject { get; }
	}
}