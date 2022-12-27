using System.Diagnostics;

namespace Morestachio.Document.Contracts;

/// <summary>
///		Defines a single entry of usage.
/// </summary>
[DebuggerDisplay("{RenderPath()}")]
public record UsageDataItem
{
	/// <summary>
	///		Creates a new <see cref="UsageDataItem"/>
	/// </summary>
	/// <param name="Path"></param>
	/// <param name="Type"></param>
	/// <param name="Parent"></param>
	public UsageDataItem(string Path, UsageDataItemTypes Type, UsageDataItem Parent)
	{
		this.Parent = Parent;
		Dependents = new HashSet<UsageDataItem>();
		this.Path = Path;
		this.Type = Type;
	}

	/// <summary>
	///		All children that are referencing this Path.
	/// </summary>
	public HashSet<UsageDataItem> Dependents { get; set; }

	/// <summary>
	///		The Path that this part relies on.
	/// </summary>
	public UsageDataItem Parent { get; set; }

	/// <summary>
	///		The data path.
	/// </summary>
	public string Path { get; }

	/// <summary>
	///		The type of access.
	/// </summary>
	public UsageDataItemTypes Type { get; }

	/// <summary>
	///		Creates a new string representation of this and all parents.
	/// </summary>
	/// <returns></returns>
	public string RenderPath()
	{
		if (Parent is null)
		{
			return Render();
		}

		var renderPath = Parent.RenderPath();

		if (!string.IsNullOrWhiteSpace(renderPath))
		{
			return renderPath + "." + Render();
		}

		return Render();
	}

	/// <summary>
	///		Creates a new string representation of this part alone.
	/// </summary>
	/// <returns></returns>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public string Render()
	{
		switch (Type)
		{
			case UsageDataItemTypes.DataPath:
				return Path;
			case UsageDataItemTypes.ArrayAccess:
				return "[]";
			case UsageDataItemTypes.Service:
			case UsageDataItemTypes.Constant:
				return $"${Path}";
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	public void Deconstruct(out string Path, out UsageDataItemTypes Type, out UsageDataItem Parent)
	{
		Path = this.Path;
		Type = this.Type;
		Parent = this.Parent;
	}

	/// <summary>
	///		Adds a new Dependend part into this.
	/// </summary>
	/// <param name="dependent"></param>
	/// <returns></returns>
	public UsageDataItem AddDependent(UsageDataItem dependent)
	{
		var entry = Dependents.FirstOrDefault(e => e.Path == dependent.Path);
		if (entry is null)
		{
			Dependents.Add(dependent);
			return dependent;
		}
		else
		{
			return entry;
		}
	}
}