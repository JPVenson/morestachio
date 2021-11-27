using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Morestachio.Util
{
	internal static class StringBuilderCache
	{
		static StringBuilderCache()
		{
			_cache = new Queue<StringBuilder>(10);
			//in theory there is no need for the full sized ConcurrentQueue because we have no external direct access to our collection
			_cacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
		}

		private static Queue<StringBuilder> _cache;
		private static ReaderWriterLockSlim _cacheLock;

		public static StringBuilder Acquire(int capacity = 16)
		{
			
			//it should also be considered to sort or order the collection of stringbuilders to ensure optimal memory usage
			//otherwise we might end up with unsessary large string builders that will be used for very small concatinations and no way in 
			//getting smaller builders
			//var cachedBuilder = _cache.Select(e => (builder: e, dif: e.Capacity.CompareTo(capacity))).OrderBy(e => e.dif).FirstOrDefault().builder;

			_cacheLock.EnterReadLock();
			if (_cache.Count > 0)
			{
				StringBuilder cachedBuilder;
				_cacheLock.ExitReadLock();
				_cacheLock.EnterWriteLock();
				try
				{
					cachedBuilder = _cache.Dequeue();
				}
				finally
				{
					_cacheLock.ExitWriteLock();
				}
				
				cachedBuilder.Clear();
				return cachedBuilder;
			}
			_cacheLock.ExitReadLock();
			return new StringBuilder(capacity);
		}

		public static void Release(StringBuilder sb)
		{
			_cacheLock.EnterWriteLock();
			try
			{
				_cache.Enqueue(sb);
			}
			finally
			{
				_cacheLock.ExitWriteLock();
			}
		}

		public static string GetStringAndRelease(StringBuilder sb)
		{
			var result = sb.ToString();
			Release(sb);
			return result;
		}
	}	
	
	//internal static class StringBuilderCache
	//{
	//	static StringBuilderCache()
	//	{
	//		_cache = new Queue<StringBuilder>(10);
	//	}

	//	private static Queue<StringBuilder> _cache;

	//	public static StringBuilder Acquire(int capacity = 16)
	//	{
	//		//var cachedBuilder = _cache.Select(e => (builder: e, dif: e.Capacity.CompareTo(capacity))).OrderBy(e => e.dif).FirstOrDefault().builder;

	//		if (_cache.Count > 0)
	//		{
	//			var cachedBuilder = _cache.Dequeue();
	//			cachedBuilder.Clear();
	//			return cachedBuilder;
	//		}
	//		return new StringBuilder(capacity);
	//	}

	//	public static void Release(StringBuilder sb)
	//	{
	//		_cache.Enqueue(sb);
	//	}

	//	public static string GetStringAndRelease(StringBuilder sb)
	//	{
	//		var result = sb.ToString();
	//		Release(sb);
	//		return result;
	//	}
	//}

	//internal static class StringBuilderCache
	//{
	//	static StringBuilderCache()
	//	{
	//		_cache = new ConcurrentQueue<StringBuilder>();
	//	}

	//	private static ConcurrentQueue<StringBuilder> _cache;

	//	public static StringBuilder Acquire(int capacity = 16)
	//	{
	//		//var cachedBuilder = _cache.Select(e => (builder: e, dif: e.Capacity.CompareTo(capacity))).OrderBy(e => e.dif).FirstOrDefault().builder;

	//		if (_cache.TryDequeue(out var cachedBuilder))
	//		{
	//			cachedBuilder.Clear();
	//			return cachedBuilder;
	//		}
	//		return new StringBuilder(capacity);
	//	}

	//	public static void Release(StringBuilder sb)
	//	{
	//		_cache.Enqueue(sb);
	//	}

	//	public static string GetStringAndRelease(StringBuilder sb)
	//	{
	//		var result = sb.ToString();
	//		Release(sb);
	//		return result;
	//	}
	//}
}
