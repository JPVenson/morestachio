using System.Runtime.CompilerServices;
using Morestachio.Document;
using Morestachio.Document.Contracts;
using Morestachio.Formatter;

#region

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Morestachio.Framework;

#endregion

namespace Morestachio
{
	/// <summary>
	///     The main entry point for this library. Use the static "Parse" methods to create template functions.
	///     Functions are safe for reuse, so you may parse and cache the resulting function.
	/// </summary>
	public static class Parser
	{
		/// <summary>
		///     Parses the Template with the given options
		/// </summary>
		/// <param name="parsingOptions">a set of options</param>
		/// <returns></returns>
		[ContractAnnotation("parsingOptions:null => halt")]
		[NotNull]
		[MustUseReturnValue("Use return value to create templates. Reuse return value if possible.")]
		[Pure]
		public static MorestachioDocumentInfo ParseWithOptions([NotNull] ParserOptions parsingOptions)
		{
			if (parsingOptions == null)
			{
				throw new ArgumentNullException(nameof(parsingOptions));
			}

			if (parsingOptions.SourceFactory == null)
			{
				throw new ArgumentNullException(nameof(parsingOptions), "The given Stream is null");
			}

			var errors = new List<IMorestachioError>();
			var profiler = new PerformanceProfiler(parsingOptions.ProfileExecution);
			Queue<TokenPair> tokens;
			using (profiler.Begin("Tokenize"))
			{
				tokens = new Queue<TokenPair>(Tokenizer.Tokenize(parsingOptions, errors, profiler));
			}

			//if there are any errors do not parse the template
			var documentInfo = new MorestachioDocumentInfo(parsingOptions,
				errors.Any() ? null : Parse(tokens), errors);
			documentInfo.Profiler = profiler;
			return documentInfo;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static IValueDocumentItem ParseAsPath(Tokenizer.HeaderTokenMatch token)
		{
			switch (token.TokenType)
			{
				case Tokenizer.HeaderArgumentType.String:
					return new ContentDocumentItem(token.Value)
					{
						ExpressionStart = token.TokenLocation
					};
				case Tokenizer.HeaderArgumentType.Expression:
					return new CallFormatterDocumentItem(token
						.Arguments
						.Select(e => new Tuple<Tokenizer.HeaderTokenMatch, IValueDocumentItem>(e, ParseAsPath(e)))
						.ToArray(), token.Value);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static Tuple<Tokenizer.HeaderTokenMatch, IValueDocumentItem>[] ParseArgumentHeader(TokenPair currentToken)
		{
			var argumentMap = new List<Tuple<Tokenizer.HeaderTokenMatch, IValueDocumentItem>>();
			foreach (var formatterPart in currentToken.FormatString ?? new Tokenizer.HeaderTokenMatch[0])
			{
				argumentMap.Add(new Tuple<Tokenizer.HeaderTokenMatch, IValueDocumentItem>(formatterPart,
					ParseAsPath(formatterPart)));
			}

			return argumentMap.ToArray();
		}

		/// <summary>
		///     Parses the Tokens into a Document.
		/// </summary>
		/// <param name="tokens">The tokens.</param>
		/// <returns></returns>
		internal static IDocumentItem Parse(Queue<TokenPair> tokens)
		{
			var buildStack = new Stack<DocumentScope>();
			//instead of recursive calling the parse function we stack the current document 
			buildStack.Push(new DocumentScope(new MorestachioDocument()));

			while (tokens.Any())
			{
				var currentToken = tokens.Dequeue();
				var currentDocumentItem = buildStack.Peek(); //get the latest document

				if (currentToken.Type == TokenType.Comment)
				{
					//just ignore this part and print nothing
				}
				else if (currentToken.Type == TokenType.Content)
				{
					currentDocumentItem.Document.Add(new ContentDocumentItem(currentToken.Value)
					{
						ExpressionStart = currentToken.TokenLocation
					});
				}
				else if (currentToken.Type == TokenType.If)
				{
					var nestedDocument = new IfExpressionScopeDocumentItem(currentToken.Value)
					{
						ExpressionStart = currentToken.TokenLocation
					};
					buildStack.Push(new DocumentScope(nestedDocument));
					currentDocumentItem.Document.Add(nestedDocument);
				}
				else if (currentToken.Type == TokenType.IfNot)
				{
					var nestedDocument = new IfNotExpressionScopeDocumentItem(currentToken.Value)
					{
						ExpressionStart = currentToken.TokenLocation
					};
					buildStack.Push(new DocumentScope(nestedDocument));
					currentDocumentItem.Document.Add(nestedDocument);
				}
				else if (currentToken.Type == TokenType.Else)
				{
					var nestedDocument = new ElseExpressionScopeDocumentItem()
					{
						ExpressionStart = currentToken.TokenLocation
					};
					buildStack.Push(new DocumentScope(nestedDocument));
					currentDocumentItem.Document.Add(nestedDocument);
				}
				else if (currentToken.Type == TokenType.CollectionOpen)
				{
					var nestedDocument = new CollectionDocumentItem(currentToken.Value)
					{
						ExpressionStart = currentToken.TokenLocation
					};
					buildStack.Push(new DocumentScope(nestedDocument));
					currentDocumentItem.Document.Add(nestedDocument);
				}
				else if (currentToken.Type == TokenType.ElementOpen)
				{
					var nestedDocument = new ExpressionScopeDocumentItem(currentToken.Value)
					{
						ExpressionStart = currentToken.TokenLocation
					};
					buildStack.Push(new DocumentScope(nestedDocument));
					currentDocumentItem.Document.Add(nestedDocument);
				}
				else if (currentToken.Type == TokenType.InvertedElementOpen)
				{
					var invertedScope = new InvertedExpressionScopeDocumentItem(currentToken.Value)
					{
						ExpressionStart = currentToken.TokenLocation
					};
					buildStack.Push(new DocumentScope(invertedScope));
					currentDocumentItem.Document.Add(invertedScope);
				}
				else if (currentToken.Type == TokenType.CollectionClose
						 || currentToken.Type == TokenType.ElementClose
						 || currentToken.Type == TokenType.IfClose
						 || currentToken.Type == TokenType.ElseClose)
				{
					if (buildStack.Peek().HasAlias) //are we in a alias then remove it
					{
						currentDocumentItem.Document.Add(new RemoveAliasDocumentItem(buildStack.Peek().AliasName));
						buildStack.Pop();
					}
					// remove the last document from the stack and go back to the parents
					buildStack.Pop();

					if (buildStack.Peek().IsFormattingScope
					) //is the remaining scope a formatting one. If it is pop it and return to its parent
					{
						buildStack.Pop();
					}
				}
				else if (currentToken.Type == TokenType.Print)
				{
					currentDocumentItem.Document.Add(new PrintContextValue());
					buildStack.Pop(); //Print formatted can only be followed by a Format and if not the parser should have not emited it
				}
				else if (currentToken.Type == TokenType.Format)
				{
					if (buildStack.Peek().IsFormattingScope)
					{
						buildStack.Pop();
					}

					var formatterItem = new IsolatedContextDocumentItem
					{
						ExpressionStart = currentToken.TokenLocation
					};
					buildStack.Push(new DocumentScope(formatterItem, true));
					formatterItem.Add(new CallFormatterDocumentItem(ParseArgumentHeader(currentToken), currentToken.Value));

					currentDocumentItem.Document.Add(formatterItem);
				}
				else if (currentToken.Type == TokenType.EscapedSingleValue ||
						 currentToken.Type == TokenType.UnescapedSingleValue)
				{
					currentDocumentItem.Document.Add(new PathDocumentItem(currentToken.Value,
							currentToken.Type == TokenType.EscapedSingleValue)
					{
						ExpressionStart = currentToken.TokenLocation
					});
				}
				else if (currentToken.Type == TokenType.PartialDeclarationOpen)
				{
					// currently same named partials will override each other
					// to allow recursive calls of partials we first have to declare the partial and then load it as we would parse
					// -the partial as a whole and then add it to the list would lead to unknown calls of partials inside the partial
					var nestedDocument = new MorestachioDocument();
					buildStack.Push(new DocumentScope(nestedDocument));
					currentDocumentItem.Document.Add(new PartialDocumentItem(currentToken.Value, nestedDocument)
					{
						ExpressionStart = currentToken.TokenLocation
					});
				}
				else if (currentToken.Type == TokenType.PartialDeclarationClose)
				{
					currentDocumentItem.Document.Add(new RenderPartialDoneDocumentItem(currentToken.Value)
					{
						ExpressionStart = currentToken.TokenLocation
					});
					buildStack.Pop();
				}
				else if (currentToken.Type == TokenType.RenderPartial)
				{
					currentDocumentItem.Document.Add(new RenderPartialDocumentItem(currentToken.Value)
					{
						ExpressionStart = currentToken.TokenLocation
					});
				}
				else if (currentToken.Type == TokenType.Alias)
				{
					var aliasDocumentItem = new AliasDocumentItem(currentToken.Value)
					{
						ExpressionStart = currentToken.TokenLocation
					};
					currentDocumentItem.Document.Add(aliasDocumentItem);
					buildStack.Push(new DocumentScope(aliasDocumentItem, currentToken.Value));
				}
			}

			if (buildStack.Count != 1)
			{
				//var invalidScopedElements = buildStack
				//throw new MorestachioSyntaxError(new Tokenizer.CharacterLocation(){Character = }, );
				throw new InvalidOperationException(
					"There is an Error with the Parser. The Parser still contains unscoped builds: " +
					buildStack.Select(e => e.Document.Kind).Aggregate((e, f) => e + ", " + f));
			}

			return buildStack.Pop().Document;
		}
	}
}