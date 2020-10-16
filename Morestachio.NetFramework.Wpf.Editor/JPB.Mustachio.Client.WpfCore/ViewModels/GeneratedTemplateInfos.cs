using System;
using Morestachio.Document.Contracts;
using Morestachio.Parsing.ParserErrors;

namespace JPB.Mustachio.Client.Wpf.Core.ViewModels
{
	public class GeneratedTemplateInfos
	{
		public string Result { get; set; }
		public IDocumentItem InferredTemplateModel { get; set; }
		public IMorestachioError[] Errors { get; set; }

		public TimeSpan RenderTime { get; set; }
	}
}