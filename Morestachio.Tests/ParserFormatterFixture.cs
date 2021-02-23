using System;
using System.Collections.Generic;
using System.Linq;
using Morestachio.Formatter.Framework;
using Morestachio.Linq;
using NUnit.Framework;

namespace Morestachio.Tests
{
	[TestFixture]
	[Parallelizable(ParallelScope.All)]
	public class ParserFormatterFixture
	{
		[Test]
		public void TestCanExecuteAsyncFormatter()
		{
			var options = new ParserOptions("{{#each data.OrderBy('it')}}{{this}},{{/each}}", null,
				ParserFixture.DefaultEncoding);
			var collection = new[] { 0, 1, 2, 3, 5, 4, 6, 7 };
			options.Formatters.AddFromType(typeof(DynamicLinq));
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
	}
}