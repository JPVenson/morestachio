using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items;
using Morestachio.Document.Items.SwitchCase;
using Morestachio.Document.TextOperations;
using Morestachio.Framework.Context.Options;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Tokenizing;
using Morestachio.Helper.Logging;
using Morestachio.Parsing;
using Morestachio.Parsing.ParserErrors;
using Morestachio.TemplateContainers;
#if ValueTask
using MorestachioDocumentInfoPromise = System.Threading.Tasks.ValueTask<Morestachio.MorestachioDocumentInfo>;
#else
using MorestachioDocumentInfoPromise = System.Threading.Tasks.Task<Morestachio.MorestachioDocumentInfo>;
#endif

namespace Morestachio
{
	/// <summary>
	///     The main entry point for this library. Use the static "Parse" methods to create template functions.
	///     Functions are safe for reuse, so you may parse and cache the resulting function.
	/// </summary>
	public static class Parser
	{
		/// <summary>
		///		Runs the Tokenizer and returns all errors in the template if present
		/// </summary>
		/// <param name="template"></param>
		/// <returns></returns>
		public static async Task<IEnumerable<IMorestachioError>> Validate(ITemplateContainer template)
		{
			var options = new ParserOptions(template);
			var tokenzierContext = new TokenzierContext(new List<int>(), null);
			await Tokenizer.Tokenize(options, tokenzierContext);
			return tokenzierContext.Errors;
		}

		/// <summary>
		///     Parses the Template with the given options
		/// </summary>
		/// <param name="parsingOptions">a set of options</param>
		/// <returns></returns>
		public static async MorestachioDocumentInfoPromise ParseWithOptionsAsync( ParserOptions parsingOptions)
		{
			if (parsingOptions == null)
			{
				throw new ArgumentNullException(nameof(parsingOptions));
			}

			parsingOptions.Seal();

			parsingOptions.Logger?.LogDebug(LoggingFormatter.ParserEventId, "Parse new Template");

			var tokenzierContext = new TokenzierContext(new List<int>(), parsingOptions.CultureInfo);
			var tokenizerResult = await Tokenizer.Tokenize(parsingOptions, tokenzierContext);

			parsingOptions.Logger?.LogDebug(LoggingFormatter.ParserEventId, "Template Parsed", new Dictionary<string, object>()
			{
				{"Errors", tokenzierContext.Errors}
			});
			//if there are any errors do not parse the template
			var documentInfo = new MorestachioDocumentInfo(parsingOptions,
				tokenzierContext.Errors.Any() ? null : Parse(tokenizerResult, parsingOptions), tokenzierContext.Errors);

			return documentInfo;
		}

		/// <summary>
		///     Parses the Template with the given options
		/// </summary>
		/// <param name="parsingOptions">a set of options</param>
		/// <returns></returns>
		public static MorestachioDocumentInfo ParseWithOptions( ParserOptions parsingOptions)
		{
			return ParseWithOptionsAsync(parsingOptions).Result;
		}

