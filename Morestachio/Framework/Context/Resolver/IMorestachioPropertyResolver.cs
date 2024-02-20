using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Morestachio.Formatter.Framework;

namespace Morestachio.Framework.Context.Resolver;

/// <summary>
///     Can be implemented by an object to provide custom resolving logic
/// </summary>
public interface IMorestachioPropertyResolver
{
	/// <summary>
	///     Gets the value of the property from this object
	/// </summary>
	bool TryGetValue(string name, out object found);
}

/// <summary>
///		Adds properties to an internal lookup for fast resolving
/// </summary>
public abstract class MorestachioPropertyListResolver
	: IMorestachioPropertyResolver
{
	/// <summary>
	/// 
	/// </summary>
	protected MorestachioPropertyListResolver()
	{
		_lookup = new Dictionary<string, Func<object, object>>();
	}

	private IDictionary<string, Func<object, object>> _lookup;

	/// <summary>
	///		Adds a new Property to the internal list
	/// </summary>
	/// <param name="name"></param>
	/// <param name="getter"></param>
	public virtual void Add(string name, Func<object, object> getter)
	{
		_lookup[name] = getter;
	}

	/// <summary>
	///		Adds all properties of the current type that matches the BindingFlags
	/// </summary>
	/// <param name="flags"></param>
	public virtual void AddAllProperties(BindingFlags flags)
	{
		foreach (var propertyInfo in GetType().GetProperties(flags).Where(e => e.GetMethod != null))
		{
			var prop = propertyInfo;
			Add(propertyInfo.Name, (instance) => prop.GetMethod?.Invoke(instance, null));
		}
	}

	/// <inheritdoc />
	public bool TryGetValue(string name, out object found)
	{
		if (_lookup.TryGetValue(name, out var getter))
		{
			found = getter(this);
			return true;
		}

		found = null;
		return false;
	}
}