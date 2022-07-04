using System.Diagnostics;
using System.Xml;
using System.Xml.Schema;
using Morestachio.Document;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression.Parser;
using Morestachio.Framework.Expression.Visitors;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Framework.Expression;

/// <summary>
///		Defines an in template declared function
/// </summary>
[DebuggerTypeProxy(typeof(ExpressionDebuggerDisplay))]
[Serializable]
public class MorestachioLambdaExpression : IMorestachioExpression
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="parameters"></param>
	/// <param name="location"></param>
	public MorestachioLambdaExpression(IMorestachioExpression parameters, TextRange location)
	{
		Parameters = parameters;
		Location = location;
	}

	/// <summary>
	///		Serialization constructor
	/// </summary>
	/// <param name="info"></param>
	/// <param name="context"></param>
	public MorestachioLambdaExpression(SerializationInfo info, StreamingContext context)
	{
		Location = TextRangeSerializationHelper.ReadTextRange(info.GetString(nameof(Location)), info, context);
		Expression = info.GetValue(nameof(Expression), typeof(IMorestachioExpression)) as IMorestachioExpression;
		Parameters = info.GetValue(nameof(Parameters), typeof(IMorestachioExpression)) as IMorestachioExpression;
	}

	/// <inheritdoc />
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		TextRangeSerializationHelper.WriteTextRangeToBinary(nameof(Location), info, context, Location);
		info.AddValue(nameof(Expression), Expression);
		info.AddValue(nameof(Parameters), Parameters);
	}

	/// <inheritdoc />
	public XmlSchema GetSchema()
	{
		return null;
	}

	/// <inheritdoc />
	public void ReadXml(XmlReader reader)
	{
		Location = TextRangeSerializationHelper.ReadTextRangeFromXml(reader, "Location");
		if (reader.IsEmptyElement)
		{
			return;
		}

		reader.ReadStartElement();
		Expression = reader.ParseExpressionFromKind();
		reader.ReadEndElement();
		reader.ReadStartElement();
		Parameters = reader.ParseExpressionFromKind();
		reader.ReadEndElement();
	}

	/// <inheritdoc />
	public void WriteXml(XmlWriter writer)
	{
		TextRangeSerializationHelper.WriteTextRangeToXml(writer, Location, "Location");
		writer.WriteStartElement(nameof(Expression));
		writer.WriteExpressionToXml(Expression);
		writer.WriteEndElement();

		writer.WriteStartElement(nameof(Parameters));
		writer.WriteExpressionToXml(Parameters);
		writer.WriteEndElement();
	}

	/// <inheritdoc />
	public TextRange Location { get; private set; }

	/// <summary>
	///		The Lambda expression
	/// </summary>
	public IMorestachioExpression Expression { get; set; }

	/// <summary>
	///		The expression arguments
	/// </summary>
	public IMorestachioExpression Parameters { get; set; }

	/// <inheritdoc />
	public ContextObjectPromise GetValue(ContextObject contextObject, ScopeData scopeData)
	{
		return scopeData.ParserOptions.CreateContextObject(".", new MorestachioTemplateExpression(this, contextObject, scopeData)).ToPromise();
	}

	/// <param name="parserOptions"></param>
	/// <inheritdoc />
	public CompiledExpression Compile(ParserOptions parserOptions)
	{
		return (contextObject, scopeData) => scopeData.ParserOptions.CreateContextObject(".", new MorestachioTemplateExpression(this, contextObject, scopeData)).ToPromise();
	}

	/// <inheritdoc />
	public void Accept(IMorestachioExpressionVisitor visitor)
	{
		visitor.Visit(this);
	}

	/// <inheritdoc />
	public bool IsCompileTimeEval()
	{
		return false;
	}

	/// <inheritdoc />
	public object GetCompileTimeValue()
	{
		return null;
	}

	private class ExpressionDebuggerDisplay
	{
		private readonly MorestachioLambdaExpression _exp;

		public ExpressionDebuggerDisplay(MorestachioLambdaExpression exp)
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

		/// <inheritdoc />
		public override string ToString()
		{
			var visitor = new DebuggerViewExpressionVisitor();
			_exp.Accept(visitor);
			return visitor.StringBuilder.ToString();
		}
	}

	/// <inheritdoc />
	public bool Equals(MorestachioLambdaExpression other)
	{
		if (ReferenceEquals(null, other))
		{
			return false;
		}

		if (ReferenceEquals(this, other))
		{
			return true;
		}

		return Location.Equals(other.Location) && Equals(Expression, other.Expression) && Equals(Parameters, other.Parameters);
	}


	/// <inheritdoc />
	public bool Equals(IMorestachioExpression other)
	{
		return Equals((object)other);
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

		return Equals((MorestachioLambdaExpression)obj);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		unchecked
		{
			var hashCode = Location.GetHashCode();
			hashCode = (hashCode * 397) ^ (Expression != null ? Expression.GetHashCode() : 0);
			hashCode = (hashCode * 397) ^ (Parameters != null ? Parameters.GetHashCode() : 0);
			return hashCode;
		}
	}
}