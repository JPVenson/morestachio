using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Morestachio.ParserErrors;

namespace Morestachio.Framework
{
	public static class ExpressionTokenizer
	{
		public static TokenPair[] Tokenize(string tokenValue,
			CharacterLocation tokenLocation,
			List<int> lines,
			int tokenIndex,
			ICollection<IMorestachioError> parseErrors,
			ParserOptions options)
		{
			var variableNameIndex = tokenValue.IndexOf("#var ");
			if (variableNameIndex != 0)
			{
				parseErrors.Add(new MorestachioSyntaxError(
					tokenLocation.AddWindow(new CharacterSnippedLocation(0, 0, tokenValue)),
					"#var", "", "#var name", "Expected #var"));
				return new TokenPair[0];
			}

			tokenValue = tokenValue.Substring("#var ".Length);
			string variableName = null;
			int i = 0;
			for (; i < tokenValue.Length; i++)
			{
				var c = tokenValue[i];
				if (c == '=')
				{
					variableName = tokenValue.Substring(0, i).Trim(' ');
					break;
				}

				if (!char.IsLetter(c) && c != ' ')
				{
					parseErrors.Add(new MorestachioSyntaxError(
						tokenLocation.AddWindow(new CharacterSnippedLocation(0, i, tokenValue)),
						"#var", "", "#var name", "Invalid character detected. Expected only spaces or letters."));
					return new TokenPair[0];
				}
			}

			if (variableName == null)
			{
				parseErrors.Add(new MorestachioSyntaxError(
					tokenLocation.AddWindow(new CharacterSnippedLocation(0, "#var ".Length, tokenValue)),
					"#var", "", "#var name", "expected variable name"));
			}

			var expression = tokenValue.Substring(tokenValue.IndexOf('=')).Trim(' ', '=');
			if (string.IsNullOrEmpty(expression))
			{
				parseErrors.Add(new MorestachioSyntaxError(
					tokenLocation.AddWindow(new CharacterSnippedLocation(0, "#var ".Length, tokenValue)),
					"#var", "", "#var name = ", "expected ether an path expression or an string value"));
				return new TokenPair[0];
			}

			var tokens = new List<TokenPair>();
			tokens.Add(new TokenPair(TokenType.VariableDeclaration, variableName, tokenLocation));
			var formats = Tokenizer.EnumerateFormats(expression, lines, tokenIndex, parseErrors);
			if (!formats.Any())
			{
				var headerArgumentType = ValidateArgument(ref expression, parseErrors, tokenLocation.Offset("#var ".Length + i), lines, tokenIndex);
				if (headerArgumentType == null)
				{
					return new TokenPair[0];
				}

				switch (headerArgumentType)
				{
					case Tokenizer.HeaderArgumentType.String:
						tokens.Add(new TokenPair(TokenType.Content,
							expression,
							Tokenizer.HumanizeCharacterLocation(tokenIndex, lines)));
						break;
					case Tokenizer.HeaderArgumentType.Expression:
						tokens.Add(new TokenPair(TokenType.EscapedSingleValue,
							expression,
							Tokenizer.HumanizeCharacterLocation(tokenIndex, lines)));
						break;
				}
			}
			else
			{
				tokens.AddRange(Tokenizer.TokenizeFormattables(formats, expression, null, lines, tokenIndex, parseErrors, options));
			}

			tokens.Add(new TokenPair(TokenType.VariableSet, variableName, tokenLocation));
			return tokens.ToArray();
		}
		
		internal static Tokenizer.HeaderArgumentType? ValidateArgument(ref string expression,
			ICollection<IMorestachioError> parseErrors,
			CharacterLocation tokenLocation,
			List<int> lines,
			int tokenIndex)
		{
			if (expression.Length == 0)
			{
				parseErrors.Add(new MorestachioSyntaxError(
					tokenLocation.AddWindow(new CharacterSnippedLocation(0, 0, expression)),
					"#var", "", "#var name = ", "expected ether an path expression or an string value"));

				return null;
			}

			if (Tokenizer.IsStringDelimiter(expression[0]))
			{
				//its a string constant
				if (!Tokenizer.IsStringDelimiter(expression[expression.Length - 1]))
				{
					parseErrors.Add(new MorestachioSyntaxError(
						tokenLocation.AddWindow(new CharacterSnippedLocation(0, expression.Length, expression)),
						"#var", "", "#var name = " + expression[0], "expected " + expression[0]));
					return null;
				}

				var expectStringDelimiter = false;
				var delimiter = expression[0];

				var resultString = "";
				expression = expression.Substring(1, expression.Length - 2);
				for (int i = 0; i < expression.Length; i++)
				{
					var c = expression[i];
					if (expectStringDelimiter)
					{
						resultString += c;
						if (c == delimiter)
						{
							expectStringDelimiter = false;
						}
					}
					else
					{
						if (c == '\\')
						{
							expectStringDelimiter = true;
						}
						else
						{
							resultString += c;
						}

						if (c == delimiter)
						{
							parseErrors.Add(new MorestachioSyntaxError(
								tokenLocation.AddWindow(new CharacterSnippedLocation(0, i, expression)),
								"#var", "", expression, "Unexpected " + c + ". Expected ether an escaped \\" + c + " or end of string"));
							return null;
						}
					}
				}

				expression = resultString;

				return Tokenizer.HeaderArgumentType.String;
			}
			else
			{
				Tokenizer.Validated(expression, tokenIndex, lines, parseErrors);
				return Tokenizer.HeaderArgumentType.Expression;
			}
		}
	}
}
