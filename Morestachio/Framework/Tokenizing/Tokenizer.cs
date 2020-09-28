using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Morestachio.Document.Custom;
using Morestachio.Document.TextOperations;
using Morestachio.Framework.Context.Options;
using Morestachio.Framework.Error;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Parsing.ParserErrors;

#if ValueTask
using TokenizerResultPromise = System.Threading.Tasks.ValueTask<Morestachio.Framework.Tokenizing.TokenizerResult>;
using Promise = System.Threading.Tasks.ValueTask;
#else
using TokenizerResultPromise = System.Threading.Tasks.Task<Morestachio.Framework.Tokenizing.TokenizerResult>;
using Promise = System.Threading.Tasks.Task;
#endif

namespace Morestachio.Framework.Tokenizing
{
	/// <summary>
	///     Reads in a mustache template and lexes it into tokens.
	/// </summary>
	/// <exception cref="IndexedParseException"></exception>
	public class Tokenizer
	{

		//internal static readonly Regex NewlineFinder
		//	= new Regex("\n", RegexOptions.Compiled);

		private static readonly Regex ExpressionAliasFinder
			= new Regex("(?:\\s+(?:AS|as|As|aS)\\s+)([A-Za-z]+)$", RegexOptions.Compiled);


		internal static readonly Regex PartialIncludeRegEx = new Regex("Include (\\w*)( (?:With) )?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		internal static CharacterLocation HumanizeCharacterLocation(int characterIndex, int[] lines)
		{
			var line = Array.BinarySearch(lines, characterIndex);
			line = line < 0 ? ~line : line;

			var charIdx = characterIndex;
			//in both of these cases, we want to increment the char index by one to account for the '\n' that is skipped in the indexes.
			if (line < lines.Length && line > 0)
			{
				charIdx = characterIndex - (lines[line - 1] + 1);
			}
			else if (line > 0)
			{
				charIdx = characterIndex - (lines.LastOrDefault() + 1);
			}

			//Humans count from 1, so let's do that, too (hence the "+1" on these).
			var textLocation = new CharacterLocation(line + 1, charIdx + 1);
			return textLocation;
		}

		/// <summary>
		///		Gets the indexes of all newlines
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static List<int> FindNewLines(string text)
		{
			var nlIdxes = new List<int>();
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] == '\n')
				{
					nlIdxes.Add(i);
				}
			}

