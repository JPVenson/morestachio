using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.Mustachio.Client.Wpf.Core.ViewModels.Formatting
{
	public class FormatterGroup : ViewModelBase
	{
		public FormatterGroup()
		{
			Formatters = new ThreadSaveObservableCollection<FormatterViewModel>();
		}

		private string _name;

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

		public ThreadSaveObservableCollection<FormatterViewModel> Formatters { get; set; }
	}
}