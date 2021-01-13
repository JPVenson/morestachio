using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using Morestachio.Document;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression.Visitors;
#if ValueTask
using ContextObjectPromise = System.Threading.Tasks.ValueTask<Morestachio.Framework.Context.ContextObject>;
#else
using ContextObjectPromise = System.Threading.Tasks.Task<Morestachio.Framework.Context.ContextObject>;
#endif

namespace Morestachio.Framework.Expression
{
	/// <summary>
	///		Defines a list of Expressions 
	/// </summary>
	[DebuggerTypeProxy(typeof(ExpressionDebuggerDisplay))]
	[Serializable]
	public abstract class MorestachioExpressionListBase : IMorestachioExpression
	{
		internal MorestachioExpressionListBase()
		{
			
		}

		/// <summary>
		/// 
		/// </summary>
		public MorestachioExpressionListBase(CharacterLocation location)
		{
			Location = location;
			Expressions = new List<IMorestachioExpression>();
		}

		/// <summary>
		///	
		/// </summary>
		public MorestachioExpressionListBase(IList<IMorestachioExpression> expressions, CharacterLocation location)
		{
			Expressions = expressions;
			Location = location;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		protected MorestachioExpressionListBase(SerializationInfo info, StreamingContext context)
		{
			Location = CharacterLocation.FromFormatString(info.GetString(nameof(Location)));
			Expressions = (IMorestachioExpression[])info.GetValue(nameof(Expressions), typeof(IMorestachioExpression[]));
		}

		/// <summary>
		///		The list of Expressions
		/// </summary>
		public IList<IMorestachioExpression> Expressions { get; private set; }

		/// <inheritdoc />
		public CharacterLocation Location { get; private set; }
		/// <inheritdoc />
		public async ContextObjectPromise GetValue(ContextObject contextObject, ScopeData scopeData)
		{
			foreach (var expression in Expressions)
			{
				contextObject = await expression.GetValue(contextObject, scopeData);
			}

			return contextObject;
		}

		/// <inheritdoc />
		public CompiledExpression Compile()
		{
			var exps = Expressions.Select(f => f.Compile()).ToArray();
			return async (contextObject, data) =>
			{
				foreach (var compiledExpression in exps)
				{
					contextObject = await compiledExpression(contextObject, data);
				}

				return contextObject;
			};
		}

		/// <inheritdoc />
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(nameof(Location), Location.ToFormatString());
			info.AddValue(nameof(Expressions), Expressions.ToArray());
		}

		/// <inheritdoc />
		public XmlSchema GetSchema()
		{
			throw new System.NotImplementedException();
		}

		/// <inheritdoc />
		public virtual void ReadXml(XmlReader reader)
		{
			Location = CharacterLocation.FromFormatString(reader.GetAttribute(nameof(Location)));
			if (reader.IsEmptyElement)
			{
				return;
			}
			reader.ReadStartElement();
			var expression = new List<IMorestachioExpression>();
			while (reader.NodeType == XmlNodeType.Element)
			{
				var childTree = reader.ReadSubtree();
				childTree.Read();
				expression.Add(childTree.ParseExpressionFromKind());
				reader.Skip();
			}

			Expressions = expression.ToArray();
		}

		/// <inheritdoc />
		public virtual void WriteXml(XmlWriter writer)
		{
			writer.WriteAttributeString(nameof(Location), Location.ToFormatString());
			foreach (var expression in Expressions)
			{
				writer.WriteExpressionToXml(expression);
			}
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
		protected bool Equals(MorestachioExpressionListBase other)
		{
			if (!Location.Equals(other.Location))
			{
				return false;
			}

			if (other.Expressions.Count != Expressions.Count)
			{
				return false;
			}

			for (var index = 0; index < Expressions.Count; index++)
			{
				var expression = Expressions[index];
				var otherExp = other.Expressions[index];
				if (!expression.Equals(otherExp))
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

			return Equals((MorestachioExpressionListBase)obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				return ((Expressions != null ? Expressions.GetHashCode() : 0) * 397) ^ (Location.GetHashCode());
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="currentScopeValue"></param>
		protected internal void Add(IMorestachioExpression currentScopeValue)
		{
			Expressions.Add(currentScopeValue);
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
			private readonly MorestachioExpressionListBase _exp;

			public ExpressionDebuggerDisplay(MorestachioExpressionListBase exp)
			{
				_exp = exp;
			}

			public string Expression
			{
				get { return _exp.ToString(); }
			}
		}
	}
}