using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.Mustachio.Client.Wpf.Services;
using JPB.WPFBase.MVVM.DelegateCommand;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.Mustachio.Client.Wpf.ViewModels
{
	public class TemplateEditorViewModel : AsyncViewModelBase
	{
		private readonly TemplateServiceProvider _templateServiceProvider;

		public TemplateEditorViewModel(TemplateServiceProvider templateServiceProvider)
		{
			_templateServiceProvider = templateServiceProvider;
			TemplateChangedCommand = new DelegateCommand(TemplateChangedExecute, CanTemplateChangedExecute);
		}

		public event EventHandler<string> TemplateChanged;

		public DelegateCommand TemplateChangedCommand { get; private set; }

		private void TemplateChangedExecute(object sender)
		{
			OnTemplateChanged(Template);
		}

		private bool CanTemplateChangedExecute(object sender)
		{
			return true;
		}

		private string _template;

		public string Template
		{
			get { return _template; }
			set
			{
				SendPropertyChanging(() => Template);
				_template = value;
				_templateServiceProvider.OnTemplateChanged(value);
				SendPropertyChanged(() => Template);
			}
		}

		protected virtual void OnTemplateChanged(string e)
		{
			TemplateChanged?.Invoke(this, e);
		}
	}
}
