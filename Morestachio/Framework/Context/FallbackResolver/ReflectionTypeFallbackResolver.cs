using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Morestachio.Document;
using Morestachio.Framework.Context.Resolver;
using Morestachio.Framework.Expression;

namespace Morestachio.Framework.Context.FallbackResolver
{
	internal class CachedReflectionTypeFallbackResolver : IFallbackValueResolver
	{
		static CachedReflectionTypeFallbackResolver()
		{
			_cache = new ConcurrentDictionary<Type, TypeCache>();
		}

		/// <inheritdoc />
		public object Resolve(ContextObject source,
							string path, 
							ScopeData scopeData,
							IMorestachioExpression morestachioExpression)
		{
			var sourceValue = source._value;
			var type = sourceValue.GetType();
			
			if (!_cache.TryGetValue(type, out var typeCache))
			{
				typeCache = new TypeCache(type.GetTypeInfo());
				_cache[type] = typeCache;
			}

			if (!typeCache._members.TryGetValue(path, out var propertyInfo))
			{
				propertyInfo = typeCache._type.GetProperty(path);
				typeCache._members[path] = propertyInfo;
			}

			if (propertyInfo != null)
			{
				return propertyInfo.GetValue(sourceValue);
			}

			scopeData.ParserOptions.OnUnresolvedPath(new InvalidPathEventArgs(source, morestachioExpression, path, type));
			return null;
		}

		private static IDictionary<Type, TypeCache> _cache;

		private class TypeCache
		{
			public TypeInfo _type;
			public IDictionary<string, PropertyInfo> _members;

			public TypeCache(TypeInfo type)
			{
				_type = type;
				_members = new Dictionary<string, PropertyInfo>();
			}
		}
	}
}
