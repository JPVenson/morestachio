using System.Threading.Tasks;
using Morestachio.Document;
using Morestachio.Framework.Context;

namespace Morestachio.Framework.Expression
{
	/// <summary>
	///		Defines a Delegate that contains the logic for an expression
	/// </summary>
	/// <param name="contextObject"></param>
	/// <param name="scopeData"></param>
	/// <returns></returns>
	public delegate Task<ContextObject> CompiledExpression(ContextObject contextObject, ScopeData scopeData);
}