using System;
using System.Collections.Generic;
using System.Text;

namespace Morestachio.Util
{
	internal static class StringBuilderCache
	{
		static StringBuilderCache()
		{
			_cache = new Queue<StringBuilder>(10);
		}

		private static Queue<StringBuilder> _cache;

		public static StringBuilder Acquire(int capacity = 16)
		{
			//var cachedBuilder = _cache.Select(e => (builder: e, dif: e.Capacity.CompareTo(capacity))).OrderBy(e => e.dif).FirstOrDefault().builder;
			
			if (_cache.Count > 0)
			{
				var cachedBuilder = _cache.Dequeue();
				cachedBuilder.Clear();
				return cachedBuilder;
			}
			return new StringBuilder(capacity);
		}

		public static void Release(StringBuilder sb)
		{
			_cache.Enqueue(sb);
		}

		public static string GetStringAndRelease(StringBuilder sb)
		{
			var result = sb.ToString();
			Release(sb);
			return result;
		}
	}
}
