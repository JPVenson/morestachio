using System;
using System.Collections.Generic;
using System.Linq;
using Morestachio.Framework;

namespace Morestachio
{
	/// <summary>
	///		Combines any number of value resolvers
	/// </summary>
	public class MultiValueResolver : List<IValueResolver>, IValueResolver
	{
		/// <inheritdoc />
		public object Resolve(Type type, object value, string path, ContextObject context)
		{
			return this.First(f => f.CanResolve(type, value, path, context)).Resolve(type, value, path, context);
		}

		/// <inheritdoc />
		public bool CanResolve(Type type, object value, string path, ContextObject context)
		{
			return this.Any(f => f.CanResolve(type, value, path, context));
		}
	}
}