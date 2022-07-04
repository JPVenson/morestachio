using System;
using System.Collections.Concurrent;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Morestachio.System.Text.Json;

/// <summary>
///		Serialization for <see cref="ISerializable"/>
/// </summary>
public class SerializableConverterFactory : JsonConverterFactory
{
	/// <summary>
	///		The shared Instance.
	/// </summary>
	public static readonly JsonConverterFactory Instance = new SerializableConverterFactory();
	private static readonly ConcurrentDictionary<Type, JsonConverter> _cache = new();
	/// <inheritdoc />
	public override bool CanConvert(Type typeToConvert)
	{
		return !typeToConvert.IsInterface && typeof(ISerializable).IsAssignableFrom(typeToConvert);
	}

	/// <inheritdoc />
	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		if (!_cache.TryGetValue(typeToConvert, out var converter))
		{
			converter = Activator.CreateInstance(typeof(SerializableConverter<>)
				.MakeGenericType(typeToConvert)) as JsonConverter;
			_cache[typeToConvert] = converter;
		}

		return converter;
	}

	private class SerializableConverter<TSerializable> : JsonConverter<TSerializable> where TSerializable : ISerializable
	{
		/// <inheritdoc />
		public override bool CanConvert(Type typeToConvert)
		{
			return !typeToConvert.IsInterface 
				&& typeof(ISerializable).IsAssignableFrom(typeToConvert);
		}

		/// <inheritdoc />
		public override TSerializable Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var serializationInfo = WithTypeDiscriminatorHelper<TSerializable>.GetSerializationInfoFromJson(ref reader, options);
			serializationInfo.SetType(typeToConvert);
			return WithTypeDiscriminatorHelper<TSerializable>.ConstructFromSerializationInfo(serializationInfo, typeToConvert);
		}

		/// <inheritdoc />
		public override void Write(Utf8JsonWriter writer, TSerializable value, JsonSerializerOptions options)
		{
			WithTypeDiscriminatorHelper<TSerializable>.Write(writer, value, options, null);
		}
	}
}