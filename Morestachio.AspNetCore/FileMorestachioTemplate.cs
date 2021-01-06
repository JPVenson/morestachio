using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Morestachio.Helper;

namespace Morestachio.AspNetCore
{
	public class FileMorestachioTemplate : CachedMorestachioTemplate
	{
		public FileInfo File { get; }
		public string Path { get; }
		public Func<HttpContext, ValueTask<object>> DataFac { get; }

		public FileMorestachioTemplate(FileInfo file, string path, Func<HttpContext, ValueTask<object>> dataFac = null)
		{
			File = file;
			Path = path;
			DataFac = dataFac;
		}

		public override bool Matches(HttpContext context)
		{
			return context.Request.Path.Equals(Path, StringComparison.CurrentCultureIgnoreCase);
		}

		public override ValueTask<MorestachioDocumentInfo> GetTemplateCore(HttpContext context)
		{
			return new ValueTask<MorestachioDocumentInfo>(Parser.ParseWithOptions(CreateOptions(
				File.OpenRead().Stringify(true, Encoding.UTF8),
				Encoding.UTF8)));
		}

		public override async ValueTask<object> GetDataCore(HttpContext context)
		{
			if (DataFac == null)
			{
				return null;
			}

			return await DataFac(context);
		}
	}
}