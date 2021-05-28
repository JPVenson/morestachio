using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Morestachio.Framework.Context.Resolver;

namespace Morestachio.Util.StaticBinding
{
	/// <summary>
	///		Wraps an object type and allows to call static properties on it
	/// </summary>
	public class Static : IMorestachioPropertyResolver
	{
		private readonly Type _type;

		/// <summary>
		///		Creates a new static object that wraps its static properties
		/// </summary>
		/// <param name="type"></param>
		public Static(Type type)
		{
			_type = type;
		}
		
		/// <inheritdoc />
		public bool TryGetValue(string name, out object found)
		{
			var property = _type.GetProperty(name, BindingFlags.Static | BindingFlags.Public);
			if (property == null || !property.CanRead)
			{
				found = null;
				return false;
			}

			found = property.GetMethod.Invoke(null, null);
			return true;
		}
	}
	
	/// <inheritdoc />
	public class Static<T> : Static
	{
		/// <inheritdoc />
		public Static() : base(typeof(T))
		{
		}
	}
}
