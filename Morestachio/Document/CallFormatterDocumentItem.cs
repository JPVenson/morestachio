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
	public class CallFormatterDocumentItem : ValueDocumentItemBase, IValueDocumentItem, IEquatable<CallFormatterDocumentItem>
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

		/// <inheritdoc />
		public CallFormatterDocumentItem(Tuple<Tokenizer.HeaderTokenMatch, IValueDocumentItem>[] formatString,
			string value,
			[CanBeNull]string targetFormatterName)
		{
			FormatString = formatString;
			TargetFormatterName = targetFormatterName;
			Value = value;
		}

		/// <inheritdoc />
		[UsedImplicitly]
		protected CallFormatterDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
			FormatString = info.GetValue(nameof(FormatString),
					typeof(Tuple<Tokenizer.HeaderTokenMatch, IValueDocumentItem>[]))
				as Tuple<Tokenizer.HeaderTokenMatch, IValueDocumentItem>[];
			TargetFormatterName = info.GetString(nameof(TargetFormatterName));
		}

		/// <inheritdoc />
		protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
		{
			base.SerializeBinaryCore(info, context);
			info.AddValue(nameof(FormatString), FormatString, FormatString.GetType());
			info.AddValue(nameof(TargetFormatterName), TargetFormatterName);
		}

		/// <inheritdoc />
		protected override void DeSerializeXml(XmlReader reader)
		{
			base.DeSerializeXml(reader);
			if (reader.NodeType == XmlNodeType.EndElement && reader.Name.Equals(nameof(Value)))
			{
				reader.ReadEndElement();//end of value
			}
			if (reader.NodeType == XmlNodeType.EndElement && reader.Name.Equals(GetType().Name))
			{
				FormatString = new Tuple<Tokenizer.HeaderTokenMatch, IValueDocumentItem>[0];
				return;
			}

			AssertElement(reader, nameof(TargetFormatterName));
			if (!reader.IsEmptyElement)
			{
				reader.ReadStartElement();//start of TargetFormatterName
				TargetFormatterName = reader.ReadString();
				reader.ReadEndElement();//end of TargetFormatterName
			}
			else
			{
				TargetFormatterName = null;
				reader.ReadStartElement();//end of TargetFormatterName
			}

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

		/// <inheritdoc />
		protected override void SerializeXml(XmlWriter writer)
		{
			base.SerializeXml(writer);
			writer.WriteElementString(nameof(TargetFormatterName), TargetFormatterName);
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

		/// <summary>
		///		The name of the Formatter to be invoked
		/// </summary>
		public string TargetFormatterName { get; private set; }

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

		/// <inheritdoc />
		public async Task<ContextObject> GetValue(ContextObject context, ScopeData scopeData)
		{
			var c = await context.GetContextForPath(Value, scopeData);
			if (FormatString != null && FormatString.Any())
			{
				var argList = new List<KeyValuePair<string, object>>();

				foreach (var formatterArgument in FormatString)
				{
					var value = context.FindNextNaturalContextObject().CloneForEdit();
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
				context.Value = c.Format(TargetFormatterName, argList.ToArray());
			}
			else
			{
				context.Value = c.Format(TargetFormatterName, new KeyValuePair<string, object>[0]);
			}
			return context;
		}

		public bool Equals(CallFormatterDocumentItem other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return base.Equals(other) &&
				   TargetFormatterName == other.TargetFormatterName &&
				   FormatString.Length == other.FormatString.Length &&
				   FormatString.Select((item, index) => Tuple.Create(item, other.FormatString[index]))
					   .All(e => e.Item1.Item1.Equals(e.Item1.Item1) && e.Item1.Item2.Equals(e.Item2.Item2));
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj.GetType() != this.GetType())
			{
				return false;
			}

			return Equals((CallFormatterDocumentItem)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = base.GetHashCode();
				hashCode = (hashCode * 397) ^ (Kind != null ? Kind.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (FormatString.Any() ? FormatString.Select(f => f.Item1.GetHashCode() ^ f.Item2.GetHashCode()).Aggregate((e, f) => e ^ f) : 0);
				hashCode = (hashCode * 397) ^ (TargetFormatterName != null ? TargetFormatterName.GetHashCode() : 0);
				return hashCode;
			}
		}
	}
}