using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using Morestachio.Benchmark.PerfRendering;
using Morestachio.Framework.IO;
using Morestachio.TemplateContainers;

namespace Morestachio.Benchmark
{
	public class Program
	{
		static async Task Main(string[] args)
		{
			//var bencher = new BenchPerfHarnessDictionary();
			//await bencher.Setup();

			//for (int i = 0; i < 1000; i++)
			//{
			//	await bencher.Bench();
			//}


			var switcher = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly);
			switcher.Run(args);
			Console.WriteLine("-----------------");
			Console.WriteLine("Benchmark done. Press any key to close.");
			Console.ReadKey();
		}
	}
}
