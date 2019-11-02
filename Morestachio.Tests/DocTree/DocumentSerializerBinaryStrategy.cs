using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Morestachio.Document;
using Morestachio.Document.Contracts;

namespace Morestachio.Tests.DocTree
{
	public class DocumentSerializerBinaryStrategy : IDocumentSerializerStrategy
	{
		public DocumentSerializerBinaryStrategy()
		{
			BinarySerializer = new DataContractSerializer(typeof(MorestachioDocument));

			//BinarySerializer.TypeFormat = FormatterTypeStyle.TypesWhenNeeded;
		}

		public DataContractSerializer BinarySerializer { get; private set; }

		public string SerializeToText(IDocumentItem obj)
		{
			using (var ms = new MemoryStream())
			{
				BinarySerializer.WriteObject(ms, obj);
				return Encoding.UTF8.GetString(ms.ToArray());
			}
		}

		public IDocumentItem DeSerializeToText(string text)
		{
			using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(text)))
			{
				return BinarySerializer.ReadObject(ms) as IDocumentItem;
			}
		}
	}
}