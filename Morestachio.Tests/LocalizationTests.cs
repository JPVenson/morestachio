using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Morestachio.Helper.Localization;
using NUnit.Framework;

namespace Morestachio.Tests
{
	[TestFixture]
	public class LocalizationTests
	{
		[Test]
		public void TestCanLocalizeString()
		{
			ParserOptions CreateParserOptions(string s, string translationResult1)
			{
				return new ParserOptions(s, null, ParserFixture.DefaultEncoding).RegisterLocalizationService(() =>
				{
					return new MorestachioLocalizationService()
						.AddResource(new MemoryTranslationResource()
							.Add("test", CultureInfo.GetCultureInfo("EN-US"), translationResult1 + " en-US")
							.Add("test", CultureInfo.GetCultureInfo("DE-DE"), translationResult1 + " de-DE"))
						.Load(new[]
						{
							CultureInfo.GetCultureInfo("EN-US"),
							CultureInfo.GetCultureInfo("DE-DE")
						});
				});
			}

			var translationResult = "TestFixture";
			var template = "{{#LOC 'test'}}";
			var options = CreateParserOptions(template, translationResult);

			options.CultureInfo = CultureInfo.GetCultureInfo("EN-US");
			var result = Parser.ParseWithOptions(options)
				.CreateAndStringify(new object());
			Assert.That(result, Is.EqualTo(translationResult + " " + options.CultureInfo.Name));

			options = CreateParserOptions(template, translationResult);
			options.CultureInfo = CultureInfo.GetCultureInfo("DE-DE");
			result = Parser.ParseWithOptions(options)
				.CreateAndStringify(new object());
			Assert.That(result, Is.EqualTo(translationResult + " " + options.CultureInfo.Name));
		}


		[Test]
		public void TestCanLocalizeData()
		{
			ParserOptions CreateParserOptions(string s, string translationResult1)
			{
				return new ParserOptions(s, null, ParserFixture.DefaultEncoding).RegisterLocalizationService(() =>
				{
					return new MorestachioLocalizationService()
						.AddResource(new MemoryTranslationResource()
							.Add("test", CultureInfo.GetCultureInfo("EN-US"), translationResult1 + " en-US")
							.Add("test", CultureInfo.GetCultureInfo("DE-DE"), translationResult1 + " de-DE"))
						.Load(new[]
					{
						CultureInfo.GetCultureInfo("EN-US"),
						CultureInfo.GetCultureInfo("DE-DE")
					});
				});
			}

			var translationResult = "TestFixture";
			var template = "{{#LOC testKey}}";
			var data = new Dictionary<string, object>()
			{
				{"testKey", "test"}
			};

			var options = CreateParserOptions(template, translationResult);
			options.CultureInfo = CultureInfo.GetCultureInfo("EN-US");
			var result = Parser.ParseWithOptions(options)
				.CreateAndStringify(data);
			Assert.That(result, Is.EqualTo(translationResult + " " + options.CultureInfo.Name));

			options = CreateParserOptions(template, translationResult);
			options.CultureInfo = CultureInfo.GetCultureInfo("DE-DE");
			result = Parser.ParseWithOptions(options)
				.CreateAndStringify(data);
			Assert.That(result, Is.EqualTo(translationResult + " " + options.CultureInfo.Name));
		}
		
		[Test]
		public void TestCanLocalizeDataWithArguments()
		{
			ParserOptions CreateParserOptions(string s)
			{
				return new ParserOptions(s, null, ParserFixture.DefaultEncoding).RegisterLocalizationService(() =>
				{
					return new MorestachioLocalizationService()
						.AddResource(new MemoryTranslationResource()
							.Add("WelcomeText", CultureInfo.GetCultureInfo("EN-US"), "Hello{0} {1}")
							.Add("WelcomeText", CultureInfo.GetCultureInfo("DE-DE"), "Hallo{0} {1}")
							.Add("WelcomeDefine", CultureInfo.GetCultureInfo("EN-US"), "World")
							.Add("WelcomeDefine", CultureInfo.GetCultureInfo("DE-DE"), "Welt")
						)
						.Load(new[]
					{
						CultureInfo.GetCultureInfo("EN-US"),
						CultureInfo.GetCultureInfo("DE-DE")
					});
				});
			}

			var template = "{{#LOCP 'WelcomeText'}}" +
			               "{{#LOCPARAM ','}}" +
			               "{{#LOC 'WelcomeDefine'}}" +
			               "{{/LOCP}}";
			var data = new Dictionary<string, object>()
			{
				
			};

			var options = CreateParserOptions(template);
			options.CultureInfo = CultureInfo.GetCultureInfo("EN-US");
			var result = Parser.ParseWithOptions(options)
				.CreateAndStringify(data);
			Assert.That(result, Is.EqualTo("Hello, World"));

			options = CreateParserOptions(template);
			options.CultureInfo = CultureInfo.GetCultureInfo("DE-DE");
			result = Parser.ParseWithOptions(options)
				.CreateAndStringify(data);
			Assert.That(result, Is.EqualTo("Hallo, Welt"));
		}


