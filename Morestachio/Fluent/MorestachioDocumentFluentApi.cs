using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Morestachio.Document.Contracts;
using Morestachio.Document.Visitor;
using Morestachio.Fluent.Expression;
using Morestachio.Framework.IO;
using Morestachio.Rendering;
using Morestachio.TemplateContainers;

namespace Morestachio.Fluent;

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
		var rootNode = CreateNodes(null, documentInfo.Document, out _);
		Context = new FluentApiContext(rootNode, rootNode, documentInfo.ParserOptions);
	}

	/// <summary>
	///		Creates nods that relates to children and render order
	/// </summary>
	/// <param name="parent"></param>
	/// <param name="document"></param>
	/// <returns></returns>
	public static MorestachioNode CreateNodes(MorestachioNode parent,
											IDocumentItem document,
											out List<MorestachioNode> nodes)
	{
		var nodesList = new List<MorestachioNode>();
		var stack = new Stack<Tuple<MorestachioNode, IDocumentItem>>();

		void PushNode(MorestachioNode node)
		{
			nodesList.Add(node);
			node.Ancestor?.Leafs.Add(node);

			if (node.Item is IBlockDocumentItem blockDocument)
			{
				foreach (var documentItem in blockDocument.Children.Reverse())
				{
					stack.Push(new Tuple<MorestachioNode, IDocumentItem>(node, documentItem));
				}
			}
		}

		var rootNode = new MorestachioNode(parent, document);
		PushNode(rootNode);

		while (stack.Any())
		{
			var item = stack.Pop();
			PushNode(new MorestachioNode(item.Item1, item.Item2));
		}

		var prevNode = rootNode;

		foreach (var morestachioNode in nodesList.Skip(1))
		{
			prevNode.Next = morestachioNode;
			morestachioNode.Previous = prevNode;
			prevNode = morestachioNode;
		}

		nodes = nodesList;
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
	public MorestachioDocumentFluentApi IfSuccess(
		Func<MorestachioDocumentFluentApi, MorestachioDocumentFluentApi> action)
	{
		if (Context.OperationStatus)
		{
			return action(this);
		}

		return this;
	}

	/// <summary>
	///		Executes the action if the last operation was not successful like searching in the tree
	/// </summary>
	/// <param name="action"></param>
	/// <returns></returns>
	public MorestachioDocumentFluentApi IfNotSuccess(
		Func<MorestachioDocumentFluentApi, MorestachioDocumentFluentApi> action)
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
	///		Executes the action if the last operation was not successful like searching in the tree
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
		var visitor = new ToParsableStringDocumentVisitor(Context.Options);
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
	public MorestachioDocumentFluentApi Render(object data, Action<IByteCounterStream> template)
	{
		var documentInfo = new MorestachioDocumentInfo(Context.Options, Context.CurrentNode.Item);
		var morestachioDocumentResult = documentInfo.CreateRenderer().Render(data);
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
	public MorestachioDocumentFluentApi Current(Action<IDocumentItem> modifyAction)
	{
		Current<IDocumentItem>(modifyAction);
		return this;
	}

	/// <summary>
	///		Removes the current Node from the tree and from the <see cref="IBlockDocumentItem.Children"/> of the current <see cref="IDocumentItem"/>
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
		((IBlockDocumentItem)currentNodeParent.Item).Children.Remove(Context.CurrentNode.Item);
		Context.CurrentNode = currentNodeParent.Previous;
		return this;
	}

	/// <summary>
	///		Parses the template and adds it as a child to the current <see cref="IDocumentItem"/>
	/// </summary>
	/// <param name="documentPart"></param>
	/// <returns></returns>
	public MorestachioDocumentFluentApi ParseAndAdd(string documentPart)
	{
		var morestachioDocumentInfo = Parser.ParseWithOptions(ParserOptionsBuilder.New()
			.WithTemplate(documentPart)
			.Build());
		AddChildInternal(f => morestachioDocumentInfo.Document);
		return this;
	}

	/// <summary>
	///		Adds the result of the item function to the current <see cref="IDocumentItem"/> and creates the necessary nodes in the tree and sets the <see cref="FluentApiContext.CurrentNode"/> to the created element
	/// </summary>
	/// <returns></returns>
	public MorestachioDocumentFluentApi AddChildAndEnter(
		Func<MorestachioExpressionBuilderBaseRootApi, IDocumentItem> item)
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

	/// <summary>
	///		Adds the result of the item function to the current <see cref="IDocumentItem"/> and creates the necessary nodes in the tree and sets the <see cref="FluentApiContext.CurrentNode"/> to the created element
	/// </summary>
	/// <returns></returns>
	public MorestachioDocumentFluentApi AddChildAndEnter(Func<IDocumentItem> item)
	{
		Context.CurrentNode = AddChildInternal(f => item());
		return this;
	}

	/// <summary>
	///		Adds the result of the item function to the current <see cref="IDocumentItem"/> and creates the necessary nodes in the tree
	/// </summary>
	/// <returns></returns>
	public MorestachioDocumentFluentApi AddChild(Func<IDocumentItem> item)
	{
		AddChildInternal(f => item());
		return this;
	}

	private MorestachioNode AddChildInternal(Func<MorestachioExpressionBuilderBaseRootApi, IDocumentItem> item)
	{
		var itemInstance = item(new MorestachioExpressionBuilderBaseRootApi());
		var node = CreateNodes(Context.CurrentNode, itemInstance, out _);
		Context.CurrentNode.Leafs.Add(node);
		((IBlockDocumentItem)Context.CurrentNode.Item).Children.Add(itemInstance);
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
	///		Sets the <see cref="FluentApiContext.CurrentNode"/> to the next <see cref="IDocumentItem"/> in render order
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public MorestachioDocumentFluentApi FindNext<T>(Func<T, bool> condition) where T : IDocumentItem
	{
		return SearchForward(f => f is T e && condition(e));
	}

	/// <summary>
	///		Sets the <see cref="FluentApiContext.CurrentNode"/> to the previous <see cref="IDocumentItem"/> in render order
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public MorestachioDocumentFluentApi FindPrevious<T>(Func<T, bool> condition) where T : IDocumentItem
	{
		return SearchBackward(f => f is T e && condition(e));
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
	///		Sets the <see cref="FluentApiContext.CurrentNode"/> to the nth child of the current node
	/// </summary>
	/// <returns></returns>
	public MorestachioDocumentFluentApi Child<T>(Func<T, bool> condition)
	{
		var fod = Context.CurrentNode.Leafs.FirstOrDefault(f => f is T e && condition(e));

		if (fod == null)
		{
			Context.OperationStatus = false;
			return this;
		}

		Context.OperationStatus = true;
		Context.CurrentNode = fod;
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
	public MorestachioDocumentFluentApi FindParent<T>(Func<T, bool> condition) where T : IDocumentItem
	{
		return FindParent(f => f is T e && condition(e));
	}

	/// <summary>
	///		Sets the <see cref="FluentApiContext.CurrentNode"/> to the parent of the current node that is of the type T
	/// </summary>
	/// <returns></returns>
	public MorestachioDocumentFluentApi FindParent(Func<IDocumentItem, bool> condition)
	{
		var node = Context.CurrentNode;

		while (node != null && !condition(node.Item))
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