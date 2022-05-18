using Morestachio.Newtonsoft.Json;
using Newtonsoft.Json;

namespace Morestachio.Tests.Json.Strategies
{
	public class NewtonsoftJsonSerializerStrategy : IJsonSerializerStrategy
	{
		/// <inheritdoc />
		public IParserOptionsBuilder Register(IParserOptionsBuilder options)
		{
			return options.WithJsonValueResolver();
		}

		/// <inheritdoc />
		public object DeSerialize(string text)
		{
			return JsonConvert.DeserializeObject(text);
		}
	}
}