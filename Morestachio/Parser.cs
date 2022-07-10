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
			var options = ParserOptionsBuilder.New().WithTemplate(template).Build();
			var tokenzierContext = new TokenzierContext(new List<int>(), null);
			await Tokenizer.Tokenize(options, tokenzierContext).ConfigureAwait(false);
			return tokenzierContext.Errors;
		}

		/// <summary>
		///     Parses the Template with the given options
		/// </summary>
		/// <param name="parsingOptions">a set of options</param>
		/// <returns></returns>
		public static async MorestachioDocumentInfoPromise ParseWithOptionsAsync(ParserOptions parsingOptions)
		{
			if (parsingOptions == null)
			{
				throw new ArgumentNullException(nameof(parsingOptions));
			}

			parsingOptions.Logger?.LogDebug(LoggingFormatter.ParserEventId, "Parse new Template");

			var tokenzierContext = new TokenzierContext(new List<int>(), parsingOptions.CultureInfo);
			var tokenizerResult = await Tokenizer.Tokenize(parsingOptions, tokenzierContext).ConfigureAwait(false);

			parsingOptions.Logger?.LogError(LoggingFormatter.ParserEventId, $"Template Parsed. {string.Join("\r\n", tokenzierContext.Errors.Select(f => f.AsFormatted()))}");
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
		public static MorestachioDocumentInfo ParseWithOptions(ParserOptions parsingOptions)
		{
			return ParseWithOptionsAsync(parsingOptions).Await();
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
			var textEdits = new List<TextEditDocumentItem>();

			foreach (var currentToken in tokenizerResult)
			{
				var currentDocumentItem = buildStack.Peek(); //get the latest document

				if (currentToken.Type is not TokenType knownType)
				{
					//when type is not a TokenType is will always be an custom token
					var tokenOptions = GetPublicOptions(currentToken);

					var customDocumentItemProvider =
						options.CustomDocumentItemProviders.FindTokenProvider(currentToken, options, tokenOptions);
					var nestedDocument = customDocumentItemProvider?.Parse(currentToken, options, buildStack, getScope, tokenOptions);

					if (nestedDocument != null)
					{
						AddOrThrow(currentDocumentItem.Document, nestedDocument);
					}
				}
				else
				{
					switch (knownType)
					{
						case TokenType.Content:
						{
							var contentDocumentItem = new ContentDocumentItem(currentToken.TokenRange, currentToken.Value, GetPublicOptions(currentToken));
							AddOrThrow(currentDocumentItem.Document, contentDocumentItem);

							if (tokenizerResult.Previous.HasValue)
							{
								if (tokenizerResult.Previous.Value.FindOption<bool>("Embedded.TrimTailing"))
								{
									AddOrThrow(contentDocumentItem, new TextEditDocumentItem(tokenizerResult.Previous.Value.TokenRange, new TrimLineBreakTextOperation()
									{
										LineBreaks = 0,
										LineBreakTrimDirection = LineBreakTrimDirection.Begin
									}, EmbeddedInstructionOrigin.Previous, GetPublicOptions(currentToken)));
								}

								if (tokenizerResult.Previous.Value.FindOption<bool>("Embedded.TrimAllTailing"))
								{
									AddOrThrow(contentDocumentItem, new TextEditDocumentItem(tokenizerResult.Previous.Value.TokenRange, new TrimLineBreakTextOperation()
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
									AddOrThrow(contentDocumentItem, new TextEditDocumentItem(tokenizerResult.Next.Value.TokenRange, new TrimLineBreakTextOperation()
									{
										LineBreaks = 0,
										LineBreakTrimDirection = LineBreakTrimDirection.End
									}, EmbeddedInstructionOrigin.Next, GetPublicOptions(currentToken)));
								}

								if (tokenizerResult.Next.Value.FindOption<bool>("Embedded.TrimAllLeading"))
								{
									AddOrThrow(contentDocumentItem, new TextEditDocumentItem(tokenizerResult.Next.Value.TokenRange, new TrimLineBreakTextOperation()
									{
										LineBreaks = -1,
										LineBreakTrimDirection = LineBreakTrimDirection.End
									}, EmbeddedInstructionOrigin.Next, GetPublicOptions(currentToken)));
								}
							}

							foreach (var textEditDocumentItem in textEdits)
							{
								AddOrThrow(contentDocumentItem, textEditDocumentItem);
							}

							textEdits.Clear();

							break;
						}
						case TokenType.If:
						{
							var nestedDocument = new IfExpressionScopeDocumentItem(currentToken.TokenRange,
								currentToken.MorestachioExpression, GetPublicOptions(currentToken), false);
							buildStack.Push(new DocumentScope(nestedDocument, getScope));
							AddOrThrow(currentDocumentItem.Document, nestedDocument);

							break;
						}
						case TokenType.IfNot:
						{
							var nestedDocument = new IfExpressionScopeDocumentItem(currentToken.TokenRange,
								currentToken.MorestachioExpression, GetPublicOptions(currentToken), true);
							buildStack.Push(new DocumentScope(nestedDocument, getScope));
							AddOrThrow(currentDocumentItem.Document, nestedDocument);

							break;
						}
						case TokenType.Else:
						{
							var nestedDocument = new ElseExpressionScopeDocumentItem(currentToken.TokenRange, GetPublicOptions(currentToken));
							buildStack.Push(new DocumentScope(nestedDocument, getScope));

							if (currentDocumentItem.Document is IfExpressionScopeDocumentItem ifDocument)
							{
								ifDocument.Add(nestedDocument);
							}

							break;
						}
						case TokenType.ElseIf:
						{
							var nestedDocument = new ElseIfExpressionScopeDocumentItem(currentToken.TokenRange,
								currentToken.MorestachioExpression,
								GetPublicOptions(currentToken));
							var documentScope = new DocumentScope(nestedDocument, getScope);
							buildStack.Push(documentScope);

							if (currentDocumentItem.Document is IfExpressionScopeDocumentItem ifDocument)
							{
								ifDocument.Add(nestedDocument);
							}

							//AddIfDocument(currentToken, documentScope);
							break;
						}
						case TokenType.CollectionOpen:
						{
							var nestedDocument = new EachDocumentItem(currentToken.TokenRange, currentToken.MorestachioExpression, GetPublicOptions(currentToken));
							buildStack.Push(new DocumentScope(nestedDocument, getScope));
							AddOrThrow(currentDocumentItem.Document, nestedDocument);

							break;
						}
						case TokenType.ForeachCollectionOpen:
						{
							var nestedDocument = new ForEachDocumentItem(currentToken.TokenRange,
								currentToken.MorestachioExpression,
								currentToken.FindOption<string>("Alias"),
								GetPublicOptions(currentToken));
							buildStack.Push(new DocumentScope(nestedDocument, getScope));
							AddOrThrow(currentDocumentItem.Document, nestedDocument);

							break;
						}
						case TokenType.SwitchOpen:
						{
							var nestedDocument = new SwitchDocumentItem(currentToken.TokenRange,
								currentToken.MorestachioExpression,
								currentToken.FindOption<bool>("ScopeTo"), GetPublicOptions(currentToken));
							buildStack.Push(new DocumentScope(nestedDocument, getScope));
							AddOrThrow(currentDocumentItem.Document, nestedDocument);

							break;
						}
						case TokenType.SwitchCaseOpen:
						{
							var nestedDocument = new SwitchCaseDocumentItem(currentToken.TokenRange, currentToken.MorestachioExpression, GetPublicOptions(currentToken));
							buildStack.Push(new DocumentScope(nestedDocument, getScope));
							AddOrThrow(currentDocumentItem.Document, nestedDocument);

							break;
						}
						case TokenType.SwitchDefaultOpen:
						{
							var nestedDocument = new SwitchDefaultDocumentItem(currentToken.TokenRange, GetPublicOptions(currentToken));
							buildStack.Push(new DocumentScope(nestedDocument, getScope));
							AddOrThrow(currentDocumentItem.Document, nestedDocument);

							break;
						}
						case TokenType.WhileLoopOpen:
						{
							var nestedDocument = new WhileLoopDocumentItem(currentToken.TokenRange, currentToken.MorestachioExpression, GetPublicOptions(currentToken));
							buildStack.Push(new DocumentScope(nestedDocument, getScope));
							AddOrThrow(currentDocumentItem.Document, nestedDocument);

							break;
						}
						case TokenType.DoLoopOpen:
						{
							var nestedDocument = new DoLoopDocumentItem(currentToken.TokenRange, currentToken.MorestachioExpression, GetPublicOptions(currentToken));
							buildStack.Push(new DocumentScope(nestedDocument, getScope));
							AddOrThrow(currentDocumentItem.Document, nestedDocument);

							break;
						}
						case TokenType.ElementOpen:
						{
							var nestedDocument = new ExpressionScopeDocumentItem(currentToken.TokenRange, currentToken.MorestachioExpression, GetPublicOptions(currentToken));
							buildStack.Push(new DocumentScope(nestedDocument, getScope));
							AddOrThrow(currentDocumentItem.Document, nestedDocument);

							break;
						}
						case TokenType.RepeatLoopOpen:
						{
							var nestedDocument = new RepeatDocumentItem(currentToken.TokenRange, currentToken.MorestachioExpression, GetPublicOptions(currentToken));
							buildStack.Push(new DocumentScope(nestedDocument, getScope));
							AddOrThrow(currentDocumentItem.Document, nestedDocument);

							break;
						}
						case TokenType.InvertedElementOpen:
						{
							var nestedDocument = new InvertedExpressionScopeDocumentItem(currentToken.TokenRange,
								currentToken.MorestachioExpression, GetPublicOptions(currentToken));
							buildStack.Push(new DocumentScope(nestedDocument, getScope));
							AddOrThrow(currentDocumentItem.Document, nestedDocument);

							break;
						}
						case TokenType.CollectionClose:
						case TokenType.ForeachCollectionClose:
						case TokenType.ElementClose:
						case TokenType.IfClose:
						case TokenType.ElseClose:
						case TokenType.ElseIfClose:
						case TokenType.WhileLoopClose:
						case TokenType.DoLoopClose:
						case TokenType.RepeatLoopClose:
						case TokenType.SwitchCaseClose:
						case TokenType.SwitchDefaultClose:
						case TokenType.SwitchClose:
						case TokenType.NoPrintClose:
						case TokenType.PartialDeclarationClose:
						case TokenType.IsolationScopeClose:
							CloseScope(buildStack, currentToken, currentDocumentItem);

							break;
						case TokenType.EscapedSingleValue:
						case TokenType.UnescapedSingleValue:
						{
							var nestedDocument = new PathDocumentItem(currentToken.TokenRange,
								currentToken.MorestachioExpression,
								knownType.Equals(TokenType.EscapedSingleValue), GetPublicOptions(currentToken));
							AddOrThrow(currentDocumentItem.Document, nestedDocument);

							break;
						}
						case TokenType.PartialDeclarationOpen:
						{
							// currently same named partials will override each other
							// to allow recursive calls of partials we first have to declare the partial and then load it as we would parse
							// -the partial as a whole and then add it to the list would lead to unknown calls of partials inside the partial
							var partialDocumentItem = new PartialDocumentItem(currentToken.TokenRange,
								currentToken.Value,
								GetPublicOptions(currentToken));
							buildStack.Push(new DocumentScope(partialDocumentItem, getScope));
							AddOrThrow(currentDocumentItem.Document, partialDocumentItem);

							break;
						}
						case TokenType.RenderPartial:
#pragma warning disable CS0618
							AddOrThrow(currentDocumentItem.Document, new RenderPartialDocumentItem(currentToken.TokenRange,
#pragma warning restore CS0618
								currentToken.Value,
								currentToken.MorestachioExpression, GetPublicOptions(currentToken)));

							break;
						case TokenType.ImportPartial:
							AddOrThrow(currentDocumentItem.Document, new ImportPartialDocumentItem(currentToken.TokenRange,
								currentToken.MorestachioExpression,
								currentToken.FindOption<IMorestachioExpression>("Context"),
								GetPublicOptions(currentToken)));

							break;
						case TokenType.IsolationScopeOpen:
						{
							var nestedDocument =
								new IsolationScopeDocumentItem(currentToken.TokenRange,
									currentToken.FindOption<IsolationOptions>("IsolationType"),
									currentToken.FindOption<IMorestachioExpression>("IsolationScopeArg"),
									GetPublicOptions(currentToken));
							AddOrThrow(currentDocumentItem.Document, nestedDocument);
							buildStack.Push(new DocumentScope(nestedDocument, getScope));

							break;
						}
						case TokenType.Alias:
						{
							var scope = GetVariableScope(buildStack);

							var nestedDocument = new AliasDocumentItem(currentToken.TokenRange,
								currentToken.Value,
								scope.VariableScopeNumber,
								GetPublicOptions(currentToken));
							AddOrThrow(currentDocumentItem.Document, nestedDocument);
							currentDocumentItem.LocalVariables.Add(currentToken.Value);

							break;
						}
						case TokenType.VariableVar:
						{
							EvaluateVariableDocumentItem nestedDocument;

							var isolationParent = buildStack.FirstOrDefault(e => e.Document is IsolationScopeDocumentItem doc &&
								doc.Isolation.HasFlag(IsolationOptions.VariableIsolation));

							if (isolationParent != null)
							{
								nestedDocument = new EvaluateVariableDocumentItem(currentToken.TokenRange,
									currentToken.Value,
									currentToken.MorestachioExpression,
									isolationParent.VariableScopeNumber,
									GetPublicOptions(currentToken));
								isolationParent.LocalVariables.Add(currentToken.Value);
							}
							else
							{
								nestedDocument = new EvaluateVariableDocumentItem(currentToken.TokenRange,
									currentToken.Value,
									currentToken.MorestachioExpression, GetPublicOptions(currentToken));
							}

							AddOrThrow(currentDocumentItem.Document, nestedDocument);

							break;
						}
						case TokenType.VariableLet:
						{
							var scope = 0;

							if (buildStack.Count > 1)
							{
								scope = GetVariableScope(buildStack)
									.VariableScopeNumber;
							}

							var nestedDocument = new EvaluateLetVariableDocumentItem(currentToken.TokenRange,
								currentToken.Value,
								currentToken.MorestachioExpression, scope, GetPublicOptions(currentToken));
							AddOrThrow(currentDocumentItem.Document, nestedDocument);

							if (buildStack.Count > 1)
							{
								currentDocumentItem.LocalVariables.Add(currentToken.Value);
							}

							break;
						}
						case TokenType.WriteLineBreak:
							AddOrThrow(currentDocumentItem.Document, new TextEditDocumentItem(currentToken.TokenRange,
								new AppendLineBreakTextOperation(),
								currentToken.IsEmbeddedToken, GetPublicOptions(currentToken)));

							break;
						case TokenType.TrimLineBreak:
							textEdits.Add(new TextEditDocumentItem(currentToken.TokenRange,
								new TrimLineBreakTextOperation()
								{
									LineBreaks = 1,
									LineBreakTrimDirection = LineBreakTrimDirection.Begin
								}, currentToken.IsEmbeddedToken, GetPublicOptions(currentToken)));

							break;
						case TokenType.TrimLineBreaks:
							textEdits.Add(new TextEditDocumentItem(currentToken.TokenRange,
								new TrimLineBreakTextOperation()
								{
									LineBreaks = currentToken.FindOption<bool>("All") ? -1 : 0,
									LineBreakTrimDirection = LineBreakTrimDirection.Begin
								}, currentToken.IsEmbeddedToken, GetPublicOptions(currentToken)));

							break;
						case TokenType.TrimPrependedLineBreaks:
							textEdits.Add(new TextEditDocumentItem(currentToken.TokenRange,
								new TrimLineBreakTextOperation()
								{
									LineBreaks = currentToken.FindOption<bool>("All") ? -1 : 0,
									LineBreakTrimDirection = LineBreakTrimDirection.End
								}, currentToken.IsEmbeddedToken, GetPublicOptions(currentToken)));

							break;
						case TokenType.TrimEverything:
							textEdits.Add(new TextEditDocumentItem(currentToken.TokenRange,
								new TrimAllWhitespacesTextOperation(),
								currentToken.IsEmbeddedToken, GetPublicOptions(currentToken)));

							break;
						case TokenType.NoPrintOpen:
						{
							var nestedDocument = new NoPrintDocumentItem(currentToken.TokenRange, GetPublicOptions(currentToken));
							AddOrThrow(currentDocumentItem.Document, nestedDocument);
							buildStack.Push(new DocumentScope(nestedDocument, getScope));

							break;
						}
						case TokenType.Comment:
						case TokenType.BlockComment:
						{
							//just ignore this part and print nothing
							if (options.TokenizeComments)
							{
								AddOrThrow(currentDocumentItem.Document,
									new CommentDocumentItem(currentToken.TokenRange, currentToken.Value,
										GetPublicOptions(currentToken), knownType.Equals(TokenType.BlockComment)));
							}

							break;
						}
						case TokenType.SwitchOptionScopeTo:
						default:
						{
							var tokenOptions = GetPublicOptions(currentToken);

							var customDocumentItemProvider =
								options.CustomDocumentItemProviders.FindTokenProvider(currentToken, options, tokenOptions);
							var nestedDocument = customDocumentItemProvider?.Parse(currentToken, options, buildStack, getScope, tokenOptions);

							if (nestedDocument != null)
							{
								AddOrThrow(currentDocumentItem.Document, nestedDocument);
							}

							break;
						}
					}
				}
			}

			if (buildStack.Count > 1)
			{
				//var invalidScopedElements = buildStack
				//throw new MorestachioSyntaxError(new Tokenizer.TextRange(){Character = }, );
				throw new InvalidOperationException(
					"There is an Error with the Parser. The Parser still contains unscoped builds: " +
					buildStack.Select(e => e.Document.GetType().Name).Aggregate((e, f) => e + ", " + f));
			}

			if (buildStack.Count == 0)
			{
				//var invalidScopedElements = buildStack
				//throw new MorestachioSyntaxError(new Tokenizer.TextRange(){Character = }, );
				throw new InvalidOperationException(
					"There is an Error with the Parser. The Parser still closed the root scope prematurely");
			}

			return buildStack.Pop().Document;
		}

		private static void CloseScope(Stack<DocumentScope> documentScopes, in TokenPair currentToken, DocumentScope currentDocumentItem)
		{
			DocumentScope scope = documentScopes.Peek();
			if (!(scope.Document is IBlockDocumentItem blockDocument))
			{
				throw new InvalidOperationException(
					$"Closing an token '{currentToken.Type}' at '{currentToken.TokenRange}'" +
					$" that is not of type '{typeof(IBlockDocumentItem)}' is not possible.");
			}

			blockDocument.BlockClosingOptions = GetPublicOptions(currentToken);
			blockDocument.BlockLocation = currentToken.TokenRange;

			if (scope.HasAlias) //are we in a alias then remove it
			{
				foreach (var scopeLocalVariable in scope.LocalVariables)
				{
					AddOrThrow(currentDocumentItem.Document,
						new RemoveAliasDocumentItem(currentToken.TokenRange,
							scopeLocalVariable,
							scope.VariableScopeNumber, null));
				}
			}

			// remove the last document from the stack and go back to the parents
			documentScopes.Pop();
		}

		private static void AddOrThrow(IDocumentItem document, IDocumentItem child)
		{
			if (document is IBlockDocumentItem block)
			{
				block.Add(child);

				return;
			}

			throw new MorestachioParserException($"Internal parsing error. Tried to add '{child}' to '{document}' which is not a {typeof(IBlockDocumentItem)}");
		}

		private static IEnumerable<ITokenOption> GetPublicOptions(TokenPair token)
		{
			var publicOptions = token.TokenOptions?.Where(e => e.Persistent).ToArray();
			return publicOptions?.Length > 0 ? publicOptions : null;
		}

		private static DocumentScope GetVariableScope(Stack<DocumentScope> buildStack)
		{
			return buildStack.FirstOrDefault(e => e.VariableScopeNumber != -1);
		}
	}
}