using System;
using System.Collections.Generic;
using Morestachio.Profiler;

namespace Morestachio;

internal static class PerformanceProfilerExtensions
{
	private class FakeDisposable : IDisposable
	{
		public void Dispose()
		{
		}
	}

	private static IDisposable _instance = new FakeDisposable();

	public static IDisposable BeginSafe(this PerformanceProfiler profiler, string name)
	{
		return profiler?.Begin(name) ?? _instance;
	}

	public static PerformanceProfiler.PerformanceKey AddOrGet(this HashSet<PerformanceProfiler.PerformanceKey> profiler, PerformanceProfiler.PerformanceKey key)
	{
		if (profiler.Contains(key))
		{
			return key;
		}

		profiler.Add(key);
		return key;
	}
}