using System.Threading.Tasks;
using Morestachio.Document;

namespace Morestachio.Framework.Expression
{
	public interface IExpression
	{
		CharacterLocation Location { get; set; }
		Task<ContextObject> GetValue(ContextObject contextObject, ScopeData scopeData);
	}
}