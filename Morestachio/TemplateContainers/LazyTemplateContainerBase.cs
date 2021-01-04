//using System;
//using System.Collections;
//using System.Collections.Generic;
//using Morestachio.Framework.Expression.Framework;
//using Morestachio.Framework.Tokenizing;

//namespace Morestachio.TemplateContainers
//{
//	/// <summary>
//	///		Defines a lazy enumerated container where only the parts of a template is fetched that are imminently needed.
//	/// <remarks>This container trades execution performance for memory usage</remarks>
//	/// </summary>
//	public abstract class LazyTemplateContainerBase : ITemplateContainer, IEnumerable<char>
//	{
//		/// <summary>
//		///		Is used to calculate an estimated buffer for static content
//		/// </summary>
//		/// <returns></returns>
//		protected abstract int? EstimateLength();

//		/// <inheritdoc />
//		public virtual IEnumerable<TokenMatch> Matches(TokenzierContext context)
//		{
//			var elementIndex = 0;
//			char? isInString = null;
//			var stringEscape = false;

//			var tokenCount = 0;
//			var inToken = false;

//			var estTokenLength = EstimateLength() ?? 40_000;
//			estTokenLength = estTokenLength < 1000 ? estTokenLength : estTokenLength / 3;

//			var tokenBuffer = new char[estTokenLength];
//			string preText = null;
//			var charComparer = new CharComparer();
//			var enumerator = GetEnumerator();
//			var lastChars = new MorestachioDefaultRollingArray();

//			while (enumerator.MoveNext())
//			{
//				var c = enumerator.Current;
//				lastChars.Add(c);
//				if (tokenBuffer.Length <= tokenCount)//basic expandable array
//				{
//					Array.Resize(ref tokenBuffer, tokenBuffer.Length + estTokenLength);
//				}

//				// all chars are buffered 
//				tokenBuffer[tokenCount] = c;
//				tokenCount++;

//				if (c == '\n')
//				{
//					context.Lines.Add(elementIndex);
//				}

//				if (isInString.HasValue && context.CommentIntend == 0)
//				{
//					if (c == '\\')
//					{
//						stringEscape = true;
//					}
//					else if (stringEscape && c == isInString.Value)
//					{
//						stringEscape = false;
//					}
//					else if (!stringEscape && c == isInString.Value)
//					{
//						isInString = null;
//					}
//				}
//				else if (!inToken)
//				{
//					if (lastChars.EndsWith(context._prefixToken, charComparer))//something like "content {{"
//					{
//						if (tokenCount - context._prefixToken.Length > 0)
//						{
//							preText = new string(tokenBuffer, 0, tokenCount - context._prefixToken.Length);
//						}
//						tokenCount = 0;
//						inToken = true;
//					}
//				}
//				else
//				{
//					if (lastChars.EndsWith(context._prefixToken))//something like "content {{{{"
//					{
//						preText = preText ?? string.Empty;
//						preText += c;
//						tokenCount = 0;
//					}
//					else if (lastChars.EndsWith(context.SuffixToken, charComparer))//something like "zzddata }}"
//					{
//						var tokenLength = tokenCount - context.SuffixToken.Length;
//						var tokenContent = new string(tokenBuffer, 0, tokenLength);

//						yield return new TokenMatch(elementIndex - tokenLength - 2 - 1,
//							tokenContent,
//							preText,
//							tokenLength + context.SuffixToken.Length + context._prefixToken.Length,
//							false);
//						tokenCount = 0;
//						preText = null;
//						inToken = false;
//					}
//					else if (Tokenizer.IsStringDelimiter(c) && context.CommentIntend == 0)
//					{
//						isInString = c;
//					}
//				}

//				elementIndex++;
//			}

//			enumerator.Dispose();
//			if (isInString.HasValue && tokenCount != 0)
//			{
//				//var token = template.Substring(elementIndex, template.Length - elementIndex);
//				yield return new TokenMatch(elementIndex, new string(tokenBuffer, 0, tokenCount), null, tokenCount, false);
//				tokenCount = 0;
//			}

//			if (tokenCount > 0)
//			{
//				yield return new TokenMatch(elementIndex, new string(tokenBuffer, 0, tokenCount), null, tokenCount, true);
//			}
//		}

//		/// <inheritdoc />
//		public abstract IEnumerator<char> GetEnumerator();
//		/// <inheritdoc />
//		IEnumerator IEnumerable.GetEnumerator()
//		{
//			return GetEnumerator();
//		}
//	}
//}