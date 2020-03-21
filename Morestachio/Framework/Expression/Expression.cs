using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Morestachio.Framework.Expression.Renderer;
using Morestachio.ParserErrors;

namespace Morestachio.Framework.Expression
{
	[DebuggerTypeProxy(typeof(ExpressionDebuggerDisplay))]
	public class Expression : IExpression
	{
		internal Expression(CharacterLocation location)
		{
			Location = location;
			PathParts = new List<KeyValuePair<string, PathTokenizer.PathType>>();
			Formats = new List<ExpressionArgument>();
			PathTokenizer = new PathTokenizer();
		}

		private class ExpressionDebuggerDisplay
		{
			private readonly Expression _exp;

			public ExpressionDebuggerDisplay(Expression exp)
			{
				_exp = exp;
			}

			public string Path
			{
				get { return string.Join(".", _exp.PathParts); }
			}

			public string FormatterName
			{
				get { return _exp.FormatterName; }
			}

			public string Expression
			{
				get { return _exp.ToString(); }
			}
		}

		public override string ToString()
		{
			return ExpressionRenderer.RenderExpression(this).ToString();
		}

		internal void CompilePath(
			TokenzierContext context,
			int index)
		{
			PathParts = PathTokenizer.Compile(out var hasError);
			if (hasError != -1)
			{
				context.Errors.Add(
					new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
							.AddWindow(new CharacterSnippedLocation(1, hasError, PathTokenizer.CurrentPart)),
						PathTokenizer.CurrentPart));
			}
		}

		internal PathTokenizer PathTokenizer { get; private set; }
		public IList<KeyValuePair<string, PathTokenizer.PathType>> PathParts { get; set; }
		public IList<ExpressionArgument> Formats { get; set; }
		public string FormatterName { get; set; }

		public CharacterLocation Location { get; set; }
		public async Task<ContextObject> GetValue(ContextObject contextObject, ScopeData scopeData)
		{
			var contextForPath = await contextObject.GetContextForPath(PathParts, scopeData);
			if (!Formats.Any() && FormatterName == null)
			{
				return contextForPath;
			}

			var argList = new List<KeyValuePair<string, object>>();
			foreach (var formatterArgument in Formats)
			{
				var value = contextObject.FindNextNaturalContextObject().CloneForEdit();
				value = await formatterArgument.Expression.GetValue(value, scopeData);

				if (value == null)
				{
					argList.Add(new KeyValuePair<string, object>(formatterArgument.Name, null));
				}
				else
				{
					await value.EnsureValue();
					argList.Add(new KeyValuePair<string, object>(formatterArgument.Name, value.Value));
				}
			}

			var formatterdContext = contextForPath.CloneForEdit();
			formatterdContext.Value = await formatterdContext.Format(FormatterName, argList.ToArray());
			return formatterdContext;
		}

		public static IExpression[] ParseFrom(string text,
			TokenzierContext context,
			out int indexVar)
		{
			var index = 0;
			text = text.Trim();
			if (!text.Contains("("))
			{
				var expression = new Expression(context.CurrentLocation);
				for (; index < text.Length; index++)
				{
					var c = text[index];
					if (!expression.PathTokenizer.Add(c))
					{
						context.Errors.Add(
							new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
									.AddWindow(new CharacterSnippedLocation(1, index, text)),
								text[index].ToString()));
					}
				}
				
				
				expression.CompilePath(context, 0);

				//foreach (var parts in text.Split('.'))
				//{
				//	expression.AddPathPart(parts);
				//}

				indexVar = text.Length;
				context.AdvanceLocation(text.Length);
				return new IExpression[]
				{
					expression
				};
			}

			var expressions = new List<IExpression>();
			//this COULD be made with regexes, i have made it and rejected it as it was no longer readable in any way.
			var tokenScopes = new Stack<HeaderTokenMatch>();
			tokenScopes.Push(new HeaderTokenMatch
			{
				State = TokenState.DecideArgumentType,
				TokenLocation = context.CurrentLocation
			});
			//var currentPathPart = new StringBuilder();
			char formatChar;

