using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Morestachio.Formatter;

namespace Morestachio.Framework
{
	/// <summary>
	///     Reads in a mustache template and lexes it into tokens.
	/// </summary>
	/// <exception cref="IndexedParseException"></exception>
	public class Tokenizer
	{
		private static readonly Regex TokenFinder = new Regex("([{]{2}[^{}]+?[}]{2})|([{]{3}[^{}]+?[}]{3})",
			RegexOptions.Compiled);

		private static readonly Regex FormatFinder
			= new Regex(@"(?:([\w.\s|$]+)*)+(\((?>\((?<c>)|[^()]+|\)(?<-c>))*(?(c)(?!))\))"
				, RegexOptions.Compiled);

		private static readonly Regex FormatInExpressionFinder
			= new Regex(@"(\((?>\((?<c>)|[^()]+|\)(?<-c>))*(?(c)(?!))\))"
				, RegexOptions.Compiled);

		private static readonly Regex NewlineFinder
			= new Regex("\n", RegexOptions.Compiled);

		private static readonly Regex FindSplitterRegEx
			= new Regex(
				@"(?!\s*$)\s*(?:'([^'\\]*(?:\\[\S\s][^'\\]*)*)'|""([^""\\]*(?:\\[\S\s][^""\\]*)*)""|([^,'""\s\\]*(?:\s+[^,'""\s\\]+)*))\s*(?:,|$)",
				RegexOptions.Compiled);

		private static readonly Regex NameFinder
			= new Regex(@"(\[[\w]*\])",
				RegexOptions.Compiled);

		/// <summary>
		///     Specifies combinations of paths that don't work.
		/// </summary>
		private static readonly Regex NegativePathSpec =
			new Regex(@"([.]{4,})|([^\w./_$?]+)|((?<![.]{2})[/])|([.]{2,}($|[^/]))",
				RegexOptions.Singleline | RegexOptions.Compiled);

		private static CharacterLocation HumanizeCharacterLocation(string content, int characterIndex, List<int> lines)
		{
			var line = Array.BinarySearch(lines.ToArray(), characterIndex);
			line = line < 0 ? ~line : line;

			var charIdx = characterIndex;
			//in both of these cases, we want to increment the char index by one to account for the '\n' that is skipped in the indexes.
			if (line < lines.Count && line > 0)
			{
				charIdx = characterIndex - (lines[line - 1] + 1);
			}
			else if (line > 0)
			{
				charIdx = characterIndex - (lines.LastOrDefault() + 1);
			}

			var retval = new CharacterLocation
			{
				//Humans count from 1, so let's do that, too (hence the "+1" on these).
				Line = line + 1,
				Character = charIdx + 1
			};
			return retval;
		}

		private static FormatterToken[] TokenizeFormatterHeader(string formatString, ICollection<IMorestachioError> error)
		{
			var preMatch = 0;
			return FindSplitterRegEx
				.Matches(formatString)
				.OfType<Match>()
				.Select(e =>
				{
					var indexOfEndMatch = e.Groups[0].Captures[0].Index + e.Groups[0].Captures[0].Length; //get everything from the index of the regex to its end
					var formatterArgument = formatString.Substring(preMatch, indexOfEndMatch - preMatch);
					var name = NameFinder.Match(formatterArgument); //find the optional [Name] attribute on the formatters argument
					preMatch = indexOfEndMatch;
					var argument = formatterArgument.Remove(name.Index, name.Value.Length)
						.Trim()
						//trim all commas from the formatter
						.Trim(',')
						//then trim all spaces that the user might have written
						.Trim();
					return new FormatterToken(name.Value.Trim('[', ']'), new FormatExpression()
					{
						OrigialString = argument,
						ParsedArguments = TokenizeFormatterArgument(argument, error),
					});
				})
				.Where(e => !string.IsNullOrWhiteSpace(e.Argument.OrigialString))
				.ToArray();
		}

		private static IFormatterArgumentType TokenizeFormatterArgument(string argument,
			ICollection<IMorestachioError> parseErrors)
		{
			argument = argument.Trim();

			if ((argument.StartsWith("\"") && argument.EndsWith("\"")) || (argument.StartsWith("'") && argument.EndsWith("'")))
			{
				//the argument starts and ends with an " or ' so it must be constant expression.
				return new ConstFormatterArgumentValue(argument.Trim('"', '\''));
			}

			//the argument does not start with a string keyword so thread them as expression

			return new PathFormatterArgumentValue(TokenizePath(parseErrors, argument, "", new List<int>(), 0).ToArray());
		}

