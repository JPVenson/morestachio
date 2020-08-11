using JetBrains.Annotations;

namespace Morestachio
{
	/// <summary>
	///		Delegate for the Event Handler in <see cref="ParserOptions.UnresolvedPath"/>
	/// </summary>
	public delegate void InvalidPath(InvalidPathEventArgs args);
}