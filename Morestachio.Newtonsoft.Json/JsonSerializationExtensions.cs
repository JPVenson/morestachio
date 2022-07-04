using System;
using System.Collections.Generic;
using System.Text;
using Morestachio.Document.Contracts;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Parser;
using Morestachio.Parsing.ParserErrors;
using Newtonsoft.Json;

namespace Morestachio.Newtonsoft.Json;

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
	public static JsonSerializerSettings AddMorestachioSerializationExtensions(this JsonSerializerSettings jsonSerializerOptions)
	{
		jsonSerializerOptions.Converters.Add(new ObjectWithTypeDiscriminatorConverter<IMorestachioExpression>(ExpressionSerializationHelper.ExpressionTypeLookup));
		jsonSerializerOptions.Converters.Add(new ObjectWithTypeDiscriminatorConverter<IMorestachioError>(ErrorSerializationHelper.ErrorTypeLookup));
		jsonSerializerOptions.Converters.Add(new ObjectWithTypeDiscriminatorConverter<IDocumentItem>(SerializationHelper.GetDocumentItemType, SerializationHelper.GetDocumentItemName));
		
		return jsonSerializerOptions;
	}
}