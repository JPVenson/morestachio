using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Tokenizing;

namespace Morestachio.TemplateContainers
{
	/// <summary>
	///		Defines a storage used to obtain token matches from a string or other source. Use the <see cref="TemplateContainerBase"/> or <see cref="LazyTemplateContainerBase"/> as base class for your own implementation
	/// </summary>
	public interface ITemplateContainer
	{
		/// <summary>
		///		Gets Tokens from the store
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		IEnumerable<TokenMatch> Matches(TokenzierContext context);
	}

	/// <summary>
	///		Compares two chars based only on its numeric value 
	/// </summary>
	public class CharComparer : IEqualityComparer<char>
	{
		/// <inheritdoc />
		public bool Equals(char x, char y)
		{
			return ((int)x) == ((int)y);
		}

		/// <inheritdoc />
		public int GetHashCode(char obj)
		{
			return obj.GetHashCode();
		}
	}

	/// <summary>
	///		Defines a string based template container
	/// </summary>
	public class StringTemplateContainer : TemplateContainerBase
	{
		/// <inheritdoc />
		public StringTemplateContainer(string template) : base(template)
		{
		}

		public static implicit operator StringTemplateContainer(string template)
		{
			return new StringTemplateContainer(template);
		}
	}

	/// <summary>
	///		Defines a lazy enumerated container where only the parts of a template is fetched that are imminently needed.
	/// <remarks>This container trades execution performance for memory usage</remarks>
	/// </summary>
	public abstract class LazyTemplateContainerBase : ITemplateContainer, IEnumerable<char>
	{
		/// <summary>
		///		Is used to calculate an estimated buffer for static content
		/// </summary>
		/// <returns></returns>
		protected abstract int? EstimateLength();

		/// <inheritdoc />
		public virtual IEnumerable<TokenMatch> Matches(TokenzierContext context)
		{
			var elementIndex = 0;
			char? isInString = null;
			var stringEscape = false;

			var tokenCount = 0;
			var inToken = false;

			var estTokenLength = EstimateLength() ?? 40_000;
			estTokenLength = estTokenLength < 1000 ? estTokenLength : estTokenLength / 3;

			var tokenBuffer = new char[estTokenLength];
			string preText = null;
			var charComparer = new CharComparer();
			var enumerator = GetEnumerator();
			var lastChars = new MorestachioDefaultRollingArray();

			while (enumerator.MoveNext())
			{
				var c = enumerator.Current;
				lastChars.Add(c);
				if (tokenBuffer.Length <= tokenCount)//basic expandable array
				{
					Array.Resize(ref tokenBuffer, tokenBuffer.Length + estTokenLength);
				}

				// all chars are buffered 
				tokenBuffer[tokenCount] = c;
				tokenCount++;

				if (c == '\n')
				{
					context.Lines.Add(elementIndex);
				}

				if (isInString.HasValue && context.CommentIntend == 0)
				{
					if (c == '\\')
					{
						stringEscape = true;
					}
					else if (stringEscape && c == isInString.Value)
					{
						stringEscape = false;
					}
					else if (!stringEscape && c == isInString.Value)
					{
						isInString = null;
					}
				}
				else if (!inToken)
				{
					if (lastChars.EndsWith(context._prefixToken, charComparer))//something like "content {{"
					{
						if (tokenCount - context._prefixToken.Length > 0)
						{
							preText = new string(tokenBuffer, 0, tokenCount - context._prefixToken.Length);
						}
						tokenCount = 0;
						inToken = true;
					}
				}
				else
				{
					if (lastChars.EndsWith(context._prefixToken))//something like "content {{{{"
					{
						preText = preText ?? string.Empty;
						preText += c;
						tokenCount = 0;
					}
					else if (lastChars.EndsWith(context.SuffixToken, charComparer))//something like "zzddata }}"
					{
						var tokenLength = tokenCount - context.SuffixToken.Length;
						yield return new TokenMatch(elementIndex - tokenLength - 2 - 1,
							new string(tokenBuffer, 0, tokenLength),
							preText,
							tokenLength + context.SuffixToken.Length + context._prefixToken.Length,
							false);
						tokenCount = 0;
						preText = null;
						inToken = false;
					}
					else if (Tokenizer.IsStringDelimiter(c) && context.CommentIntend == 0)
					{
						isInString = c;
					}
				}

				elementIndex++;
			}

			enumerator.Dispose();
			if (isInString.HasValue && tokenCount != 0)
			{
				//var token = template.Substring(elementIndex, template.Length - elementIndex);
				yield return new TokenMatch(elementIndex, new string(tokenBuffer, 0, tokenCount), null, tokenCount, false);
				tokenCount = 0;
			}

			if (tokenCount > 0)
			{
				yield return new TokenMatch(elementIndex, new string(tokenBuffer, 0, tokenCount), null, tokenCount, true);
			}
		}

