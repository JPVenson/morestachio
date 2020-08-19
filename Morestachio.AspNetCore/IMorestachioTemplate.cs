using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Morestachio.AspNetCore
{
	public interface IMorestachioTemplate
	{
		bool Matches(HttpContext context);

		ValueTask<MorestachioDocumentInfo> GetTemplate(HttpContext context);
		ValueTask<object> GetData(HttpContext context);
	}
}