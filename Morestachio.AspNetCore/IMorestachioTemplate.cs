using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Morestachio.Rendering;

namespace Morestachio.AspNetCore
{
	public interface IMorestachioTemplate
	{
		bool Matches(HttpContext context);

		ValueTask<IRenderer> GetTemplate(HttpContext context);
		ValueTask<object> GetData(HttpContext context);
	}
}