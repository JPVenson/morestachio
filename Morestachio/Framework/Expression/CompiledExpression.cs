using System.Threading.Tasks;
using Morestachio.Document;
using Morestachio.Framework.Context;
#if ValueTask
using ContextObjectPromise = System.Threading.Tasks.ValueTask<Morestachio.Framework.Context.ContextObject>;
#else
using ContextObjectPromise = System.Threading.Tasks.Task<Morestachio.Framework.Context.ContextObject>;
#endif
namespace Morestachio.Framework.Expression
{
	/// <summary>
	///		Defines a Delegate that contains the logic for an expression
	/// </summary>
	/// <param name="contextObject"></param>
	/// <param name="scopeData"></param>
	/// <returns></returns>
	public delegate ContextObjectPromise CompiledExpression(ContextObject contextObject, ScopeData scopeData);
}