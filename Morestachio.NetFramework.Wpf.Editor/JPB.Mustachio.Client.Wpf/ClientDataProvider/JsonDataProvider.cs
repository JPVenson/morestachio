using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.Mustachio.Client.Contacts.Contracts;
using JPB.WPFBase.MVVM.ViewModel;
using Newtonsoft.Json;

namespace JPB.Mustachio.Client.Wpf.ClientDataProvider
{
	public class JsonDataProvider : AsyncViewModelBase, IDataSourceProvider
	{
		public JsonDataProvider()
		{
			JsonText = @"{ data: { 
	test: ""ttt""
	}
}";
		}
		private string _jsonText;

		public object JsonSyntax
		{
			get
			{
				return ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("json");
			}
		}

		public string JsonText
		{
			get { return _jsonText; }
			set
			{
				SendPropertyChanging(() => JsonText);
				_jsonText = value;
				SendPropertyChanged(() => JsonText);
			}
		}

		public string Name { get; } = "Json Data";

		public async Task<object> Fetch()
		{
			await Task.CompletedTask;
			return JsonConvert.DeserializeObject(JsonText);
		}

		public IDictionary<string, string> StoreProviderData()
		{
			return new Dictionary<string, string>()
			{
				{"Data", JsonText }
			};
		}

		public void StoreProviderData(IDictionary<string, string> data)
		{
			if (data.TryGetValue("Data", out var dataJson))
			{
				JsonText = dataJson;
			} 
		}
	}
}
