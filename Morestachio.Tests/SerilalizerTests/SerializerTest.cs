using System;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Tokenizing;
using Morestachio.Helper.Localization;
using Morestachio.TemplateContainers;
using Morestachio.Tests.SerilalizerTests.Strategies;
using NUnit.Framework;

namespace Morestachio.Tests.SerilalizerTests
{
	[TestFixture(typeof(DocumentSerializerXmlStrategy))]
	[TestFixture(typeof(DocumentSerializerNewtonsoftJsonStrategy))]
	[TestFixture(typeof(DocumentSerializerBinaryStrategy))]
	public class SerializerTest
	{
		public SerializerTest(Type strategy)
		{
			DocumentSerializerStrategy = Activator.CreateInstance(strategy) as IDocumentSerializerStrategy;
		}

		public IDocumentSerializerStrategy DocumentSerializerStrategy { get; private set; }

		public static void AssertDocumentItemIsSameAsTemplate(ITemplateContainer textContainer, 
			IDocumentItem documentItem, 
			ParserOptions options)
		{
			var text = (textContainer as StringTemplateContainer).Template;
			AssertDocumentItemIsSameAsTemplate(text, documentItem, options);
		}

		public static void AssertDocumentItemIsSameAsTemplate(string text, IDocumentItem documentItem, ParserOptions options)
		{
			var visitor = new ToParsableStringDocumentVisitor(options);
			visitor.Visit(documentItem as MorestachioDocument);

			var format = visitor.StringBuilder.ToString();
			StringAssert.AreEqualIgnoringCase(text, format);
			if (!format.Equals(text))
			{
				if (!format.Equals(text, StringComparison.InvariantCultureIgnoreCase))
				{
					Assert.That(format, Is.EqualTo(text));
				}

				Console.WriteLine("The two string are only the same of not checked for casing");
			}
		}

		public void SerilalizeAndDeserialize(IDocumentItem document)
		{
			var text = DocumentSerializerStrategy.SerializeToText(document);
			var deserialized = DocumentSerializerStrategy.DeSerializeToText(text, document.GetType());
			var deserializedText = DocumentSerializerStrategy.SerializeToText(deserialized);
			Assert.That(text, Is.EqualTo(deserializedText));
			Assert.That(document, Is.EqualTo(deserialized), () =>
				{
					return $"Object left is: \r\n" +
					       $"\"{text}\" \r\n" +
					       $"and right ist \r\n" +
					       $"\"{deserializedText}\"" +
					       $"";
				});

			Assert.That(deserializedText, Is.EqualTo(text));
		}

		[Test]
		public void TestIsContentWithPathAndFormatterSerializable()
		{
			var template = "I am <Text> {{Data.data.test().next(arg).(last)}}";
			TestSerializableDocument(template);
		}

		private void TestSerializableDocument(string template)
		{
			var options = new ParserOptions(template);
			var morestachioDocumentInfo = Parser.ParseWithOptions(options);
			SerilalizeAndDeserialize(morestachioDocumentInfo.Document);
			AssertDocumentItemIsSameAsTemplate(template, morestachioDocumentInfo.Document, options);
		}

		[Test]
		public void TestIsScopeIsSerializable()
		{
			var template = "I am <Text> {{#Data.data.test().next(arg).(last) AS arg}} test {{/arg}}";
			TestSerializableDocument(template);
		}

		[Test]
		public void TestIsInvertScopeIsSerializable()
		{
			var template = "I am <Text> {{^Data.data.test().next(arg).(last) AS arg}} test {{/arg}}";
			TestSerializableDocument(template);
		}

		[Test]
		public void TestIsVariableSerializable()
		{
			var template = "I am <Text> {{#VAR f = data.test.Format().As.Test('', exp)}}";
			TestSerializableDocument(template);
		}

		[Test]
		public void TestIsContentSerializable()
		{
			var template = "I am <Text>";
			TestSerializableDocument(template);
		}

		[Test]
		public void TestIsContentWithPathAndEachAndFormatterSerializable()
		{
			var template = "I am <Text> {{#EACH data.('', dd)}} {{Data.data.test()}} {{/EACH}}";
			TestSerializableDocument(template);
		}

		[Test]
		public void TestIsContentWithPathAndEveryAndFormatterSerializable()
		{
			var template = "I am <Text> {{#EACH data.('', dd).?}} {{Data.data.test()}} {{/EACH}}";
			TestSerializableDocument(template);
		}

		[Test]
		public void TestCanSerializeSwitch()
		{
			var template =
				"{{#SWITCH data}}" +
				"{{#CASE 'tset'}}FAIL{{/CASE}}" +
				"{{#CASE 123}}FAIL{{/CASE}}" +
				"{{#CASE root}}FAIL{{/CASE}}" +
				"{{#CASE 'test'}}SUCCESS{{/CASE}}" +
				"{{/SWITCH}}";
			TestSerializableDocument(template);
		}

