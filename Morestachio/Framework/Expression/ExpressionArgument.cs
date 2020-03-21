using System.Threading.Tasks;
using Morestachio.Framework.Expression.Renderer;

namespace Morestachio.Framework.Expression
{
	public class ExpressionArgument : IExpression
	{
		public ExpressionArgument(CharacterLocation location)
		{
			Location = location;
		}
		public string Name { get; set; }
		public IExpression Expression { get; set; }
		public CharacterLocation Location { get; set; }
		public async Task<ContextObject> GetValue(ContextObject contextObject, ScopeData scopeData)
		{
			return await Expression.GetValue(contextObject, scopeData);
		}

		public override string ToString()
		{
			return ExpressionRenderer.RenderExpression(Expression).ToString();
		}
	}
}