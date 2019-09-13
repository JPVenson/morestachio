using System;
using JetBrains.Annotations;

namespace Morestachio
{
	/// <summary>
	///		Delegate for the Event Handler in <see cref="ParserOptions.UnresolvedPath"/>
	/// </summary>
	/// <param name="path"></param>
	/// <param name="type"></param>
	public delegate void InvalidPath(string path, [CanBeNull]Type type);
}