using System;
using System.Runtime.Serialization;
using System.Text.Json;

namespace Morestachio.System.Text.Json;

/// <summary>
///		Can convert <see cref="JsonElement"/> to the requested types
/// </summary>
public class JsonTypeFormatter : IFormatterConverter
{
	private readonly JsonSerializerOptions _options;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="options"></param>
	public JsonTypeFormatter(JsonSerializerOptions options)
	{
		_options = options;
	}

	/// <inheritdoc />
	public object Convert(object value, Type type)
	{
		if (value is JsonElement element)
		{
			return element.Deserialize(type, _options);
		}

		return global::System.Convert.ChangeType(value, type);
	}

	/// <inheritdoc />
	public object Convert(object value, TypeCode typeCode)
	{
		throw new NotImplementedException();
		if (value is JsonElement element)
		{
			return element.ToString();
		}

		return global::System.Convert.ChangeType(value, typeCode);
	}

	/// <inheritdoc />
	public bool ToBoolean(object value)
	{			
		if (value is JsonElement element)
		{
			return element.GetBoolean();
		}
			
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	public byte ToByte(object value)
	{		
		if (value is JsonElement element)
		{
			return element.GetByte();
		}
			
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	public char ToChar(object value)
	{		
		if (value is JsonElement element)
		{
			return element.GetString()[0];
		}
			
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	public DateTime ToDateTime(object value)
	{		
		if (value is JsonElement element)
		{
			return element.GetDateTime();
		}
			
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	public decimal ToDecimal(object value)
	{		
		if (value is JsonElement element)
		{
			return element.GetDecimal();
		}
			
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	public double ToDouble(object value)
	{		
		if (value is JsonElement element)
		{
			return element.GetDouble();
		}
			
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	public short ToInt16(object value)
	{		
		if (value is JsonElement element)
		{
			return element.GetInt16();
		}
			
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	public int ToInt32(object value)
	{		
		if (value is JsonElement element)
		{
			return element.GetInt32();
		}
			
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	public long ToInt64(object value)
	{		
		if (value is JsonElement element)
		{
			return element.GetInt64();
		}
			
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	public sbyte ToSByte(object value)
	{		
		if (value is JsonElement element)
		{
			return element.GetSByte();
		}
			
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	public float ToSingle(object value)
	{		
		if (value is JsonElement element)
		{
			return element.GetSingle();
		}
			
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	public string ToString(object value)
	{		
		if (value is JsonElement element)
		{
			return element.GetString();
		}
			
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	public ushort ToUInt16(object value)
	{		
		if (value is JsonElement element)
		{
			return element.GetUInt16();
		}
			
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	public uint ToUInt32(object value)
	{		
		if (value is JsonElement element)
		{
			return element.GetUInt32();
		}
			
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	public ulong ToUInt64(object value)
	{		
		if (value is JsonElement element)
		{
			return element.GetUInt64();
		}
			
		throw new NotImplementedException();
	}
}