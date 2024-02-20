using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Morestachio.Document.Contracts;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Tests.SerilalizerTests.Strategies
{
	public class DocumentSerializerBinaryStrategy : IDocumentSerializerStrategy
	{
		public DocumentSerializerBinaryStrategy()
		{
			//BinarySerializer.TypeFormat = FormatterTypeStyle.TypesWhenNeeded;
		}

		public string SerializeDocumentToText(IDocumentItem obj)
		{
			var binarySerializer = new DataContractSerializer(obj.GetType());

			using (var ms = new MemoryStream())
			{
				binarySerializer.WriteObject(ms, obj);
				return Encoding.UTF8.GetString(ms.ToArray());
			}
		}

		public IDocumentItem DeSerializeDocumentToText(string text, Type expectedType)
		{
			var binarySerializer = new DataContractSerializer(expectedType);

			using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(text)))
			{
				return binarySerializer.ReadObject(ms) as IDocumentItem;
			}
		}

		/// <inheritdoc />
		public string SerializeErrorToText(IMorestachioError obj)
		{
			var BinarySerializer = new DataContractSerializer(obj.GetType());

			using (var ms = new MemoryStream())
			{
				BinarySerializer.WriteObject(ms, obj);
				return Encoding.UTF8.GetString(ms.ToArray());
			}
		}

		/// <inheritdoc />
		public IMorestachioError DeSerializeErrorToText(string text, Type expectedType)
		{
			var binarySerializer = new DataContractSerializer(expectedType);

			using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(text)))
			{
				return binarySerializer.ReadObject(ms) as IMorestachioError;
			}
		}
	}
}