using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Expression.Visitors;
using Morestachio.Helper;
using Morestachio.ParserErrors;

namespace Morestachio.Framework.Expression
{
	[Serializable]
	public class ExpressionNumber : IMorestachioExpression
	{
		public Number Number { get; private set; }

		internal ExpressionNumber()
		{
			
		}

		public ExpressionNumber(in Number number)
		{
			Number = number;
		}

		protected ExpressionNumber(SerializationInfo info, StreamingContext context)
		{
			Number.TryParse(info.GetValue(nameof(Number), typeof(string)).ToString(), CultureInfo.CurrentCulture,
				out var nr);
			Number = nr;
			Location = CharacterLocation.FromFormatString(info.GetString(nameof(Location)));
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(nameof(Number), Number.AsParsableString());
			info.AddValue(nameof(Location), Location.ToFormatString());
		}

		public XmlSchema GetSchema()
		{
			throw new NotImplementedException();
		}

		public void ReadXml(XmlReader reader)
		{
			Location = CharacterLocation.FromFormatString(reader.GetAttribute(nameof(Location)));
			Number.TryParse(reader.GetAttribute(nameof(Number)), CultureInfo.CurrentCulture, out var nr);
			Number = nr;
		}

		public void WriteXml(XmlWriter writer)
		{
			writer.WriteAttributeString(nameof(Location), Location.ToFormatString());
			writer.WriteAttributeString(nameof(Number), Number.AsParsableString());
		}

		/// <inheritdoc />
		public bool Equals(IMorestachioExpression other)
		{
			return Equals((object)other);
		}

		public bool Equals(ExpressionNumber other)
		{
			if (!Location.Equals(other.Location))
			{
				return false;
			}

			return Number.Equals(other.Number);
		}

		/// <inheritdoc />
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

			return Equals((ExpressionNumber)obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (Number != Number.NaN ? Number.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Location != null ? Location.GetHashCode() : 0);
				return hashCode;
			}
		}

		public CharacterLocation Location { get; set; }
		public async Task<ContextObject> GetValue(ContextObject contextObject, ScopeData scopeData)
		{
			return contextObject.Options.CreateContextObject(".", contextObject.CancellationToken, Number,
				contextObject);
		}

		public void Accept(IMorestachioExpressionVisitor visitor)
		{
			visitor.Visit(this);
		}

		public static ExpressionNumber ParseFrom(string text,
			int offset,
			TokenzierContext context,
			out int index)
		{
			var isFloatingNumber = false;
			var nrText = new StringBuilder();
			for (index = offset; index < text.Length; index++)
			{
				var c = text[index];
				if (c == '.')
				{
					if (isFloatingNumber)
					{
						index--;
						break;
					}

					if (index + 1 > text.Length)
					{
						context.Errors.Add(new MorestachioSyntaxError(
							context
								.CurrentLocation
								.AddWindow(new CharacterSnippedLocation(0, index, text)),
							"", text, "Could not parse the given number"));
					}

					if (!char.IsDigit(text[index + 1]))
					{
						break;
					}

					isFloatingNumber = true;
				}
				else if (Tokenizer.IsEndOfFormatterArgument(c))
				{
					index--;
					break;
				}

				nrText.Append(c);
			}

			text = nrText.ToString();
			if (Number.TryParse(text, context.Culture, out var nr))
			{
				return new ExpressionNumber(nr)
				{
					Location = context.CurrentLocation.Offset(offset)
				};
			}
			else
			{
				context.Errors.Add(new MorestachioSyntaxError(
					context
						.CurrentLocation
						.AddWindow(new CharacterSnippedLocation(0, index, text)),
					"", text, "Could not parse the given number"));

				return null;
			}
		}
	}
}
