namespace Morestachio.Formatter.Framework;

/// <summary>
///     Allows to change the name this service is available from template
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false)]
public sealed class ServiceNameAttribute : Attribute
{
	// See the attribute guidelines at 
	//  http://go.microsoft.com/fwlink/?LinkId=85236
	public ServiceNameAttribute(string name)
	{
		Name = name;
	}

	/// <summary>
	///     The name of this Service
	/// </summary>
	public string Name { get; }
}