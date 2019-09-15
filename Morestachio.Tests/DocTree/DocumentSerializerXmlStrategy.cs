using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Morestachio.Document;
using Morestachio.Document.Contracts;

namespace Morestachio.Tests.DocTree
{
	public class DocumentSerializerXmlStrategy : IDocumentSerializerStrategy
	{
		public DocumentSerializerXmlStrategy()
		{
			XmlSerializer = new XmlSerializer(typeof(MorestachioDocument));
		}

		public XmlSerializer XmlSerializer { get; private set; }

		public string SerializeToText(IDocumentItem obj)
		{
			using (var ms = new MemoryStream())
			{
				XmlSerializer.Serialize(ms, obj);
				return Encoding.UTF8.GetString(ms.ToArray());
			}
		}

		public IDocumentItem DeSerializeToText(string text)
		{
			using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(text)))
			{
				return XmlSerializer.Deserialize(ms) as IDocumentItem;
			}
		}
	}
}