		[Test]
		public void TestCanSerializeSwitchWithContext()
		{
			var template =
				"{{#SWITCH data #SCOPE}}" +
				"{{#CASE 'tset'}}FAIL-{{.}}{{/CASE}}" +
				"{{#CASE 123}}FAIL-{{.}}{{/CASE}}" +
				"{{#CASE root}}FAIL-{{.}}{{/CASE}}" +
				"{{#CASE 'test'}}SUCCESS-{{.}}{{/CASE}}" +
				"{{/SWITCH}}";
			TestSerializableDocument(template);
		}

		[Test]
		public void TestCanSerializePartial()
		{
			var template = "Partial:" +
						   "{{#DECLARE PartialA}}" +
						   "I am <Text> {{Data.data('test')}}" +
						   "{{/DECLARE}}" +
						   "{{#IMPORT 'PartialA}'}}";
			TestSerializableDocument(template);
		}

		[Test]
		public void TestCanSerializePartialWithContext()
		{
			var template = "Partial:" +
						   "{{#DECLARE PartialA}}" +
						   "{{#LET test = 'test'}}" +
						   "I am <Text> {{Data.data('test')}}" +
						   "{{/DECLARE}}" +
						   "{{#IMPORT 'PartialA' #WITH data.extra()}}";
			TestSerializableDocument(template);
		}

		[Test]
		public void TestCanSerializeRepeat()
		{
			var template = "{{#VAR condition = 50}}" +
						   "{{#REPEAT condition}}" +
						   "{{#LET test = 'test'}}" +
						   "{{$index}}," +
						   "{{#IF condition.Equals(5)}}{{#VAR condition = false}}{{/IF}}" +
						   "{{/REPEAT}}";
			TestSerializableDocument(template);
		}

		[Test]
		public void TestCanSerializeWhile()
		{
			var template = "{{#VAR condition = true}}" +
						   "{{#WHILE condition}}" +
						   "{{#LET test = 'test'}}" +
						   "{{$index}}," +
						   "{{#IF condition.Equals(5)}}{{#VAR condition = false}}{{/IF}}" +
						   "{{/WHILE}}";
			TestSerializableDocument(template);
		}

		[Test]
		public void TestCanSerializeDo()
		{
			var template = "{{#VAR condition = true}}" +
						   "{{#DO condition}}" +
						   "{{#LET test = 'test'}}" +
						   "{{$index}}," +
						   "{{#IF condition.Equals(5)}}{{#VAR condition = false}}{{/IF}}" +
						   "{{/DO}}";
			TestSerializableDocument(template);
		}

		[Test]
		public void TestIsIfNotIfSerializable()
		{
			var template = "I am <Text> {{#IF data}}" +
						   "{{#LET test = 'test'}}" +
						   " {{/IF}} {{^IF data}} {{/IF}}";
			TestSerializableDocument(template);
		}

		[Test]
		public void TestIsIfElseIsSerializable()
		{
			var template = "I am <Text> {{#IF data}}" +
						   "{{#LET test = 'test'}}" +
						   " {{/IF}} {{#ELSE}}" +
						   "{{#LET test = 'test'}}" +
						   " {{/ELSE}}";
			TestSerializableDocument(template);
		}

		[Test]
		public void TestLocIsSerializable()
		{
			var template = "{{#loc 'Texts.Welcome' #CULTURE 'DE-DE'}} " +
						   "{{#LocCulture 'de-AT'}}" +
						   "{{#loc 'Texts.Welcome'}} " +
						   "{{/LocCulture}}" +

						   "{{#loc 'Texts.Welcome'}} " +

						   "{{#LocCulture 'de-DE'}}" +
						   "{{#loc 'Texts.Welcome'}} " +

						   "{{#LocCulture 'de-AT'}}" +
						   "{{#loc 'Texts.Welcome'}} " +
						   "{{/LocCulture}}" +

						   "{{#loc 'Texts.Welcome'}}" +
						   "{{/LocCulture}}" +
						   "{{#LOCP 'test'}}" +
						   "{{#LOCPARAM 'ParamA'}}" +
						   "{{/LOCP}}";
			var options = new ParserOptions(template)
				.RegisterLocalizationService(
					() =>
					{
						return new MorestachioLocalizationService();
					});
			var morestachioDocumentInfo = Parser.ParseWithOptions(options);
			SerilalizeAndDeserialize(morestachioDocumentInfo.Document);
			AssertDocumentItemIsSameAsTemplate(template, morestachioDocumentInfo.Document, options);
		}

		[Test]
		public void Alias()
		{
			var alias = new AliasDocumentItem(CharacterLocation.Unknown, "Alias", 101, null);
			SerilalizeAndDeserialize(alias);
		}
	}
}
