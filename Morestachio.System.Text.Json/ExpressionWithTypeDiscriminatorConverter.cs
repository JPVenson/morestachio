using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Morestachio.Document.Contracts;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Parser;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.System.Text.Json;


/// <summary>
///		Defines a converter that will add a <see cref="WithTypeDiscriminatorHelper{TObject}.TypePropertyName"/> to serialize the <see cref="IMorestachioExpression"/>
/// </summary>
public class ExpressionWithTypeDiscriminatorConverter 
	: JsonConverter<IMorestachioExpression>
{
	/// <summary>
	///		The shared converter instance
	/// </summary>
	public static readonly JsonConverter<IMorestachioExpression> Instance = new ExpressionWithTypeDiscriminatorConverter();

	private static Type ProduceAbsoluteType(string typeDiscriminator)
	{
		return ExpressionSerializationHelper.ExpressionTypeLookup[typeDiscriminator];
	}

	private string ProduceTypeDiscriminator(Type arg)
	{
		return ExpressionSerializationHelper.ExpressionTypeLookup.First(e => e.Value == arg).Key;
	}

	/// <inheritdoc />
	public override bool CanConvert(Type typeToConvert)
	{
		return typeof(IMorestachioExpression).IsAssignableFrom(typeToConvert);
	}

	/// <inheritdoc />
	public override IMorestachioExpression Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return WithTypeDiscriminatorHelper<IMorestachioExpression>.Read(ref reader, typeToConvert, options, ProduceAbsoluteType);
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, IMorestachioExpression value, JsonSerializerOptions options)
	{
		WithTypeDiscriminatorHelper<IMorestachioExpression>.Write(writer, value, options, ProduceTypeDiscriminator);
	}
}