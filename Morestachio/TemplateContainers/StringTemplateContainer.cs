namespace Morestachio.TemplateContainers;

/// <summary>
///		Defines a string based template container
/// </summary>
public class StringTemplateContainer : TemplateContainerBase
{
	/// <inheritdoc />
	public StringTemplateContainer(string template) : base(template)
	{
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="template"></param>
	public static implicit operator StringTemplateContainer(string template)
	{
		return new StringTemplateContainer(template);
	}
}