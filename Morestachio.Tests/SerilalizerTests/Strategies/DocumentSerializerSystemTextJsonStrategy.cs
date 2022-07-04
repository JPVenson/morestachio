#if NETCOREAPP3_1_OR_GREATER

using System;
using Morestachio.Document.Contracts;
using Morestachio.Parsing.ParserErrors;
using System.Text.Json;
using Morestachio.System.Text.Json;

namespace Morestachio.Tests.SerilalizerTests.Strategies;

public class DocumentSerializerSystemTextJsonStrategy : IDocumentSerializerStrategy
{
	private JsonSerializerOptions _serializerOptions;

	public DocumentSerializerSystemTextJsonStrategy()
	{
		_serializerOptions = new JsonSerializerOptions()
		{
			WriteIndented = true
		}.AddMorestachioSerializationExtensions();
	}
	
	public string SerializeDocumentToText(IDocumentItem obj)
	{
		return JsonSerializer.Serialize(obj, _serializerOptions);
	}

	public IDocumentItem DeSerializeDocumentToText(string text, Type expectedType)
	{
		return JsonSerializer.Deserialize(text, expectedType, _serializerOptions) as IDocumentItem;
	}

	/// <inheritdoc />
	public string SerializeErrorToText(IMorestachioError obj)
	{
		return JsonSerializer.Serialize(obj, _serializerOptions);
	}

	/// <inheritdoc />
	public IMorestachioError DeSerializeErrorToText(string text, Type expectedType)
	{
		return JsonSerializer.Deserialize(text, expectedType, _serializerOptions) as IMorestachioError;
	}
}
#endif