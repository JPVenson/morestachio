using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Tests.SerilalizerTests.Strategies
{
	public class DocumentSerializerXmlStrategy : IDocumentSerializerStrategy
	{
		public DocumentSerializerXmlStrategy()
		{
		}

		public string SerializeDocumentToText(IDocumentItem obj)
		{
			var inheritedTypes = typeof(MorestachioDocument).Assembly.GetTypes().Where(e => e.IsClass)
				.Where(e => typeof(IDocumentItem).IsAssignableFrom(e)).ToArray();
			var xmlSerializer = new XmlSerializer(obj.GetType(), inheritedTypes);

			using (var ms = new MemoryStream())
			{
				xmlSerializer.Serialize(ms, obj);
				return Encoding.UTF8.GetString(ms.ToArray());
			}
		}

		public IDocumentItem DeSerializeDocumentToText(string text, Type expectedType)
		{
			var inheritedTypes = typeof(MorestachioDocument).Assembly.GetTypes().Where(e => e.IsClass)
				.Where(e => typeof(IDocumentItem).IsAssignableFrom(e)).ToArray();
			var xmlSerializer = new XmlSerializer(expectedType, inheritedTypes);

			using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(text)))
			{
				return xmlSerializer.Deserialize(ms) as IDocumentItem;
			}
		}

		/// <inheritdoc />
		public string SerializeErrorToText(IMorestachioError obj)
		{
			var inheritedTypes = typeof(IMorestachioError).Assembly.GetTypes().Where(e => e.IsClass)
				.Where(e => typeof(IMorestachioError).IsAssignableFrom(e)).ToArray();
			var xmlSerializer = new XmlSerializer(obj.GetType(), inheritedTypes);

			using (var ms = new MemoryStream())
			{
				xmlSerializer.Serialize(ms, obj);
				return Encoding.UTF8.GetString(ms.ToArray());
			}
		}

		/// <inheritdoc />
		public IMorestachioError DeSerializeErrorToText(string text, Type expectedType)
		{
			var inheritedTypes = typeof(IMorestachioError).Assembly.GetTypes().Where(e => e.IsClass)
				.Where(e => typeof(IMorestachioError).IsAssignableFrom(e)).ToArray();
			var xmlSerializer = new XmlSerializer(expectedType, inheritedTypes);

			using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(text)))
			{
				return xmlSerializer.Deserialize(ms) as IMorestachioError;
			}
		}
	}
}