using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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

		private static readonly Regex NewlineFinder
			= new Regex("\n", RegexOptions.Compiled);

		private static readonly Regex ExpressionAliasFinder
			= new Regex(".*(?: AS|as )(.+)", RegexOptions.Compiled);

		//private static readonly Regex FindSplitterRegEx
		//	= new Regex(
		//		@"(?!\s*$)\s*(?:'([^'\\]*(?:\\[\S\s][^'\\]*)*)'|""([^""\\]*(?:\\[\S\s][^""\\]*)*)""|([^,'""\s\\]*(?:\s+[^,'""\s\\]+)*))\s*(?:,|$)",
		//		RegexOptions.Compiled);

		//private static readonly Regex NameFinder
		//	= new Regex(@"(\[[\w]*\])",
		//		RegexOptions.Compiled);

		/// <summary>
		///     Specifies combinations of paths that don't work.
		/// </summary>
		private static readonly Regex NegativePathSpec =
			new Regex(@"([.]{4,})|([^\w./_$?~]+)|((?<![.]{2})[/])|([.]{2,}($|[^/]))",
				RegexOptions.Singleline | RegexOptions.Compiled);

		private static CharacterLocation HumanizeCharacterLocation(int characterIndex, List<int> lines)
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

			var textLocation = new CharacterLocation
			{
				//Humans count from 1, so let's do that, too (hence the "+1" on these).
				Line = line + 1,
				Character = charIdx + 1
			};
			return textLocation;
		}

		/// <summary>
		///		Defines a Match of Arguments for a Formatter
		/// </summary>
		public class HeaderTokenMatch
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="HeaderTokenMatch"/> class.
			/// </summary>
			public HeaderTokenMatch()
			{
				Arguments = new List<HeaderTokenMatch>();
			}

			/// <summary>
			///		The Parsed Argument Name as in [Name]'arg'
			/// </summary>
			public string ArgumentName { get; set; }

			/// <summary>
			///		Ether the argument constant string or the expression unparsed
			/// </summary>
			public string Value { get; set; }

			/// <summary>
			///		If value is an Expression the parsed arguments of that expression
			/// </summary>
			public List<HeaderTokenMatch> Arguments { get; set; }

			/// <summary>
			/// Gets or sets the type of the token.
			/// </summary>
			/// <value>
			/// The type of the token.
			/// </value>
			public HeaderArgumentType TokenType { get; set; }

			/// <summary>
			///		The location within the Template of the Argument
			/// </summary>
			public CharacterLocation TokenLocation { get; set; }
			internal TokenState State { get; set; }
			internal int BracketsCounter { get; set; }
		}

		/// <summary>
		/// 
		/// </summary>
		public enum HeaderArgumentType
		{
			/// <summary>
			///		Defines the Value of the <see cref="HeaderTokenMatch"/> to be the content argument
			/// </summary>
			String,

			/// <summary>
			///		Defines the Value of the <see cref="HeaderTokenMatch"/> to be an expression
			/// </summary>
			Expression
		}

		internal enum TokenState
		{
			None,
			InString,
			EscapedString,
			Expression,
			ArgumentName,
			DecideArgumentType,
			ArgumentStart
		}

		private static bool IsStringDelimiter(char formatChar)
		{
			return formatChar == '\'' || formatChar == '\"';
		}

		private static bool IsWhiteSpaceDelimiter(char formatChar)
		{
			return formatChar == '\r' || formatChar == '\r' || formatChar == '\t' || formatChar == ' ';
		}

		private static readonly Regex IsCharRegex = new Regex("\\w", RegexOptions.Compiled);

		private static bool IsExpressionChar(char formatChar)
		{
			return formatChar == '.'
				   || formatChar == '$'
				   || formatChar == '~'
				   || IsCharRegex.IsMatch(formatChar.ToString());
		}

		private static HeaderTokenMatch[] TokenizeFormatterHeader(string formatString,
					ICollection<IMorestachioError> error,
					List<int> linesOfTemplate,
					int startOfArgumentPosition)
		{
			//this COULD be made with regexes, i have made it and rejected it as it was no longer readable in any way.
			var tokenScopes = new Stack<HeaderTokenMatch>();
			tokenScopes.Push(new HeaderTokenMatch()
			{
				State = TokenState.None
			});
			char strChar = ' ';
			var argumentExpected = false;

			for (var index = 0; index < formatString.Length; index++)
			{
				var currentScope = tokenScopes.Peek();
				var formatChar = formatString[index];
				var state = currentScope.State;
				switch (state)
				{
					case TokenState.None:
						//we are at the start of an argument
						if (IsWhiteSpaceDelimiter(formatChar))
						{
							//skip any non content chars
							continue;
						}

						var argumentScope = new HeaderTokenMatch();
						currentScope.Arguments.Add(argumentScope);
						tokenScopes.Push(argumentScope);
						index--;
						argumentScope.State = TokenState.ArgumentStart;

						break;
					case TokenState.ArgumentStart:
						//we are at the start of an argument
						if (IsWhiteSpaceDelimiter(formatChar) || formatChar == ',')
						{
							//skip any non content chars
							continue;
						}

						if (formatChar == '[')
						{
							//this is an arguments name
							currentScope.State = TokenState.ArgumentName;
						}
						else
						{
							index--; //reprocess the char
							currentScope.State = TokenState.DecideArgumentType;
						}
						break;
					case TokenState.ArgumentName:
						if (formatChar != ']')
						{
							currentScope.ArgumentName += formatChar;
						}
						else
						{
							currentScope.State = TokenState.DecideArgumentType;
						}

						break;
					case TokenState.DecideArgumentType:
						//we are at the start of an argument
						if (IsWhiteSpaceDelimiter(formatChar))
						{
							//skip any non content chars
							continue;
						}

						if (IsStringDelimiter(formatChar))
						{
							strChar = formatChar;
							currentScope.State = TokenState.InString;
							argumentExpected = false;
						}
						else if (IsExpressionChar(formatChar))
						{
							currentScope.State = TokenState.Expression;
							argumentExpected = false;
							index--;
						}
						else
						{
							//this is not the start of an expression and not a string
							error.Add(new InvalidPathSyntaxError(
								HumanizeCharacterLocation(startOfArgumentPosition + currentScope.Value.Length, linesOfTemplate), currentScope.Value));
							return new HeaderTokenMatch[0];
						}
						currentScope.TokenType = currentScope.State == TokenState.InString ? HeaderArgumentType.String : HeaderArgumentType.Expression;
						break;
					case TokenState.EscapedString:
						currentScope.Value += formatChar;
						if (formatChar != '\\') // if the user has written \\ and the first \ is omitted 
						{
							// the char was escaped return to capture everything else
							currentScope.State = TokenState.InString;
						}
						break;
					case TokenState.InString:
						if (formatChar == '\\')
						{
							currentScope.State = TokenState.EscapedString;
							break;
						}
						else if (formatChar == strChar)
						{
							tokenScopes.Pop();
							break;
						}
						currentScope.Value += formatChar;

						break;
					case TokenState.Expression:
						if (formatChar == '(')
						{
							var argument = new HeaderTokenMatch();
							argument.State = TokenState.ArgumentStart;
							currentScope.Arguments.Add(argument);
							tokenScopes.Push(argument);
							currentScope.BracketsCounter++;
							break;
						}

						if (formatChar == ')')
						{
							currentScope.BracketsCounter--;
							if (index + 1 == formatString.Length)
							{
								tokenScopes.Pop();
							}

							break;
						}

						if (currentScope.BracketsCounter == 0)
						{
							if (formatChar == ',' || index + 1 == formatString.Length)
							{
								if (formatChar != ',' && !IsWhiteSpaceDelimiter(formatChar))
								{
									currentScope.Value += formatChar;
								}

								tokenScopes.Pop();
								if (formatChar == ',')
								{
									argumentExpected = true;
								}
								break;
							}
							if (!IsWhiteSpaceDelimiter(formatChar))
							{
								currentScope.Value += formatChar;
							}
						}
						else
						{
							index--; //reprocess
							var argument = new HeaderTokenMatch();
							argument.State = TokenState.ArgumentStart;
							currentScope.Arguments.Add(argument);
							tokenScopes.Push(argument);
						}
						break;
				}
			}
			if (tokenScopes.Count != 1)
			{
				error.Add(new InvalidPathSyntaxError(
					HumanizeCharacterLocation(startOfArgumentPosition + formatString.Length, linesOfTemplate), formatString));
			}
			if (argumentExpected)
			{
				error.Add(new MorestachioSyntaxError(
					HumanizeCharacterLocation(startOfArgumentPosition + formatString.Length, linesOfTemplate), formatString,
					formatString[formatString.Length - 1].ToString(),
					")", "Expected closing bracket"));
			}

			return tokenScopes.Peek().Arguments.ToArray();
		}

		private class FormatMatch
		{
			public FormatMatch()
			{
				Scalar = new StringBuilder();
				Arguments = new StringBuilder();
			}

			public int Index { get; set; }
			public string Path { get; set; }
			public StringBuilder Scalar { get; set; }
			public StringBuilder Arguments { get; set; }
		}

		private static FormatMatch[] EnumerateFormats(string input,
			List<int> lines, int tokenIndex, ICollection<IMorestachioError> parseErrors)
		{
			char? isInString = null;
			var isEscapedString = false;
			var format = new FormatMatch();
			var formats = new List<FormatMatch>();
			var bracketCounter = 0;
			for (int i = 0; i < input.Length; i++)
			{
				var inputChar = input[i];
				format.Path += inputChar;
				if (format.Arguments.Length == 0)
				{
					//currently no argument
					if (inputChar == '(')
					{
						bracketCounter++;
						format.Arguments.Append(inputChar);
					}
					else if (inputChar == ')')
					{
						bracketCounter--;
						format.Arguments.Append(inputChar);
					}
					else
					{
						//still a scalar value
						format.Scalar.Append(inputChar);
					}
				}
				else
				{
					//we are in an argument
					format.Arguments.Append(inputChar);
					if (inputChar == ')' && !isInString.HasValue)
					{
						bracketCounter--;
						if (bracketCounter == 0)
						{
							//this is the end of the argument
							formats.Add(format);
							format = new FormatMatch();
							format.Index = i + 1;
						}
						else if (bracketCounter < 0)
						{
							parseErrors.Add(new MorestachioSyntaxError(HumanizeCharacterLocation(tokenIndex + i, lines),
								"Format", input, "", "Unmatched closing bracket"));
						}
					}
					if (inputChar == '(' && !isInString.HasValue)
					{
						bracketCounter++;
					}
					else if (IsStringDelimiter(inputChar))
					{
						if (!isInString.HasValue)
						{
							//we are currently not in an string
							isInString = inputChar;
						}
						else if (!isEscapedString && inputChar == isInString.Value)
						{
							//this is no escaped string so it must be the end delimiter
							isInString = null;
						}
						else if (isEscapedString)
						{
							isEscapedString = false;
						}
					}
					else if (inputChar == '\\')
					{
						isEscapedString = true;
					}
				}
			}

			if (isInString.HasValue)
			{
				parseErrors.Add(new MorestachioSyntaxError(HumanizeCharacterLocation(tokenIndex + input.Length, lines),
					input, input[input.Length - 1].ToString(), isInString.Value.ToString(), "Unescaped argument"));
			}

			if (bracketCounter != 0)
			{
				parseErrors.Add(new MorestachioSyntaxError(HumanizeCharacterLocation(tokenIndex + input.Length, lines),
					"Format", ")", "", "Unmatched opening bracket"));
			}

			return formats.ToArray();
		}

		private static IEnumerable<TokenPair> TokenizeFormattables(
			FormatMatch[] formattables,
			string token,
			string templateString,
			List<int> lines,
			int tokenIndex,
			ICollection<IMorestachioError> parseErrors)
		{
			var tokensHandled = 0;
			var lastPosition = 0;

			//foreach (Match tokenFormats in FormatFinder.Matches(token))
			//FormatMatch[] enumerateFormats = EnumerateFormats(token, templateString, lines, tokenIndex, parseErrors);
			foreach (var tokenFormats in formattables)
			{
				var tokenArgIndex = tokenFormats.Index + tokenIndex;
				//var found = tokenFormats.Groups[0].Value;
				//var scalarValue = tokenFormats.Groups[1].Value;
				//var formatterArgument = tokenFormats.Groups[2].Value;

				var found = tokenFormats.Path;
				var scalarValue = tokenFormats.Scalar.ToString();
				var formatterArgument = tokenFormats.Arguments.ToString();

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
					var lineSeperators = new[]
					{
						'\r',
						'\n',
						'\t',
						' ',
					};
					tokensHandled += tokensInBetween.Count(f => lineSeperators.Contains(f));
				}

				lastPosition = tokenFormats.Index + tokenFormats.Path.Length;

				if (string.IsNullOrWhiteSpace(formatterArgument))
				{
					yield return new TokenPair(TokenType.Format,
						Validated(scalarValue, tokenArgIndex, lines, parseErrors),
						HumanizeCharacterLocation(tokenArgIndex, lines));
				}
				else
				{
					//trim only the last ) not each as there is maybe an expression as last argument
					var formatExpression = formatterArgument.Substring(1, formatterArgument.Length - 2);
					var formatHeader = TokenizeFormatterHeader(formatExpression, parseErrors, lines, tokenArgIndex);
					yield return new TokenPair(TokenType.Format,
						ValidateArgumentHead(scalarValue, formatterArgument, templateString,
							tokenArgIndex, lines, parseErrors), HumanizeCharacterLocation(tokenArgIndex, lines))
					{
						FormatString = formatHeader
					};
				}
			}

			if (tokensHandled != token.Length)
			{
				yield return new TokenPair(TokenType.Format,
					Validated(token.Substring(tokensHandled), tokenIndex, lines, parseErrors),
					HumanizeCharacterLocation(tokenIndex, lines));
			}
		}

		internal static IEnumerable<TokenPair> Tokenize(ParserOptions parserOptions,
			ICollection<IMorestachioError> parseErrors,
			PerformanceProfiler profiler)
		{
			return TokenizeString(parserOptions.Template, parseErrors, profiler);
		}

		internal static Tuple<string, string> EvaluateNameFromToken(string token)
		{
			var match = ExpressionAliasFinder.Match(token);
			var name = match.Groups[1].Value;
			if (!string.IsNullOrWhiteSpace(name))
			{
				return new Tuple<string, string>(token.Substring(0, token.Length - (" AS" + name).Length), name.Trim());
			}

			return new Tuple<string, string>(token, null);
		}

		private static IEnumerable<TokenPair> TokenizeArgumentsIfNecessary(
			string token,
			string templateString,
			TokenType targetType,
			List<int> lines,
			int tokenIndex,
			ICollection<IMorestachioError> parseErrors)
		{
			var formats = EnumerateFormats(token, lines, tokenIndex, parseErrors);
			var tokens = new List<TokenPair>();
			if (!formats.Any())
			{
				tokens.Add(new TokenPair(targetType,
					Validated(token, 0, lines, parseErrors).Trim(),
					HumanizeCharacterLocation(tokenIndex, lines)));
			}
			else
			{
				tokens.AddRange(TokenizeFormattables(formats, token, templateString, lines,
					tokenIndex, parseErrors));

				tokens.Add(new TokenPair(targetType, ".",
					HumanizeCharacterLocation(tokenIndex, lines)));
			}

			return tokens;
		}

		internal static IEnumerable<TokenPair> TokenizeString(string partial,
			ICollection<IMorestachioError> parseErrors,
			PerformanceProfiler profiler)
		{
			var templateString = partial;
			MatchCollection matches;
			using (profiler.Begin("Find Tokens"))
			{
				matches = TokenFinder.Matches(templateString);
			}

			var scopestack = new Stack<Tuple<string, int>>();

			var idx = 0;

			var lines = new List<int>();

			lines.AddRange(NewlineFinder.Matches(templateString).OfType<Match>().Select(k => k.Index));

			var partialsNames = new List<string>();

			var tokens = new List<TokenPair>();
			foreach (Match match in matches)
			{
				int tokenIndex = match.Index;
				//yield front content.
				if (match.Index > idx)
				{
					tokens.Add(new TokenPair(TokenType.Content, templateString.Substring(idx, match.Index - idx),
						HumanizeCharacterLocation(idx, lines)));
				}

				var tokenValue = match.Value;
				if (tokenValue.StartsWith("{{#declare", true, CultureInfo.InvariantCulture))
				{
					scopestack.Push(Tuple.Create(tokenValue, match.Index));
					var token = tokenValue.TrimStart('{').TrimEnd('}').TrimStart('#').Trim().Substring("declare".Length);
					if (string.IsNullOrWhiteSpace(token))
					{
						parseErrors.Add(new MorestachioSyntaxError(HumanizeCharacterLocation(match.Index, lines), "open", "declare", "{{#declare name}}", " Missing the Name."));
					}
					else
					{
						partialsNames.Add(token);
						tokens.Add(new TokenPair(TokenType.PartialDeclarationOpen, token, HumanizeCharacterLocation(tokenIndex, lines)));
					}
				}
				else if (tokenValue.StartsWith("{{/declare", true, CultureInfo.InvariantCulture))
				{
					if (!string.Equals(tokenValue, "{{/declare}}", StringComparison.InvariantCultureIgnoreCase))
					{
						parseErrors.Add(new MorestachioSyntaxError(HumanizeCharacterLocation(match.Index, lines), "close", "declare", "{{/declare}}"));
					}
					else if (scopestack.Any() && scopestack.Peek().Item1.StartsWith("{{#declare"))
					{
						var token = scopestack.Pop().Item1.TrimStart('{').TrimEnd('}').TrimStart('#').Trim()
							.Substring("declare".Length);
						tokens.Add(new TokenPair(TokenType.PartialDeclarationClose, token, HumanizeCharacterLocation(tokenIndex, lines)));
					}
					else
					{
						parseErrors.Add(new MorestachioUnopendScopeError(HumanizeCharacterLocation(match.Index, lines), "declare", "{{#declare name}}"));
					}
				}
				else if (tokenValue.StartsWith("{{#include", true, CultureInfo.InvariantCulture))
				{
					var token = tokenValue.TrimStart('{').TrimEnd('}').TrimStart('#').Trim().Substring("include".Length);
					if (string.IsNullOrWhiteSpace(token) || !partialsNames.Contains(token))
					{
						parseErrors.Add(new MorestachioSyntaxError(
							HumanizeCharacterLocation(match.Index, lines),
							"use",
							"include",
							"{{#include name}}",
							$" There is no Partial declared '{token}'. Partial names are case sensitive and must be declared before an include."));
					}
					else
					{
						tokens.Add(new TokenPair(TokenType.RenderPartial, token, HumanizeCharacterLocation(tokenIndex, lines)));
					}
				}
				else if (tokenValue.StartsWith("{{#each", true, CultureInfo.InvariantCulture))
				{
					var token = tokenValue.TrimStart('{').TrimEnd('}').TrimStart('#').Trim().Substring("each".Length);
					var eval = EvaluateNameFromToken(token);
					token = eval.Item1;
					var alias = eval.Item2;

					scopestack.Push(Tuple.Create($"#each{alias ?? token}", match.Index));

					if (token.StartsWith(" ") && token.Trim() != "")
					{
						token = token.Trim();
						tokens.AddRange(TokenizeArgumentsIfNecessary(token, templateString, TokenType.CollectionOpen, lines, tokenIndex, parseErrors));
					}
					else
					{
						parseErrors.Add(new InvalidPathSyntaxError(HumanizeCharacterLocation(match.Index, lines), ""));
					}

					if (!string.IsNullOrWhiteSpace(alias))
					{
						tokens.Add(new TokenPair(TokenType.Alias, alias, HumanizeCharacterLocation(match.Index + $"#each{alias}".Length, lines)));
					}
				}
				else if (tokenValue.StartsWith("{{/each", true, CultureInfo.InvariantCulture))
				{
					if (!string.Equals(tokenValue, "{{/each}}", StringComparison.InvariantCultureIgnoreCase))
					{
						parseErrors.Add(new MorestachioSyntaxError(HumanizeCharacterLocation(match.Index, lines), "close", "each", "{{/each}}"));
					}
					else if (scopestack.Any() && scopestack.Peek().Item1.StartsWith("#each"))
					{
						var token = scopestack.Pop().Item1;
						tokens.Add(new TokenPair(TokenType.CollectionClose, token, HumanizeCharacterLocation(tokenIndex, lines)));
					}
					else
					{
						parseErrors.Add(new MorestachioUnopendScopeError(HumanizeCharacterLocation(match.Index, lines), "each", "{{#each name}}"));
					}
				}
				else if (tokenValue.StartsWith("{{#if ", true, CultureInfo.InvariantCulture))
				{
					var token = tokenValue.TrimStart('{').TrimEnd('}').TrimStart('#').Trim().Substring("if".Length);
					var eval = EvaluateNameFromToken(token);
					token = eval.Item1;
					if (eval.Item2 != null)
					{
						parseErrors.Add(new MorestachioSyntaxError(
							HumanizeCharacterLocation(tokenIndex, lines), "^if", "AS", "No Alias"));
					}

					scopestack.Push(Tuple.Create($"#if{token}", match.Index));

					if (token.StartsWith(" ") && token.Trim() != "")
					{
						token = token.Trim();
						tokens.AddRange(TokenizeArgumentsIfNecessary(token, templateString, TokenType.If, lines, tokenIndex, parseErrors));
					}
					else
					{
						parseErrors.Add(new InvalidPathSyntaxError(HumanizeCharacterLocation(match.Index, lines), ""));
					}
				}
				else if (tokenValue.StartsWith("{{^if ", true, CultureInfo.InvariantCulture))
				{
					var token = tokenValue.TrimStart('{').TrimEnd('}').TrimStart('^').Trim().Substring("if".Length);
					var eval = EvaluateNameFromToken(token);
					token = eval.Item1;
					if (eval.Item2 != null)
					{
						parseErrors.Add(new MorestachioSyntaxError(
							HumanizeCharacterLocation(tokenIndex, lines), "^if", "AS", "No Alias"));
					}

					scopestack.Push(Tuple.Create($"^if{token}", match.Index));

					if (token.StartsWith(" ") && token.Trim() != "")
					{
						token = token.Trim();
						tokens.AddRange(TokenizeArgumentsIfNecessary(token, templateString, TokenType.IfNot, lines, tokenIndex, parseErrors));
					}
					else
					{
						parseErrors.Add(new InvalidPathSyntaxError(HumanizeCharacterLocation(match.Index, lines), ""));
					}
				}
				else if (tokenValue.StartsWith("{{/if", true, CultureInfo.InvariantCulture))
				{
					if (!string.Equals(tokenValue, "{{/if}}", StringComparison.InvariantCultureIgnoreCase))
					{
						parseErrors.Add(new MorestachioSyntaxError(HumanizeCharacterLocation(match.Index, lines), "close", "if", "{{/if}}"));
					}
					else
					{
						if (!scopestack.Any())
						{
							parseErrors.Add(new MorestachioUnopendScopeError(HumanizeCharacterLocation(match.Index, lines), "if", "{{#if name}}"));
						}
						else
						{
							var item1 = scopestack.Peek().Item1;
							if (item1.StartsWith("#if") || item1.StartsWith("^if"))
							{
								var token = scopestack.Pop().Item1;
								tokens.Add(new TokenPair(TokenType.IfClose, token, HumanizeCharacterLocation(tokenIndex, lines)));
							}
							else
							{
								parseErrors.Add(new MorestachioUnopendScopeError(HumanizeCharacterLocation(match.Index, lines), "if", "{{#if name}}"));
							}
						}
					}
				}
				else if (tokenValue.Equals("{{#else}}", StringComparison.InvariantCultureIgnoreCase))
				{
					var firstNonContentToken = tokens
						.AsReadOnly()
						.Reverse()
						.FirstOrDefault(e => e.Type != TokenType.Content);
					if (firstNonContentToken == null || firstNonContentToken.Type != TokenType.IfClose)
					{
						parseErrors
							.Add(new MorestachioSyntaxError(
								HumanizeCharacterLocation(match.Index, lines), "find if block for else",
								firstNonContentToken?.Value, "{{/if}}", "Could not find an /if block for this else"));
					}
					else
					{
						scopestack.Push(Tuple.Create($"#else_{firstNonContentToken.Value}", match.Index));
						tokens.Add(new TokenPair(TokenType.Else, firstNonContentToken.Value, HumanizeCharacterLocation(match.Index, lines)));
					}
				}
				else if (tokenValue.Equals("{{/else}}", StringComparison.InvariantCultureIgnoreCase))
				{
					if (!string.Equals(tokenValue, "{{/else}}", StringComparison.InvariantCultureIgnoreCase))
					{
						parseErrors.Add(new MorestachioSyntaxError(HumanizeCharacterLocation(match.Index, lines), "close", "else", "{{/else}}"));
					}
					else
					{
						if (scopestack.Any() && (scopestack.Peek().Item1.StartsWith("#else_")))
						{
							var token = scopestack.Pop().Item1;
							tokens.Add(new TokenPair(TokenType.ElseClose, token, HumanizeCharacterLocation(tokenIndex, lines)));
						}
						else
						{
							parseErrors.Add(new MorestachioUnopendScopeError(HumanizeCharacterLocation(match.Index, lines), "else", "{{#else name}}"));
						}
					}
				}
				else if (tokenValue.StartsWith("{{#"))
				{
					//open group
					var token = tokenValue.TrimStart('{').TrimEnd('}').TrimStart('#').Trim();

					var eval = EvaluateNameFromToken(token);
					token = eval.Item1;
					var alias = eval.Item2;
					scopestack.Push(Tuple.Create(alias ?? token, match.Index));

					//if (scopestack.Any() && scopestack.Peek().Item1 == token)
					//{
					//	tokens.Add(new TokenPair(TokenType.ElementClose,
					//		Validated(token, match.Index, lines, parseErrors), HumanizeCharacterLocation(tokenIndex, lines)));
					//}
					//else
					//{
					//	scopestack.Push(Tuple.Create(alias ?? token, match.Index));
					//}

					tokens.AddRange(TokenizeArgumentsIfNecessary(token, templateString, TokenType.ElementOpen, lines, tokenIndex, parseErrors));

					if (!string.IsNullOrWhiteSpace(alias))
					{
						tokens.Add(new TokenPair(TokenType.Alias, alias, HumanizeCharacterLocation(match.Index + token.Length, lines)));
					}
				}
				else if (tokenValue.StartsWith("{{^"))
				{
					//open inverted group
					var token = tokenValue.TrimStart('{').TrimEnd('}').TrimStart('^').Trim();
					var eval = EvaluateNameFromToken(token);
					token = eval.Item1;
					var alias = eval.Item2;
					scopestack.Push(Tuple.Create(alias ?? token, match.Index));

					//if (scopestack.Any() && scopestack.Peek().Item1 == token)
					//{
					//	tokens.Add(new TokenPair(TokenType.ElementClose,
					//		Validated(token, match.Index, lines, parseErrors), HumanizeCharacterLocation(tokenIndex, lines)));
					//}
					//else
					//{
					//	scopestack.Push(Tuple.Create(alias ?? token, match.Index));
					//}

					tokens.AddRange(TokenizeArgumentsIfNecessary(token, templateString, TokenType.InvertedElementOpen, lines, tokenIndex, parseErrors));

					if (!string.IsNullOrWhiteSpace(alias))
					{
						tokens.Add(new TokenPair(TokenType.Alias, alias,
							HumanizeCharacterLocation(match.Index + $"#each{alias}".Length, lines)));
					}
				}
				else if (tokenValue.StartsWith("{{/"))
				{
					var token = tokenValue.TrimStart('{').TrimEnd('}').TrimStart('/').Trim();
					//close group
					if (scopestack.Any() && scopestack.Peek().Item1 == token)
					{
						scopestack.Pop();
						tokens.Add(new TokenPair(TokenType.ElementClose, token, HumanizeCharacterLocation(tokenIndex, lines)));
					}
					else
					{
						parseErrors.Add(new MorestachioUnopendScopeError(HumanizeCharacterLocation(match.Index, lines), "/", "{{#path}}", " There are more closing elements then open."));
					}
				}
				else if (tokenValue.StartsWith("{{{") || tokenValue.StartsWith("{{&"))
				{
					//escaped single element
					var token = tokenValue.TrimStart('{').TrimEnd('}').TrimStart('&').Trim();

					var formats = EnumerateFormats(token, lines, tokenIndex, parseErrors);
					if (!formats.Any())
					{
						tokens.Add(new TokenPair(TokenType.UnescapedSingleValue,
							Validated(token, 0, lines, parseErrors).Trim(),
							HumanizeCharacterLocation(tokenIndex, lines)));
					}
					else
					{
						tokens.AddRange(TokenizeFormattables(formats, token, templateString, lines,
							tokenIndex, parseErrors));

						tokens.Add(new TokenPair(TokenType.Print, ".",
							HumanizeCharacterLocation(tokenIndex, lines)));
					}
				}
				else if (tokenValue.StartsWith("{{!"))
				{
					//it's a comment drop this on the floor, no need to even yield it.
				}
				else if (tokenValue.StartsWith("#") || tokenValue.StartsWith("/"))
				{
					//catch expression handler
					parseErrors.Add(new MorestachioSyntaxError(HumanizeCharacterLocation(match.Index, lines),
						$"Unexpected token. Expected an valid Expression but got '{tokenValue}'", tokenValue, ""));
				}
				else
				{
					//unsingle value.
					var token = tokenValue.TrimStart('{').TrimEnd('}').Trim();

					var formats = EnumerateFormats(token, lines, tokenIndex, parseErrors);
					if (!formats.Any())
					{
						tokens.Add(new TokenPair(TokenType.EscapedSingleValue,
							Validated(token, 0, lines, parseErrors).Trim(),
							HumanizeCharacterLocation(tokenIndex, lines)));
					}
					else
					{
						tokens.AddRange(TokenizeFormattables(formats, token, templateString, lines,
							tokenIndex, parseErrors));

						tokens.Add(new TokenPair(TokenType.Print, ".",
							HumanizeCharacterLocation(tokenIndex, lines)));
					}
				}

				//move forward in the string.
				idx = match.Index + match.Length;
			}

			if (idx < templateString.Length)
			{
				tokens.Add(new TokenPair(TokenType.Content, templateString.Substring(idx), HumanizeCharacterLocation(idx, lines)));
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
						location = HumanizeCharacterLocation(k.Item2, lines)
					};
				}).Reverse())
				{
					parseErrors.Add(new MorestachioUnopendScopeError(unclosedScope.location, unclosedScope.scope, ""));
				}
			}

			return tokens;

			#endregion

			////We want to throw an aggregate template exception, but in due time.
			//if (!parseErrors.Any())
			//{
			//	yield break;
			//}

			//var innerExceptions = parseErrors.OrderBy(k => k.LineNumber).ThenBy(k => k.CharacterOnLine).ToArray();
			//throw new AggregateException(innerExceptions);
		}

		private static string Validated(string token, int index, List<int> lines,
			ICollection<IMorestachioError> exceptions)
		{
			token = token.Trim();

			if (!NegativePathSpec.Match(token).Success)
			{
				return token;
			}

			exceptions.Add(new InvalidPathSyntaxError(HumanizeCharacterLocation(index, lines), token));

			return token;
		}

		private static string ValidateArgumentHead(string token, string argument, string content,
			int index, List<int> lines, ICollection<IMorestachioError> exceptions)
		{
			token = token.Trim();

			Validated(token, index, lines, exceptions);

			//if (!PositiveArgumentSpec.Match(argument).Success)
			//{
			//	var location = HumanizeCharacterLocation(content, index, lines);
			//	exceptions.Add(new IndexedParseException(location,
			//		"The argument '{0}' is not valid. Please see documentation for examples of valid paths.", token));
			//}

			return token;
		}

		/// <summary>
		///		Describes an Position within the Template
		/// </summary>
		public class CharacterLocation : IEquatable<CharacterLocation>
		{
			/// <summary>
			///		The line of the Template
			/// </summary>
			public int Line { get; set; }

			/// <summary>
			///		The Character at the <see cref="Line"/>
			/// </summary>
			public int Character { get; set; }

			public string ToFormatString()
			{
				return $"{Line}:{Character}";
			}

			public static CharacterLocation FromFormatString(string formatString)
			{
				if (!formatString.Contains(":"))
				{
					return null;
				}

				var parts = formatString.Split(':');
				var charLoc = new CharacterLocation();
				charLoc.Line = int.Parse(parts[0]);
				charLoc.Character = int.Parse(parts[1]);
				return charLoc;
			}

			public override string ToString()
			{
				return $"Line: {Line}, Column: {Character}";
			}

			public bool Equals(CharacterLocation other)
			{
				if (ReferenceEquals(null, other))
				{
					return false;
				}

				if (ReferenceEquals(this, other))
				{
					return true;
				}

				return Line == other.Line && Character == other.Character;
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj))
				{
					return false;
				}

				if (ReferenceEquals(this, obj))
				{
					return true;
				}

				if (obj.GetType() != this.GetType())
				{
					return false;
				}

				return Equals((CharacterLocation) obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return (Line * 397) ^ Character;
				}
			}
		}
	}
}