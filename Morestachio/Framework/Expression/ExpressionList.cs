using System.Threading.Tasks;
using Morestachio.Framework.Expression.Renderer;

namespace Morestachio.Framework.Expression
{
	public class ExpressionList : IExpression
	{
		public IExpression[] Expressions { get; }

		public ExpressionList(IExpression[] expressions)
		{
			Expressions = expressions;
		}

		public CharacterLocation Location { get; set; }
		public async Task<ContextObject> GetValue(ContextObject contextObject, ScopeData scopeData)
		{
			contextObject = contextObject.CloneForEdit();
			foreach (var expression in Expressions)
			{
				contextObject = await expression.GetValue(contextObject, scopeData);
			}

			return contextObject;
		}

		public override string ToString()
		{
			return ExpressionRenderer.RenderExpression(this).ToString();
		}
	}
}