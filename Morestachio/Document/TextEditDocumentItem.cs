using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Helper;
#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using Promise = System.Threading.Tasks.ValueTask;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using Promise = System.Threading.Tasks.Task;
#endif

namespace Morestachio.Document
{
	public enum TextOperationTypes
	{
		LineBreak,
		TrimLineBreaks
	}

	[Serializable]
	public class TextOperation : IXmlSerializable
	{
		public TextOperation()
		{

		}

		protected TextOperation(SerializationInfo info, StreamingContext c)
		{
			TextOperationType = (TextOperationTypes)info.GetValue(nameof(TextOperationType), typeof(TextOperationTypes));
			switch (TextOperationType)
			{
				case TextOperationTypes.TrimLineBreaks:
					LineBreaks = info.GetInt32(nameof(LineBreaks));
					TransientEdit = true;
					IsModificator = true;
					break;
				case TextOperationTypes.LineBreak:
					TransientEdit = true;
					IsModificator = false;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public TextOperationTypes TextOperationType { get; private set; }
		public int LineBreaks { get; private set; }

		public bool TransientEdit { get; private set; }
		public bool IsModificator { get; set; }

		public static TextOperation LineBreakOperation()
		{
			return new TextOperation()
			{
				TextOperationType = TextOperationTypes.LineBreak,
				TransientEdit = true,
				IsModificator = false
			};
		}

		public static TextOperation TrimLineBreakOperation(int number)
		{
			return new TextOperation()
			{
				TextOperationType = TextOperationTypes.TrimLineBreaks,
				LineBreaks = number,
				TransientEdit = true,
				IsModificator = true
			};
		}

		public XmlSchema GetSchema()
		{
			throw new NotImplementedException();
		}

		public void ReadXml(XmlReader reader)
		{
			TextOperationType = (TextOperationTypes)Enum.Parse(typeof(TextOperationTypes), reader.GetAttribute(nameof(TextOperationType)));
			switch (TextOperationType)
			{
				case TextOperationTypes.TrimLineBreaks:
					{
						LineBreaks = int.Parse(reader.GetAttribute(nameof(LineBreaks)));
						TransientEdit = true;
						IsModificator = true;
						break;
					}
				case TextOperationTypes.LineBreak:
					{
						TransientEdit = true;
						IsModificator = false;
						break;
					}

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void WriteXml(XmlWriter writer)
		{
			writer.WriteAttributeString(nameof(TextOperationType), TextOperationType.ToString());
			if (TextOperationType == TextOperationTypes.TrimLineBreaks)
			{
				writer.WriteAttributeString(nameof(LineBreaks), LineBreaks.ToString());
			}
		}

		public string Apply(string value)
		{
			switch (TextOperationType)
			{
				case TextOperationTypes.LineBreak:
					{
						return value + Environment.NewLine;
					}
				case TextOperationTypes.TrimLineBreaks:
					{
						var breaksFound = 0;
						for (int i = 0; i < value.Length; i++)
						{
							var c = value[i];
							if (c == '\r' || c == '\n')
							{
								if (LineBreaks != -1 && LineBreaks == ++breaksFound)
								{
									if (value.Length + 1 >= i)
									{
										return value.Substring(i + 1);
									}

									return string.Empty;
								}
							}
							else
							{
								return value.Substring(i);
							}
						}
						return string.Empty;
					}
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}

	[Serializable]
	public class TextEditDocumentItem : DocumentItemBase
	{
		public TextOperation Operation { get; private set; }

		public TextEditDocumentItem(TextOperation operation)
		{
			Operation = operation;
		}

		private TextEditDocumentItem()
		{

		}

		/// <inheritdoc />
		[UsedImplicitly]
		protected TextEditDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
		}

		public override ItemExecutionPromise Render(
			IByteCounterStream outputStream,
			ContextObject context,
			ScopeData scopeData)
		{
			if (Operation.IsModificator)
			{
				if (!scopeData.CustomData.TryGetValue("TextOperationData", out var operationList))
				{
					operationList = new List<TextOperation>();
					scopeData.CustomData["TextOperationData"] = operationList;
				}
				(operationList as IList<TextOperation>).Add(Operation);
			}
			else
			{
				outputStream.Write(Operation.Apply(string.Empty));
			}

			
			return Enumerable.Empty<DocumentItemExecution>().ToPromise();
		}

		protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
		{
			base.SerializeBinaryCore(info, context);
			info.AddValue(nameof(Operation), Operation);
		}

		protected override void SerializeXml(XmlWriter writer)
		{
			base.SerializeXml(writer);
			Operation.WriteXml(writer);
		}

		protected override void DeSerializeXml(XmlReader reader)
		{
			base.DeSerializeXml(reader);
			Operation = new TextOperation();
			Operation.ReadXml(reader);
		}

		public override string Kind { get; } = "TextOperation";
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
