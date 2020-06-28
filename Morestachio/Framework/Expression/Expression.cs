using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Morestachio.Framework.Expression.Visitors;
using Morestachio.ParserErrors;

namespace Morestachio.Framework.Expression
{
	/// <summary>
	///		Defines a path with an optional formatting expression including sub expressions
	/// </summary>
	[DebuggerTypeProxy(typeof(ExpressionDebuggerDisplay))]
	[Serializable]
	public class MorestachioExpression : IMorestachioExpression
	{
		internal MorestachioExpression()
		{
			PathParts = new List<KeyValuePair<string, PathTokenizer.PathType>>();
			Formats = new List<ExpressionArgument>();
		}

		internal MorestachioExpression(CharacterLocation location) : this()
		{
			Location = location;
			PathTokenizer = new PathTokenizer();
		}

		protected MorestachioExpression(SerializationInfo info, StreamingContext context)
		{
			PathParts = info.GetValue(nameof(PathParts), typeof(IList<KeyValuePair<string, PathTokenizer.PathType>>))
				as IList<KeyValuePair<string, PathTokenizer.PathType>>;
			Formats = info.GetValue(nameof(Formats), typeof(IList<ExpressionArgument>))
				as IList<ExpressionArgument>;
			FormatterName = info.GetString(nameof(FormatterName));
			Location = CharacterLocation.FromFormatString(info.GetString(nameof(Location)));
		}

		/// <inheritdoc />
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(nameof(PathParts), PathParts);
			info.AddValue(nameof(Formats), Formats);
			info.AddValue(nameof(FormatterName), FormatterName);
			info.AddValue(nameof(Location), Location.ToFormatString());
		}

		/// <inheritdoc />
		public XmlSchema GetSchema()
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public void ReadXml(XmlReader reader)
		{
			Location = CharacterLocation.FromFormatString(reader.GetAttribute(nameof(Location)));
			FormatterName = reader.GetAttribute(nameof(FormatterName));
			var pathParts = reader.GetAttribute(nameof(PathParts));
			if (pathParts != null)
			{
				PathParts = pathParts.Split(',').Select(f =>
				{
					if (f.StartsWith("["))
					{
						return new KeyValuePair<string, PathTokenizer.PathType>(null,
							(PathTokenizer.PathType)Enum.Parse(typeof(PathTokenizer.PathType), f.TrimStart('[').TrimEnd(']')));
					}

					return new KeyValuePair<string, PathTokenizer.PathType>(f, PathTokenizer.PathType.DataPath);
				}).ToList();
			}

			if (reader.IsEmptyElement)
			{
				return;
			}

			reader.ReadStartElement();
			while (reader.Name == "Format" && reader.NodeType != XmlNodeType.EndElement)
			{
				var formatSubTree = reader.ReadSubtree();
				formatSubTree.Read();

				var expressionArgument = new ExpressionArgument();
				Formats.Add(expressionArgument);

				expressionArgument.ReadXml(formatSubTree);


				reader.Skip();
				reader.ReadEndElement();

				//var format = new ExpressionArgument(CharacterLocation.FromFormatString(reader.GetAttribute(nameof(ExpressionArgument.Location))));
				//format.Name = reader.GetAttribute(nameof(ExpressionArgument.Name));
				//Formats.Add(format);
				//reader.ReadStartElement();

				//var childTree = reader.ReadSubtree();
				//childTree.Read();
				//var exp = childTree.ParseExpressionFromKind();
				//format.MorestachioExpression = exp;
				//reader.Skip();

				//reader.ReadEndElement();
			}
		}

		/// <inheritdoc />
		public void WriteXml(XmlWriter writer)
		{
			writer.WriteAttributeString(nameof(Location), Location.ToFormatString());
			if (FormatterName != null)
			{
				writer.WriteAttributeString(nameof(FormatterName), FormatterName);
			}

			if (PathParts.Any())
			{
				var pathStr = string.Join(",", PathParts.Select(f =>
				{
					if (f.Value == PathTokenizer.PathType.DataPath)
					{
						return f.Key;
					}

					return $"[{f.Value.ToString()}]";
				}));
				writer.WriteAttributeString(nameof(PathParts), pathStr);
			}
			foreach (var expressionArgument in Formats)
			{
				writer.WriteStartElement("Format");
				expressionArgument.WriteXml(writer);
				writer.WriteEndElement();//</Format>
			}
		}

