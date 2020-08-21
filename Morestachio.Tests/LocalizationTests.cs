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
			var translationResult = "TestFixture";
			var template = "{{#loc 'test'}}";
			var options = new ParserOptions(template, null, ParserFixture.DefaultEncoding).RegisterLocalizationService(() =>
			{
				var morestachioLocalizationService = new MorestachioLocalizationService();
				morestachioLocalizationService.Load(new[]
				{
					new MemoryTranslationResource()
						.Add("test", CultureInfo.GetCultureInfo("EN-US"), translationResult + " en-US")
						.Add("test", CultureInfo.GetCultureInfo("DE-DE"), translationResult + " de-DE"),
				}, new[]
				{
					CultureInfo.GetCultureInfo("EN-US"),
					CultureInfo.GetCultureInfo("DE-DE")
				});
				return morestachioLocalizationService;
			});

			options.CultureInfo = CultureInfo.GetCultureInfo("EN-US");
			var result = Parser.ParseWithOptions(options)
				.CreateAndStringify(new object());
			Assert.That(result, Is.EqualTo(translationResult + " " + options.CultureInfo.Name));

			options.CultureInfo = CultureInfo.GetCultureInfo("DE-DE");
			result = Parser.ParseWithOptions(options)
				.CreateAndStringify(new object());
			Assert.That(result, Is.EqualTo(translationResult + " " + options.CultureInfo.Name));
		}
		[Test]
		public void TestCanLocalizeData()
		{
			var translationResult = "TestFixture";
			var template = "{{#loc testKey}}";
			var data = new Dictionary<string, object>()
			{
				{"testKey", "test"}
			};

			var options = new ParserOptions(template, null, ParserFixture.DefaultEncoding).RegisterLocalizationService(() =>
			{
				var morestachioLocalizationService = new MorestachioLocalizationService();
				morestachioLocalizationService.Load(new[]
				{
					new MemoryTranslationResource()
						.Add("test", CultureInfo.GetCultureInfo("EN-US"), translationResult + " en-US")
						.Add("test", CultureInfo.GetCultureInfo("DE-DE"), translationResult + " de-DE"),
				}, new[]
				{
					CultureInfo.GetCultureInfo("EN-US"),
					CultureInfo.GetCultureInfo("DE-DE")
				});
				return morestachioLocalizationService;
			});

			options.CultureInfo = CultureInfo.GetCultureInfo("EN-US");
			var result = Parser.ParseWithOptions(options)
				.CreateAndStringify(data);
			Assert.That(result, Is.EqualTo(translationResult + " " + options.CultureInfo.Name));

			options.CultureInfo = CultureInfo.GetCultureInfo("DE-DE");
			result = Parser.ParseWithOptions(options)
				.CreateAndStringify(data);
			Assert.That(result, Is.EqualTo(translationResult + " " + options.CultureInfo.Name));
		}
		[Test]
		public void TestCanChangeLocale()
		{
			var translationResult = "TestFixture";
			var template = "{{#loc 'Texts.Welcome'}} " +
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
			               "{{/LocCulture}}";
			var data = new Dictionary<string, object>();

			var options = new ParserOptions(template, null, ParserFixture.DefaultEncoding).RegisterLocalizationService(() =>
			{
				var morestachioLocalizationService = new MorestachioLocalizationService();
				morestachioLocalizationService.Load(new[]
				{
					new MemoryTranslationResource()
						.Add("Texts.Welcome", CultureInfo.GetCultureInfo("EN-US"), "Welcome")
						.Add("Texts.Welcome", CultureInfo.GetCultureInfo("DE-DE"), "Moin")
						.Add("Texts.Welcome", CultureInfo.GetCultureInfo("DE-AT"), "Grützli")
					,
				}, new[]
				{
					CultureInfo.GetCultureInfo("EN-US"),
					CultureInfo.GetCultureInfo("DE-DE"),
					CultureInfo.GetCultureInfo("DE-AT")
				});
				return morestachioLocalizationService;
			});

			options.CultureInfo = CultureInfo.GetCultureInfo("EN-US");
			var result = Parser.ParseWithOptions(options)
				.CreateAndStringify(data);
			Assert.That(result, Is.EqualTo("Welcome Grützli Welcome Moin Grützli Moin"));
		}
	}
}
