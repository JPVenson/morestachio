using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Tokenizing;
using Morestachio.Helper;
using Morestachio.Parsing.ParserErrors;
using Morestachio.Util;

namespace Morestachio.Framework.Expression.Parser;

/// <summary>
///		Defines methods for interpreting a string that is formatted as an path expression
/// </summary>
public class ExpressionTokenizer
{
	/// <summary>
	///		Tokenizes an expression text
	/// </summary>
	public static (ExpressionTokens, TextIndex) TokenizeExpression(
		string text,
		TokenzierContext context,
		TextRange range,
		TextIndex textIndex
	)
	{
		var queue = new ExpressionTokens(text);
		int cursorIndex;

		for (cursorIndex = range.RangeStart.Index; cursorIndex < range.RangeEnd.Index; cursorIndex++)
		{
			//remember to reset the index by 1 to account for the increment part of the for loop that will add one
			var c = text[cursorIndex];
			if (c == '(')
			{
				queue.Enqueue(new ExpressionValueToken(ExpressionTokenType.Bracket,
					"(",
					TextRange.Range(context, textIndex.Index + cursorIndex, 1, range)
					));
			}
			else if (c == ')')
			{
				if (queue.Count == 0)//a closing bracket without any leading brackets is invalid
				{
					queue.SyntaxError(context, TextRange.Range(context, textIndex.Index + cursorIndex, 1, range),
						"A closing bracket cannot lead an expression");
					break;
				}
				queue.Enqueue(new ExpressionValueToken(ExpressionTokenType.Bracket,
					")",
					TextRange.Range(context, textIndex.Index + cursorIndex, 1, range)));
			}
			else if (c == ',')
			{
				var last = queue.TryPeek();
				if (last == null)
				{
					queue.SyntaxError(context, TextRange.Range(context, textIndex.Index + cursorIndex, 1, range),
						"A argument separator cannot lead an expression");
					break;
				}

				queue.Enqueue(new ExpressionValueToken(ExpressionTokenType.ArgumentSeperator,
					",",
					TextRange.Range(context, textIndex.Index + cursorIndex, 1, range)));
			}
			else if (Tokenizer.IsOperationChar(c))
			{
				queue.Enqueue(TokenizeOperator(text, cursorIndex, textIndex, out var consumed, context));
				cursorIndex += consumed - 1;
			}
			else if (Tokenizer.IsNumberExpressionChar(c))
			{
				queue.Enqueue(TokenizeNumber(text, cursorIndex, textIndex, out var consumed, context));
				cursorIndex += consumed - 1;
			}
			else if (Tokenizer.IsStartOfExpressionPathChar(c))
			{
				var tokenizePath = TokenizePath(text, cursorIndex, textIndex, context, text);
				queue.Enqueue(tokenizePath.Item1);
				cursorIndex = tokenizePath.Item2;
			}
			else if (Tokenizer.IsStringDelimiter(c))
			{
				queue.Enqueue(TokenizeString(text, cursorIndex, textIndex, out var consumed, text, context));
				cursorIndex += consumed - 1;
			}
			else if (c == '[')
			{
				queue.Enqueue(TokenizeArgument(text, cursorIndex, textIndex, out var consumed, text, context));
				cursorIndex += consumed - 1;
			}
			else if (Tokenizer.IsEndOfExpression(c))
			{
				if (Tokenizer.IsEndOfWholeExpression(c))
				{
					//also consume the ; token
					cursorIndex += 1;
				}
					
				//context.AdvanceLocation(cursorIndex - oldIndex);
				return (queue, TextIndex.GetIndex(context, cursorIndex + textIndex.Index));
			}
			else if (Tokenizer.IsWhiteSpaceDelimiter(c))
			{
				//just skip it
			}
			else
			{
				queue.SyntaxError(context, TextRange.Range(context, textIndex.Index + cursorIndex, 1, range), "Unknown character detected");
				break;
			}
		}
		return (queue, TextIndex.GetIndex(context, cursorIndex + textIndex.Index));
	}

