using System.Collections.Generic;

namespace JPB.Mustachio.Client.Wpf.ViewModels
{
	public class StoreModel
	{
		public string MustachioTemplate { get; set; }
		public string CodeProviderType { get; set; }
		public IDictionary<string, string> CodeProviderData { get; set; }
	}
}