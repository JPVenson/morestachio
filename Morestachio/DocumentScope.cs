using Morestachio.Document;
using Morestachio.Document.Contracts;

namespace Morestachio
{
	/// <summary>
	///		Defines the scope of an DocumentItem that can has children
	/// </summary>
	public struct DocumentScope
	{
		/// <summary>
		///		Creates a new Document scope that is no formatting and is no alias
		/// </summary>
		/// <param name="document"></param>
		public DocumentScope(IDocumentItem document)
		{
			Document = document;
			IsFormattingScope = false;
			HasAlias = false;
			AliasName = null;
		}


		internal DocumentScope(IDocumentItem document, bool isFormattingScope)
		{
			Document = document;
			IsFormattingScope = isFormattingScope;
			HasAlias = false;
			AliasName = null;
		}

		internal DocumentScope(IDocumentItem document, string aliasName)
		{
			Document = document;
			IsFormattingScope = false;
			HasAlias = true;
			AliasName = aliasName;
		}

		/// <summary>
		///		The document item that has children
		/// </summary>
		public IDocumentItem Document { get; private set; }

		/// <summary>
		///		Is this a formatted scope
		/// </summary>
		public bool IsFormattingScope { get; private set; }

		/// <summary>
		///		Is this an alias
		/// </summary>
		public bool HasAlias { get; private set; }

		/// <summary>
		///		If <see cref="HasAlias"/> what name does the alias have
		/// </summary>
		public string AliasName { get; private set; }
	}
}