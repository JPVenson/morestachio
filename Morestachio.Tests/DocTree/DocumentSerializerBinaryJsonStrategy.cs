using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Morestachio.Document;

namespace Morestachio.Tests.DocTree
{
	public class DocumentSerializerBinaryJsonStrategy : IDocumentSerializerStrategy
	{
		public DocumentSerializerBinaryJsonStrategy()
		{
			BinarySerializer = new BinaryFormatter();
		}

		public BinaryFormatter BinarySerializer { get; private set; }

		public string SerializeToText(IDocumentItem obj)
		{
			using (var ms = new MemoryStream())
			{
				BinarySerializer.Serialize(ms, obj);
				return Encoding.UTF8.GetString(ms.ToArray());
			}
		}

		public IDocumentItem DeSerializeToText(string text)
		{
			using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(text)))
			{
				return BinarySerializer.Deserialize(ms) as IDocumentItem;
			}
		}
	}
}