		[Test]
		public void TestCanChangeLocale()
		{
			ParserOptions CreateParserOptions(string s)
			{
				return new ParserOptions(s, null, ParserFixture.DefaultEncoding).RegisterLocalizationService(() =>
				{
					return new MorestachioLocalizationService()
						.AddResource(new MemoryTranslationResource()
							.Add("Texts.Welcome", CultureInfo.GetCultureInfo("EN-US"), "Welcome")
							.Add("Texts.Welcome", CultureInfo.GetCultureInfo("DE-DE"), "Moin")
							.Add("Texts.Welcome", CultureInfo.GetCultureInfo("DE-AT"), "Grützli"))
						.Load(new[]
						{
							CultureInfo.GetCultureInfo("EN-US"),
							CultureInfo.GetCultureInfo("DE-DE"),
							CultureInfo.GetCultureInfo("DE-AT")
						});
				});
			}

			var template = "{{#LOC 'Texts.Welcome'}} " +
			               "{{#LOCCULTURE 'de-AT'}}" +
								"{{#LOC 'Texts.Welcome'}} " +
			               "{{/LOCCULTURE}}" +

			               "{{#LOC 'Texts.Welcome'}} " +

			               "{{#LOCCULTURE 'de-DE'}}" +
								"{{#LOC 'Texts.Welcome'}} " +

								"{{#LOCCULTURE 'de-AT'}}" +
									"{{#LOC 'Texts.Welcome'}} " +
								"{{/LOCCULTURE}}" +

								"{{#LOC 'Texts.Welcome'}}" +
			               "{{/LOCCULTURE}}";
			var data = new Dictionary<string, object>();

			var options = CreateParserOptions(template);

			options.CultureInfo = CultureInfo.GetCultureInfo("EN-US");
			var result = Parser.ParseWithOptions(options)
				.CreateAndStringify(data);
			Assert.That(result, Is.EqualTo("Welcome Grützli Welcome Moin Grützli Moin"));
		}


		[Test]
		public void TestCanChangeLocaleExplicit()
		{
			ParserOptions CreateParserOptions(string s)
			{
				return new ParserOptions(s, null, ParserFixture.DefaultEncoding).RegisterLocalizationService(() =>
				{
					return new MorestachioLocalizationService()
						.AddResource(new MemoryTranslationResource()
							.Add("Texts.Welcome", CultureInfo.GetCultureInfo("EN-US"), "Welcome")
							.Add("Texts.Welcome", CultureInfo.GetCultureInfo("DE-DE"), "Moin")
							.Add("Texts.Welcome", CultureInfo.GetCultureInfo("DE-AT"), "Grützli"))
						.Load(new[]
						{
							CultureInfo.GetCultureInfo("EN-US"),
							CultureInfo.GetCultureInfo("DE-DE"),
							CultureInfo.GetCultureInfo("DE-AT")
						});
				});
			}

			var template = "{{#LOC 'Texts.Welcome'}} " +
			               "{{#LOC 'Texts.Welcome' #CULTURE 'de-AT'}} " +
			               "{{#LOC 'Texts.Welcome'}} " +

			               "{{#LOCCULTURE 'de-DE'}}" +
								"{{#LOC 'Texts.Welcome'}} " +
								"{{#LOC 'Texts.Welcome' #CULTURE 'DE-AT'}} " +
								"{{#LOC 'Texts.Welcome' #CULTURE CulValue}}" +
			               "{{/LOCCULTURE}}";
			var data = new Dictionary<string, object>()
			{
				{ "CulValue", CultureInfo.GetCultureInfo("EN-US")}
			};

			var options = CreateParserOptions(template);

			options.CultureInfo = CultureInfo.GetCultureInfo("EN-US");
			var result = Parser.ParseWithOptions(options)
				.CreateAndStringify(data);
			Assert.That(result, Is.EqualTo("Welcome Grützli Welcome Moin Grützli Welcome"));
		}
	}
}
