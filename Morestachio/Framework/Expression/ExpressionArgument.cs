using System.Diagnostics;
using System.Xml;
using System.Xml.Schema;
using Morestachio.Document;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression.Parser;
using Morestachio.Framework.Expression.Visitors;

namespace Morestachio.Framework.Expression
{
	/// <summary>
	///     Defines an Argument used within a formatter
	/// </summary>
	[DebuggerTypeProxy(typeof(ExpressionDebuggerDisplay))]
	[Serializable]
	public class ExpressionArgument : IEquatable<ExpressionArgument>, IMorestachioExpression
	{
		internal ExpressionArgument()
		{
		}

		/// <summary>
		/// </summary>
		public ExpressionArgument(CharacterLocation location, IMorestachioExpression expression, string name)
		{
			Location = location;
			MorestachioExpression = expression;
			Name = name;
		}


		/// <summary>
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		protected ExpressionArgument(SerializationInfo info, StreamingContext context)
		{
			Name = info.GetString(nameof(Name));
			MorestachioExpression =
				info.GetValue(nameof(MorestachioExpression), typeof(IMorestachioExpression)) as IMorestachioExpression;
			Location = CharacterLocation.FromFormatString(info.GetString(nameof(Location)));
		}

		/// <summary>
		///     The name of the Argument
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		///     The value of the Argument
		/// </summary>
		public IMorestachioExpression MorestachioExpression { get; set; }

		/// <param name="parserOptions"></param>
		/// <inheritdoc />
		public CompiledExpression Compile(ParserOptions parserOptions)
		{
			return MorestachioExpression.Compile(parserOptions);
		}

		/// <inheritdoc />
		public bool Equals(ExpressionArgument other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return Name == other.Name && MorestachioExpression.Equals(other.MorestachioExpression) &&
			       Location.Equals(other.Location);
		}

		/// <summary>
		///     The Location within the Template
		/// </summary>
		public CharacterLocation Location { get; private set; }

		/// <summary>
		/// </summary>
		/// <param name="contextObject"></param>
		/// <param name="scopeData"></param>
		/// <returns></returns>
		public async ContextObjectPromise GetValue(ContextObject contextObject, ScopeData scopeData)
		{
			return await MorestachioExpression.GetValue(contextObject, scopeData);
		}

		/// <inheritdoc />
		public void Accept(IMorestachioExpressionVisitor visitor)
		{
			visitor.Visit(this);
		}
		
		/// <inheritdoc />
		public bool IsCompileTimeEval()
		{
			return MorestachioExpression.IsCompileTimeEval();
		}
		
		/// <inheritdoc />
		public object GetCompileTimeValue()
		{
			return MorestachioExpression.GetCompileTimeValue();
		}

		/// <inheritdoc />
		public XmlSchema GetSchema()
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(nameof(Name), Name);
			info.AddValue(nameof(Location), Location.ToFormatString());
			info.AddValue(nameof(MorestachioExpression), MorestachioExpression);
		}

		/// <inheritdoc />
		public void ReadXml(XmlReader reader)
		{
			Location = CharacterLocation.FromFormatString(reader.GetAttribute(nameof(Location)));
			Name = reader.GetAttribute(nameof(Name));
			reader.ReadStartElement();

			var expSubtree = reader.ReadSubtree();
			expSubtree.Read();
			MorestachioExpression = expSubtree.ParseExpressionFromKind();
		}

		/// <inheritdoc />
		public void WriteXml(XmlWriter writer)
		{
			if (Name != null)
			{
				writer.WriteAttributeString(nameof(Name), Name);
			}

			writer.WriteAttributeString(nameof(Location), Location.ToFormatString());
			writer.WriteExpressionToXml(MorestachioExpression);
		}

		/// <inheritdoc />
		public bool Equals(IMorestachioExpression other)
		{
			return Equals((object) other);
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

			if (obj.GetType() != GetType())
			{
				return false;
			}

			return Equals((ExpressionArgument) obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Name != null ? Name.GetHashCode() : 0;
				hashCode = (hashCode * 397) ^ (MorestachioExpression != null ? MorestachioExpression.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Location.GetHashCode());
				return hashCode;
			}
		}

		private class ExpressionDebuggerDisplay
		{
			private readonly ExpressionArgument _exp;

			public ExpressionDebuggerDisplay(ExpressionArgument exp)
			{
				_exp = exp;
			}

			public string Expression
			{
				get { return _exp.ToString(); }
			}

			public string Name
			{
				get { return _exp.Name; }
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