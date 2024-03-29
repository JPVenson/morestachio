﻿using System.Xml.Serialization;
using Morestachio.Document;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression.Visitors;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Framework.Expression;

/// <summary>
///		Defines an Morestachio expression that contains dynamic data
/// </summary>
public interface IMorestachioExpression : ISerializable, IXmlSerializable, IEquatable<IMorestachioExpression>
{
	/// <summary>
	///		Where in the original template was this expression located
	/// </summary>
	TextRange Location { get; }

	/// <summary>
	///		The method for obtaining the Value
	/// </summary>
	/// <param name="contextObject"></param>
	/// <param name="scopeData"></param>
	/// <returns></returns>
	ContextObjectPromise GetValue(ContextObject contextObject, ScopeData scopeData);

	///  <summary>
	/// 		Compiles the Expression for later faster execution
	///  </summary>
	///  <param name="parserOptions"></param>
	///  <returns></returns>
	CompiledExpression Compile(ParserOptions parserOptions);

	/// <summary>
	///		Visits this Expression
	/// </summary>
	/// <param name="visitor"></param>
	void Accept(IMorestachioExpressionVisitor visitor);

	/// <summary>
	///		Should return true if the expected value is completely evaluable at compile time 
	/// </summary>
	bool IsCompileTimeEval();

	/// <summary>
	///		When <see cref="IsCompileTimeEval"/> returns true this is called to obtain the value
	/// </summary>
	object GetCompileTimeValue();
}