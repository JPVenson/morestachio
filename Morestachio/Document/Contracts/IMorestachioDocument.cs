using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Morestachio.Document.Contracts;

/// <summary>
///		Defines the base document types that are fully compatible with all framework functions such as serialization 
/// </summary>
public interface IMorestachioDocument : IDocumentItem, IXmlSerializable, ISerializable
{
}