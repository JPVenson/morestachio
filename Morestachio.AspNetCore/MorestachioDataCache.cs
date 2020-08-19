using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace Morestachio.AspNetCore
{
	public class MorestachioDataCache : IMorestachioDataCache<object, MorestachioDataCache.DataCacheKey>
	{
		public MorestachioDataCache()
		{
			_cache = new ConcurrentDictionary<DataCacheKey, object>();
		}

		public readonly struct DataCacheKey : IEquatable<DataCacheKey>
		{
			public DataCacheKey(string path, KeyValuePair<string, StringValues>[] query)
			{
				Path = path;
				Query = query;
			}

			public string Path { get; }
			public KeyValuePair<string, StringValues>[] Query { get; }

			public bool Equals(DataCacheKey other)
			{
				return Path == other.Path && Equals(Query, other.Query);
			}

			public override bool Equals(object obj)
			{
				return obj is DataCacheKey other && Equals(other);
			}

			public override int GetHashCode()
			{
				return HashCode.Combine(Path, Query.Any()
					? Query.Select(f => HashCode.Combine(f.Key, f.Value)).Aggregate((e, f) => e + f) : 0);
			}
		}

		private readonly ConcurrentDictionary<DataCacheKey, object> _cache;

		public ValueTask<object> GetCache(DataCacheKey arguments, Func<DataCacheKey, ValueTask<object>> factory)
		{
			return new ValueTask<object>(_cache.GetOrAdd(arguments, f => factory(f).AsTask().GetAwaiter().GetResult()));
		}

		public ValueTask AddCache(DataCacheKey arguments, object data)
		{
			_cache.TryAdd(arguments, data);
			return new ValueTask();
		}
	}
}