using System;
using System.Collections.Generic;
using System.Text;
using Morestachio.Newtonsoft.Json;
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
			var options = new ParserOptions(template, null, ParserFixture.DefaultEncoding);
			options.ValueResolver = new JsonNetValueResolver();
			var document = Parser.ParseWithOptions(options);
			Assert.That(document.CreateAndStringify(JsonConvert.DeserializeObject(data)), Is.EqualTo("Test"));
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
			var template = "{{#each Data.PropA}}{{.}}{{/each}}{{PropB}}";
			var options = new ParserOptions(template, null, ParserFixture.DefaultEncoding);
			options.ValueResolver = new JsonNetValueResolver();
			var document = Parser.ParseWithOptions(options);
			Assert.That(document.CreateAndStringify(JsonConvert.DeserializeObject(data)), Is.EqualTo("Test"));
		}
	}
}
