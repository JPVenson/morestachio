using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Morestachio.Linq;
using NUnit.Framework;

namespace Morestachio.Tests
{
	[TestFixture(ParserOptionTypes.UseOnDemandCompile)]
	[TestFixture(ParserOptionTypes.Precompile)]
	[Parallelizable(ParallelScope.All)]
	public class ParserCollectionFixture
	{
		private readonly ParserOptionTypes _options;

		public ParserCollectionFixture(ParserOptionTypes options)
		{
			_options = options;
		}

		public class EveryObjectTest
		{
			public string TestA { get; set; }
			public string TestB { get; set; }
			public EveryObjectTest ObjectTest { get; set; }
		}

		[Test]
		public async Task TestEveryKeywordOnObject()
		{
			var template = "{{#each this.?}}{{Key}}:\"{{Value}}\"{{^IF $last}},{{/IF}}{{/each}}";

			var data = new EveryObjectTest
			{
				TestA = "Du",
				TestB = "Hast"
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _options);

			Assert.That(result, Is.EqualTo($"{nameof(EveryObjectTest.TestA)}:\"Du\"," +
				$"{nameof(EveryObjectTest.TestB)}:\"Hast\"," +
				$"{nameof(EveryObjectTest.ObjectTest)}:\"\""));
		}

		[Test]
		public async Task TestEveryKeywordOnDictionary()
		{
			var template = "{{#each this.?}}{{Key}}:\"{{Value}}\"{{^IF $last}},{{/IF}}{{/each}}";

			var data = new Dictionary<string, object>
			{
				{ nameof(EveryObjectTest.TestA), "Du" },
				{ nameof(EveryObjectTest.TestB), "Hast" },
				{ nameof(EveryObjectTest.ObjectTest), null }
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _options);

			Assert.That(result, Is.EqualTo($"{nameof(EveryObjectTest.TestA)}:\"Du\"," +
				$"{nameof(EveryObjectTest.TestB)}:\"Hast\"," +
				$"{nameof(EveryObjectTest.ObjectTest)}:\"\""));
		}

		[Test]
		public async Task TestEveryKeywordOnComplexPathDictionary()
		{
			var template = "{{#each Data.?}}{{Key}}:\"{{Value}}\"{{^IF $last}},{{/IF}}{{/each}}";

			var data = new Dictionary<string, object>
			{
				{
					"Data", new Dictionary<string, object>
					{
						{ nameof(EveryObjectTest.TestA), "Du" },
						{ nameof(EveryObjectTest.TestB), "Hast" },
						{ nameof(EveryObjectTest.ObjectTest), null }
					}
				}
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _options);

			Assert.That(result, Is.EqualTo($"{nameof(EveryObjectTest.TestA)}:\"Du\"," +
				$"{nameof(EveryObjectTest.TestB)}:\"Hast\"," +
				$"{nameof(EveryObjectTest.ObjectTest)}:\"\""));
		}

		[Test]
		public async Task TestCollectionSpecialKeyFormatting()
		{
			var template = "{{#each data}}{{$index.PlusOne()}},{{/each}}";
			var collection = new[] { 10, 11, 12, 14 };

			var data = new Dictionary<string, object>
			{
				{
					"data", collection
				}
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _options,
				options => { return options.WithFormatter(new Func<long, long>(value => value + 1), "PlusOne"); });

			Assert.That(result,
				Is.EqualTo(Enumerable.Range(1, collection.Length).Select(e => e.ToString())
					.Aggregate((e, f) => e + "," + f) + ","));
		}

		[Test]
		public async Task TestCollectionFormatting()
		{
			var template = "{{#each data.OrderBy()}}{{this}},{{/each}}";
			var collection = new[] { 0, 1, 2, 3, 5, 4, 6, 7 };

			var data = new Dictionary<string, object>
			{
				{
					"data", collection
				}
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _options,
				options => { return options.WithFormatters(typeof(DynamicLinq)); });

			Assert.That(result,
				Is.EqualTo(collection.OrderBy(e => e).Select(e => e.ToString()).Aggregate((e, f) => e + "," + f) +
					","));
		}

		[Test]
		public async Task TestCollectionFormattingScope()
		{
			var template = "{{#each data.OrderBy()}}{{this}},{{/each}}|{{#each data}}{{this}},{{/each}}";
			var collection = new[] { 0, 1, 2, 3, 5, 4, 6, 7 };

			var data = new Dictionary<string, object>
			{
				{
					"data", collection
				}
			};
			var result = await ParserFixture.CreateAndParseWithOptions(template, data, _options,
				options => { return options.WithFormatters(typeof(DynamicLinq)); });

			var resultLeftExpressionOrdered =
				collection.OrderBy(e => e).Select(e => e.ToString()).Aggregate((e, f) => e + "," + f) + ",";
			var resultRightExpression = collection.Select(e => e.ToString()).Aggregate((e, f) => e + "," + f) + ",";
			Assert.That(result, Is.EqualTo(resultLeftExpressionOrdered + "|" + resultRightExpression));
		}
	}
}