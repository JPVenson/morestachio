using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Morestachio.Tests
{
	[TestFixture]
	public class ParserFormatterFixture
	{
		private void AddAsyncCollectionTypeFormatter(ParserOptions options)
		{
			options.Formatters.AddSingle(new Func<IEnumerable, string, Task<IEnumerable>>(async (value, arg) =>
			{
				await Task.Delay(2500);
				return arg.Split('|').Aggregate(value,
					(current, format) =>
						(IEnumerable)new EnumerableFormatter().FormatArgument(current, format.Trim()));
			}));
		}

		[Test]
		public void TestCanExecuteAsyncFormatter()
		{
			var options = new ParserOptions("{{#each data('order')}}{{.}},{{/each}}", null,
				ParserFixture.DefaultEncoding);
			var collection = new[] { 0, 1, 2, 3, 5, 4, 6, 7 };
			AddAsyncCollectionTypeFormatter(options);
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