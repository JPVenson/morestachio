using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JPB.Mustachio.Client.Contacts.Contracts;
using JPB.WPFBase.MVVM.ViewModel;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace JPB.Mustachio.Client.Data.CSharp.DataSourceProvider
{
	public class CSharpCompilerViewModel : AsyncViewModelBase, IDataSourceProvider
	{
		public CSharpCompilerViewModel()
		{
			CSharpCode = "var anyTestObject = new Dictionary<string, object>();" +
						 Environment.NewLine +
						 "return anyTestObject;";
		}

		private string _cSharpCode;

		public object CSharpSyntax
		{
			get
			{
				return ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("C#");
			}
		}

		public string CSharpCode
		{
			get { return _cSharpCode; }
			set
			{
				SendPropertyChanging(() => CSharpCode);
				_cSharpCode = value;
				SendPropertyChanged(() => CSharpCode);
			}
		}

		public string Name { get; } = "C# Data Provider";
		public async Task<object> Fetch()
		{
			try
			{
				return await CSharpScript.EvaluateAsync(CSharpCode, ScriptOptions.Default.WithImports("System",
					"System.Collections.Generic",
					"System.Linq",
					"System.Text",
					"System.Collections"));
			}
			catch (Exception e)
			{
				//TODO Fetch compile errors and display
				return null;
			}
		}

		public IDictionary<string, string> StoreProviderData()
		{
			return new Dictionary<string, string>()
			{
				{"Code", CSharpCode}
			};
		}

		public void StoreProviderData(IDictionary<string, string> data)
		{
			CSharpCode = data["Code"];
		}
	}
}
