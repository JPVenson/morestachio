﻿using System.Xml;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Document.Contracts;

/// <summary>
///		Defines a Part in the Template that can be processed
/// </summary>
public interface IDocumentItem
{
	/// <summary>
	///		Renders its Value into the <see cref="IByteCounterStream"/>.
	///		If there are any Document items that should be executed directly after they should be returned, such as the children of this document item	
	/// </summary>
	/// <param name="outputStream">The output stream.</param>
	/// <param name="context">The context.</param>
	/// <param name="scopeData">The scope data.</param>
	/// <returns></returns>
	ItemExecutionPromise Render(IByteCounterStream outputStream,
								ContextObject context,
								ScopeData scopeData);

	/// <summary>
	///		Contains the list of TokenOptions used to construct this DocumentItem.
	/// </summary>
	IEnumerable<ITokenOption> TagCreationOptions { get; }

	/// <summary>
	///		If this is a Natural Document item this defines the Position within the Template where the DocumentItem is parsed from
	/// </summary>
	TextRange Location { get; }

	/// <summary>
	///		Visits this DocumentItem
	/// </summary>
	/// <param name="visitor"></param>
	void Accept(IDocumentItemVisitor visitor);

	void SerializeToXml(XmlWriter writer);
	void DeserializeFromXml(XmlReader reader);
}