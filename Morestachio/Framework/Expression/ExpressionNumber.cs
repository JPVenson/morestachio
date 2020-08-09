using System;
using System.Collections.Generic;
using System.Diagnostics;
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
#if ValueTask
using ContextObjectPromise = System.Threading.Tasks.ValueTask<Morestachio.Framework.ContextObject>;
#else
using ContextObjectPromise = System.Threading.Tasks.Task<Morestachio.Framework.ContextObject>;
#endif

namespace Morestachio.Framework.Expression
{
	[Serializable]
	[DebuggerTypeProxy(typeof(ExpressionDebuggerDisplay))]
	public class ExpressionNumber : IMorestachioExpression
	{
		internal ExpressionNumber()
		{
			
		}
		
		/// <summary>
		/// 
		/// </summary>
		public ExpressionNumber(in Number number, CharacterLocation location)
		{
			Number = number;
			Location = location;
		}
		
		/// <summary>
		///		The number of the Expression
		/// </summary>
		public Number Number { get; private set; }

		/// <inheritdoc />
		public CharacterLocation Location { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		protected ExpressionNumber(SerializationInfo info, StreamingContext context)
		{
			Number.TryParse(info.GetValue(nameof(Number), typeof(string)).ToString(), CultureInfo.CurrentCulture,
				out var nr);
			Number = nr;
			Location = CharacterLocation.FromFormatString(info.GetString(nameof(Location)));
		}
		
		/// <inheritdoc />
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(nameof(Number), Number.AsParsableString());
			info.AddValue(nameof(Location), Location.ToFormatString());
		}
		
		/// <inheritdoc />
		public XmlSchema GetSchema()
		{
			throw new NotImplementedException();
		}
		
		/// <inheritdoc />
		public void ReadXml(XmlReader reader)
		{
			Location = CharacterLocation.FromFormatString(reader.GetAttribute(nameof(Location)));
			Number.TryParse(reader.GetAttribute(nameof(Number)), CultureInfo.CurrentCulture, out var nr);
			Number = nr;
		}
		
		/// <inheritdoc />
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
		
		/// <inheritdoc />
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

		/// <inheritdoc />
		public ContextObjectPromise GetValue(ContextObject contextObject, ScopeData scopeData)
		{
			return contextObject.Options.CreateContextObject(".", contextObject.CancellationToken, Number,
				contextObject).ToPromise();
		}

		/// <inheritdoc />
		public void Accept(IMorestachioExpressionVisitor visitor)
		{
			visitor.Visit(this);
		}

		/// <summary>
		///		Parses the text and returns an Expression number
		/// </summary>
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
				if (c == CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator[0])
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
				else if (Tokenizer.IsEndOfFormatterArgument(c) || Tokenizer.IsWhiteSpaceDelimiter(c))
				{
					index--;
					break;
				}

				nrText.Append(c);
			}

			text = nrText.ToString();
			if (Number.TryParse(text, CultureInfo.InvariantCulture, out var nr))
			{
				return new ExpressionNumber(nr, context.CurrentLocation.Offset(offset));
			}

			context.Errors.Add(new MorestachioSyntaxError(
				context
					.CurrentLocation
					.AddWindow(new CharacterSnippedLocation(0, index, text)),
				"", text, "Could not parse the given number"));

			return null;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			var visitor = new DebuggerViewExpressionVisitor();
			Accept(visitor);
			return visitor.StringBuilder.ToString();
		}

		private class ExpressionDebuggerDisplay
		{
			private readonly ExpressionNumber _exp;

			public ExpressionDebuggerDisplay(ExpressionNumber exp)
			{
				_exp = exp;
			}

			public string Expression
			{
				get { return _exp.ToString(); }
			}

			public Number Number
			{
				get { return _exp.Number; }
			}
		}
	}
}
