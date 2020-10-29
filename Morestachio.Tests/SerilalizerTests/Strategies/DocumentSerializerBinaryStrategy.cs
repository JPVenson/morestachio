using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Morestachio.Document.Contracts;

namespace Morestachio.Tests.SerilalizerTests.Strategies
{
	public class DocumentSerializerBinaryStrategy : IDocumentSerializerStrategy
	{
		public DocumentSerializerBinaryStrategy()
		{
			//BinarySerializer.TypeFormat = FormatterTypeStyle.TypesWhenNeeded;
		}
		
		public string SerializeToText(IDocumentItem obj)
		{
			var BinarySerializer = new DataContractSerializer(obj.GetType());
			using (var ms = new MemoryStream())
			{
				BinarySerializer.WriteObject(ms, obj);
				return Encoding.UTF8.GetString(ms.ToArray());
			}
		}

		public IDocumentItem DeSerializeToText(string text, Type expectedType)
		{
			var BinarySerializer = new DataContractSerializer(expectedType);
			using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(text)))
			{
				return BinarySerializer.ReadObject(ms) as IDocumentItem;
			}
		}
	}
}