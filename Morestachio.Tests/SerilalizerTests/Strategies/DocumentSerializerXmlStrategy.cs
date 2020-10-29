using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items;

namespace Morestachio.Tests.SerilalizerTests.Strategies
{
	public class DocumentSerializerXmlStrategy : IDocumentSerializerStrategy
	{
		public DocumentSerializerXmlStrategy()
		{
		}
		
		public string SerializeToText(IDocumentItem obj)
		{
			var devidedTypes = typeof(MorestachioDocument).Assembly.GetTypes().Where(e => e.IsClass)
				.Where(e => typeof(IDocumentItem).IsAssignableFrom(e)).ToArray();
			var XmlSerializer = new XmlSerializer(obj.GetType(), devidedTypes);

			using (var ms = new MemoryStream())
			{
				XmlSerializer.Serialize(ms, obj);
				return Encoding.UTF8.GetString(ms.ToArray());
			}
		}

		public IDocumentItem DeSerializeToText(string text, Type expectedType)
		{
			var devidedTypes = typeof(MorestachioDocument).Assembly.GetTypes().Where(e => e.IsClass)
				.Where(e => typeof(IDocumentItem).IsAssignableFrom(e)).ToArray();
			var XmlSerializer = new XmlSerializer(expectedType, devidedTypes);

			using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(text)))
			{
				return XmlSerializer.Deserialize(ms) as IDocumentItem;
			}
		}
	}
}