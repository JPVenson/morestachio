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

//#if Span
//#if NET5_0_OR_GREATER
//	public ReadOnlySpan<char> Concat(ReadOnlySpan<char> s1, ReadOnlySpan<char> s2)
//	{
//		return string.Concat(s1, s2).AsSpan();
//	}
//	public ReadOnlySpan<char> Concat(ReadOnlySpan<char> s1, ReadOnlySpan<char> s2, ReadOnlySpan<char> s3)
//	{
//		return string.Concat(s1, s2, s3).AsSpan();
//	}
//#else
//	public ReadOnlySpan<T> Concat<T>(ReadOnlySpan<T> s1, ReadOnlySpan<T> s2)
//	{
//		var buffer = ArrayPool<T>.Shared.Rent(s1.Length + s2.Length);
//		try
//		{
//			s1.CopyTo(buffer);
//			s2.CopyTo(buffer.AsSpan(s1.Length));
//			return buffer;
//		}
//		finally
//		{
//			ArrayPool<T>.Shared.Return(buffer);
//		}
//	}
//	public ReadOnlySpan<T> Concat<T>(ReadOnlySpan<T> s1, ReadOnlySpan<T> s2, ReadOnlySpan<T> s3)
//	{
//		var buffer = ArrayPool<T>.Shared.Rent(s1.Length + s2.Length + s3.Length);
//		try
//		{
//			s1.CopyTo(buffer);
//			s2.CopyTo(buffer.AsSpan(s1.Length));
//			s3.CopyTo(buffer.AsSpan(s1.Length + s2.Length));
//			return buffer;
//		}
//		finally
//		{
//			ArrayPool<T>.Shared.Return(buffer);
//		}
//	}
//#endif

//	/// <inheritdoc />
//	public virtual IEnumerable<TokenMatch> Matches(TokenzierContext context)
//	{
//		var tokens = new List<TokenMatch>();
//		var isInString = new InStringInfo(-1, ReadOnlySpan<char>.Empty);
//		var stringEscape = false;

//		var templateString = Template.Substring(0..).Span;
//		var index = 0;
//		var lastIndex = 0;

//		//use the index of method for fast lookup of the next token
//		var prefixToken = new ReadOnlySpan<char>(context._prefixToken);
//		var nl = new ReadOnlySpan<char>(new[] { '\n' });
//		var escapeChar = new ReadOnlySpan<char>(new[] { '\\' });
//		var length = templateString.Length;

//		while (((index = templateString[index..].IndexOf(prefixToken)) != -1))
//		{
//			index += lastIndex;
//			index += context._prefixToken.Length;
//			while (templateString[(index)..(index + 1)] == prefixToken) //skip all excess {
//			{
//				index++;
//			}

//			//var preText = templateString.Substring(preLastIndex, index - context._prefixToken.Length - preLastIndex);
//			var preText = templateString[lastIndex..(index - context._prefixToken.Length)];

//			var startOfToken = index;
//			var tokenCount = 0;

//			var nlsIdx = 0;
//			//iterrate all newlines for character humanization
//			while ((nlsIdx = preText[nlsIdx..].IndexOf(nl, StringComparison.InvariantCultureIgnoreCase)) != -1)
//			{
//				context.Lines.Add(nlsIdx + lastIndex);
//				nlsIdx += 1;
//			}
//			while (index < length)
//			{
//				var c = templateString[index..(index + 1)];
//				tokenCount++;

//				if (c == nl)
//				{
//					context.Lines.Add(index);
//				}

//				if (isInString.Index != -1 && context.CommentIntend == 0)
//				{
//					if (c == escapeChar)
//					{
//						stringEscape = true;
//					}
//					else if (stringEscape && c == isInString.Delimiter)
//					{
//						stringEscape = false;
//					}
//					else if (!stringEscape && c == isInString.Delimiter)
//					{
//						isInString = new InStringInfo(-1, ReadOnlySpan<char>.Empty);
//					}
//				}
//				else if (index > 2)
//				{
//					var lastChars = templateString[(index - 2)..(index)];

//					if (lastChars.Equals(context._prefixToken, StringComparison.OrdinalIgnoreCase)) //something like "content {{"
//					{
//						preText = Concat(preText, c);
//						tokenCount = 0;
//					}
//					else if (lastChars.Equals(context.SuffixToken, StringComparison.OrdinalIgnoreCase)) //something like "zzddata }}"
//					{
//						var tokenLength = tokenCount - context.SuffixToken.Length;

//						var tokenContent = templateString[startOfToken..(startOfToken + tokenLength)];
//						//it's a comment drop this on the floor, no need to even yield it.
//						if (tokenContent[0] == '!')
//						{
//							if (preText != string.Empty)
//							{
//								tokens.Add(new TokenMatch(lastIndex, preText.ToString(), null, preText.Length, true));
//							}

//							if (tokenContent == "!")
//							{
//								context.CommentIntend++;
//								while (context.CommentIntend > 0)
//								{
//									var nextCommentIndex =
//										templateString[index..].IndexOf("{{!}}");
//									var nextCommentCloseIndex =
//										templateString[index..].IndexOf("{{/!}}");
//									if (nextCommentCloseIndex == -1 && nextCommentIndex == -1)
//									{
//										return tokens;
//									}

