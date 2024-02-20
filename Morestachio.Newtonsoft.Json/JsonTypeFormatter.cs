using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Morestachio.Newtonsoft.Json;

/// <summary>
///		Can convert <see cref="JToken"/> to the requested types
/// </summary>
public class JsonTypeFormatter : IFormatterConverter
{
	private readonly JsonSerializer _serializer;

	public JsonTypeFormatter(JsonSerializer serializer)
	{
		_serializer = serializer;
	}

	/// <inheritdoc />
	public object Convert(object value, Type type)
	{
		if (value is JToken token)
		{
			if (token.Type is JTokenType.Null)
			{
				return null;
			}

			return token.ToObject(type, _serializer);
		}

		throw new JsonException($"Could not convert {value} to {type}");
	}

	/// <inheritdoc />
	public object Convert(object value, TypeCode typeCode)
	{
		throw new JsonException($"Could not convert {value} to {typeCode}");
	}

	/// <inheritdoc />
	public bool ToBoolean(object value)
	{
		if (value is JValue token)
		{
			return token.Value<bool>();
		}

		throw new JsonException($"Could not convert {value} to {typeof(bool)}");
	}

	/// <inheritdoc />
	public byte ToByte(object value)
	{
		if (value is JValue token)
		{
			return token.Value<byte>();
		}

		throw new JsonException($"Could not convert {value} to {typeof(byte)}");
	}

	/// <inheritdoc />
	public char ToChar(object value)
	{
		if (value is JValue token)
		{
			return token.Value<char>();
		}

		throw new JsonException($"Could not convert {value} to {typeof(char)}");
	}

	/// <inheritdoc />
	public DateTime ToDateTime(object value)
	{
		if (value is JValue token)
		{
			return token.Value<DateTime>();
		}

		throw new JsonException($"Could not convert {value} to {typeof(DateTime)}");
	}

	/// <inheritdoc />
	public decimal ToDecimal(object value)
	{
		if (value is JValue token)
		{
			return token.Value<decimal>();
		}

		throw new JsonException($"Could not convert {value} to {typeof(decimal)}");
	}

	/// <inheritdoc />
	public double ToDouble(object value)
	{
		if (value is JValue token)
		{
			return token.Value<double>();
		}

		throw new JsonException($"Could not convert {value} to {typeof(double)}");
	}

	/// <inheritdoc />
	public short ToInt16(object value)
	{
		if (value is JValue token)
		{
			return token.Value<short>();
		}

		throw new JsonException($"Could not convert {value} to {typeof(short)}");
	}

	/// <inheritdoc />
	public int ToInt32(object value)
	{
		if (value is JValue token)
		{
			return token.Value<int>();
		}

		throw new JsonException($"Could not convert {value} to {typeof(int)}");
	}

	/// <inheritdoc />
	public long ToInt64(object value)
	{
		if (value is JValue token)
		{
			return token.Value<long>();
		}

		throw new JsonException($"Could not convert {value} to {typeof(long)}");
	}

	/// <inheritdoc />
	public sbyte ToSByte(object value)
	{
		if (value is JValue token)
		{
			return token.Value<sbyte>();
		}

		throw new JsonException($"Could not convert {value} to {typeof(sbyte)}");
	}

	/// <inheritdoc />
	public float ToSingle(object value)
	{
		if (value is JValue token)
		{
			return token.Value<float>();
		}

		throw new JsonException($"Could not convert {value} to {typeof(float)}");
	}

	/// <inheritdoc />
	public string ToString(object value)
	{
		if (value is JValue token)
		{
			return token.Value<string>();
		}

		throw new JsonException($"Could not convert {value} to {typeof(string)}");
	}

	/// <inheritdoc />
	public ushort ToUInt16(object value)
	{
		if (value is JValue token)
		{
			return token.Value<ushort>();
		}

		throw new JsonException($"Could not convert {value} to {typeof(ushort)}");
	}

	/// <inheritdoc />
	public uint ToUInt32(object value)
	{
		if (value is JValue token)
		{
			return token.Value<uint>();
		}

		throw new JsonException($"Could not convert {value} to {typeof(uint)}");
	}

	/// <inheritdoc />
	public ulong ToUInt64(object value)
	{
		if (value is JValue token)
		{
			return token.Value<ulong>();
		}

		throw new JsonException($"Could not convert {value} to {typeof(ulong)}");
	}
}