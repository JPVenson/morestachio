using System;
using System.Collections.Generic;
using Morestachio.Document.Contracts;

namespace Morestachio.Parsing
{
	/// <summary>
	///		Defines the scope of an DocumentItem that can has children
	/// </summary>
	public class DocumentScope
	{
		private readonly Lazy<int> _variableScopeNumber;

		/// <summary>
		///		Creates a new Document scope that is no formatting and is no alias
		/// </summary>
		public DocumentScope(IDocumentItem document, Func<int> variableScopeNumber)
		{
			Document = document;
			_variableScopeNumber = new Lazy<int>(variableScopeNumber);
			LocalVariables = new List<string>();
		}


		internal DocumentScope(IDocumentItem document, int variableScopeNumber)
		{
			Document = document;
			_variableScopeNumber = new Lazy<int>(() => variableScopeNumber);
			LocalVariables = new List<string>();
		}

		/// <summary>
		///		The document item that has children
		/// </summary>
		public IDocumentItem Document { get; private set; }

		/// <summary>
		///		All variables and alias within this block
		/// </summary>
		public IList<string> LocalVariables { get; private set; }

		/// <summary>
		///		Is this an alias
		/// </summary>
		public bool HasAlias
		{
			get { return LocalVariables.Count > 0; }
		}

		/// <summary>
		///		The ID of the scope
		/// </summary>
		public int VariableScopeNumber
		{
			get { return _variableScopeNumber?.Value ?? -1; }
		}
	}
}