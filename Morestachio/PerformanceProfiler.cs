using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Morestachio
{
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

	public class PerformanceProfiler
	{
		public PerformanceProfiler(bool isEnabled)
		{
			IsEnabled = isEnabled;
			PerformanceKeys = new HashSet<PerformanceKey>();
			OperationStack = new Stack<PerformanceKey>();
		}

		public bool IsEnabled { get; private set; }

		public HashSet<PerformanceKey> PerformanceKeys { get; private set; }

		public Stack<PerformanceKey> OperationStack { get; set; }

		public IDisposable Begin(string name)
		{
			var performanceKey = (OperationStack.Peek()?.Children ?? PerformanceKeys)
				.AddOrGet(new PerformanceKey(name, name, () => OperationStack.Pop()));
			OperationStack.Push(performanceKey);
			return performanceKey.Start();
		}

		public class PerformanceKey : IDisposable, IEquatable<PerformanceKey>
		{
			private Action _disposed;

			public PerformanceKey(string key, string name, Action disposed)
			{
				_disposed = disposed;
				Key = key;
				Name = name;
				Time = TimeSpan.Zero;
				_stopwatch = new Stopwatch();
			}

			public string Key { get; }
			public string Name { get; }

			public TimeSpan Time { get; private set; }

			public HashSet<PerformanceKey> Children
			{
				get { return _children ?? (_children = new HashSet<PerformanceKey>()); }
			}

			private Stopwatch _stopwatch;
			private HashSet<PerformanceKey> _children;

			public PerformanceKey Start()
			{
				_stopwatch.Start();
				return this;
			}

			public void Dispose()
			{
				if (_disposed != null)
				{
					_disposed();
					_disposed = null;
					
					_stopwatch.Stop();
					Time = _stopwatch.Elapsed;
					foreach (var performanceKey in _children ?? Enumerable.Empty<PerformanceKey>())
					{
						performanceKey.Dispose();
					}
				}
			}

			public bool Equals(PerformanceKey other)
			{
				if (ReferenceEquals(null, other))
				{
					return false;
				}

				if (ReferenceEquals(this, other))
				{
					return true;
				}

				return Key == other.Key;
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj))
				{
					return false;
				}

				if (ReferenceEquals(this, obj))
				{
					return true;
				}

				if (obj.GetType() != this.GetType())
				{
					return false;
				}

				return Equals((PerformanceKey) obj);
			}

			public override int GetHashCode()
			{
				return (Key != null ? Key.GetHashCode() : 0);
			}
		}
	}
}