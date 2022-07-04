using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Morestachio.Document.Contracts;

namespace Morestachio.System.Text.Json
{
	public class ObjectWithTypeDiscriminatorFactory<TInterface> : JsonConverterFactory
	{
		private readonly Func<string, Type> _typeLookup;
		private readonly Func<Type, string> _keyLookup;
		public ObjectWithTypeDiscriminatorFactory(IDictionary<string, Type> lookup)
		{
			_typeLookup = s => lookup[s];
			_keyLookup = s => lookup.First(e => e.Value == s).Key;
		}

		public ObjectWithTypeDiscriminatorFactory(Func<string, Type> typeLookup, Func<Type, string> keyLookup)
		{
			_typeLookup = typeLookup;
			_keyLookup = keyLookup;
		}

		/// <inheritdoc />
		public override bool CanConvert(Type typeToConvert)
		{
			return typeof(TInterface).IsAssignableFrom(typeToConvert);
		}

		/// <inheritdoc />
		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			return new ObjectWithTypeDiscriminatorConverter(_typeLookup, _keyLookup);
		}

		/// <summary>
		///		Defines a converter that will add a <see cref="WithTypeDiscriminatorHelper{TObject}.TypePropertyName"/> to serialize the <see cref="IDocumentItem"/>
		/// </summary>
		public class ObjectWithTypeDiscriminatorConverter : JsonConverter<TInterface> 
		{
			private readonly Func<string, Type> _lookupType;
			private readonly Func<Type, string> _keyLookup;

			public ObjectWithTypeDiscriminatorConverter(Func<string, Type> lookupType, Func<Type, string> keyLookup)
			{
				_lookupType = lookupType;
				_keyLookup = keyLookup;
			}

			/// <inheritdoc />
			public override bool CanConvert(Type typeToConvert)
			{
				return WithTypeDiscriminatorHelper<IDocumentItem>.CanConvert(typeToConvert);
			}

			/// <inheritdoc />
			public override TInterface Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				return WithTypeDiscriminatorHelper<TInterface>.Read(ref reader, typeToConvert, options, _lookupType);
			}

			/// <inheritdoc />
			public override void Write(Utf8JsonWriter writer, TInterface value, JsonSerializerOptions options)
			{
				WithTypeDiscriminatorHelper<TInterface>.Write(writer, value, options, _keyLookup);
			}
		}
	}

	/// <summary>
	///		Defines a converter that will add a <see cref="WithTypeDiscriminatorHelper{TObject}.TypePropertyName"/> to serialize the <see cref="IDocumentItem"/>
	/// </summary>
	public class DocumentItemWithTypeDiscriminatorConverter : JsonConverter<IDocumentItem> 
	{
		/// <summary>
		///		The shared converter instance
		/// </summary>
		public static readonly JsonConverter<IDocumentItem> Instance = new DocumentItemWithTypeDiscriminatorConverter();
		
		private static Type ProduceAbsoluteType(string typeDiscriminator)
		{
			return SerializationHelper.GetDocumentItemType(typeDiscriminator);
		}

		/// <inheritdoc />
		public override bool CanConvert(Type typeToConvert)
		{
			return WithTypeDiscriminatorHelper<IDocumentItem>.CanConvert(typeToConvert);
		}

		/// <inheritdoc />
		public override IDocumentItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return WithTypeDiscriminatorHelper<IDocumentItem>.Read(ref reader, typeToConvert, options, ProduceAbsoluteType);
		}

		/// <inheritdoc />
		public override void Write(Utf8JsonWriter writer, IDocumentItem value, JsonSerializerOptions options)
		{
			WithTypeDiscriminatorHelper<IDocumentItem>.Write(writer, value, options, type => type.ToString());
		}
	}
}
