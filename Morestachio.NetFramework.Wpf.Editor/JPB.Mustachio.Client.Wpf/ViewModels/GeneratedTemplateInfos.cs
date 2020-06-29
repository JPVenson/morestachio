using System;
using Morestachio;
using Morestachio.Document.Contracts;
using Morestachio.Framework;
using Morestachio.ParserErrors;

namespace JPB.Mustachio.Client.Wpf.ViewModels
{
	public class GeneratedTemplateInfos
	{
		public string Result { get; set; }
		public IDocumentItem InferredTemplateModel { get; set; }
		public IMorestachioError[] Errors { get; set; }

		public TimeSpan RenderTime { get; set; }
	}
}