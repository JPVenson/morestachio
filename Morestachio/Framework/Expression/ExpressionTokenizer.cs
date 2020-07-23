using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Morestachio.Framework.Expression.Framework;
using Morestachio.ParserErrors;

namespace Morestachio.Framework.Expression
{
	/// <summary>
	///		This class provides methods for parsing ether a String or an Expression
	/// </summary>
	public static class ExpressionTokenizer
	{
		internal const string ExpressionNodeName = "Expression";
		internal const string ExpressionKindNodeName = "ExpressionKind";

		internal static IMorestachioExpression ParseExpressionFromKind(this XmlReader reader)
		{
			IMorestachioExpression exp = null;
			switch (reader.GetAttribute(ExpressionKindNodeName))
			{
				case "Expression":
					exp = new MorestachioExpression();
					break;
				case "ExpressionList":
					exp = new MorestachioExpressionList();
					break;
				case "ExpressionString":
					exp = new MorestachioExpressionString();
					break;
				case "ExpressionNumber":
					exp = new ExpressionNumber();
					break;
				case "OperatorExpression":
					exp = new MorestachioOperatorExpression();
					break;
			}
			exp.ReadXml(reader);
			return exp;
		}

		internal static void WriteExpressionToXml(this XmlWriter writer, IMorestachioExpression morestachioExpression)
		{
			writer.WriteStartElement(ExpressionNodeName);
			switch (morestachioExpression)
			{
				case MorestachioExpression _:
					writer.WriteAttributeString(ExpressionKindNodeName, "Expression");
					break;
				case MorestachioExpressionList _:
					writer.WriteAttributeString(ExpressionKindNodeName, "ExpressionList");
					break;
				case MorestachioExpressionString _:
					writer.WriteAttributeString(ExpressionKindNodeName, "ExpressionString");
					break;
				case ExpressionNumber _:
					writer.WriteAttributeString(ExpressionKindNodeName, "ExpressionNumber");
					break;
				case MorestachioOperatorExpression _:
					writer.WriteAttributeString(ExpressionKindNodeName, "OperatorExpression");
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(morestachioExpression));
			}
			morestachioExpression.WriteXml(writer);
			writer.WriteEndElement();
		}

		internal static TokenPair TokenizeVariableAssignment(string tokenValue,
			TokenzierContext context)
		{
			var startOfExpression = context.CurrentLocation;
			var variableNameIndex = tokenValue.IndexOf("#var ", StringComparison.InvariantCultureIgnoreCase);
			if (variableNameIndex != 0)
			{
				variableNameIndex = tokenValue.IndexOf("#let ", StringComparison.InvariantCultureIgnoreCase);
				if (variableNameIndex != 0)
				{
					context.Errors.Add(new MorestachioSyntaxError(
						context.CurrentLocation.AddWindow(new CharacterSnippedLocation(0, 0, tokenValue)),
						"#var", "", "#var name", "Expected #var"));
					return null;
				}
			}

			tokenValue = tokenValue.Substring("#var ".Length);
			context.AdvanceLocation("#var ".Length);
			string variableName = null;
			int i = 0;
			var lengthToExpression = 0;
			for (; i < tokenValue.Length; i++)
			{
				var c = tokenValue[i];
				if (c == '=')
				{
					variableName = tokenValue.Substring(0, i).Trim(' ');
					break;
				}

				lengthToExpression++;

				if (!char.IsLetter(c) && c != ' ')
				{
					context.Errors.Add(new MorestachioSyntaxError(
						context
							.CurrentLocation
							.Offset(i)
							.AddWindow(new CharacterSnippedLocation(0, i, tokenValue)),
						"#var", "", "#var name", "Invalid character detected. Expected only spaces or letters."));
					return null;
				}
			}

			context.AdvanceLocation(lengthToExpression);
			if (variableName == null)
			{
				context.Errors.Add(new MorestachioSyntaxError(
					context
						.CurrentLocation
						.AddWindow(new CharacterSnippedLocation(0, "#var ".Length, tokenValue)),
					"#var", "", "#var name", "expected variable name"));
			}

			var expression = tokenValue.Substring(tokenValue.IndexOf('=')).Trim(' ', '=');
			if (string.IsNullOrEmpty(expression))
			{
				context.Errors.Add(new MorestachioSyntaxError(
					context.CurrentLocation
						.AddWindow(new CharacterSnippedLocation(0, "#var ".Length, tokenValue)),
					"#var", "", "#var name = ", "expected ether an path expression or an string value"));
				return null;
			}

			return new TokenPair(TokenType.VariableVar, variableName, startOfExpression)
			{
				MorestachioExpression = ParseExpression(expression, context)
			};
		}

		/// <summary>
		///		Parses the given text to ether an expression or an string
		/// </summary>
		/// <param name="expression"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		public static IMorestachioExpression ParseExpression(string expression,
			TokenzierContext context)
		{
			if (expression.Length == 0)
			{
				context.Errors.Add(new MorestachioSyntaxError(
					context.CurrentLocation.AddWindow(new CharacterSnippedLocation(0, 0, expression)),
					"", "", "", "expected ether an path expression or an string value"));

				return null;
			}

			if (Tokenizer.IsStringDelimiter(expression[0]))
			{
				//its a string constant
				if (!Tokenizer.IsStringDelimiter(expression[expression.Length - 1]))
				{
					context.Errors.Add(new MorestachioSyntaxError(
						context.CurrentLocation.AddWindow(new CharacterSnippedLocation(0, expression.Length, expression)),
						"", "", "" + expression[0], "expected " + expression[0]));
					return null;
				}

				return MorestachioExpressionString.ParseFrom(expression, 0, context, out _);
			}
			//else if (Tokenizer.IsNumberExpressionChar(expression[0]))
			//{
			//	//its a string constant
			//	if (!Tokenizer.IsStringDelimiter(expression[expression.Length - 1]))
			//	{
			//		context.Errors.Add(new MorestachioSyntaxError(
			//			context.CurrentLocation.AddWindow(new CharacterSnippedLocation(0, expression.Length, expression)),
			//			"", "", "" + expression[0], "expected " + expression[0]));
			//		return null;
			//	}

			//	return MorestachioExpressionString.ParseFrom(expression, 0, context, out _);
			//}
			else
			{
				return MorestachioExpression.ParseFrom(expression, context, out _);
			}
		}

		///  <summary>
		/// 		Parses the given text to ether an expression or an string
		///  </summary>
		///  <param name="expression"></param>
		///  <param name="context"></param>
		///  <param name="cultureInfo"></param>
		///  <returns></returns>
		public static IMorestachioExpression ParseExpression(string expression, out TokenzierContext context,
			CultureInfo cultureInfo = null)
		{
			//context = 
			//	new TokenzierContext(Tokenizer.NewlineFinder.Matches(expression).OfType<Match>().Select(k => k.Index)
			//	.ToArray());
			context = 
				new TokenzierContext(Tokenizer.FindNewLines(expression).ToArray(), cultureInfo ?? CultureInfo.CurrentCulture);
			context.SetLocation(0);
			return ParseExpression(expression,
				context);
		}
	}
}