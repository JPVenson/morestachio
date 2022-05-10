﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Morestachio.Document;
using Morestachio.Formatter.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression.Visitors;
using SysExpression = System.Linq.Expressions.Expression;

namespace Morestachio.Framework.Expression;

/// <summary>
///		Defines methods for wrapping the Morestachio expression into an c# delegate
/// </summary>
public class MorestachioTemplateExpression
{
	private readonly MorestachioLambdaExpression _expression;
	private readonly ContextObject _contextObject;
	private ScopeData _scopeData;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="expression"></param>
	/// <param name="contextObject"></param>
	/// <param name="scopeData"></param>
	public MorestachioTemplateExpression(MorestachioLambdaExpression expression, ContextObject contextObject, ScopeData scopeData)
	{
		_expression = expression;
		_contextObject = contextObject;
		_scopeData = scopeData;
	}

	private void AssertParameterCount(int parameters)
	{
		if ((_expression.Parameters is MorestachioBracketExpression listOfParams && listOfParams.Expressions.Count != parameters)
			|| (!(_expression.Parameters is MorestachioExpression) && parameters > 1))
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

	/// <inheritdoc />
	public override string ToString()
	{
		var visitor = new ToParsableStringExpressionVisitor();
		_expression.Accept(visitor);
		return visitor.StringBuilder.ToString();
	}

	#region Autogenerated

	//public T AsDelegate<T>()
	//{
	//	var methodType = typeof(T);
	//	var invokeMethod = methodType.GetMethod("Invoke");

	//	if (invokeMethod == null)
	//	{
	//		throw new InvalidOperationException($"{typeof(T)} does not seem to be a delegate type");
	//	}

	//	var returnType = invokeMethod.ReturnType;
	//	var parameters = invokeMethod.GetParameters();

	//	AssertParameterCount(parameters.Length);

	//	var expArguments = new ParameterExpression[parameters.Length];

	//	for (int i = 0; i < parameters.Length; i++)
	//	{
	//		var parameter = parameters[i];
	//		expArguments[i] = SysExpression.Parameter(parameter.ParameterType, "arg" + i);
	//	}

	//	var extParamContext = SysExpression.Parameter(_contextObject.GetType(), "_contextObject");
	//	var extParamExpression = SysExpression.Parameter(_expression.GetType(), "_expression");
	//	var extParamScope = SysExpression.Parameter(_scopeData.GetType(), "_scopeData");
	//	var extParamCallee = SysExpression.Parameter(GetType(), "callee");

	//	var cloneVarExp = SysExpression.Variable(_contextObject.GetType(), "clone");
	//	var assignCloneExp = SysExpression.Assign(cloneVarExp, SysExpression.Call(extParamContext, extParamContext.GetType().GetMethod(nameof(ContextObject.CloneForEdit))));
	//	MethodCallExpression pushArgsExp = null;
	//	MethodCallExpression popArgsExp = null;

	//	if (parameters.Length > 0)
	//	{
	//		pushArgsExp = SysExpression.Call(extParamCallee, GetType().GetMethod(nameof(AddArguments)), expArguments);
	//		popArgsExp = SysExpression.Call(extParamCallee, GetType().GetMethod(nameof(RemoveArguments)));
	//	}

	//	var methodCallExpression = SysExpression.Call(
	//		SysExpression.Property(extParamExpression, nameof(MorestachioLambdaExpression.Expression)),
	//		typeof(MorestachioLambdaExpression).GetMethod(nameof(MorestachioLambdaExpression.GetValue)),
	//		cloneVarExp, extParamScope);

	//	if (returnType != typeof(void))
	//	{
	//		//its a function
	//		SysExpression.Property(SysExpression.Call(null, typeof(AsyncHelper).GetMethod(nameof(AsyncHelper.Await)),
	//				methodCallExpression),
	//			nameof(ContextObject.Value));
	//	}

	//	return SysExpression.Lambda<T>(SysExpression.Block(typeof(T), new SysExpression[]
	//	{
	//		assignCloneExp,
	//		pushArgsExp,
	//		methodCallExpression,
	//		popArgsExp
	//	}),)
	//}

	/// <summary>
	///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
	/// </summary>
	public Func<TResult> AsFunc<TResult>()
	{
		AssertParameterCount(0);
		return () =>
		{
			var clone = _contextObject.CloneForEdit();
			return (TResult)_expression.Expression.GetValue(clone, _scopeData).Await().Value;
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
		AssertParameterCount(1);
		return (arg1) =>
		{
			AddArguments(arg1);
			var clone = _contextObject.CloneForEdit();
			var result = (TResult)((_expression.Expression.GetValue(clone, _scopeData)).Await().Value);
			RemoveArguments();
			return result;
		};
	}

	/// <summary>
	///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
	/// </summary>
	public Func<T0, T1, TResult> AsFunc<T0, T1, TResult>()
	{
		AssertParameterCount(2);
		return (arg1, arg2) =>
		{
			AddArguments(arg1, arg2);
			var clone = _contextObject.CloneForEdit();
			var result = (TResult)((_expression.Expression.GetValue(clone, _scopeData)).Await().Value);
			RemoveArguments();
			return result;
		};
	}

	/// <summary>
	///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
	/// </summary>
	public Func<T0, T1, T2, TResult> AsFunc<T0, T1, T2, TResult>()
	{
		AssertParameterCount(3);
		return (arg1, arg2, arg3) =>
		{
			AddArguments(arg1, arg2, arg3);
			var clone = _contextObject.CloneForEdit();
			var result = (TResult)((_expression.Expression.GetValue(clone, _scopeData)).Await().Value);
			RemoveArguments();
			return result;
		};
	}

	/// <summary>
	///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
	/// </summary>
	public Func<T0, T1, T2, T3, TResult> AsFunc<T0, T1, T2, T3, TResult>()
	{
		AssertParameterCount(4);
		return (arg1, arg2, arg3, arg4) =>
		{
			AddArguments(arg1, arg2, arg3, arg4);
			var clone = _contextObject.CloneForEdit();
			var result = (TResult)((_expression.Expression.GetValue(clone, _scopeData)).Await().Value);
			RemoveArguments();
			return result;
		};
	}

	/// <summary>
	///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
	/// </summary>
	public Func<T0, T1, T2, T3, T4, TResult> AsFunc<T0, T1, T2, T3, T4, TResult>()
	{
		AssertParameterCount(5);
		return (arg1, arg2, arg3, arg4, arg5) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5);
			var clone = _contextObject.CloneForEdit();
			var result = (TResult)((_expression.Expression.GetValue(clone, _scopeData)).Await().Value);
			RemoveArguments();
			return result;
		};
	}

