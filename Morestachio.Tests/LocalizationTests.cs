using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Morestachio.Helper.Localization;
using NUnit.Framework;

namespace Morestachio.Tests
{
	[TestFixture(ParserOptionTypes.UseOnDemandCompile)]
	[TestFixture(ParserOptionTypes.Precompile)]
	//[Parallelizable(ParallelScope.All)]
	public class LocalizationTests
	{
		private readonly ParserOptionTypes _options;

		public LocalizationTests(ParserOptionTypes options)
		{
			_options = options;
		}

		[Test]
		public async Task TestCanLocalizeString()
		{
			var translationResult = "TestFixture";
			var template = "{{#LOC 'test'}}";
			var result = await ParserFixture.CreateAndParseWithOptions(template, new object(), _options, parserOptions =>
			{
				parserOptions.RegisterLocalizationService(() =>
				{
					return new MorestachioLocalizationService()
						.AddResource(new MemoryTranslationResource()
							.Add("test", CultureInfo.GetCultureInfo("EN-US"), translationResult + " en-US")
							.Add("test", CultureInfo.GetCultureInfo("DE-DE"), translationResult + " de-DE"))
						.Load(new[]
						{
							CultureInfo.GetCultureInfo("EN-US"),
							CultureInfo.GetCultureInfo("DE-DE")
						});
				});
				parserOptions.CultureInfo = CultureInfo.GetCultureInfo("EN-US");
			});
			Assert.That(result, Is.EqualTo(translationResult + " " + "en-US"));

			result = await ParserFixture.CreateAndParseWithOptions(template, new object(), _options, parserOptions =>
			{
				parserOptions.RegisterLocalizationService(() =>
				{
					return new MorestachioLocalizationService()
						.AddResource(new MemoryTranslationResource()
							.Add("test", CultureInfo.GetCultureInfo("EN-US"), translationResult + " en-US")
							.Add("test", CultureInfo.GetCultureInfo("DE-DE"), translationResult + " de-DE"))
						.Load(new[]
						{
							CultureInfo.GetCultureInfo("EN-US"),
							CultureInfo.GetCultureInfo("DE-DE")
						});
				});
				parserOptions.CultureInfo = CultureInfo.GetCultureInfo("DE-DE");
			});
			Assert.That(result, Is.EqualTo(translationResult + " " + "de-DE"));
		}


		[Test]
		public async Task TestCanLocalizeData()
		{
			var data = new Dictionary<string, object>()
			{
				{"testKey", "test"}
			};
			var translationResult = "TestFixture";
			var template = "{{#LOC testKey}}";
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _options, parserOptions =>
			{
				parserOptions.RegisterLocalizationService(() =>
				{
					return new MorestachioLocalizationService()
						.AddResource(new MemoryTranslationResource()
							.Add("test", CultureInfo.GetCultureInfo("EN-US"), translationResult + " en-US")
							.Add("test", CultureInfo.GetCultureInfo("DE-DE"), translationResult + " de-DE"))
						.Load(new[]
						{
							CultureInfo.GetCultureInfo("EN-US"),
							CultureInfo.GetCultureInfo("DE-DE")
						});
				});
				parserOptions.CultureInfo = CultureInfo.GetCultureInfo("EN-US");
			});
			Assert.That(result, Is.EqualTo(translationResult + " en-US"));
			result = await ParserFixture.CreateAndParseWithOptions(template, data, _options, parserOptions =>
			{
				parserOptions.RegisterLocalizationService(() =>
				{
					return new MorestachioLocalizationService()
						.AddResource(new MemoryTranslationResource()
							.Add("test", CultureInfo.GetCultureInfo("EN-US"), translationResult + " en-US")
							.Add("test", CultureInfo.GetCultureInfo("DE-DE"), translationResult + " de-DE"))
						.Load(new[]
						{
							CultureInfo.GetCultureInfo("EN-US"),
							CultureInfo.GetCultureInfo("DE-DE")
						});
				});
				parserOptions.CultureInfo = CultureInfo.GetCultureInfo("DE-DE");
			});
			Assert.That(result, Is.EqualTo(translationResult + " de-DE"));
		}

		[Test]
		public async Task TestCanLocalizeDataWithArguments()
		{
			var template = "{{#LOCP 'WelcomeText'}}" +
						   "{{#LOCPARAM ','}}" +
						   "{{#LOC 'WelcomeDefine'}}" +
						   "{{/LOCP}}";
			var data = new Dictionary<string, object>()
			{

			};

			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _options, parserOptions =>
			{
				parserOptions.RegisterLocalizationService(() =>
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
				parserOptions.CultureInfo = CultureInfo.GetCultureInfo("EN-US");
			});
			Assert.That(result, Is.EqualTo("Hello, World"));


			result = await ParserFixture.CreateAndParseWithOptions(template, data, _options, parserOptions =>
			{
				parserOptions.RegisterLocalizationService(() =>
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
				parserOptions.CultureInfo = CultureInfo.GetCultureInfo("DE-DE");
			});
			Assert.That(result, Is.EqualTo("Hallo, Welt"));
		}


		[Test]
		public async Task TestCanChangeLocale()
		{
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

			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _options, parserOptions =>
			{
				parserOptions.RegisterLocalizationService(() =>
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
				parserOptions.CultureInfo = CultureInfo.GetCultureInfo("EN-US");
			});

			Assert.That(result, Is.EqualTo("Welcome Grützli Welcome Moin Grützli Moin"));
		}


		[Test]
		public async Task TestCanChangeLocaleExplicit()
		{
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

			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _options, parserOptions =>
			{
				parserOptions.RegisterLocalizationService(() =>
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
				parserOptions.CultureInfo = CultureInfo.GetCultureInfo("EN-US");
			});

			Assert.That(result, Is.EqualTo("Welcome Grützli Welcome Moin Grützli Welcome"));
		}
	}
}
