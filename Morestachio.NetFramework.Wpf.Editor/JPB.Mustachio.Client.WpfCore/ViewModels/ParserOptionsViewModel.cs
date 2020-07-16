using System;
using JPB.Mustachio.Client.Wpf.Core.Services;
using JPB.WPFBase.MVVM.ViewModel;
using Morestachio;

namespace JPB.Mustachio.Client.Wpf.Core.ViewModels
{
	public interface IParserOptions
	{
		string NullSubstitute { get; set; }
		long MaxSize { get; set; }
		bool DisableContentEscaping { get; set; }
		bool EnableExtendedMissingPathOutput { get; set; }
		PartialStackOverflowBehavior PartialStackOverflowBehavior { get; set; }
		uint PartialStackSize { get; set; }
		TimeSpan Timeout { get; set; }
	}

	public class ParserOptionsViewModel : ViewModelBase, IParserOptions
	{
		private readonly TemplateServiceProvider _service;

		public ParserOptionsViewModel(TemplateServiceProvider service)
		{
			_service = service;
			_service.ParserOptions = this;
			var parserOptions = new ParserOptions("");
			NullSubstitute = parserOptions.Null;
			MaxSize = parserOptions.MaxSize;
			DisableContentEscaping = parserOptions.DisableContentEscaping;
			PartialStackSize = parserOptions.PartialStackSize;
			PartialStackOverflowBehavior = parserOptions.StackOverflowBehavior;
			Timeout = parserOptions.Timeout;
		}

		private TimeSpan _timeout;
		private uint _partialStackSize;
		private PartialStackOverflowBehavior _partialStackOverflowBehavior;
		private bool _disableContentEscaping;
		private long _maxSize;
		private string _nullSubstitute;
		private bool _enableExtendedMissingPathOutput;

		public bool EnableExtendedMissingPathOutput
		{
			get { return _enableExtendedMissingPathOutput; }
			set
			{
				SendPropertyChanging(() => EnableExtendedMissingPathOutput);
				_enableExtendedMissingPathOutput = value;
				SendPropertyChanged(() => EnableExtendedMissingPathOutput);
			}
		}

		public string NullSubstitute
		{
			get { return _nullSubstitute; }
			set
			{
				SendPropertyChanging(() => NullSubstitute);
				_nullSubstitute = value;
				SendPropertyChanged(() => NullSubstitute);
			}
		}

		public long MaxSize
		{
			get { return _maxSize; }
			set
			{
				SendPropertyChanging(() => MaxSize);
				_maxSize = value;
				SendPropertyChanged(() => MaxSize);
			}
		}

		public bool DisableContentEscaping
		{
			get { return _disableContentEscaping; }
			set
			{
				SendPropertyChanging(() => DisableContentEscaping);
				_disableContentEscaping = value;
				SendPropertyChanged(() => DisableContentEscaping);
			}
		}

		public PartialStackOverflowBehavior PartialStackOverflowBehavior
		{
			get { return _partialStackOverflowBehavior; }
			set
			{
				SendPropertyChanging(() => PartialStackOverflowBehavior);
				_partialStackOverflowBehavior = value;
				SendPropertyChanged(() => PartialStackOverflowBehavior);
			}
		}

		public uint PartialStackSize
		{
			get { return _partialStackSize; }
			set
			{
				SendPropertyChanging(() => PartialStackSize);
				_partialStackSize = value;
				SendPropertyChanged(() => PartialStackSize);
			}
		}

		public TimeSpan Timeout
		{
			get { return _timeout; }
			set
			{
				SendPropertyChanging(() => Timeout);
				_timeout = value;
				SendPropertyChanged(() => Timeout);
			}
		}
	}
}
