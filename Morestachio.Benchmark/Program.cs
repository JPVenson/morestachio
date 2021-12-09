using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
	public class BenchRenderPerformance
	{
		private BenchPerfHarness _dictionaryCall;
		private BenchPerfHarness _objectCall;
		[GlobalSetup]
		public async Task Setup()
		{
			_dictionaryCall = new BenchPerfHarnessDictionary();
			await _dictionaryCall.Setup();
			_objectCall = new BenchPerfHarnessObject();
			await _objectCall.Setup();
		}
		
		[Benchmark(Baseline = false, Description = "Call Dictionary")]
		public ValueTask<string> BenchDictionary()
		{
			return _dictionaryCall.Bench();
		}

		[Benchmark(Baseline = true, Description = "Call Object")]
		public ValueTask<string> BenchObject()
		{
			return _objectCall.Bench();
		}
	}
	
	public abstract class BenchPerfHarness
	{
		private IRenderer _templateCompiled;
		private object _data;
		
		public virtual async Task Setup()
		{
			var parsingOptions = new ParserOptions(GetTemplate());
			parsingOptions.DisableContentEscaping = true;
			_templateCompiled = (await Parser.ParseWithOptionsAsync(parsingOptions))
				.CreateCompiledRenderer(new DocumentCompiler());
			_data = GetData();

			var bench = await Bench();
			//Console.WriteLine(bench);
			if (string.IsNullOrWhiteSpace(bench))
			{
				throw new InvalidOperationException("Result does not equal expected value");
			}
		}
		
		public virtual async ValueTask<string> Bench()
		{
			var output = new ByteCounterStringBuilder(new StringBuilder(), _templateCompiled.ParserOptions);
			//var output = new ByteCounterStringBuilderV2(_templateCompiled.ParserOptions);
			await _templateCompiled.RenderAsync(_data, CancellationToken.None, output);
			return output.ToString();
		}

		public const string Lorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum";

		public abstract object GetData();

		public string GetTemplate()
		{
			return @"<ul id='products'>
  {{#EACH Products}}
	<li>
	  <h2>{{Name}}</h2>
		   Only {{Price}}
		   {{Description.Truncate(15)}}
	</li>
  {{/EACH}}
</ul>";
		}
	}
	
	public class BenchPerfHarnessDictionary : BenchPerfHarness
	{

		public override object GetData()
		{
			var productsDict = new DictObject();
			const int ProductCount = 500;

			List<DictObject> prodList = new List<DictObject>();
			productsDict.Add("Products", prodList);

			var lorem = Lorem.AsMemory();
			for (int i = 0; i < ProductCount; i++)
			{
				prodList.Add(new Dictionary<string, object>()
				{
					{ "Name", "Name" + i},
					{ "Price", i},
					{ "Description", lorem},
				});
			}

			return productsDict;
		}
	}
	
	public class BenchPerfHarnessObject : BenchPerfHarness
	{
		public override object GetData()
		{
			const int ProductCount = 500;

			var items = new List<object>();
			var lorem = Lorem.AsMemory();
			for (int i = 0; i < ProductCount; i++)
			{
				items.Add(new
				{
					Name = "Name" + i,
					Price = i,
					Description = lorem
				});
			}

			return new
			{
				Products = items
			};
		}
	}
}
