using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;

namespace Morestachio.Document.TextOperations
{
	/// <summary>
	///		Trims all Whitespaces from a content
	/// </summary>
	public class TrimAllWhitespacesTextOperation : ITextOperation
	{
		/// <summary>
		/// 
		/// </summary>
		public TrimAllWhitespacesTextOperation()
		{
			TextOperationType = TextOperationTypes.ContinuesTrimming;
			TransientEdit = false;
			IsModificator = true;
		}
		
		/// <inheritdoc />
		public XmlSchema GetSchema()
		{
			throw new NotImplementedException();
		}
		
		/// <inheritdoc />
		public void ReadXml(XmlReader reader)
		{
		}
		
		/// <inheritdoc />
		public void WriteXml(XmlWriter writer)
		{
		}
		
		/// <inheritdoc />
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
		}
		
		/// <inheritdoc />
		public TextOperationTypes TextOperationType { get; }
		/// <inheritdoc />
		public bool TransientEdit { get; }
		/// <inheritdoc />
		public bool IsModificator { get; }
		/// <inheritdoc />
		public string Apply(string value)
		{
			var lines = value.Split('\n');
			var nLines = new string[lines.Length];
			for (var index = 0; index < lines.Length; index++)
			{
				var line = lines[index];
				nLines[index] = line.TrimStart('\t');
			}

			return string.Join("", nLines);
		}
	}

	/// <summary>
	///		Adds one LineBreak
	/// </summary>
	[Serializable]
	public class AppendLineBreakTextOperation : ITextOperation
	{
		/// <summary>
		/// 
		/// </summary>
		public AppendLineBreakTextOperation()
		{
			TransientEdit = true;
			IsModificator = false;
			TextOperationType = TextOperationTypes.LineBreak;
		}

		/// <inheritdoc />
		protected AppendLineBreakTextOperation(SerializationInfo info, StreamingContext c) : this()
		{
		}
		
		/// <inheritdoc />
		public XmlSchema GetSchema()
		{
			throw new NotImplementedException();
		}
		
		/// <inheritdoc />
		public void ReadXml(XmlReader reader)
		{
		}
		
		/// <inheritdoc />
		public void WriteXml(XmlWriter writer)
		{
		}
		
		/// <inheritdoc />
		public TextOperationTypes TextOperationType { get; }
		
		/// <inheritdoc />
		public bool TransientEdit { get; }
		
		/// <inheritdoc />
		public bool IsModificator { get; }
		
		/// <inheritdoc />
		public string Apply(string value)
		{
			return value + Environment.NewLine;
		}
		
		/// <inheritdoc />
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
		}
	}
}
