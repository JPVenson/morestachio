using Morestachio.Framework.Context;

namespace Morestachio.Framework.Expression.Framework;

/// <summary>
///		Defines the types the <see cref="ContextObject"/> can traverse
/// </summary>
public enum PathType
{
	/// <summary>
	///		Traverse the data structure down by the value of <see cref="PathPart.Value"/>
	/// </summary>
	DataPath,

	/// <summary>
	///		Traverse the structure up to the upper most object
	/// </summary>
	RootSelector,

	/// <summary>
	///		Traverse the structure up one level
	/// </summary>
	ParentSelector,

	/// <summary>
	///		Do nothing and get yourself
	/// </summary>
	SelfAssignment,

	/// <summary>
	///		Do nothing and get yourself
	/// </summary>
	ThisPath,

	/// <summary>
	///		Enumerate the current objects structure and output a list
	/// </summary>
	ObjectSelector,

	///// <summary>
	/////		Create a new Number based on the value of <see cref="PathPart.Value"/>
	///// </summary>
	//Number,

	/// <summary>
	///		Defines a null value
	/// </summary>
	Null,

	/// <summary>
	///		Defines an boolean that can ether be true or false
	/// </summary>
	Boolean
}