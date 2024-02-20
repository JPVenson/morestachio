using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Morestachio.Rendering;

namespace Morestachio.AspNetCore
{
	public abstract class CachedMorestachioTemplate : MorestachioTemplateBase
	{
		protected CachedMorestachioTemplate()
		{
			_dataCache = new MorestachioDataCache();
		}

		public bool CacheTemplate { get; set; }
		private IRenderer _templateCache;

		public bool CacheData { get; set; }
		private IMorestachioDataCache<object, MorestachioDataCache.DataCacheKey> _dataCache;

		public override async ValueTask<IRenderer> GetTemplate(HttpContext context)
		{
			if (CacheTemplate)
			{
				return _templateCache ?? (_templateCache = await base.GetTemplate(context));
			}

			return await base.GetTemplate(context);
		}

		public override async ValueTask<object> GetData(HttpContext context)
		{
			if (CacheData)
			{
				return _dataCache
					.GetCache(
						new MorestachioDataCache.DataCacheKey(context.Request.Path, context.Request.Query.ToArray()),
						async key => await base.GetData(context));
			}

			return await base.GetData(context);
		}
	}
}