	/// <summary>
	///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
	/// </summary>
	public Func<T0, T1, T2, T3, T4, T5, TResult> AsFunc<T0, T1, T2, T3, T4, T5, TResult>()
	{
		AssertParameterCount(6);
		return (arg1, arg2, arg3, arg4, arg5, arg6) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6);
			var clone = _contextObject.CloneForEdit();
			var result = (TResult)((_expression.Expression.GetValue(clone, _scopeData)).Await().Value);
			RemoveArguments();
			return result;
		};
	}

	/// <summary>
	///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
	/// </summary>
	public Func<T0, T1, T2, T3, T4, T5, T6, TResult> AsFunc<T0, T1, T2, T3, T4, T5, T6, TResult>()
	{
		AssertParameterCount(7);
		return (arg1, arg2, arg3, arg4, arg5, arg6, arg7) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
			var clone = _contextObject.CloneForEdit();
			var result = (TResult)((_expression.Expression.GetValue(clone, _scopeData)).Await().Value);
			RemoveArguments();
			return result;
		};
	}

	/// <summary>
	///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
	/// </summary>
	public Func<T0, T1, T2, T3, T4, T5, T6, T7, TResult> AsFunc<T0, T1, T2, T3, T4, T5, T6, T7, TResult>()
	{
		AssertParameterCount(8);
		return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
			var clone = _contextObject.CloneForEdit();
			var result = (TResult)((_expression.Expression.GetValue(clone, _scopeData)).Await().Value);
			RemoveArguments();
			return result;
		};
	}

	/// <summary>
	///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
	/// </summary>
	public Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, TResult> AsFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, TResult>()
	{
		AssertParameterCount(9);
		return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
			var clone = _contextObject.CloneForEdit();
			var result = (TResult)((_expression.Expression.GetValue(clone, _scopeData)).Await().Value);
			RemoveArguments();
			return result;
		};
	}

	/// <summary>
	///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
	/// </summary>
	public Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> AsFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>()
	{
		AssertParameterCount(10);
		return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
			var clone = _contextObject.CloneForEdit();
			var result = (TResult)((_expression.Expression.GetValue(clone, _scopeData)).Await().Value);
			RemoveArguments();
			return result;
		};
	}

	/// <summary>
	///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
	/// </summary>
	public Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> AsFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>()
	{
		AssertParameterCount(11);
		return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
			var clone = _contextObject.CloneForEdit();
			var result = (TResult)((_expression.Expression.GetValue(clone, _scopeData)).Await().Value);
			RemoveArguments();
			return result;
		};
	}

	/// <summary>
	///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
	/// </summary>
	public Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> AsFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>()
	{
		AssertParameterCount(12);
		return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
			var clone = _contextObject.CloneForEdit();
			var result = (TResult)((_expression.Expression.GetValue(clone, _scopeData)).Await().Value);
			RemoveArguments();
			return result;
		};
	}

	/// <summary>
	///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
	/// </summary>
	public Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> AsFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>()
	{
		AssertParameterCount(13);
		return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
			var clone = _contextObject.CloneForEdit();
			var result = (TResult)((_expression.Expression.GetValue(clone, _scopeData)).Await().Value);
			RemoveArguments();
			return result;
		};
	}

	/// <summary>
	///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
	/// </summary>
	public Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> AsFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>()
	{
		AssertParameterCount(14);
		return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
			var clone = _contextObject.CloneForEdit();
			var result = (TResult)((_expression.Expression.GetValue(clone, _scopeData)).Await().Value);
			RemoveArguments();
			return result;
		};
	}

	/// <summary>
	///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
	/// </summary>
	public Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> AsFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>()
	{
		AssertParameterCount(15);
		return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
			var clone = _contextObject.CloneForEdit();
			var result = (TResult)((_expression.Expression.GetValue(clone, _scopeData)).Await().Value);
			RemoveArguments();
			return result;
		};
	}

	/// <summary>
	///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
	/// </summary>
	public Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> AsFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>()
	{
		AssertParameterCount(16);
		return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
			var clone = _contextObject.CloneForEdit();
			var result = (TResult)((_expression.Expression.GetValue(clone, _scopeData)).Await().Value);
			RemoveArguments();
			return result;
		};
	}


	/// <summary>
	///		Creates a new c# Delegate that invokes the morestachio expression and returns its value cast into the TResult
	/// </summary>
	public Func<T0, Task<TResult>> AsAsyncFunc<T0, TResult>()
	{
		AssertParameterCount(1);
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
		AssertParameterCount(2);
		return async (arg1, arg2) =>
		{
			AddArguments(arg1, arg2);
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
		AssertParameterCount(3);
		return async (arg1, arg2, arg3) =>
		{
			AddArguments(arg1, arg2, arg3);
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
		AssertParameterCount(4);
		return async (arg1, arg2, arg3, arg4) =>
		{
			AddArguments(arg1, arg2, arg3, arg4);
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
		AssertParameterCount(5);
		return async (arg1, arg2, arg3, arg4, arg5) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5);
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
		AssertParameterCount(6);
		return async (arg1, arg2, arg3, arg4, arg5, arg6) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6);
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
		AssertParameterCount(7);
		return async (arg1, arg2, arg3, arg4, arg5, arg6, arg7) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
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
		AssertParameterCount(8);
		return async (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
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
		AssertParameterCount(9);
		return async (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
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
		AssertParameterCount(10);
		return async (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
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
		AssertParameterCount(11);
		return async (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
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
		AssertParameterCount(12);
		return async (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
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
		AssertParameterCount(13);
		return async (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
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
		AssertParameterCount(14);
		return async (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
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
		AssertParameterCount(15);
		return async (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
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
		AssertParameterCount(16);
		return async (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
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
			_expression.Expression.GetValue(clone, _scopeData).Await();
			RemoveArguments();
		};
	}

	/// <summary>
	///		Creates a new c# Delegate that invokes the morestachio expression
	/// </summary>
	public Action<T0, T1> AsAction<T0, T1>()
	{
		AssertParameterCount(1);
		return (arg1, arg2) =>
		{
			AddArguments(arg1, arg2);
			var clone = _contextObject.CloneForEdit();
			_expression.Expression.GetValue(clone, _scopeData).Await();
			RemoveArguments();
		};
	}

	/// <summary>
	///		Creates a new c# Delegate that invokes the morestachio expression
	/// </summary>
	public Action<T0, T1, T2> AsAction<T0, T1, T2>()
	{
		AssertParameterCount(2);
		return (arg1, arg2, arg3) =>
		{
			AddArguments(arg1, arg2, arg3);
			var clone = _contextObject.CloneForEdit();
			_expression.Expression.GetValue(clone, _scopeData).Await();
			RemoveArguments();
		};
	}

	/// <summary>
	///		Creates a new c# Delegate that invokes the morestachio expression
	/// </summary>
	public Action<T0, T1, T2, T3> AsAction<T0, T1, T2, T3>()
	{
		AssertParameterCount(3);
		return (arg1, arg2, arg3, arg4) =>
		{
			AddArguments(arg1, arg2, arg3, arg4);
			var clone = _contextObject.CloneForEdit();
			_expression.Expression.GetValue(clone, _scopeData).Await();
			RemoveArguments();
		};
	}

	/// <summary>
	///		Creates a new c# Delegate that invokes the morestachio expression
	/// </summary>
	public Action<T0, T1, T2, T3, T4> AsAction<T0, T1, T2, T3, T4>()
	{
		AssertParameterCount(4);
		return (arg1, arg2, arg3, arg4, arg5) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5);
			var clone = _contextObject.CloneForEdit();
			_expression.Expression.GetValue(clone, _scopeData).Await();
			RemoveArguments();
		};
	}

	/// <summary>
	///		Creates a new c# Delegate that invokes the morestachio expression
	/// </summary>
	public Action<T0, T1, T2, T3, T4, T5> AsAction<T0, T1, T2, T3, T4, T5>()
	{
		AssertParameterCount(5);
		return (arg1, arg2, arg3, arg4, arg5, arg6) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6);
			var clone = _contextObject.CloneForEdit();
			_expression.Expression.GetValue(clone, _scopeData).Await();
			RemoveArguments();
		};
	}

	/// <summary>
	///		Creates a new c# Delegate that invokes the morestachio expression
	/// </summary>
	public Action<T0, T1, T2, T3, T4, T5, T6> AsAction<T0, T1, T2, T3, T4, T5, T6>()
	{
		AssertParameterCount(6);
		return (arg1, arg2, arg3, arg4, arg5, arg6, arg7) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
			var clone = _contextObject.CloneForEdit();
			_expression.Expression.GetValue(clone, _scopeData).Await();
			RemoveArguments();
		};
	}

	/// <summary>
	///		Creates a new c# Delegate that invokes the morestachio expression
	/// </summary>
	public Action<T0, T1, T2, T3, T4, T5, T6, T7> AsAction<T0, T1, T2, T3, T4, T5, T6, T7>()
	{
		AssertParameterCount(7);
		return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
			var clone = _contextObject.CloneForEdit();
			_expression.Expression.GetValue(clone, _scopeData).Await();
			RemoveArguments();
		};
	}

	/// <summary>
	///		Creates a new c# Delegate that invokes the morestachio expression
	/// </summary>
	public Action<T0, T1, T2, T3, T4, T5, T6, T7, T8> AsAction<T0, T1, T2, T3, T4, T5, T6, T7, T8>()
	{
		AssertParameterCount(8);
		return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
			var clone = _contextObject.CloneForEdit();
			_expression.Expression.GetValue(clone, _scopeData).Await();
			RemoveArguments();
		};
	}

	/// <summary>
	///		Creates a new c# Delegate that invokes the morestachio expression
	/// </summary>
	public Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> AsAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>()
	{
		AssertParameterCount(9);
		return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
			var clone = _contextObject.CloneForEdit();
			_expression.Expression.GetValue(clone, _scopeData).Await();
			RemoveArguments();
		};
	}

	/// <summary>
	///		Creates a new c# Delegate that invokes the morestachio expression
	/// </summary>
	public Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> AsAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>()
	{
		AssertParameterCount(10);
		return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
			var clone = _contextObject.CloneForEdit();
			_expression.Expression.GetValue(clone, _scopeData).Await();
			RemoveArguments();
		};
	}

	/// <summary>
	///		Creates a new c# Delegate that invokes the morestachio expression
	/// </summary>
	public Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> AsAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>()
	{
		AssertParameterCount(11);
		return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
			var clone = _contextObject.CloneForEdit();
			_expression.Expression.GetValue(clone, _scopeData).Await();
			RemoveArguments();
		};
	}

	/// <summary>
	///		Creates a new c# Delegate that invokes the morestachio expression
	/// </summary>
	public Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> AsAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>()
	{
		AssertParameterCount(12);
		return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
			var clone = _contextObject.CloneForEdit();
			_expression.Expression.GetValue(clone, _scopeData).Await();
			RemoveArguments();
		};
	}

	/// <summary>
	///		Creates a new c# Delegate that invokes the morestachio expression
	/// </summary>
	public Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> AsAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>()
	{
		AssertParameterCount(13);
		return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
			var clone = _contextObject.CloneForEdit();
			_expression.Expression.GetValue(clone, _scopeData).Await();
			RemoveArguments();
		};
	}

	/// <summary>
	///		Creates a new c# Delegate that invokes the morestachio expression
	/// </summary>
	public Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> AsAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>()
	{
		AssertParameterCount(14);
		return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
			var clone = _contextObject.CloneForEdit();
			_expression.Expression.GetValue(clone, _scopeData).Await();
			RemoveArguments();
		};
	}

	/// <summary>
	///		Creates a new c# Delegate that invokes the morestachio expression
	/// </summary>
	public Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> AsAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>()
	{
		AssertParameterCount(15);
		return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16) =>
		{
			AddArguments(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
			var clone = _contextObject.CloneForEdit();
			_expression.Expression.GetValue(clone, _scopeData).Await();
			RemoveArguments();
		};
	}



	#endregion

	///// <summary>
	/////		Tries to reflect the matching AsAction or AsFunc or AsAsyncFunc method and invokes it
	///// </summary>
	///// <param name="delegateType"></param>
	///// <returns></returns>
	//public Delegate As(Type delegateType)
	//{
	//	//try
	//	//{
	//	//	var invokeMethod = delegateType.GetMethod(nameof(Action.Invoke));

	//	//	var parameters = invokeMethod.GetParameters()
	//	//		.Select((f, index) => SysExpression.Parameter(f.ParameterType, "arg" + index));
	//	//	var exp = SysExpression.Lambda(SysExpression.Call(invokeMethod, parameters));

	//	//	return exp.Compile();
	//	//}
	//	//catch (Exception e)
	//	//{
	//	//	Console.WriteLine(e);
	//	//	throw;
	//	//}


	//	var invokeMethod = delegateType.GetMethod(nameof(Action.Invoke));

	//	if (invokeMethod == null)
	//	{
	//		throw new InvalidOperationException($"The supplied type '{delegateType}' is nether a Action nor a Func delegate type");
	//	}

	//	var methodGenerics = invokeMethod.GetParameters();

	//	if (invokeMethod.ReturnParameter == null)
	//	{
	//		//action<> method
	//		var convAction = GetType().GetMethods()
	//			.Where(e => e.Name == nameof(AsAction))
	//			.FirstOrDefault(e => e.GetGenericArguments().Length == methodGenerics.Length);
	//		if (convAction == null)
	//		{
	//			throw new InvalidOperationException($"Could not find a matching converter func for '{delegateType}'");
	//		}

	//		return convAction.MakeGenericMethod(delegateType.GetGenericArguments()).Invoke(this, null) as Delegate;
	//	}
	//	//Func<> method
	//	//.Where(e => e.ReturnParameter?.Equals(invokeMethod.ReturnParameter) == true)
	//	var convFunc = GetType().GetMethods()
	//		.Where(e => e.Name == nameof(InvokeFunc))
	//		.FirstOrDefault(e => e.GetGenericArguments().Length == methodGenerics.Length + 1);
	//	if (convFunc == null)
	//	{
	//		convFunc = GetType().GetMethods()
	//			.Where(e => e.Name == nameof(AsAsyncFunc))
	//			.FirstOrDefault(e => e.GetGenericArguments().Length == methodGenerics.Length + 1);
	//		if (convFunc == null)
	//		{
	//			throw new InvalidOperationException($"Could not find a matching converter func for '{delegateType}'");
	//		}
	//	}

	//	return convFunc.CreateDelegate(delegateType);

	//	var types = delegateType.GetGenericArguments();
	//	//var makeGenericMethod = convFunc.MakeGenericMethod(types);
	//	return convFunc.Invoke(this, null) as Delegate;
	//}
}