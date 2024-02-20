using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Morestachio.Configuration.Transform
{
	public class MorestachioRuntimeConfigBuilder : IMorestachioConfigurationBuilder
	{
		private readonly IConfigurationBuilder _builder;

		public MorestachioRuntimeConfigBuilder(IConfigurationBuilder builder)
		{
			_builder = builder;
			Options = new MorestachioConfigOptions();
		}

		/// <inheritdoc />
		public IConfigurationBuilder Add(IConfigurationSource source)
		{
			return _builder.Add(source);
		}

		/// <inheritdoc />
		public IConfigurationRoot Build()
		{
			return new MorestachioConfigRoot(_builder.Build(), Options);
		}

		/// <inheritdoc />
		public IDictionary<string, object> Properties
		{
			get { return _builder.Properties; }
		}

		/// <inheritdoc />
		public IList<IConfigurationSource> Sources
		{
			get { return _builder.Sources; }
		}

		/// <summary>
		///		The options used to generate the Morestachio config
		/// </summary>
		public MorestachioConfigOptions Options { get; set; }
	}
}