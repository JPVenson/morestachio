

using System.Runtime.CompilerServices;
using Morestachio.Formatter;

#region

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Morestachio.Framework;

#endregion

using FormattingScope = System.Tuple<Morestachio.IDocumentItem, bool>;

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
			var tokens = new Queue<TokenPair>(Tokenizer.Tokenize(parsingOptions, errors));
			//if there are any errors do not parse the template
			var documentInfo = new MorestachioDocumentInfo(parsingOptions,
				errors.Any() ? null : Parse(tokens, parsingOptions), errors);
			return documentInfo;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static ICollection<IValueDocumentItem> ParseAsPath(Queue<TokenPair> tokens)
		{
			var buildStack = new List<IValueDocumentItem>();

			while (tokens.Any())
			{
				var currentToken = tokens.Dequeue();

				if (currentToken.Type == TokenType.Content)
				{
					buildStack.Add(new ContentDocumentItem(currentToken.Value)
						{ExpressionStart = currentToken.TokenLocation});
				}
				else if (currentToken.Type == TokenType.PrintFormatted)
				{
					buildStack.Add(new PrintFormattedItem());
				}
				else if (currentToken.Type == TokenType.Format)
				{
					buildStack.Add(new CallFormatterDocumentItem(ParseArgumentHeader(currentToken), currentToken.Value));
				}
				else if (currentToken.Type == TokenType.EscapedSingleValue ||
				         currentToken.Type == TokenType.UnescapedSingleValue)
				{
					buildStack.Add(new PathDocumentItem(currentToken.Value,
							currentToken.Type == TokenType.EscapedSingleValue)
						{ExpressionStart = currentToken.TokenLocation});
				}
				else
				{
					throw new InvalidPathSyntaxError(currentToken.TokenLocation, currentToken.Value).GetException();
				}
			}

			return buildStack;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static List<Tuple<string, ICollection<IValueDocumentItem>>> ParseArgumentHeader(TokenPair currentToken)
		{
			var argumentMap = new List<Tuple<string, ICollection<IValueDocumentItem>>>();
			foreach (var formatterPart in currentToken.FormatString ?? new FormatterToken[0])
			{
				var tokenExpression = formatterPart.Argument.ParsedArguments.GetValue();

				argumentMap.Add(new Tuple<string, ICollection<IValueDocumentItem>>(formatterPart.Name,
					ParseAsPath(new Queue<TokenPair>(tokenExpression))));
			}

			return argumentMap;
		}

		/// <summary>
		///     Parses the Tokens into a Document.
		/// </summary>
		/// <param name="tokens">The tokens.</param>
		/// <param name="options">The options.</param>
		/// <returns></returns>
		internal static IDocumentItem Parse(Queue<TokenPair> tokens, ParserOptions options)
		{
			var buildStack = new Stack<FormattingScope>();
			//instead of recursive calling the parse function we stack the current document 
			buildStack.Push(new FormattingScope(new MorestachioDocument(), false));

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
					currentDocumentItem.Item1.Add(new ContentDocumentItem(currentToken.Value)
						{ExpressionStart = currentToken.TokenLocation});
				}
				else if (currentToken.Type == TokenType.CollectionOpen)
				{
					var nestedDocument = new CollectionDocumentItem(currentToken.Value)
						{ExpressionStart = currentToken.TokenLocation};
					buildStack.Push(new FormattingScope(nestedDocument, false));
					currentDocumentItem.Item1.Add(nestedDocument);
				}
				else if (currentToken.Type == TokenType.ElementOpen)
				{
					var nestedDocument = new ExpressionScopeDocumentItem(currentToken.Value)
						{ExpressionStart = currentToken.TokenLocation};
					buildStack.Push(new FormattingScope(nestedDocument, false));
					currentDocumentItem.Item1.Add(nestedDocument);
				}
				else if (currentToken.Type == TokenType.InvertedElementOpen)
				{
					var invertedScope = new InvertedExpressionScopeDocumentItem(currentToken.Value)
						{ExpressionStart = currentToken.TokenLocation};
					buildStack.Push(new FormattingScope(invertedScope, false));
					currentDocumentItem.Item1.Add(invertedScope);
				}
				else if (currentToken.Type == TokenType.CollectionClose || currentToken.Type == TokenType.ElementClose)
				{
					// remove the last document from the stack and go back to the parents
					buildStack.Pop();
					if (buildStack.Peek().Item2
					) //is the remaining scope a formatting one. If it is pop it and return to its parent
					{
						buildStack.Pop();
					}
				}
				else if (currentToken.Type == TokenType.PrintFormatted)
				{
					currentDocumentItem.Item1.Add(new PrintFormattedItem());
					buildStack.Pop(); //Print formatted can only be followed by a Format and if not the parser should have not emited it
				}
				else if (currentToken.Type == TokenType.Format)
				{
					if (buildStack.Peek().Item2)
					{
						buildStack.Pop();
					}

					var formatterItem = new IsolatedContextDocumentItem {ExpressionStart = currentToken.TokenLocation};
					buildStack.Push(new FormattingScope(formatterItem, true));
					formatterItem.Add(new CallFormatterDocumentItem(ParseArgumentHeader(currentToken), currentToken.Value));

					currentDocumentItem.Item1.Add(formatterItem);
				}
				else if (currentToken.Type == TokenType.EscapedSingleValue ||
				         currentToken.Type == TokenType.UnescapedSingleValue)
				{
					currentDocumentItem.Item1.Add(new PathDocumentItem(currentToken.Value,
							currentToken.Type == TokenType.EscapedSingleValue)
						{ExpressionStart = currentToken.TokenLocation});
				}
				else if (currentToken.Type == TokenType.PartialDeclarationOpen)
				{
					// currently same named partials will override each other
					// to allow recursive calls of partials we first have to declare the partial and then load it as we would parse
					// -the partial as a whole and then add it to the list would lead to unknown calls of partials inside the partial
					var nestedDocument = new MorestachioDocument();
					buildStack.Push(new FormattingScope(nestedDocument, false));
					currentDocumentItem.Item1.Add(new PartialDocumentItem(currentToken.Value, nestedDocument)
						{ExpressionStart = currentToken.TokenLocation});
				}
				else if (currentToken.Type == TokenType.PartialDeclarationClose)
				{
					currentDocumentItem.Item1.Add(new RenderPartialDoneDocumentItem(currentToken.Value)
						{ExpressionStart = currentToken.TokenLocation});
					buildStack.Pop();
				}
				else if (currentToken.Type == TokenType.RenderPartial)
				{
					currentDocumentItem.Item1.Add(new RenderPartialDocumentItem(currentToken.Value)
						{ExpressionStart = currentToken.TokenLocation});
				}
			}

			if (buildStack.Count != 1)
			{
				//var invalidScopedElements = buildStack
				//throw new MorestachioSyntaxError(new Tokenizer.CharacterLocation(){Character = }, );
				throw new InvalidOperationException(
					"There is an Error with the Parser. The Parser still contains unscoped builds: " +
					buildStack.Select(e => e.Item1.Kind).Aggregate((e, f) => e + ", " + f));
			}

			return buildStack.Pop().Item1;
		}
	}
}