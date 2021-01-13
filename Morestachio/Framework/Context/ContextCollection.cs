using System;
using System.Collections.Generic;

namespace Morestachio.Framework.Context
{
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
		/// <param name="options"></param>
		/// <param name="key"></param>
		public ContextCollection(long index, 
			bool last, 
			 ParserOptions options, 
			string key, 
			ContextObject parent,
			object value) 
			: base(options, key, parent, value)
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

		///// <inheritdoc />
		//public override ContextObject GetContextVariable(string path)
		//{
		//	object value = null;
		//	if (path.Equals("$first"))
		//	{
		//		value = Index == 0;
		//	}
		//	else if (path.Equals("$index"))
		//	{
		//		value = Index;
		//	}
		//	else if (path.Equals("$middel"))
		//	{
		//		value = Index != 0 && !Last;
		//	}
		//	else if (path.Equals("$last"))
		//	{
		//		value = Last;
		//	}
		//	else if (path.Equals("$odd"))
		//	{
		//		value = Index % 2 != 0;
		//	}
		//	else if (path.Equals("$even"))
		//	{
		//		value = Index % 2 == 0;
		//	}
		//	return value == null ? null : Options.CreateContextObject(path, CancellationToken, value, this);
		//}
	}
}