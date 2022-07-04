using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace Morestachio.Newtonsoft.Json
{
	public class ObjectWithTypeDiscriminatorConverter<TInterface> : JsonConverter<TInterface>
	{
		private readonly Func<string, Type> _typeLookup;
		private readonly Func<Type, string> _keyLookup;

		public ObjectWithTypeDiscriminatorConverter(IDictionary<string, Type> lookup)
		{
			_typeLookup = s => lookup[s];
			_keyLookup = s => lookup.First(e => e.Value == s).Key;
		}

		public ObjectWithTypeDiscriminatorConverter(Func<string, Type> getDocumentItemType, Func<Type, string> getDocumentItemName)
		{
			_typeLookup = getDocumentItemType;
			_keyLookup = getDocumentItemName;
		}

		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, TInterface value, JsonSerializer serializer)
		{
			WithTypeDiscriminatorHelper<TInterface>.Write(writer, value, serializer, _keyLookup);
		}

		/// <inheritdoc />
		public override TInterface ReadJson(
			JsonReader reader,
			Type objectType,
			TInterface existingValue,
			bool hasExistingValue,
			JsonSerializer serializer
		)
		{
			return WithTypeDiscriminatorHelper<TInterface>.Read(reader, objectType, serializer, _typeLookup);
		}
	}
}
