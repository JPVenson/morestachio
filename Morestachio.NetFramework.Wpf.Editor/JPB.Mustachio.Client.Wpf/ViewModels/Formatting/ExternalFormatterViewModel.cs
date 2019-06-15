using System;
using System.Reflection;
using JPB.Mustachio.Client.Wpf.Services;

namespace JPB.Mustachio.Client.Wpf.ViewModels.Formatting
{
	public class ExternalFormatterViewModel : FormatterViewModel, IDisposable
	{
		private readonly MethodInfo _methodInfo;

		public ExternalFormatterViewModel(TemplateServiceProvider service, string file, MethodInfo methodInfo) : base(service)
		{
			_methodInfo = methodInfo;
			File = file;
			IsExternal = true;
		}

		public string File { get; private set; }

		public override void ParseCode()
		{
		}

		public override MethodInfo CreateFunction()
		{
			return _methodInfo;
		}
	}
}