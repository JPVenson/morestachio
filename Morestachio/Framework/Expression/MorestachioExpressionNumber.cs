using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Morestachio.Document;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Expression.Visitors;
using Morestachio.Framework.Tokenizing;
using Morestachio.Helper;
using Morestachio.Parsing.ParserErrors;
#if ValueTask
using ContextObjectPromise = System.Threading.Tasks.ValueTask<Morestachio.Framework.Context.ContextObject>;
#else
using ContextObjectPromise = System.Threading.Tasks.Task<Morestachio.Framework.Context.ContextObject>;
#endif

namespace Morestachio.Framework.Expression
{
	/// <summary>
	///		Defines a number within an expression
	/// </summary>
	[Serializable]
	[DebuggerTypeProxy(typeof(ExpressionDebuggerDisplay))]
	public class MorestachioExpressionNumber : IMorestachioExpression
	{
		internal MorestachioExpressionNumber()
		{
			
		}
		
		/// <summary>
		/// 
		/// </summary>
		public MorestachioExpressionNumber(in Number number, CharacterLocation location)
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
		protected MorestachioExpressionNumber(SerializationInfo info, StreamingContext context)
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
		public bool Equals(MorestachioExpressionNumber other)
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

			return Equals((MorestachioExpressionNumber)obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (Number != Number.NaN ? Number.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Location.GetHashCode());
				return hashCode;
			}
		}

		/// <inheritdoc />
		public ContextObjectPromise GetValue(ContextObject contextObject, ScopeData scopeData)
		{
			return scopeData.ParserOptions.CreateContextObject(".", Number,
				contextObject).ToPromise();
		}
		
		/// <inheritdoc />
		public CompiledExpression Compile()
		{
			return (contextObject, scopeData) =>
			{
				var nrContext = scopeData.ParserOptions.CreateContextObject(".",
					Number,
					contextObject);
				return nrContext.ToPromise();
			};
		}

		/// <inheritdoc />
		public void Accept(IMorestachioExpressionVisitor visitor)
		{
			visitor.Visit(this);
		}

		private class ExpressionDebuggerDisplay
		{
			private readonly MorestachioExpressionNumber _exp;

			public ExpressionDebuggerDisplay(MorestachioExpressionNumber exp)
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

			/// <inheritdoc />
			public override string ToString()
			{
				var visitor = new DebuggerViewExpressionVisitor();
				_exp.Accept(visitor);
				return visitor.StringBuilder.ToString();
			}
		}
	}
}
