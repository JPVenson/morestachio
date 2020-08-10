using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using JetBrains.Annotations;
using Morestachio.Document.Visitor;
using Morestachio.Framework;

#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using Promise = System.Threading.Tasks.ValueTask;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using Promise = System.Threading.Tasks.Task;
#endif

namespace Morestachio.Document.Contracts
{
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
		ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context,
			ScopeData scopeData);
		
		/// <summary>
		///		The list of Children that are children of this Document item
		/// </summary>
		IList<IDocumentItem> Children { get; }

		/// <summary>
		///		Adds the specified childs.
		/// </summary>
		void Add(params IDocumentItem[] documentChildren);

		/// <summary>
		///		If this is a Natural Document item this defines the Position within the Template where the DocumentItem is parsed from
		/// </summary>
		CharacterLocation ExpressionStart { get; set; }

		/// <summary>
		///		Can be used to allow custom data to be serialized for XML serialization
		/// </summary>
		/// <param name="writer"></param>
		void SerializeXmlCore(XmlWriter writer);

		/// <summary>
		///		Can be used to allow custom data to be deserialized for XML serialization
		/// </summary>
		/// <param name="writer"></param>
		void DeSerializeXmlCore(XmlReader writer);

		/// <summary>
		///		Visits this DocumentItem
		/// </summary>
		/// <param name="visitor"></param>
		void Accept(IDocumentItemVisitor visitor);
	}
}