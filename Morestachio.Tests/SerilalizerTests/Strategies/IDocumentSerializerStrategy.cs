using System;
using Morestachio.Document.Contracts;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Tests.SerilalizerTests.Strategies
{
	public interface IDocumentSerializerStrategy
	{
		string SerializeDocumentToText(IDocumentItem obj);
		IDocumentItem DeSerializeDocumentToText(string text, Type expectedType);

		string SerializeErrorToText(IMorestachioError obj);
		IMorestachioError DeSerializeErrorToText(string text, Type expectedType);
	}
}