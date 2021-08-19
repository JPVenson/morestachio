#if ValueTask
using ObjectPromise = System.Threading.Tasks.ValueTask<object>;
#else
using ObjectPromise = System.Threading.Tasks.Task<object>;
#endif

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using Morestachio.Document;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Expression.StringParts;
using Morestachio.Framework.Tokenizing;
using Morestachio.Helper;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Framework.Expression.Parser
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

			var variableNameIndex = tokenValue.IndexOf(strVarType, StringComparison.OrdinalIgnoreCase);
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
		public static async ObjectPromise EvaluateExpression(string expressionText,
			ParserOptions options,
			object context,
			TokenzierContext tokenzierContext)
		{
			var expression = ParseExpression(expressionText, tokenzierContext);
			if (expression == null)
			{
				return null;
			}
			var contextObject = new ContextObject("", null, context);
			var value = await expression.GetValue(contextObject, new ScopeData(options));
			return value.Value;
		}
		
		/// <summary>
		///		Parses an Expression and then executes it
		/// </summary>
		public static ObjectPromise EvaluateExpression(string expressionText,
			ParserOptions options,
			object context)
		{
			return EvaluateExpression(expressionText, options, context, TokenzierContext.FromText(expressionText));
		}
		
		/// <summary>
		///		Parses an Expression and then executes it
		/// </summary>
		public static ObjectPromise EvaluateExpression(string expressionText,
			object context)
		{
			return EvaluateExpression(expressionText, new ParserOptions(), context);
		}
		
		/// <summary>
		///		Parses an Expression and then executes it
		/// </summary>
		public static ObjectPromise EvaluateExpression(string expressionText,
			ParserOptions options)
		{
			return EvaluateExpression(expressionText, options, new Dictionary<string, object>());
		}
		
		/// <summary>
		///		Parses an Expression and then executes it
		/// </summary>
		public static ObjectPromise EvaluateExpression(string expressionText)
		{
			return EvaluateExpression(expressionText, new ParserOptions(), new Dictionary<string, object>());
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

		///  <summary>
		/// 		Parses the given text to ether an expression or an string
		///  </summary>
		///  <param name="text">the text expression that should be parsed until ether EOEX or ; or #</param>
		///  <param name="context">the context describing the whole document</param>
		///  <param name="parsedUntil">outputs the number of chars consumed by the parser</param>
		///  <param name="index">the index within <see cref="text"/> from where to start the parsing</param>
		///  <returns></returns>
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

			var oldStartIndex = context.CurrentLocation.ToPosition(context);
			var tokenize = ExpressionTokenizer.TokenizeExpression(text, context, index);
			if (context.Errors.Any())
			{
				return null;
			}

			tokenize.Reset();
			var morestachioExpression = ParseExpressionRoot(tokenize, context);
			parsedUntil = index + (context.CurrentLocation.ToPosition(context) - oldStartIndex);
			return morestachioExpression;
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
			context =
				new TokenzierContext(Tokenizer.FindNewLines(expression), cultureInfo ?? CultureInfo.CurrentCulture);
			context.SetLocation(0);
			return ParseExpression(expression,
				context);
		}

		private static IMorestachioExpression ParseExpressionRoot(ExpressionTokens tokens,
			TokenzierContext context)
		{
			return ParseAnyExpression(tokens, context, token =>
			{
				switch (token.TokenType)
				{
					//only paths,brackets,numbers,string and operators are expected to lead an expression
					case ExpressionTokenType.Path:
					case ExpressionTokenType.Bracket:
					case ExpressionTokenType.Number:
					case ExpressionTokenType.String:
					case ExpressionTokenType.Operator:
						return true;
					default:
						tokens.SyntaxError(context,
							token.Location.AddWindow(new CharacterSnippedLocation(1, tokens.SourceExpression.Length,
								tokens.SourceExpression)), "Expected an expression, opening bracket, number, string or operator but got an argument name or argument seperator instead.");
						return false;
				}
			});
		}

		private static IMorestachioExpression ParseAnyExpression(ExpressionTokens tokens,
			TokenzierContext context,
			Func<IExpressionToken, bool> condition)
		{
			var expressionToken = tokens.TryPeek();
			var topParent = new MorestachioMultiPartExpressionList(expressionToken.Location);

			tokens.PeekLoop(condition, token =>
			{
				ParseExpression(topParent, tokens, context, condition);
			});

			//optimize result tree
			if (topParent.Expressions.Count == 1)
			{
				return topParent.Expressions[0];
			}

			return topParent;
		}

		private static void ParseExpression(
			IMorestachioExpression topParent,
			ExpressionTokens tokens,
			TokenzierContext context,
			Func<IExpressionToken, bool> condition)
		{
			void AddToParent(IMorestachioExpression expression)
			{
				if (topParent is MorestachioMultiPartExpressionList parentBracket)
				{
					parentBracket.Add(expression);
				}
				else if (topParent is MorestachioExpression exp)
				{
					exp.Formats.Add(new ExpressionArgument(exp.Location, expression, null));
				}
				else if (topParent is MorestachioOperatorExpression opr)
				{
					opr.RightExpression = expression;
				}
				else
				{
					throw new MorestachioParserException($"Internal Parser error. Tried to add '{expression.GetType()}' to '{topParent.GetType()}'");
				}
			}

			void ParseAndAddExpression(ExpressionToken expressionToken)
			{
				var expression = ParseMorestachioExpression(expressionToken, tokens, context);
				AddToParent(expression);
			}

			void ParseAndAddBracket(ExpressionValueToken token)
			{
				var exp = new MorestachioBracketExpression(token.Location);
				

				bool SubCondition(IExpressionToken subToken)
				{
					return !(subToken.TokenType == ExpressionTokenType.Bracket && ((ExpressionValueToken)subToken).Value == ")");
				}
				tokens.PeekLoop(SubCondition, subToken =>
				{
					exp.Add(ParseAnyExpression(tokens, context, SubCondition));
				});

				tokens.TryDequeue(() =>
				{
					tokens.SyntaxError(context,
						token.Location.AddWindow(new CharacterSnippedLocation(1, tokens.SourceExpression.Length,
							tokens.SourceExpression)), "Expected a )");
				});//dequeue )

				AddToParent(exp);
			}

			void ParseAndAddString(StringToken token)
			{
				var strExpression = new MorestachioExpressionString(token.Location, token.Delimiter);
				strExpression.StringParts.Add(new ExpressionStringConstPart(token.Value, token.Location));
				AddToParent(strExpression);
			}

			void ParseAndAddNumber(NumberToken token)
			{
				var expressionNumber = new MorestachioExpressionNumber(token.Number, token.Location);
				AddToParent(expressionNumber);
			}
			
			void ParseAndAddOperator(OperatorToken token)
			{
				var op = MorestachioOperator.Operators[token.Value];
				if (op.Placement == OperatorPlacement.Right)
				{
					MorestachioOperatorExpression operat;

					//the operator is placed to the right hand of the expression.
					//remove the last added expression or expression list and replace it with this operator
					if (topParent is MorestachioMultiPartExpressionList parentBracket)
					{
						if (parentBracket.Expressions.Count == 0)
						{
							tokens.SyntaxError(context,
								token.Location.AddWindow(new CharacterSnippedLocation(1, tokens.SourceExpression.Length,
									tokens.SourceExpression)), "Invalid use of right hand operator without an expression to its left");
							return;
						}

						var leftExpression = parentBracket.Expressions.Last();
						parentBracket.Expressions.Remove(leftExpression);
						parentBracket.Expressions.Add(
							operat = new MorestachioOperatorExpression(op, leftExpression, leftExpression.Location));
					}
					else if (topParent is MorestachioExpression exp)
					{
						if (exp.Formats.Count == 0)
						{
							tokens.SyntaxError(context,
								token.Location.AddWindow(new CharacterSnippedLocation(1, tokens.SourceExpression.Length,
									tokens.SourceExpression)), "Invalid use of right hand operator without an expression to its left");
							return;
						}

						var argExp = exp.Formats.Last().MorestachioExpression;
						exp.Formats.Last().MorestachioExpression =
							operat = new MorestachioOperatorExpression(op, argExp, argExp.Location);
					}
					else if (topParent is MorestachioOperatorExpression opr)
					{
						opr.RightExpression = operat = new MorestachioOperatorExpression(op,
							opr.RightExpression, opr.Location);
					}
					else
					{
						tokens.SyntaxError(context,
							token.Location.AddWindow(new CharacterSnippedLocation(1, tokens.SourceExpression.Length,
								tokens.SourceExpression)), "Invalid use of a Binary operator on an unsupported expression type");
						return;
					}

					if (op.IsBinaryOperator)
					{
						if (tokens.Count == 0)
						{
							tokens.SyntaxError(context,
								token.Location.AddWindow(new CharacterSnippedLocation(1, tokens.SourceExpression.Length,
									tokens.SourceExpression)), "Expected a 2nd expression for the used binary operator");
							return;
						}
						
						var operatRightExpression = ParseAnyExpression(tokens, context, subToken =>
						{
							return condition(subToken) && subToken.TokenType != ExpressionTokenType.Operator;
						});
						operat.RightExpression = operatRightExpression;
					}
				}
				else
				{
					//the operator is placed on the left hand of the expression so it muse be an unary operator
					//it can only accept one argument
					var operatorExp = new MorestachioOperatorExpression(op, null, context.CurrentLocation);
					AddToParent(operatorExp);
					operatorExp.LeftExpression = ParseAnyExpression(tokens, context, subToken =>
					{
						return condition(subToken) && subToken.TokenType != ExpressionTokenType.Operator;
					});
				}
			}

			bool LoopExpressions(IExpressionToken token)
			{
				switch (token.TokenType)
				{
					case ExpressionTokenType.Path:
						ParseAndAddExpression((ExpressionToken)token);
						break;
					case ExpressionTokenType.Bracket:
						ParseAndAddBracket((ExpressionValueToken)token);
						break;
					case ExpressionTokenType.Number:
						ParseAndAddNumber((NumberToken)token);
						break;
					case ExpressionTokenType.String:
						ParseAndAddString((StringToken)token);
						break;
					case ExpressionTokenType.Operator:
						ParseAndAddOperator((OperatorToken)token);
						break;
					case ExpressionTokenType.ArgumentSeperator:
					default:

						tokens.SyntaxError(context, token.Location.AddWindow(new CharacterSnippedLocation(1, tokens.SourceExpression.Length, tokens.SourceExpression)), "Unexpected use of an argument seperator");
						return false;
				}

				return true;
			}

			tokens.Loop(condition, LoopExpressions);
		}

		private static IMorestachioExpression ParseMorestachioExpression(
			ExpressionToken token,
			ExpressionTokens tokens,
			TokenzierContext context)
		{
			var expression = new MorestachioExpression(token.Location);
			var next = tokens.TryPeek();
			if (next?.TokenType == ExpressionTokenType.Bracket && ((ExpressionValueToken)next).Value == "(")
			{
				expression.FormatterName = token.Value.GetFormatterName(context, 0);

				expression.PathParts = new Traversable(token.Value.Compile(context, 0));
				tokens.TryDequeue(() =>
				{
					tokens.SyntaxError(context,
						token.Location.AddWindow(new CharacterSnippedLocation(1, tokens.SourceExpression.Length,
							tokens.SourceExpression)), "Expected a (");
				});

				bool Condition(IExpressionToken innerToken)
				{
					return !(innerToken.TokenType == ExpressionTokenType.Bracket
							 && ((ExpressionValueToken)innerToken).Value == ")");
				}

				tokens.PeekLoop(Condition, subToken =>
				{
					string argumentName = null;
					if (subToken.TokenType == ExpressionTokenType.Argument)
					{
						tokens.Dequeue();
						argumentName = ((ExpressionValueToken)subToken).Value;
					}

					var anyElse = ParseAnyExpression(tokens, context, (innerToken) =>
					{
						return Condition(innerToken)
							   && innerToken.TokenType != ExpressionTokenType.ArgumentSeperator;
					});
					expression.Formats.Add(new ExpressionArgument(anyElse.Location, anyElse, argumentName));

					next = tokens.TryPeek();
					if (next == null)
					{
						tokens.SyntaxError(context,
						token.Location.AddWindow(new CharacterSnippedLocation(1, tokens.SourceExpression.Length,
							tokens.SourceExpression)), "Unexpected end of expression. Expected ether a argument seperator ',' or a closing bracket ')'");
					}

					if (next?.TokenType != ExpressionTokenType.ArgumentSeperator)
					{
						return false;
					}
					if (next?.TokenType == ExpressionTokenType.ArgumentSeperator)
					{
						tokens.Dequeue();//dequeue ,
					}
					return true;
				});

				tokens.TryDequeue(() =>
				{
					tokens.SyntaxError(context,
						token.Location.AddWindow(new CharacterSnippedLocation(1, tokens.SourceExpression.Length,
							tokens.SourceExpression)), "Expected a )");
				});
			}
			else
			{
				expression.PathParts = new Traversable(token.Value.CompileListWithCurrent(context, 0));
			}

			return expression;
		}
	}
}