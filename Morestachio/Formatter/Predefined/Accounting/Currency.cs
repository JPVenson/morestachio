using System;
using System.ComponentModel;

namespace Morestachio.Formatter.Predefined.Accounting;

/// <summary>
///		Contains the ISO4217 currency name and its symbol
/// </summary>
public readonly struct Currency : IFormattable, IComparable<Currency>, IEquatable<Currency>
{
	/// <summary>
	///		Creates a new Currency
	/// </summary>
	/// <param name="displayValue"></param>
	/// <param name="isoName"></param>
	public Currency(string displayValue, string isoName)
	{
		DisplayValue = displayValue ??
			throw new ArgumentException("The display name of a currency cannot be null", nameof(displayValue));
		IsoName = isoName ??
			throw new ArgumentException("The ISO 4217 name of a currency cannot be null", nameof(isoName));
	}

	/// <summary>
	///		An currency that can be used if the real currency type is unknown
	/// </summary>
	/// <remarks>Uses the ¤ (U+00A4) sign to display its values as set in ISO8859</remarks>
	[Description("An currency that can be used if the real currency type is unknown")]
	public static readonly Currency UnknownCurrency = new Currency("¤", "¤¤");

	/// <summary>
	///		The Symbol that represents the current currency
	/// </summary>
	[Description("The Symbol that represents the current currency")]
	public string DisplayValue { get; }

	/// <summary>
	///		The ISO4217 currency name
	/// </summary>
	[Description("The ISO4217 currency name")]
	public string IsoName { get; }

	/// <inheritdoc />
	public override string ToString()
	{
		return IsoName;
	}

	/// <inheritdoc />
	public string ToString(string format, IFormatProvider formatProvider)
	{
		//TODO change currency format to allow culture specific display
		return format + DisplayValue;
	}

	/// <inheritdoc />
	public int CompareTo(Currency other)
	{
		return string.Compare(IsoName, other.IsoName, StringComparison.Ordinal);
	}

	/// <inheritdoc />
	public bool Equals(Currency other)
	{
		return IsoName == other.IsoName;
	}

	/// <inheritdoc />
	public override bool Equals(object obj)
	{
		return obj is Currency other && Equals(other);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		return (IsoName != null ? IsoName.GetHashCode() : 0);
	}
}