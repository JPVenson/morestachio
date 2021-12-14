using System;
using System.Linq;

namespace Morestachio.Formatter.Framework;

/// <summary>
///		Used to compare and search within the list of all cached formatters
/// </summary>
public readonly struct FormatterCacheCompareKey : IEquatable<FormatterCacheCompareKey>
{
	/// <summary>
	/// 
	/// </summary>
	public FormatterCacheCompareKey(Type sourceType, FormatterArgumentType[] arguments, string name) : this()
	{
		SourceType = sourceType;
		Arguments = arguments;
		Name = name;
		_hashCode = GetHashCodeHelper();
	}

	/// <summary>
	///		The name of the formatter
	/// </summary>
	public string Name { get; }

	/// <summary>
	///		The source type
	/// </summary>
	public Type SourceType { get; }

	/// <summary>
	///		The argument types
	/// </summary>
	public FormatterArgumentType[] Arguments { get; }

	/// <inheritdoc />
	public bool Equals(FormatterCacheCompareKey other)
	{
		return Name == other.Name && SourceType == other.SourceType && Arguments.SequenceEqual(other.Arguments);
	}

	/// <inheritdoc />
	public override bool Equals(object obj)
	{
		return obj is FormatterCacheCompareKey other && Equals(other);
	}

	private readonly int _hashCode;

	/// <inheritdoc />
	public override int GetHashCode()
	{
		return _hashCode;
	}

	private int GetHashCodeHelper()
	{
		unchecked
		{
			var hashCode = (Name != null ? Name.GetHashCode() : 0);
			hashCode = (hashCode * 397) ^ (SourceType != null ? SourceType.GetHashCode() : 0);
			hashCode = (hashCode * 397) ^
				(Arguments != null && Arguments.Length > 0 ? Arguments.Select(f => f.GetHashCode()).Aggregate((e, f) => e ^ f) : 0);
			return hashCode;
		}
	}
}