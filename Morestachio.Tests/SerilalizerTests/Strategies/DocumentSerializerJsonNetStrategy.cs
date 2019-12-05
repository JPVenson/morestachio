using System;
using Morestachio.Document;
using Morestachio.Document.Contracts;
using Newtonsoft.Json;

namespace Morestachio.Tests.DocTree
{
	public class DocumentSerializerJsonNetStrategy : IDocumentSerializerStrategy
	{
		public DocumentSerializerJsonNetStrategy()
		{
			jsonSerializerSettings = new JsonSerializerSettings();
			jsonSerializerSettings.Formatting = Formatting.Indented;
			jsonSerializerSettings.TypeNameHandling = TypeNameHandling.Objects;
		}

		JsonSerializerSettings jsonSerializerSettings;

		public string SerializeToText(IDocumentItem obj)
		{
			return JsonConvert.SerializeObject(obj, jsonSerializerSettings);
		}

		public IDocumentItem DeSerializeToText(string text, Type expectedType)
		{
			return JsonConvert.DeserializeObject(text, expectedType, jsonSerializerSettings)
				as IDocumentItem;
		}
	}
}