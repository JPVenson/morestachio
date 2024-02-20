using System.Collections.Generic;
using System.Runtime.InteropServices;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Tokenizing;
using Morestachio.Parsing.ParserErrors;
using Morestachio.Util;

#if Span
using System.Buffers;
#endif

namespace Morestachio.TemplateContainers;

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
	///		The template
	/// </summary>
	public TemplateResource Template { get; }

	private readonly
		struct InStringInfo
	{
		public InStringInfo(int index, char delimiter)
		{
			Index = index;
			Delimiter = delimiter;
		}

		public readonly char Delimiter;
		public readonly int Index;
	}

	/// <inheritdoc />
	public virtual IEnumerable<TokenMatch> Matches(TokenzierContext context)
	{
		var isInString = new InStringInfo(-1, ' ');
		var stringEscape = false;

		var templateString = Template;
		var index = 0;
		var preLastIndex = 0;
		var lastChars = new MorestachioDefaultRollingArray();

		//use the index of method for fast lookup of the next token
		var prefixToken = new string(context._prefixToken);

		while ((index = templateString.IndexOf(prefixToken, index)) != -1)
		{
			var startOfToken = index;
			index += context._prefixToken.Length;

			while (templateString[index] == context._prefixToken[0]) //skip all excess {
			{
				index++;
			}

			var preText = templateString.Substring(preLastIndex, index - context._prefixToken.Length - preLastIndex);

			if (preText.Length > 0)
			{
				yield return TokenMatch
					.CreateContentToken(TextRange.Range(context, preLastIndex, preText.Length - 1), preText);

				var nlsIdx = 0;

				//iterrate all newlines for character humanization
				while ((nlsIdx = preText.IndexOf('\n', nlsIdx)) != -1)
				{
					context.Lines.Add(nlsIdx + preLastIndex);
					nlsIdx += 1;
				}

				preLastIndex += preText.Length;
				preText = string.Empty;
			}

			var startOfTokenContent = index;
			var tokenCount = 0;

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
					if (lastChars.EndsWith(context._prefixToken)) //something like "content {{"
					{
						preText = preText ?? string.Empty;
						preText += c;
						tokenCount = 0;
					}
					else if (lastChars.EndsWith(context.SuffixToken)) //something like "zzddata }}"
					{
						var tokenLength = tokenCount - context.SuffixToken.Length;

						var tokenContent = templateString.Substring(startOfTokenContent, tokenLength);

						//it's a comment drop this on the floor, no need to even yield it.
						if (tokenContent[0] == '!')
						{
							if (preText.Length > 0)
							{
								yield return TokenMatch
									.CreateContentToken(TextRange.RangeIndex(context, preLastIndex, startOfToken),
										preText);
							}

							if (tokenContent.IsEquals('!'))
							{
								context.CommentIntend++;

								if (context.TokenizeComments)
								{
									yield return TokenMatch.CreateExpressionToken(
										TextRange.Range(context, index, 4),
										"{{!}}");
								}

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
										var commentCloseIndex = nextCommentCloseIndex;

										if (context.TokenizeComments && context.CommentIntend == 0)
										{
											var comment = templateString.Substring(index, commentCloseIndex - index);
											yield return TokenMatch
												.CreateContentToken(
													TextRange.RangeIndex(context, startOfToken, commentCloseIndex),
													comment);
											yield return TokenMatch
												.CreateExpressionToken(TextRange.Range(context, commentCloseIndex, 6),
													"{{/!}}");
										}

										index = commentCloseIndex + "{{/!}}".Length - 1;
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

									yield return TokenMatch
										.CreateContentToken(TextRange.Range(context, preLastIndex, preText.Length - 1),
											preText);
									yield break;
								}

								preText = templateString.Substring(index + 1, nextCommentCloseIndex - index - 1);
								index = nextCommentCloseIndex + "{{/!?}}".Length - 1;

								yield return TokenMatch
									.CreateContentToken(TextRange.RangeIndex(context, preLastIndex, index), preText);
							}
							else if (tokenContent.StartsWith("!="))
							{
								preText = prefixToken
									+ tokenContent.Substring("!=".Length)
									+ new string(context.SuffixToken);

								yield return TokenMatch
									.CreateContentToken(TextRange.RangeIndex(context, preLastIndex, index), preText);
							}
							else if (context.TokenizeComments)
							{
								yield return TokenMatch
									.CreateExpressionToken(TextRange.RangeIndex(context, startOfToken, index),
										tokenContent);
							}
							//intentionally do nothing to drop all tags with leading ! as they are considered comments
						}
						else
						{
							yield return TokenMatch
								.CreateExpressionToken(TextRange.RangeIndex(context, startOfToken, index),
									tokenContent);
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
					TextRange.Range(context, isInString.Index, 10),
					"string",
					isInString.Delimiter.ToString(),
					isInString.Delimiter.ToString(),
					"Expected an closing string delimiter"));
			}

			preLastIndex = index + 1;
		}

		if (preLastIndex >= templateString.Length())
		{
			yield break;
		}

		var substring = templateString.Substring(preLastIndex);

		if (isInString.Index != -1)
		{
			context.Errors.Add(new MorestachioSyntaxError(
				TextRange.Range(context, isInString.Index, 10),
				"string",
				isInString.Delimiter.ToString(),
				isInString.Delimiter.ToString(),
				"Expected an closing string delimiter"));

			yield return TokenMatch
				.CreateExpressionToken(TextRange.Range(context, preLastIndex, substring.Length - 1), substring);
		}
		else
		{
			yield return TokenMatch
				.CreateContentToken(TextRange.Range(context, preLastIndex, substring.Length - 1), substring);
		}
	}
//#endif
}