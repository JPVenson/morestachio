using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Morestachio.Formatter;
using Morestachio.Framework;

namespace Morestachio
{
	/// <summary>
	///		Calls a formatter on the current context value
	/// </summary>
	public class CallFormatterDocumentItem : DocumentItemBase, IValueDocumentItem
	{
		/// <inheritdoc />
		public CallFormatterDocumentItem(List<Tuple<string, Stack<IValueDocumentItem>>> formatString, string value)
		{
			FormatString = formatString;
			Value = value;
		}

		/// <inheritdoc />
		public override string Kind { get; } = "CallFormatter";

		/// <summary>
		///		Gets the parsed list of arguments for <see cref="Value"/>
		/// </summary>
		public List<Tuple<string, Stack<IValueDocumentItem>>> FormatString { get; }

		/// <summary>
		///		The expression that defines the Value that should be formatted
		/// </summary>
		public string Value { get; }

		/// <inheritdoc />
		public override async Task<IEnumerable<DocumentItemExecution>> Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			if (context == null)
			{
				return new DocumentItemExecution[0];
			}

			context = await GetValue(context, scopeData);
			return Children.WithScope(context);
		}

		public async Task<ContextObject> GetValue(ContextObject context, ScopeData scopeData)
		{
			await Task.CompletedTask;
			var c = await context.GetContextForPath(Value, scopeData);

			if (FormatString != null && FormatString.Any())
			{
				var argList = new List<KeyValuePair<string, object>>();

				foreach (var formatterArgument in FormatString)
				{
					var value = context.Clone();
					foreach (var valueDocumentItem in formatterArgument.Item2)
					{
						if (value == null)
						{
							break;
						}
						value = await valueDocumentItem.GetValue(value, scopeData);
					}

					if (value == null)
					{
						argList.Add(new KeyValuePair<string, object>(formatterArgument.Item1, null));
					}
					else
					{
						await value.EnsureValue();
						argList.Add(new KeyValuePair<string, object>(formatterArgument.Item1, value.Value));
					}
					

					////if pre and suffixed by a $ its a reference to another field.
					////walk the path in the $ and use the value in the formatter
					//var trimmedArg = formatterArgument.Argument;
					//if (trimmedArg != null && trimmedArg.StartsWith("$") &&
					//    trimmedArg.EndsWith("$"))
					//{
					//	var formatContext = await context.GetContextForPath(trimmedArg.Trim('$'), scopeData);
					//	await formatContext.EnsureValue();
					//	argList.Add(new KeyValuePair<string, object>(formatterArgument.Name, formatContext.Value));
					//}
					//else
					//{
					//}
				}
				//we do NOT await the task here. We await the task only if we need the value
				context.Value = c.Format(argList.ToArray());
			}
			else
			{
				context.Value = c.Format(new KeyValuePair<string, object>[0]);
			}

			return context;
		}
	}
}