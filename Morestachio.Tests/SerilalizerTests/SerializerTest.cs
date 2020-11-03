using System;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Helper.Localization;
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

		public static void AssertDocumentItemIsSameAsTemplate(string text, IDocumentItem documentItem)
		{
			var visitor = new ToParsableStringDocumentVisitor();
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

		private void SerilalizeAndDeserialize(IDocumentItem document)
		{
			var text = DocumentSerializerStrategy.SerializeToText(document);
			var deserialized = DocumentSerializerStrategy.DeSerializeToText(text, document.GetType());
			var deserializedText = DocumentSerializerStrategy.SerializeToText(deserialized);
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
			var morestachioDocumentInfo = Parser.ParseWithOptions(new ParserOptions(template));
			SerilalizeAndDeserialize(morestachioDocumentInfo.Document);
			AssertDocumentItemIsSameAsTemplate(template, morestachioDocumentInfo.Document);
		}

		[Test]
		public void TestIsScopeIsSerializable()
		{
			var template = "I am <Text> {{#Data.data.test().next(arg).(last) AS arg}} test {{/arg}}";
			var morestachioDocumentInfo = Parser.ParseWithOptions(new ParserOptions(template));
			SerilalizeAndDeserialize(morestachioDocumentInfo.Document);
			AssertDocumentItemIsSameAsTemplate(template, morestachioDocumentInfo.Document);
		}

		[Test]
		public void TestIsInvertScopeIsSerializable()
		{
			var template = "I am <Text> {{^Data.data.test().next(arg).(last) AS arg}} test {{/arg}}";
			var morestachioDocumentInfo = Parser.ParseWithOptions(new ParserOptions(template));
			SerilalizeAndDeserialize(morestachioDocumentInfo.Document);
			AssertDocumentItemIsSameAsTemplate(template, morestachioDocumentInfo.Document);
		}

		[Test]
		public void TestIsVariableSerializable()
		{
			var template = "I am <Text> {{#VAR f = data.test.Format().As.Test('', exp)}}";
			var morestachioDocumentInfo = Parser.ParseWithOptions(new ParserOptions(template));
			SerilalizeAndDeserialize(morestachioDocumentInfo.Document);
			AssertDocumentItemIsSameAsTemplate(template, morestachioDocumentInfo.Document);
		}

		[Test]
		public void TestIsContentSerializable()
		{
			var template = "I am <Text>";
			var morestachioDocumentInfo = Parser.ParseWithOptions(new ParserOptions(template));
			SerilalizeAndDeserialize(morestachioDocumentInfo.Document);
			AssertDocumentItemIsSameAsTemplate(template, morestachioDocumentInfo.Document);
		}

		[Test]
		public void TestIsContentWithPathAndEachAndFormatterSerializable()
		{
			var template = "I am <Text> {{#EACH data.('', dd)}} {{Data.data.test()}} {{/EACH}}";
			var morestachioDocumentInfo = Parser.ParseWithOptions(new ParserOptions(template));
			SerilalizeAndDeserialize(morestachioDocumentInfo.Document);
			AssertDocumentItemIsSameAsTemplate(template, morestachioDocumentInfo.Document);
		}

		[Test]
		public void TestIsContentWithPathAndEveryAndFormatterSerializable()
		{
			var template = "I am <Text> {{#EACH data.('', dd).?}} {{Data.data.test()}} {{/EACH}}";
			var morestachioDocumentInfo = Parser.ParseWithOptions(new ParserOptions(template));
			SerilalizeAndDeserialize(morestachioDocumentInfo.Document);
			AssertDocumentItemIsSameAsTemplate(template, morestachioDocumentInfo.Document);
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

			var morestachioDocumentInfo = Parser.ParseWithOptions(new ParserOptions(template));
			SerilalizeAndDeserialize(morestachioDocumentInfo.Document);
			AssertDocumentItemIsSameAsTemplate(template, morestachioDocumentInfo.Document);
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

			var morestachioDocumentInfo = Parser.ParseWithOptions(new ParserOptions(template));
			SerilalizeAndDeserialize(morestachioDocumentInfo.Document);
			AssertDocumentItemIsSameAsTemplate(template, morestachioDocumentInfo.Document);
		}

		[Test]
		public void TestCanSerializePartial()
		{
			var template = "Partial:" +
						   "{{#DECLARE PartialA}}" +
						   "I am <Text> {{Data.data('test')}}" +
						   "{{/DECLARE}}" +
						   "{{#IMPORT 'PartialA}'}";
			var morestachioDocumentInfo = Parser.ParseWithOptions(new ParserOptions(template));
			SerilalizeAndDeserialize(morestachioDocumentInfo.Document);
			AssertDocumentItemIsSameAsTemplate(template, morestachioDocumentInfo.Document);
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
			var morestachioDocumentInfo = Parser.ParseWithOptions(new ParserOptions(template));
			SerilalizeAndDeserialize(morestachioDocumentInfo.Document);
			AssertDocumentItemIsSameAsTemplate(template, morestachioDocumentInfo.Document);
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

			var morestachioDocumentInfo = Parser.ParseWithOptions(new ParserOptions(template));
			SerilalizeAndDeserialize(morestachioDocumentInfo.Document);
			AssertDocumentItemIsSameAsTemplate(template, morestachioDocumentInfo.Document);
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

			var morestachioDocumentInfo = Parser.ParseWithOptions(new ParserOptions(template));
			SerilalizeAndDeserialize(morestachioDocumentInfo.Document);
			AssertDocumentItemIsSameAsTemplate(template, morestachioDocumentInfo.Document);
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

			var morestachioDocumentInfo = Parser.ParseWithOptions(new ParserOptions(template));
			SerilalizeAndDeserialize(morestachioDocumentInfo.Document);
			AssertDocumentItemIsSameAsTemplate(template, morestachioDocumentInfo.Document);
		}

		[Test]
		public void TestIsIfNotIfSerializable()
		{
			var template = "I am <Text> {{#IF data}}" +
						   "{{#LET test = 'test'}}" +
						   " {{/IF}} {{^IF data}} {{/IF}}";
			var morestachioDocumentInfo = Parser.ParseWithOptions(new ParserOptions(template));
			SerilalizeAndDeserialize(morestachioDocumentInfo.Document);
			AssertDocumentItemIsSameAsTemplate(template, morestachioDocumentInfo.Document);
		}

		[Test]
		public void TestIsIfElseIsSerializable()
		{
			var template = "I am <Text> {{#IF data}}" +
						   "{{#LET test = 'test'}}" +
						   " {{/IF}} {{#ELSE}}" +
						   "{{#LET test = 'test'}}" +
						   " {{/ELSE}}";
			var morestachioDocumentInfo = Parser.ParseWithOptions(new ParserOptions(template));
			SerilalizeAndDeserialize(morestachioDocumentInfo.Document);
			AssertDocumentItemIsSameAsTemplate(template, morestachioDocumentInfo.Document);
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
			var morestachioDocumentInfo = Parser.ParseWithOptions(new ParserOptions(template)
				.RegisterLocalizationService(
				() =>
				{
					return new MorestachioLocalizationService();
				}));
			SerilalizeAndDeserialize(morestachioDocumentInfo.Document);
			AssertDocumentItemIsSameAsTemplate(template, morestachioDocumentInfo.Document);
		}

		[Test]
		public void Alias()
		{
			var alias = new AliasDocumentItem(CharacterLocation.Unknown, "Alias", 101);
			SerilalizeAndDeserialize(alias);
		}

		//[Test]
		//public void CallFormatter()
		//{
		//	var alias = new CallFormatterDocumentItem(new Tuple<Tokenizer.HeaderTokenMatch, IValueDocumentItem>[0], "");
		//	SerilalizeAndDeserialize(alias);
		//	alias = new CallFormatterDocumentItem(new Tuple<Tokenizer.HeaderTokenMatch, IValueDocumentItem>[0], null);
		//	SerilalizeAndDeserialize(alias);
		//	alias = new CallFormatterDocumentItem(new Tuple<Tokenizer.HeaderTokenMatch, IValueDocumentItem>[0], "test");
		//	SerilalizeAndDeserialize(alias);
		//	alias = new CallFormatterDocumentItem(new Tuple<Tokenizer.HeaderTokenMatch, IValueDocumentItem>[]
		//	{
		//		new Tuple<Tokenizer.HeaderTokenMatch, IValueDocumentItem>(new Tokenizer.HeaderTokenMatch()
		//		{
		//			Value = "TEST",
		//			Arguments = new List<Tokenizer.HeaderTokenMatch>()
		//			{
		//				new Tokenizer.HeaderTokenMatch()
		//				{
		//					Value = "TESTINNER"
		//				}
		//			},
		//			TokenType = Tokenizer.HeaderArgumentType.String,
		//			TokenLocation = new CharacterLocation()
		//			{
		//				Character = 123,
		//				Line = 321
		//			},
		//			ArgumentName = "TESTARG"
		//		}, new ContentDocumentItem("CONTENT")),
		//	}, "test");
		//	SerilalizeAndDeserialize(alias);	

		//	alias = new CallFormatterDocumentItem(new Tuple<Tokenizer.HeaderTokenMatch, IValueDocumentItem>[]
		//	{
		//		new Tuple<Tokenizer.HeaderTokenMatch, IValueDocumentItem>(new Tokenizer.HeaderTokenMatch()
		//		{
		//			Value = "",
		//			Arguments = new List<Tokenizer.HeaderTokenMatch>()
		//			{
		//				new Tokenizer.HeaderTokenMatch()
		//				{
		//					Value = ""
		//				}
		//			},
		//			TokenType = Tokenizer.HeaderArgumentType.String,
		//			TokenLocation = new CharacterLocation()
		//			{
		//				Character = 123,
		//				Line = 321
		//			},
		//			ArgumentName = ""
		//		}, new ContentDocumentItem("")),
		//	}, "test");
		//	SerilalizeAndDeserialize(alias);
		//}
	}
}
