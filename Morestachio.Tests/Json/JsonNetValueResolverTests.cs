using System;
using Morestachio.Newtonsoft.Json;
using Morestachio.Rendering;
using Morestachio.Helper;
using Morestachio.Tests.Json.Strategies;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Morestachio.Tests.Json
{
#if LastestNet
	[TestFixture(typeof(LazySystemTextJsonSerializerStrategy))]
#endif

	[TestFixture(typeof(NewtonsoftJsonSerializerStrategy))]
	public class JsonNetValueResolverTests
	{
		private readonly IJsonSerializerStrategy _jsonSerializerStrategy;

		public JsonNetValueResolverTests(Type jsonSerializerStrategyType)
		{
			_jsonSerializerStrategy = Activator.CreateInstance(jsonSerializerStrategyType) as IJsonSerializerStrategy;
		}

		[Test]
		public void TestValueResolverCanGetProperty()
		{
			var data = @"{
	""Data"": {
		""PropA"": ""Te"",
	},
	""PropB"": ""st""
}
";
			var template = "{{Data.PropA}}{{PropB}}";

			var document = _jsonSerializerStrategy.Register(ParserFixture.TestBuilder().WithTemplate(template)).BuildAndParse();
			Assert.That(document.CreateRenderer().RenderAndStringify(_jsonSerializerStrategy.DeSerialize(data)), Is.EqualTo("Test"));
		}

		[Test]
		public void TestValueResolverCanGetPropertyList()
		{
			var data = @"{
	""Data"": {
		""PropA"": [""T"",""e""],
	},
	""PropB"": ""st""
}
";
			var template = "{{#each Data.PropA}}{{this}}{{/each}}{{PropB}}";
			var document = _jsonSerializerStrategy.Register(ParserFixture.TestBuilder().WithTemplate(template)).BuildAndParse();
			Assert.That(document.CreateRenderer().RenderAndStringify(_jsonSerializerStrategy.DeSerialize(data)), Is.EqualTo("Test"));
		}

		[Test]
		public void TestValueResolverCanGetPropertyListAndFormat()
		{
			var data = @"{
	""Data"": {
		""PropA"": [""E"", ""T"",""e""],
	},
	""PropB"": ""st""
}
";
			var template = "{{#each Data.PropA.Skip(1)}}{{this}}{{/each}}{{PropB}}";
			var document = _jsonSerializerStrategy.Register(ParserFixture.TestBuilder().WithTemplate(template)).BuildAndParse();
			Assert.That(document.CreateRenderer().RenderAndStringify(_jsonSerializerStrategy.DeSerialize(data)), Is.EqualTo("Test"));
		}
	}
}