		/// <inheritdoc />
		public abstract IEnumerator<char> GetEnumerator();
		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	/// <summary>
	///		Defines the Default container for an string template and and tokenize method
	/// </summary>
	public abstract class TemplateContainerBase : ITemplateContainer
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="template"></param>
		public TemplateContainerBase(string template)
		{
			Template = template;
		}

		/// <summary>
		///		The string template
		/// </summary>
		public string Template { get; }

		/// <inheritdoc />
		public virtual IEnumerable<TokenMatch> Matches(TokenzierContext context)
		{
			char? isInString = null;
			var stringEscape = false;

			var templateString = Template;
			var index = 0;
			var preLastIndex = 0;
			var charComparer = new CharComparer();
			var lastChars = new MorestachioDefaultRollingArray();

			//use the index of method for fast lookup of the next token
			while ((index = templateString.IndexOf(new string(context._prefixToken), index, StringComparison.Ordinal)) != -1)
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
				while ((nlsIdx = preText.IndexOf('\n', nlsIdx)) != -1)
				{
					context.Lines.Add(nlsIdx + preLastIndex);
					nlsIdx += 1;
				}
				while (index < templateString.Length)
				{
					var c = templateString[index];
					lastChars.Add(c);
					tokenCount++;

					if (c == '\n')
					{
						context.Lines.Add(index);
					}

					if (isInString.HasValue && context.CommentIntend == 0)
					{
						if (c == '\\')
						{
							stringEscape = true;
						}
						else if (stringEscape && c == isInString.Value)
						{
							stringEscape = false;
						}
						else if (!stringEscape && c == isInString.Value)
						{
							isInString = null;
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

							yield return new TokenMatch(startOfToken - context._prefixToken.Length,
								templateString.Substring(startOfToken, tokenLength),
								preText,
								tokenLength + context.SuffixToken.Length + context._prefixToken.Length,
								false);
							break;
						}
						else if (Tokenizer.IsStringDelimiter(c) && context.CommentIntend == 0)
						{
							isInString = c;
						}
					}

					index++;
				}

				preLastIndex = index + 1;
			}

			if (preLastIndex < templateString.Length)
			{
				var substring = templateString.Substring(preLastIndex);
				if (isInString.HasValue)
				{
					//var token = template.Substring(elementIndex, template.Length - elementIndex);
					yield return new TokenMatch(preLastIndex, substring, null, substring.Length, false);
				}
				else
				{
					yield return new TokenMatch(preLastIndex, substring, null, substring.Length, true);
				}
			}
		}
	}

	/// <summary>
	///		Defines a Tokenizer Match
	/// </summary>
	public readonly struct TokenMatch
	{
		/// <summary>
		///		Creates a new Match
		/// </summary>
		/// <param name="index"></param>
		/// <param name="value"></param>
		/// <param name="preText"></param>
		/// <param name="length"></param>
		/// <param name="contentToken"></param>
		public TokenMatch(int index, string value, string preText, int length, bool contentToken)
		{
			Index = index;
			Value = value;
			PreText = preText;
			ContentToken = contentToken;
			Length = length;
		}

		/// <summary>
		///		The index within the template where this token occurs
		/// </summary>
		public int Index { get; }
		/// <summary>
		///		The length of the Value. Should differ as value omits the <see cref="TokenzierContext.PrefixToken"/> and <see cref="TokenzierContext.SuffixToken"/>
		/// </summary>
		public int Length { get; }

		/// <summary>
		///		The Tokens value excluding <see cref="TokenzierContext.PrefixToken"/> and <see cref="TokenzierContext.SuffixToken"/>
		/// </summary>
		public string Value { get; }

		/// <summary>
		///		If present, any preciding text
		/// </summary>
		public string PreText { get; }

		/// <summary>
		///		<value>true</value> if this is a dedicated content token
		/// </summary>
		public bool ContentToken { get; }
	}
}