	private static IExpressionToken TokenizeString(string textPart, 
													int index, 
													TextIndex textIndex, 
													out int consumed, 
													string text, 
													TokenzierContext context)
	{
		var delimiter = textPart[index];
		index++;
		consumed = 1;
		var stringContents = StringBuilderCache.Acquire(textPart.Length);

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
					index++;
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
			context.Errors.Add(new MorestachioSyntaxError(TextRange.Range(context, textIndex.Index + index, 1), 
				"string", 
				text[text.Length - 1].ToString(),
				"expected to find " + delimiter));
		}

		return new StringToken(StringBuilderCache.GetStringAndRelease(stringContents), delimiter,
			TextRange.RangeIndex(context, textIndex.Index, index));
	}

	private static IExpressionToken TokenizeArgument(
		string textPart,
		int index,
		TextIndex textIndex,
		out int consumed,
		string text,
		TokenzierContext context
	)
	{
		var expressionContents = StringBuilderCache.Acquire();
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
					StringBuilderCache.GetStringAndRelease(expressionContents), 
					TextRange.Range(context, index - consumed + textIndex.Index, consumed));
			}

			expressionContents.Append(c);
		}

		context.Errors.Add(new MorestachioSyntaxError(TextRange.Range(context, index - consumed + textIndex.Index, consumed),
			"argument", 
			text[text.Length - 1].ToString(), 
			"expected an expression after argument name declaration"));

		return null;
	}

	private static IExpressionToken TokenizeOperator(
		string textPart,
		int index,
		TextIndex textIndex,
		out int consumed,
		TokenzierContext context
	)
	{
		var opText = textPart[index].ToString();
		consumed = 1;
		index++;
		if (index < textPart.Length)
		{
			opText += textPart[index];
		}

		if (opText == "=>")
		{
			consumed++;
			return new LambdaExpressionToken(TextRange.Range(context, textIndex.Index + index, consumed));
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
				TextRange.Range(context, textIndex.Index + index, consumed + 1),
				"Operator", 
				opText,
				"",
				"Could not find expected operator"));
			return null;
		}
		return new OperatorToken(op.OperatorType, TextRange.Range(context, textIndex.Index + index, consumed));
	}

	private static (IExpressionToken, int) TokenizePath(
		string textPart,
		int index,
		TextIndex textIndex,
		TokenzierContext context,
		string text
	)
	{
		var pathTokenizer = new PathTokenizer();
		var sourceIndex = index;
			
		for (; index < textPart.Length; index++)
		{
			var c = text[index];
			if (Tokenizer.IsWhiteSpaceDelimiter(c))
			{
				continue;
			}

			if (!pathTokenizer.Add(c, context, index, out Func<TextRange, IMorestachioError> err))
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
						//consume everything before that current char!
						return (new ExpressionToken(pathTokenizer, TextRange.RangeIndex(context,
									sourceIndex + textIndex.Index,
									index)), index - 1);
					}
				}

				context.Errors.Add(err(TextRange.Range(context, index, text.Length)));
				return (new ExpressionToken(pathTokenizer, TextRange.RangeIndex(context,
							sourceIndex,
							index)), index);
			}
		}

		//decrement the index by one as the for loop will always add one after running out
		return (new ExpressionToken(pathTokenizer, TextRange.RangeIndex(context,
					sourceIndex,
					index)), index - 1);
	}

	private static IExpressionToken TokenizeNumber(
		string textPart,
		int index,
		TextIndex textIndex,
		out int consumed,
		TokenzierContext context
	)
	{
		consumed = 0;
		var isFloatingNumber = false;
		var nrText = StringBuilderCache.Acquire();
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
					context.Errors.Add(new MorestachioSyntaxError(TextRange.Range(context, textIndex.Index + index + 1, consumed),
						"",
						textPart,
						"Could not parse the given number"));
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

		textPart = StringBuilderCache.GetStringAndRelease(nrText);
		if (Number.TryParse(textPart, CultureInfo.InvariantCulture, out var nr))
		{
			return new NumberToken(nr, TextRange.Range(context, textIndex.Index + index + 1, consumed));
		}

		context.Errors.Add(new MorestachioSyntaxError(TextRange.Range(context, textIndex.Index + index + 1, consumed),
			"",
			textPart,
			"Could not parse the given number"));
		return null;
	}
}