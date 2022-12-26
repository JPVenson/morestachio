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

namespace Morestachio.Framework.Context.FallbackResolver;

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
		var sourceValue = source.InternalValue;
		var type = sourceValue.GetType();
			
		var typeCache = _cache.GetOrAdd(type, (key) => new TypeCache(key.GetTypeInfo()));
		var propertyInfo = typeCache._members.GetOrAdd(path, (key) => typeCache._type.GetProperty(key));
			
		if (propertyInfo != null)
		{
			return propertyInfo.GetValue(sourceValue);
		}

		scopeData.ParserOptions.OnUnresolvedPath(new InvalidPathEventArgs(source, morestachioExpression, path, type));
		return null;
	}

	private static ConcurrentDictionary<Type, TypeCache> _cache;

	private class TypeCache
	{
		public TypeInfo _type;
		public ConcurrentDictionary<string, PropertyInfo> _members;

		public TypeCache(TypeInfo type)
		{
			_type = type;
			_members = new ConcurrentDictionary<string, PropertyInfo>();
		}
	}
}