		internal PathTokenizer PathTokenizer { get; }

		/// <summary>
		///		Contains all parts of the path
		/// </summary>
		public IList<KeyValuePair<string, PathTokenizer.PathType>> PathParts { get; private set; }

		/// <summary>
		///		If filled contains the arguments to be used to format the value located at PathParts
		/// </summary>
		public IList<ExpressionArgument> Formats { get; private set; }

		/// <summary>
		///		If set the formatter name to be used to format the value located at PathParts
		/// </summary>
		public string FormatterName { get; private set; }

		/// <inheritdoc />
		public CharacterLocation Location { get; set; }

		/// <inheritdoc />
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
				value = await formatterArgument.MorestachioExpression.GetValue(value, scopeData);

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

		/// <inheritdoc />
		public void Accept(IMorestachioExpressionVisitor visitor)
		{
			visitor.Visit(this);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			var visitor = new ToParsableStringExpressionVisitor();
			Accept(visitor);
			return visitor.StringBuilder.ToString();
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
		private static char?[] _closingChars = {'.', ',', ')'};

		/// <summary>
		///		Parses the text into one or more expressions
		/// </summary>
		/// <param name="text">the path to parse excluding {{ and }}</param>
		/// <param name="context">The context used to tokenize the text</param>
		/// <param name="indexVar">the index of where the parsing stoped</param>
		/// <returns></returns>
		public static IMorestachioExpression ParseFrom(string text,
			TokenzierContext context,
			out int indexVar)
		{
			var index = 0;
			text = text.Trim();
			if (!text.Contains("("))
			{
				var expression = new MorestachioExpression(context.CurrentLocation);
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
				indexVar = text.Length;
				context.AdvanceLocation(text.Length);
				return expression;
			}

			IMorestachioExpression morestachioExpressions = null;
			HeaderTokenMatch currentScope = null;
			//this COULD be made with regexes, i have made it and rejected it as it was no longer readable in any way.
			var tokenScopes = new Stack<HeaderTokenMatch>();
			tokenScopes.Push(new HeaderTokenMatch
			{
				State = TokenState.DecideArgumentType,
				TokenLocation = context.CurrentLocation
			});
			//var currentPathPart = new StringBuilder();
			char formatChar;

			int SkipWhitespaces()
			{
				if (Tokenizer.IsWhiteSpaceDelimiter(formatChar))
				{
					return Seek(f => !Tokenizer.IsWhiteSpaceDelimiter(f), true);
				}

				return index;
			}

			int Seek(Func<char, bool> condition, bool includeCurrent)
			{
				var idx = index;
				if (!includeCurrent)
				{
					if (idx + 1 >= text.Length)
					{
						return idx;
					}

					idx++;
				}

				for (; idx < text.Length; idx++)
				{
					formatChar = text[idx];
					if (condition(formatChar))
					{
						return idx;
					}
				}
				return idx;
			}

			bool Eoex()
			{
				index = SkipWhitespaces();
				return index + 1 == text.Length;
			}
			
			char? SeekNext(out int nIndex)
			{
				nIndex = Seek(f => Tokenizer.IsExpressionChar(f) || Tokenizer.IsPathDelimiterChar(f), false);
				if (nIndex != -1)
				{
					return text[nIndex];
				}

				return null;
			}

			char[] Take(Func<char, bool> condition, out int idx)
			{
				idx = index;
				var chrs = new List<char>();
				for (int i = idx; i < text.Length; i++)
				{
					var c = text[i];
					idx = i;
					if (!condition(c))
					{
						break;
					}
					chrs.Add(c);
				}

				return chrs.ToArray();
			}

			void TerminateCurrentScope(bool tryTerminate = false)
			{
				if ((tryTerminate && tokenScopes.Any()) || !tryTerminate)
				{
					currentScope.State = TokenState.EndOfExpression;
					tokenScopes.Pop();
				}
			}

			int EndParameterBracket()
			{
				var parent = currentScope.Parent?.Parent;
				char? seekNext;
				while (_closingChars.Contains(seekNext = SeekNext(out var seekIndex)))
				{
					index = seekIndex;
					if (seekNext == ')') //the next char is also an closing bracket so there is no next parameter nor an followup expression
					{
						//there is nothing after this expression so close it
						TerminateCurrentScope();
						HeaderTokenMatch scope = null;
						if (tokenScopes.Any())
						{
							scope = tokenScopes.Peek();
						}

						if (scope?.Value is MorestachioExpressionList)//if the parent expression is a list close that list too
						{
							TerminateCurrentScope();
							parent = parent?.Parent;
						}
						parent = parent?.Parent;//set the new parent for followup expression
					}
					else
					{
						//there is something after this expression
						if (seekNext == '.')//the next char indicates a followup expression
						{
							HeaderTokenMatch scope = null;
							if (tokenScopes.Any())
							{
								scope = tokenScopes.Peek();
							}
							if (scope != null && scope.Parent != null) //this is a nested expression
							{
								if ((scope.Parent?.Value is MorestachioExpressionList))//the parents parent expression is already a list so close the parent
								{
									scope = scope.Parent;
									TerminateCurrentScope();
								}
								else if (!(scope.Value is MorestachioExpressionList))//the parent is not an list expression so replace the parent with a list expression
								{
									var oldValue = scope.Value as MorestachioExpression;
									scope.Value = new MorestachioExpressionList(new List<IMorestachioExpression>
									{
										oldValue
									});
									var parValue = (scope.Parent.Value as MorestachioExpression);
									var hasFormat = parValue.Formats.FirstOrDefault(f => f.MorestachioExpression == oldValue);
									if (hasFormat != null)
									{
										hasFormat.MorestachioExpression = scope.Value;
									}
								}
								parent = scope;	
							}
							else
							{
								//we are at root level no need to do anything as the root is already a list expression
								TerminateCurrentScope(true);
							}
						}
						else
						{
							//the next char indicates a new parameter so close this expression and allow next
							TerminateCurrentScope(true);
						}

						if (!Eoex())
						{
							HeaderTokenMatch item;
							if (parent != null)
							{
								//if there is a parent set then this indicates a new argument
								item = new HeaderTokenMatch
								{
									State = TokenState.ArgumentStart,
									Parent = parent,
									TokenLocation = context.CurrentLocation.Offset(index + 1)
								};
							}
							else
							{
								//if there is no parent set this indicates a followup expression
								item = new HeaderTokenMatch
								{
									State = TokenState.StartOfExpression,
									Parent = parent,
									Value = new MorestachioExpression(context.CurrentLocation.Offset(index)),
									TokenLocation = context.CurrentLocation.Offset(index + 1)
								};
							}

							tokenScopes.Push(item);
						}

						if (seekNext == '.')
						{
							index--;
						}

						break;
					}

					if (Eoex())
					{
						TerminateCurrentScope(true);
						break;
					}
				}

				return index;
			}

			for (index = 0; index < text.Length; index++)
			{
				currentScope = tokenScopes.Peek();
				formatChar = text[index];
				switch (currentScope.State)
				{
					case TokenState.ArgumentStart:
						//we are at the start of an argument
						index = SkipWhitespaces();

						if (formatChar == '[')
						{
							index++;
							currentScope.ArgumentName = new string(Take(f => f != ']', out var idxa));
							index = idxa + 1;
						}

						index--; //reprocess the char
						currentScope.State = TokenState.DecideArgumentType;

						break;
					case TokenState.DecideArgumentType:
						//we are at the start of an argument
						index = SkipWhitespaces();

						var idx = index;
						if (Tokenizer.IsStringDelimiter(formatChar))
						{
							//this is an string
							var cidx = context.Character;
							currentScope.Value = MorestachioExpressionString.ParseFrom(text, index, context, out index);
							context.SetLocation(cidx);
							currentScope.State = TokenState.Expression;
						}
						else if (Tokenizer.IsExpressionChar(formatChar))
						{
							currentScope.State = TokenState.Expression;
							//this is the first char of an expression.
							index--;
							currentScope.Value = new MorestachioExpression(context.CurrentLocation.Offset(index));
						}
						else
						{
							//this is not the start of an expression and not a string
							context.Errors.Add(new InvalidPathSyntaxError(
								context.CurrentLocation.Offset(index)
									.AddWindow(new CharacterSnippedLocation(1, index, text)),
								currentScope.Value.ToString()));
							indexVar = 0;
							return null;
						}

						if (currentScope.Parent == null)
						{
							if (morestachioExpressions == null)
							{
								morestachioExpressions = currentScope.Value;
							}
							else
							{
								if (morestachioExpressions is MorestachioExpressionList expList)
								{
									expList.Add(currentScope.Value);
								}
								else
								{
									var oldExp = morestachioExpressions;
									morestachioExpressions = new MorestachioExpressionList(new List<IMorestachioExpression>()
									{
										oldExp,
										currentScope.Value
									})
									{
										Location = oldExp.Location
									};	
								}
							}
						}
						else
						{
							if (currentScope.Parent?.Value is MorestachioExpression exp)
							{
								exp.Formats.Add(
									new ExpressionArgument(context.CurrentLocation.Offset(idx))
									{
										MorestachioExpression = currentScope.Value,
										Name = currentScope.ArgumentName
									});
							}
							
							if (currentScope.Parent?.Value is MorestachioExpressionList expList)
							{
								expList.Add(currentScope.Value);
							}
						}
						break;
					case TokenState.Expression:
						index = SkipWhitespaces();
						if (formatChar == '(')
						{
							//in this case the current path has ended and we must prepare for arguments

							//if this scope was opened multible times, set an error
							if (currentScope.BracketsCounter > 1)
							{
								context.Errors.Add(new MorestachioSyntaxError(context.CurrentLocation.Offset(index)
										.AddWindow(new CharacterSnippedLocation(1, index, text)),
									"Format", "(", "Name of Formatter",
									"Did expect to find the name of a formatter but found single path. Did you forgot to put an . before the 2nd formatter?"));
								indexVar = 0;
								return null;
							}

							var currentExpression = currentScope.Value as MorestachioExpression;
							currentExpression.CompilePath(context, index);
							if (currentExpression.PathParts.Count == 0)
							{
								//in this case there are no parts in the path that indicates ether {{(}} or {{data.(())}}
								context.Errors.Add(new MorestachioSyntaxError(context.CurrentLocation.Offset(index)
										.AddWindow(new CharacterSnippedLocation(1, index, text)),
									"Format", "(", "Name of Formatter",
									"Did expect to find the name of a formatter but found single path. Did you forgot to put an . before the 2nd formatter?"));
								indexVar = 0;
								return null;
							}

							//get the last part of the path as the name of the formatter
							currentExpression.FormatterName = currentExpression.PathTokenizer.GetFormatterName();
							currentScope.BracketsCounter++;
							//seek the next non whitespace char. That should be ether " or an expression char
							index = Seek(f => !Tokenizer.IsWhiteSpaceDelimiter(f), false);
							if (formatChar == ')')
							{
								//the only next char is the closing bracket so no arguments
								currentScope.BracketsCounter--;
								index = EndParameterBracket();
							}
							else
							{
								//indicates the start of an argument
								index--;
								tokenScopes.Push(new HeaderTokenMatch
								{
									State = TokenState.ArgumentStart,
									Parent = currentScope
								});
							}
						}
						else if (formatChar == ')')
						{
							////close the current scope. This scope is an parameter expression
							//TerminateCurrentScope();

							var parentExpression = currentScope.Parent?.Value as MorestachioExpression;
							currentScope.Parent.BracketsCounter--;
							if (currentScope.Value is MorestachioExpression currentScopeValue)
							{
								currentScopeValue.CompilePath(context, index);
								if (currentScopeValue != null &&
								    !currentScopeValue.PathParts.Any() && parentExpression?.Formats.Any() == true)
								{
									context.Errors.Add(new InvalidPathSyntaxError(
										context.CurrentLocation.Offset(index)
											.AddWindow(new CharacterSnippedLocation(1, index, text)),
										currentScope.Value.ToString()));
								}
							}
							
							TerminateCurrentScope();
							index = EndParameterBracket();
						}
						else if (formatChar == ',')
						{
							if (currentScope.Value is MorestachioExpression currentScopeValue)
							{
								currentScopeValue.CompilePath(context, index);
								if (currentScopeValue != null &&
									!currentScopeValue.PathParts.Any())
								{
									context.Errors.Add(
										new InvalidPathSyntaxError(currentScopeValue.Location
												.AddWindow(new CharacterSnippedLocation(1, index, text)),
											","));
								}
							}

							TerminateCurrentScope();
							//add a new one into the stack as , indicates a new argument
							tokenScopes.Push(new HeaderTokenMatch
							{
								State = TokenState.ArgumentStart,
								Parent = currentScope.Parent
							});
						}
						else if (currentScope.BracketsCounter == 0)
						{
							//we are in an path expression
							//like data.data.data.data

							if ((currentScope.Value as MorestachioExpression)?.PathTokenizer.Add(formatChar) == false)
							{
								context.Errors.Add(
									new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
											.AddWindow(new CharacterSnippedLocation(1, index, text)),
										formatChar.ToString()));
							}

							if (Eoex())
							{
								//an expression can be ended just at any time
								//it just should not end with an .

								if (formatChar == '.')
								{
									context.Errors.Add(new MorestachioSyntaxError(context.CurrentLocation.Offset(index)
											.AddWindow(new CharacterSnippedLocation(1, index, text)),
										"Format", "(", "Name of Formatter",
										"Did not expect a . at the end of an expression without an formatter"));
								}

								(currentScope.Value as MorestachioExpression).CompilePath(context, index);
								TerminateCurrentScope();
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
					case TokenState.StartOfExpression:
						index = SkipWhitespaces();
						if (!Tokenizer.IsStartOfExpressionPathChar(formatChar) && formatChar != ')')
						{
							context.Errors.Add(new InvalidPathSyntaxError(
								context.CurrentLocation.Offset(index)
									.AddWindow(new CharacterSnippedLocation(1, index, text)),
								currentScope.Value.ToString()));
							indexVar = 0;
							return null;
						}

						if (morestachioExpressions != null)
						{
							if (formatChar != '.')
							{
								context.Errors.Add(new InvalidPathSyntaxError(
									context.CurrentLocation.Offset(index)
										.AddWindow(new CharacterSnippedLocation(1, index, text)),
									currentScope.Value.ToString()));

								indexVar = 0;
								return null;
							}
						}

						index--;
						currentScope.State = TokenState.DecideArgumentType;
						break;
					case TokenState.EndOfExpression:
						//Console.WriteLine();
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
			return morestachioExpressions;
		}

		private class ExpressionDebuggerDisplay
		{
			private readonly MorestachioExpression _exp;

			public ExpressionDebuggerDisplay(MorestachioExpression exp)
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

		protected bool Equals(MorestachioExpression other)
		{
			if (other.PathParts.Count != PathParts.Count)
			{
				return false;
			}
			if (other.Formats.Count != Formats.Count)
			{
				return false;
			}

			if (other.FormatterName != FormatterName)
			{
				return false;
			}

			if (!other.Location.Equals(Location))
			{
				return false;
			}

			for (var index = 0; index < PathParts.Count; index++)
			{
				var thisPart = PathParts[index];
				var thatPart = other.PathParts[index];
				if (thatPart.Value != thisPart.Value || thatPart.Key != thisPart.Key)
				{
					return false;
				}
			}

			for (var index = 0; index < Formats.Count; index++)
			{
				var thisArgument = Formats[index];
				var thatArgument = other.Formats[index];
				if (!thisArgument.Equals(thatArgument))
				{
					return false;
				}
			}

			return true;
		}

		public bool Equals(IMorestachioExpression other)
		{
			return Equals((object)other);
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

			return Equals((MorestachioExpression)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (PathParts != null ? PathParts.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Formats != null ? Formats.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (FormatterName != null ? FormatterName.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Location != null ? Location.GetHashCode() : 0);
				return hashCode;
			}
		}
	}
}