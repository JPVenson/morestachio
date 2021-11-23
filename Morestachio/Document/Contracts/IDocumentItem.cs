using System.Xml;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;

namespace Morestachio.Document.Contracts
{
	/// <summary>
	///		Should be implemented to allow custom usage reporting on the structure of a DocumentItem for the <see cref="Morestachio.Analyzer.DataAccess.DataAccessAnalyzer"/>
	/// </summary>
	public interface IReportUsage
	{
		/// <summary>
		///		Gets all paths that will called
		/// </summary>
		/// <returns></returns>
		IEnumerable<string> Usage(UsageData data);
	}

	public class UsageData
	{
		public UsageData()
		{
			VariableSource = new Dictionary<string, string>();
			_scopes = new Stack<string>();
		}

		public IDictionary<string, string> VariableSource { get; set; }

		public string CurrentPath
		{
			get { return _scopes.Count == 0 ? null : _scopes.Peek(); }
		}

		private Stack<string> _scopes;

		public UsageData ScopeTo(string currentPath)
		{
			_scopes.Push(currentPath);
			return this;
		}

		public UsageData PopScope(string currentExpectedPath)
		{
			if (_scopes.Pop() != currentExpectedPath)
			{
				throw new InvalidOperationException($"Popped an unexpected scope while evaluating the usage. The document might be malformed or custom document item does not properly implement {nameof(IReportUsage)}");
			}
			return this;
		}
	}

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
		///		Contains the list of TokenOptions used to construct this DocumentItem.
		/// </summary>
		IEnumerable<ITokenOption> TagCreationOptions { get; }

		/// <summary>
		///		If this is a Natural Document item this defines the Position within the Template where the DocumentItem is parsed from
		/// </summary>
		CharacterLocation ExpressionStart { get; }

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