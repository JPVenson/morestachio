using System;
using NUnit.Framework;

namespace Morestachio.Tests.DocTree
{
	[TestFixture (typeof(DocumentSerializerXmlStrategy))]
	[TestFixture (typeof(DocumentSerializerJsonNetStrategy))]
	[TestFixture (typeof(DocumentSerializerBinaryJsonStrategy))]
	public class DocumentTreeFixture
	{
		public IDocumentSerializerStrategy DocumentSerializerStrategy { get; private set; }

		public DocumentTreeFixture(Type strategy)
		{
			DocumentSerializerStrategy = Activator.CreateInstance(strategy) as IDocumentSerializerStrategy;
		}

		[Test]
		public void TestIsContentWithPathAndFormatterSerializable()
		{
			var template = "I am <Text> {{Data.data('test')}}";
			var morestachioDocumentInfo = Parser.ParseWithOptions(new ParserOptions(template));
			var text = DocumentSerializerStrategy.SerializeToText(morestachioDocumentInfo.Document);
			var deserialized = DocumentSerializerStrategy.DeSerializeToText(text);
			var deserializedText = DocumentSerializerStrategy.SerializeToText(deserialized);
			Assert.That(deserializedText, Is.EqualTo(text));
		}

		[Test]
		public void TestCanSerializePartial()
		{
			var template = "Partial:" +
			               "{{#declare PartialA}}" +
						   "I am <Text> {{Data.data('test')}}" +
			               "{{/declare}}" +
			               "{{#Include PartialA}}";
			var morestachioDocumentInfo = Parser.ParseWithOptions(new ParserOptions(template));
			var text = DocumentSerializerStrategy.SerializeToText(morestachioDocumentInfo.Document);
			var deserialized = DocumentSerializerStrategy.DeSerializeToText(text);

			var deserializedText = DocumentSerializerStrategy.SerializeToText(deserialized);
			Assert.That(deserializedText, Is.EqualTo(text));
		}
	}
}
