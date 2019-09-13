using System.Collections.Generic;
using System.Threading.Tasks;
using Morestachio.Framework;

namespace Morestachio
{
	/// <summary>
	///		Removes the alias from the scope
	/// </summary>
	public class RemoveAliasDocumentItem : DocumentItemBase
	{
		private readonly string _aliasName;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="aliasName"></param>
		public RemoveAliasDocumentItem(string aliasName)
		{
			_aliasName = aliasName;
		}
		/// <inheritdoc />
		public override async Task<IEnumerable<DocumentItemExecution>> Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			scopeData.Alias.Remove(_aliasName);
			await Task.CompletedTask;
			return new DocumentItemExecution[0];
		}

		/// <inheritdoc />
		public override string Kind { get; } = "RemoveAlias";
	}
}