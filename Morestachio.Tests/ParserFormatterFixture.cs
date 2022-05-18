using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Morestachio.Formatter.Framework;
using Morestachio.Linq;
using Morestachio.Rendering;
using Morestachio.Helper;
using Morestachio.Util;
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
			var template = "{{#each data.OrderBy('it')}}{{this}},{{/each}}";

			var options = ParserFixture.TestBuilder().WithTemplate(template).Build();
			var collection = new[] { 0, 1, 2, 3, 5, 4, 6, 7 };
			options.Formatters.AddFromType(typeof(DynamicLinq));
			var report = Parser.ParseWithOptions(options).CreateRenderer().Render(new Dictionary<string, object>
			{
				{
					"data", collection
				}
			}).Stream.Stringify(true, ParserFixture.DefaultEncoding);
			Assert.That(report,
				Is.EqualTo(collection.OrderBy(e => e).Select(e => e.ToString()).Aggregate((e, f) => e + "," + f) + ","));
			Console.WriteLine(report);
		}
		
#if NET5_0_OR_GREATER
		public object ToValStr(object source, ReadOnlyMemory<char> var)
		{
			return source.ToString();
		}

		public static ReadOnlyMemory<char> Truncate(ReadOnlyMemory<char> source, int length, string ellipsis = "...")
		{
			if (source.IsEmpty)
			{
				return ReadOnlyMemory<char>.Empty;
			}
			ellipsis = ellipsis ?? "...";
			int lMinusTruncate = length - ellipsis.Length;
			if (source.Length > length)
			{
				var builder = new ValueStringBuilder(length + ellipsis.Length);
				builder.Append(source[..(lMinusTruncate < 0 ? 0 : lMinusTruncate)].Span);
				builder.Append(ellipsis);
				return builder.ToString().AsMemory();
			}
			return source;
		}
#endif
	}
}