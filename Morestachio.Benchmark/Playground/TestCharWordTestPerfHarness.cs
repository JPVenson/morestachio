using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;

namespace Morestachio.Benchmark.Playground;

[MemoryDiagnoser()]
public class TestCharWordTestPerfHarness
{
	public TestCharWordTestPerfHarness()
	{
		regex = new Regex("^\\w\\d$");
	}

	Regex regex;

	[Benchmark]
	public void TestCharIsLetterOrDigit()
	{
		var isLetterOrDigit = char.IsLetterOrDigit('C');
	}

	[Benchmark]
	public void TestRegexIsLetterOrDigit()
	{
		var match = new Regex("^\\w\\d$").IsMatch("C");
	}

	[Benchmark]
	public void TestPreCompiledRegexIsLetterOrDigit()
	{
		var match = regex.IsMatch("C");
	}
}