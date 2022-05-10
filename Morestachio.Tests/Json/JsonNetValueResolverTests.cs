using Morestachio.Newtonsoft.Json;
using Morestachio.Rendering;
using Morestachio.Helper;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Morestachio.Tests.Json
{
	[TestFixture]
	public class JsonNetValueResolverTests
	{
		[Test]
		public void TestValueResolverCanGetProperty()
		{
			var data = @"{
	Data: {
		PropA: ""Te"",
	},
	PropB: ""st""
}
";
			var template = "{{Data.PropA}}{{PropB}}";

			var options = ParserFixture.TestBuilder().WithTemplate(template).WithValueResolver(new JsonNetValueResolver()).Build();
			var document = Parser.ParseWithOptions(options);
			Assert.That(document.CreateRenderer().Render(JsonConvert.DeserializeObject(data)).Stream.Stringify(true, ParserFixture.DefaultEncoding), Is.EqualTo("Test"));
		}

		[Test]
		public void TestValueResolverCanGetPropertyList()
		{
			var data = @"{
	Data: {
		PropA: [""T"",""e""],
	},
	PropB: ""st""
}
";
			var template = "{{#each Data.PropA}}{{this}}{{/each}}{{PropB}}";
			var options = ParserFixture.TestBuilder().WithTemplate(template).WithValueResolver(new JsonNetValueResolver()).Build();
			var document = Parser.ParseWithOptions(options);
			Assert.That(document.CreateRenderer().Render(JsonConvert.DeserializeObject(data)).Stream.Stringify(true, ParserFixture.DefaultEncoding), Is.EqualTo("Test"));
		}

		[Test]
		public void TestValueResolverCanGetPropertyListAndFormat()
		{
			var data = @"{
	Data: {
		PropA: [""E"", ""T"",""e""],
	},
	PropB: ""st""
}
";
			var template = "{{#each Data.PropA.Skip(1)}}{{this}}{{/each}}{{PropB}}";
			var options = ParserFixture.TestBuilder().WithTemplate(template).WithValueResolver(new JsonNetValueResolver()).Build();
			var document = Parser.ParseWithOptions(options);
			Assert.That(document.CreateRenderer().Render(JsonConvert.DeserializeObject(data)).Stream.Stringify(true, ParserFixture.DefaultEncoding), Is.EqualTo("Test"));
		}
	}
}
