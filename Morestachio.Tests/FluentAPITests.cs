using Morestachio.Document.Items;
using Morestachio.Fluent;
using NUnit.Framework;

namespace Morestachio.Tests
{
	[TestFixture]
	[Parallelizable(ParallelScope.All)]
	public class FluentAPITests
	{
		public FluentAPITests()
		{

		}

		private MorestachioDocumentInfo GenerateTemplate(string template)
		{
			return Parser.ParseWithOptions(ParserFixture.TestBuilder().WithTemplate(template).Build());
		}

		[Test]
		public void TestGetCurrent()
		{
			var morestachioDocumentInfo = GenerateTemplate("{{test}}content{{#if test}}data{{/if}}");
			var api = morestachioDocumentInfo.Fluent();
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
			ParserFixture.TestLocationsInOrder(morestachioDocumentInfo);
		}

		[Test]
		public void TestCanRenderTree()
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

		[Test]
		public void TestCanBuildExpression()
		{
			var template = "{{test.data.format(argA, [delta] 123, null, ~test.data)}}content";
			string apiTemplate = null;
			var api = GenerateTemplate("")
				.Fluent()
				.AddPath((builder) =>
					builder
						.Property("test")
						.Property("data")
						.Call("format", argumentBuilder =>
							argumentBuilder
								.Argument(null, f => f.Property("argA"))
								.Argument("delta", f => f.Number(123))
								.Argument(null, f => f.Null())
								.Argument(null, f => f.GoToRoot().Property("test.data"))
							)
					)
				.AddContent("content")
				.Root()
				.RenderTree(f =>
				{
					apiTemplate = f;
				});
			
			StringAssert.AreEqualIgnoringCase(template, apiTemplate);
		}
	}
}
