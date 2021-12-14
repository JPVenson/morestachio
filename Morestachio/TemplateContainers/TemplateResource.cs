namespace Morestachio.TemplateContainers;

/// <summary>
///		Defines a resource that can be parsed
/// </summary>
public abstract class TemplateResource
{
	/// <summary>
	///		Returns the index of a substring
	/// </summary>
	/// <param name="token">The search string</param>
	/// <param name="startIndex">the position from where to start the search</param>
	/// <returns></returns>
	public abstract int IndexOf(string token, int startIndex);

	/// <summary>
	///		Returns a substring at the <see cref="index"/> with <see cref="length"/>
	/// </summary>
	/// <param name="index"></param>
	/// <param name="length"></param>
	/// <returns></returns>
	public abstract string Substring(int index, int length);

	/// <summary>
	///		Returns all content after <see cref="index"/>
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	public abstract string Substring(int index);

	/// <summary>
	///		Returns a single character at <see cref="index"/>
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	public abstract char this[int index]
	{
		get;
	}

	/// <summary>
	///		Returns the whole length of the resource in characters
	/// </summary>
	/// <returns></returns>
	public abstract int Length();

	/// <summary>
	/// 
	/// </summary>
	/// <param name="template"></param>
	public static implicit operator TemplateResource(string template)
	{
		return new StringTemplateResource(template);
	}
}