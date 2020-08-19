using System;
using Morestachio.Document.Contracts;

namespace Morestachio.Tests.DocTree
{
	public interface IDocumentSerializerStrategy
	{
		string SerializeToText(IDocumentItem obj);
		IDocumentItem DeSerializeToText(string text, Type expectedType);
	}
}