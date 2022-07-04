using System;
using Morestachio.Document.Contracts;
using Morestachio.Newtonsoft.Json;
using Morestachio.Parsing.ParserErrors;
using Newtonsoft.Json;

namespace Morestachio.Tests.SerilalizerTests.Strategies
{
	public class DocumentSerializerNewtonsoftJsonStrategy : IDocumentSerializerStrategy
	{
		public DocumentSerializerNewtonsoftJsonStrategy()
		{
			jsonSerializerSettings = new JsonSerializerSettings()
				.AddMorestachioSerializationExtensions();
			jsonSerializerSettings.Formatting = Formatting.Indented;
		}

		JsonSerializerSettings jsonSerializerSettings;

		public string SerializeDocumentToText(IDocumentItem obj)
		{
			return JsonConvert.SerializeObject(obj, jsonSerializerSettings);
		}

		public IDocumentItem DeSerializeDocumentToText(string text, Type expectedType)
		{
			return JsonConvert.DeserializeObject(text, expectedType, jsonSerializerSettings)
				as IDocumentItem;
		}

		/// <inheritdoc />
		public string SerializeErrorToText(IMorestachioError obj)
		{
			return JsonConvert.SerializeObject(obj, jsonSerializerSettings);
		}

		/// <inheritdoc />
		public IMorestachioError DeSerializeErrorToText(string text, Type expectedType)
		{
			return JsonConvert.DeserializeObject(text, expectedType, jsonSerializerSettings)
				as IMorestachioError;
		}
	}
}