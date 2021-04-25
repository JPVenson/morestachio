using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Morestachio.Framework.Expression.Parser;

namespace Morestachio.Configuration.Transform
{
	/// <summary>
	///		Wrapper for Runtime Morestachio configs
	/// </summary>
	public class MorestachioConfig : IConfiguration
	{
		protected IConfiguration Config { get; }
		protected MorestachioConfigOptions Options { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="config"></param>
		/// <param name="options"></param>
		public MorestachioConfig(IConfiguration config, MorestachioConfigOptions options)
		{
			Config = config;
			Options = options;
		}

		/// <inheritdoc />
		public IConfigurationSection GetSection(string key)
		{
			return new MorestachioConfigSection(Config.GetSection(key), Options);
		}

		/// <inheritdoc />
		public IEnumerable<IConfigurationSection> GetChildren()
		{
			return Config.GetChildren().Select(e => new MorestachioConfigSection(e, Options));
		}

		/// <inheritdoc />
		public IChangeToken GetReloadToken()
		{
			return Config.GetReloadToken();
		}

		/// <inheritdoc />
		public string this[string key]
		{
			get
			{
				return CheckAndTransformValue(key, Config[key], Options).Value;
			}
			set { Config[key] = value; }
		}

		/// <summary>
		///		Checks if a key matches the <see cref="MorestachioConfigOptions.TransformCondition"/> and converts it
		/// </summary>
		/// <returns></returns>
		public static KeyValuePair<string, string> CheckAndTransformValue(string key, string value, MorestachioConfigOptions options)
		{
			if (options.TransformCondition(new KeyValuePair<string, string>(key, value)))
			{
				return TransformValue(new KeyValuePair<string, string>(key, value), options);
			}

			return new KeyValuePair<string, string>(key, value);
		}

		/// <summary>
		///		Transforms the value by using <see cref="Options"/>
		/// </summary>
		/// <returns></returns>
		public static KeyValuePair<string, string> TransformValue(KeyValuePair<string, string> keyValue, MorestachioConfigOptions options)
		{
			keyValue = options.PreTransform(keyValue);
			var parserOptions = options.ParserOptions();
			var values = new Dictionary<string, object>();
			if (options.Values.TryGetValue(string.Empty, out var rootValues))
			{
				foreach (var rootValue in rootValues)
				{
					values[rootValue.Key] = rootValue.Value;
				}
			}

			IList<string> keyPaths = new List<string>();
			foreach (var keyPathPart in keyValue.Key.Split(':'))
			{
				keyPaths.Add(keyPathPart);
				var keyPath = string.Join(":", keyPaths);
				if (options.Values.TryGetValue(keyPath, out var specificValues))
				{
					foreach (var specificValue in specificValues)
					{
						values[specificValue.Key] = specificValue.Value;
					}
				}
			}
			var valueTask = ExpressionParser.EvaluateExpression(keyValue.Value, parserOptions, values);
			string result;
			if (valueTask.IsCompleted)
			{
				result = valueTask.Result?.ToString();
			}
			else
			{
				result = valueTask.GetAwaiter().GetResult()?.ToString();
			}

			return options.PostTransform(new KeyValuePair<string, string>(keyValue.Key, result));
		}
	}
}
