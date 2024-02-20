using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Morestachio.Document.Contracts;
using Morestachio.Formatter.Framework;
using Morestachio.Formatter.Predefined;
using Morestachio.Framework.Context;
using Morestachio.Framework.Context.Resolver;
using Morestachio.Profiler;
using Morestachio.Util.StaticBinding;

namespace Morestachio.Document;

/// <summary>
///		Hosts all infos about the current execution path of a part in the Template.
///		Can be used for future parallel execution
/// </summary>
public class ScopeData : IDisposable
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ScopeData"/> class.
	/// </summary>
	public ScopeData(ParserOptions parserOptions, CancellationToken? cancellationToken = null)
	{
		_parserOptions = parserOptions;
		Partials = new Dictionary<string, IDocumentItem>();
		CompiledPartials = new Dictionary<string, CompilationAsync>();
		PartialDepth = new Stack<Tuple<string, int>>();
		Alias = new Dictionary<string, IDictionary<int, object>>();
		Variables = new Dictionary<string, object>();
		CustomData = new Dictionary<string, object>();
		CancellationToken = cancellationToken ?? CancellationToken.None;
		IsOutputLimited = !cancellationToken.Equals(CancellationToken.None) || parserOptions.MaxSize != 0;
		AddCollectionContextSpecialVariables();
		AddServicesVariable();
		AddConstants(parserOptions);
	}

	private void AddConstants(ParserOptions parserOptions)
	{
		var constants = parserOptions.Formatters.Constants;

		foreach (var @constValue in constants)
		{
			var value = @constValue.Value;

			if (value is Type type)
			{
				value = new Static(type);
			}

			AddVariable(@constValue.Key, (scope) => scope.ParserOptions.CreateContextObject(@constValue.Key, value));
		}
	}

	private class ServiceUiWrapper : IMorestachioPropertyResolver
	{
		private readonly IDictionary<Type, object> _services;

		public ServiceUiWrapper(IDictionary<Type, object> services)
		{
			_services = services;
		}

		public bool TryGetValue(string name, out object found)
		{
			found = _services.FirstOrDefault(e =>
				e.Key.GetCustomAttribute<ServiceNameAttribute>()?.Name == name || e.Key.Name == name).Value;
			return found != null;
		}
	}

	private void AddServicesVariable()
	{
		AddVariable("$services", (scopeData, context) =>
		{
			if (context == null)
			{
				return null;
			}

			var services = scopeData.ParserOptions.Formatters.Services.Enumerate();
			return scopeData.ParserOptions.CreateContextObject(".", new ServiceUiWrapper(services));
		});
	}

	private void AddCollectionContextSpecialVariables()
	{
		foreach (var keyValuePair in ContextCollection.GetVariables())
		{
			AddVariable(keyValuePair.Key, (scopeData, context) =>
			{
				ContextCollection coll = null;

				if (context is ContextCollection)
				{
					coll = context as ContextCollection;
				}
				else
				{
					var ctx = context;

					while (ctx != null && ctx is not ContextCollection)
					{
						ctx = ctx.Parent;
					}

					coll = ctx as ContextCollection;
				}

				if (coll != null)
				{
					return scopeData.ParserOptions.CreateContextObject(keyValuePair.Key, keyValuePair.Value(coll),
						context);
				}

				return null;
			});
		}
	}

	internal ParserOptions _parserOptions;

	/// <summary>
	///		The ParserOptions used to parse this template
	/// </summary>
	public ParserOptions ParserOptions
	{
		get { return _parserOptions; }
	}

	internal readonly bool IsOutputLimited;

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
	public IDictionary<string, CompilationAsync> CompiledPartials { get; private set; }

	///  <summary>
	/// 		Adds a new variable or alias. An alias is bound to its scope and will
	/// be reset when the scoping <see cref="IDocumentItem"/> is closed.
	/// An Variable is global when the idVariableScope is 0
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
	/// An Variable is global when the idVariableScope is 0
	///  </summary>
	public void AddVariable(string name, ContextObject value, int idVariableScope = 0)
	{
		AddVariableInternal(name, value, idVariableScope);
	}

	///  <summary>
	/// 		Adds a new variable or alias.
	/// An alias is bound to its scope and will be reset when the scoping <see cref="IDocumentItem"/> is closed.
	/// An Variable is global when the idVariableScope is 0
	///  </summary>
	public void AddVariable(string name, Func<ScopeData, ContextObject> value, int idVariableScope = 0)
	{
		AddVariableInternal(name, value, idVariableScope);
	}

	///  <summary>
	/// 		Adds a new variable or alias.
	/// An alias is bound to its scope and will be reset when the scoping <see cref="IDocumentItem"/> is closed.
	/// An Variable is global when the idVariableScope is 0
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
	///		Lists all variables
	/// </summary>
	public IDictionary<string, object> Variables { get; private set; }

	/// <summary>
	///		Can be used by 3rd party document items to store data.
	///		If it contains an IDisposable it will be disposed after the execution is finished.
	/// </summary>
	public IDictionary<string, object> CustomData { get; set; }

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