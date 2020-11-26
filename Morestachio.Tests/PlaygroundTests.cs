using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Morestachio.Framework.Context.Resolver;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Tokenizing;
using Morestachio.Helper;
using Morestachio.Profiler;
using Morestachio.Tests.PerfTests;
using NUnit.Framework;

#if ValueTask
using Promise = System.Threading.Tasks.ValueTask;
#else
using Promise = System.Threading.Tasks.Task;
#endif

namespace Morestachio.Tests
{
	[TestFixture(Category = "Playground")]
	[Explicit]
	public class PlaygroundTests
	{
		public PlaygroundTests()
		{

		}

		public void Generic<T>(IEnumerable<T> value)
		{
			Console.WriteLine(value);
		}

		public const string TextTemplateMorestachio = @"
<ul id='products'>
  {{#each Products}}
	<li>
	  <h2>{{Name}}</h2>
		   Only {{Price}}
		   {{Description.Truncate(15)}}
	</li>
  {{/each}}
</ul>
";
		private const string Lorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum";

		public class Product : IMorestachioPropertyResolver
		{
			public Product()
			{
				
			}

			public Product(string name, float price, string description)
			{
				Name = name;
				Price = price;
				Description = description;
			}

			public string Name { get; set; }

			public float Price { get; set; }

			public string Description { get; set; }

			public bool TryGetValue(string name, out object found)
			{
				if (name == nameof(Name))
				{
					found = Name;
					return true;
				}
				if (name == nameof(Price))
				{
					found = Price;
					return true;
				}
				if (name == nameof(Description))
				{
					found = Description;
					return true;
				}

				found = null;
				return false;
			}
		}

		private class Stopwatches
		{
			public Stopwatches()
			{
				Watches = new List<TimeSpan>();
			}
			public Stopwatch Current { get; set; }
			public List<TimeSpan> Watches { get; set; }

			public void Start()
			{
				Current = new Stopwatch();
				Current.Start();
			}

			public void Stop()
			{
				Current.Stop();
				Watches.Add(Current.Elapsed);
			}

			public TimeSpan Elapsed
			{
				get
				{
					return TimeSpan.FromTicks(Watches.Sum(f => f.Ticks));
				}
			}

			public TimeSpan ElapsedAverage
			{
				get
				{
					return TimeSpan.FromTicks((long) Watches.Average(f => (decimal)f.Ticks));
				}
			}

			public TimeSpan ElapsedMin
			{
				get
				{
					return Watches.Min();
				}
			}

			public TimeSpan ElapsedMax
			{
				get
				{
					return Watches.Max();
				}
			}
		}

		[Test]
		[Explicit]
		public async Task ProfileTest()
		{
			string variation = "Template Size";
			int modelDepth = 5;
			int sizeOfTemplate = 100000;
			int inserts = 5;
			int runs = 1;

			var model = PerfHarness.ConstructModelAndPath(modelDepth);
			var baseTemplate = Enumerable.Range(1, 5)
				.Aggregate("", (seed, current) => seed += " {{" + model.Item2 + "}}");
			while (baseTemplate.Length <= sizeOfTemplate)
			{
				baseTemplate += model.Item2 + "\r\n";
			}

			MorestachioDocumentInfo template = null;
			TokenizerResult tokenizerResult = null;

			//make sure this class is JIT'd before we start timing.
			//await Parser.ParseWithOptionsAsync(new ParserOptions("asdf"));

			var totalTime = Stopwatch.StartNew();
			var tokenizingTime = Stopwatch.StartNew();

			for (var i = 0; i < runs; i++)
			{
				var options = new ParserOptions(baseTemplate, () => Stream.Null);
				var tokenzierContext = new TokenzierContext(new List<int>(), options.CultureInfo);
				tokenizerResult = await Tokenizer.Tokenize(options, tokenzierContext);
			}

			tokenizingTime.Stop();

			//var parseTime = Stopwatch.StartNew();
			//for (var i = 0; i < runs; i++)
			//{
			//	var options = new ParserOptions(baseTemplate, () => Stream.Null);
			//	template = new MorestachioDocumentInfo(options, Parser.Parse(tokenizerResult, options));
			//}

			//parseTime.Stop();

			//var tmp = await template.CreateAndStringifyAsync(model.Item1);

			//var renderTime = Stopwatch.StartNew();
			//for (var i = 0; i < runs; i++)
			//{
			//	var morestachioDocumentResult = await template.CreateAsync(model.Item1);
			//	morestachioDocumentResult.Stream.Dispose();
			//}

			//renderTime.Stop();
			//totalTime.Stop();

			//var compileTime = Stopwatch.StartNew();
			//CompilationResult compilationResult = null;
			//for (var i = 0; i < runs; i++)
			//{
			//	compilationResult = template.Compile();
			//}

			//compileTime.Stop();

			//var compiledRenderTime = Stopwatch.StartNew();
			//for (var i = 0; i < runs; i++)
			//{
			//	var morestachioDocumentResult = await compilationResult(model.Item1, CancellationToken.None);
			//	morestachioDocumentResult.Stream.Dispose();
			//}

			//compiledRenderTime.Stop();
		}

		[Test]
		[Explicit]
		[Repeat(5)]
		public async Task PerformanceDebuggerTest()
		{
			var _products = new List<object>(500);
			for (int i = 0; i < 500; i++)
			{
				_products.Add(new Dictionary<string, object>()
				{
					{"Name", "Name" + i},
					{"Price", i},
					{"Description", Lorem},
				});
				//_products.Add(new Product()
				//{
				//	Name = "Name" + i,
				//	Price = i,
				//	Description = Lorem
				//});
			}

			var parsingOptions = new ParserOptions(TextTemplateMorestachio, null, Encoding.UTF8, true);
			parsingOptions.ProfileExecution = false;
			var parsed = await Parser.ParseWithOptionsAsync(parsingOptions);
			var andStringifyAsync = await parsed.CreateAndStringifyAsync(new
			{
				Products = _products
			});
			var runs = 200;
			for (int i = 0; i < runs / 5; i++)
			{
				andStringifyAsync = await parsed.CreateAndStringifyAsync(new
				{
					Products = _products
				});
			}

			var compiled = parsed.Compile();

			var sw = new Stopwatches();
			for (int i = 0; i < runs; i++)
			{
				sw.Start();
				await compiled(new
				{
					Products = _products
				}, CancellationToken.None);
				//var f = await parsed.CreateAsync(new
				//{
				//	Products = _products
				//});
				sw.Stop();
			}

			var swElapsed = sw.Elapsed;
			Console.WriteLine("Done in: " 
			                  + HumanizeTimespan(swElapsed)
			                  + " thats " 
			                  + HumanizeTimespan(sw.ElapsedAverage)
			                  + " per run with lower " 
			                  + HumanizeTimespan(sw.ElapsedMin)
			                  + " and high " 
			                  + HumanizeTimespan(sw.ElapsedMax));
			#if NETCOREAPP
			Console.WriteLine("- Mem: " + Process.GetCurrentProcess().PrivateMemorySize64);
			#endif
			//PrintPerformanceGroup(profiler.SelectMany(f => f.))
		}

		[Test]
		[Explicit]
		[Repeat(5)]
		public async Task PerformanceCompiledDebuggerTest()
		{
			var _products = new List<object>(500);
			for (int i = 0; i < 500; i++)
			{
				//_products.Add(new Dictionary<string, object>()
				//{
				//	{"Name", "Name" + i},
				//	{"Price", i},
				//	{"Description", Lorem},
				//});
				_products.Add(new Product()
				{
					Name = "Name" + i,
					Price = i,
					Description = Lorem
				});
			}

			var parsingOptions = new ParserOptions(TextTemplateMorestachio, null, Encoding.UTF8, true);
			parsingOptions.ProfileExecution = false;
			var parsed = Parser.ParseWithOptions(parsingOptions).Compile();
			var andStringifyAsync = await parsed(new
			{
				Products = _products
			}, CancellationToken.None);
			var runs = 200;
			for (int i = 0; i < runs / 5; i++)
			{
				andStringifyAsync = await parsed(new
				{
					Products = _products
				}, CancellationToken.None);
			}
			var sw = new Stopwatches();
			var profiler = new List<PerformanceProfiler>();
			for (int i = 0; i < runs; i++)
			{
				sw.Start();
				var f = await parsed(new
				{
					Products = _products
				}, CancellationToken.None);
				sw.Stop();
				profiler.Add(f.Profiler);
			}

			var swElapsed = sw.Elapsed;
			Console.WriteLine("Done in: " 
			                  + HumanizeTimespan(swElapsed)
			                  + " thats " 
			                  + HumanizeTimespan(sw.ElapsedAverage)
			                  + " per run with lower " 
			                  + HumanizeTimespan(sw.ElapsedMin)
			                  + " and high " 
			                  + HumanizeTimespan(sw.ElapsedMax));
			#if NETCOREAPP
			Console.WriteLine("- Mem: " + Process.GetCurrentProcess().PrivateMemorySize64);
			#endif
			//PrintPerformanceGroup(profiler.SelectMany(f => f.))
		}

		public string HumanizeTimespan(TimeSpan timespan)
		{
			var str = new StringBuilder();
			if (timespan.Seconds > 0)
			{
				str.Append(timespan.Seconds + " s");

				if (timespan.Milliseconds > 0)
				{
					str.Append(" " + timespan.Milliseconds + " ms");
				}
			}
			else if (timespan.Milliseconds > 0)
			{
				str.Append(timespan.Milliseconds + ".");
				timespan = timespan.Subtract(TimeSpan.FromMilliseconds(timespan.Milliseconds));
				str.Append((((double)timespan.Ticks) / Stopwatch.Frequency) * 1000000000 + " ms");
			}
			else
			{
				str.Append((((double)timespan.Ticks) / Stopwatch.Frequency) * 1000000000 + " ns");
			}

			return str.ToString();
		}

		//private void PrintPerformanceGroup(IEnumerable<PerformanceProfiler.PerformanceKey> key, int intention = 0)
		//{
		//	foreach (var performanceKey in key)
		//	{
		//		var entry = performanceKey.Key;
		//		var time = perfGroup.Select(e => e.Time.Ticks).Average();

		//		Console.WriteLine($"- {entry} took '{TimeSpan.FromTicks((long) time)}' thats '{new TimeSpan((long) (time / runs))}'");
		//	}
		//}

		[Test]
		[Explicit]
		public void GenericsTest()
		{
			var x = 0;
			var y = new Number(0);

			var sw = Stopwatch.StartNew();
			var runs = 1_000_000;
			for (int i = 0; i < runs; i++)
			{
				x += 1;
			}

			sw.Stop();
			Console.WriteLine("Native Operation: " + sw.Elapsed);

			sw = Stopwatch.StartNew();

			for (int i = 0; i < runs; i++)
			{
				y += 1;
			}

			sw.Stop();
			Console.WriteLine("Number Operation: " + sw.Elapsed);
		}
	}
}

//void SetGenerics(Type target, Type source)
//{
//	var targetGenerics = target.GetGenericArguments();
//	var sourceGenerics = source.GetGenericArguments();

//	for (var index = 0; index < targetGenerics.Length; index++)
//	{
//		var targetGeneric = targetGenerics[index];
//		var sourceGeneric = sourceGenerics[index];

//		if (targetGeneric.IsGenericParameter)
//		{
//			generics.Add(sourceGeneric);
//		}
//		else
//		{
//			SetGenerics(targetGeneric, sourceGeneric);
//		}
//	}
//}

//foreach (var parameterInfo in methodInfo.GetParameters())
//{
//	var value = values[parameterInfo.Name];
//	endValues.Add(value);

//	if (parameterInfo.ParameterType.ContainsGenericParameters)
//	{
//		if (parameterInfo.ParameterType.IsGenericParameter)
//		{
//			generics.Add(value.GetType());
//		}
//		else
//		{
//			var targetGenerics = parameterInfo.ParameterType.GetGenericArguments();
//			var sourceGenerics = value.GetType().GetGenericArguments();
//			for (var index = 0; index < targetGenerics.Length; index++)
//			{
//				var targetGeneric = targetGenerics[index];
//				var sourceGeneric = sourceGenerics[index];
//				SetGenerics(targetGeneric, sourceGeneric);
//			}
//		}
//	}
//}