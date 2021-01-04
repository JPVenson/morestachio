namespace Morestachio.TemplateContainers
{
	public abstract class TemplateResource
	{
		public abstract int IndexOf(string token, int startIndex);
		public abstract string Substring(int index, int length);
		public abstract string Substring(int index);

		public abstract char this[int index]
		{
			get;
		}
		public abstract int Length();

		public static implicit operator TemplateResource(string template)
		{
			return new StringTemplateResource(template);
		}
	}
}