using System;
using System.Collections.Generic;
using System.Dynamic;
using Morestachio.Document;
using Morestachio.Framework.Context;
using Morestachio.Framework.Context.Resolver;
using Morestachio.Helper.Logging;
using Newtonsoft.Json.Linq;

namespace Morestachio.Newtonsoft.Json
{
	/// <summary>
	///		Extension method for adding the JsonValue Resolver
	/// </summary>
	public static class JsonNetValueResolverExtensions
	{
		/// <summary>
		///		Sets or Adds the Json Value resolver
		/// </summary>
		/// <param name="optionsBuilder"></param>
		/// <returns></returns>
		public static IParserOptionsBuilder WithJsonValueResolver(this IParserOptionsBuilder optionsBuilder)
		{
			return optionsBuilder.WithValueResolver(new JsonNetValueResolver());
		}
	}

	/// <summary>
	///		Defines a Value resolver that can resolve values from an Newtonsoft.Json parsed json file
	/// </summary>
	public class JsonNetValueResolver : IValueResolver
	{
		/// <inheritdoc />
		public object Resolve(
			Type type,
			object value,
			string path,
			ContextObject context,
			ScopeData scopeData
		)
		{
			return ResolveJObject(value, path, context, scopeData);
		}

		public static object ResolveJObject(object value,
											string path,
											ContextObject contextObject,
											ScopeData scopeData)
		{
			switch (value)
			{
				case JObject jValue:
				{
					var val = jValue.Value<object>(path);

					if (val is JToken jToken)
					{
						return EvalJToken(jToken);
					}

					return val;
				}
				case JValue jVal:
					return jVal.Value;
				case JArray jArr:
					return EvalJToken(jArr);
				default:
					scopeData.ParserOptions.Logger?.LogWarn(nameof(JsonNetValueResolver),
						$"Could not resolve Json object path from type: {(value?.GetType().ToString() ?? "null")}");
					return value;
			}
		}

		public static object EvalJToken(JToken token)
		{
			return token switch
			{
				JValue jToken => jToken.Value,
				JArray jArray => EvalJArray(jArray),
				JObject jObject => EvalJObject(jObject),
				_ => null
			};
		}

		public static IDictionary<string, object> EvalJObject(JObject obj)
		{
			var dict = (IDictionary<string, object>)new ExpandoObject();

			foreach (var property in obj.Properties())
			{
				dict[property.Name] = EvalJToken(property.Value);
			}

			return dict;
		}

		private static IEnumerable<object> EvalJArray(JArray jArr)
		{
			var arrElements = new List<object>();

			foreach (var jToken in jArr)
			{
				arrElements.Add(EvalJToken(jToken));
			}

			return arrElements;
		}

		/// <inheritdoc />
		public bool CanResolve(
			Type type,
			object value,
			string path,
			ContextObject context,
			ScopeData scopeData
		)
		{
			return type == typeof(JObject) || type == typeof(JValue) || type == typeof(JArray);
		}

		public bool IsSealed { get; private set; }

		public void Seal()
		{
			IsSealed = true;
		}
	}
}