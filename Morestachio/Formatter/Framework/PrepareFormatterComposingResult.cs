using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Morestachio.Formatter.Framework
{
	/// <summary>
	/// </summary>
	public class PrepareFormatterComposingResult
	{
		/// <summary>
		/// 
		/// </summary>
		public PrepareFormatterComposingResult(
			 Func<object[], MethodInfo> methodInfo,
			 IDictionary<MultiFormatterInfo, FormatterArgumentMap> arguments)
		{
			MethodInfo = methodInfo;
			Arguments = arguments;
		}

		/// <summary>
		///     The Result Method of the Composing operation. It can be different from the original.
		/// </summary>

		public Func<object[], MethodInfo> MethodInfo { get; }

		private MethodInfo _methodInfo;
		private Func<object, object[], object> _callCache;

		/// <summary>
		///		Gets an compiled Method info
		/// </summary>
		/// <param name="arguments"></param>
		/// <returns></returns>
		public (Func<object, object[], object>, MethodInfo) PrepareInvoke(object[] arguments)
		{
			if (_methodInfo is not null)
			{
				return (_callCache, _methodInfo);
			}

			_methodInfo = MethodInfo(arguments);
			_callCache = BuildCaller(_methodInfo, arguments);
			return (_callCache, _methodInfo);
		}

		internal static Func<object, object[], object> BuildCaller(MethodInfo method, object[] arguments)
		{
			try
			{
				var argsParam = Expression.Parameter(typeof(object[]), "args");
				var instParam = Expression.Parameter(typeof(object), "instance");

				var parameterInfos = method.GetParameters();

				UnaryExpression callTarget;

				if (method.IsStatic)
				{
					callTarget = null;
				}
				else
				{
					callTarget = Expression.Convert(instParam, method.DeclaringType);
				}

				Expression body = Expression.Call(callTarget, method,
					parameterInfos.Select<ParameterInfo, Expression>((parameterInfo, index) =>
					{
						var val = parameterInfos.Length > index ? arguments[index] : null;
						if (val == null)
						{
							return Expression.Default(parameterInfo.ParameterType);
						}
						return Expression.Convert(Expression.ArrayAccess(argsParam, Expression.Constant(index)), val.GetType());
					}));

				if (!method.ReturnParameter.ParameterType.IsClass && !method.ReturnParameter.ParameterType.IsInterface)
				{
					//if its not a class and not an interface its an struct and we have to box it
					body = Expression.Convert(body, typeof(object));
				}
				if (body.CanReduce)
				{
					body = body.Reduce();
				}

				return Expression.Lambda<Func<object, object[], object>>(body, true, instParam, argsParam).Compile();
			}
			catch (Exception e)
			{
				//in the case that the expression cannot be compiled. Fall back to reflection.
				return method.Invoke;
			}
		}

		/// <summary>
		///     The list of arguments for the <see cref="MethodInfo" />
		/// </summary>

		public IDictionary<MultiFormatterInfo, FormatterArgumentMap> Arguments { get; }
	}
}