			void SkipWhitespaces()
			{
				if (Tokenizer.IsWhiteSpaceDelimiter(formatChar))
				{
					for (; index < text.Length; index++)
					{
						formatChar = text[index];
						if (Tokenizer.IsWhiteSpaceDelimiter(formatChar))
						{
							continue;
						}
						break;
					}
				}
			}

			for (index = 0; index < text.Length; index++)
			{
				var currentScope = tokenScopes.Peek();
				formatChar = text[index];
				var state = currentScope.State;
				switch (state)
				{
					case TokenState.None:
						SkipWhitespaces();

						//var argumentScope = new HeaderTokenMatch();
						//currentScope.Arguments.Add(argumentScope);
						//tokenScopes.Push(argumentScope);
						index--;
						currentScope.State = TokenState.ArgumentStart;
						break;
					case TokenState.ArgumentStart:
						//we are at the start of an argument
						SkipWhitespaces();

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
						SkipWhitespaces();

						if (Tokenizer.IsStringDelimiter(formatChar))
						{
							//this is an string
							var cidx = context.Character;
							var idx = index;
							currentScope.Value = ExpressionString.ParseFrom(text, index, context, out index);
							context.SetLocation(cidx);
							currentScope.State = TokenState.Expression;
							((currentScope.Parent)?.Value as Expression)?.Formats.Add(new ExpressionArgument(context.CurrentLocation.Offset(idx))
							{
								Expression = currentScope.Value,
								Name = currentScope.ArgumentName
							});
							//tokenScopes.Pop();
						}
						else if (Tokenizer.IsExpressionChar(formatChar))
						{
							currentScope.State = TokenState.Expression;
							//this is the first char of an expression.
							index--;
							currentScope.Value = new Expression(context.CurrentLocation.Offset(index));

							((currentScope.Parent)?.Value as Expression)?.Formats.Add(new ExpressionArgument(context.CurrentLocation.Offset(index))
							{
								Expression = currentScope.Value,
								Name = currentScope.ArgumentName
							});
						}
						else
						{
							//this is not the start of an expression and not a string
							context.Errors.Add(new InvalidPathSyntaxError(
								context.CurrentLocation.Offset(index)
									.AddWindow(new CharacterSnippedLocation(1, index, text)),
								currentScope.Value.ToString()));
							indexVar = 0;
							return new IExpression[0];
						}

						break;
					case TokenState.Expression:
						SkipWhitespaces();
						if (!Tokenizer.IsExpressionChar(formatChar))
						{
							context.Errors.Add(new MorestachioSyntaxError(context.CurrentLocation.Offset(index)
									.AddWindow(new CharacterSnippedLocation(1, index, text)),
								"Path", "", "Path Expression", $"Did not expect the character '{formatChar}'"));
							indexVar = 0;
							return new IExpression[0];
						}
						
						if (formatChar == '(')
						{
							if (currentScope.BracketsCounter > 1)
							{
								context.Errors.Add(new MorestachioSyntaxError(context.CurrentLocation.Offset(index)
										.AddWindow(new CharacterSnippedLocation(1, index, text)),
									"Format", "(", "Name of Formatter", "Did expect to find the name of a formatter but found single path. Did you forgot to put an . before the 2nd formatter?"));
								indexVar = 0;
								return new IExpression[0];
							}
							var currentExpression = (currentScope.Value as Expression);
							currentExpression.CompilePath(context, index);
							if (currentExpression.PathParts.Count == 0)
							{
								context.Errors.Add(new MorestachioSyntaxError(context.CurrentLocation.Offset(index)
										.AddWindow(new CharacterSnippedLocation(1, index, text)),
									"Format", "(", "Name of Formatter", "Did expect to find the name of a formatter but found single path. Did you forgot to put an . before the 2nd formatter?"));
								indexVar = 0;
								return new IExpression[0];
							}

							currentExpression.FormatterName = currentExpression.PathTokenizer.GetFormatterName();
							currentScope.BracketsCounter++;
							tokenScopes.Push(new HeaderTokenMatch()
							{
								State = TokenState.ArgumentStart,
								Parent = currentScope
							});

						}
						else if (formatChar == ')')
						{
							currentScope.State = TokenState.EndOfExpression;
							tokenScopes.Pop();
							(currentScope.Value as Expression)?.CompilePath(context, index);

							var parentScope = tokenScopes.TryPeek();
							if (parentScope != null)
							{
								//for expressions as the last argument we cannot know when they end so we have to evaluate "the last" argument
								parentScope.BracketsCounter--;
								var expression = (parentScope.Value as Expression);
								if (parentScope.Parent == null)
								{
									expressions.Add(expression);
									tokenScopes.Pop();
									//check if we are NOT at the end of the expression and then add a new HeaderToken
									SkipWhitespaces();
									if (index + 1 != text.Length)
									{
										var item = new HeaderTokenMatch
										{
											State = TokenState.Expression,
											Parent = null,
											Value = new Expression(context.CurrentLocation.Offset(index))
										};
										tokenScopes.Push(item);
									}
								}
							}
							else
							{
								//this was the last argument and we are at the end of the string
								expressions.Add(currentScope.Value);
								if (index + 1 != text.Length)
								{
									tokenScopes.Push(new HeaderTokenMatch()
									{
										Value = new Expression(context.CurrentLocation.Offset(index + 1)),
										State = TokenState.Expression,
										TokenLocation = context.CurrentLocation.Offset(index + 1),
										Parent = currentScope
									});
								}
							}
							//currentPathPart.Clear();

							//in case there was only one argument and this argument is empty drop the FormatterItem
							if ((currentScope.Parent?.Value is Expression parentExpression))
							{
								if (parentExpression.Formats.Count == 1
									&& parentExpression.Formats.FirstOrDefault()?.Expression is Expression maybeEmptyFormat)
								{
									if (!maybeEmptyFormat.Formats.Any()
										&& maybeEmptyFormat.FormatterName == null
										&& !maybeEmptyFormat.PathParts.Any())
									{
										parentExpression.Formats.Clear();
									}
								}
							}
						}
						else if (formatChar == ',')
						{
							(currentScope.Value as Expression)?.CompilePath(context, index);

							tokenScopes.Pop();
							var parent = tokenScopes.Peek();
							//if (currentScope.BracketsCounter == 0)
							//{
							//	//this is the end of an expression as an argument
							//	currentScope.State = TokenState.EndOfExpression;
							//	//remove it from processing stack
							//	tokenScopes.Pop();
							//}
							//add a new one into the stack as , indicates a new argument
							tokenScopes.Push(new HeaderTokenMatch()
							{
								State = TokenState.ArgumentStart,
								Parent = parent
							});
						}
						else if (currentScope.BracketsCounter == 0)
						{
							//we are in an path expression
							//like data.data.data.data

							if ((currentScope.Value as Expression)?.PathTokenizer.Add(formatChar) == false)
							{
								context.Errors.Add(
									new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
											.AddWindow(new CharacterSnippedLocation(1, index, text)),
										formatChar.ToString()));
							}
							if (index + 1 == text.Length)
							{
								//an expression can be ended just at any time
								//it just should not end with an .

								if (formatChar == '.')
								{
									context.Errors.Add(new MorestachioSyntaxError(context.CurrentLocation.Offset(index)
											.AddWindow(new CharacterSnippedLocation(1, index, text)),
										"Format", "(", "Name of Formatter", "Did not expect a . at the end of an expression without an formatter"));
								}
								(currentScope.Value as Expression).CompilePath(context, index);
								//(currentScope.Value as Expression).AddPathPart(currentPathPart.ToString());
								//currentPathPart.Clear();
								expressions.Add(currentScope.Value);
								//this is the end of an expression as an argument
								currentScope.State = TokenState.EndOfExpression;
								//remove it from processing stack
								tokenScopes.Pop();
							}
						}
						//else
						//{
						//	var argument = new HeaderTokenMatch
						//	{
						//		State = TokenState.ArgumentStart,
						//		Parent = currentScope
						//	};
						//	tokenScopes.Push(argument);
						//}

						break;
					case TokenState.EndOfExpression:
						Console.WriteLine();
						break;
				}
			}

			if (tokenScopes.Any())
			{
				context.Errors.Add(new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
						.AddWindow(new CharacterSnippedLocation(1, index, text)),
					text));
			}

			context.AdvanceLocation(index);
			indexVar = index;
			return expressions.ToArray();
		}
	}
}