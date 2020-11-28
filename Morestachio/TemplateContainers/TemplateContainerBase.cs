﻿using System;
using System.Collections.Generic;
using System.Text;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Tokenizing;

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
}