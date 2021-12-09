using System.Text.RegularExpressions;
using Morestachio.Document.Custom;
using Morestachio.Framework.Context.Options;
using Morestachio.Framework.Error;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Expression.Parser;
using Morestachio.Helper.Logging;
using Morestachio.Parsing.ParserErrors;
using Morestachio.TemplateContainers;
using Morestachio.Util;

namespace Morestachio.Framework.Tokenizing
{
	/// <summary>
	///     Reads in a mustache template and lexes it into tokens.
	/// </summary>
	/// <exception cref="IndexedParseException"></exception>
	public class Tokenizer
	{
		internal static readonly Regex PartialIncludeRegEx
			= new Regex("Include (\\w*)( (?:With) )?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		internal static CharacterLocation HumanizeCharacterLocation(int characterIndex, List<int> lines)
		{
			var line = lines.BinarySearch(characterIndex);
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

			//Humans count from 1, so let's do that, too (hence the "+1" on these).
			var textLocation = new CharacterLocation(line + 1, charIdx + 1, characterIndex + 1);
			return textLocation;
		}

		/// <summary>
		///     Gets the indexes of all newlines
		/// </summary>
		/// <param name="text"></param>
		/// <returns>A list of all indexes where \n is present</returns>
		public static List<int> FindNewLines(string text)
		{
			var nlIdxes = new List<int>();
			var idx = -1;

			while ((idx = text.IndexOf('\n', idx + 1)) != -1)
			{
				nlIdxes.Add(idx);
			}

			return nlIdxes;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool IsStringDelimiter(char formatChar)
		{
			return formatChar == '\'' || formatChar == '\"';
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static string GetAsKeyword()
		{
			return " AS ";
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool IsEndOfExpression(char formatChar)
		{
			return IsEndOfWholeExpression(formatChar) || IsEndOfExpressionSection(formatChar);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool IsEndOfExpressionSection(char formatChar)
		{
			return formatChar == '#';
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool IsEndOfWholeExpression(char formatChar)
		{
			return formatChar == ';';
		}

		private static char[] _whitespaceDelimiters = new[] { '\r', '\n', '\t', ' ' };

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static char[] GetWhitespaceDelimiters()
		{
			return _whitespaceDelimiters;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool IsWhiteSpaceDelimiter(char formatChar)
		{
			return formatChar == '\r' || formatChar == '\n' || formatChar == '\t' || formatChar == ' ';
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool IsExpressionPathChar(char formatChar)
		{
			return formatChar == '?'
				|| formatChar == '/'
				|| IsStartOfExpressionPathChar(formatChar);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool IsStartOfExpressionPathChar(char formatChar)
		{
			return formatChar == '$'
				|| formatChar == '?'
				|| IsSingleExpressionPathChar(formatChar);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool IsSingleExpressionPathChar(char formatChar)
		{
			return formatChar == '.'
				|| formatChar == '~'
				|| IsExpressionDataPathChar(formatChar);
			//|| IsCharRegex.IsMatch(formatChar.ToString());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool IsExpressionDataPathChar(char formatChar)
		{
			return char.IsLetterOrDigit(formatChar) || formatChar == '_';
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool IsNumberExpressionChar(char formatChar)
		{
			return char.IsDigit(formatChar);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool IsExpressionChar(char formatChar)
		{
			return IsExpressionPathChar(formatChar) ||
				formatChar == '(' ||
				formatChar == ')';
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool IsPathDelimiterChar(char formatChar)
		{
			return formatChar == ',';
		}


		/// <summary>
		///     This method is hard coded for performance reasons. If modified here, the changes must be reflected in
		///     <see cref="MorestachioOperator" />
		/// </summary>
		/// <param name="formatChar"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
				formatChar == '?' ||
				formatChar == '!' ||
				formatChar == '|';
		}

		/// <summary>
		///     This method is hard coded for performance reasons. If modified here, the changes must be reflected in
		///     <see cref="MorestachioOperator" />
		/// </summary>
		/// <param name="operatorText"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool IsOperationString(string operatorText)
		{
			return
				operatorText == "+" ||
				operatorText == "-" ||
				operatorText == "*" ||
				operatorText == "/" ||
				operatorText == "^" ||
				operatorText == "%" ||
				operatorText == "<<" ||
				operatorText == ">>" ||
				operatorText == "==" ||
				operatorText == "!=" ||
				operatorText == "<" ||
				operatorText == "<=" ||
				operatorText == ">" ||
				operatorText == ">=" ||
				operatorText == "&&" ||
				operatorText == "||" ||
				operatorText == "<?" ||
				operatorText == ">?" ||
				operatorText == "!" ||
				operatorText == "??";
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool IsEndOfFormatterArgument(char? formatChar)
		{
			return formatChar == ',' || formatChar == '.' || formatChar == ')';
		}

		/// <summary>
		///     Defines one item on the Template stack
		/// </summary>
		public readonly struct ScopeStackItem
		{
			/// <summary>
			///     Creates a new Stack item
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
			///     The type of Stack
			/// </summary>
			public TokenType TokenType { get; }

			/// <summary>
			///     The string value of the token
			/// </summary>
			public string Value { get; }

			/// <summary>
			///     The index in the template
			/// </summary>
			public int Index { get; }
		}

		/// <summary>
		///     Goes through the template and evaluates all tokens that are enclosed by {{ }}.
		/// </summary>
		/// <param name="parserOptions"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		public static TokenizerResultPromise Tokenize(ParserOptions parserOptions,
													TokenzierContext context)
		{
			return Tokenize(parserOptions, context, parserOptions.Template.Matches(context));
		}

		internal static async TokenizerResultPromise Tokenize(ParserOptions parserOptions,
															TokenzierContext context,
															IEnumerable<TokenMatch> templateString)
		{
			var scopestack = new Stack<ScopeStackItem>();
			List<string> partialsNames;

			if (parserOptions.PartialsStore is IAsyncPartialsStore asyncPartialStore)
			{
				partialsNames = new List<string>(await asyncPartialStore.GetNamesAsync(parserOptions));
			}
			else if (parserOptions.PartialsStore != null)
			{
				partialsNames = new List<string>(parserOptions.PartialsStore.GetNames(parserOptions));
			}
			else
			{
				partialsNames = new List<string>(Enumerable.Empty<string>());
			}

			var tokens = new List<TokenPair>();
			var tokenOptions = new List<ITokenOption>();

			void EndIf(TokenMatch match)
			{
				if (!scopestack.Any())
				{
					context.Errors.Add(new MorestachioUnopendScopeError(context.CurrentLocation
						.AddWindow(new CharacterSnippedLocation(1, 1, match.Value)),
					"if",
					"{{#if name}}"));
					return;
				}

				var item1 = scopestack.Peek();

				if (item1.TokenType != TokenType.If && item1.TokenType != TokenType.IfNot)
				{
					context.Errors.Add(new MorestachioUnopendScopeError(
					context.CurrentLocation
						.AddWindow(new CharacterSnippedLocation(1, 1, match.Value)),
					"if",
					"{{#if name}}"));
					return;
				}

				var token = scopestack.Pop().Value;
				tokens.Add(new TokenPair(TokenType.IfClose, token,
				context.CurrentLocation, tokenOptions));
			}

			string TrimToken(string token, string keyword, char key = '#')
			{
				token = token.TrimStart(key).TrimStart();
				return keyword != null ? token.Substring(keyword.Length) : token;
			}

			IMorestachioExpression ExtractExpression(ref string token)
			{
				var pre = context.Character;
				var expression = ExpressionParser.ParseExpression(token, context);
				token = token.Remove(0, context.Character - pre);
				return expression;
			}

			bool TryParseFlagOption(ref string token, string flagName)
			{
				var indexOf = token.IndexOf(flagName, StringComparison.OrdinalIgnoreCase);

				if (indexOf == -1)
				{
					return false;
				}

				token = token.Remove(indexOf, flagName.Length);
				return true;
			}

			bool TryParseExpressionOption(ref string token, string optionName, out IMorestachioExpression expression)
			{
				var optionIndex = token.IndexOf(optionName, StringComparison.OrdinalIgnoreCase);

				if (optionIndex != -1)
				{
					token = token.Remove(optionIndex, optionName.Length);
					expression = ExpressionParser.ParseExpression(token,
					TokenzierContext.FromText(token), out _, optionIndex);
					return true;
				}

				expression = null;
				return false;
			}

			bool TryParseStringOption(ref string token, string optionName, out string expression)
			{
				var optionIndex = token.IndexOf(optionName, StringComparison.OrdinalIgnoreCase);

				if (optionIndex != -1)
				{
					token = token.Remove(optionIndex, optionName.Length);
					var endOfNameIndex = token.IndexOfAny(GetWhitespaceDelimiters(), optionIndex);

					if (endOfNameIndex == -1)
					{
						endOfNameIndex = token.IndexOf('#', optionIndex);
					}

					if (endOfNameIndex == -1)
					{
						endOfNameIndex = token.Length;
					}

					expression = token.Substring(optionIndex, endOfNameIndex - optionIndex);
					token = token.Remove(optionIndex, endOfNameIndex - optionIndex);

					return true;
				}

				expression = null;
				return false;
			}

			void ValidateAliasName(string alias, string tokenTypeName)
			{
				if (string.IsNullOrWhiteSpace(alias))
				{
					context.Errors.Add(new MorestachioSyntaxError(
					context.CurrentLocation.AddWindow(new CharacterSnippedLocation(1, 1, tokenTypeName)), alias,
					tokenTypeName, tokenTypeName + " name",
					tokenTypeName + " option was specified but did not follow any value."));
					return;
				}

				for (var i = 0; i < alias.Length; i++)
				{
					var c = alias[i];

					if (char.IsLetter(c))
					{
						continue;
					}

					if (char.IsDigit(c))
					{
						if (i == 0)
						{
							context.Errors.Add(new MorestachioSyntaxError(
							context.CurrentLocation.AddWindow(new CharacterSnippedLocation(1, i, alias)), alias,
							tokenTypeName, tokenTypeName + " name",
							tokenTypeName + " option cannot not start with a digit."));
						}

						continue;
					}

					context.Errors.Add(new MorestachioSyntaxError(
					context.CurrentLocation.AddWindow(new CharacterSnippedLocation(1, i, alias)), alias, tokenTypeName,
					tokenTypeName + " name", tokenTypeName + " option can only consists of letters and numbers."));
				}
			}

			//[MethodImpl(MethodImplOptions.AggressiveInlining)]
			bool StartsWith(string token, string keyword)
			{
				return token.StartsWith(keyword, StringComparison.OrdinalIgnoreCase);
			}

			context.TokenizeComments = parserOptions.TokenizeComments;
			context.SetLocation(0);

			void FallbackProcessToken(string s, string tokenValue1)
			{
				//check for custom DocumentItem provider

				void UnmatchedTagBehavior()
				{
					if (parserOptions.UnmatchedTagBehavior.HasFlagFast(Context.Options.UnmatchedTagBehavior
							.LogWarning))
					{
						parserOptions.Logger?.LogWarn(LoggingFormatter.TokenizerEventId,
						$"Unknown Tag '{s}'.",
						new Dictionary<string, object>()
						{
							{ "Location", context.CurrentLocation },
						});
					}

					if (parserOptions.UnmatchedTagBehavior.HasFlagFast(Context.Options.UnmatchedTagBehavior
							.Output))
					{
						tokens.Add(new TokenPair(TokenType.Content, "{{" + s + "}}",
						context.CurrentLocation));
					}

					if (parserOptions.UnmatchedTagBehavior.HasFlagFast(Context.Options.UnmatchedTagBehavior
							.ThrowError))
					{
						context.Errors.Add(new MorestachioUnopendScopeError(context.CurrentLocation
							.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue1)),
						"{{" + s + "}}", s,
						$"Unexpected token " + s));
					}
				}

				var customDocumentProvider =
					parserOptions.CustomDocumentItemProviders.FindTokenProvider(s);

				if (customDocumentProvider != null)
				{
					var tokenPairs = customDocumentProvider
						.Tokenize(
						new CustomDocumentItemProvider.TokenInfo(s, context, scopestack,
						tokenOptions),
						parserOptions);
					tokens.AddRange(tokenPairs);
				}
				else if (s.StartsWith('#'))
				{
					UnmatchedTagBehavior();
				}
				else if (s.StartsWith('^'))
				{
					UnmatchedTagBehavior();
				}
				else if (s.StartsWith('/'))
				{
					UnmatchedTagBehavior();
				}
				else
				{
					//unsingle value.
					var token = s.Trim();
					tokens.Add(new TokenPair(TokenType.EscapedSingleValue,
					token, context.CurrentLocation, ExpressionParser.ParseExpression(token, context),
					tokenOptions));
				}
			}

			foreach (var match in templateString)
			{
				if (match.ContentToken)
				{
					tokens.Add(new TokenPair(TokenType.Content, match.Value,
					context.CurrentLocation));
					continue;
				}

				var tokenValue = match.Value;
				var trimmedToken = tokenValue
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
					else if (trimmedToken.IsEquals('!'))
					{
						context.CommentIntend++;
					}
				}
				else
				{
					//check if the token is appended by a -|
					if (trimmedToken.StartsWith("-|") || context.TrimLeading)
					{
						if (!context.TrimLeading)
						{
							trimmedToken = trimmedToken.Substring("-|".Length).Trim();
						}

						tokenOptions.Add(new PersistantTokenOption("Embedded.TrimLeading", true));
					}

					if (trimmedToken.StartsWith("--|") || context.TrimAllLeading)
					{
						if (!context.TrimLeading)
						{
							trimmedToken = trimmedToken.Substring("--|".Length).Trim();
						}

						tokenOptions.Add(new PersistantTokenOption("Embedded.TrimAllLeading", true));
					}

					//check if the token is appended by a |-
					if (trimmedToken.EndsWith("|-") || context.TrimTailing)
					{
						if (!context.TrimTailing)
						{
							trimmedToken = trimmedToken.Remove(trimmedToken.Length - "|-".Length).Trim();
						}

						tokenOptions.Add(new PersistantTokenOption("Embedded.TrimTailing", true));
					}

					if (trimmedToken.EndsWith("|--") || context.TrimAllTailing)
					{
						if (!context.TrimTailing)
						{
							trimmedToken = trimmedToken.Remove(trimmedToken.Length - "|--".Length).Trim();
						}

						tokenOptions.Add(new PersistantTokenOption("Embedded.TrimAllTailing", true));
					}

					//yield front content.
					if (match.Index > context.Character && match.PreText != null)
					{
						tokens.Add(new TokenPair(TokenType.Content, match.PreText, context.CurrentLocation));
					}

					context.SetLocation(match.Index + context._prefixToken.Length);

					if (trimmedToken.StartsWith('#'))
					{
						if (StartsWith(trimmedToken, "#declare "))
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
								context.CurrentLocation, tokenOptions));
							}
						}
						else if (StartsWith(trimmedToken, "#include "))
						{
							parserOptions.Logger?.LogWarn(LoggingFormatter.TokenizerEventId,
							"Use the new #Import tag instead of the #include tag", new Dictionary<string, object>()
							{
								{ "Location", context.CurrentLocation },
							});
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

								tokens.Add(new TokenPair(TokenType.RenderPartial, partialName, context.CurrentLocation,
								exp,
								tokenOptions));
							}
						}
						else if (StartsWith(trimmedToken, "#import "))
						{
							var token = trimmedToken.TrimStart('#').Substring("import".Length)
								.Trim(Tokenizer.GetWhitespaceDelimiters());
							var tokenNameExpression = ExtractExpression(ref token);

							if (TryParseExpressionOption(ref token, "#WITH", out var withExpression))
							{
								tokenOptions.Add(new TokenOption("Context", withExpression));
							}

							//late bound expression, cannot check at parse time for existance
							tokens.Add(new TokenPair(TokenType.ImportPartial,
							context.CurrentLocation, tokenNameExpression, tokenOptions));
						}
						else if (StartsWith(trimmedToken, "#each "))
						{
							var token = TrimToken(trimmedToken, "each");
							TryParseStringOption(ref token, GetAsKeyword(), out var alias);

							scopestack.Push(new ScopeStackItem(TokenType.CollectionOpen, alias ?? token, match.Index));

							if (token.Trim() != "")
							{
								token = token.Trim();
								tokens.Add(new TokenPair(TokenType.CollectionOpen,
								token, context.CurrentLocation, ExpressionParser.ParseExpression(token, context),
								tokenOptions));
							}
							else
							{
								context.Errors.Add(new InvalidPathSyntaxError(context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), ""));
							}

							if (!string.IsNullOrWhiteSpace(alias))
							{
								ValidateAliasName(alias, "AS");
								context.AdvanceLocation("each ".Length + alias.Length);
								tokens.Add(new TokenPair(TokenType.Alias, alias, context.CurrentLocation));
							}
						}
						else if (StartsWith(trimmedToken, "#foreach "))
						{
							var token = TrimToken(trimmedToken, "foreach");
							var inKeywordLocation = token.IndexOf("IN", StringComparison.OrdinalIgnoreCase);

							if (inKeywordLocation == -1)
							{
								context.Errors.Add(new MorestachioSyntaxError(context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "#foreach", "in", "in",
								"the foreach keyword expects the format of: '{{#FOREACH item IN list}}'"));
							}
							else
							{
								var alias = token.Substring(0, inKeywordLocation).Trim();

								if (string.IsNullOrWhiteSpace(alias))
								{
									context.Errors.Add(new MorestachioSyntaxError(context.CurrentLocation
										.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "#foreach", "in",
									"in",
									"the foreach keyword expects the format of: '{{#FOREACH item IN list}}'"));
								}
								else
								{
									ValidateAliasName(alias, "IN");
									var expression = token.Substring(inKeywordLocation + 2);

									scopestack.Push(new ScopeStackItem(TokenType.ForeachCollectionOpen,
									expression ?? token,
									match.Index));
									tokenOptions.Add(new TokenOption("Alias", alias));

									expression = expression.Trim();

									if (!string.IsNullOrWhiteSpace(expression))
									{
										tokens.Add(new TokenPair(TokenType.ForeachCollectionOpen,
										token,
										context.CurrentLocation,
										ExpressionParser.ParseExpression(expression, context),
										tokenOptions));
									}
									else
									{
										context.Errors.Add(new InvalidPathSyntaxError(context.CurrentLocation
											.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), ""));
									}
								}
							}
						}
						else if (StartsWith(trimmedToken, "#while "))
						{
							var token = TrimToken(trimmedToken, "while");
							TryParseStringOption(ref token, GetAsKeyword(), out var alias);

							scopestack.Push(new ScopeStackItem(TokenType.WhileLoopOpen, token, match.Index));

							if (token.Trim() != "")
							{
								token = token.Trim();
								tokens.Add(new TokenPair(TokenType.WhileLoopOpen,
								token, context.CurrentLocation, ExpressionParser.ParseExpression(token, context),
								tokenOptions));
							}
							else
							{
								context.Errors.Add(new InvalidPathSyntaxError(context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), ""));
							}

							if (!string.IsNullOrWhiteSpace(alias))
							{
								ValidateAliasName(alias, "AS");
								context.AdvanceLocation("each ".Length + alias.Length);
								tokens.Add(new TokenPair(TokenType.Alias, alias,
								context.CurrentLocation));
							}
						}
						else if (StartsWith(trimmedToken, "#do "))
						{
							var token = TrimToken(trimmedToken, "do");
							TryParseStringOption(ref token, GetAsKeyword(), out var alias);

							scopestack.Push(new ScopeStackItem(TokenType.DoLoopOpen, token, match.Index));

							if (token.Trim() != "")
							{
								token = token.Trim();
								tokens.Add(new TokenPair(TokenType.DoLoopOpen,
								token, context.CurrentLocation, ExpressionParser.ParseExpression(token, context),
								tokenOptions));
							}
							else
							{
								context.Errors.Add(new InvalidPathSyntaxError(context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), ""));
							}

							if (!string.IsNullOrWhiteSpace(alias))
							{
								ValidateAliasName(alias, "AS");
								context.AdvanceLocation("do ".Length + alias.Length);
								tokens.Add(new TokenPair(TokenType.Alias, alias,
								context.CurrentLocation));
							}
						}
						else if (StartsWith(trimmedToken, "#repeat "))
						{
							var token = TrimToken(trimmedToken, "repeat");
							TryParseStringOption(ref token, GetAsKeyword(), out var alias);

							scopestack.Push(new ScopeStackItem(TokenType.RepeatLoopOpen, token, match.Index));

							if (token.Trim() != "")
							{
								token = token.Trim();
								tokens.Add(new TokenPair(TokenType.RepeatLoopOpen,
								token, context.CurrentLocation, ExpressionParser.ParseExpression(token, context),
								tokenOptions));
							}
							else
							{
								context.Errors.Add(new InvalidPathSyntaxError(context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), ""));
							}

							if (!string.IsNullOrWhiteSpace(alias))
							{
								ValidateAliasName(alias, "AS");
								context.AdvanceLocation("repeat ".Length + alias.Length);
								tokens.Add(new TokenPair(TokenType.Alias, alias,
								context.CurrentLocation));
							}
						}
						else if (StartsWith(trimmedToken, "#switch "))
						{
							var token = TrimToken(trimmedToken, "switch");
							TryParseStringOption(ref token, GetAsKeyword(), out var alias);
							var shouldScope = TryParseFlagOption(ref token, "#SCOPE");

							if (alias != null)
							{
								context.Errors.Add(new MorestachioSyntaxError(
								context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "#switch", "AS",
								"No Alias"));
							}

							scopestack.Push(new ScopeStackItem(TokenType.SwitchOpen, token, match.Index));

							if (token.Trim() != "")
							{
								token = token.Trim();
								tokenOptions.Add(new TokenOption("ScopeTo", shouldScope));
								tokens.Add(new TokenPair(TokenType.SwitchOpen,
								context.CurrentLocation, ExpressionParser.ParseExpression(token, context),
								tokenOptions));
							}
							else
							{
								context.Errors.Add(new InvalidPathSyntaxError(context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), ""));
							}
						}
						else if (StartsWith(trimmedToken, "#case "))
						{
							var token = TrimToken(trimmedToken, "case");
							TryParseStringOption(ref token, GetAsKeyword(), out var alias);

							if (alias != null)
							{
								context.Errors.Add(new MorestachioSyntaxError(
								context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "#case", "AS",
								"No Alias"));
							}

							scopestack.Push(new ScopeStackItem(TokenType.SwitchCaseOpen, token, match.Index));

							if (token.Trim() != "")
							{
								token = token.Trim();
								tokens.Add(new TokenPair(TokenType.SwitchCaseOpen,
								token, context.CurrentLocation, ExpressionParser.ParseExpression(token, context),
								tokenOptions));
							}
							else
							{
								context.Errors.Add(new InvalidPathSyntaxError(context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), ""));
							}
						}
						else if (StartsWith(trimmedToken, "#if "))
						{
							var token = TrimToken(trimmedToken, "if");
							TryParseStringOption(ref token, GetAsKeyword(), out var alias);

							if (alias != null)
							{
								context.Errors.Add(new MorestachioSyntaxError(
								context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "^if", "AS",
								"No Alias"));
							}

							scopestack.Push(new ScopeStackItem(TokenType.If, token, match.Index));

							if (token.Trim() != "")
							{
								token = token.Trim();
								tokens.Add(new TokenPair(TokenType.If,
								token, context.CurrentLocation, ExpressionParser.ParseExpression(token, context),
								tokenOptions));
							}
							else
							{
								context.Errors.Add(new InvalidPathSyntaxError(context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), ""));
							}
						}
						else if (trimmedToken.Equals("#ifelse", StringComparison.OrdinalIgnoreCase))
						{
							parserOptions.Logger?.LogWarn(LoggingFormatter.TokenizerEventId,
							"IFELSE is considered obsolete and should no longer be used. Just use the #ELSE keyword instead",
							new Dictionary<string, object>()
							{
								{ "Location", context.CurrentLocation },
							});
						}
						else if (trimmedToken.Equals("#else", StringComparison.OrdinalIgnoreCase))
						{
							ScopeStackItem currentScope;

							//the new else block must be located inside an #IF or ^IF block 
							if (
								scopestack.Count > 0 &&
								(!(currentScope = scopestack.Peek()).Equals(default)) &&
								(currentScope.TokenType == TokenType.If
									|| currentScope.TokenType == TokenType.IfNot
									|| currentScope.TokenType == TokenType.ElseIf))
							{
								scopestack.Push(new ScopeStackItem(TokenType.Else, trimmedToken, match.Index));
								tokens.Add(new TokenPair(TokenType.Else, trimmedToken,
								context.CurrentLocation, tokenOptions));
							}
							else
							{
								context.Errors.Add(new MorestachioSyntaxError(
								context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)),
								"else", "{{#ELSE}}",
								"Expected the else keyword to be a direct descended of an #if or #elseif"));
							}
						}
						else if (StartsWith(trimmedToken, "#elseif "))
						{
							ScopeStackItem currentScope;

							//the new else block must be located inside an #IF or ^IF block 
							if (
								scopestack.Count > 0 &&
								(!(currentScope = scopestack.Peek()).Equals(default)) &&
								(currentScope.TokenType == TokenType.If
									|| currentScope.TokenType == TokenType.IfNot
									|| currentScope.TokenType == TokenType.ElseIf))
							{
								var token = TrimToken(trimmedToken, "elseif");
								TryParseStringOption(ref token, GetAsKeyword(), out var alias);

								if (alias != null)
								{
									context.Errors.Add(new MorestachioSyntaxError(
									context.CurrentLocation
										.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "^elseif", "AS",
									"No Alias"));
								}

								scopestack.Push(new ScopeStackItem(TokenType.ElseIf, token, match.Index));
								tokens.Add(new TokenPair(TokenType.ElseIf, context.CurrentLocation,
								ExpressionParser.ParseExpression(token, context), tokenOptions));
							}
							else
							{
								context.Errors.Add(new MorestachioSyntaxError(
								context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)),
								"else", "{{#ELSEIF}}",
								"Expected the elseif keyword to be a direct descended of an #if"));
							}
						}
						else if (trimmedToken.Equals("#default", StringComparison.OrdinalIgnoreCase))
						{
							var token = TrimToken(trimmedToken, "default");
							TryParseStringOption(ref token, GetAsKeyword(), out var alias);

							if (alias != null)
							{
								context.Errors.Add(new MorestachioSyntaxError(
								context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "#default", "AS",
								"No Alias"));
							}

							scopestack.Push(new ScopeStackItem(TokenType.SwitchDefaultOpen, token, match.Index));

							if (token.Trim() == "")
							{
								token = token.Trim();
								tokens.Add(new TokenPair(TokenType.SwitchDefaultOpen,
								token,
								context.CurrentLocation, tokenOptions));
							}
							else
							{
								context.Errors.Add(new InvalidPathSyntaxError(context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), ""));
							}
						}
						else if (StartsWith(trimmedToken, "#var "))
						{
							tokens.Add(ExpressionParser.TokenizeVariableAssignment(trimmedToken,
							context, TokenType.VariableVar, tokenOptions));
						}
						else if (StartsWith(trimmedToken, "#let "))
						{
							if (scopestack.Count == 0)
							{
								parserOptions.Logger?.LogWarn(LoggingFormatter.TokenizerEventId,
								"Using an #let on the topmost of your template has the same effect as using an #var",
								new Dictionary<string, object>()
								{
									{ "Location", context.CurrentLocation },
								});
							}

							tokens.Add(ExpressionParser.TokenizeVariableAssignment(trimmedToken,
							context, TokenType.VariableLet, tokenOptions));
						}
						else if (StartsWith(trimmedToken, "#scope "))
						{
							var token = TrimToken(trimmedToken, "scope ");
							TryParseStringOption(ref token, GetAsKeyword(), out var alias);

							scopestack.Push(new ScopeStackItem(TokenType.ElementOpen, alias ?? token, match.Index));

							if (token.Trim() != "")
							{
								token = token.Trim();
								tokens.Add(new TokenPair(TokenType.ElementOpen,
								token, context.CurrentLocation, ExpressionParser.ParseExpression(token, context),
								tokenOptions));
							}
							else
							{
								context.Errors.Add(new InvalidPathSyntaxError(context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), ""));
							}

							if (!string.IsNullOrWhiteSpace(alias))
							{
								ValidateAliasName(alias, "AS");
								context.AdvanceLocation("scope ".Length + alias.Length);
								tokens.Add(new TokenPair(TokenType.Alias, alias,
								context.CurrentLocation));
							}
						}
						else if (trimmedToken.Equals("#NL", StringComparison.OrdinalIgnoreCase))
						{
							tokens.Add(new TokenPair(TokenType.WriteLineBreak, trimmedToken, context.CurrentLocation,
							tokenOptions));
						}
						else if (trimmedToken.Equals("#TNL", StringComparison.OrdinalIgnoreCase))
						{
							tokens.Add(new TokenPair(TokenType.TrimLineBreak, trimmedToken, context.CurrentLocation,
							tokenOptions));
						}
						else if (trimmedToken.Equals("#TNLS", StringComparison.OrdinalIgnoreCase))
						{
							tokenOptions.Add(new TokenOption("All", true));
							tokens.Add(new TokenPair(TokenType.TrimLineBreaks, trimmedToken, context.CurrentLocation,
							tokenOptions));
						}
						else if (trimmedToken.Equals("#TRIMALL", StringComparison.OrdinalIgnoreCase))
						{
							tokens.Add(new TokenPair(TokenType.TrimEverything, trimmedToken, context.CurrentLocation,
							tokenOptions));
						}
						else if (StartsWith(trimmedToken, "#ISOLATE "))
						{
							var token = TrimToken(trimmedToken, "ISOLATE ");
							IsolationOptions scope = 0;

							if (TryParseFlagOption(ref token, "#VARIABLES"))
							{
								scope |= IsolationOptions.VariableIsolation;
							}

							if (TryParseExpressionOption(ref token, "#SCOPE ", out var scopeExpression))
							{
								tokenOptions.Add(new TokenOption("IsolationScopeArg", scopeExpression));
								scope |= IsolationOptions.ScopeIsolation;
							}

							tokenOptions.Add(new TokenOption("IsolationType", scope));
							tokens.Add(new TokenPair(TokenType.IsolationScopeOpen, token, context.CurrentLocation,
							tokenOptions));
							scopestack.Push(new ScopeStackItem(TokenType.IsolationScopeOpen, token, match.Index));
						}
						else if (StartsWith(trimmedToken, "#SET OPTION "))
						{
							var token = TrimToken(trimmedToken, "SET OPTION ");
							var expectEquals = false;
							string name = null;
							IMorestachioExpression value = null;

							for (var i = 0; i < token.Length; i++)
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
											.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "/",
										"{{#SET OPTION Name = Value}}",
										$" Expected to find '=' or whitespace after name but found '{c}'"));
									}
									else
									{
										name = token.Substring(0, i - 1).Trim();
										value = ExpressionParser.ParseExpression(token.Substring(i + 1).Trim(),
										context);
										break;
									}
								}
							}

							if (string.IsNullOrWhiteSpace(name))
							{
								context.Errors.Add(new MorestachioUnopendScopeError(context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "/",
								"{{#SET OPTION Name = Value}}",
								$" Expected to find '=' after name"));
								break;
							}

							if (value == null)
							{
								context.Errors.Add(new MorestachioUnopendScopeError(context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "/",
								"{{#SET OPTION Name = Value}}",
								$" Expected to find an expression after '='"));
								break;
							}

							await context.SetOption(name, value, parserOptions);
						}
						else
						{
							FallbackProcessToken(trimmedToken, tokenValue);
						}
					}
					else if (trimmedToken.StartsWith('/'))
					{
						if (trimmedToken.Equals("/declare", StringComparison.OrdinalIgnoreCase))
						{
							if (scopestack.Any() && scopestack.Peek().TokenType == TokenType.PartialDeclarationOpen)
							{
								var token = scopestack.Pop().Value;
								tokens.Add(new TokenPair(TokenType.PartialDeclarationClose, token,
								context.CurrentLocation, tokenOptions));
							}
							else
							{
								context.Errors.Add(new MorestachioUnopendScopeError(context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "declare",
								"{{#declare name}}"));
							}
						}
						else if (trimmedToken.Equals("/each", StringComparison.OrdinalIgnoreCase))
						{
							if (scopestack.Any() && scopestack.Peek().TokenType == TokenType.CollectionOpen)
							{
								var token = scopestack.Pop().Value;
								tokens.Add(new TokenPair(TokenType.CollectionClose, token,
								context.CurrentLocation, tokenOptions));
							}
							else
							{
								context.Errors.Add(new MorestachioUnopendScopeError(context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "each",
								"{{#each name}}"));
							}
						}
						else if (trimmedToken.Equals("/foreach", StringComparison.OrdinalIgnoreCase))
						{
							if (scopestack.Any() && scopestack.Peek().TokenType == TokenType.ForeachCollectionOpen)
							{
								var token = scopestack.Pop().Value;
								tokens.Add(new TokenPair(TokenType.ForeachCollectionClose, token,
								context.CurrentLocation, tokenOptions));
							}
							else
							{
								context.Errors.Add(new MorestachioUnopendScopeError(context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "each",
								"{{#each name}}"));
							}
						}
						else if (trimmedToken.Equals("/while", StringComparison.OrdinalIgnoreCase))
						{
							if (scopestack.Any() && scopestack.Peek().TokenType == TokenType.WhileLoopOpen)
							{
								var token = scopestack.Pop().Value;
								tokens.Add(new TokenPair(TokenType.WhileLoopClose, token,
								context.CurrentLocation, tokenOptions));
							}
							else
							{
								context.Errors.Add(new MorestachioUnopendScopeError(context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "while",
								"{{#while Expression}}"));
							}
						}
						else if (trimmedToken.Equals("/do", StringComparison.OrdinalIgnoreCase))
						{
							if (scopestack.Any() && scopestack.Peek().TokenType == TokenType.DoLoopOpen)
							{
								var token = scopestack.Pop().Value;
								tokens.Add(new TokenPair(TokenType.DoLoopClose, token,
								context.CurrentLocation, tokenOptions));
							}
							else
							{
								context.Errors.Add(new MorestachioUnopendScopeError(context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "do",
								"{{#do Expression}}"));
							}
						}
						else if (trimmedToken.Equals("/repeat", StringComparison.OrdinalIgnoreCase))
						{
							if (scopestack.Any() && scopestack.Peek().TokenType == TokenType.RepeatLoopOpen)
							{
								var token = scopestack.Pop().Value;
								tokens.Add(new TokenPair(TokenType.RepeatLoopClose, token,
								context.CurrentLocation, tokenOptions));
							}
							else
							{
								context.Errors.Add(new MorestachioUnopendScopeError(context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "repeat",
								"{{#repeat Expression}}"));
							}
						}
						else if (trimmedToken.Equals("/switch", StringComparison.OrdinalIgnoreCase))
						{
							if (scopestack.Any() && scopestack.Peek().TokenType == TokenType.SwitchOpen)
							{
								var token = scopestack.Pop().Value;
								tokens.Add(new TokenPair(TokenType.SwitchClose, token,
								context.CurrentLocation, tokenOptions));
							}
							else
							{
								context.Errors.Add(new MorestachioUnopendScopeError(context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "switch",
								"{{#switch Expression}}"));
							}
						}
						else if (trimmedToken.Equals("/case", StringComparison.OrdinalIgnoreCase))
						{
							if (scopestack.Any() && scopestack.Peek().TokenType == TokenType.SwitchCaseOpen)
							{
								var token = scopestack.Pop().Value;
								tokens.Add(new TokenPair(TokenType.SwitchCaseClose, token,
								context.CurrentLocation, tokenOptions));
							}
							else
							{
								context.Errors.Add(new MorestachioUnopendScopeError(context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "case",
								"{{#case Expression}}"));
							}
						}
						else if (trimmedToken.Equals("/default", StringComparison.OrdinalIgnoreCase))
						{
							if (scopestack.Any() && scopestack.Peek().TokenType == TokenType.SwitchDefaultOpen)
							{
								var token = scopestack.Pop().Value;
								tokens.Add(new TokenPair(TokenType.SwitchDefaultClose, token,
								context.CurrentLocation, tokenOptions));
							}
							else
							{
								context.Errors.Add(new MorestachioUnopendScopeError(context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "default",
								"{{#default}}"));
							}
						}
						else if (trimmedToken.Equals("/if", StringComparison.OrdinalIgnoreCase))
						{
							EndIf(match);
						}
						else if (trimmedToken.Equals("/elseif", StringComparison.OrdinalIgnoreCase))
						{
							if (scopestack.Any() && scopestack.Peek().TokenType == TokenType.ElseIf)
							{
								var token = scopestack.Pop().Value;
								tokens.Add(new TokenPair(TokenType.ElseIfClose, token,
								context.CurrentLocation, tokenOptions));
								//EndIf(match);//as the parent is still an if block close that one of
							}
							else
							{
								context.Errors.Add(new MorestachioUnopendScopeError(
								context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "elseif",
								"{{#ELSEIF expression}}"));
							}
						}
						else if (trimmedToken.Equals("/else", StringComparison.OrdinalIgnoreCase))
						{
							if (scopestack.Any() && scopestack.Peek().TokenType == TokenType.Else)
							{
								var token = scopestack.Pop().Value;
								tokens.Add(new TokenPair(TokenType.ElseClose, token,
								context.CurrentLocation, tokenOptions));
								//EndIf(match);//as the parent is still an if block close that one of
							}
							else
							{
								context.Errors.Add(new MorestachioUnopendScopeError(
								context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "else",
								"{{#else name}}"));
							}
						}
						else if (trimmedToken.Equals("/scope", StringComparison.OrdinalIgnoreCase))
						{
							if (scopestack.Any() &&
								(scopestack.Peek().TokenType == TokenType.ElementOpen ||
									scopestack.Peek().TokenType == TokenType.InvertedElementOpen))
							{
								var token = scopestack.Pop().Value;

								tokens.Add(new TokenPair(TokenType.ElementClose, token,
								context.CurrentLocation, tokenOptions));
							}
							else
							{
								context.Errors.Add(new MorestachioUnopendScopeError(context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "/", "{{#SCOPE path}}",
								" There are more closing elements then open."));
							}
						}
						else if (trimmedToken.Equals("/ISOLATE", StringComparison.OrdinalIgnoreCase))
						{
							if (scopestack.Any() &&
								(scopestack.Peek().TokenType == TokenType.IsolationScopeOpen))
							{
								var token = scopestack.Pop().Value;

								tokens.Add(new TokenPair(TokenType.IsolationScopeClose, token,
								context.CurrentLocation, tokenOptions));
							}
							else
							{
								context.Errors.Add(new MorestachioUnopendScopeError(context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "/",
								"{{#ISOLATION ...}}",
								" There are more closing elements then open."));
							}
						}
						else
						{
							FallbackProcessToken(trimmedToken, tokenValue);
						}
					}
					else if (trimmedToken.StartsWith('^'))
					{
						if (StartsWith(trimmedToken, "^if "))
						{
							var token = TrimToken(trimmedToken, "if", '^');
							TryParseStringOption(ref token, GetAsKeyword(), out var alias);

							if (alias != null)
							{
								context.Errors.Add(new MorestachioSyntaxError(
								context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), "^if", "AS",
								"No Alias"));
							}

							scopestack.Push(new ScopeStackItem(TokenType.IfNot, token, match.Index));

							if (token.Trim() != "")
							{
								token = token.Trim();
								tokens.Add(new TokenPair(TokenType.IfNot,
								token, context.CurrentLocation, ExpressionParser.ParseExpression(token, context),
								tokenOptions));
							}
							else
							{
								context.Errors.Add(new InvalidPathSyntaxError(context.CurrentLocation
									.AddWindow(new CharacterSnippedLocation(1, 1, tokenValue)), ""));
							}
						}
						else if (StartsWith(trimmedToken, "^scope "))
						{
							//open inverted group
							var token = TrimToken(trimmedToken, "scope ", '^');
							TryParseStringOption(ref token, GetAsKeyword(), out var alias);
							scopestack.Push(new ScopeStackItem(TokenType.InvertedElementOpen, alias ?? token,
							match.Index));
							tokens.Add(new TokenPair(TokenType.InvertedElementOpen,
							token, context.CurrentLocation, ExpressionParser.ParseExpression(token, context),
							tokenOptions));

							if (!string.IsNullOrWhiteSpace(alias))
							{
								ValidateAliasName(alias, "AS");
								context.AdvanceLocation(1 + alias.Length);
								tokens.Add(new TokenPair(TokenType.Alias, alias,
								context.CurrentLocation));
							}
						}
						else
						{
							parserOptions.Logger?.LogWarn(LoggingFormatter.TokenizerEventId,
							"Use the new {{^scope path}} block instead of the {{^path}} block",
							new Dictionary<string, object>()
							{
								{ "Location", context.CurrentLocation },
							});
							//open inverted group
							var token = trimmedToken.TrimStart('^').Trim();
							TryParseStringOption(ref token, GetAsKeyword(), out var alias);
							scopestack.Push(new ScopeStackItem(TokenType.InvertedElementOpen, alias ?? token,
							match.Index));
							tokenOptions.Add(new PersistantTokenOption("Render.LegacyStyle", true));
							tokens.Add(new TokenPair(TokenType.InvertedElementOpen,
							token, context.CurrentLocation, ExpressionParser.ParseExpression(token, context),
							tokenOptions));

							if (!string.IsNullOrWhiteSpace(alias))
							{
								ValidateAliasName(alias, "AS");
								context.AdvanceLocation(1 + alias.Length);
								tokens.Add(new TokenPair(TokenType.Alias, alias,
								context.CurrentLocation));
							}
						}
					}

					else if (trimmedToken.IsEquals('!'))
					{
						tokens.Add(new TokenPair(TokenType.BlockComment, trimmedToken, context.CurrentLocation,
						tokenOptions));
					}
					else if (trimmedToken.StartsWith('!'))
					{
						tokens.Add(
						new TokenPair(TokenType.Comment, trimmedToken, context.CurrentLocation, tokenOptions));
					}
					else if (trimmedToken.StartsWith('&'))
					{
						//escaped single element
						var token = trimmedToken.TrimStart('&').Trim();
						tokens.Add(new TokenPair(TokenType.UnescapedSingleValue,
						token, context.CurrentLocation, ExpressionParser.ParseExpression(token, context),
						tokenOptions));
					}
					else
					{
						FallbackProcessToken(trimmedToken, tokenValue);
					}

					tokenOptions.Clear();

					//move forward in the string.
					if (context.Character > match.Index + match.Length)
					{
						throw new InvalidOperationException("Internal index location error");
					}

					context.SetLocation(match.Index + match.Length);
				}
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
	}
}