using System.Collections.Generic;
using Morestachio.Document.Contracts;

namespace Morestachio.Fluent;

/// <summary>
///		A node in the Tree for the <see cref="MorestachioDocumentFluentApi"/>
/// </summary>
public class MorestachioNode
{
	/// <summary>
	///		Creates a new Node
	/// </summary>
	/// <param name="ancestor"></param>
	/// <param name="item"></param>
	public MorestachioNode(MorestachioNode ancestor, IDocumentItem item)
	{
		Ancestor = ancestor;
		Item = item;
		Leafs = new List<MorestachioNode>();
	}

	/// <summary>
	///		Gets the parent of this node
	/// </summary>
	public MorestachioNode Ancestor { get; }

	/// <summary>
	///		Gets the Document item
	/// </summary>
	public IDocumentItem Item { get; }

	/// <summary>
	///		Gets all leafs of this node
	/// </summary>
	public IList<MorestachioNode> Leafs { get; }

	/// <summary>
	///		Gets the next node in order
	/// </summary>
	public MorestachioNode Next { get; internal set; }

	/// <summary>
	///		Gets the previous node in order
	/// </summary>
	public MorestachioNode Previous { get; internal set; }
}