using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Framework;

namespace Morestachio.Document
{
	/// <summary>
	///		Calls a formatter on the current context value
	/// </summary>
	[System.Serializable]
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
			reader.ReadEndElement();
			if (reader.NodeType == XmlNodeType.EndElement && reader.Name.Equals(GetType().Name))
			{
				FormatString = new Tuple<Tokenizer.HeaderTokenMatch, IValueDocumentItem>[0];
				return;
			}

			AssertElement(reader, nameof(FormatString));
			var formatString = new List<Tuple<Tokenizer.HeaderTokenMatch, IValueDocumentItem>>();
			reader.ReadStartElement(); //Argument
			while (reader.NodeType != XmlNodeType.EndElement || !reader.Name.Equals(nameof(FormatString)))
			{
				AssertElement(reader, "Argument");
				var formatStr = new Tokenizer.HeaderTokenMatch();
				formatStr.ArgumentName = reader.GetAttribute("Name");
				reader.ReadStartElement(); //Content

				var child = DocumentExtenstions.CreateDocumentValueItemInstance(reader.Name);
				var childTree = reader.ReadSubtree();
				childTree.Read();
				child.ReadXml(childTree);
				reader.Skip();
				reader.ReadEndElement();//Argument
				formatString.Add(new Tuple<Tokenizer.HeaderTokenMatch, IValueDocumentItem>(formatStr, child));
			}

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
					writer.WriteStartElement("Argument");
					if (!string.IsNullOrWhiteSpace(formatStr.Item1.ArgumentName))
					{
						writer.WriteAttributeString("Name", formatStr.Item1.ArgumentName);
					}
					writer.WriteStartElement(formatStr.Item2.GetType().Name);
					formatStr.Item2.WriteXml(writer);
					writer.WriteEndElement();//formatStr.Item2.GetType().Name
					writer.WriteEndElement();//Argument

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