using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Morestachio.Document;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Expression.StringParts;
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
	///		Defines a string as or inside an expression
	/// </summary>
	[DebuggerTypeProxy(typeof(ExpressionDebuggerDisplay))]
	[Serializable]
	public class MorestachioExpressionString : IMorestachioExpression
	{
		internal MorestachioExpressionString()
		{
			StringParts = new List<ExpressionStringConstPart>();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="location"></param>
		public MorestachioExpressionString(CharacterLocation location, char delimiter)
		{
			Location = location;
			Delimiter = delimiter;
			StringParts = new List<ExpressionStringConstPart>();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		protected MorestachioExpressionString(SerializationInfo info, StreamingContext context)
		{
			StringParts = (IList<ExpressionStringConstPart>)info.GetValue(nameof(StringParts), typeof(IList<ExpressionStringConstPart>));
			Location = CharacterLocation.FromFormatString(info.GetString(nameof(Location)));
			Delimiter = info.GetChar(nameof(Delimiter));
		}

		/// <inheritdoc />
		public XmlSchema GetSchema()
		{
			throw new System.NotImplementedException();
		}

		/// <inheritdoc />
		public void ReadXml(XmlReader reader)
		{
			Location = CharacterLocation.FromFormatString(reader.GetAttribute(nameof(Location)));
			if (reader.IsEmptyElement)
			{
				return;
			}
			reader.ReadStartElement();
			while (reader.Name == nameof(ExpressionStringConstPart) && reader.NodeType != XmlNodeType.EndElement)
			{
				var strLocation = CharacterLocation.FromFormatString(reader.GetAttribute(nameof(Location)));
				var constStrPartText = reader.ReadElementContentAsString();
				Delimiter = constStrPartText[0];
				var strPartText = constStrPartText.Substring(1, constStrPartText.Length - 2);

				StringParts.Add(new ExpressionStringConstPart(strPartText, strLocation));
				reader.ReadEndElement();
			}
		}

		/// <inheritdoc />
		public void WriteXml(XmlWriter writer)
		{
			writer.WriteAttributeString(nameof(Location), Location.ToFormatString());
			foreach (var expressionStringConstPart in StringParts.Where(e => !(e.PartText is null)))
			{
				writer.WriteStartElement(expressionStringConstPart.GetType().Name);
				writer.WriteAttributeString(nameof(Location), expressionStringConstPart.Location.ToFormatString());
				writer.WriteString(Delimiter + expressionStringConstPart.PartText + Delimiter);
				writer.WriteEndElement();
			}
		}

		/// <inheritdoc />
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(nameof(StringParts), StringParts);
			info.AddValue(nameof(Location), Location.ToFormatString());
			info.AddValue(nameof(Delimiter), Delimiter);
		}

		/// <summary>
		///		Defines the list of string parts for this string expression
		/// </summary>
		public IList<ExpressionStringConstPart> StringParts { get; set; }

		/// <inheritdoc />
		public CharacterLocation Location { get; set; }

		/// <summary>
		///		The original Delimiter
		/// </summary>
		public char Delimiter { get; set; }

		/// <inheritdoc />
		public async ContextObjectPromise GetValue(ContextObject contextObject, ScopeData scopeData)
		{
			await Task.CompletedTask;
			return scopeData.ParserOptions.CreateContextObject(".",
				string.Join("", StringParts.Select(f => f.PartText)),
				contextObject);
		}

		/// <inheritdoc />
		public CompiledExpression Compile()
		{
			var str = string.Join("", StringParts.Select(f => f.PartText));
			var value = new ContextObject(".", null, str);
			return (contextObject, scopeData) => value.ToPromise();
		}

		/// <inheritdoc />
		public void Accept(IMorestachioExpressionVisitor visitor)
		{
			visitor.Visit(this);
		}

		/// <inheritdoc />
		public bool Equals(IMorestachioExpression other)
		{
			return Equals((object)other);
		}

		/// <inheritdoc />
		protected bool Equals(MorestachioExpressionString other)
		{
			if (Delimiter != other.Delimiter || !Location.Equals(other.Location))
			{
				return false;
			}

			if (StringParts.Count != other.StringParts.Count)
			{
				return false;
			}

			for (var index = 0; index < StringParts.Count; index++)
			{
				var leftStrPart = StringParts[index];
				var rightStrPart = other.StringParts[index];
				if (!leftStrPart.Equals(rightStrPart))
				{
					return false;
				}
			}

			return true;
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

			return Equals((MorestachioExpressionString)obj);
		}
		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (StringParts != null ? StringParts.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Location.GetHashCode());
				hashCode = (hashCode * 397) ^ Delimiter.GetHashCode();
				return hashCode;
			}
		}

		private class ExpressionDebuggerDisplay
		{
			private readonly MorestachioExpressionString _exp;

			public ExpressionDebuggerDisplay(MorestachioExpressionString exp)
			{
				_exp = exp;
			}

			public string Expression
			{
				get { return _exp.ToString(); }
			}

			public char Delimiter
			{
				get { return _exp.Delimiter; }
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