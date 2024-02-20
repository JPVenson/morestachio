using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Morestachio.Configuration.Transform.Tests
{
	[TestFixture(true)]
	[TestFixture(false)]
	public class TestConfigTransformation
	{
		private readonly bool _useBuildTime;

		public TestConfigTransformation(bool useBuildTime)
		{
			_useBuildTime = useBuildTime;
		}

		public IMorestachioConfigurationBuilder CreateConfig(
			Func<IConfigurationBuilder, IConfigurationBuilder> builderConfig = null)
		{
			IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
			configurationBuilder = builderConfig != null ? builderConfig(configurationBuilder) : configurationBuilder;

			if (_useBuildTime)
			{
				return configurationBuilder.UseBuildtimeMorestachio();
			}
			else
			{
				return configurationBuilder.UseRuntimeMorestachio();
			}
		}

		[Test]
		public void TestCanReplaceBuildTime()
		{
			var builder = CreateConfig(c => c
				.AddInMemoryCollection(new[]
				{
					new KeyValuePair<string, string>("Str", "test"),
					new KeyValuePair<string, string>("Number", "mex{{2 + 2}}")
				}));

			var config = builder.Build();
			Assert.That(config["Str"], Is.EqualTo("test"));
			Assert.That(config["Number"], Is.EqualTo("4"));
		}

		[Test]
		public void TestCanReplaceBuildTimeWithDeepPaths()
		{
			var builder = CreateConfig(c => c.AddJsonStream(JsonfyText(
				@"{
	""strValue"": ""test"",
	""objValue"": {
		""constValue"": 123,
		""valueA"": ""mex{{1 + 2 + 3}}""
	},
	""valueB"": ""mex{{5 * 3}}""
}")));

			var config = builder.Build();
			Assert.That(config["strValue"], Is.EqualTo("test"));
			Assert.That(config["objValue:constValue"], Is.EqualTo("123"));
			Assert.That(config["objValue:valueA"], Is.EqualTo("6"));
			Assert.That(config["valueB"], Is.EqualTo("15"));
		}

		[Test]
		public void TestCanReplaceBuildTimeWithArguments()
		{
			var builder = CreateConfig(c => c.AddJsonStream(JsonfyText(
					@"{
	""strValue"": ""test"",
	""objValue"": {
		""constValue"": 123,
		""exp_mEx"": ""mex{{1 + 2 + 3 - Cores}}"",
		""expRoot_mEx"": ""mex{{1 + 2 + 3 - Cores}}"",
	},
	""expA_mEx"": ""mex{{Cores * 3}}""
}")))
				.UseValues(null, new Dictionary<string, object>()
				{
					{ "Cores", 5 }
				})
				.UseValues("objValue:exp_mEx", new Dictionary<string, object>()
				{
					{ "Cores", 1 }
				})
				.UseValues("objValue", new Dictionary<string, object>()
				{
					{ "Cores", 3 }
				});

			var config = builder.Build();
			Assert.That(config["strValue"], Is.EqualTo("test"));
			Assert.That(config["objValue:constValue"], Is.EqualTo("123"));
			Assert.That(config["objValue:exp_mEx"], Is.EqualTo("5"));
			Assert.That(config["objValue:expRoot_mEx"], Is.EqualTo("3"));
			Assert.That(config["expA_mEx"], Is.EqualTo("15"));
		}

		private Stream JsonfyText(string text)
		{
			return new MemoryStream(Encoding.UTF8.GetBytes(text));
		}
	}
}