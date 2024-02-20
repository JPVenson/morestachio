using System.Text;
using System.Threading;
using Morestachio.Util;

namespace Morestachio.Framework.IO.SingleStream;

/// <summary>
///		Uses are shared StringBuilder if available
/// </summary>
public class SharedByteCounterStringBuilder : ByteCounterStringBuilder
{
	private static StringBuilder _cachedInstance;

	private static StringBuilder Acquire(int estimatedSize)
	{
		if (_cachedInstance == null)
		{
			return new StringBuilder(estimatedSize);
		}

		_cachedInstance.Clear();
		return _cachedInstance;
	}

	private static string GetStringAndRelease(StringBuilder builder)
	{
		_cachedInstance = builder;
		return builder.ToString();
	}

	/// <inheritdoc />
	public SharedByteCounterStringBuilder(ParserOptions options) : base(Acquire(2024), options)
	{
	}

	/// <inheritdoc />
	public override string ToString()
	{
		if (StringBuilder == null)
		{
			throw new NotSupportedException(
				"This Shared Builder can only be enumerated once. you have to store the text yourself.");
		}

		var builder = StringBuilder;
		StringBuilder = null;
		return GetStringAndRelease(builder);
	}
}