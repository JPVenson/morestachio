using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Morestachio.Framework.Context.Resolver
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
		
		/// <inheritdoc />
		public bool IsSealed { get; private set; }

		/// <inheritdoc />
		public void Seal()
		{
			IsSealed = true;
		}
	}

	/// <summary>
	///		Can resolve fields from objects in addition to properties
	/// </summary>
	public class FieldValueResolver : IValueResolver
	{
		/// <inheritdoc />
		public bool CanResolve(Type type, object value, string path, ContextObject context)
		{
			return type.GetField(path, BindingFlags.Public | BindingFlags.Instance) != null;
		}
		
		/// <inheritdoc />
		public object Resolve(Type type, object value, string path, ContextObject context)
		{
			return type.GetField(path, BindingFlags.Public | BindingFlags.Instance).GetValue(value);
		}

		/// <inheritdoc />
		public bool IsSealed { get; private set; }

		/// <inheritdoc />
		public void Seal()
		{
			IsSealed = true;
		}
	}
}