			return nlIdxes;
		}

		internal static bool IsStringDelimiter(char formatChar)
		{
			return formatChar == '\'' || formatChar == '\"';
		}

		private static char[] _whitespaceDelimiters = new[] { '\r', '\n', '\t', ' ' };
		internal static char[] GetWhitespaceDelimiters()
		{
			return _whitespaceDelimiters;
		}

		internal static bool IsWhiteSpaceDelimiter(char formatChar)
		{
			return formatChar == '\r' || formatChar == '\n' || formatChar == '\t' || formatChar == ' ';
		}

		internal static bool IsExpressionPathChar(char formatChar)
		{
			return formatChar == '?'
				   || formatChar == '/'
				   || IsStartOfExpressionPathChar(formatChar);
		}

		internal static bool IsStartOfExpressionPathChar(char formatChar)
		{
			return formatChar == '$'
				   || IsSingleExpressionPathChar(formatChar);
		}

		internal static bool IsSingleExpressionPathChar(char formatChar)
		{
			return formatChar == '.'
				   || formatChar == '~'
				   || IsExpressionDataPathChar(formatChar);
			//|| IsCharRegex.IsMatch(formatChar.ToString());
		}

		internal static bool IsExpressionDataPathChar(char formatChar)
		{
			return char.IsLetterOrDigit(formatChar) || formatChar == '_';
		}

		internal static bool IsNumberExpressionChar(char formatChar)
		{
			return char.IsDigit(formatChar);
		}

		internal static bool IsExpressionChar(char formatChar)
		{
			return IsExpressionPathChar(formatChar) ||
				   formatChar == '(' ||
				   formatChar == ')';
		}

		internal static bool IsPathDelimiterChar(char formatChar)
		{
			return formatChar == ',';
		}

		internal static bool IsOperationChar(char formatChar)
		{
			return
				formatChar == '+' ||
				formatChar == '-' ||
				formatChar == '*' ||
				formatChar == '/' ||
				formatChar == '^' ||
				formatChar == '%' ||
				formatChar == '<' ||
				formatChar == '>' ||
				formatChar == '=' ||
				formatChar == '!' ||
				formatChar == '&' ||
				formatChar == '|';
			//return MorestachioOperator.Yield().Any(f => f.OperatorText[0] == formatChar);
		}

		internal static bool IsEndOfFormatterArgument(char? formatChar)
		{
			return formatChar == ',' || formatChar == '.' || formatChar == ')';
		}

		/// <summary>
		///		Defines one item on the Template stack
		/// </summary>
		public readonly struct ScopeStackItem
		{
			/// <summary>
			///		Creates a new Stack item
			/// </summary>
			/// <param name="tokenType"></param>
			/// <param name="value"></param>
			/// <param name="index"></param>
			public ScopeStackItem(TokenType tokenType, string value, int index)
			{
				TokenType = tokenType;
				Value = value;
				Index = index;
			}

			/// <summary>
			///		The type of Stack
			/// </summary>
			public TokenType TokenType { get; }

			/// <summary>
			///		The string value of the token
			/// </summary>
			public string Value { get; }

			/// <summary>
			///		The index in the template
			/// </summary>
			public int Index { get; }
		}

		private readonly struct TokenMatch
		{
			public TokenMatch(int index, string value)
			{
				Index = index;
				Value = value;
				Length = value.Length;
			}

			public int Index { get; }
			public int Length { get; }
			public string Value { get; }
		}



		private static IEnumerable<TokenMatch> MatchTokens(string template,
			TokenzierContext context,
			RollingArray<char> lastChars)
		{

			var elementIndex = -1;
			char? isInString = null;
			var stringEscape = false;

			for (int i = 0; i < template.Length; i++)
			{
				var c = template[i];
				lastChars.Add(c);
				if (isInString.HasValue && context.CommentIntend == 0)
				{
					if (c == '\\')
					{
						stringEscape = true;
					}
					else if (stringEscape && c == isInString.Value)
					{
						stringEscape = false;
					}
					else if (!stringEscape && c == isInString.Value)
					{
						isInString = null;
					}
				}
				else if (lastChars.EndsWith(context.PrefixToken))
				{
					elementIndex = i - 1;
				}
				else if (elementIndex != -1 && lastChars.EndsWith(context.SuffixToken))
				{
					var token = template.Substring(elementIndex, i - elementIndex + 1);
					yield return new TokenMatch(elementIndex, token);
					elementIndex = -1;
				}
				else if (elementIndex != -1 && IsStringDelimiter(c) && context.CommentIntend == 0)
				{
					isInString = c;
				}
			}

			if (isInString.HasValue && elementIndex != -1)
			{
				var token = template.Substring(elementIndex, template.Length - elementIndex);
				yield return new TokenMatch(elementIndex, token);
			}
		}

		/// <summary>
		///		Goes through the template and evaluates all tokens that are enclosed by {{ }}.
		/// </summary>
		/// <param name="parserOptions"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		public static async TokenizerResultPromise Tokenize(ParserOptions parserOptions,
			TokenzierContext context)
		{
			var templateString = parserOptions.Template;

			var scopestack = new Stack<ScopeStackItem>();
			var partialsNames = new List<string>(parserOptions.PartialsStore?.GetNames() ?? new string[0]);
			context.SetLocation(0);
			var tokens = new List<TokenPair>();

			void BeginElse(TokenMatch match)
			{
				var firstNonContentToken = tokens
					.AsReadOnly()
					.Reverse()
					.FirstOrDefault(e => !e.Type.Equals(TokenType.Content));
				if (!firstNonContentToken.Type.Equals(TokenType.IfClose))
				{
					context.Errors
						.Add(new MorestachioSyntaxError(
							context.CurrentLocation
								.AddWindow(new CharacterSnippedLocation(1, 1, match.Value)), "find if block for else",
							firstNonContentToken.Value, "{{/if}}", "Could not find an /if block for this else"));
				}
				else
				{
					scopestack.Push(new ScopeStackItem(TokenType.Else, firstNonContentToken.Value, match.Index));
					tokens.Add(new TokenPair(TokenType.Else, firstNonContentToken.Value,
						context.CurrentLocation));
				}
			}

			void EndIf(TokenMatch match, string expected)
			{
				if (!scopestack.Any())
				{
					context.Errors.Add(new MorestachioUnopendScopeError(context.CurrentLocation
							.AddWindow(new CharacterSnippedLocation(1, 1, match.Value)),
						"if",
						"{{#if name}}"));
				}
				else
				{
					var item1 = scopestack.Peek();
					if (item1.TokenType == TokenType.If || item1.TokenType == TokenType.IfNot)
					{
						var token = scopestack.Pop().Value;
						tokens.Add(new TokenPair(TokenType.IfClose, token,
							context.CurrentLocation));
					}
					else
					{
						context.Errors.Add(new MorestachioUnopendScopeError(
							context.CurrentLocation
								.AddWindow(new CharacterSnippedLocation(1, 1, match.Value)),
							"if",
							"{{#if name}}"));
					}
				}
			}

			string TrimToken(string token, string keyword, char key = '#')
			{
				token = token.TrimStart(key).TrimStart();
				if (keyword != null)
				{
					token = token.Substring(keyword.Length);
				}

				return token;
			}


			var lastChars = new RollingArray<char>(Math.Max(context.PrefixToken.Length, context.SuffixToken.Length) + 1);

			foreach (var match in MatchTokens(templateString, context, lastChars))
			{
				var tokenValue = match.Value;
				var trimmedToken = tokenValue
					.Remove(0, context.PrefixToken.Length);
				trimmedToken = trimmedToken
					.Remove(trimmedToken.Length - context.SuffixToken.Length)
					.Trim(GetWhitespaceDelimiters());

				if (context.CommentIntend > 0)
				{
					if (trimmedToken == "/!")
					{
						context.CommentIntend--;
						if (context.CommentIntend == 0)
						{
							//move forward in the string.
							if (context.Character > match.Index + match.Length)
							{
								throw new InvalidOperationException("Internal index location error");
							}

							context.SetLocation(match.Index + match.Length);
						}
					}
					else if (trimmedToken.Equals("!"))
					{
						context.CommentIntend++;
					}
				}
				else
				{
					var trimLeading = context.TrimLeading;
					var trimTailing = context.TrimTailing;

					//check if the token is appended by a -| in that case we want to trim all whitespaces at the end of any following content
					if (trimmedToken.StartsWith("-"))
					{
						var pipeIndex = trimmedToken.IndexOf('|');
						if (pipeIndex != -1)
						{
							var stopEx = false;
							//it is possible for an number or an operation to start with an - so the pipe is optional here
							for (int i = 1; i < pipeIndex - 1; i++)
							{
								if (trimmedToken[i] != ' ')
								{
									context.Errors.Add(new MorestachioSyntaxError(context.CurrentLocation
											.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "trim", "trim",
										"{{operation | -}}", $" expected to find '- |' with only whitespaces between the pipe and minus but found '{trimmedToken[i]}'"));
									stopEx = true;
									break;
								}
							}

							if (stopEx)
							{
								continue;
							}

							trimmedToken = trimmedToken.Substring(pipeIndex + 1).Trim();
							trimLeading = true;

							tokens.Add(new TokenPair(TokenType.TrimPrependedLineBreaks, trimmedToken, context.CurrentLocation, EmbeddedState.Previous));
						}
					}

					//yield front content.
					if (match.Index > context.Character)
					{
						if (trimLeading)
						{
							tokens.Add(new TokenPair(TokenType.TrimPrependedLineBreaks, trimmedToken, context.CurrentLocation, EmbeddedState.Previous));
						}
						tokens.Add(new TokenPair(TokenType.Content, templateString.Substring(context.Character, match.Index - context.Character),
							context.CurrentLocation));
					}

					context.SetLocation(match.Index + context.PrefixToken.Length);

					//check if the token is appended by a |- in that case we want to trim all folowing whitespaces
					if (trimmedToken.EndsWith("-"))
					{
						var pipeIndex = trimmedToken.LastIndexOf('|');
						if (pipeIndex == -1)
						{
							context.Errors.Add(new MorestachioSyntaxError(context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "trim", "trim",
								"{{operation | -}}", $" expected to find '| -' but missing '|'"));
						}

						var stopEx = false;
						for (int i = pipeIndex + 1; i < trimmedToken.Length - 1; i++)
						{
							if (trimmedToken[i] != ' ')
							{
								context.Errors.Add(new MorestachioSyntaxError(context.CurrentLocation.Offset(1)
										.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "trim", "trim",
									"{{operation | -}}", $" expected to find '| -' with only whitespaces between the pipe and minus but found '{trimmedToken[i]}'"));
								stopEx = true;
								break;
							}
						}

						if (stopEx)
						{
							continue;
						}

						trimmedToken = trimmedToken.Remove(pipeIndex).Trim();
						trimTailing = true;
					}

					if (trimmedToken.StartsWith("#declare ", true, CultureInfo.InvariantCulture))
					{
						var token = TrimToken(trimmedToken, "declare ");
						scopestack.Push(new ScopeStackItem(TokenType.PartialDeclarationOpen, token, match.Index));
						if (string.IsNullOrWhiteSpace(token))
						{
							context.Errors.Add(new MorestachioSyntaxError(context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "open", "declare",
								"{{#declare name}}", " Missing the Name."));
						}
						else
						{
							partialsNames.Add(token);
							tokens.Add(new TokenPair(TokenType.PartialDeclarationOpen, token,
								context.CurrentLocation));
						}
					}
					else if (trimmedToken.Equals("/declare", StringComparison.CurrentCultureIgnoreCase))
					{
						if (scopestack.Any() && scopestack.Peek().TokenType == TokenType.PartialDeclarationOpen)
						{
							var token = scopestack.Pop().Value;
							tokens.Add(new TokenPair(TokenType.PartialDeclarationClose, token,
								context.CurrentLocation));
						}
						else
						{
							context.Errors.Add(new MorestachioUnopendScopeError(context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "declare",
								"{{#declare name}}"));
						}
					}
					else if (trimmedToken.StartsWith("#include ", true, CultureInfo.InvariantCulture))
					{
						var token = trimmedToken.TrimStart('#').Trim();
						var partialRegex = PartialIncludeRegEx.Match(token);
						var partialName = partialRegex.Groups[1].Value;
						var partialContext = partialRegex.Groups[2].Value;
						if (!string.IsNullOrWhiteSpace(partialContext))
						{
							partialContext = token.Substring(partialRegex.Groups[2].Index + "WITH ".Length);
						}
						if (string.IsNullOrWhiteSpace(partialName) || !partialsNames.Contains(partialName))
						{
							context.Errors.Add(new MorestachioSyntaxError(
								context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)),
								"use",
								"include",
								"{{#include name}}",
								$" There is no Partial declared '{partialName}'. Partial names are case sensitive and must be declared before an include."));
						}
						else
						{
							IMorestachioExpression exp = null;
							if (!string.IsNullOrWhiteSpace(partialContext))
							{
								exp = ExpressionParser.ParseExpression(partialContext, context);
							}
							var tokenPair = new TokenPair(TokenType.RenderPartial, partialName, context.CurrentLocation, exp);
							tokens.Add(tokenPair);
						}
					}
					else if (trimmedToken.StartsWith("#each ", true, CultureInfo.InvariantCulture))
					{
						var token = TrimToken(trimmedToken, "each");
						var eval = EvaluateNameFromToken(token);
						token = eval.Value;
						var alias = eval.Name;

						scopestack.Push(new ScopeStackItem(TokenType.CollectionOpen, alias ?? token, match.Index));

						if (token.Trim() != "")
						{
							token = token.Trim();
							ScopingBehavior? scopeBehavior = null;
							if (!string.IsNullOrWhiteSpace(alias))
							{
								if (token.EndsWith("NoScope", StringComparison.InvariantCultureIgnoreCase))
								{
									scopeBehavior = ScopingBehavior.DoNotScope;
								}
								if (token.EndsWith("WithScope", StringComparison.InvariantCultureIgnoreCase))
								{
									scopeBehavior = ScopingBehavior.ScopeAnyway;
								}
							}

							tokens.Add(new TokenPair(TokenType.CollectionOpen,
								token,
								context.CurrentLocation,
								ExpressionParser.ParseExpression(token, context), EmbeddedState.None, scopeBehavior));
						}
						else
						{
							context.Errors.Add(new InvalidPathSyntaxError(context.CurrentLocation
								.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), ""));
						}

						if (!string.IsNullOrWhiteSpace(alias))
						{
							context.AdvanceLocation("each ".Length + alias.Length);
							tokens.Add(new TokenPair(TokenType.Alias, alias,
								context.CurrentLocation));
						}
					}
					else if (trimmedToken.Equals("/each", StringComparison.InvariantCultureIgnoreCase))
					{
						if (scopestack.Any() && scopestack.Peek().TokenType == TokenType.CollectionOpen)
						{
							var token = scopestack.Pop().Value;
							tokens.Add(new TokenPair(TokenType.CollectionClose, token,
								context.CurrentLocation));
						}
						else
						{
							context.Errors.Add(new MorestachioUnopendScopeError(context.CurrentLocation
								.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "each", "{{#each name}}"));
						}
					}
					else if (trimmedToken.StartsWith("#while ", true, CultureInfo.InvariantCulture))
					{
						var token = TrimToken(trimmedToken, "while");

						scopestack.Push(new ScopeStackItem(TokenType.WhileLoopOpen, token, match.Index));

						if (token.Trim() != "")
						{
							token = token.Trim();
							tokens.Add(new TokenPair(TokenType.WhileLoopOpen,
								token,
								context.CurrentLocation, ExpressionParser.ParseExpression(token, context)));
						}
						else
						{
							context.Errors.Add(new InvalidPathSyntaxError(context.CurrentLocation
								.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), ""));
						}
					}
					else if (trimmedToken.Equals("/while", StringComparison.InvariantCultureIgnoreCase))
					{
						if (scopestack.Any() && scopestack.Peek().TokenType == TokenType.WhileLoopOpen)
						{
							var token = scopestack.Pop().Value;
							tokens.Add(new TokenPair(TokenType.WhileLoopClose, token,
								context.CurrentLocation));
						}
						else
						{
							context.Errors.Add(new MorestachioUnopendScopeError(context.CurrentLocation
								.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "while", "{{#while Expression}}"));
						}
					}
					else if (trimmedToken.StartsWith("#do ", true, CultureInfo.InvariantCulture))
					{
						var token = TrimToken(trimmedToken, "do");
						scopestack.Push(new ScopeStackItem(TokenType.DoLoopOpen, token, match.Index));

						if (token.Trim() != "")
						{
							token = token.Trim();
							tokens.Add(new TokenPair(TokenType.DoLoopOpen,
								token,
								context.CurrentLocation, ExpressionParser.ParseExpression(token, context)));
						}
						else
						{
							context.Errors.Add(new InvalidPathSyntaxError(context.CurrentLocation
								.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), ""));
						}
					}
					else if (trimmedToken.Equals("/do", StringComparison.InvariantCultureIgnoreCase))
					{
						if (scopestack.Any() && scopestack.Peek().TokenType == TokenType.DoLoopOpen)
						{
							var token = scopestack.Pop().Value;
							tokens.Add(new TokenPair(TokenType.DoLoopClose, token,
								context.CurrentLocation));
						}
						else
						{
							context.Errors.Add(new MorestachioUnopendScopeError(context.CurrentLocation
								.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "do", "{{#do Expression}}"));
						}
					}
					else if (trimmedToken.StartsWith("#repeat ", true, CultureInfo.InvariantCulture))
					{
						var token = TrimToken(trimmedToken, "repeat");
						scopestack.Push(new ScopeStackItem(TokenType.RepeatLoopOpen, token, match.Index));

						if (token.Trim() != "")
						{
							token = token.Trim();
							tokens.Add(new TokenPair(TokenType.RepeatLoopOpen,
								token,
								context.CurrentLocation, ExpressionParser.ParseExpression(token, context)));
						}
						else
						{
							context.Errors.Add(new InvalidPathSyntaxError(context.CurrentLocation
								.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), ""));
						}
					}
					else if (trimmedToken.Equals("/repeat", StringComparison.InvariantCultureIgnoreCase))
					{
						if (scopestack.Any() && scopestack.Peek().TokenType == TokenType.RepeatLoopOpen)
						{
							var token = scopestack.Pop().Value;
							tokens.Add(new TokenPair(TokenType.RepeatLoopClose, token,
								context.CurrentLocation));
						}
						else
						{
							context.Errors.Add(new MorestachioUnopendScopeError(context.CurrentLocation
								.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "repeat", "{{#repeat Expression}}"));
						}
					}
					else if (trimmedToken.StartsWith("#switch ", true, CultureInfo.InvariantCulture))
					{
						var token = TrimToken(trimmedToken, "switch");
						var eval = EvaluateNameFromToken(token);
						token = eval.Value;
						if (eval.Name != null)
						{
							context.Errors.Add(new MorestachioSyntaxError(
								context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "#switch", "AS", "No Alias"));
						}

						scopestack.Push(new ScopeStackItem(TokenType.SwitchOpen, token, match.Index));

						if (token.Trim() != "")
						{
							token = token.Trim();
							tokens.Add(new TokenPair(TokenType.SwitchOpen,
								token,
								context.CurrentLocation, ExpressionParser.ParseExpression(token, context)));
						}
						else
						{
							context.Errors.Add(new InvalidPathSyntaxError(context.CurrentLocation
								.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), ""));
						}
					}
					else if (trimmedToken.Equals("/switch", StringComparison.InvariantCultureIgnoreCase))
					{
						if (scopestack.Any() && scopestack.Peek().TokenType == TokenType.SwitchOpen)
						{
							var token = scopestack.Pop().Value;
							tokens.Add(new TokenPair(TokenType.SwitchClose, token,
								context.CurrentLocation));
						}
						else
						{
							context.Errors.Add(new MorestachioUnopendScopeError(context.CurrentLocation
								.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "switch", "{{#switch Expression}}"));
						}
					}
					else if (trimmedToken.StartsWith("#case ", true, CultureInfo.InvariantCulture))
					{
						var token = TrimToken(trimmedToken, "case");
						var eval = EvaluateNameFromToken(token);
						token = eval.Value;
						if (eval.Name != null)
						{
							context.Errors.Add(new MorestachioSyntaxError(
								context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "#case", "AS", "No Alias"));
						}

						scopestack.Push(new ScopeStackItem(TokenType.SwitchCaseOpen, token, match.Index));

						if (token.Trim() != "")
						{
							token = token.Trim();
							tokens.Add(new TokenPair(TokenType.SwitchCaseOpen,
								token,
								context.CurrentLocation, ExpressionParser.ParseExpression(token, context)));
						}
						else
						{
							context.Errors.Add(new InvalidPathSyntaxError(context.CurrentLocation
								.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), ""));
						}
					}
					else if (trimmedToken.Equals("/case", StringComparison.InvariantCultureIgnoreCase))
					{
						if (scopestack.Any() && scopestack.Peek().TokenType == TokenType.SwitchCaseOpen)
						{
							var token = scopestack.Pop().Value;
							tokens.Add(new TokenPair(TokenType.SwitchCaseClose, token,
								context.CurrentLocation));
						}
						else
						{
							context.Errors.Add(new MorestachioUnopendScopeError(context.CurrentLocation
								.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "case", "{{#case Expression}}"));
						}
					}
					else if (trimmedToken.Equals("#default", StringComparison.InvariantCultureIgnoreCase))
					{
						var token = TrimToken(trimmedToken, "default");
						var eval = EvaluateNameFromToken(token);
						token = eval.Value;
						if (eval.Name != null)
						{
							context.Errors.Add(new MorestachioSyntaxError(
								context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "#default", "AS", "No Alias"));
						}

						scopestack.Push(new ScopeStackItem(TokenType.SwitchDefaultOpen, token, match.Index));

						if (token.Trim() == "")
						{
							token = token.Trim();
							tokens.Add(new TokenPair(TokenType.SwitchDefaultOpen,
								token,
								context.CurrentLocation));
						}
						else
						{
							context.Errors.Add(new InvalidPathSyntaxError(context.CurrentLocation
								.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), ""));
						}
					}
					else if (trimmedToken.Equals("/default", StringComparison.InvariantCultureIgnoreCase))
					{
						if (scopestack.Any() && scopestack.Peek().TokenType == TokenType.SwitchDefaultOpen)
						{
							var token = scopestack.Pop().Value;
							tokens.Add(new TokenPair(TokenType.SwitchDefaultClose, token,
								context.CurrentLocation));
						}
						else
						{
							context.Errors.Add(new MorestachioUnopendScopeError(context.CurrentLocation
								.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "default", "{{#default}}"));
						}
					}
					else if (trimmedToken.StartsWith("#if ", true, CultureInfo.InvariantCulture))
					{
						var token = TrimToken(trimmedToken, "if");
						var eval = EvaluateNameFromToken(token);
						token = eval.Value;
						if (eval.Name != null)
						{
							context.Errors.Add(new MorestachioSyntaxError(
								context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "^if", "AS", "No Alias"));
						}

						scopestack.Push(new ScopeStackItem(TokenType.If, token, match.Index));

						if (token.Trim() != "")
						{
							token = token.Trim();
							tokens.Add(new TokenPair(TokenType.If,
								token,
								context.CurrentLocation, ExpressionParser.ParseExpression(token, context)));
						}
						else
						{
							context.Errors.Add(new InvalidPathSyntaxError(context.CurrentLocation
								.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), ""));
						}
					}
					else if (trimmedToken.StartsWith("^if ", true, CultureInfo.InvariantCulture))
					{
						var token = TrimToken(trimmedToken, "if", '^');
						var eval = EvaluateNameFromToken(token);
						token = eval.Value;
						if (eval.Name != null)
						{
							context.Errors.Add(new MorestachioSyntaxError(
								context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "^if", "AS", "No Alias"));
						}

						scopestack.Push(new ScopeStackItem(TokenType.IfNot, token, match.Index));

						if (token.Trim() != "")
						{
							token = token.Trim();
							tokens.Add(new TokenPair(TokenType.IfNot,
								token,
								context.CurrentLocation, ExpressionParser.ParseExpression(token, context)));
						}
						else
						{
							context.Errors.Add(new InvalidPathSyntaxError(context.CurrentLocation
								.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), ""));
						}
					}
					else if (trimmedToken.Equals("/if", StringComparison.InvariantCultureIgnoreCase))
					{
						EndIf(match, "/If");
					}
					else if (trimmedToken.Equals("#ifelse", StringComparison.InvariantCultureIgnoreCase))
					{
						EndIf(match, "#ifelse");
						BeginElse(match);
					}
					else if (trimmedToken.Equals("#else", StringComparison.InvariantCultureIgnoreCase))
					{
						BeginElse(match);
					}
					else if (trimmedToken.Equals("/else", StringComparison.InvariantCultureIgnoreCase))
					{
						if (scopestack.Any() && scopestack.Peek().TokenType == TokenType.Else)
						{
							var token = scopestack.Pop().Value;
							tokens.Add(new TokenPair(TokenType.ElseClose, token,
								context.CurrentLocation));
						}
						else
						{
							context.Errors.Add(new MorestachioUnopendScopeError(
								context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "else",
								"{{#else name}}"));
						}
					}
					else if (trimmedToken.StartsWith("#var ", true, CultureInfo.InvariantCulture))
					{
						tokens.Add(ExpressionParser.TokenizeVariableAssignment(trimmedToken,
							context, TokenType.VariableVar));
					}
					else if (trimmedToken.StartsWith("#let ", true, CultureInfo.InvariantCulture))
					{
						tokens.Add(ExpressionParser.TokenizeVariableAssignment(trimmedToken,
							context, TokenType.VariableLet));
					}
					else if (trimmedToken.StartsWith("^"))
					{
						//open inverted group
						var token = trimmedToken.TrimStart('^').Trim();
						var eval = EvaluateNameFromToken(token);
						token = eval.Value;
						var alias = eval.Name;
						scopestack.Push(new ScopeStackItem(TokenType.InvertedElementOpen, alias ?? token, match.Index));
						tokens.Add(new TokenPair(TokenType.InvertedElementOpen,
							token,
							context.CurrentLocation, ExpressionParser.ParseExpression(token, context)));

						if (!string.IsNullOrWhiteSpace(alias))
						{
							context.AdvanceLocation(1 + alias.Length);
							tokens.Add(new TokenPair(TokenType.Alias, alias,
								context.CurrentLocation));
						}
					}
					else if (trimmedToken.StartsWith("&"))
					{
						//escaped single element
						var token = trimmedToken.TrimStart('&').Trim();
						tokens.Add(new TokenPair(TokenType.UnescapedSingleValue,
							token,
							context.CurrentLocation, ExpressionParser.ParseExpression(token, context)));
					}
					else if (trimmedToken.StartsWith("!"))
					{
						//it's a comment drop this on the floor, no need to even yield it.
						if (trimmedToken.Equals("!"))
						{
							//except for when its a block comment then set the isCommentBlock flag
							context.CommentIntend++;
						}
					}
					else if (trimmedToken.Equals("#NL", StringComparison.InvariantCultureIgnoreCase))
					{
						tokens.Add(new TokenPair(TokenType.WriteLineBreak, trimmedToken, context.CurrentLocation));
					}
					else if (trimmedToken.Equals("#TNL", StringComparison.InvariantCultureIgnoreCase))
					{
						tokens.Add(new TokenPair(TokenType.TrimLineBreak, trimmedToken, context.CurrentLocation));
					}
					else if (trimmedToken.Equals("#TNLS", StringComparison.InvariantCultureIgnoreCase))
					{
						tokens.Add(new TokenPair(TokenType.TrimLineBreaks, trimmedToken, context.CurrentLocation));
					}
					else if (trimmedToken.Equals("#TRIMALL", StringComparison.InvariantCultureIgnoreCase))
					{
						tokens.Add(new TokenPair(TokenType.TrimEverything, trimmedToken, context.CurrentLocation));
					}
					else if (trimmedToken.StartsWith("#SET OPTION ", StringComparison.InvariantCultureIgnoreCase))
					{
						var token = TrimToken(trimmedToken, "SET OPTION ");
						var expectEquals = false;
						string name = null;
						IMorestachioExpression value = null;
						for (int i = 0; i < token.Length; i++)
						{
							var c = token[i];
							if (IsWhiteSpaceDelimiter(c))
							{
								expectEquals = true;
								continue;
							}

							if (expectEquals || c == '=')
							{
								if (c != '=')
								{
									context.Errors.Add(new MorestachioUnopendScopeError(context.CurrentLocation
											.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "/", "{{#SET OPTION Name = Value}}",
										$" Expected to find '=' or whitespace after name but found '{c}'"));
								}
								else
								{
									name = token.Substring(0, i - 1).Trim();
									value = ExpressionParser.ParseExpression(token.Substring(i + 1).Trim(), context);
									break;
								}
							}
						}

						if (string.IsNullOrWhiteSpace(name))
						{
							context.Errors.Add(new MorestachioUnopendScopeError(context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "/", "{{#SET OPTION Name = Value}}",
								$" Expected to find '=' after name"));
							break;
						}

						if (value == null)
						{
							context.Errors.Add(new MorestachioUnopendScopeError(context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "/", "{{#SET OPTION Name = Value}}",
								$" Expected to find an expression after '='"));
							break;
						}

						await context.SetOption(name, value, parserOptions);
					}
					//else if (tokenValue.Equals("{{/TRIMALL}}", StringComparison.InvariantCultureIgnoreCase))
					//{
					//	tokens.Add(new TokenPair(TokenType.StopTrimEverything, tokenValue, context.CurrentLocation));
					//}
					else
					{
						//check for custom DocumentItem provider

						var customDocumentProvider =
							parserOptions.CustomDocumentItemProviders.FirstOrDefault(e => e.ShouldTokenize(trimmedToken));

						if (customDocumentProvider != null)
						{
							var tokenPairs = customDocumentProvider
								.Tokenize(new CustomDocumentItemProvider.TokenInfo(trimmedToken, context, scopestack),
									parserOptions);
							tokens.AddRange(tokenPairs);
						}
						else if (trimmedToken.StartsWith("#"))
						{
							//open group
							var token = trimmedToken.TrimStart('#').Trim();

							var eval = EvaluateNameFromToken(token);
							token = eval.Value;
							var alias = eval.Name;
							scopestack.Push(new ScopeStackItem(TokenType.ElementOpen, alias ?? token, match.Index));
							tokens.Add(new TokenPair(TokenType.ElementOpen,
								token,
								context.CurrentLocation, ExpressionParser.ParseExpression(token, context)));

							if (!string.IsNullOrWhiteSpace(alias))
							{
								context.AdvanceLocation(3 + alias.Length);
								tokens.Add(new TokenPair(TokenType.Alias, alias,
									context.CurrentLocation));
							}
						}
						else if (trimmedToken.StartsWith("/"))
						{
							var token = trimmedToken.TrimStart('/').Trim();
							//close group
							if (!scopestack.Any())
							{
								context.Errors.Add(new MorestachioUnopendScopeError(context.CurrentLocation
										.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "/", "{{#path}}",
									" There are more closing elements then open."));
							}
							else
							{
								var item = scopestack.Peek();
								if ((item.TokenType == TokenType.ElementOpen ||
									 item.TokenType == TokenType.InvertedElementOpen)
									&& item.Value == token)
								{
									scopestack.Pop();
									tokens.Add(new TokenPair(TokenType.ElementClose, token,
										context.CurrentLocation));
								}
								else
								{
									context.Errors.Add(new MorestachioUnopendScopeError(context.CurrentLocation
											.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "/", "{{#path}}",
										" There are more closing elements then open."));
								}
							}
						}
						else
						{
							//unsingle value.
							var token = trimmedToken.Trim();
							tokens.Add(new TokenPair(TokenType.EscapedSingleValue,
								token,
								context.CurrentLocation, ExpressionParser.ParseExpression(token, context)));
						}
					}

					if (trimTailing)
					{
						tokens.Add(new TokenPair(TokenType.TrimLineBreaks, trimmedToken, context.CurrentLocation, EmbeddedState.Next));
					}

					//move forward in the string.
					if (context.Character > match.Index + match.Length)
					{
						throw new InvalidOperationException("Internal index location error");
					}

					context.SetLocation(match.Index + match.Length);
				}
			}

			if (context.Character < templateString.Length)
			{
				tokens.Add(new TokenPair(TokenType.Content, templateString.Substring(context.Character),
					context.CurrentLocation));
			}

			if (scopestack.Any() || parserOptions.CustomDocumentItemProviders.Any(f => f.ScopeStack.Any()))
			{
				foreach (var unclosedScope in scopestack
					.Concat(parserOptions.CustomDocumentItemProviders.SelectMany(f => f.ScopeStack))
					.Select(k =>
					{
						return new
						{
							scope = k.TokenType.ToString(),
							location = HumanizeCharacterLocation(k.Index, context.Lines)
						};
					}).Reverse())
				{
					context.Errors.Add(new MorestachioUnclosedScopeError(unclosedScope.location
						.AddWindow(new CharacterSnippedLocation(1, -1, "")), unclosedScope.scope, ""));
				}
			}

			return new TokenizerResult(tokens);
		}

		internal static NameValueToken EvaluateNameFromToken(string token)
		{
			var match = ExpressionAliasFinder.Match(token);
			var name = match.Groups[1].Value;
			if (!string.IsNullOrWhiteSpace(name))
			{
				return new NameValueToken(token.Substring(0, token.Length - (" AS" + name).Length), name.Trim());
			}

			return new NameValueToken(token, null);
		}

		internal readonly struct NameValueToken
		{
			public NameValueToken(string value, string name)
			{
				Name = name;
				Value = value;
			}

			public string Name { get; }
			public string Value { get; }
		}
	}
}