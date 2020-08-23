using System;
using System.Collections.Generic;
using System.Dynamic;
using Morestachio.Framework.Context;
using Morestachio.Framework.Context.Resolver;
using Newtonsoft.Json.Linq;

namespace Morestachio.Newtonsoft.Json
{
	public static class JsonNetValueResolverExtensions
	{
		public static void AddJsonValueResolver(this ParserOptions options)
		{
			if (options.ValueResolver is IList<IValueResolver> listResolver)
			{
				listResolver.Add(new JsonNetValueResolver());
			}
			else
			{
				options.ValueResolver = new JsonNetValueResolver();	
			}
		}
	}

	/// <summary>
	///		Defines a Value resolver that can resolve values from an Newtonsoft.Json parsed json file
	/// </summary>
	public class JsonNetValueResolver : IValueResolver
	{
		/// <inheritdoc />
		public object Resolve(Type type, object value, string path, ContextObject context)
		{
			if (value is JObject jValue)
			{
				var val = jValue.Value<object>(path);
				if (val is JToken jToken)
				{
					return EvalJToken(jToken);
				}
				return val;
			}

			if (value is JValue jVal)
			{
				return jVal.Value;
			}

			if (value is JArray jArr)
			{
				return EvalJToken(jArr);
			}

			return value;
		}

		public object EvalJToken(JToken token)
		{
			if (token is JValue jToken)
			{
				return jToken.Value;
			}
			if (token is JArray jArray)
			{
				return EvalJArray(jArray);
			}
			if (token is JObject jObject)
			{
				return EvalJObject(jObject);
			}

			return null;
		}

		public IDictionary<string, object> EvalJObject(JObject obj)
		{
			var dict = (IDictionary<string, object>)new ExpandoObject();
			foreach (var property in obj.Properties())
			{
				dict[property.Name] = EvalJToken(property.Value);
			}

			return dict;
		}

		private IEnumerable<object> EvalJArray(JArray jArr)
		{
			var arrElements = new List<object>();
			foreach (var jToken in jArr)
			{
				arrElements.Add(EvalJToken(jToken));
			}

			return arrElements;
		}

		/// <inheritdoc />
		public bool CanResolve(Type type, object value, string path, ContextObject context)
		{
			return type == typeof(JObject) || type == typeof(JValue);
		}

		public bool IsSealed { get; private set; }
		public void Seal()
		{
			IsSealed = true;
		}
	}
}
