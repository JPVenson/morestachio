using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Morestachio
{
	internal class PerformanceProfiler
	{
		public PerformanceProfiler(bool isEnabled)
		{
			IsEnabled = isEnabled;
		}

		public bool IsEnabled { get; private set; }

		public HashSet<PerformanceKey> PerformanceKeys { get; private set; }

		public IDisposable Begin(string name)
		{
			return new PerformanceKey(null, name).Start();
		}

		public class PerformanceKey : IDisposable
		{
			public PerformanceKey(string key, string name)
			{
				Key = key;
				Name = name;
				Time = TimeSpan.Zero;
				_stopwatch = new Stopwatch();
			}

			public string Key { get; }
			public string Name { get; }

			public TimeSpan Time { get; private set; }
			private Stopwatch _stopwatch;

			public PerformanceKey Start()
			{
				_stopwatch.Start();
				return this;
			}

			public void Dispose()
			{
				_stopwatch.Stop();
				Time = _stopwatch.Elapsed;
			}
		}
	}
}