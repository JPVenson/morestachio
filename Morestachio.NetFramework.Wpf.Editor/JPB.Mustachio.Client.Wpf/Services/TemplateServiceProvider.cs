using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Documents;
using JPB.Mustachio.Client.Contacts.Contracts;
using Morestachio.Formatter;
using Morestachio.Formatter.Framework;

namespace JPB.Mustachio.Client.Wpf.Services
{
	public interface IFormatterInfo
	{
		MorestachioFormatterModel CreateFormatter();
	}

	public class FormatterInfo : IFormatterInfo
	{
		public FormatterInfo(string name, string description, Type inputType, Type outputType,
			InputDescription[] inputDescriptions, string outputHint, Func<MethodInfo> delegateFactory)
		{
			Name = name;
			Description = description;
			InputType = inputType;
			OutputType = outputType;
			InputDescriptions = inputDescriptions;
			OutputHint = outputHint;
			DelegateFactory = delegateFactory;
		}

		public MorestachioFormatterModel CreateFormatter()
		{
			return new MorestachioFormatterModel(Name, Description, InputType,
				OutputType,
				InputDescriptions,
				OutputHint,
				DelegateFactory(), new MultiFormatterInfoCollection(new List<MultiFormatterInfo>()));
		}

		public string Name { get; }
		public string Description { get; }
		public Type InputType { get; }
		public Type OutputType { get; }
		public InputDescription[] InputDescriptions { get; }
		public string OutputHint { get; }
		public Func<MethodInfo> DelegateFactory { get; }
	}

	public class TemplateServiceProvider
	{
		public TemplateServiceProvider()
		{
			Formatter = new List<IFormatterInfo>();
		}

		public event EventHandler<IDataSourceProvider> DataProviderChanged;
		public event EventHandler<string> TemplateChanged;

		public virtual void OnDataProviderChanged(IDataSourceProvider e)
		{
			LastProvider = e;
			DataProviderChanged?.Invoke(this, e);
		}

		public virtual void OnTemplateChanged(string e)
		{
			LastTemplate = e;
			TemplateChanged?.Invoke(this, e);
		}

		public IDataSourceProvider LastProvider { get; set; }
		public string LastTemplate { get; set; }
		public IList<IFormatterInfo> Formatter { get; set; }

		public event EventHandler<List<FormatterInfo>> CreateFormatter;

		public IEnumerable<FormatterInfo> ObtainFormatters()
		{
			var formatter = new List<FormatterInfo>();
			OnCreateFormatter(formatter);
			return formatter;
		}

		protected virtual void OnCreateFormatter(List<FormatterInfo> e)
		{
			CreateFormatter?.Invoke(this, e);
		}
	}
}