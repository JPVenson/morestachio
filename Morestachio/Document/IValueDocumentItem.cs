using System.Threading.Tasks;
using System.Xml.Serialization;
using Morestachio.Framework;

namespace Morestachio.Document
{
	/// <summary>
	///		Defines a Part in the Template that provides a direct access to 
	/// </summary>
	public interface IValueDocumentItem : IXmlSerializable
	{
		/// <summary>
		///		Traverses the path down
		/// </summary>
		Task<ContextObject> GetValue(ContextObject context,
			ScopeData scopeData);
	}
}