		/// <summary>
		///     Parses the Tokens into a Document.
		/// </summary>
		/// <param name="tokenizerResult">The result of an Tokenizer.Tokenize call.</param>
		/// <param name="options">The ParserOptions</param>
		/// <returns></returns>
		public static IDocumentItem Parse(TokenizerResult tokenizerResult, ParserOptions options)
		{
			var buildStack = new Stack<DocumentScope>();
			//this is the scope id that determines a scope that is using let or alias variables
			int variableScope = 1;
			var getScope = new Func<int>(() => variableScope++);
			//instead of recursive calling the parse function we stack the current document 
			buildStack.Push(new DocumentScope(new MorestachioDocument(), () => 0));

			DocumentScope GetVariabeScope()
			{
				return buildStack.FirstOrDefault(e => e.VariableScopeNumber != -1);
			}
			IEnumerable<ITokenOption> GetPublicOptions(TokenPair token)
			{
				var publicOptions = token.TokenOptions?.Where(e => e.Persistent).ToArray();
				return publicOptions?.Length > 0 ? publicOptions : null;
			}

			bool TryAdd(IDocumentItem document, IDocumentItem child)
			{
				if (document is IBlockDocumentItem block)
				{
					block.Add(child);
					return true;
				}

				return false;
			}

			foreach (var currentToken in tokenizerResult)
			{
				var currentDocumentItem = buildStack.Peek(); //get the latest document
				if (currentToken.Type.Equals(TokenType.Content))
				{
					var contentDocumentItem = new ContentDocumentItem(currentToken.TokenLocation, currentToken.Value, GetPublicOptions(currentToken));
					TryAdd(currentDocumentItem.Document, contentDocumentItem);

					if (tokenizerResult.Previous.HasValue)
					{
						if (tokenizerResult.Previous.Value.FindOption<bool>("Embedded.TrimTailing"))
						{
							TryAdd(contentDocumentItem, new TextEditDocumentItem(tokenizerResult.Previous.Value.TokenLocation, new TrimLineBreakTextOperation()
							{
								LineBreaks = 0,
								LineBreakTrimDirection = LineBreakTrimDirection.Begin
							}, EmbeddedInstructionOrigin.Previous, GetPublicOptions(currentToken)));
						}
						if (tokenizerResult.Previous.Value.FindOption<bool>("Embedded.TrimAllTailing"))
						{
							TryAdd(contentDocumentItem, new TextEditDocumentItem(tokenizerResult.Previous.Value.TokenLocation, new TrimLineBreakTextOperation()
							{
								LineBreaks = -1,
								LineBreakTrimDirection = LineBreakTrimDirection.Begin
							}, EmbeddedInstructionOrigin.Previous, GetPublicOptions(currentToken)));
						}
					}

					if (tokenizerResult.Next.HasValue)
					{
						if (tokenizerResult.Next.Value.FindOption<bool>("Embedded.TrimLeading"))
						{
							TryAdd(contentDocumentItem, new TextEditDocumentItem(tokenizerResult.Next.Value.TokenLocation, new TrimLineBreakTextOperation()
							{
								LineBreaks = 0,
								LineBreakTrimDirection = LineBreakTrimDirection.End
							}, EmbeddedInstructionOrigin.Next, GetPublicOptions(currentToken)));
						}
						if (tokenizerResult.Next.Value.FindOption<bool>("Embedded.TrimAllLeading"))
						{
							TryAdd(contentDocumentItem, new TextEditDocumentItem(tokenizerResult.Next.Value.TokenLocation, new TrimLineBreakTextOperation()
							{
								LineBreaks = -1,
								LineBreakTrimDirection = LineBreakTrimDirection.End
							}, EmbeddedInstructionOrigin.Next, GetPublicOptions(currentToken)));
						}
					}
				}
				else if (currentToken.Type.Equals(TokenType.If))
				{
					var nestedDocument = new IfExpressionScopeDocumentItem(currentToken.TokenLocation,
						currentToken.MorestachioExpression, GetPublicOptions(currentToken));
					buildStack.Push(new DocumentScope(nestedDocument, getScope));
					TryAdd(currentDocumentItem.Document, nestedDocument);
				}
				else if (currentToken.Type.Equals(TokenType.IfNot))
				{
					var nestedDocument = new IfNotExpressionScopeDocumentItem(currentToken.TokenLocation,
						currentToken.MorestachioExpression, GetPublicOptions(currentToken));
					buildStack.Push(new DocumentScope(nestedDocument, getScope));
					TryAdd(currentDocumentItem.Document, nestedDocument);
				}
				else if (currentToken.Type.Equals(TokenType.Else))
				{
					var nestedDocument = new ElseExpressionScopeDocumentItem(currentToken.TokenLocation, GetPublicOptions(currentToken));
					buildStack.Push(new DocumentScope(nestedDocument, getScope));
					TryAdd(currentDocumentItem.Document, nestedDocument);
				}
				else if (currentToken.Type.Equals(TokenType.CollectionOpen))
				{
					var nestedDocument = new EachDocumentItem(currentToken.TokenLocation, currentToken.MorestachioExpression, GetPublicOptions(currentToken));
					buildStack.Push(new DocumentScope(nestedDocument, getScope));
					TryAdd(currentDocumentItem.Document, nestedDocument);
				}
				else if (currentToken.Type.Equals(TokenType.SwitchOpen))
				{
					var nestedDocument = new SwitchDocumentItem(currentToken.TokenLocation,
						currentToken.MorestachioExpression,
						currentToken.FindOption<bool>("ScopeTo"), GetPublicOptions(currentToken));
					buildStack.Push(new DocumentScope(nestedDocument, getScope));
					TryAdd(currentDocumentItem.Document, nestedDocument);
				}
				else if (currentToken.Type.Equals(TokenType.SwitchCaseOpen))
				{
					var nestedDocument = new SwitchCaseDocumentItem(currentToken.TokenLocation, currentToken.MorestachioExpression, GetPublicOptions(currentToken));
					buildStack.Push(new DocumentScope(nestedDocument, getScope));
					TryAdd(currentDocumentItem.Document, nestedDocument);
				}
				else if (currentToken.Type.Equals(TokenType.SwitchDefaultOpen))
				{
					var nestedDocument = new SwitchDefaultDocumentItem(currentToken.TokenLocation, GetPublicOptions(currentToken));
					buildStack.Push(new DocumentScope(nestedDocument, getScope));
					TryAdd(currentDocumentItem.Document, nestedDocument);
				}
				else if (currentToken.Type.Equals(TokenType.WhileLoopOpen))
				{
					var nestedDocument = new WhileLoopDocumentItem(currentToken.TokenLocation, currentToken.MorestachioExpression, GetPublicOptions(currentToken));
					buildStack.Push(new DocumentScope(nestedDocument, getScope));
					TryAdd(currentDocumentItem.Document, nestedDocument);
				}
				else if (currentToken.Type.Equals(TokenType.DoLoopOpen))
				{
					var nestedDocument = new DoLoopDocumentItem(currentToken.TokenLocation, currentToken.MorestachioExpression, GetPublicOptions(currentToken));
					buildStack.Push(new DocumentScope(nestedDocument, getScope));
					TryAdd(currentDocumentItem.Document, nestedDocument);
				}
				else if (currentToken.Type.Equals(TokenType.ElementOpen))
				{
					var nestedDocument = new ExpressionScopeDocumentItem(currentToken.TokenLocation, currentToken.MorestachioExpression, GetPublicOptions(currentToken));
					buildStack.Push(new DocumentScope(nestedDocument, getScope));
					TryAdd(currentDocumentItem.Document, nestedDocument);
				}
				else if (currentToken.Type.Equals(TokenType.RepeatLoopOpen))
				{
					var nestedDocument = new RepeatDocumentItem(currentToken.TokenLocation, currentToken.MorestachioExpression, GetPublicOptions(currentToken));
					buildStack.Push(new DocumentScope(nestedDocument, getScope));
					TryAdd(currentDocumentItem.Document, nestedDocument);
				}
				else if (currentToken.Type.Equals(TokenType.InvertedElementOpen))
				{
					var nestedDocument = new InvertedExpressionScopeDocumentItem(currentToken.TokenLocation,
						currentToken.MorestachioExpression, GetPublicOptions(currentToken));
					buildStack.Push(new DocumentScope(nestedDocument, getScope));
					TryAdd(currentDocumentItem.Document, nestedDocument);
				}
				else if (currentToken.Type.Equals(TokenType.CollectionClose)
						|| currentToken.Type.Equals(TokenType.ElementClose)
						|| currentToken.Type.Equals(TokenType.IfClose)
						|| currentToken.Type.Equals(TokenType.ElseClose)
						|| currentToken.Type.Equals(TokenType.WhileLoopClose)
						|| currentToken.Type.Equals(TokenType.DoLoopClose)
						|| currentToken.Type.Equals(TokenType.RepeatLoopClose)
						|| currentToken.Type.Equals(TokenType.SwitchCaseClose)
						|| currentToken.Type.Equals(TokenType.SwitchDefaultClose)
						|| currentToken.Type.Equals(TokenType.SwitchClose))
				{
					DocumentScope scope = buildStack.Peek();
					if (!(scope.Document is IBlockDocumentItem blockDocument))
					{
						throw new InvalidOperationException($"Closing an token '{currentToken.Type}' at '{currentToken.TokenLocation}'" +
						                                    $" that is not of type '{typeof(IBlockDocumentItem)}' is not possible.");
					}

					blockDocument.BlockClosingOptions = GetPublicOptions(currentToken);

					if (scope.HasAlias) //are we in a alias then remove it
					{
						foreach (var scopeLocalVariable in scope.LocalVariables)
						{
							TryAdd(currentDocumentItem.Document, new RemoveAliasDocumentItem(currentToken.TokenLocation,
								scopeLocalVariable,
								scope.VariableScopeNumber, null));
						}
					}
					// remove the last document from the stack and go back to the parents
					buildStack.Pop();
				}
				else if (currentToken.Type.Equals(TokenType.EscapedSingleValue) ||
						currentToken.Type.Equals(TokenType.UnescapedSingleValue))
				{
					var nestedDocument = new PathDocumentItem(currentToken.TokenLocation,
						currentToken.MorestachioExpression,
						currentToken.Type.Equals(TokenType.EscapedSingleValue), GetPublicOptions(currentToken));
					TryAdd(currentDocumentItem.Document, nestedDocument);
				}
				else if (currentToken.Type.Equals(TokenType.PartialDeclarationOpen))
				{
					// currently same named partials will override each other
					// to allow recursive calls of partials we first have to declare the partial and then load it as we would parse
					// -the partial as a whole and then add it to the list would lead to unknown calls of partials inside the partial
					var nestedDocument = new MorestachioDocument();
					buildStack.Push(new DocumentScope(nestedDocument, getScope));
					TryAdd(currentDocumentItem.Document, new PartialDocumentItem(currentToken.TokenLocation,
						currentToken.Value,
						nestedDocument, GetPublicOptions(currentToken)));
				}
				else if (currentToken.Type.Equals(TokenType.PartialDeclarationClose))
				{
					buildStack.Pop();
				}
				else if (currentToken.Type.Equals(TokenType.RenderPartial))
				{
					TryAdd(currentDocumentItem.Document, new RenderPartialDocumentItem(currentToken.TokenLocation,
						currentToken.Value,
						currentToken.MorestachioExpression, GetPublicOptions(currentToken)));
				}
				else if (currentToken.Type.Equals(TokenType.ImportPartial))
				{
					TryAdd(currentDocumentItem.Document, new ImportPartialDocumentItem(currentToken.TokenLocation,
						currentToken.MorestachioExpression,
						currentToken.FindOption<IMorestachioExpression>("Context"), GetPublicOptions(currentToken)));
				}
				else if (currentToken.Type.Equals(TokenType.Alias))
				{
					var scope = GetVariabeScope();
					var nestedDocument = new AliasDocumentItem(currentToken.TokenLocation,
						currentToken.Value,
						scope.VariableScopeNumber, GetPublicOptions(currentToken));
					TryAdd(currentDocumentItem.Document, nestedDocument);
					currentDocumentItem.LocalVariables.Add(currentToken.Value);
				}
				else if (currentToken.Type.Equals(TokenType.VariableVar))
				{
					var nestedDocument = new EvaluateVariableDocumentItem(currentToken.TokenLocation,
						currentToken.Value,
						currentToken.MorestachioExpression, GetPublicOptions(currentToken));
					TryAdd(currentDocumentItem.Document, nestedDocument);
				}
				else if (currentToken.Type.Equals(TokenType.WriteLineBreak))
				{
					TryAdd(currentDocumentItem.Document, new TextEditDocumentItem(currentToken.TokenLocation,
						new AppendLineBreakTextOperation(),
						currentToken.IsEmbeddedToken, GetPublicOptions(currentToken)));
				}
				else if (currentToken.Type.Equals(TokenType.TrimLineBreak))
				{
					TryAdd(currentDocumentItem.Document, new TextEditDocumentItem(currentToken.TokenLocation,
						new TrimLineBreakTextOperation()
						{
							LineBreaks = 1,
							LineBreakTrimDirection = LineBreakTrimDirection.Begin
						}, currentToken.IsEmbeddedToken, GetPublicOptions(currentToken)));
				}
				else if (currentToken.Type.Equals(TokenType.TrimLineBreaks))
				{
					TryAdd(currentDocumentItem.Document, new TextEditDocumentItem(currentToken.TokenLocation,
						new TrimLineBreakTextOperation()
						{
							LineBreaks = currentToken.FindOption<bool>("All") ? -1 : 0,
							LineBreakTrimDirection = LineBreakTrimDirection.Begin
						}, currentToken.IsEmbeddedToken, GetPublicOptions(currentToken)));
				}
				else if (currentToken.Type.Equals(TokenType.TrimPrependedLineBreaks))
				{
					TryAdd(currentDocumentItem.Document, new TextEditDocumentItem(currentToken.TokenLocation,
						new TrimLineBreakTextOperation()
						{
							LineBreaks = currentToken.FindOption<bool>("All") ? -1 : 0,
							LineBreakTrimDirection = LineBreakTrimDirection.End
						}, currentToken.IsEmbeddedToken, GetPublicOptions(currentToken)));
				}
				else if (currentToken.Type.Equals(TokenType.TrimEverything))
				{
					TryAdd(currentDocumentItem.Document, new TextEditDocumentItem(currentToken.TokenLocation,
						new TrimAllWhitespacesTextOperation(),
						currentToken.IsEmbeddedToken, GetPublicOptions(currentToken)));
				}
				else if (currentToken.Type.Equals(TokenType.VariableLet))
				{
					var scope = 0;
					if (buildStack.Count > 1)
					{
						scope = GetVariabeScope()
							.VariableScopeNumber;
					}
					var nestedDocument = new EvaluateVariableDocumentItem(currentToken.TokenLocation, currentToken.Value,
						currentToken.MorestachioExpression, scope, GetPublicOptions(currentToken));

					TryAdd(currentDocumentItem.Document, nestedDocument);
					if (buildStack.Count > 1)
					{
						currentDocumentItem.LocalVariables.Add(currentToken.Value);
					}
				}
				else if (currentToken.Type.Equals(TokenType.Comment) || currentToken.Type.Equals(TokenType.BlockComment))
				{
					//just ignore this part and print nothing
				}
				else
				{
					var tokenOptions = GetPublicOptions(currentToken);
					var customDocumentItemProvider =
						options.CustomDocumentItemProviders.FirstOrDefault(e => e.ShouldParse(currentToken, options, tokenOptions));
					var nestedDocument = customDocumentItemProvider?.Parse(currentToken, options, buildStack, getScope, tokenOptions);
					if (nestedDocument != null)
					{
						TryAdd(currentDocumentItem.Document, nestedDocument);
					}
				}
			}

			if (buildStack.Count != 1)
			{
				//var invalidScopedElements = buildStack
				//throw new MorestachioSyntaxError(new Tokenizer.CharacterLocation(){Character = }, );
				throw new InvalidOperationException(
					"There is an Error with the Parser. The Parser still contains unscoped builds: " +
					buildStack.Select(e => e.Document.GetType().Name).Aggregate((e, f) => e + ", " + f));
			}

			return buildStack.Pop().Document;
		}
	}
}