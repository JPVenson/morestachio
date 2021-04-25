using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Morestachio.Configuration.Transform
{
	public class MorestachioConfigProvider : IConfigurationProvider
	{
		private readonly IConfigurationProvider _provider;
		private readonly MorestachioConfigOptions _options;

		public MorestachioConfigProvider(IConfigurationProvider provider, MorestachioConfigOptions options)
		{
			_provider = provider;
			_options = options;
		}

		public bool TryGet(string key, out string value)
		{
			if (_provider.TryGet(key, out value))
			{
				value = MorestachioConfig.CheckAndTransformValue(key, value, _options).Value;
				return true;
			}
			return false;
		}

		public void Set(string key, string value)
		{
			_provider.Set(key, value);
		}

		public IChangeToken GetReloadToken()
		{
			return _provider.GetReloadToken();
		}

		public void Load()
		{
			_provider.Load();
		}

		public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string parentPath)
		{
			return _provider.GetChildKeys(earlierKeys, parentPath);
		}
	}
}