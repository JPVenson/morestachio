using System;
using System.Linq;
using System.Reflection;
using System.Text;
using JPB.Mustachio.Client.Wpf.Services;
using JPB.WPFBase.MVVM.ViewModel;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Morestachio.Formatter.Framework;

namespace JPB.Mustachio.Client.Wpf.ViewModels.Formatting
{
	public class FormatterViewModel : AsyncViewModelBase, IDisposable
	{
		protected TemplateServiceProvider Service { get; }

		public FormatterViewModel(TemplateServiceProvider service)
		{
			Service = service;
			Arguments = new ThreadSaveObservableCollection<FormatterArgument>();
			Service.CreateFormatter += _service_CreateFormatter;
		}

		private void _service_CreateFormatter(object sender, System.Collections.Generic.List<FormatterInfo> e)
		{
			e.Add(new FormatterInfo(Name, "Dynamic Code", InputType, OutputType, new InputDescription[0], "", CreateFunction));
		}

		private string _code;
		private string _name;
		public ThreadSaveObservableCollection<FormatterArgument> Arguments { get; set; }
		private bool _isExternal;
		private Type _inputType;
		private Type _outputType;

		public virtual MethodInfo CreateFunction()
		{
			ParseCode();
			var method = CSharpScript.Create(FullCode);
			var scriptRunner = method.CreateDelegate();
			return scriptRunner.Method;
		}

		public Type OutputType
		{
			get { return _outputType; }
			set
			{
				SendPropertyChanging(() => OutputType);
				_outputType = value;
				ParseCode();
				SendPropertyChanged(() => OutputType);
			}
		}

		public Type InputType
		{
			get { return _inputType; }
			set
			{
				SendPropertyChanging(() => InputType);
				_inputType = value;
				ParseCode();
				SendPropertyChanged(() => InputType);
			}
		}

		public bool IsExternal
		{
			get { return _isExternal; }
			set
			{
				SendPropertyChanging(() => IsExternal);
				_isExternal = value;
				ParseCode();
				SendPropertyChanged(() => IsExternal);
			}
		}

		public string Name
		{
			get { return _name; }
			set
			{
				SendPropertyChanging(() => Name);
				_name = value;
				ParseCode();
				SendPropertyChanged(() => Name);
			}
		}

		public string Code
		{
			get { return _code; }
			set
			{
				SendPropertyChanging(() => Code);
				_code = value;
				ParseCode();
				SendPropertyChanged(() => Code);
			}
		}

		private string _fullCode;
		public string FullCode
		{
			get { return _fullCode; }
			set
			{
				SendPropertyChanging(() => FullCode);
				_fullCode = value;
				SendPropertyChanged(() => FullCode);
			}
		}

		public virtual void ParseCode()
		{
			var sb = new StringBuilder();
			sb.Append("public " + OutputType.ToString() + " func(");
			if (Arguments.Any())
			{
				sb.Append(Arguments.Select(e => e.InputType.ToString() + " " + e.Name).Aggregate((e, f) => $"{e},{f}"));
			}

			sb.AppendLine();
			sb.Append("{");
			sb.Append(Code);
			sb.Append("}");
			FullCode = sb.ToString();
		}

		public virtual void Dispose()
		{
			Service.CreateFormatter -= _service_CreateFormatter;
		}
	}
}