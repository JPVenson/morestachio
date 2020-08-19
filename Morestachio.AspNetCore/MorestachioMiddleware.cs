using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Morestachio.Document.Items;

namespace Morestachio.AspNetCore
{
	public class MorestachioMiddleware
	{
		private readonly MorestachioMiddlewareOptions _options;

		public MorestachioMiddleware(MorestachioMiddlewareOptions options)
		{
			_options = options;
		}

		public async Task Invoke(HttpContext context)
		{
			var template = _options.Templates.FirstOrDefault(e => e.Matches(context));
			if (template == null)
			{
				return;
			}

			var data = template.GetData(context);
			var templateValue = template.GetTemplate(context);
			var result = await (await templateValue).CreateAsync(await data);
			context.Response.Body = result.Stream;
			context.Response.ContentLength = result.Stream.Length;
			context.Response.StatusCode = StatusCodes.Status200OK;
		}
	}
}
