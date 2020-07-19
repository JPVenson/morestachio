using System;
using System.Collections.Generic;
using System.Linq;
using Morestachio.Document;
using Morestachio.Document.Contracts;
using Morestachio.Framework;
using Morestachio.Framework.Expression;

namespace Morestachio
{
	/// <summary>
	///		Hosts all infos about the current execution path of a part in the Template.
	///		Can be used for future parallel execution
	/// </summary>
	public class ScopeData : IDisposable
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ScopeData"/> class.
		/// </summary>
		public ScopeData()
		{
			Partials = new Dictionary<string, IDocumentItem>();
			PartialDepth = new Stack<string>();
			Alias = new Dictionary<string, IDictionary<int, ContextObject>>();
			Variables = new Dictionary<string, ContextObject>();
			CustomData = new Dictionary<string, object>();
		}

		/// <summary>
		///		List of all Partials
		/// </summary>
		public IDictionary<string, IDocumentItem> Partials { get; private set; }

		///  <summary>
		/// 		Adds a new variable or alias. An alias is bound to its scope and will be reset when the scoping <see cref="IDocumentItem"/> is closed. An Variable is global
		///  </summary>
		///  <param name="name"></param>
		///  <param name="value"></param>
		///  <param name="scope"></param>
		///  <param name="idVariableScope"></param>
		public void AddVariable(string name, ContextObject value, int idVariableScope)
		{
			if (idVariableScope == 0)
			{
				Variables[name] = value;
			}
			else
			{
				if (!Alias.TryGetValue(name, out var stack))
				{
					stack = new Dictionary<int, ContextObject>();
					Alias.Add(name, stack);
				}

				stack[idVariableScope] = value;
			}
		}

		/// <summary>
		///		Removes the variable from the internal stack
		/// </summary>
		/// <param name="name"></param>
		/// <param name="idVariableScope"></param>
		public void RemoveVariable(string name, int idVariableScope)
		{
			if (Alias.TryGetValue(name, out var stack))
			{
				stack.Remove(idVariableScope);
			}
		}

		/// <summary>
		///		Gets the Variable with the given name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public ContextObject GetVariable(string name)
		{
			if (Alias.TryGetValue(name, out var stack) && stack.Count > 0)
			{
				return stack.LastOrDefault().Value;
			}

			if (Variables.TryGetValue(name, out var value))
			{
				return value;
			}

			return null;
		}

		/// <summary>
		///		The Depth of current Partial usage
		/// </summary>
		public Stack<string> PartialDepth { get; private set; }

		/// <summary>
		///		Lists all Alias objects
		/// </summary>
		public IDictionary<string, IDictionary<int, ContextObject>> Alias { get; private set; }

		/// <summary>
		///		Lists all Alias objects
		/// </summary>
		public IDictionary<string, ContextObject> Variables { get; private set; }

		/// <summary>
		///		Can be used by 3rd party document items to store data.
		///		If it contains an IDisposable it will be disposed after the execution is finished.
		/// </summary>
		public IDictionary<string, object> CustomData { get; set; }

		/// <summary>
		///		Will be set by any preceding If statement if the expression was not rendered to true
		/// </summary>
		public bool ExecuteElse { get; set; }

		/// <inheritdoc />
		public void Dispose()
		{
			foreach (var disposable in CustomData.OfType<IDisposable>())
			{
				disposable.Dispose();
			}
		}
	}
}