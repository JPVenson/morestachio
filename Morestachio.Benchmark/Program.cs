using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
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
		private MorestachioDocumentInfo _template;
		private CompilationResult _templateCompiled;
		private object _data;

		[GlobalSetup]
		public void Setup()
		{
			_templateCompiled = Parser.ParseWithOptions(new ParserOptions(GetTemplate(), null, Encoding.UTF8))
				.Compile();
			_template = Parser.ParseWithOptions(new ParserOptions(GetTemplate(), null, Encoding.UTF8));
			_data = GetData();
		}

		[Benchmark]
		public async ValueTask Bench()
		{
			await _templateCompiled(_data, CancellationToken.None);
		}

		public object GetData()
		{
			var item = new DictObject()
			{
				{"strVal", "strVal"},
				{"Number", 500},
			};
			var obj = new DictObject();
			var items = new List<DictObject>();
			for (int i = 0; i < 1000; i++)
			{
				items.Add(item);
			}

			obj["List"] = items;
			return obj;
		}

		public string GetTemplate()
		{
			return @"<p>Static content this is</p>
{{#each List}}
	{{item.strVal}} {{item.Number}}	
{{/each}}";
		}
	}
}
