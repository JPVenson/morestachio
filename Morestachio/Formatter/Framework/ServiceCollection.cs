using System;
using System.Collections.Generic;

namespace Morestachio.Formatter.Framework
{
	public class ServiceCollection
	{
		private readonly IDictionary<Type, object> _localSource;
		private readonly IDictionary<Type, object> _source;

		public ServiceCollection(IDictionary<Type, object> source)
		{
			_source = source;
			_localSource = new Dictionary<Type, object>
			{
				{typeof(ServiceCollection), this}
			};
		}

		/// <inheritdoc />
		public void AddService<T, TE>(TE service) where TE : T
		{
			_localSource[typeof(TE)] = service;
		}

		/// <inheritdoc />
		public void AddService<T>(T service)
		{
			_localSource[typeof(T)] = service;
		}

		/// <inheritdoc />
		public void AddService<T, TE>(Func<TE> serviceFactory) where TE : T
		{
			_localSource[typeof(TE)] = serviceFactory;
		}

		/// <inheritdoc />
		public void AddService<T>(Func<T> serviceFactory)
		{
			_localSource[typeof(T)] = serviceFactory;
		}

		public bool GetService<T>(out T service)
		{
			var found = GetService(typeof(T), out var serviceTem);
			if (found)
			{
				service = (T) serviceTem;
			}
			else
			{
				service = default;
			}

			return found;
		}

		public bool GetService(Type serviceType, out object service)
		{
			if (!_localSource.TryGetValue(serviceType, out service))
			{
				if (!_source.TryGetValue(serviceType, out service))
				{
					return false;
				}
			}

			if (service is Delegate factory)
			{
				service = factory.DynamicInvoke();
			}

			return true;
		}
	}
}