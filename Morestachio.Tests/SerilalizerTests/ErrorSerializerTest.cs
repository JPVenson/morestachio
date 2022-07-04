using System;
using Morestachio.Parsing.ParserErrors;
using Morestachio.Tests.SerilalizerTests.Strategies;
using NUnit.Framework;

namespace Morestachio.Tests.SerilalizerTests;

[TestFixture(typeof(DocumentSerializerXmlStrategy))]
[TestFixture(typeof(DocumentSerializerNewtonsoftJsonStrategy))]
#if NETCOREAPP3_1_OR_GREATER
[TestFixture(typeof(DocumentSerializerSystemTextJsonStrategy))]
#endif
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
		var error = new InvalidPathSyntaxError(new TextRange(TextIndex.Start, new TextIndex(510, 2, 8)),
			"invalid.path.noaaa", "Help me obi wan.");
		SerializeAndDeserialize(error);
	}

	[Test]
	public void InvalidMorestachioSyntaxError()
	{
		var error = new MorestachioSyntaxError(new TextRange(TextIndex.Start, new TextIndex(510, 2, 8)),
			"any op",
			"No op",
			"Foo op",
			"Help me obi wan.");
		SerializeAndDeserialize(error);
	}

	[Test]
	public void InvalidMorestachioUnclosedScopeError()
	{
		var error = new MorestachioUnclosedScopeError(new TextRange(TextIndex.Start, new TextIndex(510, 2, 8)),
			"any op",
			"Help me obi wan.");
		SerializeAndDeserialize(error);
	}

	[Test]
	public void InvalidMorestachioUnopendScopeError()
	{
		var error = new MorestachioUnopendScopeError(new TextRange(TextIndex.Start, new TextIndex(510, 2, 8)),
			"any op",
			"Help me obi wan.");
		SerializeAndDeserialize(error);
	}
}