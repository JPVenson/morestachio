using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Morestachio.Document;
using Morestachio.Document.Contracts;
using Morestachio.Document.TextOperations;
using Morestachio.Document.Visitor;
using Morestachio.Framework.Expression;

namespace Morestachio.Fluent
{
	public static class MorestachioDocumentFluentApiExtensions
	{
		/// <summary>
		///		Adds a new <see cref="ContentDocumentItem"/>.
		/// </summary>
		public static MorestachioDocumentFluentApi AddContent(this MorestachioDocumentFluentApi api, string content)
		{
			return api.AddChild(f => new ContentDocumentItem(content));
		}

		/// <summary>
		///		Adds a new <see cref="DoLoopDocumentItem"/> and enters it.
		/// </summary>
		public static MorestachioDocumentFluentApi AddDoLoopAndEnter(this MorestachioDocumentFluentApi api,
			Func<MorestachioExpressionBuilderBaseRootApi, IMorestachioExpression> condition)
		{
			return api.AddChildAndEnter(builder => new DoLoopDocumentItem(condition(builder)));
		}

		/// <summary>
		///		Adds a new <see cref="EachDocumentItem"/> and enters it.
		/// </summary>
		public static MorestachioDocumentFluentApi AddEachLoopAndEnter(this MorestachioDocumentFluentApi api,
			Func<MorestachioExpressionBuilderBaseRootApi, IMorestachioExpression> condition)
		{
			return api.AddChildAndEnter(builder => new EachDocumentItem(condition(builder)));
		}

		/// <summary>
		///		Adds a new <see cref="ElseExpressionScopeDocumentItem"/> and enters it.
		/// </summary>
		public static MorestachioDocumentFluentApi AddElseAndEnter(this MorestachioDocumentFluentApi api)
		{
			return api
				.AddChildAndEnter(builder => new ElseExpressionScopeDocumentItem());
		}

		/// <summary>
		///		Adds a new <see cref="ElseExpressionScopeDocumentItem"/> and enters it.
		/// </summary>
		public static MorestachioDocumentFluentApi AddGlobalVariable(this MorestachioDocumentFluentApi api,
			string name,
			Func<MorestachioExpressionBuilderBaseRootApi, IMorestachioExpression> condition)
		{
			return api.AddChild(builder => new EvaluateVariableDocumentItem(name, condition(builder)));
		}

		/// <summary>
		///		Adds a new <see cref="ExpressionScopeDocumentItem"/> and enters it.
		/// </summary>
		public static MorestachioDocumentFluentApi AddScopeAndEnter(this MorestachioDocumentFluentApi api,
			Func<MorestachioExpressionBuilderBaseRootApi, IMorestachioExpression> condition)
		{
			return api.AddChildAndEnter(builder => new ExpressionScopeDocumentItem(condition(builder)));
		}

		/// <summary>
		///		Adds a new <see cref="IfExpressionScopeDocumentItem"/> and enters it.
		/// </summary>
		public static MorestachioDocumentFluentApi AddIfAndEnter(this MorestachioDocumentFluentApi api,
			Func<MorestachioExpressionBuilderBaseRootApi, IMorestachioExpression> condition)
		{
			return api.AddChildAndEnter(builder => new IfExpressionScopeDocumentItem(condition(builder)));
		}

		/// <summary>
		///		Adds a new <see cref="IfNotExpressionScopeDocumentItem"/> and enters it.
		/// </summary>
		public static MorestachioDocumentFluentApi AddInvertedIfAndEnter(this MorestachioDocumentFluentApi api,
			Func<MorestachioExpressionBuilderBaseRootApi, IMorestachioExpression> condition)
		{
			return api.AddChildAndEnter(builder => new IfNotExpressionScopeDocumentItem(condition(builder)));
		}

		/// <summary>
		///		Adds a new <see cref="InvertedExpressionScopeDocumentItem"/> and enters it.
		/// </summary>
		public static MorestachioDocumentFluentApi AddInvertedScopeAndEnter(this MorestachioDocumentFluentApi api,
			Func<MorestachioExpressionBuilderBaseRootApi, IMorestachioExpression> condition)
		{
			return api.AddChildAndEnter(builder => new InvertedExpressionScopeDocumentItem(condition(builder)));
		}

		/// <summary>
		///		Adds a new <see cref="PartialDocumentItem"/> and enters it.
		/// </summary>
		public static MorestachioDocumentFluentApi AddPartial(this MorestachioDocumentFluentApi api,
			string name,
			Func<MorestachioDocumentFluentApi, MorestachioDocumentFluentApi> factory)
		{
			return api.AddChild(builder => new PartialDocumentItem(name,
				factory(new MorestachioDocumentFluentApi(new MorestachioDocumentInfo(api.Context.Options,
					new MorestachioDocument()))).Context.RootNode.Item));
		}

