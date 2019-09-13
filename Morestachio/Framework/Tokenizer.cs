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
			= new Regex(@"(?:([\w.\s|$|/|~]+)*)+(\((?>\((?<c>)|[^()]+|\)(?<-c>))*(?(c)(?!))\))"
				, RegexOptions.Compiled, TimeSpan.FromSeconds(10));

		private static readonly Regex FormatInExpressionFinder
			= new Regex(@"(\((?>\((?<c>)|[^()]+|\)(?<-c>))*(?(c)(?!))\))"
				, RegexOptions.Compiled, TimeSpan.FromSeconds(10));

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

			var retval = new CharacterLocation
			{
				//Humans count from 1, so let's do that, too (hence the "+1" on these).
				Line = line + 1,
				Character = charIdx + 1
			};
			return retval;
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

		private static Regex _isCharRegex = new Regex("\\w", RegexOptions.Compiled);

		private static bool IsExpressionChar(char formatChar)
		{
			return formatChar == '.'
				   || formatChar == '$'
				   || formatChar == '~'
				   || _isCharRegex.IsMatch(formatChar.ToString());
		}

		class FormatMatch
		{
			public string Path { get; set; }
			public HeaderTokenMatch[] Format { get; set; }
		}

		private static readonly Regex AllowedPathChars
			= new Regex(@"([\w.\s|$|/|~]+)", RegexOptions.Compiled);


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
						}
						else if (IsExpressionChar(formatChar))
						{
							currentScope.State = TokenState.Expression;
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
								var parent = tokenScopes.Peek();
								currentScope.State = TokenState.None;
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

			return tokenScopes.Peek().Arguments.ToArray();
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
					Validated(token.Substring(tokensHandled), templateString, tokenIndex, lines, parseErrors),
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

			return new Tuple<string, string>(token, token);
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
			foreach (Match m in matches)
			{
				int tokenIndex = m.Index;
				//yield front content.
				if (m.Index > idx)
				{
					tokens.Add(new TokenPair(TokenType.Content, templateString.Substring(idx, m.Index - idx), HumanizeCharacterLocation(tokenIndex, lines)));
				}

				if (m.Value.StartsWith("{{#declare", true, CultureInfo.InvariantCulture))
				{
					scopestack.Push(Tuple.Create(m.Value, m.Index));
					var token = m.Value.TrimStart('{').TrimEnd('}').TrimStart('#').Trim().Substring("declare".Length);
					if (string.IsNullOrWhiteSpace(token))
					{
						parseErrors.Add(new MorestachioSyntaxError(HumanizeCharacterLocation(m.Index, lines), "open", "declare", "{{#declare name}}", " Missing the Name."));
					}
					else
					{
						partialsNames.Add(token);
						tokens.Add(new TokenPair(TokenType.PartialDeclarationOpen, token, HumanizeCharacterLocation(tokenIndex, lines)));
					}
				}
				else if (m.Value.StartsWith("{{/declare", true, CultureInfo.InvariantCulture))
				{
					if (!string.Equals(m.Value, "{{/declare}}", StringComparison.InvariantCultureIgnoreCase))
					{
						parseErrors.Add(new MorestachioSyntaxError(HumanizeCharacterLocation(m.Index, lines), "close", "declare", "{{/declare}}"));
					}
					else if (scopestack.Any() && scopestack.Peek().Item1.StartsWith("{{#declare"))
					{
						var token = scopestack.Pop().Item1.TrimStart('{').TrimEnd('}').TrimStart('#').Trim()
							.Substring("declare".Length);
						tokens.Add(new TokenPair(TokenType.PartialDeclarationClose, token, HumanizeCharacterLocation(tokenIndex, lines)));
					}
					else
					{
						parseErrors.Add(new MorestachioUnopendScopeError(HumanizeCharacterLocation(m.Index, lines), "declare", "{{#declare name}}"));
					}
				}
				else if (m.Value.StartsWith("{{#include", true, CultureInfo.InvariantCulture))
				{
					var token = m.Value.TrimStart('{').TrimEnd('}').TrimStart('#').Trim().Substring("include".Length);
					if (string.IsNullOrWhiteSpace(token) || !partialsNames.Contains(token))
					{
						parseErrors.Add(new MorestachioSyntaxError(
							HumanizeCharacterLocation(m.Index, lines),
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
				else if (m.Value.StartsWith("{{#each", true, CultureInfo.InvariantCulture))
				{
					var token = m.Value.TrimStart('{').TrimEnd('}').TrimStart('#').Trim().Substring("each".Length);
					var eval = EvaluateNameFromToken(token);
					token = eval.Item1;
					var alias = eval.Item2;

					scopestack.Push(Tuple.Create($"#each{alias}", m.Index));

					if (token.StartsWith(" ") && token.Trim() != "")
					{
						token = token.Trim();
						if (FormatInExpressionFinder.IsMatch(token))
						{
							tokens.AddRange(TokenizeFormattables(token, templateString, lines,
								tokenIndex, parseErrors));

							tokens.Add(new TokenPair(TokenType.CollectionOpen, ".", HumanizeCharacterLocation(tokenIndex, lines)));
						}
						else
						{
							tokens.Add(new TokenPair(TokenType.CollectionOpen,
								Validated(token, templateString, m.Index, lines, parseErrors).Trim(), HumanizeCharacterLocation(tokenIndex, lines)));
						}
					}
					else
					{
						parseErrors.Add(new InvalidPathSyntaxError(HumanizeCharacterLocation(m.Index, lines), ""));
					}

					if (!string.IsNullOrWhiteSpace(alias))
					{
						tokens.Add(new TokenPair(TokenType.Alias, alias, HumanizeCharacterLocation(m.Index + $"#each{alias}".Length, lines)));
					}
				}
				else if (m.Value.StartsWith("{{#if ", true, CultureInfo.InvariantCulture))
				{
					var token = m.Value.TrimStart('{').TrimEnd('}').TrimStart('#').Trim().Substring("if".Length);
					var eval = EvaluateNameFromToken(token);
					token = eval.Item1;
					var alias = eval.Item2;

					scopestack.Push(Tuple.Create($"#if{alias}", m.Index));

					if (token.StartsWith(" ") && token.Trim() != "")
					{
						token = token.Trim();
						if (FormatInExpressionFinder.IsMatch(token))
						{
							tokens.AddRange(TokenizeFormattables(token, templateString, lines,
								tokenIndex, parseErrors));

							tokens.Add(new TokenPair(TokenType.If, ".", HumanizeCharacterLocation(tokenIndex, lines)));
						}
						else
						{
							tokens.Add(new TokenPair(TokenType.If,
								Validated(token, templateString, m.Index, lines, parseErrors).Trim(), HumanizeCharacterLocation(tokenIndex, lines)));
						}
					}
					else
					{
						parseErrors.Add(new InvalidPathSyntaxError(HumanizeCharacterLocation(m.Index, lines), ""));
					}

					if (!string.IsNullOrWhiteSpace(alias))
					{
						tokens.Add(new TokenPair(TokenType.Alias, alias, HumanizeCharacterLocation(m.Index + $"#if{alias}".Length, lines)));
					}
				}
				else if (m.Value.StartsWith("{{^if ", true, CultureInfo.InvariantCulture))
				{
					var token = m.Value.TrimStart('{').TrimEnd('}').TrimStart('^').Trim().Substring("if".Length);
					var eval = EvaluateNameFromToken(token);
					token = eval.Item1;
					var alias = eval.Item2;

					scopestack.Push(Tuple.Create($"^if{alias}", m.Index));

					if (token.StartsWith(" ") && token.Trim() != "")
					{
						token = token.Trim();
						if (FormatInExpressionFinder.IsMatch(token))
						{
							tokens.AddRange(TokenizeFormattables(token, templateString, lines,
								tokenIndex, parseErrors));

							tokens.Add(new TokenPair(TokenType.IfNot, ".", HumanizeCharacterLocation(tokenIndex, lines)));
						}
						else
						{
							tokens.Add(new TokenPair(TokenType.IfNot,
								Validated(token, templateString, m.Index, lines, parseErrors).Trim(), HumanizeCharacterLocation(tokenIndex, lines)));
						}
					}
					else
					{
						parseErrors.Add(new InvalidPathSyntaxError(HumanizeCharacterLocation(m.Index, lines), ""));
					}

					if (!string.IsNullOrWhiteSpace(alias))
					{
						tokens.Add(new TokenPair(TokenType.Alias, alias, HumanizeCharacterLocation(m.Index + $"^if{alias}".Length, lines)));
					}
				}
				else if (m.Value.StartsWith("{{/if", true, CultureInfo.InvariantCulture))
				{
					if (!string.Equals(m.Value, "{{/if}}", StringComparison.InvariantCultureIgnoreCase))
					{
						parseErrors.Add(new MorestachioSyntaxError(HumanizeCharacterLocation(m.Index, lines), "close", "if", "{{/if}}"));
					}
					else if (scopestack.Any() && scopestack.Peek().Item1.StartsWith("#if"))
					{
						var token = scopestack.Pop().Item1;
						tokens.Add(new TokenPair(TokenType.IfClose, token, HumanizeCharacterLocation(tokenIndex, lines)));
					}
					else
					{
						parseErrors.Add(new MorestachioUnopendScopeError(HumanizeCharacterLocation(m.Index, lines), "if", "{{#if name}}"));
					}
				}
				else if (m.Value.StartsWith("{{/each", true, CultureInfo.InvariantCulture))
				{
					if (!string.Equals(m.Value, "{{/each}}", StringComparison.InvariantCultureIgnoreCase))
					{
						parseErrors.Add(new MorestachioSyntaxError(HumanizeCharacterLocation(m.Index, lines), "close", "each", "{{/each}}"));
					}
					else if (scopestack.Any() && scopestack.Peek().Item1.StartsWith("#each"))
					{
						var token = scopestack.Pop().Item1;
						tokens.Add(new TokenPair(TokenType.CollectionClose, token, HumanizeCharacterLocation(tokenIndex, lines)));
					}
					else
					{
						parseErrors.Add(new MorestachioUnopendScopeError(HumanizeCharacterLocation(m.Index, lines), "each", "{{#each name}}"));
					}
				}
				else if (m.Value.StartsWith("{{#"))
				{
					//open group
					var token = m.Value.TrimStart('{').TrimEnd('}').TrimStart('#').Trim();

					var eval = EvaluateNameFromToken(token);
					token = eval.Item1;
					var alias = eval.Item2;

					if (scopestack.Any() && scopestack.Peek().Item1 == token)
					{
						tokens.Add(new TokenPair(TokenType.ElementClose,
							Validated(token, templateString, m.Index, lines, parseErrors), HumanizeCharacterLocation(tokenIndex, lines)));
					}
					else
					{
						scopestack.Push(Tuple.Create(alias, m.Index));
					}

					if (FormatInExpressionFinder.IsMatch(token))
					{
						tokens.AddRange(TokenizeFormattables(token, templateString, lines, tokenIndex,
							parseErrors));

						tokens.Add(new TokenPair(TokenType.ElementOpen, ".", HumanizeCharacterLocation(tokenIndex, lines)));
					}
					else
					{
						tokens.Add(new TokenPair(TokenType.ElementOpen,
							Validated(token, templateString, m.Index, lines, parseErrors), HumanizeCharacterLocation(tokenIndex, lines)));
					}

					if (!string.IsNullOrWhiteSpace(alias))
					{
						tokens.Add(new TokenPair(TokenType.Alias, alias, HumanizeCharacterLocation(m.Index + token.Length, lines)));
					}
				}
				else if (m.Value.StartsWith("{{^"))
				{
					//open inverted group
					var token = m.Value.TrimStart('{').TrimEnd('}').TrimStart('^').Trim();
					var eval = EvaluateNameFromToken(token);
					token = eval.Item1;
					var alias = eval.Item2;

					if (scopestack.Any() && scopestack.Peek().Item1 == token)
					{
						tokens.Add(new TokenPair(TokenType.ElementClose,
							Validated(token, templateString, m.Index, lines, parseErrors), HumanizeCharacterLocation(tokenIndex, lines)));
					}
					else
					{
						scopestack.Push(Tuple.Create(alias, m.Index));
					}

					if (FormatInExpressionFinder.IsMatch(token))
					{
						tokens.AddRange(TokenizeFormattables(token, templateString, lines, tokenIndex,
							parseErrors));

						tokens.Add(new TokenPair(TokenType.InvertedElementOpen, ".", HumanizeCharacterLocation(tokenIndex, lines)));
					}
					else
					{
						tokens.Add(new TokenPair(TokenType.InvertedElementOpen,
							Validated(token, templateString, m.Index, lines, parseErrors), HumanizeCharacterLocation(tokenIndex, lines)));
					}

					if (!string.IsNullOrWhiteSpace(alias))
					{
						tokens.Add(new TokenPair(TokenType.Alias, alias,
							HumanizeCharacterLocation(m.Index + $"#each{alias}".Length, lines)));
					}
				}
				else if (m.Value.StartsWith("{{/"))
				{
					var token = m.Value.TrimStart('{').TrimEnd('}').TrimStart('/').Trim();
					//close group
					if (scopestack.Any() && scopestack.Peek().Item1 == token)
					{
						scopestack.Pop();
						tokens.Add(new TokenPair(TokenType.ElementClose, token, HumanizeCharacterLocation(tokenIndex, lines)));
					}
					else
					{
						parseErrors.Add(new MorestachioUnopendScopeError(HumanizeCharacterLocation(m.Index, lines), "/", "{{#path}}", " There are more closing elements then open."));
					}
				}
				else if (m.Value.StartsWith("{{{") | m.Value.StartsWith("{{&"))
				{
					//escaped single element
					var token = m.Value.TrimStart('{').TrimEnd('}').TrimStart('&').Trim();
					tokens.Add(new TokenPair(TokenType.UnescapedSingleValue,
						Validated(token, templateString, m.Index, lines, parseErrors), HumanizeCharacterLocation(tokenIndex, lines)));
				}
				else if (m.Value.StartsWith("{{!"))
				{
					//it's a comment drop this on the floor, no need to even yield it.
				}
				else
				{
					tokens.AddRange(TokenizePath(parseErrors, m.Value, templateString, lines, tokenIndex));
				}

				//move forward in the string.
				idx = m.Index + m.Length;
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

				yield return new TokenPair(TokenType.Print, ".",
					HumanizeCharacterLocation(tokenIndex, lines));
			}
			else
			{
				yield return new TokenPair(TokenType.EscapedSingleValue,
					Validated(token, templateString, tokenIndex, lines, parseErrors),
					HumanizeCharacterLocation(tokenIndex, lines));
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

			exceptions.Add(new InvalidPathSyntaxError(HumanizeCharacterLocation(index, lines), token));

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

		/// <summary>
		///		Describes an Position within the Template
		/// </summary>
		public class CharacterLocation
		{
			/// <summary>
			///		The line of the Template
			/// </summary>
			public int Line { get; set; }

			/// <summary>
			///		The Character at the <see cref="Line"/>
			/// </summary>
			public int Character { get; set; }

			public override string ToString()
			{
				return $"Line: {Line}, Column: {Character}";
			}
		}
	}
}