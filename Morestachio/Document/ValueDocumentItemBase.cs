using System.Runtime.Serialization;
using System.Xml;

namespace Morestachio.Document
{
	[System.Serializable]
	public abstract class ValueDocumentItemBase : DocumentItemBase
	{
		protected ValueDocumentItemBase()
		{
			
		}

		public string Value { get; protected set; }

		protected ValueDocumentItemBase(SerializationInfo info, StreamingContext c) : base(info, c)
		{
			Value = info.GetString(nameof(Value));
		}

		protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
		{
			base.SerializeBinaryCore(info, context);
			info.AddValue(nameof(Value), Value);
		}

		protected override void SerializeXml(XmlWriter writer)
		{
			if (string.IsNullOrEmpty(Value))
			{
				return;
			}
			writer.WriteStartElement(nameof(Value));
			writer.WriteAttributeString("xml", "space", null, "preserve");
			writer.WriteString(Value);
			writer.WriteEndElement();
		}

		protected override void DeSerializeXml(XmlReader reader)
		{
			reader.ReadStartElement();
			if (reader.Name == nameof(Value))
			{
				if (reader.IsEmptyElement)
				{
					return;
				}
				//reader.ReadToFollowing(nameof(Value));
				Value = reader.ReadString();
			}
		}
	}
}