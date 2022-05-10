namespace Morestachio.Formatter.Framework;

/// <summary>
///     Allows to change the name this service is available from template
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false)]
public sealed class ServiceNameAttribute : Attribute
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="name"></param>
	public ServiceNameAttribute(string name)
	{
		Name = name;
	}

	/// <summary>
	///     The name of this Service
	/// </summary>
	public string Name { get; }
}