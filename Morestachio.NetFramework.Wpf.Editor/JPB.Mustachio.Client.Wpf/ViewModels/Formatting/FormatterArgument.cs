using System;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.Mustachio.Client.Wpf.ViewModels.Formatting
{
	public class FormatterArgument : AsyncViewModelBase
	{
		private Type _inputType;
		private string _name;

		public Type InputType
		{
			get { return _inputType; }
			set
			{
				SendPropertyChanging(() => InputType);
				_inputType = value;
				SendPropertyChanged(() => InputType);
			}
		}

		public string Name
		{
			get { return _name; }
			set
			{
				SendPropertyChanging(() => Name);
				_name = value;
				SendPropertyChanged(() => Name);
			}
		}
	}
}