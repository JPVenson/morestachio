using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Morestachio.Document;
using Morestachio.Document.Contracts;
using Morestachio.Framework;
using Morestachio.Tests.DocTree;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Morestachio.Tests.SerilalizerTests
{
	[TestFixture (typeof(DocumentSerializerXmlStrategy))]
	[TestFixture (typeof(DocumentSerializerJsonNetStrategy))]
	public class SerializerTest
	{
		public SerializerTest(Type strategy)
		{
			DocumentSerializerStrategy = Activator.CreateInstance(strategy) as IDocumentSerializerStrategy;
		}

		public IDocumentSerializerStrategy DocumentSerializerStrategy { get; private set; }

		private void SerilalizeAndDeserialize(IDocumentItem document)
		{
			var text = DocumentSerializerStrategy.SerializeToText(document);
			var deserialized = DocumentSerializerStrategy.DeSerializeToText(text, document.GetType());
			var deserializedText = DocumentSerializerStrategy.SerializeToText(deserialized);
			Assert.That(document, Is.EqualTo(deserialized), () =>
				{
					return $"Object left is: " +
					       $"\"{text}\" " +
					       $"and right ist " +
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
		}

		[Test]
		public void TestIsVariableSerializable()
		{
			var template = "I am <Text> {{#var f = data.test.Format().As.Test('', exp)}}";
			var morestachioDocumentInfo = Parser.ParseWithOptions(new ParserOptions(template));
			SerilalizeAndDeserialize(morestachioDocumentInfo.Document);
		}

		[Test]
		public void TestIsContentSerializable()
		{
			var template = "I am <Text>";
			var morestachioDocumentInfo = Parser.ParseWithOptions(new ParserOptions(template));
			SerilalizeAndDeserialize(morestachioDocumentInfo.Document);
		}

		[Test]
		public void TestIsContentWithPathAndEachAndFormatterSerializable()
		{
			var template = "I am <Text> {{#each data.('', dd)}} {{Data.data.test()}} {{/each}}";
			var morestachioDocumentInfo = Parser.ParseWithOptions(new ParserOptions(template));
			SerilalizeAndDeserialize(morestachioDocumentInfo.Document);
		}

		[Test]
		public void TestCanSerializePartial()
		{
			var template = "Partial:" +
			               "{{#declare PartialA}}" +
						   "I am <Text> {{Data.data('test')}}" +
			               "{{/declare}}" +
			               "{{#Include PartialA}}";
			var morestachioDocumentInfo = Parser.ParseWithOptions(new ParserOptions(template));
			SerilalizeAndDeserialize(morestachioDocumentInfo.Document);
		}

		[Test]
		public void TestCanSerializeWhile()
		{
			var template = "{{#VAR condition = true}}" +
			               "{{#WHILE condition}}" +
			               "{{$index}}," +
			               "{{#IF condition.Equals(5)}}{{#VAR condition = false}}{{/IF}}" +
			               "{{/WHILE}}";

			var morestachioDocumentInfo = Parser.ParseWithOptions(new ParserOptions(template));
			SerilalizeAndDeserialize(morestachioDocumentInfo.Document);
		}

		[Test]
		public void TestCanSerializeDo()
		{
			var template = "{{#VAR condition = true}}" +
			               "{{#DO condition}}" +
			               "{{$index}}," +
			               "{{#IF condition.Equals(5)}}{{#VAR condition = false}}{{/IF}}" +
			               "{{/DO}}";

			var morestachioDocumentInfo = Parser.ParseWithOptions(new ParserOptions(template));
			SerilalizeAndDeserialize(morestachioDocumentInfo.Document);
		}

		[Test]
		public void TestIsIfNotIfSerializable()
		{
			var template = "I am <Text> {{#IF data}} {{/If}} {{^IF data}} {{/If}}";
			var morestachioDocumentInfo = Parser.ParseWithOptions(new ParserOptions(template));
			SerilalizeAndDeserialize(morestachioDocumentInfo.Document);
		}
		
		[Test]
		public void TestIsIfElseIsSerializable()
		{
			var template = "I am <Text> {{#IF data}} {{/If}} {{#else}} {{/else}}";
			var morestachioDocumentInfo = Parser.ParseWithOptions(new ParserOptions(template));
			SerilalizeAndDeserialize(morestachioDocumentInfo.Document);
		}

		[Test]
		public void Alias()
		{
			var alias = new AliasDocumentItem("Alias");
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
