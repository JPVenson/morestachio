using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Morestachio.Formatter.Linq;
using NUnit.Framework;

namespace Morestachio.Formatter.Framework.Tests
{
	[TestFixture]
	public class FormatterTests
	{
		public static Encoding DefaultEncoding { get; set; } = new UnicodeEncoding(true, false, false);

		[Test]
		public void FormatterCanFormatObjectTwice()
		{
			var formatterService = new MorestachioFormatterService();
			formatterService.AddFromType(typeof(StringFormatter));

			var options = new ParserOptions("{{.('Plus', NumberB, NumberB)}}", null, DefaultEncoding);
			formatterService.AddFormatterToMorestachio(options);
			var template = Parser.ParseWithOptions(options);

			var andStringify = template.CreateAndStringify(new Dictionary<string, object>()
			{
				{ "NumberA", 5 },
				{ "NumberB", 6 }
			});
			Assert.That(andStringify, Is.EqualTo("12"));
		}


		[Test]
		public void TestSingleNamed()
		{
			var formatterService = new MorestachioFormatterService();
			formatterService.AddFromType(typeof(StringFormatter));

			var options = new ParserOptions("{{data([Name]'reverse')}}", null, DefaultEncoding);
			formatterService.AddFormatterToMorestachio(options);
			var template = Parser.ParseWithOptions(options);

			var andStringify = template.CreateAndStringify(new Dictionary<string, object>() { { "data", "Test" } });
			Assert.That(andStringify, Is.EqualTo("tseT"));
		}

		[Test]
		public void TestAsync()
		{
			var formatterService = new MorestachioFormatterService();
			formatterService.AddFromType(typeof(StringFormatter));

			var options = new ParserOptions("{{data([Name]'reverseAsync')}}", null, DefaultEncoding);
			formatterService.AddFormatterToMorestachio(options);
			var template = Parser.ParseWithOptions(options);

			var andStringify = template.CreateAndStringify(new Dictionary<string, object>() { { "data", "Test" } });
			Assert.That(andStringify, Is.EqualTo("tseT"));
		}

		[Test]
		public void TestOptionalArgument()
		{
			var formatterService = new MorestachioFormatterService();
			formatterService.AddFromType(typeof(StringFormatter));

			var options = new ParserOptions("{{data([Name]'optional')}}", null, DefaultEncoding);
			formatterService.AddFormatterToMorestachio(options);
			var template = Parser.ParseWithOptions(options);

			var andStringify = template.CreateAndStringify(new Dictionary<string, object>() { { "data", "Test" } });
			Assert.That(andStringify, Is.EqualTo("OPTIONAL Test"));
		}

		[Test]
		public void TestDefaultArgument()
		{
			var formatterService = new MorestachioFormatterService();
			formatterService.AddFromType(typeof(StringFormatter));

			var options = new ParserOptions("{{data([Name]'defaultValue')}}", null, DefaultEncoding);
			formatterService.AddFormatterToMorestachio(options);
			var template = Parser.ParseWithOptions(options);

			var andStringify = template.CreateAndStringify(new Dictionary<string, object>() { { "data", "Test" } });
			Assert.That(andStringify, Is.EqualTo("DEFAULT Test"));
		}

		[Test]
		public void TestNamed()
		{
			var formatterService = new MorestachioFormatterService();
			formatterService.AddFromType(typeof(StringFormatter));

			var options = new ParserOptions("{{data([Name]'reverse-arg', 'TEST')}}", null, DefaultEncoding);
			formatterService.AddFormatterToMorestachio(options);
			var template = Parser.ParseWithOptions(options);

			var andStringify = template.CreateAndStringify(new Dictionary<string, object>() { { "data", "Test" } });
			Assert.That(andStringify, Is.EqualTo("TEST"));
		}

		[Test]
		public void GenericsTest()
		{
			var formatterService = new MorestachioFormatterService();
			formatterService.AddFromType(typeof(StringFormatter));
			formatterService.AddFromType(typeof(ListFormatter));

			var options = new ParserOptions("{{data([Name]'fod')}}", null, DefaultEncoding);
			formatterService.AddFormatterToMorestachio(options);
			var template = Parser.ParseWithOptions(options);

			var andStringify = template.CreateAndStringify(new Dictionary<string, object>() { { "data", new[] { "TEST", "test" } } });
			Assert.That(andStringify, Is.EqualTo("TEST"));
		}
	}
}
