using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Morestachio.Configuration.Transform
{
	/// <summary>
	///		Wrapper for Runtime Morestachio configs
	/// </summary>
	public class MorestachioConfigRoot : MorestachioConfig, IConfigurationRoot
	{
		private readonly IConfigurationRoot _root;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="root"></param>
		/// <param name="options"></param>
		public MorestachioConfigRoot(IConfigurationRoot root, MorestachioConfigOptions options) : base(root, options)
		{
			_root = root;
		}

		/// <inheritdoc />
		public void Reload()
		{
			_root.Reload();
		}

		/// <inheritdoc />
		public IEnumerable<IConfigurationProvider> Providers
		{
			get { return _root.Providers.Select(e => new MorestachioConfigProvider(e, Options)); }
		}
	}
}