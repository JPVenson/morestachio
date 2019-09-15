using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using System.Xml.Serialization;
using Morestachio.Document;
using NUnit.Framework;

namespace Morestachio.Tests.Core
{
	[TestFixture]
	public class DocumentTreeFixture
	{
		public XmlSerializer XmlSerializer { get; }

		public DocumentTreeFixture()
		{
			XmlSerializer = new XmlSerializer(typeof(MorestachioDocument));
		}

		public string SerializeXmlToText(object obj)
		{
			using (var ms = new MemoryStream())
			{
				XmlSerializer.Serialize(ms, obj);
				return Encoding.UTF8.GetString(ms.ToArray());
			}
		}

		public IDocumentItem DeSerializeXmlToText(string xmltext)
		{
			using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(xmltext)))
			{
				return XmlSerializer.Deserialize(ms) as IDocumentItem;
			}
		}

		[Test]
		public void TestDocumentIsXmlSerilizable()
		{
			var template = "I am <Text> {{Data.data('test')}}";
			var morestachioDocumentInfo = Parser.ParseWithOptions(new ParserOptions(template));
			var text = SerializeXmlToText(morestachioDocumentInfo.Document);
			var deserialized = DeSerializeXmlToText(text);
		}
	}
}
