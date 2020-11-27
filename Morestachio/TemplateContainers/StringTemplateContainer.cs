namespace Morestachio.TemplateContainers
{
	/// <summary>
	///		Defines a string based template container
	/// </summary>
	public class StringTemplateContainer : TemplateContainerBase
	{
		/// <inheritdoc />
		public StringTemplateContainer(string template) : base(template)
		{
		}

		public static implicit operator StringTemplateContainer(string template)
		{
			return new StringTemplateContainer(template);
		}
	}
}