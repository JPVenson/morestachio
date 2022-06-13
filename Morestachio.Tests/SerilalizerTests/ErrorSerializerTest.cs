using System;
using Morestachio.Parsing.ParserErrors;
using Morestachio.Tests.SerilalizerTests.Strategies;
using NUnit.Framework;

namespace Morestachio.Tests.SerilalizerTests
{
	[TestFixture(typeof(DocumentSerializerXmlStrategy))]
	[TestFixture(typeof(DocumentSerializerNewtonsoftJsonStrategy))]
	[TestFixture(typeof(DocumentSerializerBinaryStrategy))]
	public class ErrorSerializerTest
	{	
		public ErrorSerializerTest(Type strategy)
		{
			DocumentSerializerStrategy = Activator.CreateInstance(strategy) as IDocumentSerializerStrategy;
		}

		public IDocumentSerializerStrategy DocumentSerializerStrategy { get; private set; }

		public void SerializeAndDeserialize(IMorestachioError document)
		{
			var text = DocumentSerializerStrategy.SerializeErrorToText(document);
			var deserialized = DocumentSerializerStrategy.DeSerializeErrorToText(text, document.GetType());
			var deserializedText = DocumentSerializerStrategy.SerializeErrorToText(deserialized);
			Assert.That(text, Is.EqualTo(deserializedText));
			Assert.That(document, Is.EqualTo(deserialized), () =>
			{
				return $"Object left is: \r\n" +
					$"\"{text}\" \r\n" +
					$"and right ist \r\n" +
					$"\"{deserializedText}\"" +
					$"";
			});

			Assert.That(deserializedText, Is.EqualTo(text));
		}

		[Test]
		public void InvalidInvalidPathSyntaxError()
		{
			var error = new InvalidPathSyntaxError(new TextRangeExtended(510, 12, new CharacterSnippedLocation(5, 1, "noaaa")),
				"invalid.path.noaaa", "Help me obi wan.");
			SerializeAndDeserialize(error);
		}

		[Test]
		public void InvalidMorestachioSyntaxError()
		{
			var error = new MorestachioSyntaxError(new TextRangeExtended(510, 12, new CharacterSnippedLocation(5, 1, "noaaa")),
				"any op",
				"No op",
				"Foo op",
				"Help me obi wan.");
			SerializeAndDeserialize(error);
		}

		[Test]
		public void InvalidMorestachioUnclosedScopeError()
		{
			var error = new MorestachioUnclosedScopeError(new TextRangeExtended(510, 12, new CharacterSnippedLocation(5, 1, "noaaa")),
				"any op",
				"Help me obi wan.");
			SerializeAndDeserialize(error);
		}

		[Test]
		public void InvalidMorestachioUnopendScopeError()
		{
			var error = new MorestachioUnopendScopeError(new TextRangeExtended(510, 12, new CharacterSnippedLocation(5, 1, "noaaa")),
				"any op",
				"Help me obi wan.");
			SerializeAndDeserialize(error);
		}
	}
}