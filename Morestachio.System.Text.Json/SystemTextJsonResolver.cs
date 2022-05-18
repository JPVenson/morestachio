using System;
using System.Linq;
using System.Text.Json;
using Morestachio.Document;
using Morestachio.Framework.Context;
using Morestachio.Framework.Context.Resolver;
using Morestachio.Helper;
using Morestachio.Helper.Logging;

namespace Morestachio.System.Text.Json
{
	/// <summary>
	///		Extension method for adding the JsonValue Resolver
	/// </summary>
	public static class SystemTextJsonResolverExtensions
	{
		///  <summary>
		/// 		Sets or Adds the Json Value resolver
		///  </summary>
		///  <param name="optionsBuilder"></param>
		///  <param name="lazyEvaluation">If set to true the resolver will return an <see cref="JsonElement"/> for arrays and objects instead of Dictionary and arrays</param>
		///  <returns></returns>
		public static IParserOptionsBuilder WithSystemTextJsonValueResolver(this IParserOptionsBuilder optionsBuilder)
		{
			return optionsBuilder.WithValueResolver(new SystemTextJsonResolver());
		}
	}

	/// <summary>
	///		Defines a Value resolver that can resolve from System.Text.Json documents.
	/// </summary>
	public class SystemTextJsonResolver : IValueResolver
	{
		/// <summary>
		///		Creates a new <see cref="SystemTextJsonResolver"/>.
		/// </summary>
		public SystemTextJsonResolver()
		{
		}

		/// <inheritdoc />
		public bool IsSealed { get; private set; }

		/// <inheritdoc />
		public void Seal()
		{
			IsSealed = true;
		}

		/// <inheritdoc />
		public object Resolve(
			Type type,
			object value,
			string path,
			ContextObject context,
			ScopeData scopeData
		)
		{
			return ResolveJsonObject(value, path, scopeData);
		}

		public object ResolveJsonObject(in object value, in string path, in ScopeData scopeData)
		{
			switch (value)
			{
				case JsonDocument document:
					return ResolveJsonElement(document.RootElement, path);
				case JsonElement element:
					return ResolveJsonElement(element, path);
	
				default:
					scopeData.ParserOptions.Logger?.LogWarn(nameof(SystemTextJsonResolver), 
						$"Could not resolve Json object path from type: {(value?.GetType().ToString() ?? "null")}");
					return value;
			}
		}

		private object ResolveJsonElement(in JsonElement jsonElement, in string path)
		{
			var jsonAtPath = jsonElement.GetProperty(path);
			return EvalJsonValue(jsonAtPath);
		}

		private object EvalJsonValue(JsonElement jsonAtPath)
		{
			switch (jsonAtPath.ValueKind)
			{
				case JsonValueKind.Object:
					return EvalJsonObject(jsonAtPath);
				case JsonValueKind.Array:
					return jsonAtPath.EnumerateArray().Select(EvalJsonValue).ToArray();
				case JsonValueKind.Null:
				case JsonValueKind.Undefined:
					return null;
				case JsonValueKind.String:
					return jsonAtPath.ToString();
				case JsonValueKind.Number:
					if (jsonAtPath.TryGetDecimal(out var decNo))
					{
						return new Number(decNo);
					}

					return new Number(jsonAtPath.GetInt64());
				case JsonValueKind.True:
					return true;
				case JsonValueKind.False:
					return false;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private object EvalJsonObject(JsonElement jsonAtPath)
		{
			return jsonAtPath.EnumerateObject().ToDictionary(e => e.Name, e => EvalJsonValue(e.Value));
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
			return type == typeof(JsonDocument) || type == typeof(JsonElement);
		}
	}
}