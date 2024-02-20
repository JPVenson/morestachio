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

namespace Morestachio.Framework.Tokenizing;

/// <summary>
///     Reads in a mustache template and lexes it into tokens.
/// </summary>
/// <exception cref="IndexedParseException"></exception>
public class Tokenizer
{
	internal static readonly Regex PartialIncludeRegEx
		= new("Include (\\w*)( (?:With) )?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

	private static readonly char[] _whitespaceDelimiters = { '\r', '\n', '\t', ' ' };

	//internal static TextRange HumanizeTextRange(int characterIndex, List<int> lines)
	//{
	//	var line = lines.BinarySearch(characterIndex);
	//	line = line < 0 ? ~line : line;
	//	var charIdx = characterIndex;

	//	//in both of these cases, we want to increment the char index by one to account for the '\n' that is skipped in the indexes.
	//	if (line < lines.Count && line > 0)
	//	{
	//		charIdx = characterIndex - (lines[line - 1] + 1);
	//	}
	//	else if (line > 0)
	//	{
	//		charIdx = characterIndex - (lines.LastOrDefault() + 1);
	//	}

	//	//Humans count from 1, so let's do that, too (hence the "+1" on these).
	//	var textLocation = new TextRange(line + 1, charIdx + 1, characterIndex + 1);

	//	return textLocation;
	//}

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
	internal static bool IsStringDelimiter(in char formatChar)
	{
		return formatChar is '\'' or '\"';
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static string GetAsKeyword()
	{
		return " AS ";
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsEndOfExpression(in char formatChar)
	{
		return IsEndOfWholeExpression(in formatChar) || IsEndOfExpressionSection(in formatChar);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsEndOfExpressionSection(in char formatChar)
	{
		return formatChar == '#';
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsEndOfWholeExpression(in char formatChar)
	{
		return formatChar == ';';
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static char[] GetWhitespaceDelimiters()
	{
		return _whitespaceDelimiters;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsWhiteSpaceDelimiter(in char formatChar)
	{
		return formatChar is '\r' or '\n' or '\t' or ' ';
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsExpressionPathChar(in char formatChar)
	{
		return formatChar is '?' or '/' || IsStartOfExpressionPathChar(in formatChar);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsStartOfExpressionPathChar(in char formatChar)
	{
		return formatChar is '$' or '?' || IsSingleExpressionPathChar(in formatChar);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsSingleExpressionPathChar(in char formatChar)
	{
		return formatChar is '.' or '~' || IsExpressionDataPathChar(in formatChar);
		//|| IsCharRegex.IsMatch(formatChar.ToString());
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsExpressionDataPathChar(in char formatChar)
	{
		return char.IsLetterOrDigit(formatChar) || formatChar == '_';
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsNumberExpressionChar(in char formatChar)
	{
		return char.IsDigit(formatChar);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsExpressionChar(in char formatChar)
	{
		return IsExpressionPathChar(in formatChar) ||
			formatChar is '(' or ')';
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsPathDelimiterChar(in char formatChar)
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
	internal static bool IsOperationChar(in char formatChar)
	{
		return
			formatChar is '+'
				or '-'
				or '*'
				or '/'
				or '^'
				or '%'
				or '<'
				or '>'
				or '='
				or '!'
				or '&'
				or '?'
				or '!'
				or '|';
	}

	/// <summary>
	///     This method is hard coded for performance reasons. If modified here, the changes must be reflected in
	///     <see cref="MorestachioOperator" />
	/// </summary>
	/// <param name="operatorText"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsOperationString(in string operatorText)
	{
		return
			operatorText is "+"
				or "-"
				or "*"
				or "/"
				or "^"
				or "%"
				or "<<"
				or ">>"
				or "=="
				or "!="
				or "<"
				or "<="
				or ">"
				or ">="
				or "&&"
				or "||"
				or "<?"
				or ">?"
				or "!"
				or "??";
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsEndOfFormatterArgument(in char? formatChar)
	{
		return formatChar is ',' or '.' or ')';
	}

	/// <summary>
	///     Goes through the template and evaluates all tokens that are enclosed by {{ }}.
	/// </summary>
	/// <param name="parserOptions"></param>
	/// <param name="context"></param>
	/// <returns></returns>
	public static TokenizerResultPromise Tokenize(
		ParserOptions parserOptions,
		TokenzierContext context
	)
	{
		return Tokenize(parserOptions, context, parserOptions.Template.Matches(context));
	}

	internal static async TokenizerResultPromise Tokenize(
		ParserOptions parserOptions,
		TokenzierContext context,
		IEnumerable<TokenMatch> templateString
	)
	{
		var scopestack = new Stack<ScopeStackItem>();
		List<string> partialsNames;

		if (parserOptions.PartialsStore is IAsyncPartialsStore asyncPartialStore)
		{
			partialsNames
				= new List<string>(await asyncPartialStore.GetNamesAsync(parserOptions).ConfigureAwait(false));
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

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		context.TokenizeComments = parserOptions.TokenizeComments;

		foreach (var match in templateString)
		{
			if (match.ContentToken)
			{
				tokens.Add(new TokenPair(TokenType.Content, match.Value, match.Range));

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

					//if (context.CommentIntend == 0)
					//{
					//	//move forward in the string.
					//	if (context.Character > match.Range + match.Length)
					//	{
					//		throw new InvalidOperationException("Internal index location error");
					//	}

					//	context.SetLocation(match.Range + match.Length);
					//}
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
				//if (!string.IsNullOrEmpty(match.PreText))
				//{
				//	tokens.Add(new TokenPair(TokenType.Content, match.PreText, match.Range));
				//}

				//context.SetLocation(match.Range + context._prefixToken.Length);

				if (trimmedToken.StartsWith('#'))
				{
					if (StartsWith(trimmedToken, "#declare "))
					{
						var token = TrimToken(trimmedToken, "declare ");
						scopestack.Push(new ScopeStackItem(TokenType.PartialDeclarationOpen, token, match.Range));

						if (string.IsNullOrWhiteSpace(token))
						{
							context.Errors.Add(new MorestachioSyntaxError(match.Range, "open", "declare",
								"{{#declare name}}", " Missing the Name."));
						}
						else
						{
							partialsNames.Add(token);

							tokens.Add(
								new TokenPair(TokenType.PartialDeclarationOpen, token, match.Range, tokenOptions));
						}
					}
					else if (StartsWith(trimmedToken, "#include "))
					{
						parserOptions.Logger?.LogWarn(LoggingFormatter.TokenizerEventId,
							"Use the new #Import tag instead of the #include tag", new Dictionary<string, object>
							{
								{ "Location", match.Range }
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
							context.Errors.Add(new MorestachioSyntaxError(match.Range,
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
								exp = ExpressionParser.ParseExpression(partialContext, context, match.Range.RangeStart)
									.Expression;
							}

							tokens.Add(new TokenPair(TokenType.RenderPartial, partialName, match.Range,
								exp,
								tokenOptions));
						}
					}
					else if (StartsWith(trimmedToken, "#import "))
					{
						var token = trimmedToken.TrimStart('#')
							.Substring("import".Length)
							.Trim(GetWhitespaceDelimiters());
						var tokenNameExpression = ExtractExpression(ref token, context, match.Range.RangeStart);

						if (TryParseExpressionOption(ref token, "#WITH", match.Range.RangeStart, context,
								out var withExpression))
						{
							tokenOptions.Add(new TokenOption("Context", withExpression));
						}

						//late bound expression, cannot check at parse time for existance
						tokens.Add(new TokenPair(TokenType.ImportPartial,
							match.Range, tokenNameExpression, tokenOptions));
					}
					else if (StartsWith(trimmedToken, "#each "))
					{
						var token = TrimToken(trimmedToken, "each");
						TryParseStringOption(ref token, context, GetAsKeyword(), out var alias, in match);
						scopestack.Push(new ScopeStackItem(TokenType.CollectionOpen, alias.Text ?? token, match.Range));

						if (token.Trim() != "")
						{
							token = token.Trim();

							tokens.Add(new TokenPair(TokenType.CollectionOpen,
								token,
								match.Range,
								ExpressionParser.ParseExpression(token, context, match.Range.RangeStart).Expression,
								tokenOptions));
						}
						else
						{
							context.Errors.Add(new InvalidPathSyntaxError(match.Range, ""));
						}

						if (!string.IsNullOrWhiteSpace(alias.Text))
						{
							ValidateAliasName(alias.Text, "AS", context, match);
							//context.AdvanceLocation("each ".Length + alias.Length);
							tokens.Add(new TokenPair(TokenType.Alias, alias.Text, alias.TextRange));
						}
					}
					else if (StartsWith(trimmedToken, "#foreach "))
					{
						var token = TrimToken(trimmedToken, "foreach");
						var inKeywordLocation = token.IndexOf("IN", StringComparison.OrdinalIgnoreCase);

						if (inKeywordLocation == -1)
						{
							context.Errors.Add(new MorestachioSyntaxError(match.Range, "#foreach", "in", "in",
								"the foreach keyword expects the format of: '{{#FOREACH item IN list}}'"));
						}
						else
						{
							var alias = token.Substring(0, inKeywordLocation).Trim();

							if (string.IsNullOrWhiteSpace(alias))
							{
								context.Errors.Add(new MorestachioSyntaxError(match.Range, "#foreach", "in",
									"in",
									"the foreach keyword expects the format of: '{{#FOREACH item IN list}}'"));
							}
							else
							{
								ValidateAliasName(alias, "IN", context, match);
								var expression = token.Substring(inKeywordLocation + 2);

								scopestack.Push(new ScopeStackItem(TokenType.ForeachCollectionOpen,
									expression ?? token,
									match.Range));
								tokenOptions.Add(new TokenOption("Alias", alias));
								expression = expression.Trim();

								if (!string.IsNullOrWhiteSpace(expression))
								{
									tokens.Add(new TokenPair(TokenType.ForeachCollectionOpen,
										token,
										match.Range,
										ExpressionParser.ParseExpression(expression, context, match.Range.RangeStart)
											.Expression,
										tokenOptions));
								}
								else
								{
									context.Errors.Add(new InvalidPathSyntaxError(match.Range, ""));
								}
							}
						}
					}
					else if (StartsWith(trimmedToken, "#while "))
					{
						var token = TrimToken(trimmedToken, "while");
						TryParseStringOption(ref token, context, GetAsKeyword(), out var alias, in match);
						scopestack.Push(new ScopeStackItem(TokenType.WhileLoopOpen, token, match.Range));

						if (token.Trim() != "")
						{
							token = token.Trim();

							tokens.Add(new TokenPair(TokenType.WhileLoopOpen,
								token,
								match.Range,
								ExpressionParser.ParseExpression(token, context, match.Range.RangeStart).Expression,
								tokenOptions));
						}
						else
						{
							context.Errors.Add(new InvalidPathSyntaxError(match.Range, ""));
						}

						if (!string.IsNullOrWhiteSpace(alias.Text))
						{
							ValidateAliasName(alias.Text, "AS", context, match);
							//context.AdvanceLocation("each ".Length + alias.Length);

							tokens.Add(new TokenPair(TokenType.Alias, alias.Text, match.Range));
						}
					}
					else if (StartsWith(trimmedToken, "#do "))
					{
						var token = TrimToken(trimmedToken, "do");
						TryParseStringOption(ref token, context, GetAsKeyword(), out var alias, in match);
						scopestack.Push(new ScopeStackItem(TokenType.DoLoopOpen, token, match.Range));

						if (token.Trim() != "")
						{
							token = token.Trim();

							tokens.Add(new TokenPair(TokenType.DoLoopOpen,
								token,
								match.Range,
								ExpressionParser.ParseExpression(token, context, match.Range.RangeStart).Expression,
								tokenOptions));
						}
						else
						{
							context.Errors.Add(new InvalidPathSyntaxError(match.Range, ""));
						}

						if (!string.IsNullOrWhiteSpace(alias.Text))
						{
							ValidateAliasName(alias.Text, "AS", context, match);
							//context.AdvanceLocation("do ".Length + alias.Length);

							tokens.Add(new TokenPair(TokenType.Alias, alias.Text, match.Range));
						}
					}
					else if (StartsWith(trimmedToken, "#repeat "))
					{
						var token = TrimToken(trimmedToken, "repeat");
						TryParseStringOption(ref token, context, GetAsKeyword(), out var alias, in match);
						scopestack.Push(new ScopeStackItem(TokenType.RepeatLoopOpen, token, match.Range));

						if (token.Trim() != "")
						{
							token = token.Trim();

							tokens.Add(new TokenPair(TokenType.RepeatLoopOpen,
								token,
								match.Range,
								ExpressionParser.ParseExpression(token, context, match.Range.RangeStart).Expression,
								tokenOptions));
						}
						else
						{
							context.Errors.Add(new InvalidPathSyntaxError(match.Range, ""));
						}

						if (!string.IsNullOrWhiteSpace(alias.Text))
						{
							ValidateAliasName(alias.Text, "AS", context, match);
							//context.AdvanceLocation("repeat ".Length + alias.Length);

							tokens.Add(new TokenPair(TokenType.Alias, alias.Text, match.Range));
						}
					}
					else if (StartsWith(trimmedToken, "#switch "))
					{
						var token = TrimToken(trimmedToken, "switch");
						var shouldScope = TryParseFlagOption(ref token, "#SCOPE");

						if (TryParseStringOption(ref token, context, GetAsKeyword(), out _, in match))
						{
							context.Errors.Add(new MorestachioSyntaxError(
								match.Range,
								"#switch",
								"AS",
								"No Alias"));
						}

						scopestack.Push(new ScopeStackItem(TokenType.SwitchOpen, token, match.Range));

						if (token.Trim() != "")
						{
							token = token.Trim();
							tokenOptions.Add(new TokenOption("ScopeTo", shouldScope));

							tokens.Add(new TokenPair(TokenType.SwitchOpen,
								match.Range,
								ExpressionParser.ParseExpression(token, context, match.Range.RangeStart).Expression,
								tokenOptions));
						}
						else
						{
							context.Errors.Add(new InvalidPathSyntaxError(match.Range, ""));
						}
					}
					else if (StartsWith(trimmedToken, "#case "))
					{
						var token = TrimToken(trimmedToken, "case");

						if (TryParseStringOption(ref token, context, GetAsKeyword(), out _, in match))
						{
							context.Errors.Add(new MorestachioSyntaxError(
								match.Range, "#case", "AS",
								"No Alias"));
						}

						scopestack.Push(new ScopeStackItem(TokenType.SwitchCaseOpen, token, match.Range));

						if (token.Trim() != "")
						{
							token = token.Trim();

							tokens.Add(new TokenPair(TokenType.SwitchCaseOpen,
								token,
								match.Range,
								ExpressionParser.ParseExpression(token, context, match.Range.RangeStart).Expression,
								tokenOptions));
						}
						else
						{
							context.Errors.Add(new InvalidPathSyntaxError(match.Range, ""));
						}
					}
					else if (StartsWith(trimmedToken, "#if "))
					{
						var token = TrimToken(trimmedToken, "if");

						if (TryParseStringOption(ref token, context, GetAsKeyword(), out _, in match))
						{
							context.Errors.Add(new MorestachioSyntaxError(match.Range, "^if", "AS",
								"No Alias"));
						}

						scopestack.Push(new ScopeStackItem(TokenType.If, token, match.Range));

						if (token.Trim() != "")
						{
							token = token.Trim();

							tokens.Add(new TokenPair(TokenType.If,
								token, match.Range,
								ExpressionParser.ParseExpression(token, context, match.Range.RangeStart).Expression,
								tokenOptions));
						}
						else
						{
							context.Errors.Add(new InvalidPathSyntaxError(match.Range, ""));
						}
					}
					else if (StartsWith(trimmedToken, "#elseif "))
					{
						ScopeStackItem currentScope;

						//the new else block must be located inside an #IF or ^IF block 
						if (
							scopestack.Count > 0 &&
							!(currentScope = scopestack.Peek()).Equals(default) &&
							currentScope.TokenType is TokenType.If or TokenType.IfNot or TokenType.ElseIf)
						{
							var token = TrimToken(trimmedToken, "elseif");

							if (TryParseStringOption(ref token, context, GetAsKeyword(), out _, in match))
							{
								context.Errors.Add(new MorestachioSyntaxError(
									match.Range, "^elseif", "AS",
									"No Alias"));
							}

							scopestack.Push(new ScopeStackItem(TokenType.ElseIf, token, match.Range));

							tokens.Add(new TokenPair(TokenType.ElseIf,
								match.Range,
								ExpressionParser.ParseExpression(token, context, match.Range.RangeStart).Expression,
								tokenOptions));
						}
						else
						{
							context.Errors.Add(new MorestachioSyntaxError(match.Range,
								"else", "{{#ELSEIF}}",
								"Expected the elseif keyword to be a direct descended of an #if"));
						}
					}
					else if (StartsWith(trimmedToken, "#var "))
					{
						tokens.Add(ExpressionParser.TokenizeVariableAssignment(trimmedToken,
							context, TokenType.VariableVar, tokenOptions, match));
					}
					else if (StartsWith(trimmedToken, "#let "))
					{
						if (scopestack.Count == 0)
						{
							parserOptions.Logger?.LogWarn(LoggingFormatter.TokenizerEventId,
								"Using an #let on the topmost of your template has the same effect as using an #var",
								new Dictionary<string, object>
								{
									{ "Location", match.Range }
								});
						}

						tokens.Add(ExpressionParser.TokenizeVariableAssignment(trimmedToken,
							context, TokenType.VariableLet, tokenOptions, match));
					}
					else if (StartsWith(trimmedToken, "#scope "))
					{
						var token = TrimToken(trimmedToken, "scope ");
						TryParseStringOption(ref token, context, GetAsKeyword(), out var alias, in match);
						scopestack.Push(new ScopeStackItem(TokenType.ElementOpen, alias.Text ?? token, match.Range));

						if (token.Trim() != "")
						{
							token = token.Trim();

							tokens.Add(new TokenPair(TokenType.ElementOpen,
								token, match.Range,
								ExpressionParser.ParseExpression(token, context, match.Range.RangeStart).Expression,
								tokenOptions));
						}
						else
						{
							context.Errors.Add(new InvalidPathSyntaxError(match.Range, ""));
						}

						if (!string.IsNullOrWhiteSpace(alias.Text))
						{
							ValidateAliasName(alias.Text, "AS", context, match);
							//context.AdvanceLocation("scope ".Length + alias.Length);

							tokens.Add(new TokenPair(TokenType.Alias, alias.Text, match.Range));
						}
					}
					else if (StartsWith(trimmedToken, "#ISOLATE "))
					{
						var token = TrimToken(trimmedToken, "ISOLATE ");
						IsolationOptions scope = 0;

						if (TryParseFlagOption(ref token, "#VARIABLES"))
						{
							scope |= IsolationOptions.VariableIsolation;
						}

						if (TryParseExpressionOption(ref token, "#SCOPE ",
								match.Range.RangeStart,
								context,
								out var scopeExpression))
						{
							tokenOptions.Add(new TokenOption("IsolationScopeArg", scopeExpression));
							scope |= IsolationOptions.ScopeIsolation;
						}

						tokenOptions.Add(new TokenOption("IsolationType", scope));

						tokens.Add(new TokenPair(TokenType.IsolationScopeOpen, token, match.Range,
							tokenOptions));
						scopestack.Push(new ScopeStackItem(TokenType.IsolationScopeOpen, token, match.Range));
					}
					else if (ProcessHashTokenWithoutArgument(parserOptions, context, trimmedToken, tokenValue,
								scopestack,
								match, tokens, tokenOptions))
					{
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
									context.Errors.Add(new MorestachioUnopendScopeError(match.Range, "/",
										"{{#SET OPTION Name = Value}}",
										$" Expected to find '=' or whitespace after name but found '{c}'"));
								}
								else
								{
									name = token.Substring(0, i - 1).Trim();

									value = ExpressionParser.ParseExpression(token.Substring(i + 1).Trim(), context,
											match.Range.RangeStart)
										.Expression;

									break;
								}
							}
						}

						if (string.IsNullOrWhiteSpace(name))
						{
							context.Errors.Add(new MorestachioUnopendScopeError(match.Range, "/",
								"{{#SET OPTION Name = Value}}",
								" Expected to find '=' after name"));

							break;
						}

						if (value == null)
						{
							context.Errors.Add(new MorestachioUnopendScopeError(match.Range, "/",
								"{{#SET OPTION Name = Value}}",
								" Expected to find an expression after '='"));

							break;
						}

						await context.SetOption(name, value, parserOptions).ConfigureAwait(false);
					}
					else
					{
						FallbackProcessToken(trimmedToken, tokenValue, parserOptions, context, scopestack,
							tokenOptions, tokens, match);
					}
				}
				else if (trimmedToken.StartsWith('/'))
				{
					ProcessClosingScope(parserOptions, context, trimmedToken, tokenValue, scopestack,
						tokens, tokenOptions, match);
				}
				else if (trimmedToken.StartsWith('^'))
				{
					ProcessInvertedScope(parserOptions, context, trimmedToken, tokenValue, scopestack,
						match, tokens, tokenOptions);
				}
				else if (trimmedToken.IsEquals('!'))
				{
					tokens.Add(new TokenPair(TokenType.BlockComment, trimmedToken, match.Range,
						tokenOptions));
				}
				else if (trimmedToken.StartsWith('!'))
				{
					tokens.Add(
						new TokenPair(TokenType.Comment, trimmedToken, match.Range, tokenOptions));
				}
				else if (trimmedToken.StartsWith('&'))
				{
					//escaped single element
					var token = trimmedToken.TrimStart('&').Trim();

					tokens.Add(new TokenPair(TokenType.UnescapedSingleValue,
						token,
						match.Range,
						ExpressionParser.ParseExpression(token, context, match.Range.RangeStart).Expression,
						tokenOptions));
				}
				else
				{
					FallbackProcessToken(trimmedToken, tokenValue, parserOptions, context, scopestack,
						tokenOptions, tokens, match);
				}

				tokenOptions.Clear();

				//move forward in the string.
				//if (context.Character > match.Range + match.Length)
				//{
				//	throw new InvalidOperationException("Internal index location error");
				//}

				//context.SetLocation(match.Range + match.Length);
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
								location = k.Index
							};
						})
						.Reverse())
			{
				context.Errors.Add(new MorestachioUnclosedScopeError(unclosedScope.location, unclosedScope.scope, ""));
			}
		}

		return new TokenizerResult(tokens);
	}

	private static bool ProcessHashTokenWithoutArgument(ParserOptions parserOptions,
														TokenzierContext context,
														in string trimmedToken,
														in string tokenValue,
														Stack<ScopeStackItem> scopeStack,
														in TokenMatch match,
														ICollection<TokenPair> tokens,
														ICollection<ITokenOption> tokenOptions)
	{
		var upperCaseToken = trimmedToken.ToUpperInvariant();

		switch (upperCaseToken)
		{
			case "#IFELSE":
				parserOptions.Logger?.LogWarn(LoggingFormatter.TokenizerEventId,
					"IFELSE is considered obsolete and should no longer be used and has no effect. Use the #ELSE keyword instead",
					new Dictionary<string, object>
					{
						{ "Location", match.Range }
					});
				break;
			case "#ELSE":
				ScopeStackItem currentScope;

				//the new else block must be located inside an #IF or ^IF block 
				if (scopeStack.Count > 0 &&
					!(currentScope = scopeStack.Peek()).Equals(default) &&
					currentScope.TokenType is TokenType.If or TokenType.IfNot or TokenType.ElseIf)
				{
					scopeStack.Push(new ScopeStackItem(TokenType.Else, trimmedToken, match.Range));

					tokens.Add(new TokenPair(TokenType.Else, trimmedToken,
						match.Range, tokenOptions));
				}
				else
				{
					context.Errors.Add(new MorestachioSyntaxError(
						match.Range,
						"else", "{{#ELSE}}",
						"Expected the else keyword to be a direct descended of an #if or #elseif"));
				}

				break;
			case "#DEFAULT":
				var token = TrimToken(trimmedToken, "default");

				if (TryParseStringOption(ref token, context, GetAsKeyword(), out _, in match))
				{
					context.Errors.Add(new MorestachioSyntaxError(
						match.Range, "#default", "AS",
						"No Alias"));
				}

				scopeStack.Push(new ScopeStackItem(TokenType.SwitchDefaultOpen, token, match.Range));

				if (token.Trim() == "")
				{
					token = token.Trim();

					tokens.Add(new TokenPair(TokenType.SwitchDefaultOpen,
						token,
						match.Range, tokenOptions));
				}
				else
				{
					context.Errors.Add(new InvalidPathSyntaxError(match.Range, ""));
				}

				break;
			case "#NL":
				tokens.Add(new TokenPair(TokenType.WriteLineBreak, trimmedToken, match.Range,
					tokenOptions));
				break;
			case "#TNL":
				tokens.Add(new TokenPair(TokenType.TrimLineBreak, trimmedToken, match.Range,
					tokenOptions));
				break;
			case "#TNLS":
				tokenOptions.Add(new TokenOption("All", true));

				tokens.Add(new TokenPair(TokenType.TrimLineBreaks, trimmedToken, match.Range,
					tokenOptions));
				break;
			case "#TRIMALL":
				tokens.Add(new TokenPair(TokenType.TrimEverything, trimmedToken, match.Range,
					tokenOptions));
				break;
			case "#NOPRINT":
				tokens.Add(new TokenPair(TokenType.NoPrintOpen, trimmedToken, match.Range, tokenOptions));
				scopeStack.Push(new ScopeStackItem(TokenType.NoPrintOpen, trimmedToken, match.Range));
				break;
			default:
				return false;
		}

		return true;
	}

	private static void ProcessInvertedScope(
		ParserOptions parserOptions,
		TokenzierContext context,
		in string trimmedToken,
		in string tokenValue,
		Stack<ScopeStackItem> scopeStack,
		in TokenMatch match,
		ICollection<TokenPair> tokens,
		ICollection<ITokenOption> tokenOptions
	)
	{
		if (StartsWith(trimmedToken, "^if "))
		{
			var token = TrimToken(trimmedToken, "if", '^');

			if (TryParseStringOption(ref token, context, GetAsKeyword(), out _, in match))
			{
				context.Errors.Add(new MorestachioSyntaxError(
					match.Range, "^if", "AS",
					"No Alias"));
			}

			scopeStack.Push(new ScopeStackItem(TokenType.IfNot, token, match.Range));

			if (token.Trim() != "")
			{
				token = token.Trim();

				tokens.Add(new TokenPair(TokenType.IfNot,
					token,
					match.Range,
					ExpressionParser.ParseExpression(token, context, match.Range.RangeStart).Expression,
					tokenOptions));
			}
			else
			{
				context.Errors.Add(new InvalidPathSyntaxError(match.Range, ""));
			}
		}
		else if (StartsWith(trimmedToken, "^scope "))
		{
			//open inverted group
			var token = TrimToken(trimmedToken, "scope ", '^');
			TryParseStringOption(ref token, context, GetAsKeyword(), out var alias, in match);

			scopeStack.Push(new ScopeStackItem(TokenType.InvertedElementOpen, alias.Text ?? token,
				match.Range));

			tokens.Add(new TokenPair(TokenType.InvertedElementOpen,
				token,
				match.Range,
				ExpressionParser.ParseExpression(token, context, match.Range.RangeStart).Expression,
				tokenOptions));

			if (!string.IsNullOrWhiteSpace(alias.Text))
			{
				ValidateAliasName(alias.Text, "AS", context, match);
				//context.AdvanceLocation(1 + alias.Length);

				tokens.Add(new TokenPair(TokenType.Alias, alias.Text, match.Range));
			}
		}
		else
		{
			parserOptions.Logger?.LogWarn(LoggingFormatter.TokenizerEventId,
				"Use the {{^SCOPE path}} block instead of the {{^path}} block",
				new Dictionary<string, object>
				{
					{ "Location", match.Range }
				});
			//open inverted group
			var token = trimmedToken.TrimStart('^').Trim();
			TryParseStringOption(ref token, context, GetAsKeyword(), out var alias, in match);

			scopeStack.Push(new ScopeStackItem(TokenType.InvertedElementOpen, alias.Text ?? token,
				match.Range));
			tokenOptions.Add(new PersistantTokenOption("Render.LegacyStyle", true));

			tokens.Add(new TokenPair(TokenType.InvertedElementOpen,
				token,
				match.Range,
				ExpressionParser.ParseExpression(token, context, match.Range.RangeStart).Expression,
				tokenOptions));

			if (string.IsNullOrWhiteSpace(alias.Text))
			{
				return;
			}

			ValidateAliasName(alias.Text, "AS", context, match);
			//context.AdvanceLocation(1 + alias.Length);

			tokens.Add(new TokenPair(TokenType.Alias, alias.Text, match.Range));
		}
	}

	private static void ProcessClosingScope(
		ParserOptions parserOptions,
		TokenzierContext context,
		in string trimmedToken,
		in string tokenValue,
		Stack<ScopeStackItem> scopeStack,
		List<TokenPair> tokens,
		IList<ITokenOption> tokenOptions,
		TokenMatch match
	)
	{
		var upperToken = trimmedToken.ToUpperInvariant();

		switch (upperToken)
		{
			case "/DECLARE":
				CloseScope(tokenValue, TokenType.PartialDeclarationOpen, TokenType.PartialDeclarationClose,
					"DECLARE ...", scopeStack,
					tokens, context, tokenOptions, match);
				break;
			case "/EACH":
				CloseScope(tokenValue, TokenType.CollectionOpen, TokenType.CollectionClose, "EACH ...", scopeStack,
					tokens, context, tokenOptions, match);
				break;
			case "/FOREACH":
				CloseScope(tokenValue, TokenType.ForeachCollectionOpen, TokenType.ForeachCollectionClose, "FOREACH ...",
					scopeStack,
					tokens, context, tokenOptions, match);
				break;
			case "/WHILE":
				CloseScope(tokenValue, TokenType.WhileLoopOpen, TokenType.WhileLoopClose, "FOREACH ...", scopeStack,
					tokens, context, tokenOptions, match);
				break;
			case "/DO":
				CloseScope(tokenValue, TokenType.DoLoopOpen, TokenType.DoLoopClose, "WHILE ...", scopeStack,
					tokens, context, tokenOptions, match);
				break;
			case "/REPEAT":
				CloseScope(tokenValue, TokenType.RepeatLoopOpen, TokenType.RepeatLoopClose, "REPEAT ...", scopeStack,
					tokens, context, tokenOptions, match);
				break;
			case "/SWITCH":
				CloseScope(tokenValue, TokenType.SwitchOpen, TokenType.SwitchClose, "SWITCH ...", scopeStack,
					tokens, context, tokenOptions, match);
				break;
			case "/CASE":
				CloseScope(tokenValue, TokenType.SwitchCaseOpen, TokenType.SwitchCaseClose, "CASE ...", scopeStack,
					tokens, context, tokenOptions, match);
				break;
			case "/DEFAULT":
				CloseScope(tokenValue, TokenType.SwitchDefaultOpen, TokenType.SwitchDefaultClose, "DEFAULT", scopeStack,
					tokens, context, tokenOptions, match);
				break;
			case "/IF":
				CloseScope(tokenValue, TokenType.If | TokenType.IfNot, TokenType.IfClose, "IF ...", scopeStack,
					tokens, context, tokenOptions, match);
				break;
			case "/ELSEIF":
				CloseScope(tokenValue, TokenType.ElseIf, TokenType.ElseIfClose, "ELSEIF ...", scopeStack,
					tokens, context, tokenOptions, match);
				break;
			case "/ELSE":
				CloseScope(tokenValue, TokenType.Else, TokenType.ElseClose, "ELSE", scopeStack,
					tokens, context, tokenOptions, match);
				break;
			case "/SCOPE":
				CloseScope(tokenValue, TokenType.ElementOpen | TokenType.InvertedElementOpen, TokenType.ElementClose,
					"SCOPE ...", scopeStack,
					tokens, context, tokenOptions, match);
				break;
			case "/ISOLATE":
				CloseScope(tokenValue, TokenType.IsolationScopeOpen, TokenType.IsolationScopeClose, "ISOLATION ...",
					scopeStack,
					tokens, context, tokenOptions, match);
				break;
			case "/NOPRINT":
				CloseScope(tokenValue, TokenType.NoPrintOpen, TokenType.NoPrintClose, "NOPRINT", scopeStack,
					tokens, context, tokenOptions, match);
				break;
			default:
				FallbackProcessToken(trimmedToken, tokenValue, parserOptions, context, scopeStack,
					tokenOptions, tokens, match);
				break;
		}
	}

	private static bool TryParseStringOption(
		ref string token,
		TokenzierContext context,
		in string optionName,
		out TextRangeContent expression,
		in TokenMatch match
	)
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

			expression = new TextRangeContent(TextRange.RangeIndex(context, optionIndex, endOfNameIndex, match.Range),
				token.Substring(optionIndex, endOfNameIndex - optionIndex));
			token = token.Remove(optionIndex, endOfNameIndex - optionIndex);

			return true;
		}

		expression = default;

		return false;
	}

	private static bool TryParseExpressionOption(
		ref string token,
		in string optionName,
		TextIndex index,
		TokenzierContext context,
		out IMorestachioExpression expression
	)
	{
		var optionIndex = token.IndexOf(optionName, StringComparison.OrdinalIgnoreCase);

		if (optionIndex != -1)
		{
			token = token.Remove(optionIndex, optionName.Length);
			expression = ExpressionParser
				.ParseExpression(token, TokenzierContext.FromText(token), index.Add(context, optionIndex))
				.Expression;

			return true;
		}

		expression = null;

		return false;
	}

	private static bool TryParseFlagOption(ref string token, in string flagName)
	{
		var indexOf = token.IndexOf(flagName, StringComparison.OrdinalIgnoreCase);

		if (indexOf == -1)
		{
			return false;
		}

		token = token.Remove(indexOf, flagName.Length);

		return true;
	}

	private static IMorestachioExpression ExtractExpression(ref string token, TokenzierContext context, TextIndex index)
	{
		var expression = ExpressionParser.ParseExpression(token, context, index);
		token = token.Remove(0, expression.SourceBoundary.RangeEnd.Index);

		return expression.Expression;
	}

	private static string TrimToken(string token, in string keyword, in char key = '#')
	{
		token = token.TrimStart(key).TrimStart();

		return keyword != null ? token.Substring(keyword.Length) : token;
	}

	private static void ValidateAliasName(
		in string alias,
		in string tokenTypeName,
		TokenzierContext context,
		TokenMatch match
	)
	{
		if (string.IsNullOrWhiteSpace(alias))
		{
			context.Errors.Add(new MorestachioSyntaxError(
				match.Range, alias,
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
						match.Range, alias,
						tokenTypeName, tokenTypeName + " name",
						tokenTypeName + " option cannot not start with a digit."));
				}

				continue;
			}

			context.Errors.Add(new MorestachioSyntaxError(
				match.Range, alias, tokenTypeName,
				tokenTypeName + " name", tokenTypeName + " option can only consists of letters and numbers."));
		}
	}

	private static bool StartsWith(string token, string keyword)
	{
		return token.StartsWith(keyword, StringComparison.OrdinalIgnoreCase);
	}

	private static void FallbackProcessToken(
		in string tokenName,
		in string tokenValue1,
		ParserOptions parserOptions,
		TokenzierContext context,
		Stack<ScopeStackItem> scopeStack,
		IList<ITokenOption> tokenOptions,
		List<TokenPair> tokens,
		TokenMatch match
	)
	{
		//check for custom DocumentItem provider

		var customDocumentProvider =
			parserOptions.CustomDocumentItemProviders.FindTokenProvider(tokenName);

		if (customDocumentProvider != null)
		{
			var tokenPairs = customDocumentProvider
				.Tokenize(
					new CustomDocumentItemProvider.TokenInfo(tokenName, context, scopeStack, tokenOptions, match.Range),
					parserOptions);
			tokens.AddRange(tokenPairs);
		}
		else if (tokenName.StartsWith('#'))
		{
			UnmatchedTagBehavior(parserOptions, tokenName, context, tokens, tokenValue1,
				match);
		}
		else if (tokenName.StartsWith('^'))
		{
			UnmatchedTagBehavior(parserOptions, tokenName, context, tokens, tokenValue1,
				match);
		}
		else if (tokenName.StartsWith('/'))
		{
			UnmatchedTagBehavior(parserOptions, tokenName, context, tokens, tokenValue1,
				match);
		}
		else
		{
			//unsingle value.
			var token = tokenName.Trim();

			tokens.Add(new TokenPair(TokenType.EscapedSingleValue,
				token, match.Range,
				ExpressionParser.ParseExpression(token, context, match.Range.RangeStart).Expression,
				tokenOptions));
		}
	}

	private static void UnmatchedTagBehavior(
		ParserOptions parserOptions,
		in string tokenName,
		TokenzierContext context,
		ICollection<TokenPair> tokens,
		in string tokenValue1,
		TokenMatch match
	)
	{
		if (parserOptions.UnmatchedTagBehavior.HasFlagFast(Context.Options.UnmatchedTagBehavior
				.LogWarning))
		{
			parserOptions.Logger?.LogWarn(LoggingFormatter.TokenizerEventId,
				$"Unknown Tag '{tokenName}'.",
				new Dictionary<string, object>
				{
					{ "Location", match.Range }
				});
		}

		if (parserOptions.UnmatchedTagBehavior.HasFlagFast(Context.Options.UnmatchedTagBehavior
				.Output))
		{
			tokens.Add(new TokenPair(TokenType.Content, "{{" + tokenName + "}}",
				match.Range));
		}

		if (parserOptions.UnmatchedTagBehavior.HasFlagFast(Context.Options.UnmatchedTagBehavior
				.ThrowError))
		{
			context.Errors.Add(new MorestachioUnopendScopeError(match.Range,
				"{{" + tokenName + "}}", tokenName,
				"Unexpected token " + tokenName));
		}
	}

	private static void CloseScope(
		in string tokenValue,
		in TokenType openToken,
		in TokenType closeToken,
		in string expectedToken,
		Stack<ScopeStackItem> scopestack,
		ICollection<TokenPair> tokens,
		TokenzierContext context,
		IEnumerable<ITokenOption> tokenOptions,
		TokenMatch match
	)
	{
		if (scopestack.Any() &&
			openToken.HasFlagFast(scopestack.Peek().TokenType))
		{
			var token = scopestack.Pop().Value;

			tokens.Add(new TokenPair(closeToken, token, match.Range, tokenOptions));
		}
		else
		{
			context.Errors.Add(new MorestachioUnopendScopeError(match.Range, "/",
				"{{" + expectedToken + "}}",
				" There are more closing elements then open."));
		}
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
		public ScopeStackItem(TokenType tokenType, string value, TextRange index)
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
		public TextRange Index { get; }
	}
}