using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Morestachio.Configuration.Transform
{
	/// <summary>
	///		Wraps the <see cref="IConfigurationBuilder"/> and preprocesses all keys that matches the <see cref="MorestachioConfigOptions.TransformCondition"/>
	/// </summary>
	public class MorestachioBuildtimeConfigBuilder : IMorestachioConfigurationBuilder
	{
		private readonly IConfigurationBuilder _builder;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="builder"></param>
		public MorestachioBuildtimeConfigBuilder(IConfigurationBuilder builder)
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
			var configurationRoot = _builder.Build();
			foreach (var keyValuePair in configurationRoot.AsEnumerable())
			{
				if (Options.TransformCondition(keyValuePair))
				{
					var transformValue = MorestachioConfig
						.TransformValue(new KeyValuePair<string, string>(keyValuePair.Key, keyValuePair.Value), Options);
					configurationRoot[transformValue.Key] = transformValue.Value;
				}
			}
			return configurationRoot;
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
		
		/// <inheritdoc />
		public MorestachioConfigOptions Options { get; set; }
	}
}