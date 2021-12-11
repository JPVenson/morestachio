using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Morestachio.Benchmark.PerfRendering
{
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

		//[Benchmark(Baseline = true, Description = "Call Object")]
		//public ValueTask<string> BenchObject()
		//{
		//	return _objectCall.Bench();
		//}
	}
}