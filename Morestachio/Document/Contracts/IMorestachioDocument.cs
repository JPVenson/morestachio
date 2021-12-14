using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Morestachio.Document.Contracts;

internal interface IMorestachioDocument : IDocumentItem, IXmlSerializable, ISerializable
{

}