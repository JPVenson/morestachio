using System;

namespace Morestachio.Framework.Expression.Framework
{
	/// <summary>
	///		Types for specifying the level of isolation
	/// </summary>
	[Flags]
	public enum IsolationOptions
	{
		/// <summary>
		///		No Isolation
		/// </summary>
		None = 1 << 0,
		/// <summary>
		///		Variables cannot be overwritten
		/// </summary>
		VariableIsolation = 1 << 1,
	}
}