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
			var tokens = new List<TokenPair>();

			//var formats = Tokenizer.EvaluateFormattables(
			//	Tokenizer.EnumerateFormats(expression,lines,tokenIndex,parseErrors),
			//	expression, 
			//	null,
			//	lines, 
			//	tokenIndex, 
			//	parseErrors)
			//	.ToArray();
			//if (formats.Length == 0)
			//{
			//	parseErrors.Add(new MorestachioSyntaxError(
			//		tokenLocation.AddWindow(new CharacterSnippedLocation(0, "#var ".Length, tokenValue)),
			//		"#var", "", "#var name = expression", "Expected at least one expression or constant but got none"));
			//}

			//var headerTokenMatches = Tokenizer.TokenizeExpression(expression,
			//	null,
			//	lines,
			//	parseErrors,
			//	tokenIndex,
			//	expression,
			//	out var formatterName);
			tokens.Add(new TokenPair(TokenType.VariableDeclaration, variableName, tokenLocation)
			{
			});

			var formats = Tokenizer.EnumerateFormats(expression, lines, tokenIndex, parseErrors);
			if (!formats.Any())
			{
				tokens.Add(new TokenPair(TokenType.EscapedSingleValue,
					Tokenizer.Validated(expression, tokenIndex, lines, parseErrors),
					Tokenizer.HumanizeCharacterLocation(tokenIndex, lines)));
			}
			else
			{
				tokens.AddRange(Tokenizer.TokenizeFormattables(formats, expression, null, lines, tokenIndex, parseErrors, options));
			}

			tokens.Add(new TokenPair(TokenType.VariableSet, variableName, tokenLocation));
			return tokens.ToArray();
		}
	}
}
