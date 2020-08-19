using System;
using System.Threading.Tasks;

namespace Morestachio.AspNetCore
{
	public interface IMorestachioDataCache<T, TKey>
	{
		ValueTask<T> GetCache(TKey arguments, Func<TKey, ValueTask<T>> factory);
		ValueTask AddCache(TKey arguments, T data);
	}
}