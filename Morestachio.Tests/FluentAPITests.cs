using Morestachio.Document.Items;
using Morestachio.Fluent;
using NUnit.Framework;

namespace Morestachio.Tests
{
	[TestFixture]
	public class FluentAPITests
	{
		public FluentAPITests()
		{

		}

		private MorestachioDocumentInfo GenerateTemplate(string template)
		{
			return Parser.ParseWithOptions(new ParserOptions(template));
		}

		[Test]
		public void TestGetCurrent()
		{
			var api = GenerateTemplate("{{test}}content{{#if test}}data{{/if}}").Fluent();
			api.Current(f => Assert.That(f, Is.TypeOf<MorestachioDocument>()))
				.FindNext<PathDocumentItem>()
				.IfNotSuccess(f => Assert.Fail("Could not find PathDocumentItem"))
				.Current(f => Assert.That(f, Is.TypeOf<PathDocumentItem>()))
				.FindNext<ContentDocumentItem>()
				.IfNotSuccess(f => Assert.Fail("Could not find ContentDocumentItem"))
				.Current(f => Assert.That(f, Is.TypeOf<ContentDocumentItem>()))
				.FindNext<IfExpressionScopeDocumentItem>()
				.IfNotSuccess(f => Assert.Fail("Could not find IfExpressionScopeDocumentItem"))
				.Current(f => Assert.That(f, Is.TypeOf<IfExpressionScopeDocumentItem>()))
				.FindNext<PathDocumentItem>()
				.IfSuccess(f => Assert.Fail("Found PathDocumentItem but should have not"))
				.Current(f => Assert.That(f, Is.TypeOf<IfExpressionScopeDocumentItem>()));
		}

		[Test]
		public void TestCanBuild()
		{
			var template = "{{test}}content{{#IF test}}data{{/IF}}";
			string apiTemplate = null;
			var api = GenerateTemplate("")
				.Fluent()
				.AddPath((builder) => builder.Parse("test"))
				.AddContent("content")
				.AddIfAndEnter(builder => builder.Parse("test"))
					.AddContent("data")
				.Parent()
				.RenderTree(f =>
				{
					apiTemplate = f;
				});
			StringAssert.AreEqualIgnoringCase(template, apiTemplate);
		}
	}
}
