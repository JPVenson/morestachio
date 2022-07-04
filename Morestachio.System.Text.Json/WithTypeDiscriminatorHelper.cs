using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Morestachio.Helper.Serialization;

namespace Morestachio.System.Text.Json;

/// <summary>
///		Helper methods for Reading and Writing objects with a Type Discriminator
/// </summary>
/// <typeparam name="TObject"></typeparam>
public static class WithTypeDiscriminatorHelper<TObject>
{
	public const string TypePropertyName = "$type";

	/// <summary>
	///		Checks for inheritance
	/// </summary>
	/// <param name="typeToConvert"></param>
	/// <returns></returns>
	public static bool CanConvert(Type typeToConvert)
	{
		return typeof(TObject).IsAssignableFrom(typeToConvert);
	}

	/// <summary>
	///		Constructs a <see cref="SerializationInfo"/> from the current json object and will create a new type based on the Type Discriminator
	/// </summary>
	/// <param name="reader"></param>
	/// <param name="typeToConvert"></param>
	/// <param name="options"></param>
	/// <param name="produceAbsoluteType"></param>
	/// <returns></returns>
	/// <exception cref="JsonException"></exception>
	public static TObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options, Func<string, Type> produceAbsoluteType)
	{
		var serializationInfo = GetSerializationInfoFromJson(ref reader, options);
		var typeName = serializationInfo.serializationInfo.GetString(TypePropertyName);

		if (string.IsNullOrWhiteSpace(typeName))
		{
			throw new JsonException($"Expected to read the property '{TypePropertyName}' but got null.");
		}

		var documentType = produceAbsoluteType(typeName);
		serializationInfo.serializationInfo.SetType(documentType);

		return ConstructFromSerializationInfo(serializationInfo, documentType);
	}

	/// <summary>
	///		Creates a new <see cref="TObject"/> from the <see cref="SerializationInfo"/>
	/// </summary>
	/// <param name="serializationInfo"></param>
	/// <param name="documentType"></param>
	/// <returns></returns>
	public static TObject ConstructFromSerializationInfo((SerializationInfo serializationInfo, StreamingContext streamingContext) serializationInfo, Type documentType)
	{
		var parameter = new object[]
		{
			serializationInfo.serializationInfo,
			serializationInfo.streamingContext
		};

		var ctor = documentType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance,
				null,
				new[] { typeof(SerializationInfo), typeof(StreamingContext) },
				null) ??
			documentType.GetConstructor(BindingFlags.Public | BindingFlags.Instance,
				null,
				new[] { typeof(SerializationInfo), typeof(StreamingContext) },
				null);

		if (ctor == null)
		{
			throw new JsonException($"To deserialize {documentType} the object must have a public or protected constructor that takes an {typeof(SerializationInfo)} and {typeof(StreamingContext)}");
		}

		return (TObject)ctor.Invoke(parameter);
	}

	/// <summary>
	///		Reads the current object in <see cref="Utf8JsonReader"/> and produces a <see cref="SerializationInfo"/> from its properties
	/// </summary>
	/// <param name="reader"></param>
	/// <param name="options"></param>
	/// <returns></returns>
	/// <exception cref="JsonException"></exception>
	public static (SerializationInfo serializationInfo, StreamingContext streamingContext) GetSerializationInfoFromJson(ref Utf8JsonReader reader, JsonSerializerOptions options)
	{
		var jsonTypeFormatter = new JsonTypeFormatter(options);
		var serializationInfo = new SerializationInfo(typeof(object), jsonTypeFormatter);

		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.EndObject)
			{
				break;
			}

			if (reader.TokenType != JsonTokenType.PropertyName)
			{
				throw new JsonException($"Expected to read a property but got '{reader.TokenType}' instead.");
			}

			var name = reader.GetString();

			if (name == null)
			{
				throw new JsonException("Expected to read a property name but got null");
			}
			reader.Read();
			var value = JsonElement.ParseValue(ref reader);
			serializationInfo.AddValue(name, value);
		}

		return (serializationInfo, new StreamingContext(StreamingContextStates.All, new MorestachioSerializationContext(jsonTypeFormatter)));
	}

	/// <summary>
	///		Writes a object into the <see cref="Utf8JsonWriter"/>
	/// </summary>
	/// <param name="writer"></param>
	/// <param name="value"></param>
	/// <param name="options"></param>
	/// <param name="produceTypeDiscriminator"></param>
	public static void Write(Utf8JsonWriter writer,
							TObject value,
							JsonSerializerOptions options,
							Func<Type, string> produceTypeDiscriminator)
	{
		writer.WriteStartObject();

		if (produceTypeDiscriminator != null)
		{
			writer.WriteString(TypePropertyName, produceTypeDiscriminator.Invoke(value.GetType()));
		}

		WriteISerializableJson(writer, value, options);
		writer.WriteEndObject();
	}

	private static void WriteISerializableJson(Utf8JsonWriter writer, TObject value, JsonSerializerOptions options)
	{
		if (value is not ISerializable serializable)
		{
			throw new JsonException($"Expected type '{typeof(TObject)}' to implement '{typeof(ISerializable)}'");
		}
		var serializationInfo = new SerializationInfo(value.GetType(), new FormatterConverter());
		serializable.GetObjectData(serializationInfo, new StreamingContext());
		var values = serializationInfo.GetEnumerator();

		while (values.MoveNext())
		{
			var current = values.Current;

			if (current.Value is null || current.Value is IList { Count: 0 } or Array { Length: 0 } 
				&& options.DefaultIgnoreCondition is not JsonIgnoreCondition.Never)
			{
				continue;
			}

			writer.WritePropertyName(current.Name);
			JsonSerializer.SerializeToElement(current.Value, current.ObjectType, options).WriteTo(writer);
		}
	}
}
