using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using Morestachio.Document;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Expression.StringParts;
using Morestachio.Framework.Tokenizing;
using Morestachio.Helper;
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

			var oldStartIndex = context.CurrentLocation.ToPosition(context);
			var tokenize = TokenizeExpression(text, context, index);
			if (context.Errors.Any())
			{
				return null;
			}
			var morestachioExpression = ParseExpression(tokenize, context);
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

		private static IMorestachioExpression ParseExpression(ExpressionTokens tokens,
			TokenzierContext context)
		{
			return ParseAnyExpression(tokens, context, token =>
			{
				switch (token.TokenType)
				{
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
			var topParent = new MorestachioMultiPartExpressionList(tokens.First().Location);
			tokens.PeekLoop(condition, token =>
			{
				ParseExpression(topParent, tokens, context, condition);
			});

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
				Func<IExpressionToken, bool> subCondition = subToken =>
				{
					return !(subToken.TokenType == ExpressionTokenType.Bracket &&
							 ((ExpressionValueToken)subToken).Value == ")");
				};
				tokens.PeekLoop(subCondition, subToken =>
				{
					exp.Add(ParseAnyExpression(tokens, context, subCondition));
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
					//the operator is placed on the left hand of the expression
					//it can only accept one argument
					//TODO implement unary left hand operators
				}
			}

			tokens.Loop(condition, token =>
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

						tokens.SyntaxError(context,
							token.Location.AddWindow(new CharacterSnippedLocation(1, tokens.SourceExpression.Length,
								tokens.SourceExpression)), "Unexpected use of an argument seperator");
						return false;
				}

				return true;
			});
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
				expression.FormatterName = token.Value.GetFormatterName(context, 0, out var found, out var err);
				if (err != null)
				{
					context.Errors.Add(err());
				}

				expression.PathParts = new Traversable(token.Value.Compile(context, 0));
				tokens.TryDequeue(() =>
				{
					tokens.SyntaxError(context,
						token.Location.AddWindow(new CharacterSnippedLocation(1, tokens.SourceExpression.Length,
							tokens.SourceExpression)), "Expected a (");
				});
				Func<IExpressionToken, bool> condition = subToken =>
				{
					return !(subToken.TokenType == ExpressionTokenType.Bracket &&
							 ((ExpressionValueToken)subToken).Value == ")");
				};
				tokens.PeekLoop(condition, subToken =>
				{
					string argumentName = null;
					if (subToken.TokenType == ExpressionTokenType.Argument)
					{
						tokens.Dequeue();
						argumentName = ((ExpressionValueToken)subToken).Value;
					}

					var anyElse = ParseAnyExpression(tokens, context, (innerToken) =>
					{
						return condition(innerToken)
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
				expression.PathParts = new Traversable(token.Value.CompileListWithCurrent(context, 0, out var err));
				if (err != null)
				{
					context.Errors.Add(err());
				}
			}

			return expression;
		}

		private static ExpressionTokens TokenizeExpression(string text, TokenzierContext context, int tokenIndex)
		{
			var queue = new ExpressionTokens(text);

			IExpressionToken TokenizeNumber(string textPart, int index, out int consumed)
			{
				consumed = 0;
				var isFloatingNumber = false;
				var nrText = new StringBuilder();
				for (; index < textPart.Length; index++)
				{
					consumed++;
					var c = textPart[index];
					if (c == CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator[0])
					{
						if (isFloatingNumber)
						{
							consumed--;
							break;
						}

						if (index + 1 > textPart.Length)
						{
							context.Errors.Add(new MorestachioSyntaxError(
								context
									.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(0, index, textPart)),
								"", textPart, "Could not parse the given number"));
						}

						if (!char.IsDigit(textPart[index + 1]))
						{
							consumed--;
							break;
						}

						isFloatingNumber = true;
					}
					else if (Tokenizer.IsEndOfFormatterArgument(c)
							 || Tokenizer.IsWhiteSpaceDelimiter(c)
							 || Tokenizer.IsEndOfExpression(c))
					{
						consumed--;
						break;
					}

					nrText.Append(c);
				}

				textPart = nrText.ToString();
				if (Number.TryParse(textPart, CultureInfo.InvariantCulture, out var nr))
				{
					return new NumberToken(nr, context.CurrentLocation.Offset(index - consumed + 1));
				}

				context.Errors.Add(new MorestachioSyntaxError(
					context
						.CurrentLocation
						.AddWindow(new CharacterSnippedLocation(0, index, textPart)),
					"", textPart, "Could not parse the given number"));
				return null;
			}

			IExpressionToken TokenizePath(string textPart, int index, out int consumed)
			{
				if (Tokenizer.IsNumberExpressionChar(textPart[index]))
				{
					return TokenizeNumber(textPart, index, out consumed);
				}

				var pathTokenizer = new PathTokenizer();

				consumed = 0;
				for (int i = index; i < textPart.Length; i++)
				{
					consumed++;
					var c = text[i];
					if (Tokenizer.IsWhiteSpaceDelimiter(c))
					{
						continue;
					}

					if (!pathTokenizer.Add(c, context, index, out var err))
					{
						if (!Tokenizer.IsExpressionPathChar(c) || Tokenizer.IsOperationChar(c))
						{
							if (c == '('
								|| c == ')'
								|| Tokenizer.IsPathDelimiterChar(c)
								|| Tokenizer.IsEndOfExpression(c)
								|| Tokenizer.IsOperationChar(c)
								)
							//the only char that can follow on a expression is ether an bracket or an argument seperator or an operator
							{
								consumed--;
								return new ExpressionToken(
									pathTokenizer,
									context.CurrentLocation.Offset(index - consumed + 1));
							}
						}
						context.Errors.Add(err());
						return null;
					}
				}
				return new ExpressionToken(
					pathTokenizer,
					context.CurrentLocation.Offset(index - consumed + 1));
			}

			IExpressionToken TokenizeOperator(string textPart, int index, out int consumed)
			{
				var opText = textPart[index].ToString();
				consumed = 1;
				index++;
				if (index < textPart.Length)
				{
					opText += textPart[index];
				}
				OperatorTypes opType;
				if (Tokenizer.IsOperationString(opText))
				{
					consumed++;
				}
				else
				{
					opText = textPart[index - 1].ToString();
				}
				opType = MorestachioOperator.Yield().FirstOrDefault(e => e.OperatorText.Equals(opText)).OperatorType;
				return new OperatorToken(
					opType,
					context.CurrentLocation.Offset(index - consumed + 1));
			}

			IExpressionToken TokenizeArgument(string textPart, int index, out int consumed)
			{
				var expressionContents = new StringBuilder();
				consumed = 1;
				index++;
				for (int i = index; i < textPart.Length; i++)
				{
					consumed++;
					var c = text[i];
					if (Tokenizer.IsWhiteSpaceDelimiter(c))
					{
						continue;
					}

					if (c == ']')
					{
						return new ExpressionValueToken(ExpressionTokenType.Argument,
							expressionContents.ToString(),
							context.CurrentLocation.Offset(index - consumed + 1));
					}

					expressionContents.Append(c);
				}
				context.Errors.Add(new MorestachioSyntaxError(
					context
						.CurrentLocation
						.Offset(index - consumed + 1)
						.AddWindow(new CharacterSnippedLocation(0, text.Length - 1, text)),
					"argument", text[text.Length - 1].ToString(), "expected an expression after argument name declaration"));

				return null;
			}

			IExpressionToken TokenizeString(string textPart, int index, out int consumed)
			{
				var delimiter = textPart[index];
				index++;
				consumed = 1;
				var stringContents = new StringBuilder();

				var endDelimiterFound = false;
				var isEscapeChar = false;
				for (; index < text.Length; index++)
				{
					consumed++;
					var c = text[index];
					if (isEscapeChar)
					{
						stringContents.Append(c);
						if (c == delimiter)
						{
							isEscapeChar = false;
						}
					}
					else
					{
						if (c == '\\')
						{
							isEscapeChar = true;
						}
						else if (c == delimiter)
						{
							endDelimiterFound = true;
							break;
						}
						else
						{
							stringContents.Append(c);
						}
					}
				}

				if (!endDelimiterFound)
				{
					consumed--;
					context.Errors.Add(new MorestachioSyntaxError(
						context
							.CurrentLocation
							.Offset(index - consumed + 1)
							.AddWindow(new CharacterSnippedLocation(0, text.Length - 1, text)),
						"string", text[text.Length - 1].ToString(), "expected to find " + delimiter));
				}
				return new StringToken(stringContents.ToString(),
					delimiter,
					context.CurrentLocation.Offset(index - consumed + 1));
			}

			var oldIndex = tokenIndex;
			for (; tokenIndex < text.Length; tokenIndex++)
			{
				var c = text[tokenIndex];
				if (c == '(')
				{
					queue.Enqueue(new ExpressionValueToken(ExpressionTokenType.Bracket,
						"(",
						context.CurrentLocation.Offset(tokenIndex)));
				}
				else if (c == ')')
				{
					if (queue.Count == 0)
					{
						queue.SyntaxError(context, context
								.CurrentLocation
								.Offset(tokenIndex)
								.AddWindow(new CharacterSnippedLocation(0, tokenIndex, text)),
							"A closing bracket cannot lead an expression");
						break;
					}
					queue.Enqueue(new ExpressionValueToken(ExpressionTokenType.Bracket,
						")",
						context.CurrentLocation.Offset(tokenIndex)));
				}
				else if (c == ',')
				{
					var last = queue.TryPeek();
					if (last == null)
					{
						queue.SyntaxError(context, context
							.CurrentLocation
							.Offset(tokenIndex)
							.AddWindow(new CharacterSnippedLocation(0, tokenIndex, text)),
							"A argument separator cannot lead an expression");
						break;
					}

					queue.Enqueue(new ExpressionValueToken(ExpressionTokenType.ArgumentSeperator,
						",",
						context.CurrentLocation.Offset(tokenIndex)));
				}
				else if (Tokenizer.IsOperationChar(c))
				{
					queue.Enqueue(TokenizeOperator(text, tokenIndex, out var consumed));
					tokenIndex += consumed - 1;
				}
				else if (Tokenizer.IsNumberExpressionChar(c))
				{
					queue.Enqueue(TokenizeNumber(text, tokenIndex, out var consumed));
					tokenIndex += consumed - 1;
				}
				else if (Tokenizer.IsStartOfExpressionPathChar(c))
				{
					queue.Enqueue(TokenizePath(text, tokenIndex, out var consumed));
					tokenIndex += consumed - 1;
				}
				else if (Tokenizer.IsStringDelimiter(c))
				{
					queue.Enqueue(TokenizeString(text, tokenIndex, out var consumed));
					tokenIndex += consumed - 1;
				}
				else if (c == '[')
				{
					queue.Enqueue(TokenizeArgument(text, tokenIndex, out var consumed));
					tokenIndex += consumed - 1;
				}
				else if (Tokenizer.IsEndOfExpression(c))
				{
					if (c == ';')
					{
						tokenIndex++;
					}
					break;
				}
				else if (Tokenizer.IsWhiteSpaceDelimiter(c))
				{
					//just skip it
				}
				else
				{
					queue.SyntaxError(context, context.CurrentLocation.Offset(tokenIndex)
						.AddWindow(new CharacterSnippedLocation(0, tokenIndex, text)), "Unknown character detected");
					break;
				}
			}

			queue.ParsedUntilIndex = tokenIndex - oldIndex;
			context.AdvanceLocation(queue.ParsedUntilIndex);
			return queue;
		}
	}

	/// <summary>
	///		Represents a Expression token
	/// </summary>
	public interface IExpressionToken
	{
		/// <summary>
		///		Defines the type of token
		/// </summary>
		ExpressionTokenType TokenType { get; }

		/// <summary>
		///		Defines the location within the template of this token
		/// </summary>
		CharacterLocation Location { get; }
	}

	internal readonly struct NumberToken : IExpressionToken
	{
		public NumberToken(Number number, CharacterLocation location)
		{
			TokenType = ExpressionTokenType.Number;
			Number = number;
			Location = location;
		}

		public ExpressionTokenType TokenType { get; }
		public Number Number { get; }
		public CharacterLocation Location { get; }
	}

	internal readonly struct StringToken : IExpressionToken
	{
		public StringToken(string value, char delimiter, CharacterLocation location)
		{
			TokenType = ExpressionTokenType.String;
			Value = value;
			Delimiter = delimiter;
			Location = location;
		}

		public ExpressionTokenType TokenType { get; }
		public string Value { get; }
		public CharacterLocation Location { get; }
		public char Delimiter { get; }
	}

	internal readonly struct ExpressionValueToken : IExpressionToken
	{
		public ExpressionValueToken(ExpressionTokenType tokenType, string value, CharacterLocation location)
		{
			TokenType = tokenType;
			Value = value;
			Location = location;
		}

		public ExpressionTokenType TokenType { get; }
		public string Value { get; }
		public CharacterLocation Location { get; }
	}

	internal readonly struct ExpressionToken : IExpressionToken
	{
		public ExpressionToken(PathTokenizer value, CharacterLocation location)
		{
			TokenType = ExpressionTokenType.Path;
			Value = value;
			Location = location;
		}

		public ExpressionTokenType TokenType { get; }
		public PathTokenizer Value { get; }
		public CharacterLocation Location { get; }
	}

	internal readonly struct OperatorToken : IExpressionToken
	{
		public OperatorToken(OperatorTypes value, CharacterLocation location)
		{
			TokenType = ExpressionTokenType.Operator;
			Value = value;
			Location = location;
		}

		public ExpressionTokenType TokenType { get; }
		public OperatorTypes Value { get; }
		public CharacterLocation Location { get; }
	}

	/// <summary>
	///		Defines all possible tokens that can be tokenized by the <see cref="ExpressionParser"/>
	/// </summary>
	public enum ExpressionTokenType
	{
		/// <summary>
		///		An path pointing to a property or formatter. 	
		/// </summary>
		Path,

		/// <summary>
		///		Defines a seperator for two formatter arguments
		/// </summary>
		ArgumentSeperator,

		/// <summary>
		///		Defines the start of an named formatter argument
		/// </summary>
		Argument,

		/// <summary>
		///		Defines a bracket used ether for seperation or a formatter call 
		/// </summary>
		Bracket,

		/// <summary>
		///		Defines a number
		/// </summary>
		Number,

		/// <summary>
		///		Defines a string
		/// </summary>
		String,

		/// <summary>
		///		Defines the use of an operator
		/// </summary>
		Operator,
	}

	/// <summary>
	///		Contains a queue where all tokens of a expression is loaded
	/// </summary>
	public class ExpressionTokens : Queue<IExpressionToken>
	{
		/// <summary>
		///		Creates a new Token queue
		/// </summary>
		/// <param name="sourceExpression"></param>
		public ExpressionTokens(string sourceExpression)
		{
			SourceExpression = sourceExpression;
		}

		/// <summary>
		///		Contains the original expression in its string form
		/// </summary>
		public string SourceExpression { get; }

		public int ParsedUntilIndex { get; set; }

		internal IExpressionToken TryPeek()
		{
			if (Count > 0)
			{
				return Peek();
			}

			return null;
		}

		internal void Loop(
			Func<IExpressionToken, bool> condition,
			Action<IExpressionToken> action)
		{
			while (Count > 0 && condition(Peek()))
			{
				action(Dequeue());
			}
		}

		internal void PeekLoop(
			Func<IExpressionToken, bool> condition,
			Action<IExpressionToken> action)
		{
			IExpressionToken peek;
			IExpressionToken oldPeek = default;
			while (Count > 0
				   && condition(peek = Peek()))
			{
				if (Equals(oldPeek, peek))
				{
					throw new Exception();
				}
				action(peek);
				oldPeek = peek;
			}
		}

		internal void Loop(
			Func<IExpressionToken, bool> condition,
			Func<IExpressionToken, bool> action)
		{
			while (Count > 0 && condition(Peek()))
			{
				if (!action(Dequeue()))
				{
					break;
				}
			}
		}

		internal void PeekLoop(
			Func<IExpressionToken, bool> condition,
			Func<IExpressionToken, bool> action)
		{
			IExpressionToken peek;
			IExpressionToken oldPeek = default;
			while (Count > 0
				   && condition(peek = Peek()))
			{
				if (Equals(oldPeek, peek))
				{
					throw new Exception();
				}

				if (!action(peek))
				{
					break;
				}
				oldPeek = peek;
			}
		}

		internal IExpressionToken TryDequeue(Action onError)
		{
			if (Count == 0)
			{
				onError();
				return null;
			}

			return Dequeue();
		}

		internal void SyntaxError(
			TokenzierContext context,
			CharacterLocationExtended location,
			string helpText)
		{
			context.Errors.Add(new InvalidPathSyntaxError(location, SourceExpression, helpText));
		}
	}
}