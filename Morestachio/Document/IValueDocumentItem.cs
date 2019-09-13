using System.Threading.Tasks;
using Morestachio.Framework;

namespace Morestachio
{
	/// <summary>
	///		Defines a Part in the Template that provides a direct access to 
	/// </summary>
	public interface IValueDocumentItem
	{
		/// <summary>
		///		Traverses the path down
		/// </summary>
		Task<ContextObject> GetValue(ContextObject context,
			ScopeData scopeData);
	}
}