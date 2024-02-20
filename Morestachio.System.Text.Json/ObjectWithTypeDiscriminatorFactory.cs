using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Morestachio.Document.Contracts;

namespace Morestachio.System.Text.Json
{
	/// <summary>
	///		Adds support for polymorphic serialization.
	/// </summary>
	/// <typeparam name="TInterface"></typeparam>
	public class ObjectWithTypeDiscriminatorFactory<TInterface> : JsonConverterFactory
	{
		private readonly Func<string, Type> _typeLookup;
		private readonly Func<Type, string> _keyLookup;

		/// <summary>
		///		Creates a new <see cref="ObjectWithTypeDiscriminatorConverter"/> that can serialize and deserialize from a static dictionary of types.
		/// </summary>
		/// <param name="lookup"></param>
		public ObjectWithTypeDiscriminatorFactory(IDictionary<string, Type> lookup)
		{
			_typeLookup = s => lookup[s];
			_keyLookup = s => lookup.First(e => e.Value == s).Key;
		}

		/// <summary>
		///		Creates a new <see cref="ObjectWithTypeDiscriminatorConverter"/> that can serialize and deserialize objects dynamically via the provided callbacks.
		/// </summary>
		/// <param name="typeLookup"></param>
		/// <param name="keyLookup"></param>
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
			public override TInterface Read(ref Utf8JsonReader reader,
											Type typeToConvert,
											JsonSerializerOptions options)
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
}