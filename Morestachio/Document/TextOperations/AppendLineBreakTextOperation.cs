using System;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;

namespace Morestachio.Document.TextOperations;

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

#if Span
		/// <inheritdoc />
		public ReadOnlySpan<char> Apply(ReadOnlySpan<char> value)
		{
			return string.Concat(value, Environment.NewLine.AsSpan());
		}
#endif
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