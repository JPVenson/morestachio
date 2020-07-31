using System;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;

namespace Morestachio.Document.TextOperations
{
	/// <summary>
	///		Removes a number of linebreaks 
	/// </summary>
	[Serializable]
	public class TrimLineBreakTextOperation : ITextOperation
	{
		/// <inheritdoc />
		public TrimLineBreakTextOperation()
		{
			TransientEdit = true;
			IsModificator = true;
			TextOperationType = TextOperationTypes.TrimLineBreaks;
		}
		
		/// <inheritdoc />
		protected TrimLineBreakTextOperation(SerializationInfo info, StreamingContext c) : this()
		{
			LineBreaks = info.GetInt32(nameof(LineBreaks));
		}
		
		/// <inheritdoc />
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(nameof(LineBreaks), LineBreaks);
		}
		
		/// <inheritdoc />
		public XmlSchema GetSchema()
		{
			throw new NotImplementedException();
		}
		
		/// <inheritdoc />
		public void ReadXml(XmlReader reader)
		{
			LineBreaks = int.Parse(reader.GetAttribute(nameof(LineBreaks)));
		}
		
		/// <inheritdoc />
		public void WriteXml(XmlWriter writer)
		{
			writer.WriteAttributeString(nameof(LineBreaks), LineBreaks.ToString());
		}

		/// <summary>
		///		The Number of LineBreaks to be removed as -1 indicates all
		/// </summary>
		public int LineBreaks { get; set; }
		
		/// <inheritdoc />
		public TextOperationTypes TextOperationType { get; }
		
		/// <inheritdoc />
		public bool TransientEdit { get; }
		
		/// <inheritdoc />
		public bool IsModificator { get; }
		
		/// <inheritdoc />
		public string Apply(string value)
		{
			var breaksFound = 0;
			for (int i = 0; i < value.Length; i++)
			{
				var c = value[i];
				if (c == '\r' || c == '\n')
				{
					if (LineBreaks != -1 && LineBreaks == ++breaksFound)
					{
						if (value.Length + 1 >= i)
						{
							return value.Substring(i + 1);
						}
			
						return string.Empty;
					}
				}
				else
				{
					return value.Substring(i);
				}
			}
			return string.Empty;
		}
	}
}