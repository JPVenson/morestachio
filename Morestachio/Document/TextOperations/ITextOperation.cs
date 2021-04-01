using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Morestachio.Document.TextOperations
{
	/// <summary>
	///		The base interface for all TextOperations
	/// </summary>
	public interface ITextOperation: IXmlSerializable, ISerializable
	{
		/// <summary>
		///		The kind of text operation
		/// </summary>
		TextOperationTypes TextOperationType { get; }
		
		/// <summary>
		///		Should this text edit only applied once or until it is explicitly removed from the stack
		/// </summary>
		bool TransientEdit { get; }

		/// <summary>
		///		Does this text operation modify the next content element or should it be called immediately 
		/// </summary>
		bool IsModificator { get; }

		/// <summary>
		///		Executes the Text Operation
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		string Apply(string value);

	}
}