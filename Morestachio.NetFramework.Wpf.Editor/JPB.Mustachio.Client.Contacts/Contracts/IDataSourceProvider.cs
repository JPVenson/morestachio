using System.Collections.Generic;
using System.Threading.Tasks;

namespace JPB.Mustachio.Client.Contacts.Contracts
{
	public interface IDataSourceProvider
	{
		string Name { get; }
		Task<object> Fetch();
		IDictionary<string, string> StoreProviderData();
		void StoreProviderData(IDictionary<string, string> data);
	}
}