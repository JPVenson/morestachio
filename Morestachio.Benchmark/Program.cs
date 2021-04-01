using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Morestachio.Document;
using Morestachio.Framework.IO;
using Morestachio.Framework.IO.SingleStream;
using Morestachio.Rendering;
using Morestachio.TemplateContainers;
using DictObject = System.Collections.Generic.Dictionary<string, object>;

namespace Morestachio.Benchmark
{
	public class Program
	{
		static void Main(string[] args)
		{
			var switcher = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly);
			switcher.Run(args);
			Console.WriteLine("-----------------");
			Console.WriteLine("Benchmark done. Press any key to close.");
			Console.ReadKey();
		}
	}

	[MemoryDiagnoser]
	[MarkdownExporter]
	[HtmlExporter]
	public class BenchPerfHarness
	{
		private IRenderer _templateCompiled;
		private object _data;

		[GlobalSetup]
		public void Setup()
		{
			var parsingOptions = new ParserOptions(new StringTemplateContainer(GetTemplate()),
				(options) => new ByteCounterStringBuilder(new StringBuilder(), options),
				Encoding.UTF8);
			_templateCompiled = Parser.ParseWithOptions(parsingOptions)
				.CreateCompiledRenderer(new DocumentCompiler());
			_data = GetData();
		}

		[Benchmark]
		public async ValueTask Bench()
		{
			await _templateCompiled.RenderAsync(_data, CancellationToken.None);
		}
		private const string Lorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum";


		public object GetData()
		{
			var productsDict = new DictObject();
			const int ProductCount = 500;

			List<DictObject> prodList = new List<DictObject>();
			productsDict.Add("Products", prodList);

			for (int i = 0; i < ProductCount; i++)
			{
				prodList.Add(new Dictionary<string, object>()
				{
					{ "Name", "Name" + i},
					{ "Price", i},
					{ "Description", Lorem},
				});
			}

			return productsDict;
		}

		public string GetTemplate()
		{
			return @"<ul id='products'>
  {{#each Products}}
	<li>
	  <h2>{{Name}}</h2>
		   Only {{Price}}
		   {{Description.Truncate(15)}}
	</li>
  {{/each}}
</ul>";
		}
	}
}
