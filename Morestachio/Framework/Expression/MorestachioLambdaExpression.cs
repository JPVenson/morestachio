using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Morestachio.Document;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression.Parser;
using Morestachio.Framework.Expression.Visitors;
using Morestachio.Helper;
#if ValueTask
using ContextObjectPromise = System.Threading.Tasks.ValueTask<Morestachio.Framework.Context.ContextObject>;
#else
using ContextObjectPromise = System.Threading.Tasks.Task<Morestachio.Framework.Context.ContextObject>;
#endif

namespace Morestachio.Framework.Expression
{
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
		public MorestachioLambdaExpression(IMorestachioExpression parameters, CharacterLocation location)
		{
			Parameters = parameters;
			Location = location;
		}

		public MorestachioLambdaExpression(SerializationInfo info, StreamingContext context)
		{
			Location = CharacterLocation.FromFormatString(info.GetString(nameof(Location)));
			Expression = info.GetValue(nameof(Expression), typeof(IMorestachioExpression)) as IMorestachioExpression;
			Parameters = info.GetValue(nameof(Parameters), typeof(IMorestachioExpression)) as IMorestachioExpression;
		}

		/// <inheritdoc />
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(nameof(Location), Location.ToFormatString());
			info.AddValue(nameof(Expression), Expression);
			info.AddValue(nameof(Parameters), Parameters);
		}

		public XmlSchema GetSchema()
		{
			return null;
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
			Expression = reader.ParseExpressionFromKind();
			reader.ReadEndElement();
			reader.ReadStartElement();
			Parameters = reader.ParseExpressionFromKind();
			reader.ReadEndElement();
		}

		/// <inheritdoc />
		public void WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement(nameof(Expression));
			writer.WriteExpressionToXml(Expression);
			writer.WriteEndElement();

			writer.WriteStartElement(nameof(Parameters));
			writer.WriteExpressionToXml(Parameters);
			writer.WriteEndElement();
		}

		/// <inheritdoc />
		public CharacterLocation Location { get; private set; }

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

		/// <inheritdoc />
		public CompiledExpression Compile()
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
					var visitor = new ToParsableStringExpressionVisitor();
					_exp.Accept(visitor);
					return visitor.StringBuilder.ToString();
				}
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

	public class MorestachioTemplateExpression
	{
		private readonly MorestachioLambdaExpression _expression;
		private readonly ContextObject _contextObject;
		private ScopeData _scopeData;

		public MorestachioTemplateExpression(MorestachioLambdaExpression expression, ContextObject contextObject, ScopeData scopeData)
		{
			_expression = expression;
			_contextObject = contextObject;
			_scopeData = scopeData;
		}

		private void AssertParameterCount(int parameters)
		{
			if ((_expression.Parameters is MorestachioBracketExpression listOfParams && listOfParams.Expressions.Count != parameters) || (!(_expression.Parameters is MorestachioExpression) && parameters == 1))
			{
				ThrowNotMatchingParameters();
			}
		}

		private void ThrowNotMatchingParameters()
		{
			throw new InvalidOperationException("Cannot create a delegate");
		}

		private IEnumerable<MorestachioExpression> Parameter()
		{
			if (_expression.Parameters is MorestachioBracketExpression brackets)
			{
				foreach (var bracket in brackets.Expressions.Cast<MorestachioExpression>())
				{
					yield return bracket;
				}
			}
			else if (_expression.Parameters is MorestachioExpression exp)
			{
				yield return exp;
			}
		}

		private void AddArguments(params object[] args)
		{
			var parameter = Parameter().ToArray();
			for (var index = 0; index < parameter.Length; index++)
			{
				var morestachioExpression = parameter[index];
				var value = args[index];
				_scopeData.AddVariable(morestachioExpression.PathParts.Current.Key, (s) => s.ParserOptions.CreateContextObject("", value), 0);
			}
		}

		private void RemoveArguments()
		{
			var parameter = Parameter().ToArray();
			for (var index = 0; index < parameter.Length; index++)
			{
				var morestachioExpression = parameter[index];
				_scopeData.RemoveVariable(morestachioExpression.PathParts.Current.Key, 0);
			}
		}

		#region Autogenerated

		﻿﻿

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<TResult> AsFunc<TResult>()
		{
			AssertParameterCount(0);
			return () =>
			{
				var clone = _contextObject.CloneForEdit();
				return (TResult)_expression.Expression.GetValue(clone, _scopeData).GetAwaiter().GetResult().Value;
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<Task<TResult>> AsAsyncFunc<TResult>()
		{
			AssertParameterCount(0);
			return async () =>
			{
				var clone = _contextObject.CloneForEdit();
				return (TResult)((await _expression.Expression.GetValue(clone, _scopeData)).Value);
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<T0, TResult> AsFunc<T0, TResult>()
		{
			AssertParameterCount(0);
			return (arg1) =>
			{
				AddArguments(arg1);
				var clone = _contextObject.CloneForEdit();
				var result = (TResult)((_expression.Expression.GetValue(clone, _scopeData)).GetAwaiter().GetResult().Value);
				RemoveArguments();
				return result;
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<T0, T1, TResult> AsFunc<T0, T1, TResult>()
		{
			AssertParameterCount(1);
			return (arg0, arg2) =>
			{
				AddArguments(arg0, arg2);
				var clone = _contextObject.CloneForEdit();
				var result = (TResult)((_expression.Expression.GetValue(clone, _scopeData)).GetAwaiter().GetResult().Value);
				RemoveArguments();
				return result;
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<T0, T1, T2, TResult> AsFunc<T0, T1, T2, TResult>()
		{
			AssertParameterCount(2);
			return (arg0, arg1, arg3) =>
			{
				AddArguments(arg0, arg1, arg3);
				var clone = _contextObject.CloneForEdit();
				var result = (TResult)((_expression.Expression.GetValue(clone, _scopeData)).GetAwaiter().GetResult().Value);
				RemoveArguments();
				return result;
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<T0, T1, T2, T3, TResult> AsFunc<T0, T1, T2, T3, TResult>()
		{
			AssertParameterCount(3);
			return (arg0, arg1, arg2, arg4) =>
			{
				AddArguments(arg0, arg1, arg2, arg4);
				var clone = _contextObject.CloneForEdit();
				var result = (TResult)((_expression.Expression.GetValue(clone, _scopeData)).GetAwaiter().GetResult().Value);
				RemoveArguments();
				return result;
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<T0, T1, T2, T3, T4, TResult> AsFunc<T0, T1, T2, T3, T4, TResult>()
		{
			AssertParameterCount(4);
			return (arg0, arg1, arg2, arg3, arg5) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg5);
				var clone = _contextObject.CloneForEdit();
				var result = (TResult)((_expression.Expression.GetValue(clone, _scopeData)).GetAwaiter().GetResult().Value);
				RemoveArguments();
				return result;
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<T0, T1, T2, T3, T4, T5, TResult> AsFunc<T0, T1, T2, T3, T4, T5, TResult>()
		{
			AssertParameterCount(5);
			return (arg0, arg1, arg2, arg3, arg4, arg6) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg6);
				var clone = _contextObject.CloneForEdit();
				var result = (TResult)((_expression.Expression.GetValue(clone, _scopeData)).GetAwaiter().GetResult().Value);
				RemoveArguments();
				return result;
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<T0, T1, T2, T3, T4, T5, T6, TResult> AsFunc<T0, T1, T2, T3, T4, T5, T6, TResult>()
		{
			AssertParameterCount(6);
			return (arg0, arg1, arg2, arg3, arg4, arg5, arg7) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg5, arg7);
				var clone = _contextObject.CloneForEdit();
				var result = (TResult)((_expression.Expression.GetValue(clone, _scopeData)).GetAwaiter().GetResult().Value);
				RemoveArguments();
				return result;
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<T0, T1, T2, T3, T4, T5, T6, T7, TResult> AsFunc<T0, T1, T2, T3, T4, T5, T6, T7, TResult>()
		{
			AssertParameterCount(7);
			return (arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg8) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg8);
				var clone = _contextObject.CloneForEdit();
				var result = (TResult)((_expression.Expression.GetValue(clone, _scopeData)).GetAwaiter().GetResult().Value);
				RemoveArguments();
				return result;
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, TResult> AsFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, TResult>()
		{
			AssertParameterCount(8);
			return (arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg9) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg9);
				var clone = _contextObject.CloneForEdit();
				var result = (TResult)((_expression.Expression.GetValue(clone, _scopeData)).GetAwaiter().GetResult().Value);
				RemoveArguments();
				return result;
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> AsFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>()
		{
			AssertParameterCount(9);
			return (arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg10) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg10);
				var clone = _contextObject.CloneForEdit();
				var result = (TResult)((_expression.Expression.GetValue(clone, _scopeData)).GetAwaiter().GetResult().Value);
				RemoveArguments();
				return result;
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> AsFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>()
		{
			AssertParameterCount(10);
			return (arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg11) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg11);
				var clone = _contextObject.CloneForEdit();
				var result = (TResult)((_expression.Expression.GetValue(clone, _scopeData)).GetAwaiter().GetResult().Value);
				RemoveArguments();
				return result;
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> AsFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>()
		{
			AssertParameterCount(11);
			return (arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg12) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg12);
				var clone = _contextObject.CloneForEdit();
				var result = (TResult)((_expression.Expression.GetValue(clone, _scopeData)).GetAwaiter().GetResult().Value);
				RemoveArguments();
				return result;
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> AsFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>()
		{
			AssertParameterCount(12);
			return (arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg13) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg13);
				var clone = _contextObject.CloneForEdit();
				var result = (TResult)((_expression.Expression.GetValue(clone, _scopeData)).GetAwaiter().GetResult().Value);
				RemoveArguments();
				return result;
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> AsFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>()
		{
			AssertParameterCount(13);
			return (arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg14) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg14);
				var clone = _contextObject.CloneForEdit();
				var result = (TResult)((_expression.Expression.GetValue(clone, _scopeData)).GetAwaiter().GetResult().Value);
				RemoveArguments();
				return result;
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> AsFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>()
		{
			AssertParameterCount(14);
			return (arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg15) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg15);
				var clone = _contextObject.CloneForEdit();
				var result = (TResult)((_expression.Expression.GetValue(clone, _scopeData)).GetAwaiter().GetResult().Value);
				RemoveArguments();
				return result;
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> AsFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>()
		{
			AssertParameterCount(15);
			return (arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg16) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg16);
				var clone = _contextObject.CloneForEdit();
				var result = (TResult)((_expression.Expression.GetValue(clone, _scopeData)).GetAwaiter().GetResult().Value);
				RemoveArguments();
				return result;
			};
		}


		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<T0, Task<TResult>> AsAsyncFunc<T0, TResult>()
		{
			AssertParameterCount(0);
			return async (arg1) =>
			{
				AddArguments(arg1);
				var clone = _contextObject.CloneForEdit();
				var result = (TResult)((await _expression.Expression.GetValue(clone, _scopeData)).Value);
				RemoveArguments();
				return result;
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<T0, T1, Task<TResult>> AsAsyncFunc<T0, T1, TResult>()
		{
			AssertParameterCount(1);
			return async (arg0, arg2) =>
			{
				AddArguments(arg0, arg2);
				var clone = _contextObject.CloneForEdit();
				var result = (TResult)((await _expression.Expression.GetValue(clone, _scopeData)).Value);
				RemoveArguments();
				return result;
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<T0, T1, T2, Task<TResult>> AsAsyncFunc<T0, T1, T2, TResult>()
		{
			AssertParameterCount(2);
			return async (arg0, arg1, arg3) =>
			{
				AddArguments(arg0, arg1, arg3);
				var clone = _contextObject.CloneForEdit();
				var result = (TResult)((await _expression.Expression.GetValue(clone, _scopeData)).Value);
				RemoveArguments();
				return result;
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<T0, T1, T2, T3, Task<TResult>> AsAsyncFunc<T0, T1, T2, T3, TResult>()
		{
			AssertParameterCount(3);
			return async (arg0, arg1, arg2, arg4) =>
			{
				AddArguments(arg0, arg1, arg2, arg4);
				var clone = _contextObject.CloneForEdit();
				var result = (TResult)((await _expression.Expression.GetValue(clone, _scopeData)).Value);
				RemoveArguments();
				return result;
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<T0, T1, T2, T3, T4, Task<TResult>> AsAsyncFunc<T0, T1, T2, T3, T4, TResult>()
		{
			AssertParameterCount(4);
			return async (arg0, arg1, arg2, arg3, arg5) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg5);
				var clone = _contextObject.CloneForEdit();
				var result = (TResult)((await _expression.Expression.GetValue(clone, _scopeData)).Value);
				RemoveArguments();
				return result;
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<T0, T1, T2, T3, T4, T5, Task<TResult>> AsAsyncFunc<T0, T1, T2, T3, T4, T5, TResult>()
		{
			AssertParameterCount(5);
			return async (arg0, arg1, arg2, arg3, arg4, arg6) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg6);
				var clone = _contextObject.CloneForEdit();
				var result = (TResult)((await _expression.Expression.GetValue(clone, _scopeData)).Value);
				RemoveArguments();
				return result;
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<T0, T1, T2, T3, T4, T5, T6, Task<TResult>> AsAsyncFunc<T0, T1, T2, T3, T4, T5, T6, TResult>()
		{
			AssertParameterCount(6);
			return async (arg0, arg1, arg2, arg3, arg4, arg5, arg7) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg5, arg7);
				var clone = _contextObject.CloneForEdit();
				var result = (TResult)((await _expression.Expression.GetValue(clone, _scopeData)).Value);
				RemoveArguments();
				return result;
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<T0, T1, T2, T3, T4, T5, T6, T7, Task<TResult>> AsAsyncFunc<T0, T1, T2, T3, T4, T5, T6, T7, TResult>()
		{
			AssertParameterCount(7);
			return async (arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg8) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg8);
				var clone = _contextObject.CloneForEdit();
				var result = (TResult)((await _expression.Expression.GetValue(clone, _scopeData)).Value);
				RemoveArguments();
				return result;
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, Task<TResult>> AsAsyncFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, TResult>()
		{
			AssertParameterCount(8);
			return async (arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg9) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg9);
				var clone = _contextObject.CloneForEdit();
				var result = (TResult)((await _expression.Expression.GetValue(clone, _scopeData)).Value);
				RemoveArguments();
				return result;
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, Task<TResult>> AsAsyncFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>()
		{
			AssertParameterCount(9);
			return async (arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg10) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg10);
				var clone = _contextObject.CloneForEdit();
				var result = (TResult)((await _expression.Expression.GetValue(clone, _scopeData)).Value);
				RemoveArguments();
				return result;
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, Task<TResult>> AsAsyncFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>()
		{
			AssertParameterCount(10);
			return async (arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg11) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg11);
				var clone = _contextObject.CloneForEdit();
				var result = (TResult)((await _expression.Expression.GetValue(clone, _scopeData)).Value);
				RemoveArguments();
				return result;
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, Task<TResult>> AsAsyncFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>()
		{
			AssertParameterCount(11);
			return async (arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg12) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg12);
				var clone = _contextObject.CloneForEdit();
				var result = (TResult)((await _expression.Expression.GetValue(clone, _scopeData)).Value);
				RemoveArguments();
				return result;
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, Task<TResult>> AsAsyncFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>()
		{
			AssertParameterCount(12);
			return async (arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg13) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg13);
				var clone = _contextObject.CloneForEdit();
				var result = (TResult)((await _expression.Expression.GetValue(clone, _scopeData)).Value);
				RemoveArguments();
				return result;
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, Task<TResult>> AsAsyncFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>()
		{
			AssertParameterCount(13);
			return async (arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg14) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg14);
				var clone = _contextObject.CloneForEdit();
				var result = (TResult)((await _expression.Expression.GetValue(clone, _scopeData)).Value);
				RemoveArguments();
				return result;
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, Task<TResult>> AsAsyncFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>()
		{
			AssertParameterCount(14);
			return async (arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg15) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg15);
				var clone = _contextObject.CloneForEdit();
				var result = (TResult)((await _expression.Expression.GetValue(clone, _scopeData)).Value);
				RemoveArguments();
				return result;
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
		/// </summary>
		public Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, Task<TResult>> AsAsyncFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>()
		{
			AssertParameterCount(15);
			return async (arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg16) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg16);
				var clone = _contextObject.CloneForEdit();
				var result = (TResult)((await _expression.Expression.GetValue(clone, _scopeData)).Value);
				RemoveArguments();
				return result;
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression
		/// </summary>
		public Action<T> AsAction<T>()
		{
			AssertParameterCount(1);
			return arg1 =>
			{
				AddArguments(arg1);
				var clone = _contextObject.CloneForEdit();
				_expression.Expression.GetValue(clone, _scopeData).GetAwaiter().GetResult();
				RemoveArguments();
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression
		/// </summary>
		public Action<T0, T1> AsAction<T0, T1>()
		{
			AssertParameterCount(1);
			return (arg0, arg2) =>
			{
				AddArguments(arg0, arg2);
				var clone = _contextObject.CloneForEdit();
				_expression.Expression.GetValue(clone, _scopeData).GetAwaiter().GetResult();
				RemoveArguments();
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression
		/// </summary>
		public Action<T0, T1, T2> AsAction<T0, T1, T2>()
		{
			AssertParameterCount(2);
			return (arg0, arg1, arg3) =>
			{
				AddArguments(arg0, arg1, arg3);
				var clone = _contextObject.CloneForEdit();
				_expression.Expression.GetValue(clone, _scopeData).GetAwaiter().GetResult();
				RemoveArguments();
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression
		/// </summary>
		public Action<T0, T1, T2, T3> AsAction<T0, T1, T2, T3>()
		{
			AssertParameterCount(3);
			return (arg0, arg1, arg2, arg4) =>
			{
				AddArguments(arg0, arg1, arg2, arg4);
				var clone = _contextObject.CloneForEdit();
				_expression.Expression.GetValue(clone, _scopeData).GetAwaiter().GetResult();
				RemoveArguments();
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression
		/// </summary>
		public Action<T0, T1, T2, T3, T4> AsAction<T0, T1, T2, T3, T4>()
		{
			AssertParameterCount(4);
			return (arg0, arg1, arg2, arg3, arg5) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg5);
				var clone = _contextObject.CloneForEdit();
				_expression.Expression.GetValue(clone, _scopeData).GetAwaiter().GetResult();
				RemoveArguments();
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression
		/// </summary>
		public Action<T0, T1, T2, T3, T4, T5> AsAction<T0, T1, T2, T3, T4, T5>()
		{
			AssertParameterCount(5);
			return (arg0, arg1, arg2, arg3, arg4, arg6) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg6);
				var clone = _contextObject.CloneForEdit();
				_expression.Expression.GetValue(clone, _scopeData).GetAwaiter().GetResult();
				RemoveArguments();
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression
		/// </summary>
		public Action<T0, T1, T2, T3, T4, T5, T6> AsAction<T0, T1, T2, T3, T4, T5, T6>()
		{
			AssertParameterCount(6);
			return (arg0, arg1, arg2, arg3, arg4, arg5, arg7) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg5, arg7);
				var clone = _contextObject.CloneForEdit();
				_expression.Expression.GetValue(clone, _scopeData).GetAwaiter().GetResult();
				RemoveArguments();
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression
		/// </summary>
		public Action<T0, T1, T2, T3, T4, T5, T6, T7> AsAction<T0, T1, T2, T3, T4, T5, T6, T7>()
		{
			AssertParameterCount(7);
			return (arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg8) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg8);
				var clone = _contextObject.CloneForEdit();
				_expression.Expression.GetValue(clone, _scopeData).GetAwaiter().GetResult();
				RemoveArguments();
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression
		/// </summary>
		public Action<T0, T1, T2, T3, T4, T5, T6, T7, T8> AsAction<T0, T1, T2, T3, T4, T5, T6, T7, T8>()
		{
			AssertParameterCount(8);
			return (arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg9) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg9);
				var clone = _contextObject.CloneForEdit();
				_expression.Expression.GetValue(clone, _scopeData).GetAwaiter().GetResult();
				RemoveArguments();
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression
		/// </summary>
		public Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> AsAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>()
		{
			AssertParameterCount(9);
			return (arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg10) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg10);
				var clone = _contextObject.CloneForEdit();
				_expression.Expression.GetValue(clone, _scopeData).GetAwaiter().GetResult();
				RemoveArguments();
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression
		/// </summary>
		public Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> AsAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>()
		{
			AssertParameterCount(10);
			return (arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg11) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg11);
				var clone = _contextObject.CloneForEdit();
				_expression.Expression.GetValue(clone, _scopeData).GetAwaiter().GetResult();
				RemoveArguments();
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression
		/// </summary>
		public Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> AsAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>()
		{
			AssertParameterCount(11);
			return (arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg12) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg12);
				var clone = _contextObject.CloneForEdit();
				_expression.Expression.GetValue(clone, _scopeData).GetAwaiter().GetResult();
				RemoveArguments();
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression
		/// </summary>
		public Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> AsAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>()
		{
			AssertParameterCount(12);
			return (arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg13) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg13);
				var clone = _contextObject.CloneForEdit();
				_expression.Expression.GetValue(clone, _scopeData).GetAwaiter().GetResult();
				RemoveArguments();
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression
		/// </summary>
		public Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> AsAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>()
		{
			AssertParameterCount(13);
			return (arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg14) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg14);
				var clone = _contextObject.CloneForEdit();
				_expression.Expression.GetValue(clone, _scopeData).GetAwaiter().GetResult();
				RemoveArguments();
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression
		/// </summary>
		public Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> AsAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>()
		{
			AssertParameterCount(14);
			return (arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg15) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg15);
				var clone = _contextObject.CloneForEdit();
				_expression.Expression.GetValue(clone, _scopeData).GetAwaiter().GetResult();
				RemoveArguments();
			};
		}

		/// <summary>
		///		Creates a new c# Delegate that invokes the morestachio expression
		/// </summary>
		public Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> AsAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>()
		{
			AssertParameterCount(15);
			return (arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg16) =>
			{
				AddArguments(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg16);
				var clone = _contextObject.CloneForEdit();
				_expression.Expression.GetValue(clone, _scopeData).GetAwaiter().GetResult();
				RemoveArguments();
			};
		}




		#endregion
	}
}