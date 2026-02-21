using System.IO;
using System.Xml;
using System.Xml.Linq;
using Morestachio.Formatter.Constants;
using Morestachio.Rendering;
using Morestachio.System.Xml.Linq;
using NUnit.Framework;
using Encoding = System.Text.Encoding;

namespace Morestachio.Tests.Xml
{
	public class XmlValueResolverTests
	{
		[Test]
		public void TestValueResolverCanGetProperty()
		{
			var data = """
                <Root>
                	<Data>
                		<PropA>Te</PropA>
                	</Data>
                	<PropB>st</PropB>
                </Root>

                """;
			var template = "{{Root.Data.PropA}}{{Root.PropB}}";

			var document = ParserFixture.TestBuilder()
				.WithTemplate(template)
				.WithXmlDocumentValueResolver()
				.BuildAndParse();
			Assert.That(
				document.CreateRenderer()
					.RenderAndStringify(XDocument.Load(new MemoryStream(Encoding.UTF8.GetBytes(data)))),
				Is.EqualTo("Test"));
		}

		[Test]
		public void TestValueResolverCanGetPropertyList()
		{
			var data = """
                <Root>
                	<Data>
                		<PropA>T</PropA>
                		<PropA>e</PropA>
                	</Data>
                	<PropB>st</PropB>
                </Root>

                """;
			var template = "{{#EACH Root.Data.PropA}}{{this}}{{/EACH}}{{Root.PropB}}";
			var document = ParserFixture.TestBuilder()
				.WithTemplate(template)
				.WithXmlDocumentValueResolver()
				.BuildAndParse();
			Assert.That(
				document.CreateRenderer()
					.RenderAndStringify(XDocument.Load(new MemoryStream(Encoding.UTF8.GetBytes(data)))),
				Is.EqualTo("Test"));
		}

		[Test]
		public void TestValueResolverCanGetPropertyListAndFormat()
		{
			var data = """
                <Root>
                	<Data>
                		<PropA>E</PropA>
                		<PropA>T</PropA>
                		<PropA>e</PropA>
                	</Data>
                	<PropB>st</PropB>
                </Root>

                """;
			var template = "{{#each Root.Data.PropA.Skip(1)}}{{this}}{{/each}}{{Root.PropB}}";
			var document = ParserFixture.TestBuilder()
				.WithTemplate(template)
				.WithXmlDocumentValueResolver()
				.BuildAndParse();
			Assert.That(
				document.CreateRenderer()
					.RenderAndStringify(XDocument.Load(new MemoryStream(Encoding.UTF8.GetBytes(data)))),
				Is.EqualTo("Test"));
		}
	}
}