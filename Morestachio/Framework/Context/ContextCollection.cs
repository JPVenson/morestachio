using System;
using System.Collections.Generic;
using Morestachio.Framework.Context.Resolver;

namespace Morestachio.Framework.Context;

/// <summary>
///     A context object for collections that is generated for each item inside a collection
/// </summary>
public class ContextCollection : ContextObject
{
	/// <summary>
	///     ctor
	/// </summary>
	/// <param name="index">the current index of the item inside the collection</param>
	/// <param name="last">true if its the last item</param>
	public ContextCollection(long index, 
							bool last, 
							string key, 
							ContextObject parent,
							object value) 
		: base(key, parent, value)
	{
		Index = index;
		Last = last;
	}

	/// <summary>
	///     The current index inside the collection
	/// </summary>
	public long Index { get; internal set; }

	/// <summary>
	///     True if its the last item in the current collection
	/// </summary>
	public bool Last { get; internal set; }

	/// <summary>
	///		Gets all variables and delegates for the context collection
	/// </summary>
	/// <returns></returns>
	public static IEnumerable<KeyValuePair<string, Func<ContextCollection, object>>> GetVariables()
	{
		yield return new KeyValuePair<string, Func<ContextCollection, object>>("$first", context => context.Index == 0);
		yield return new KeyValuePair<string, Func<ContextCollection, object>>("$index", context => context.Index);
		yield return new KeyValuePair<string, Func<ContextCollection, object>>("$middel", context => context.Index != 0 && !context.Last);
		yield return new KeyValuePair<string, Func<ContextCollection, object>>("$last", context => context.Last);
		yield return new KeyValuePair<string, Func<ContextCollection, object>>("$odd", context => context.Index % 2 != 0);
		yield return new KeyValuePair<string, Func<ContextCollection, object>>("$even", context => context.Index % 2 == 0);
	}
}