		private static IEnumerable<TokenPair> TokenizeFormattables(string token, string templateString,
			List<int> lines, int tokenIndex, ICollection<IMorestachioError> parseErrors)
		{
			var tokensHandled = 0;
			var lastPosition = 0;

			foreach (Match tokenFormats in FormatFinder.Matches(token))
			{
				var tokenArgIndex = tokenFormats.Index + tokenIndex;
				var found = tokenFormats.Groups[0].Value;
				var scalarValue = tokenFormats.Groups[1].Value;
				var formatterArgument = tokenFormats.Groups[2].Value;

				if (string.IsNullOrEmpty(scalarValue))
				{
					continue;
				}

				tokensHandled += found.Length;
				if (lastPosition != 0)
				{
					//this is for handling /r/n/t between arguments like
					//path.
					//(test)
					var tokensInBetween = token.Substring(lastPosition, tokenFormats.Index - lastPosition);
					var lineSeperators = new char[]
					{
						'\r',
						'\n',
						'\t',
						' ',
					};
					tokensHandled += tokensInBetween.Count(f => lineSeperators.Contains(f));
				}

				lastPosition = tokenFormats.Index + tokenFormats.Length;

				if (string.IsNullOrWhiteSpace(formatterArgument))
				{
					yield return new TokenPair(TokenType.Format,
						Validated(scalarValue, templateString, tokenArgIndex, lines, parseErrors),
						HumanizeCharacterLocation(templateString, tokenArgIndex, lines));
				}
				else
				{
					//trim only the last ) not each as there is maybe an expression as last argument
					var formatExpression = formatterArgument.Substring(1, formatterArgument.Length - 2);


					var formatHeader = TokenizeFormatterHeader(formatExpression, parseErrors);

					yield return new TokenPair(TokenType.Format,
						ValidateArgumentHead(scalarValue, formatterArgument, templateString,
							tokenArgIndex, lines, parseErrors), HumanizeCharacterLocation(templateString, tokenArgIndex, lines))
					{
						FormatString = formatHeader
					};
				}
			}

			if (tokensHandled != token.Length)
			{
				yield return new TokenPair(TokenType.Format,
					Validated(token.Substring(tokensHandled), templateString, tokenIndex, lines, parseErrors),
					HumanizeCharacterLocation(templateString, tokenIndex, lines));
			}
		}

		internal static IEnumerable<TokenPair> Tokenize(ParserOptions parserOptions, ICollection<IMorestachioError> parseErrors)
		{
			return TokenizeString(parserOptions.Template, parseErrors);
		}

