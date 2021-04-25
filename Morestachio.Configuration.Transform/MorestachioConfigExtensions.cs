using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Morestachio.Configuration.Transform
{
	public static class MorestachioConfigExtensions
	{
		/// <summary>
		///		Replaces the builder with a <see cref="MorestachioRuntimeConfigBuilder"/> that will wrap all Configuration calls and check for a morestachio config
		/// </summary>
		/// <param name="builder"></param>
		/// <returns></returns>
		public static IMorestachioConfigurationBuilder UseRuntimeMorestachio(this IConfigurationBuilder builder)
		{
			return new MorestachioRuntimeConfigBuilder(builder);
		}

		/// <summary>
		///		Replaces the builder with a <see cref="MorestachioBuildtimeConfigBuilder"/> that will preprocess all keys
		/// </summary>
		/// <param name="builder"></param>
		/// <returns></returns>
		public static IMorestachioConfigurationBuilder UseBuildtimeMorestachio(this IConfigurationBuilder builder)
		{
			return new MorestachioBuildtimeConfigBuilder(builder);
		}

		/// <summary>
		///		Sets a function to get a new <see cref="ParserOptions"/>
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static IMorestachioConfigurationBuilder UseOptions(this IMorestachioConfigurationBuilder builder, Func<ParserOptions> options)
		{
			builder.Options.ParserOptions = options;
			return builder;
		}

		/// <summary>
		///		Sets a function to evaluate if a key or value needs to be processed by morestachio
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="discovery"></param>
		/// <returns></returns>
		public static IMorestachioConfigurationBuilder UseDiscovery(this IMorestachioConfigurationBuilder builder, Func<KeyValuePair<string, string>, bool> discovery)
		{
			builder.Options.TransformCondition = discovery;
			return builder;
		}

		/// <summary>
		///		Adds a set of values to the morestachio config.
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="key">Can be ether null for all or a key</param>
		/// <param name="values">To set of key-values that should be passed to the config expression</param>
		/// <returns></returns>
		public static IMorestachioConfigurationBuilder UseValues(this IMorestachioConfigurationBuilder builder, string key, IDictionary<string, object> values)
		{
			key = key ?? string.Empty;

			if (!builder.Options.Values.TryGetValue(key, out var vals))
			{
				vals = new Dictionary<string, object>();
				builder.Options.Values.Add(key, vals);
			}

			foreach (var value in values)
			{
				vals[value.Key] = value.Value;
			}

			return builder;
		}

		/// <summary>
		///		Adds the EnvironmentVariables to the key generation
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static IMorestachioConfigurationBuilder UseEnvironmentVariableValues(
			this IMorestachioConfigurationBuilder builder, string key)
		{
			var values = Environment.GetEnvironmentVariables()
				.OfType<DictionaryEntry>()
				.ToDictionary(e => e.Key.ToString(), e => e.Value);

			return builder.UseValues(key, values);
		}
	}
}