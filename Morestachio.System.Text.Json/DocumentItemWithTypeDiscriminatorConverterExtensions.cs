using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Morestachio.Document.Contracts;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Parser;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.System.Text.Json;

public static class DocumentItemWithTypeDiscriminatorConverterExtensions
{
	public static JsonSerializerOptions AddMorestachioSerializationExtensions(this JsonSerializerOptions jsonSerializerOptions)
	{
		jsonSerializerOptions.Converters.Add(new ObjectWithTypeDiscriminatorFactory<IMorestachioExpression>(ExpressionSerializationHelper.ExpressionTypeLookup));
		jsonSerializerOptions.Converters.Add(new ObjectWithTypeDiscriminatorFactory<IMorestachioError>(ErrorSerializationHelper.ErrorTypeLookup));
		jsonSerializerOptions.Converters.Add(new ObjectWithTypeDiscriminatorFactory<IDocumentItem>(SerializationHelper.GetDocumentItemType, SerializationHelper.GetDocumentItemName));
		
		jsonSerializerOptions.Converters.Add(new SerializableConverterFactory());
		return jsonSerializerOptions;
	}
}