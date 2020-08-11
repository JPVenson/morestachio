namespace Morestachio
{
	/// <summary>
	///		Defines the behavior how to react to a call of a formatter that does not exist
	/// </summary>
	public enum UnmatchedFormatterBehavior
	{
		/// <summary>
		///		If the Option is set to null and no formatter that matches is found the result of that operation is null
		/// </summary>
		Null,

		/// <summary>
		///		If the option is set to ParentValue and no formatter that matches is found the result of that operation is the last non-null value
		/// </summary>
		ParentValue
	}
}