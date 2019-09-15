using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Morestachio.Framework;

namespace Morestachio.Document
{
	/// <summary>
	///		Removes the alias from the scope
	/// </summary>
	public class RemoveAliasDocumentItem : ValueDocumentItemBase
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal RemoveAliasDocumentItem()
		{

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="aliasName"></param>
		public RemoveAliasDocumentItem(string aliasName)
		{
			Value = aliasName;
		}

		/// <inheritdoc />
		public override async Task<IEnumerable<DocumentItemExecution>> Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			scopeData.Alias.Remove(Value);
			await Task.CompletedTask;
			return new DocumentItemExecution[0];
		}

		/// <inheritdoc />
		public override string Kind { get; } = "RemoveAlias";
	}
}