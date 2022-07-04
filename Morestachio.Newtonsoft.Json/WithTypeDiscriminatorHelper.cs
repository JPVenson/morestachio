using System;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Morestachio.Newtonsoft.Json;

public static class WithTypeDiscriminatorHelper<TObject>
{
	public const string TypePropertyName = "$type";

	
	/// <summary>
	///		Reads the current object in <see cref="JsonReader"/> and produces a <see cref="SerializationInfo"/> from its properties
	/// </summary>
	/// <returns></returns>
	/// <exception cref="JsonException"></exception>
	public static SerializationInfo GetSerializationInfoFromJson(JsonReader reader,
																JsonSerializer serializer)
	{
		var serializationInfo = new SerializationInfo(typeof(object), new JsonTypeFormatter(serializer));
			
		while (reader.Read())
		{
			if (reader.TokenType == JsonToken.EndObject)
			{
				break;
			}

			if (reader.TokenType != JsonToken.PropertyName)
			{
				throw new JsonException($"Expected to read a property but got '{reader.TokenType}' instead.");
			}

			var name = reader.Value?.ToString();
			
			if (name == null)
			{
				throw new JsonException("Expected to read a property name but got null");
			}
			reader.Read();
			var value = JToken.ReadFrom(reader);
			serializationInfo.AddValue(name, value);
		}

		return serializationInfo;
	}
	
	/// <summary>
	///		Constructs a <see cref="SerializationInfo"/> from the current json object and will create a new type based on the Type Discriminator
	/// </summary>
	/// <param name="reader"></param>
	/// <param name="typeToConvert"></param>
	/// <param name="serializer"></param>
	/// <param name="produceAbsoluteType"></param>
	/// <returns></returns>
	/// <exception cref="JsonException"></exception>
	public static TObject Read(JsonReader reader, Type typeToConvert, JsonSerializer serializer, Func<string, Type> produceAbsoluteType)
	{
		var serializationInfo = GetSerializationInfoFromJson(reader, serializer);
		var typeName = serializationInfo.GetString(TypePropertyName);

		if (string.IsNullOrWhiteSpace(typeName))
		{
			throw new JsonException($"Expected to read the property '{TypePropertyName}' but got null.");
		}

		var documentType = produceAbsoluteType(typeName);
		serializationInfo.SetType(documentType);

		return ConstructFromSerializationInfo(serializationInfo, documentType);
	}

	/// <summary>
	///		Creates a new <see cref="TObject"/> from the <see cref="SerializationInfo"/>
	/// </summary>
	/// <param name="serializationInfo"></param>
	/// <param name="documentType"></param>
	/// <returns></returns>
	public static TObject ConstructFromSerializationInfo(SerializationInfo serializationInfo, Type documentType)
	{
		var parameter = new object[]
		{
			serializationInfo,
			new StreamingContext()
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
	///		Writes a object into the <see cref="JsonWriter"/>
	/// </summary>
	/// <param name="writer"></param>
	/// <param name="value"></param>
	/// <param name="serializer"></param>
	/// <param name="produceTypeDiscriminator"></param>
	public static void Write(JsonWriter writer, 
							TObject value, 
							JsonSerializer serializer, 
							Func<Type, string> produceTypeDiscriminator)
	{
		writer.WriteStartObject();

		if (produceTypeDiscriminator != null)
		{
			writer.WritePropertyName(TypePropertyName);
			writer.WriteValue(produceTypeDiscriminator.Invoke(value.GetType()));
		}
		
		WriteISerializableJson(writer, value, serializer);
		writer.WriteEndObject();
	}

	private static void WriteISerializableJson(JsonWriter writer, TObject value, JsonSerializer serializer)
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
			writer.WritePropertyName(current.Name);
			serializer.Serialize(writer, current.Value, current.ObjectType);
		}
	}
}