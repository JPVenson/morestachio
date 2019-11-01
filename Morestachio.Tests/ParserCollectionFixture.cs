using System;
using System.Collections.Generic;
using System.Linq;
using Morestachio.Helper;
using Morestachio.Formatter;
using Morestachio.Formatter.Framework.Tests;
using Morestachio.Formatter.Linq;
using NUnit.Framework;

namespace Morestachio.Tests
{
	[TestFixture]
	public class ParserCollectionFixture
	{
		public class EveryObjectTest
		{
			public string TestA { get; set; }
			public string TestB { get; set; }
			public EveryObjectTest ObjectTest { get; set; }
		}

		[Test]
		public void TestEveryKeywordOnObject()
		{
			var options = new ParserOptions("{{#each ?}}{{Key}}:\"{{Value}}\"{{^$last}},{{/$last}}{{/each}}", null,
				ParserFixture.DefaultEncoding);
			var andStringify = Parser.ParseWithOptions(options).CreateAndStringify(new EveryObjectTest()
			{
				TestA = "Du",
				TestB = "Hast"
			});
			Assert.That(andStringify, Is.EqualTo($"{nameof(EveryObjectTest.TestA)}:\"Du\"," +
												 $"{nameof(EveryObjectTest.TestB)}:\"Hast\"," +
												 $"{nameof(EveryObjectTest.ObjectTest)}:\"\""));
		}

		[Test]
		public void TestEveryKeywordOnDictionary()
		{
			var options = new ParserOptions("{{#each ?}}{{Key}}:\"{{Value}}\"{{^$last}},{{/$last}}{{/each}}", null,
				ParserFixture.DefaultEncoding);
			var andStringify = Parser.ParseWithOptions(options).CreateAndStringify(new Dictionary<string, object>()
			{
				{nameof(EveryObjectTest.TestA), "Du"},
				{nameof(EveryObjectTest.TestB), "Hast"},
				{nameof(EveryObjectTest.ObjectTest), null},
			});
			Assert.That(andStringify, Is.EqualTo($"{nameof(EveryObjectTest.TestA)}:\"Du\"," +
												 $"{nameof(EveryObjectTest.TestB)}:\"Hast\"," +
												 $"{nameof(EveryObjectTest.ObjectTest)}:\"\""));
		}

		[Test]
		public void TestEveryKeywordOnComplexPathDictionary()
		{
			var options = new ParserOptions("{{#each Data.?}}{{Key}}:\"{{Value}}\"{{^$last}},{{/$last}}{{/each}}", null,
				ParserFixture.DefaultEncoding);
			var andStringify = Parser.ParseWithOptions(options).CreateAndStringify(new Dictionary<string, object>()
			{
				{
					"Data", new Dictionary<string, object>()
					{
						{nameof(EveryObjectTest.TestA), "Du"},
						{nameof(EveryObjectTest.TestB), "Hast"},
						{nameof(EveryObjectTest.ObjectTest), null},
					}
				}
			});
			Assert.That(andStringify, Is.EqualTo($"{nameof(EveryObjectTest.TestA)}:\"Du\"," +
												 $"{nameof(EveryObjectTest.TestB)}:\"Hast\"," +
												 $"{nameof(EveryObjectTest.ObjectTest)}:\"\""));
		}

		[Test]
		public void TestCollectionSpecialKeyFormatting()
		{
			var options = new ParserOptions("{{#each data}}{{$index([Name]'plus one')}},{{/each}}", null,
				ParserFixture.DefaultEncoding);
			var collection = new[] { 10, 11, 12, 14 };
			options.Formatters.AddSingle(new Func<long, long>((value) => value + 1), "plus one");

			var report = Parser.ParseWithOptions(options).CreateAndStringify(new Dictionary<string, object>
			{
				{
					"data", collection
				}
			});
			Assert.That(report,
				Is.EqualTo(Enumerable.Range(1, collection.Length).Select(e => e.ToString()).Aggregate((e, f) => e + "," + f) + ","));
			Console.WriteLine(report);
		}

		[Test]
		public void TestCollectionFormatting()
		{
			var options = new ParserOptions("{{#each data('order asc')}}{{.}},{{/each}}", null,
				ParserFixture.DefaultEncoding);
			var collection = new[] { 0, 1, 2, 3, 5, 4, 6, 7 };
			options.Formatters.AddFromType(typeof(ListFormatter));
			var report = Parser.ParseWithOptions(options).CreateAndStringify(new Dictionary<string, object>
			{
				{
					"data", collection
				}
			});
			Assert.That(report,
				Is.EqualTo(collection.OrderBy(e => e).Select(e => e.ToString()).Aggregate((e, f) => e + "," + f) + ","));
			Console.WriteLine(report);
		}

		[Test]
		public void TestCollectionFormattingScope()
		{
			var options = new ParserOptions("{{#each data('order asc')}}{{.}},{{/each}}|{{#each data}}{{.}},{{/each}}", null,
				ParserFixture.DefaultEncoding);
			var collection = new[] { 0, 1, 2, 3, 5, 4, 6, 7 };
			options.Formatters.AddFromType(typeof(ListFormatter));
			var report = Parser.ParseWithOptions(options).CreateAndStringify(new Dictionary<string, object>
			{
				{
					"data", collection
				}
			});

			var resultLeftExpressionOrdered =
				collection.OrderBy(e => e).Select(e => e.ToString()).Aggregate((e, f) => e + "," + f) + ",";
			var resultRightExpression = collection.Select(e => e.ToString()).Aggregate((e, f) => e + "," + f) + ",";

			Assert.That(report, Is.EqualTo(resultLeftExpressionOrdered + "|" + resultRightExpression));
			Console.WriteLine(report);
		}
	}
}