		internal static IEnumerable<TokenPair> TokenizeString(string partial,
			ICollection<IMorestachioError> parseErrors)
		{
			var templateString = partial;
			var matches = TokenFinder.Matches(templateString);
			var scopestack = new Stack<Tuple<string, int>>();

			var idx = 0;

			var lines = new List<int>();

			lines.AddRange(NewlineFinder.Matches(templateString).OfType<Match>().Select(k => k.Index));

			var partialsNames = new List<string>();

			foreach (Match m in matches)
			{
				int tokenIndex = m.Index;
				//yield front content.
				if (m.Index > idx)
				{
					yield return new TokenPair(TokenType.Content, templateString.Substring(idx, m.Index - idx), HumanizeCharacterLocation(templateString, tokenIndex, lines));
				}

				if (m.Value.StartsWith("{{#declare"))
				{
					scopestack.Push(Tuple.Create(m.Value, m.Index));
					var token = m.Value.TrimStart('{').TrimEnd('}').TrimStart('#').Trim().Substring("declare".Length);
					if (string.IsNullOrWhiteSpace(token))
					{
						parseErrors.Add(new MorestachioSyntaxError(HumanizeCharacterLocation(templateString, m.Index, lines), "open", "declare", "{{#declare name}}", " Missing the Name."));
					}
					else
					{
						partialsNames.Add(token);
						yield return new TokenPair(TokenType.PartialDeclarationOpen, token, HumanizeCharacterLocation(templateString, tokenIndex, lines));
					}
				}
				else if (m.Value.StartsWith("{{/declare"))
				{
					if (m.Value != "{{/declare}}")
					{
						parseErrors.Add(new MorestachioSyntaxError(HumanizeCharacterLocation(templateString, m.Index, lines), "close", "declare", "{{/declare}}"));
					}
					else if (scopestack.Any() && scopestack.Peek().Item1.StartsWith("{{#declare"))
					{
						var token = scopestack.Pop().Item1.TrimStart('{').TrimEnd('}').TrimStart('#').Trim()
							.Substring("declare".Length);
						yield return new TokenPair(TokenType.PartialDeclarationClose, token, HumanizeCharacterLocation(templateString, tokenIndex, lines));
					}
					else
					{
						parseErrors.Add(new MorestachioUnopendScopeError(HumanizeCharacterLocation(templateString, m.Index, lines), "declare", "{{#declare name}}"));
					}
				}
				else if (m.Value.StartsWith("{{#include"))
				{
					var token = m.Value.TrimStart('{').TrimEnd('}').TrimStart('#').Trim().Substring("include".Length);
					if (string.IsNullOrWhiteSpace(token) || !partialsNames.Contains(token))
					{
						parseErrors.Add(new MorestachioSyntaxError(
							HumanizeCharacterLocation(templateString, m.Index, lines),
							"use",
							"include",
							"{{#include name}}",
							$" There is no Partial declared '{token}'. Partial names are case sensitive and must be declared before an include."));
					}
					else
					{
						yield return new TokenPair(TokenType.RenderPartial, token, HumanizeCharacterLocation(templateString, tokenIndex, lines));
					}
				}
				else if (m.Value.StartsWith("{{#each"))
				{
					scopestack.Push(Tuple.Create(m.Value, m.Index));
					var token = m.Value.TrimStart('{').TrimEnd('}').TrimStart('#').Trim().Substring("each".Length);

					if (token.StartsWith(" ") && token.Trim() != "")
					{
						token = token.Trim();
						if (FormatInExpressionFinder.IsMatch(token))
						{
							foreach (var tokenizeFormattable in TokenizeFormattables(token, templateString, lines,
								tokenIndex, parseErrors))
							{
								yield return tokenizeFormattable;
							}

							yield return new TokenPair(TokenType.CollectionOpen, ".", HumanizeCharacterLocation(templateString, tokenIndex, lines));
						}
						else
						{
							yield return new TokenPair(TokenType.CollectionOpen,
								Validated(token, templateString, m.Index, lines, parseErrors).Trim(), HumanizeCharacterLocation(templateString, tokenIndex, lines));
						}
					}
					else
					{
						parseErrors.Add(new InvalidPathSyntaxError(HumanizeCharacterLocation(templateString, m.Index, lines), ""));
					}
				}
				else if (m.Value.StartsWith("{{/each"))
				{
					if (m.Value != "{{/each}}")
					{
						parseErrors.Add(new MorestachioSyntaxError(HumanizeCharacterLocation(templateString, m.Index, lines), "close", "each", "{{/each}}"));
					}
					else if (scopestack.Any() && scopestack.Peek().Item1.StartsWith("{{#each"))
					{
						var token = scopestack.Pop().Item1;
						yield return new TokenPair(TokenType.CollectionClose, token, HumanizeCharacterLocation(templateString, tokenIndex, lines));
					}
					else
					{
						parseErrors.Add(new MorestachioUnopendScopeError(HumanizeCharacterLocation(templateString, m.Index, lines), "each", "{{#each name}}"));
					}
				}
				else if (m.Value.StartsWith("{{#"))
				{
					//open group
					var token = m.Value.TrimStart('{').TrimEnd('}').TrimStart('#').Trim();
					if (scopestack.Any() && scopestack.Peek().Item1 == token)
					{
						yield return new TokenPair(TokenType.ElementClose,
							Validated(token, templateString, m.Index, lines, parseErrors), HumanizeCharacterLocation(templateString, tokenIndex, lines));
					}
					else
					{
						scopestack.Push(Tuple.Create(token, m.Index));
					}

					if (FormatInExpressionFinder.IsMatch(token))
					{
						foreach (var tokenizeFormattable in TokenizeFormattables(token, templateString, lines, tokenIndex,
							parseErrors))
						{
							yield return tokenizeFormattable;
						}

						yield return new TokenPair(TokenType.ElementOpen, ".", HumanizeCharacterLocation(templateString, tokenIndex, lines));
					}
					else
					{
						yield return new TokenPair(TokenType.ElementOpen,
							Validated(token, templateString, m.Index, lines, parseErrors), HumanizeCharacterLocation(templateString, tokenIndex, lines));
					}
				}
				else if (m.Value.StartsWith("{{^"))
				{
					//open inverted group
					var token = m.Value.TrimStart('{').TrimEnd('}').TrimStart('^').Trim();

					if (scopestack.Any() && scopestack.Peek().Item1 == token)
					{
						yield return new TokenPair(TokenType.ElementClose,
							Validated(token, templateString, m.Index, lines, parseErrors), HumanizeCharacterLocation(templateString, tokenIndex, lines));
					}
					else
					{
						scopestack.Push(Tuple.Create(token, m.Index));
					}

					if (FormatInExpressionFinder.IsMatch(token))
					{
						foreach (var tokenizeFormattable in TokenizeFormattables(token, templateString, lines, tokenIndex,
							parseErrors))
						{
							yield return tokenizeFormattable;
						}

						yield return new TokenPair(TokenType.InvertedElementOpen, ".", HumanizeCharacterLocation(templateString, tokenIndex, lines));
					}
					else
					{
						yield return new TokenPair(TokenType.InvertedElementOpen,
							Validated(token, templateString, m.Index, lines, parseErrors), HumanizeCharacterLocation(templateString, tokenIndex, lines));
					}
				}
				else if (m.Value.StartsWith("{{/"))
				{
					var token = m.Value.TrimStart('{').TrimEnd('}').TrimStart('/').Trim();
					//close group
					if (scopestack.Any() && scopestack.Peek().Item1 == token)
					{
						scopestack.Pop();
						yield return new TokenPair(TokenType.ElementClose, token, HumanizeCharacterLocation(templateString, tokenIndex, lines));
					}
					else
					{
						parseErrors.Add(new MorestachioUnopendScopeError(HumanizeCharacterLocation(templateString, m.Index, lines), "/", "{{#path}}", " There are more closing elements then open."));
					}
				}
				else if (m.Value.StartsWith("{{{") | m.Value.StartsWith("{{&"))
				{
					//escaped single element
					var token = m.Value.TrimStart('{').TrimEnd('}').TrimStart('&').Trim();
					yield return new TokenPair(TokenType.UnescapedSingleValue,
						Validated(token, templateString, m.Index, lines, parseErrors), HumanizeCharacterLocation(templateString, tokenIndex, lines));
				}
				else if (m.Value.StartsWith("{{!"))
				{
					//it's a comment drop this on the floor, no need to even yield it.
				}
				else
				{
					foreach (var tokenPair in TokenizePath(parseErrors, m.Value, templateString, lines, tokenIndex))
					{
						yield return tokenPair;
					}
				}

				//move forward in the string.
				idx = m.Index + m.Length;
			}

			if (idx < templateString.Length)
			{
				yield return new TokenPair(TokenType.Content, templateString.Substring(idx), HumanizeCharacterLocation(templateString, idx, lines));
			}

			#region Assert that any scopes opened must be closed.

			if (scopestack.Any())
			{
				foreach (var unclosedScope in scopestack.Select(k =>
				{
					var value = k.Item1.Trim('{', '#', '}');
					if (value.StartsWith("each "))
					{
						value = value.Substring(5);
					}

					return new
					{
						scope = value,
						location = HumanizeCharacterLocation(templateString, k.Item2, lines)
					};
				}).Reverse())
				{
					parseErrors.Add(new MorestachioUnopendScopeError(unclosedScope.location, unclosedScope.scope, ""));
				}
			}

			yield break;

			#endregion

			////We want to throw an aggregate template exception, but in due time.
			//if (!parseErrors.Any())
			//{
			//	yield break;
			//}

			//var innerExceptions = parseErrors.OrderBy(k => k.LineNumber).ThenBy(k => k.CharacterOnLine).ToArray();
			//throw new AggregateException(innerExceptions);
		}

