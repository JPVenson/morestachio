using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using JetBrains.Annotations;
using Morestachio.Framework;

namespace Morestachio.Document
{
	/// <summary>
	///		Calls a formatter on the current context value
	/// </summary>
	public class CallFormatterDocumentItem : ValueDocumentItemBase, IValueDocumentItem
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal CallFormatterDocumentItem()
		{
			
		}

		/// <inheritdoc />
		public CallFormatterDocumentItem(Tuple<Tokenizer.HeaderTokenMatch, IValueDocumentItem>[] formatString, string value)
		{
			FormatString = formatString;
			Value = value;
		}

		[UsedImplicitly]
		protected CallFormatterDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
			FormatString = info.GetValue(nameof(FormatString),
					typeof(Tuple<Tokenizer.HeaderTokenMatch, IValueDocumentItem>[]))
				as Tuple<Tokenizer.HeaderTokenMatch, IValueDocumentItem>[];
		}

		protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
		{
			base.SerializeBinaryCore(info, context);
			info.AddValue(nameof(FormatString), FormatString, FormatString.GetType());
		}

		protected override void DeSerializeXml(XmlReader reader)
		{
			base.DeSerializeXml(reader);
			if (reader.NodeType == XmlNodeType.EndElement)
			{
				return;
			}
			AssertElement(reader, nameof(FormatString));
			var formatString = new List<Tuple<Tokenizer.HeaderTokenMatch, IValueDocumentItem>>();
			while (reader.NodeType != XmlNodeType.EndElement || !reader.Name.Equals(nameof(FormatString)))
			{
				var formatStr = new Tokenizer.HeaderTokenMatch();
				formatStr.ArgumentName = reader.GetAttribute("Name");
				reader.ReadStartElement();

				var type = Type.GetType(GetType().Namespace + "." + reader.Name)
				           ?? throw new InvalidOperationException($"The specified type '{reader.Name}' does not exist");

				if (!(type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance,
						null, Type.EmptyTypes, null)
					?.Invoke(null) is IValueDocumentItem child))
				{
					throw new InvalidOperationException($"The specified type '{reader.Name}' does not exist");
				}
				var childTree = reader.ReadSubtree();
				childTree.Read();
				child.ReadXml(childTree);
				reader.Skip();
				formatString.Add(new Tuple<Tokenizer.HeaderTokenMatch, IValueDocumentItem>(formatStr, child));
			}
			reader.ReadEndElement();//nameof(FormatString)

			FormatString = formatString.ToArray();
		}

		protected override void SerializeXml(XmlWriter writer)
		{
			base.SerializeXml(writer);
			if (FormatString.Any())
			{
				writer.WriteStartElement(nameof(FormatString));
				foreach (var formatStr in FormatString)
				{
					writer.WriteAttributeString("Name", formatStr.Item1.ArgumentName);
					writer.WriteStartElement(formatStr.Item2.GetType().Name);
					formatStr.Item2.WriteXml(writer);
					writer.WriteEndElement();

				}
				writer.WriteEndElement(); //nameof(FormatString)
			}
		}

		/// <inheritdoc />
		public override string Kind { get; } = "CallFormatter";

		/// <summary>
		///		Gets the parsed list of arguments for <see cref="Value"/>
		/// </summary>
		public Tuple<Tokenizer.HeaderTokenMatch, IValueDocumentItem>[] FormatString { get; private set; }

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
					var value = context.FindNextNaturalContextObject().Clone();
					value = await formatterArgument.Item2.GetValue(value, scopeData);

					if (value == null)
					{
						argList.Add(new KeyValuePair<string, object>(formatterArgument.Item1.ArgumentName, null));
					}
					else
					{
						await value.EnsureValue();
						argList.Add(new KeyValuePair<string, object>(formatterArgument.Item1.ArgumentName, value.Value));
					}
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