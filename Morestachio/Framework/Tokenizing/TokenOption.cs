namespace Morestachio.Framework.Tokenizing;

/// <summary>
///		Defines an option declared inline with the keyword
/// </summary>
public readonly struct TokenOption : ITokenOption
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="name"></param>
	/// <param name="value"></param>
	public TokenOption(string name, object value)
	{
		Name = name;
		Value = value;
		Persistent = false;
	}

	/// <summary>
	///		The name of the Option
	/// </summary>
	public string Name { get; }

	/// <summary>
	///		The value of the Option
	/// </summary>
	public object Value { get; }

	/// <summary>
	///		Marks this Option as Persistent. It will be included in the DocumentItems list of TokenOptions.
	/// </summary>
	public bool Persistent { get; }
		
	/// <inheritdoc />
	public bool Equals(ITokenOption other)
	{
		return Name == other.Name && Equals(Value, other.Value) && Persistent == other.Persistent;
	}
		
	/// <inheritdoc />
	public override bool Equals(object obj)
	{
		return obj is TokenOption other && Equals(other);
	}
		
	/// <inheritdoc />
	public override int GetHashCode()
	{
		unchecked
		{
			var hashCode = (Name != null ? Name.GetHashCode() : 0);
			hashCode = (hashCode * 397) ^ (Value != null ? Value.GetHashCode() : 0);
			hashCode = (hashCode * 397) ^ Persistent.GetHashCode();
			return hashCode;
		}
	}
}