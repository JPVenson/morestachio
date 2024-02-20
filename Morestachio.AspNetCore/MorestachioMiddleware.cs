using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Morestachio.Framework.IO.SingleStream;
using Morestachio.Rendering;

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
			var templateValue = await template.GetTemplate(context);

			var result = await templateValue.RenderAsync(await data,
				new ByteCounterStream(context.Response.Body, 2042, true, templateValue.ParserOptions));
			context.Response.ContentLength = result.Stream.BytesWritten;
			context.Response.StatusCode = StatusCodes.Status200OK;
		}
	}
}