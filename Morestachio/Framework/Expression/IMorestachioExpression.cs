using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
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
	///		Defines an Morestachio expression that contains dynamic data
	/// </summary>
	public interface IMorestachioExpression : ISerializable, IXmlSerializable, IEquatable<IMorestachioExpression>
	{
		/// <summary>
		///		Where in the original template was this expression located
		/// </summary>
		CharacterLocation Location { get; }

		/// <summary>
		///		The method for obtaining the Value
		/// </summary>
		/// <param name="contextObject"></param>
		/// <param name="scopeData"></param>
		/// <returns></returns>
		ContextObjectPromise GetValue(ContextObject contextObject, ScopeData scopeData);

		/// <summary>
		///		Compiles the Expression for later faster execution
		/// </summary>
		/// <returns></returns>
		CompiledExpression Compile();

		/// <summary>
		///		Visits this Expression
		/// </summary>
		/// <param name="visitor"></param>
		void Accept(IMorestachioExpressionVisitor visitor);
	}
}