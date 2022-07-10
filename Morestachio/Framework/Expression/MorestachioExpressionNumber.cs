using System.Diagnostics;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using Morestachio.Document;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression.Visitors;
using Morestachio.Helper.Serialization;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Framework.Expression;

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
	public MorestachioExpressionNumber(in Number number, TextRange location)
	{
		Number = number;
		Location = location;
	}

	/// <summary>
	///		The number of the Expression
	/// </summary>
	public Number Number { get; private set; }

	/// <inheritdoc />
	public TextRange Location { get; private set; }

	/// <summary>
	/// 
	/// </summary>
	protected MorestachioExpressionNumber(SerializationInfo info, StreamingContext context)
	{
		Number.TryParse(info.GetValue(nameof(Number), typeof(string)).ToString(), CultureInfo.CurrentCulture,
			out var nr);
		Number = nr;
		Location = TextRangeSerializationHelper.ReadTextRange(nameof(Location), info, context);
	}

	/// <inheritdoc />
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue(nameof(Number), Number.AsParsableString());
		TextRangeSerializationHelper.WriteTextRangeToBinary(nameof(Location), info, context, Location);
	}

	/// <inheritdoc />
	public XmlSchema GetSchema()
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	public void ReadXml(XmlReader reader)
	{
		Location = TextRangeSerializationHelper.ReadTextRangeFromXml(reader, "Location");
		Number.TryParse(reader.GetAttribute(nameof(Number)), CultureInfo.CurrentCulture, out var nr);
		Number = nr;
	}

	/// <inheritdoc />
	public void WriteXml(XmlWriter writer)
	{
		TextRangeSerializationHelper.WriteTextRangeToXml(writer, Location, "Location");
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

	/// <param name="parserOptions"></param>
	/// <inheritdoc />
	public CompiledExpression Compile(ParserOptions parserOptions)
	{
		var nrContext = new ContextObject(".", null, Number);
		return (contextObject, scopeData) =>
		{
			return nrContext.ToPromise();
			//var nrContext = scopeData.ParserOptions.CreateContextObject(".",
			//	Number,
			//	contextObject);
			//return nrContext.ToPromise();
		};
	}

	/// <inheritdoc />
	public void Accept(IMorestachioExpressionVisitor visitor)
	{
		visitor.Visit(this);
	}

	/// <inheritdoc />
	public bool IsCompileTimeEval()
	{
		return true;
	}

	/// <inheritdoc />
	public object GetCompileTimeValue()
	{
		return Number;
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
			get
			{
				return _exp.AsStringExpression();
			}
		}

		public string DbgView
		{
			get
			{
				return _exp.AsDebugExpression();
			}
		}

		public TextRange Location
		{
			get { return _exp.Location; }
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