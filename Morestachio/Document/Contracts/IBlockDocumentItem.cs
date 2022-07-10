using System.Collections.Generic;
using Morestachio.Framework.Tokenizing;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Document.Contracts;

/// <summary>
///		Defines a part that can store itself and children
/// </summary>
public interface IBlockDocumentItem : IDocumentItem
{
	/// <summary>
	///		The list of Children that are children of this Document item
	/// </summary>
	IList<IDocumentItem> Children { get; }

	/// <summary>
	///		The token options set on a closing tag of an block
	/// </summary>
	IEnumerable<ITokenOption> BlockClosingOptions { get; set; }

	/// <summary>
	///		The location of the closing token for this block.
	/// </summary>
	TextRange BlockLocation { get; set; }

	/// <summary>
	///		Adds the specified childs.
	/// </summary>
	void Add(params IDocumentItem[] documentChildren);
}