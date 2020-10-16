using System;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using Morestachio.Framework.Tokenizing;

namespace Morestachio.Document.TextOperations
{
	/// <summary>
	///		Removes a number of linebreaks 
	/// </summary>
	[Serializable]
	public class TrimLineBreakTextOperation : ITextOperation
	{
		/// <summary>
		///		Creates a new <see cref="TrimLineBreakTextOperation"/>
		/// </summary>
		public TrimLineBreakTextOperation()
		{
			TransientEdit = true;
			IsModificator = true;
			TextOperationType = TextOperationTypes.TrimLineBreaks;
			LineBreakTrimDirection = LineBreakTrimDirection.Begin;
		}
		
		/// <inheritdoc />
		protected TrimLineBreakTextOperation(SerializationInfo info, StreamingContext c) : this()
		{
			LineBreaks = info.GetInt32(nameof(LineBreaks));
			LineBreakTrimDirection = (LineBreakTrimDirection)info.GetValue(nameof(LineBreakTrimDirection), typeof(LineBreakTrimDirection));
		}
		
		/// <inheritdoc />
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(nameof(LineBreaks), LineBreaks);
			info.AddValue(nameof(LineBreakTrimDirection), LineBreakTrimDirection);
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
			LineBreakTrimDirection = (LineBreakTrimDirection) Enum.Parse(typeof(LineBreakTrimDirection), reader.GetAttribute(nameof(LineBreakTrimDirection)));
		}
		
		/// <inheritdoc />
		public void WriteXml(XmlWriter writer)
		{
			writer.WriteAttributeString(nameof(LineBreaks), LineBreaks.ToString());
			writer.WriteAttributeString(nameof(LineBreakTrimDirection), LineBreakTrimDirection.ToString());
		}

		/// <summary>
		///		The Number of LineBreaks to be removed as 0
		/// </summary>
		public int LineBreaks { get; set; }
		
		/// <inheritdoc />
		public TextOperationTypes TextOperationType { get; }

		/// <summary>
		///		The direction of the line trim operation
		/// </summary>
		public LineBreakTrimDirection LineBreakTrimDirection { get; set; }
		
		/// <inheritdoc />
		public bool TransientEdit { get; }
		
		/// <inheritdoc />
		public bool IsModificator { get; }
		
		/// <inheritdoc />
		public string Apply(string value)
		{
			var breaksFound = 0;
			if (LineBreaks == 0 && LineBreakTrimDirection == LineBreakTrimDirection.Begin)
			{
				return value.TrimStart(Tokenizer.GetWhitespaceDelimiters());
			}
			if (LineBreaks == 0 && LineBreakTrimDirection == LineBreakTrimDirection.End)
			{
				return value.TrimEnd(Tokenizer.GetWhitespaceDelimiters());
			}
			for (int i = 0; i < value.Length; i++)
			{
				var c = value[i];
				if (Tokenizer.IsWhiteSpaceDelimiter(c))
				{
					if (LineBreaks == ++breaksFound)
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

	/// <summary>
	///		Indicates the position of where the linebreaks should be removed with the <see cref="TrimLineBreakTextOperation"/>
	/// </summary>
	[Flags]
	public enum LineBreakTrimDirection
	{
		/// <summary>
		///		Default none
		/// </summary>
		None = 0,
		/// <summary>
		///		Should trim all linebreaks at the start of the next content
		/// </summary>
		Begin = 1 << 0,

		/// <summary>
		///		Should tirm all linebreaks at the end of the next content
		/// </summary>
		End = 1 << 1
	}

	public static class LineBreakTrimDirectionExtensions
	{
		public static bool HasFlagFast(this LineBreakTrimDirection value, LineBreakTrimDirection flag)
		{
			return (value & flag) != 0;
		}
	}
}