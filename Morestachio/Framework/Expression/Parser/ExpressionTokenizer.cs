using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Tokenizing;
using Morestachio.Helper;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Framework.Expression.Parser
{
	/// <summary>
	///		Defines methods for interpreting a string that is formatted as an path expression
	/// </summary>
	public class ExpressionTokenizer
	{
		public static ExpressionTokens TokenizeExpression(string text, TokenzierContext context, int tokenIndex)
		{
			var queue = new ExpressionTokens(text);

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
					if (queue.Count == 0)//a closing bracket without any leading brackets is invalid
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
					queue.Enqueue(TokenizeOperator(text, tokenIndex, out var consumed, context));
					tokenIndex += consumed - 1;
				}
				else if (Tokenizer.IsNumberExpressionChar(c))
				{
					queue.Enqueue(TokenizeNumber(text, tokenIndex, out var consumed, context));
					tokenIndex += consumed - 1;
				}
				else if (Tokenizer.IsStartOfExpressionPathChar(c))
				{
					queue.Enqueue(TokenizePath(text, tokenIndex, out var consumed, context, text));
					tokenIndex += consumed - 1;
				}
				else if (Tokenizer.IsStringDelimiter(c))
				{
					queue.Enqueue(TokenizeString(text, tokenIndex, out var consumed, text, context));
					tokenIndex += consumed - 1;
				}
				else if (c == '[')
				{
					queue.Enqueue(TokenizeArgument(text, tokenIndex, out var consumed, text, context));
					tokenIndex += consumed - 1;
				}
				else if (Tokenizer.IsEndOfExpression(c))
				{
					if (Tokenizer.IsEndOfWholeExpression(c))
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
			context.AdvanceLocation(tokenIndex - oldIndex);
			return queue;
		}

		private static IExpressionToken TokenizeString(string textPart, int index, out int consumed, string text, TokenzierContext context)
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
				context.Errors.Add(new MorestachioSyntaxError(context.CurrentLocation.Offset(index - consumed + 1)
					.AddWindow(new CharacterSnippedLocation(0, text.Length - 1, text)), "string", text[text.Length - 1].ToString(), "expected to find " + delimiter));
			}

			return new StringToken(stringContents.ToString(), delimiter, context.CurrentLocation.Offset(index - consumed + 1));
		}

		private static IExpressionToken TokenizeArgument(string textPart, int index, out int consumed, string text, TokenzierContext context)
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
					return new ExpressionValueToken(ExpressionTokenType.Argument, expressionContents.ToString(), context.CurrentLocation.Offset(index - consumed + 1));
				}

				expressionContents.Append(c);
			}

			context.Errors.Add(new MorestachioSyntaxError(context.CurrentLocation.Offset(index - consumed + 1)
				.AddWindow(new CharacterSnippedLocation(0, text.Length - 1, text)), "argument", text[text.Length - 1].ToString(), "expected an expression after argument name declaration"));

			return null;
		}

		private static IExpressionToken TokenizeOperator(string textPart, int index, out int consumed, TokenzierContext context)
		{
			var opText = textPart[index].ToString();
			consumed = 1;
			index++;
			if (index < textPart.Length)
			{
				opText += textPart[index];
			}
			
			if (Tokenizer.IsOperationString(opText))
			{
				consumed++;
			}
			else
			{
				opText = textPart[index - 1].ToString();
			}

			var op = MorestachioOperator.Yield().FirstOrDefault(e => e.OperatorText.Equals(opText));
			if (op == null)
			{
				context.Errors.Add(new MorestachioSyntaxError(
					context.CurrentLocation.Offset(index - consumed + 1)
						.AddWindow(new CharacterSnippedLocation(0, index, textPart)), "Operator", opText, "",
					"Could not find expected operator"));
				return null;
			}
			return new OperatorToken(op.OperatorType, context.CurrentLocation.Offset(index - consumed + 1));
		}

		private static IExpressionToken TokenizePath(string textPart, int index, out int consumed, TokenzierContext context, string text)
		{
			if (Tokenizer.IsNumberExpressionChar(textPart[index]))
			{
				return TokenizeNumber(textPart, index, out consumed, context);
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
						    || Tokenizer.IsOperationChar(c))
							//the only char that can follow on a expression is ether an bracket or an argument seperator or an operator
						{
							consumed--;
							return new ExpressionToken(pathTokenizer, context.CurrentLocation.Offset(index - consumed + 1));
						}
					}

					context.Errors.Add(err());
					return null;
				}
			}

			return new ExpressionToken(pathTokenizer, context.CurrentLocation.Offset(index - consumed + 1));
		}

		private static IExpressionToken TokenizeNumber(string textPart, int index, out int consumed, TokenzierContext context)
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
						context.Errors.Add(new MorestachioSyntaxError(context.CurrentLocation.AddWindow(new CharacterSnippedLocation(0, index, textPart)), "", textPart, "Could not parse the given number"));
					}

					if (!char.IsDigit(textPart[index + 1]))
					{
						consumed--;
						break;
					}

					isFloatingNumber = true;
				}
				else if (Tokenizer.IsEndOfFormatterArgument(c) || Tokenizer.IsWhiteSpaceDelimiter(c) || Tokenizer.IsEndOfExpression(c))
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

			context.Errors.Add(new MorestachioSyntaxError(context.CurrentLocation.AddWindow(new CharacterSnippedLocation(0, index, textPart)), "", textPart, "Could not parse the given number"));
			return null;
		}
	}
}
