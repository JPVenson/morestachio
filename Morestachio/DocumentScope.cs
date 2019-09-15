using Morestachio.Document;
using Morestachio.Document.Contracts;

namespace Morestachio
{
	internal struct DocumentScope
	{
		public DocumentScope(IDocumentItem document)
		{
			Document = document;
			IsFormattingScope = false;
			HasAlias = false;
			AliasName = null;
		}
		public DocumentScope(IDocumentItem document, bool isFormattingScope)
		{
			Document = document;
			IsFormattingScope = isFormattingScope;
			HasAlias = false;
			AliasName = null;
		}
		public DocumentScope(IDocumentItem document, string aliasName)
		{
			Document = document;
			IsFormattingScope = false;
			HasAlias = true;
			AliasName = aliasName;
		}

		public IDocumentItem Document { get; private set; }
		public bool IsFormattingScope { get; private set; }
		public bool HasAlias { get; private set; }
		public string AliasName { get; private set; }
	}
}