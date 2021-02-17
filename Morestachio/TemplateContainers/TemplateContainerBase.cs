using System.Collections.Generic;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Tokenizing;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.TemplateContainers
{
	/// <summary>
	///		Defines the Default container for an string template and and tokenize method
	/// </summary>
	public abstract class TemplateContainerBase : ITemplateContainer
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="template"></param>
		public TemplateContainerBase(TemplateResource template)
		{
			Template = template;
		}

		/// <summary>
		///		The string template
		/// </summary>
		public TemplateResource Template { get; }

		private readonly struct InStringInfo
		{
			public InStringInfo(int index, char delimiter)
			{
				Index = index;
				Delimiter = delimiter;
			}

			public int Index { get; }
			public char Delimiter { get; }
		}

		/// <inheritdoc />
		public virtual IEnumerable<TokenMatch> Matches(TokenzierContext context)
		{
			InStringInfo isInString = new InStringInfo(-1, ' ');
			var stringEscape = false;

			var templateString = Template;
			var index = 0;
			var preLastIndex = 0;
			var charComparer = new CharComparer();
			var lastChars = new MorestachioDefaultRollingArray();

			//use the index of method for fast lookup of the next token
			while ((index = templateString.IndexOf(new string(context._prefixToken), index)) != -1)
			{
				index += context._prefixToken.Length;
				while (templateString[index] == context._prefixToken[0])//skip all excess {
				{
					index++;
				}

				var preText = templateString.Substring(preLastIndex, index - context._prefixToken.Length - preLastIndex);

				var startOfToken = index;
				var tokenCount = 0;

				var nlsIdx = 0;
				//iterrate all newlines for character humanization
				while ((nlsIdx = preText.IndexOf('\n', nlsIdx)) != -1)
				{
					context.Lines.Add(nlsIdx + preLastIndex);
					nlsIdx += 1;
				}
				while (index < templateString.Length())
				{
					var c = templateString[index];
					lastChars.Add(c);
					tokenCount++;

					if (c == '\n')
					{
						context.Lines.Add(index);
					}

					if (isInString.Index != -1 && context.CommentIntend == 0)
					{
						if (c == '\\')
						{
							stringEscape = true;
						}
						else if (stringEscape && c == isInString.Delimiter)
						{
							stringEscape = false;
						}
						else if (!stringEscape && c == isInString.Delimiter)
						{
							isInString = new InStringInfo(-1, ' ');
						}
					}
					else
					{
						if (lastChars.EndsWith(context._prefixToken))//something like "content {{"
						{
							preText = preText ?? string.Empty;
							preText += c;
							tokenCount = 0;
						}
						else if (lastChars.EndsWith(context.SuffixToken, charComparer))//something like "zzddata }}"
						{
							var tokenLength = tokenCount - context.SuffixToken.Length;

							var tokenContent = templateString.Substring(startOfToken, tokenLength);
							//it's a comment drop this on the floor, no need to even yield it.
							if (tokenContent.StartsWith("!"))
							{
								if (preText != string.Empty)
								{
									yield return new TokenMatch(preLastIndex, preText, null, preText.Length, true);
								}

								if (tokenContent.Equals("!"))
								{
									context.CommentIntend++;
									while (context.CommentIntend > 0)
									{
										var nextCommentIndex =
											templateString.IndexOf("{{!}}", index);
										var nextCommentCloseIndex =
											templateString.IndexOf("{{/!}}", index);
										if (nextCommentCloseIndex == -1 && nextCommentIndex == -1)
										{
											yield break;
										}

										if (nextCommentIndex < nextCommentCloseIndex && nextCommentIndex == -1)
										{
											context.CommentIntend++;
											index = nextCommentIndex + "{{!}}".Length - 1;
										}
										else
										{
											context.CommentIntend--;
											index = nextCommentCloseIndex + "{{/!}}".Length - 1;
										}
									}
								}
								else if (tokenContent.Equals("!?"))
								{
									var nextCommentCloseIndex =
										templateString.IndexOf("{{/!?}}", index);

									if (nextCommentCloseIndex == -1)
									{
										preText = templateString.Substring(index + 1);
										yield return new TokenMatch(preLastIndex, preText, null, preText.Length, true);
										yield break;
									}
									preText = templateString.Substring(index + 1, nextCommentCloseIndex - index - 1);
									yield return new TokenMatch(preLastIndex, preText, null, preText.Length, true);
									index = nextCommentCloseIndex + "{{/!?}}".Length - 1;
								}
								else if (tokenContent.StartsWith("!="))
								{
									preText = new string(context._prefixToken)
										 + tokenContent.Substring("!=".Length)
										+ new string(context.SuffixToken);
									yield return new TokenMatch(preLastIndex, preText, null, preText.Length, true);
								}
								//intentionally do nothing to drop all tags with leading ! as they are considered comments
							}
							else
							{
								yield return new TokenMatch(startOfToken - context._prefixToken.Length,
									tokenContent,
									preText,
									tokenLength + context.SuffixToken.Length + context._prefixToken.Length,
									false);
							}
							break;
						}
						else if (Tokenizer.IsStringDelimiter(c) && context.CommentIntend == 0)
						{
							isInString = new InStringInfo(index, c);
						}
					}

					index++;
				}

				if (isInString.Index != -1)
				{
					context.Errors.Add(new MorestachioSyntaxError(
						context
							.Location(isInString.Index)
							.AddWindow(new CharacterSnippedLocation(0, 5, templateString.Substring(isInString.Index - 5, 10))), 
						"string",
						isInString.Delimiter.ToString(), 
						isInString.Delimiter.ToString(), 
						"Expected an closing string delimiter"));
				}

				preLastIndex = index + 1;
			}

			if (preLastIndex < templateString.Length())
			{
				var substring = templateString.Substring(preLastIndex);
				if (isInString.Index != -1)
				{
					context.Errors.Add(new MorestachioSyntaxError(
						context
							.Location(isInString.Index)
							.AddWindow(new CharacterSnippedLocation(0, 5, templateString.Substring(isInString.Index - 5, 10))), 
						"string",
						isInString.Delimiter.ToString(), 
						isInString.Delimiter.ToString(), 
						"Expected an closing string delimiter"));


					yield return new TokenMatch(preLastIndex, substring, null, substring.Length, false);
				}
				else
				{
					yield return new TokenMatch(preLastIndex, substring, null, substring.Length, true);
				}
			}
		}
	}
}
