using System;
using System.Runtime.InteropServices;

namespace Morestachio.TemplateContainers;

/// <summary>
///		Defines a string resource
/// </summary>
public class StringTemplateResource : TemplateResource
{
	/// <summary>
	///		Creates a new String based resource
	/// </summary>
	/// <param name="template"></param>
	public StringTemplateResource(string template)
	{
		_template = template;
	}

	private readonly string _template;

	/// <inheritdoc />
	public override int IndexOf(string token, int startIndex)
	{
		return _template.IndexOf(token, startIndex, StringComparison.Ordinal);
	}

	/// <inheritdoc />
	public override string Substring(int index, int length)
	{
		return _template.Substring(index, length);
	}

	/// <inheritdoc />
	public override string Substring(int index)
	{
		return _template.Substring(index);
	}

	/// <inheritdoc />
	public override char this[int index]
	{
		get { return _template[index]; }
	}

	/// <inheritdoc />
	public override int Length()
	{
		return _template.Length;
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return _template.ToString();
	}
}