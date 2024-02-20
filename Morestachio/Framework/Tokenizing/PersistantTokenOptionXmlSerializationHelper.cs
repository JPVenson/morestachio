using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Parser;

namespace Morestachio.Framework.Tokenizing;

internal static class PersistantTokenOptionXmlSerializationHelper
{
	public static void WriteOptions(this XmlWriter writer, IEnumerable<ITokenOption> options, string elementName)
	{
		var tagOptions = options?.OfType<PersistantTokenOption>().ToArray();

		if (tagOptions != null && tagOptions.Length > 0)
		{
			writer.WriteStartElement(elementName);

			foreach (var tokenOption in tagOptions)
			{
				writer.WriteXml(tokenOption);
			}

			writer.WriteEndElement(); //elementName
		}
	}

	public static IEnumerable<ITokenOption> ReadOptions(this XmlReader reader, string elementName)
	{
		if (reader.NodeType == XmlNodeType.Element && reader.Name.Equals(elementName))
		{
			reader.ReadStartElement(); //elementName
			var options = new List<ITokenOption>();

			while (reader.NodeType == XmlNodeType.Element && reader.Name.Equals("Option"))
			{
				options.Add(reader.ReadXmlOption());
			}

			reader.ReadEndElement(); //elementName
			return options.ToArray();
		}

		return null;
	}

	public static PersistantTokenOption ReadXmlOption(this XmlReader reader)
	{
		var name = reader.GetAttribute("Name");
		var type = reader.GetAttribute("Type");
		var value = reader.GetAttribute("Value");
		reader.ReadStartElement();

		switch (type)
		{
			case "x:string":
				return new PersistantTokenOption(name, value);
			case "x:boolean":
				return new PersistantTokenOption(name, value == bool.TrueString);
			case "x:int32":
				return new PersistantTokenOption(name, int.Parse(value));
			case "x:expression":
				reader.ReadStartElement();
				var expression = reader.ParseExpressionFromKind();
				return new PersistantTokenOption(name, expression);
			default:
				throw new InvalidOperationException(
					$"Cannot deserialize the token option with the name '{name}' and value(string) '{value}' to the type '{type}' as there is no conversion known");
		}
	}

	public static void WriteXml(this XmlWriter writer, PersistantTokenOption token)
	{
		writer.WriteStartElement("Option");
		writer.WriteAttributeString("Name", token.Name);

		if (token.Value != null)
		{
			if (token.Value is string)
			{
				writer.WriteAttributeString("Type", "x:string");
				writer.WriteAttributeString("Value", token.Value.ToString());
			}
			else if (token.Value is bool)
			{
				writer.WriteAttributeString("Type", "x:boolean");
				writer.WriteAttributeString("Value", token.Value.ToString());
			}
			else if (token.Value is int i)
			{
				writer.WriteAttributeString("Type", "x:int32");
				writer.WriteAttributeString("Value", i.ToString());
			}
			else if (token.Value is Enum)
			{
				writer.WriteAttributeString("Type", "x:int32");
				writer.WriteAttributeString("Value", ((int)token.Value).ToString());
			}
			else if (token.Value is IMorestachioExpression expression)
			{
				writer.WriteAttributeString("Type", "x:expression");
				writer.WriteExpressionToXml(expression);
			}
			else
			{
				throw new InvalidOperationException(
					$"Cannot serialize the object of value: {token.Value}:{token.Value?.GetType()} for the TokenOption: {token.Name}");
			}
		}

		writer.WriteEndElement(); //Option
	}
}