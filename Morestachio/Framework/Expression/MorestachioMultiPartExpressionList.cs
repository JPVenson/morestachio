using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;

namespace Morestachio.Framework.Expression
{
	/// <summary>
	///		Defines an expression that consists of multiple expressions
	/// </summary>
	[Serializable]
	public class MorestachioMultiPartExpressionList : MorestachioExpressionListBase
	{
		
		internal MorestachioMultiPartExpressionList()
		{
			
		}

		/// <inheritdoc />
		public MorestachioMultiPartExpressionList(CharacterLocation location) : base(location)
		{
		}
		
		/// <inheritdoc />
		public MorestachioMultiPartExpressionList(IList<IMorestachioExpression> expressions, CharacterLocation location) : base(expressions, location)
		{
		}
		
		/// <inheritdoc />
		protected MorestachioMultiPartExpressionList(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			EndsWithDelimiter = info.GetBoolean(nameof(EndsWithDelimiter));
		}
		
		/// <inheritdoc />
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue(nameof(EndsWithDelimiter), EndsWithDelimiter);
		}
		
		/// <inheritdoc />
		public override void ReadXml(XmlReader reader)
		{
			EndsWithDelimiter = reader.GetAttribute(nameof(EndsWithDelimiter)) == bool.TrueString;
			base.ReadXml(reader);
		}
		
		/// <inheritdoc />
		public override void WriteXml(XmlWriter writer)
		{
			writer.WriteAttributeString(nameof(EndsWithDelimiter), EndsWithDelimiter.ToString());
			base.WriteXml(writer);
		}

		/// <summary>
		///		Gets whenever this collection of expression where explicitly closed with an delimiter ';'
		/// </summary>
		public bool EndsWithDelimiter { get; set; }
	}
}