using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Morestachio.Document;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Tokenizing;
using Morestachio.Parsing.ParserErrors;
#if ValueTask
using ObjectPromise = System.Threading.Tasks.ValueTask<object>;
#else
using ObjectPromise = System.Threading.Tasks.Task<object>;
#endif

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
			switch (reader.Name)
			{
				case "Expression":
					exp = new MorestachioExpression();
					break;
				case "ExpressionMultiPart":
					exp = new MorestachioMultiPartExpressionList();
					break;
				case "ExpressionArgList":
					exp = new MorestachioArgumentExpressionList();
					break;
				case "ExpressionString":
					exp = new MorestachioExpressionString();
					break;
				case "ExpressionNumber":
					exp = new MorestachioExpressionNumber();
					break;
				case "ExpressionOperator":
					exp = new MorestachioOperatorExpression();
					break;
				case "ExpressionBracket":
					exp = new MorestachioBracketExpression();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(ExpressionKindNodeName));
			}
			exp.ReadXml(reader);
			return exp;
		}

		internal static void WriteExpressionToXml(this XmlWriter writer, IMorestachioExpression morestachioExpression)
		{
			switch (morestachioExpression)
			{
				case MorestachioExpression _:
					writer.WriteStartElement("Expression");
					break;
				case MorestachioBracketExpression _:
					writer.WriteStartElement("ExpressionBracket");
					break;
				case MorestachioArgumentExpressionList _:
					writer.WriteStartElement("ExpressionArgList");
					break;
				case MorestachioMultiPartExpressionList _:
					writer.WriteStartElement("ExpressionMultiPart");
					break;
				case MorestachioExpressionString _:
					writer.WriteStartElement("ExpressionString");
					break;
				case MorestachioExpressionNumber _:
					writer.WriteStartElement("ExpressionNumber");
					break;
				case MorestachioOperatorExpression _:
					writer.WriteStartElement("ExpressionOperator");
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
			TokenType type,
			IEnumerable<ITokenOption> options)
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
			if (strVarType.Length < 3)
			{
				context.Errors.Add(new MorestachioSyntaxError(
					context
						.CurrentLocation
						.Offset(0)
						.AddWindow(new CharacterSnippedLocation(0, 0, tokenValue)),
					strVarType, "", strVarType + "name", "Invalid character detected. Expected only spaces or letters."));
				return default;
			}

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
					if (i == 0 && c == '$')
					{
						continue;
					}

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

			return new TokenPair(type, variableName, startOfExpression, ParseExpression(expression, context), options);
		}

		/// <summary>
		///		Parses an Expression and then executes it
		/// </summary>
		/// <param name="expressionText"></param>
		/// <param name="options"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		public static async ObjectPromise EvaluateExpression(string expressionText, 
			ParserOptions options,  
			object context,
			TokenzierContext tokenzierContext = null)
		{
			tokenzierContext = tokenzierContext ?? TokenzierContext.FromText(expressionText);
			var expression = ParseExpression(expressionText, tokenzierContext);
			var contextObject = new ContextObject(options, "", null, context);
			var value = await expression.GetValue(contextObject, new ScopeData());
			return value.Value;
		}

		/// <summary>
		///		Parses the given text to ether an expression or an string
		/// </summary>
		/// <param name="expression"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		public static IMorestachioExpression ParseExpression(
			string expression,
			TokenzierContext context)
		{
			return ParseExpression(expression, context, out _);
		}
		/// <summary>
		///		Parses the given text to ether an expression or an string
		/// </summary>
		/// <param name="text"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		public static IMorestachioExpression ParseExpression(
			string text,
			TokenzierContext context,
			out int parsedUntil,
			int index = 0)
		{
			parsedUntil = 0;
			if (text.Length == 0)
			{
				context.Errors.Add(new MorestachioSyntaxError(
					context.CurrentLocation.AddWindow(new CharacterSnippedLocation(0, 0, text)),
					"", "", "", "expected ether an path expression or an string value"));

				return null;
			}
			
			var nIndex = MorestachioExpression.SkipWhitespaces(text, index);
			context.AdvanceLocation(nIndex - index);
			if (Tokenizer.IsStringDelimiter(text[nIndex]))
			{
				return MorestachioExpressionString.ParseFrom(text, context, out parsedUntil, nIndex);
			}
			else
			{
				return MorestachioExpression.ParseFrom(text, context, out parsedUntil, nIndex);
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
				new TokenzierContext(Tokenizer.FindNewLines(expression), cultureInfo ?? CultureInfo.CurrentCulture);
			context.SetLocation(0);
			return ParseExpression(expression,
				context);
		}
	}
}