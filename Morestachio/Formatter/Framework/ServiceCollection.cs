using System.ComponentModel.Design;

namespace Morestachio.Formatter.Framework;

/// <summary>
///     Collection of services
/// </summary>
public class ServiceCollection : IServiceContainer
{
	private readonly IDictionary<Type, object> _localSource;
	private readonly ServiceCollection _parentProvider;

	/// <summary>
	///     Creates a new Top level Service collection
	/// </summary>
	public ServiceCollection() : this(null)
	{
	}

	/// <summary>
	/// </summary>
	public ServiceCollection(ServiceCollection parentProvider)
	{
		_parentProvider = parentProvider;

		_localSource = new Dictionary<Type, object>
		{
			{ typeof(ServiceCollection), this },
			{ typeof(IServiceContainer), this }
		};
	}

	/// <inheritdoc />
	public object GetService(Type serviceType)
	{
		TryGetService(serviceType, out var service);
		return service;
	}

	/// <inheritdoc />
	public void AddService(Type serviceType, ServiceCreatorCallback callback)
	{
		AddService(serviceType, callback, false);
	}

	/// <inheritdoc />
	public void AddService(Type serviceType, ServiceCreatorCallback callback, bool promote)
	{
		AddService(serviceType, (object)callback, false);
	}

	/// <inheritdoc />
	public void AddService(Type serviceType, object serviceInstance)
	{
		AddService(serviceType, serviceInstance, false);
	}

	/// <inheritdoc />
	public void AddService(Type serviceType, object serviceInstance, bool promote)
	{
		_localSource[serviceType] = serviceInstance;
	}

	/// <inheritdoc />
	public void RemoveService(Type serviceType)
	{
		_localSource.Remove(serviceType);
	}

	/// <inheritdoc />
	public void RemoveService(Type serviceType, bool promote)
	{
		_localSource.Remove(serviceType);
	}


	/// <summary>
	///     Enumerates all services known
	/// </summary>
	/// <returns></returns>
	public IDictionary<Type, object> Enumerate()
	{
		var services = _parentProvider?.Enumerate() ?? new Dictionary<Type, object>();

		foreach (var item in _localSource)
		{
			services[item.Key] = item.Value;
		}

		return services;
	}

	/// <summary>
	///     Creates a new Service collection with this collection as its parent
	/// </summary>
	/// <returns></returns>
	public ServiceCollection CreateChild()
	{
		return new ServiceCollection(this);
	}

	/// <summary>
	///     Adds an service using an interface and Implementation
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="TE"></typeparam>
	/// <param name="service"></param>
	public void AddService<T, TE>(TE service) where TE : T
	{
		AddService(typeof(T), service);
	}

	/// <summary>
	///     Adds an Service
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="service"></param>
	public void AddService<T>(T service)
	{
		AddService(typeof(T), service);
	}

	/// <summary>
	///     Adds an service using an interface and factory
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="TE"></typeparam>
	/// <param name="serviceFactory"></param>
	public void AddService<T, TE>(Func<TE> serviceFactory) where TE : T
	{
		AddService(typeof(T), serviceFactory);
	}

	/// <summary>
	///     Adds an service factory
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="serviceFactory"></param>
	public void AddService<T>(Func<T> serviceFactory)
	{
		AddService(typeof(T), serviceFactory);
	}

	/// <summary>
	///     Gets the service if present
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
	///     Gets the service if present or null
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public T TryGetService<T>() where T : class
	{
		var found = TryGetService(typeof(T), out var serviceTem);
		return serviceTem as T;
	}

	/// <summary>
	///     Gets the service or throws an exception
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
	///     Gets an service
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

		if (_parentProvider == null)
		{
			return false;
		}

		service = _parentProvider.GetService(serviceType);

		if (service is Delegate factory)
		{
			service = factory.DynamicInvoke();
		}

		return service != null;
	}

	/// <summary>
	///     Searches in the list of services and if necessary executes the factory
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