		/// <summary>
		///		Adds a new <see cref="PathDocumentItem"/>
		/// </summary>
		public static MorestachioDocumentFluentApi AddPath(this MorestachioDocumentFluentApi api,
			Func<MorestachioExpressionBuilderBaseRootApi, IMorestachioExpression> condition,
			bool escapeValue = false)
		{
			return api.AddChild(builder => new PathDocumentItem(condition(builder), escapeValue));
		}

		/// <summary>
		///		Adds a new <see cref="RenderPartialDocumentItem"/>
		/// </summary>
		public static MorestachioDocumentFluentApi AddInclude(this MorestachioDocumentFluentApi api,
			string name,
			Func<MorestachioExpressionBuilderBaseRootApi, IMorestachioExpression> condition = null)
		{
			return api.AddChild(builder => new RenderPartialDocumentItem(name, condition?.Invoke(builder)));
		}

		/// <summary>
		///		Adds a new <see cref="TextEditDocumentItem"/>
		/// </summary>
		public static MorestachioDocumentFluentApi AddTextModification(this MorestachioDocumentFluentApi api,
			ITextOperation textOperation)
		{
			return api.AddChild(builder => new TextEditDocumentItem(textOperation));
		}

		/// <summary>
		///		Adds a new <see cref="WhileLoopDocumentItem"/>
		/// </summary>
		public static MorestachioDocumentFluentApi AddWhileLoopAndEnter(this MorestachioDocumentFluentApi api,
			Func<MorestachioExpressionBuilderBaseRootApi, IMorestachioExpression> condition)
		{
			return api.AddChildAndEnter(builder => new WhileLoopDocumentItem(condition(builder)));
		}
	}

	/// <summary>
	///		Fluent api class for Traversing or Modifying an Morestachio Document
	/// </summary>
	public class MorestachioDocumentFluentApi
	{
		/// <summary>
		///		Creates a new Fluent api
		/// </summary>
		internal MorestachioDocumentFluentApi(MorestachioDocumentInfo documentInfo)
		{
			var rootNode = CreateNodes(null, documentInfo.Document);
			Context = new FluentApiContext(rootNode, rootNode, documentInfo.ParserOptions);
		}

