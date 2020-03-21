using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Morestachio.Framework.Expression.Renderer;
using Morestachio.ParserErrors;

namespace Morestachio.Framework.Expression
{
	public class ExpressionString : IExpression
	{
		public ExpressionString()
		{
			StringParts = new List<ExpressionStringConstPart>();
		}

		
		public override string ToString()
		{
			return ExpressionRenderer.RenderExpression(this).ToString();
		}

		public IList<ExpressionStringConstPart> StringParts { get; set; }
		public CharacterLocation Location { get; set; }
		public char Delimiter { get; set; }

		public async Task<ContextObject> GetValue(ContextObject contextObject, ScopeData scopeData)
		{
			await Task.CompletedTask;
			return new ContextObject(contextObject.Options, ".", contextObject)
			{
				Value = string.Join("", StringParts.Select(f => f.PartText))
			};
		}

		public class ExpressionStringConstPart
		{
			public string PartText { get; set; }
			public CharacterLocation Location { get; set; }
		}

		public static ExpressionString ParseFrom(string text,
			int offset,
			TokenzierContext context,
			out int index)
		{
			var result = new ExpressionString()
			{
				Location = context.CurrentLocation
			};
			var expectStringDelimiter = false;
			var currentPart = new ExpressionStringConstPart()
			{
				Location = context.CurrentLocation
			};
			//get the string delimiter thats ether " or '
			result.Delimiter = text[offset];
			result.StringParts.Add(currentPart);
			//skip the string delimiter
			for (index = offset + 1; index < text.Length; index++)
			{
				var c = text[index];
				if (expectStringDelimiter)
				{
					currentPart.PartText += c;
					if (c == result.Delimiter)
					{
						expectStringDelimiter = false;
					}
				}
				else
				{
					if (c == '\\')
					{
						expectStringDelimiter = true;
					}
					else if (c == result.Delimiter)
					{
						if (offset == 0 && index + 1 != text.Length)
						{
							context.Errors.Add(new MorestachioSyntaxError(
								context
									.CurrentLocation
									.Offset(index)
									.AddWindow(new CharacterSnippedLocation(0, index, text)),
								"", c.ToString(), "did not expect " + result.Delimiter));
							break;
						}

						break;
					}
					else
					{
						currentPart.PartText += c;
					}
				}
			}
			context.AdvanceLocation(text.Length);
			return result;
		}
	}
}