using System;
using System.Collections.Generic;

namespace Morestachio.Formatter.Framework
{
	/// <summary>
	///		Collection of services
	/// </summary>
	public class ServiceCollection
	{
		private readonly IDictionary<Type, object> _localSource;
		private readonly IDictionary<Type, object> _source;

		/// <summary>
		/// 
		/// </summary>
		public ServiceCollection(IDictionary<Type, object> source)
		{
			_source = source;
			_localSource = new Dictionary<Type, object>
			{
				{typeof(ServiceCollection), this}
			};
		}

		/// <summary>
		///		Adds an service using an interface and Implementation
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TE"></typeparam>
		/// <param name="service"></param>
		public void AddService<T, TE>(TE service) where TE : T
		{
			_localSource[typeof(T)] = service;
		}

		/// <summary>
		///		Adds an Service
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="service"></param>
		public void AddService<T>(T service)
		{
			_localSource[typeof(T)] = service;
		}

		/// <summary>
		///		Adds an service using an interface and factory
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TE"></typeparam>
		/// <param name="serviceFactory"></param>
		public void AddService<T, TE>(Func<TE> serviceFactory) where TE : T
		{
			_localSource[typeof(T)] = serviceFactory;
		}

		/// <summary>
		///		Adds an service factory
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="serviceFactory"></param>
		public void AddService<T>(Func<T> serviceFactory)
		{
			_localSource[typeof(T)] = serviceFactory;
		}

		/// <summary>
		///		Gets the service if present
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="service"></param>
		/// <returns></returns>
		public bool TryGetService<T>(out T service)
		{
			var found = TryGetService(typeof(T), out var serviceTem);
			if (found)
			{
				service = (T)serviceTem;
			}
			else
			{
				service = default;
			}

			return found;
		}

		/// <summary>
		///		Gets the service if present or null
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T TryGetService<T>() where T : class
		{
			var found = TryGetService(typeof(T), out var serviceTem);
			return serviceTem as T;
		}

		/// <summary>
		///		Gets the service or throws an exception
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T GetRequiredService<T>()
		{
			var found = TryGetService(typeof(T), out var serviceTem);
			if (found)
			{
				return (T)serviceTem;
			}
			throw new InvalidOperationException($"The required service {typeof(T)} was not found");
		}

		/// <summary>
		///		Gets an service
		/// </summary>
		/// <param name="serviceType"></param>
		/// <param name="service"></param>
		/// <returns></returns>
		public bool TryGetService(Type serviceType, out object service)
		{
			if (TryInvokeService(_localSource, serviceType, out service))
			{
				return true;
			}
			return TryInvokeService(_source, serviceType, out service);
		}

		/// <summary>
		///		Searches in the list of services and if necessary executes the factory
		/// </summary>
		/// <param name="services"></param>
		/// <param name="serviceType"></param>
		/// <param name="service"></param>
		/// <returns></returns>
		public static bool TryInvokeService(IDictionary<Type, object> services, Type serviceType, out object service)
		{
			if (!services.TryGetValue(serviceType, out service))
			{
				return false;
			}

			if (service is Delegate factory)
			{
				service = factory.DynamicInvoke();
			}

			return true;
		}
	}
}