//									if (nextCommentIndex < nextCommentCloseIndex && nextCommentIndex == -1)
//									{
//										context.CommentIntend++;
//										index = nextCommentIndex + "{{!}}".Length - 1;
//									}
//									else
//									{
//										context.CommentIntend--;
//										var commentCloseIndex = nextCommentCloseIndex;
//										if (context.TokenizeComments && context.CommentIntend == 0)
//										{
//											var comment = templateString[index..(commentCloseIndex)];
//											tokens.Add(new TokenMatch(index, comment.ToString(), null, comment.Length,
//												false));
//										}
//										index = commentCloseIndex + "{{/!}}".Length - 1;
//									}
//								}
//							}
//							else if (tokenContent.Equals("!?"))
//							{
//								var nextCommentCloseIndex =
//									templateString[index..].IndexOf("{{/!?}}");

//								if (nextCommentCloseIndex == -1)
//								{
//									preText = templateString[(index + 1)..];
//									tokens.Add(new TokenMatch(lastIndex, preText.ToString(), null, preText.Length, true));

//									return tokens;
//								}
//								preText = templateString[(index + 1)..(nextCommentCloseIndex)];
//								tokens.Add(new TokenMatch(lastIndex, preText.ToString(), null, preText.Length, true));
//								index = nextCommentCloseIndex + "{{/!?}}".Length - 1;
//							}
//							else if (tokenContent.StartsWith("!="))
//							{
//								preText = Concat(prefixToken,
//									tokenContent[2..],
//									context.SuffixToken);
//								tokens.Add(new TokenMatch(lastIndex, preText.ToString(), null, preText.Length, true));
//							}
//							else if (context.TokenizeComments)
//							{
//								tokens.Add(new TokenMatch(index, tokenContent.ToString(), null, tokenContent.Length,
//									false));
//							}
//							//intentionally do nothing to drop all tags with leading ! as they are considered comments
//						}
//						else
//						{
//							tokens.Add(new TokenMatch(startOfToken - context._prefixToken.Length,
//								tokenContent.ToString(),
//								preText.ToString(),
//								tokenLength + context.SuffixToken.Length + context._prefixToken.Length,
//								false));
//						}
//						break;
//					}
//					else if (Tokenizer.IsStringDelimiter(c[0]) && context.CommentIntend == 0)
//					{
//						isInString = new InStringInfo(index, c);
//					}
//				}

//				index++;
//			}

//			if (isInString.Index != -1)
//			{
//				context.Errors.Add(new MorestachioSyntaxError(
//					context
//						.Location(isInString.Index)
//						.AddWindow(new CharacterSnippedLocation(0, 5, templateString[(isInString.Index - 5)..10].ToString())),
//					"string",
//					isInString.Delimiter.ToString(),
//					isInString.Delimiter.ToString(),
//					"Expected an closing string delimiter"));
//			}

//			lastIndex = index + 1;
//		}

//		if (lastIndex < length)
//		{
//			var substring = templateString[lastIndex..];
//			if (isInString.Index != -1)
//			{
//				context.Errors.Add(new MorestachioSyntaxError(
//					context
//						.Location(isInString.Index)
//						.AddWindow(new CharacterSnippedLocation(0, 5, templateString[(isInString.Index - 5)..10].ToString())),
//					"string",
//					isInString.Delimiter.ToString(),
//					isInString.Delimiter.ToString(),
//					"Expected an closing string delimiter"));
//				tokens.Add(new TokenMatch(lastIndex, substring.ToString(), null, substring.Length, false));
//			}
//			else
//			{
//				tokens.Add(new TokenMatch(lastIndex, substring.ToString(), null, substring.Length, true));
//			}
//		}

//		return tokens;
//	}
//#else
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
			index += context._prefixToken.Length;
			while (templateString[index] == context._prefixToken[0]) //skip all excess {
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
					if (lastChars.EndsWith(context._prefixToken)) //something like "content {{"
					{
						preText = preText ?? string.Empty;
						preText += c;
						tokenCount = 0;
					}
					else if (lastChars.EndsWith(context.SuffixToken))//something like "zzddata }}"
					{
						var tokenLength = tokenCount - context.SuffixToken.Length;

						var tokenContent = templateString.Substring(startOfToken, tokenLength);
						//it's a comment drop this on the floor, no need to even yield it.
						if (tokenContent[0] == '!')
						{
							if (preText != string.Empty)
							{
								yield return new TokenMatch(preLastIndex, preText, null, preText.Length, true);
							}

							if (tokenContent.IsEquals('!'))
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
										var commentCloseIndex = nextCommentCloseIndex;
										if (context.TokenizeComments && context.CommentIntend == 0)
										{
											var comment = templateString.Substring(index, commentCloseIndex - index);
											yield return new TokenMatch(index, comment, null, comment.Length,
												false);
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
									yield return new TokenMatch(preLastIndex, preText, null, preText.Length, true);
									yield break;
								}
								preText = templateString.Substring(index + 1, nextCommentCloseIndex - index - 1);
								yield return new TokenMatch(preLastIndex, preText, null, preText.Length, true);
								index = nextCommentCloseIndex + "{{/!?}}".Length - 1;
							}
							else if (tokenContent.StartsWith("!="))
							{
								preText = prefixToken
									+ tokenContent.Substring("!=".Length)
									+ new string(context.SuffixToken);
								yield return new TokenMatch(preLastIndex, preText, null, preText.Length, true);
							}
							else if (context.TokenizeComments)
							{
								yield return new TokenMatch(index, tokenContent, null, tokenContent.Length,
									false);
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
//#endif
}