		private static IEnumerable<TokenPair> TokenizePath(ICollection<IMorestachioError> parseErrors, string path, string templateString, List<int> lines,
			int tokenIndex)
		{
			//unsingle value.
			var token = path.TrimStart('{').TrimEnd('}').Trim();
			if (FormatInExpressionFinder.IsMatch(token))
			{
				foreach (var tokenizeFormattable in TokenizeFormattables(token, templateString, lines, tokenIndex,
					parseErrors).ToArray())
				{
					yield return tokenizeFormattable;
				}

				yield return new TokenPair(TokenType.PrintFormatted, ".",
					HumanizeCharacterLocation(templateString, tokenIndex, lines));
			}
			else
			{
				yield return new TokenPair(TokenType.EscapedSingleValue,
					Validated(token, templateString, tokenIndex, lines, parseErrors),
					HumanizeCharacterLocation(templateString, tokenIndex, lines));
			}
		}

		private static string Validated(string token, string content, int index, List<int> lines,
			ICollection<IMorestachioError> exceptions)
		{
			token = token.Trim();

			if (!NegativePathSpec.Match(token).Success)
			{
				return token;
			}

			exceptions.Add(new InvalidPathSyntaxError(HumanizeCharacterLocation(content, index, lines), token));

			return token;
		}

		private static string ValidateArgumentHead(string token, string argument, string content,
			int index, List<int> lines, ICollection<IMorestachioError> exceptions)
		{
			token = token.Trim();

			Validated(token, content, index, lines, exceptions);

			//if (!PositiveArgumentSpec.Match(argument).Success)
			//{
			//	var location = HumanizeCharacterLocation(content, index, lines);
			//	exceptions.Add(new IndexedParseException(location,
			//		"The argument '{0}' is not valid. Please see documentation for examples of valid paths.", token));
			//}

			return token;
		}

		public class CharacterLocation
		{
			public int Line { get; set; }
			public int Character { get; set; }

			public override string ToString()
			{
				return $"Line: {Line}, Column: {Character}";
			}
		}
	}
}