		private static MorestachioNode CreateNodes(MorestachioNode parent, IDocumentItem document)
		{
			var nodes = new List<MorestachioNode>();
			var stack = new Stack<Tuple<IDocumentItem, IDocumentItem>>();
			stack.Push(new Tuple<IDocumentItem, IDocumentItem>(parent?.Item, document));
			var rootNode = new MorestachioNode(parent, document);
			nodes.Add(rootNode);
			while (stack.Any())
			{
				var item = stack.Pop();
				var parentNode = nodes.Find(f => f.Item == item.Item2) ?? rootNode;
				foreach (var documentItem in item.Item2.Children)
				{
					stack.Push(new Tuple<IDocumentItem, IDocumentItem>(item.Item2, documentItem));
					var morestachioNode = new MorestachioNode(parentNode, documentItem);

					nodes.Add(morestachioNode);
					parentNode.Leafs.Add(morestachioNode);
				}
			}

			var prevNode = rootNode;
			foreach (var morestachioNode in nodes.Skip(1))
			{
				prevNode.Next = morestachioNode;
				morestachioNode.Previous = prevNode;
				prevNode = morestachioNode;
			}

			return rootNode;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		public MorestachioDocumentFluentApi(FluentApiContext context)
		{
			Context = context;
		}

		/// <summary>
		///		Gets the context this Fluent api is currently operating with
		/// </summary>
		public FluentApiContext Context { get; }

		/// <summary>
		///		Executes the action if the last operation was successful like searching in the tree
		/// </summary>
		/// <param name="action"></param>
		/// <returns></returns>
		public MorestachioDocumentFluentApi IfSuccess(Func<MorestachioDocumentFluentApi, MorestachioDocumentFluentApi> action)
		{
			if (Context.OperationStatus)
			{
				return action(this);
			}

			return this;
		}

		/// <summary>
		///		Executes the action if the last operation was bot successful like searching in the tree
		/// </summary>
		/// <param name="action"></param>
		/// <returns></returns>
		public MorestachioDocumentFluentApi IfNotSuccess(Func<MorestachioDocumentFluentApi, MorestachioDocumentFluentApi> action)
		{
			if (!Context.OperationStatus)
			{
				return action(this);
			}

			return this;
		}

		/// <summary>
		///		Executes the action if the last operation was successful like searching in the tree
		/// </summary>
		/// <param name="action"></param>
		/// <returns></returns>
		public MorestachioDocumentFluentApi IfSuccess(Action<MorestachioDocumentFluentApi> action)
		{
			if (Context.OperationStatus)
			{
				action(this);
			}

			return this;
		}

		/// <summary>
		///		Executes the action if the last operation was bot successful like searching in the tree
		/// </summary>
		/// <param name="action"></param>
		/// <returns></returns>
		public MorestachioDocumentFluentApi IfNotSuccess(Action<MorestachioDocumentFluentApi> action)
		{
			if (!Context.OperationStatus)
			{
				action(this);
			}

			return this;
		}

		/// <summary>
		///		Renders the current <see cref="FluentApiContext.CurrentNode"/> and all its children using the <see cref="ToParsableStringDocumentVisitor"/>
		/// </summary>
		/// <param name="template"></param>
		/// <returns></returns>
		public MorestachioDocumentFluentApi RenderTree(Action<string> template)
		{
			var visitor = new ToParsableStringDocumentVisitor();
			Context.CurrentNode.Item.Accept(visitor);
			template(visitor.StringBuilder.ToString());
			return this;
		}

		/// <summary>
		///		Renders the current <see cref="FluentApiContext.CurrentNode"/>
		/// </summary>
		/// <param name="data"></param>
		/// <param name="template"></param>
		/// <returns></returns>
		public MorestachioDocumentFluentApi Render(object data, Action<Stream> template)
		{
			var documentInfo = new MorestachioDocumentInfo(Context.Options, Context.CurrentNode.Item);
			var morestachioDocumentResult = documentInfo.Create(data);
			template(morestachioDocumentResult.Stream);
			return this;
		}

		/// <summary>
		///		Gets the current Document item if its of the given type.
		/// <para>Do not modify the Children of this document item directly and use the methods of <see cref="MorestachioDocumentFluentApi"/> instead!</para>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="modifyAction"></param>
		/// <returns></returns>
		public MorestachioDocumentFluentApi Current<T>(Action<T> modifyAction) where T : IDocumentItem
		{
			if (Context.CurrentNode.Item is T item)
			{
				Context.OperationStatus = true;
				modifyAction(item);
			}
			Context.OperationStatus = false;
			return this;
		}

		/// <summary>
		///		Gets the current Document item
		/// <para>Do not modify the Children of this document item directly and use the methods of <see cref="MorestachioDocumentFluentApi"/> instead!</para>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="modifyAction"></param>
		/// <returns></returns>
		public MorestachioDocumentFluentApi Current(Action<IDocumentItem> modifyAction)
		{
			Current<IDocumentItem>(modifyAction);
			return this;
		}

		/// <summary>
		///		Removes the current Node from the tree and from the <see cref="IDocumentItem.Children"/> of the current <see cref="IDocumentItem"/>
		/// </summary>
		/// <returns></returns>
		public MorestachioDocumentFluentApi Remove()
		{
			var currentNodeParent = Context.CurrentNode;
			if (currentNodeParent == null)
			{
				Context.OperationStatus = false;
				return this;
			}
			Context.OperationStatus = true;
			if (currentNodeParent.Previous != null)
			{
				currentNodeParent.Previous.Next = currentNodeParent.Next;
			}

			if (currentNodeParent.Next != null)
			{
				currentNodeParent.Next.Previous = currentNodeParent.Previous;
			}

			currentNodeParent.Leafs.Remove(currentNodeParent);
			currentNodeParent.Item.Children.Remove(Context.CurrentNode.Item);
			Context.CurrentNode = currentNodeParent.Previous;
			return this;
		}

		/// <summary>
		///		Adds the result of the item function to the current <see cref="IDocumentItem"/> and creates the necessary nodes in the tree and sets the <see cref="FluentApiContext.CurrentNode"/> to the created element
		/// </summary>
		/// <returns></returns>
		public MorestachioDocumentFluentApi AddChildAndEnter(Func<MorestachioExpressionBuilderBaseRootApi, IDocumentItem> item)
		{
			Context.CurrentNode = AddChildInternal(item);
			return this;
		}

		/// <summary>
		///		Adds the result of the item function to the current <see cref="IDocumentItem"/> and creates the necessary nodes in the tree
		/// </summary>
		/// <returns></returns>
		public MorestachioDocumentFluentApi AddChild(Func<MorestachioExpressionBuilderBaseRootApi, IDocumentItem> item)
		{
			AddChildInternal(item);
			return this;
		}

		private MorestachioNode AddChildInternal(Func<MorestachioExpressionBuilderBaseRootApi, IDocumentItem> item)
		{
			var itemInstance = item(new MorestachioExpressionBuilderBaseRootApi());
			var node = CreateNodes(Context.CurrentNode, itemInstance);
			Context.CurrentNode.Leafs.Add(node);
			Context.CurrentNode.Item.Children.Add(itemInstance);
			return node;
		}

		/// <summary>
		///		Sets the <see cref="FluentApiContext.CurrentNode"/> to the next <see cref="IDocumentItem"/> in render order
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public MorestachioDocumentFluentApi FindNext<T>() where T : IDocumentItem
		{
			return SearchForward(f => f is T);
		}

		/// <summary>
		///		Sets the <see cref="FluentApiContext.CurrentNode"/> to the previous <see cref="IDocumentItem"/> in render order
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public MorestachioDocumentFluentApi FindPrevious<T>() where T : IDocumentItem
		{
			return SearchBackward(f => f is T);
		}

		/// <summary>
		///		Sets the <see cref="FluentApiContext.CurrentNode"/> to the parent of the current node
		/// </summary>
		/// <returns></returns>
		public MorestachioDocumentFluentApi Parent()
		{
			if (Context.CurrentNode.Ancestor == null)
			{
				Context.OperationStatus = false;
				return this;
			}

			Context.OperationStatus = true;
			Context.CurrentNode = Context.CurrentNode.Ancestor;
			return this;
		}

		/// <summary>
		///		Sets the <see cref="FluentApiContext.CurrentNode"/> to the root of the document
		/// </summary>
		/// <returns></returns>
		public MorestachioDocumentFluentApi Root()
		{
			Context.OperationStatus = true;
			Context.CurrentNode = Context.RootNode;
			return this;
		}

		/// <summary>
		///		Sets the <see cref="FluentApiContext.CurrentNode"/> to the nth child of the current node
		/// </summary>
		/// <returns></returns>
		public MorestachioDocumentFluentApi Child(int index)
		{
			if (Context.CurrentNode.Leafs.Count < index)
			{
				Context.OperationStatus = false;
				return this;
			}

			Context.OperationStatus = true;
			Context.CurrentNode = Context.CurrentNode.Leafs[index];
			return this;
		}

		/// <summary>
		///		Sets the <see cref="FluentApiContext.CurrentNode"/> to the parent of the current node that is of the type T
		/// </summary>
		/// <returns></returns>
		public MorestachioDocumentFluentApi FindParent<T>() where T : IDocumentItem
		{
			return FindParent(f => f is T);
		}

		/// <summary>
		///		Sets the <see cref="FluentApiContext.CurrentNode"/> to the parent of the current node that is of the type T
		/// </summary>
		/// <returns></returns>
		public MorestachioDocumentFluentApi FindParent(Func<IDocumentItem, bool> condition)
		{
			var node = Context.CurrentNode;
			while (!condition(node.Item) && node != null)
			{
				node = Context.CurrentNode.Ancestor;
			}

			if (node != null)
			{
				Context.OperationStatus = true;
				Context.CurrentNode = node;
			}
			else
			{
				Context.OperationStatus = false;
			}

			return this;
		}

		/// <summary>
		///		Searches all Parents and optionally their siblings until the condition is met
		/// </summary>
		/// <param name="condition"></param>
		/// <param name="includeCurrent"></param>
		/// <returns></returns>
		public MorestachioDocumentFluentApi SearchBackward(Func<IDocumentItem, bool> condition, bool includeCurrent = true)
		{
			var node = includeCurrent ? Context.CurrentNode : Context.CurrentNode.Ancestor;
			while (node != null)
			{
				if (condition(node.Item))
				{
					Context.CurrentNode = node;
					Context.OperationStatus = true;
					return this;
				}
				node = node.Previous;
			}

			if (node != null)
			{
				Context.OperationStatus = true;
				Context.CurrentNode = node;
			}
			else
			{
				Context.OperationStatus = false;
			}

			return this;
		}

		/// <summary>
		///		Searches all children optionally including the current item
		/// </summary>
		public MorestachioDocumentFluentApi SearchForward(Func<IDocumentItem, bool> condition, bool includeCurrent = false)
		{
			var node = includeCurrent ? Context.CurrentNode : Context.CurrentNode.Next;
			while (node != null)
			{
				if (condition(node.Item))
				{
					Context.CurrentNode = node;
					Context.OperationStatus = true;
					return this;
				}
				node = node.Next;
			}
			Context.OperationStatus = false;
			return this;
		}
	}
}
