namespace Morestachio.Framework.Expression.Framework
{
	/// <summary>
	///		Defines a part that can be traversed by the <see cref="ContextObject"/>
	/// </summary>
	public readonly struct PathPart
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="pathType"></param>
		/// <param name="value"></param>
		public PathPart(PathType pathType, string value)
		{
			PathType = pathType;
			Value = value;
		}

		/// <summary>
		///		The type of path to traverse
		/// </summary>
		public PathType PathType { get; }

		/// <summary>
		///		In case of DataPath or Number the value
		/// </summary>
		public string Value { get; }
	}
}