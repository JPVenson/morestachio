using System;

namespace Morestachio.Framework.Tokenizing;

/// <summary>
///		Defines an option declared inline with the keyword that is bound to a DocumentItem
/// </summary>
public interface ITokenOption : IEquatable<ITokenOption>
{
	/// <summary>
	///		The name of the Option
	/// </summary>
	string Name { get; }

	/// <summary>
	///		The value of the Option
	/// </summary>
	object Value { get; }

	/// <summary>
	///		Marks this Option as Persistent. It will be included in the DocumentItems list of TokenOptions.
	/// </summary>
	bool Persistent { get; }
}