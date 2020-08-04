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
	public static class ExpressionParser
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
				default:
					throw new ArgumentOutOfRangeException(nameof(ExpressionKindNodeName));
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

		internal static string StringifyVariableAssignmentType(TokenType type)
		{
			switch (type)
			{
				case TokenType.VariableLet:
					return "#let";
				case TokenType.VariableVar:
					return "#var";
				default:
					return "???";
			}
		}

		internal static TokenPair TokenizeVariableAssignment(
			string tokenValue,
			TokenzierContext context,
			TokenType type)
		{
			var startOfExpression = context.CurrentLocation;
			//if (type != TokenType.VariableLet && type != TokenType.VariableVar)
			//{
			//	context.Errors.Add(new MorestachioSyntaxError(
			//	   context.CurrentLocation.AddWindow(new CharacterSnippedLocation(0, 0, tokenValue)),
			//	   "#var", "", "#var name", "Expected #var or #let"));
			//}
			switch (type)
			{
				case TokenType.VariableLet:
					break;
				case TokenType.VariableVar:
					break;
				default:
					context.Errors.Add(new MorestachioSyntaxError(
						context.CurrentLocation.AddWindow(new CharacterSnippedLocation(0, 0, tokenValue)),
						"#var", "", "#var name", "Expected #var or #let"));
					break;
			}

			var strVarType = StringifyVariableAssignmentType(type) + " ";

			var variableNameIndex = tokenValue.IndexOf(strVarType, StringComparison.InvariantCultureIgnoreCase);
			if (variableNameIndex != 0)
			{
				context.Errors.Add(new MorestachioSyntaxError(
					context.CurrentLocation.AddWindow(new CharacterSnippedLocation(0, 0, tokenValue)),
					strVarType, "", strVarType + "name", "Expected " + strVarType));
				return default;
			}

			tokenValue = tokenValue.Substring(strVarType.Length);
			context.AdvanceLocation(strVarType.Length);
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
						strVarType, "", strVarType + "name", "Invalid character detected. Expected only spaces or letters."));
					return default;
				}
			}

			context.AdvanceLocation(lengthToExpression);
			if (variableName == null)
			{
				context.Errors.Add(new MorestachioSyntaxError(
					context
						.CurrentLocation
						.AddWindow(new CharacterSnippedLocation(0, strVarType.Length, tokenValue)),
					strVarType, "", strVarType + "name", "expected variable name"));
			}

			var expression = tokenValue.Substring(tokenValue.IndexOf('=')).Trim(' ', '=');
			if (string.IsNullOrEmpty(expression))
			{
				context.Errors.Add(new MorestachioSyntaxError(
					context.CurrentLocation
						.AddWindow(new CharacterSnippedLocation(0, strVarType.Length, tokenValue)),
					strVarType, "", strVarType + "name = ", "expected ether an path expression or an string value"));
				return default;
			}

			return new TokenPair(type, variableName, ParseExpression(expression, context), startOfExpression);
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