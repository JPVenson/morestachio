using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using FileSystemPath = System.IO.Path;

namespace Morestachio.AspNetCore
{
	public static class MorestachioMiddlewareExtensions
	{
		public static IApplicationBuilder UseMorestachio(this IApplicationBuilder app,
														MorestachioMiddlewareOptions options)
		{
			app.UseMiddleware<MorestachioMiddleware>(options);
			return app;
		}
	}

	public class MorestachioMiddlewareBuilder
	{
		public MorestachioMiddlewareBuilder(string path)
		{
			Path = path;
			Templates = new List<IMorestachioTemplate>();
		}

		public string Path { get; }

		private List<IMorestachioTemplate> Templates { get; set; }

		public MorestachioMiddlewareBuilder Add(IMorestachioTemplate template)
		{
			Templates.Add(template);
			return this;
		}

		public MorestachioMiddlewareBuilder MapDirectory(string directory,
														Func<HttpContext, ValueTask<object>> dataFac = null)
		{
			foreach (var enumerateFile in Directory.EnumerateFiles(directory))
			{
				Templates.Add(new FileMorestachioTemplate(new FileInfo(enumerateFile),
					FileSystemPath.Combine(Path, FileSystemPath.GetFileName(enumerateFile)), dataFac));
			}

			return this;
		}

		public MorestachioMiddlewareBuilder Map(Action<MorestachioMiddlewareBuilder> fac, string path)
		{
			var subBuilder = new MorestachioMiddlewareBuilder(FileSystemPath.Combine(Path, path));
			fac(subBuilder);
			Templates.AddRange(subBuilder.Templates);
			return this;
		}
	}
}