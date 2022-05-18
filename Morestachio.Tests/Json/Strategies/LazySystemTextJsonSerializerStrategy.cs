
#if LastestNet
using System.Text.Json;
using Morestachio.System.Text.Json;

namespace Morestachio.Tests.Json.Strategies
{
	public class LazySystemTextJsonSerializerStrategy : IJsonSerializerStrategy
	{
		/// <inheritdoc />
		public IParserOptionsBuilder Register(IParserOptionsBuilder options)
		{
			return options.WithSystemTextJsonValueResolver();
		}

		/// <inheritdoc />
		public object DeSerialize(string text)
		{
			return JsonDocument.Parse(text, new JsonDocumentOptions()
			{
				AllowTrailingCommas = true
			});
		}
	}
}
#endif