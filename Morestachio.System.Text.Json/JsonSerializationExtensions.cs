using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Morestachio.Document.Contracts;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Parser;
using Morestachio.Helper.Serialization;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.System.Text.Json;

/// <summary>
///		Contains extensions for setup of System.Text.Json serialization of DocumentItems and Errors
/// </summary>
public static class JsonSerializationExtensions
{
	/// <summary>
	///		Adds support for Serialization of the Morestachio document tree.
	/// </summary>
	/// <param name="jsonSerializerOptions"></param>
	/// <returns></returns>
	public static JsonSerializerOptions AddMorestachioSerializationExtensions(this JsonSerializerOptions jsonSerializerOptions)
	{
		jsonSerializerOptions.Converters.Add(new ObjectWithTypeDiscriminatorFactory<IMorestachioExpression>(ExpressionSerializationHelper.ExpressionTypeLookup));
		jsonSerializerOptions.Converters.Add(new ObjectWithTypeDiscriminatorFactory<IMorestachioError>(ErrorSerializationHelper.ErrorTypeLookup));
		jsonSerializerOptions.Converters.Add(new ObjectWithTypeDiscriminatorFactory<IDocumentItem>(SerializationHelper.GetDocumentItemType, SerializationHelper.GetDocumentItemName));
		
		jsonSerializerOptions.Converters.Add(new SerializableConverterFactory());
		return jsonSerializerOptions;
	}
}