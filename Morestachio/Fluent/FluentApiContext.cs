namespace Morestachio.Fluent
{
	/// <summary>
	///		The context that is used to work with the <see cref="MorestachioDocumentFluentApi"/>
	/// </summary>
	public class FluentApiContext
	{
		/// <summary>
		///		Creates a new Context
		/// </summary>
		/// <param name="rootNode"></param>
		/// <param name="currentNode"></param>
		public FluentApiContext(MorestachioNode rootNode, MorestachioNode currentNode, ParserOptions options)
		{
			RootNode = rootNode;
			CurrentNode = currentNode;
			Options = options;
			OperationStatus = true;
		}

		/// <summary>
		///		The options used to generate the document
		/// </summary>
		public ParserOptions Options { get; }

		/// <summary>
		///		The top most element
		/// </summary>
		public MorestachioNode RootNode { get; }

		/// <summary>
		///		The current node
		/// </summary>
		public MorestachioNode CurrentNode { get; set; }

		/// <summary>
		///		Gets or Sets the state of the last executed operation
		/// </summary>
		public bool OperationStatus { get; set; }
		
		/// <summary>
		///		Creates a new <see cref="FluentApiContext"/> and copies all data from this instance to the new one
		/// </summary>
		/// <returns></returns>
		public FluentApiContext Copy()
		{
			return new FluentApiContext(RootNode, CurrentNode, Options);
		}
	}
}