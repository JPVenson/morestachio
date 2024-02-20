using System.Reflection;
using System.Xml;

namespace Morestachio.Framework.Expression.Parser;

/// <summary>
///		Defines header information about Expressions for Serialization
/// </summary>
public static class ExpressionSerializationHelper
{
	static ExpressionSerializationHelper()
	{
		ExpressionTypeLookup = new Dictionary<string, Type>();
		ExpressionTypeLookup["Expression"] = typeof(MorestachioExpression);
		ExpressionTypeLookup["ExpressionMultiPart"] = typeof(MorestachioMultiPartExpressionList);
		ExpressionTypeLookup["ExpressionArgList"] = typeof(MorestachioArgumentExpressionList);
		ExpressionTypeLookup["ExpressionString"] = typeof(MorestachioExpressionString);
		ExpressionTypeLookup["ExpressionNumber"] = typeof(MorestachioExpressionNumber);
		ExpressionTypeLookup["ExpressionOperator"] = typeof(MorestachioOperatorExpression);
		ExpressionTypeLookup["ExpressionBracket"] = typeof(MorestachioBracketExpression);
		ExpressionTypeLookup["ExpressionArgument"] = typeof(ExpressionArgument);
	}

	/// <summary>
	///		Mapping for type - serialized name
	/// </summary>
	public static IDictionary<string, Type> ExpressionTypeLookup;

	internal static IMorestachioExpression ParseExpressionFromKind(this XmlReader reader)
	{
		IMorestachioExpression exp = null;

		if (!ExpressionTypeLookup.TryGetValue(reader.Name, out var type))
		{
			throw new ArgumentOutOfRangeException(nameof(ExpressionParser.ExpressionKindNodeName));
		}

		var ctor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance,
			null, Type.EmptyTypes, null);

		if (ctor == null)
		{
			ctor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance,
				null, Type.EmptyTypes, null);
		}

		if (ctor == null)
		{
			throw new InvalidOperationException(
				"There is no ether public or private constructor that has no parameter for " + type.Name);
		}

		exp = ctor.Invoke(null) as IMorestachioExpression;
		exp.ReadXml(reader);

		return exp;
	}

	internal static void WriteExpressionToXml(this XmlWriter writer, IMorestachioExpression morestachioExpression)
	{
		var typeName = morestachioExpression.GetType();
		var type = ExpressionTypeLookup.First(e => e.Value == typeName);
		writer.WriteStartElement(type.Key);
		morestachioExpression.WriteXml(writer);
		writer.WriteEndElement();
	}
}