using System;
using System.Collections.Generic;
using System.Linq;
using Morestachio.Document;

namespace Morestachio.Framework.Context.Resolver;

/// <summary>
///		Combines any number of value resolvers
/// </summary>
public class MultiValueResolver : List<IValueResolver>, IValueResolver
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
		return this.First(f => f.CanResolve(type, value, path, context, scopeData))
			.Resolve(type, value, path, context, scopeData);
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
		return this.Any(f => f.CanResolve(type, value, path, context, scopeData));
	}

	/// <inheritdoc />
	public bool IsSealed { get; private set; }

	/// <inheritdoc />
	public void Seal()
	{
		IsSealed = true;
	}
}