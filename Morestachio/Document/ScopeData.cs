using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Morestachio.Document.Contracts;
using Morestachio.Framework.Context;
using Morestachio.Profiler;

namespace Morestachio.Document
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
		/// <param name="cancellationToken"></param>
		public ScopeData(ParserOptions parserOptions, CancellationToken? cancellationToken = null)
		{
			ParserOptions = parserOptions;
			Partials = new Dictionary<string, IDocumentItem>();
			CompiledPartials = new Dictionary<string, Compilation>();
			PartialDepth = new Stack<Tuple<string, int>>();
			Alias = new Dictionary<string, IDictionary<int, object>>();
			Variables = new Dictionary<string, object>();
			CustomData = new Dictionary<string, object>();
			CancellationToken = cancellationToken ?? CancellationToken.None;
			AddCollectionContextSpecialVariables();
			AddServicesVariable();
		}

		private void AddServicesVariable()
		{
			AddVariable("$services", (scopeData, context) =>
			{
				if (context == null)
				{
					return null;
				}

				var services = new Dictionary<string, object>();
				foreach (var service in scopeData.ParserOptions.Formatters.ServiceCollection)
				{
					services[service.Key.Name] = service.Value;
				}

				return scopeData.ParserOptions.CreateContextObject(".", services);
			});
		}

		private void AddCollectionContextSpecialVariables()
		{
			foreach (var keyValuePair in ContextCollection.GetVariables())
			{
				AddVariable(keyValuePair.Key, (scopeData, context) =>
				{
					if (context is ContextCollection coll)
					{
						return scopeData.ParserOptions.CreateContextObject(keyValuePair.Key, keyValuePair.Value(coll), context);
					}

					return null;
				});
			}
		}

		/// <summary>
		///		The ParserOptions used to parse this template
		/// </summary>
		public ParserOptions ParserOptions { get; private set; }

		/// <summary>
		///		The Run specific stop token
		/// </summary>
		public CancellationToken CancellationToken { get; private set; }

		/// <summary>
		///		List of all Partials
		/// </summary>
		public IDictionary<string, IDocumentItem> Partials { get; private set; }

		/// <summary>
		///		List of all Partials that where added by using the compile method
		/// </summary>
		public IDictionary<string, Compilation> CompiledPartials { get; private set; }

		///  <summary>
		/// 		Adds a new variable or alias. An alias is bound to its scope and will
		/// be reset when the scoping <see cref="IDocumentItem"/> is closed.
		/// An Variable is global when the <see cref="idVariableScope"/> is 0
		///  </summary>
		private void AddVariableInternal(string name, object value, int idVariableScope)
		{
			if (idVariableScope == 0)
			{
				Variables[name] = value;
			}
			else
			{
				if (!Alias.TryGetValue(name, out var stack))
				{
					stack = new Dictionary<int, object>();
					Alias.Add(name, stack);
				}

				stack[idVariableScope] = value;
			}
		}

		internal ContextObject GetFromVariable(ContextObject contextObject, object variableValue)
		{
			if (variableValue is ContextObject ctx)
			{
				return ctx;
			}

			if (variableValue is Func<ScopeData, ContextObject> fnc)
			{
				return fnc(this);
			}

			if (variableValue is Func<ScopeData, ContextObject, ContextObject> fncC)
			{
				return fncC(this, contextObject);
			}
			throw new InvalidOperationException("Cannot evaluate the variable or factory: " + variableValue);
		}

		///  <summary>
		/// 		Adds a new variable or alias.
		/// An alias is bound to its scope and will be reset when the scoping <see cref="IDocumentItem"/> is closed.
		/// An Variable is global when the <see cref="idVariableScope"/> is 0
		///  </summary>
		public void AddVariable(string name, ContextObject value, int idVariableScope = 0)
		{
			AddVariableInternal(name, value, idVariableScope);
		}

		///  <summary>
		/// 		Adds a new variable or alias.
		/// An alias is bound to its scope and will be reset when the scoping <see cref="IDocumentItem"/> is closed.
		/// An Variable is global when the <see cref="idVariableScope"/> is 0
		///  </summary>
		public void AddVariable(string name, Func<ScopeData, ContextObject> value, int idVariableScope = 0)
		{
			AddVariableInternal(name, value, idVariableScope);
		}

		///  <summary>
		/// 		Adds a new variable or alias.
		/// An alias is bound to its scope and will be reset when the scoping <see cref="IDocumentItem"/> is closed.
		/// An Variable is global when the <see cref="idVariableScope"/> is 0
		///  </summary>
		public void AddVariable(string name, Func<ScopeData, ContextObject, ContextObject> value, int idVariableScope = 0)
		{
			AddVariableInternal(name, value, idVariableScope);
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

		///  <summary>
		/// 		Gets the Variable with the given name
		///  </summary>
		///  <param name="contextObject"></param>
		///  <param name="name"></param>
		///  <returns></returns>
		public ContextObject GetVariable(ContextObject contextObject, string name)
		{
			if (Alias.TryGetValue(name, out var stack) && stack.Count > 0)
			{
				return GetFromVariable(contextObject, stack.LastOrDefault().Value)?.CloneForEdit();
			}

			if (Variables.TryGetValue(name, out var value))
			{
				return GetFromVariable(contextObject, value)?.CloneForEdit();
			}

			return null;
		}

		/// <summary>
		///		The Depth of current Partial usage
		/// </summary>
		public Stack<Tuple<string, int>> PartialDepth { get; private set; }

		/// <summary>
		///		Lists all Alias objects
		/// </summary>
		public IDictionary<string, IDictionary<int, object>> Alias { get; private set; }

		/// <summary>
		///		Lists all Alias objects
		/// </summary>
		public IDictionary<string, object> Variables { get; private set; }

		/// <summary>
		///		Can be used by 3rd party document items to store data.
		///		If it contains an IDisposable it will be disposed after the execution is finished.
		/// </summary>
		public IDictionary<string, object> CustomData { get; set; }

		/// <summary>
		///		Will be set by any preceding If statement if the expression was not rendered to true
		/// </summary>
		public bool ExecuteElse { get; set; }

		internal PerformanceProfiler Profiler { get; set; }

		/// <inheritdoc />
		public void Dispose()
		{
			foreach (var disposable in CustomData.Values.OfType<IDisposable>())
			{
				disposable.Dispose();
			}
			foreach (var disposable in Variables.Values.OfType<IDisposable>())
			{
				disposable.Dispose();
			}
		}
	}
}