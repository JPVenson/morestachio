using System.ComponentModel;
using System.Dynamic;
using System.Reflection;
using Morestachio.Document;
using Morestachio.Formatter.Framework;
using Morestachio.Framework.Context.Resolver;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Framework;
using PathPartElement
	= System.Collections.Generic.KeyValuePair<string, Morestachio.Framework.Expression.Framework.PathType>;

namespace Morestachio.Framework.Context;

/// <summary>
///     The current context for any given expression
/// </summary>
public class ContextObject
{
	/// <summary>
	///     <para>Gets the Default Definition of false.</para>
	///     This is ether:
	///     <para>- Null</para>
	///     <para>- boolean false</para>
	///     <para>- 0 double or int</para>
	///     <para>- string.Empty (whitespaces are allowed)</para>
	///     <para>- collection not Any().</para>
	///     This field can be used to define your own <see cref="DefinitionOfFalse" /> and then fallback to the default logic
	/// </summary>
	public static readonly Func<object, bool> DefaultDefinitionOfFalse;

	private static Func<object, bool> _definitionOfFalse;

	internal object InternalValue;

	private readonly ContextObject _parent;
	private bool _isNaturalContext;

	static ContextObject()
	{
		DefaultDefinitionOfFalse = value => value != null &&
			value as bool? != false &&
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			value as double? != 0 &&
			value as int? != 0 &&
			value as string != string.Empty &&
			// We've gotten this far, if it is an object that does NOT cast as enumberable, it exists
			// OR if it IS an enumerable and .Any() returns true, then it exists as well
			(value is not IEnumerable enumerable || enumerable.Cast<object>().Any()
			);
		DefinitionOfFalse = DefaultDefinitionOfFalse;
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="ContextObject" /> class.
	/// </summary>
	public ContextObject(
		in string key,
		in ContextObject parent,
		in object value
	)
	{
		Key = key;
		_parent = parent;
		InternalValue = value;
		_isNaturalContext = _parent?._isNaturalContext ?? true;
	}

	/// <summary>
	///     Gets the Definition of false on your Template.
	/// </summary>
	/// <value>
	///     Must no be null
	/// </value>
	/// <exception cref="InvalidOperationException">If the value is null</exception>

	public static Func<object, bool> DefinitionOfFalse
	{
		get { return _definitionOfFalse; }
		set { _definitionOfFalse = value ?? throw new InvalidOperationException("The value must not be null"); }
	}

	/// <summary>
	///     Gets a value indicating whether this instance is natural context.
	///     A Natural context is a context outside an Isolated scope
	/// </summary>
	// ReSharper disable once ConvertToAutoPropertyWhenPossible
	public bool IsNaturalContext
	{
		get { return _isNaturalContext; }
		protected internal set { _isNaturalContext = value; }
	}

	/// <summary>
	///     The parent of the current context or null if its the root context
	/// </summary>
	// ReSharper disable once ConvertToAutoPropertyWhenPossible
	public ContextObject Parent
	{
		get { return _parent; }
	}

	/// <summary>
	///     The evaluated value of the expression
	/// </summary>
	// ReSharper disable once ConvertToAutoPropertyWhenPossible
	public ref object Value
	{
		get { return ref InternalValue; }
		//set { InternalValue = value; }
	}

	///// <summary>
	/////	Ensures that the Value is loaded if needed
	///// </summary>
	///// <returns></returns>
	//public async Task EnsureValue()
	//{
	//	if (Value is Task)
	//	{
	//		Value = await Value.UnpackFormatterTask();
	//	}
	//}

	/// <summary>
	///     The name of the property or key inside the value or indexer expression for lists
	/// </summary>
	// ReSharper disable once ConvertToAutoProperty
	public string Key { get; }

	internal ContextObject FindNextNaturalContextObject()
	{
		var context = this;

		while (context is { _isNaturalContext: false })
		{
			context = context._parent;
		}

		return context;
	}

	/// <summary>
	///     Makes this instance natural
	/// </summary>
	/// <returns></returns>
	internal ContextObject MakeNatural()
	{
		_isNaturalContext = true;
		return this;
	}

	/// <summary>
	///     Makes this instance natural
	/// </summary>
	/// <returns></returns>
	internal ContextObject MakeSyntetic()
	{
		_isNaturalContext = false;
		return this;
	}

	/// <summary>
	///     if overwritten by a class it returns a context object for any non standard key or operation.
	///     if non of that
	///     <value>null</value>
	/// </summary>
	/// <returns></returns>
	public virtual ContextObject HandlePathContext(
		ref PathPartElement currentElement,
		IMorestachioExpression morestachioExpression,
		ScopeData scopeData
	)
	{
		return null;
	}

	private ContextObject GetContextForPathInternal(
		Traversable elements,
		ScopeData scopeData,
		IMorestachioExpression morestachioExpression
	)
	{
		var retval = this;

		if (elements == null)
		{
			return retval;
		}

		var preHandeld = HandlePathContext(ref elements.Current, morestachioExpression, scopeData);

		if (preHandeld != null)
		{
			return preHandeld;
		}

		switch (elements.Current.Value)
		{
			//go the root object
			case PathType.RootSelector:
				return ExecuteRootSelector() ?? this;
			//go one level up
			case PathType.ParentSelector when Parent != null:
				return FindNextNaturalContextObject()?.Parent ?? Parent ?? this;
			case PathType.ParentSelector:
				return this;
			//enumerate ether an IDictionary, an cs object or an IEnumerable to a KeyValuePair array
			case PathType.ObjectSelector when InternalValue is null:
				return scopeData._parserOptions.CreateContextObject("x:null", null);
			//ALWAYS return the context, even if the value is null.
			case PathType.ObjectSelector:
				return ExecuteObjectSelector(elements.Current.Key, scopeData);
			case PathType.Boolean when elements.Current.Key is "true" or "false":
			{
				var booleanContext =
					scopeData._parserOptions.CreateContextObject(".", elements.Current.Key == "true", this);
				booleanContext.IsNaturalContext = IsNaturalContext;

				return booleanContext;
			}
			case PathType.Boolean:
				return this;
			case PathType.Null:
				return scopeData._parserOptions.CreateContextObject("x:null", null);
			case PathType.DataPath:
				return ExecuteDataPath(elements.Current.Key, morestachioExpression, scopeData);
			case PathType.SelfAssignment:
			case PathType.ThisPath:
			default:
				return retval;
		}
	}

	internal ContextObject ExecuteObjectSelector(string key, ScopeData scopeData)
	{
		ContextObject innerContext = null;

		if (!scopeData._parserOptions.HandleDictionaryAsObject && InternalValue is IDictionary<string, object> dictList)
		{
			innerContext = scopeData._parserOptions.CreateContextObject(key, dictList.Select(e => e), this);
		}
		else
		{
			if (InternalValue != null)
			{
				var type = InternalValue.GetType();

				innerContext = scopeData._parserOptions.CreateContextObject(key, type
						.GetTypeInfo()
						.GetProperties(BindingFlags.Instance | BindingFlags.Public)
						.Where(e => !e.IsSpecialName && !e.GetIndexParameters().Any() && e.CanRead)
						.Select(e =>
						{
							object value;

							if (scopeData._parserOptions.ValueResolver?.CanResolve(type, InternalValue, e.Name, null,
									scopeData) == true)
							{
								value = scopeData._parserOptions.ValueResolver.Resolve(type, InternalValue, e.Name,
									null, scopeData);
							}
							else
							{
								value = e.GetValue(Value);
							}

							return new KeyValuePair<string, object>(e.Name, value);
						}),
					this);
			}
		}

		return innerContext ?? this;
	}

	internal ContextObject ExecuteDataPath(
		string key,
		IMorestachioExpression morestachioExpression,
		ScopeData scopeData
	)
	{
		if (InternalValue is null)
		{
			scopeData._parserOptions.OnUnresolvedPath(new InvalidPathEventArgs(this, morestachioExpression,
				key, InternalValue?.GetType()));

			return scopeData._parserOptions.CreateContextObject("x:null", null);
		}
		//ALWAYS return the context, even if the value is null.

		//allow build-in variables to be accessed at any level
		//DO NOT use string.StartsWith here as it happens to have be a huge performance impact
		if (key.StartsWith('$'))
		{
			var getFromAlias = scopeData.GetVariable(this, key);

			if (getFromAlias != null)
			{
				return getFromAlias;
			}
		}

		//A value resolver should always be checked first to allow overwriting of all other logic
		if (scopeData._parserOptions.ValueResolver != null)
		{
			var type = InternalValue.GetType();
			var innerContext = scopeData._parserOptions.CreateContextObject(key, null, this);

			if (scopeData._parserOptions.ValueResolver.CanResolve(type, InternalValue, key, innerContext, scopeData))
			{
				innerContext.InternalValue
					= scopeData._parserOptions.ValueResolver.Resolve(type, InternalValue, key, innerContext, scopeData);

				return innerContext;
			}
		}

		if (!scopeData._parserOptions.HandleDictionaryAsObject && InternalValue is IDictionary<string, object> ctx)
		{
			if (!ctx.TryGetValue(key, out var o))
			{
				scopeData._parserOptions.OnUnresolvedPath(new InvalidPathEventArgs(this, morestachioExpression,
					key, InternalValue?.GetType()));
			}

			return scopeData._parserOptions.CreateContextObject(key, o, this);
		}

		switch (InternalValue)
		{
			case IMorestachioPropertyResolver cResolver:
			{
				if (!cResolver.TryGetValue(key, out var o))
				{
					scopeData._parserOptions.OnUnresolvedPath(new InvalidPathEventArgs(this, morestachioExpression,
						key, InternalValue?.GetType()));
				}

				return scopeData._parserOptions.CreateContextObject(key, o, this);
			}
			case ICustomTypeDescriptor descriptor:
			{
				var propertyDescriptor = descriptor.GetProperties().Find(key, false);

				if (propertyDescriptor != null)
				{
					return scopeData._parserOptions.CreateContextObject(key, propertyDescriptor.GetValue(InternalValue),
						this);
				}

				scopeData._parserOptions.OnUnresolvedPath(new InvalidPathEventArgs(this, morestachioExpression, key,
					InternalValue?.GetType()));

				return scopeData._parserOptions.CreateContextObject(key, null, this);
			}
			case DynamicObject dynObject when dynObject.TryGetMember(new DynamicObjectBinder(key, false), out var val):
				return scopeData._parserOptions.CreateContextObject(key, val, this);
			case DynamicObject:
				scopeData._parserOptions.OnUnresolvedPath(new InvalidPathEventArgs(this, morestachioExpression, key,
					InternalValue?.GetType()));

				return scopeData._parserOptions.CreateContextObject(key, null, this);
		}

		var value = scopeData._parserOptions.FallbackValueResolver.Resolve(this, key, scopeData,
			morestachioExpression);

		return scopeData._parserOptions.CreateContextObject(key, value, this);
	}

	private class DynamicObjectBinder : GetMemberBinder
	{
		public DynamicObjectBinder(string name, bool ignoreCase) : base(name, ignoreCase)
		{
		}

		public override DynamicMetaObject FallbackGetMember(
			DynamicMetaObject target,
			DynamicMetaObject errorSuggestion
		)
		{
			return errorSuggestion;
		}
	}

	internal ContextObject ExecuteRootSelector()
	{
		var parent = _parent ?? this;
		var lastParent = parent;

		while (parent != null)
		{
			parent = parent._parent;

			if (parent != null)
			{
				lastParent = parent;
			}
		}

		return lastParent;
	}

	/// <summary>
	///     Returns a variable that is only present in this context but not below it
	/// </summary>
	/// <param name="path"></param>
	/// <returns></returns>
	public virtual ContextObject GetContextVariable(string path)
	{
		return null;
	}

	/// <summary>
	///     Will walk the path by using the path seperator "." and evaluate the object at the end
	/// </summary>
	/// <returns></returns>
	internal ContextObject GetContextForPath(
		Traversable elements,
		ScopeData scopeData,
		IMorestachioExpression morestachioExpression
	)
	{
		if (Key == "x:null" || !elements.HasValue)
		{
			return this;
		}

		//look at the first element if its an alias switch to that alias
		//var peekPathPart = elements.Peek();
		if (elements.Count == 1 && elements.Current.Value == PathType.Null)
		{
			return scopeData._parserOptions.CreateContextObject("x:null", null);
		}

		var targetContext = this;

		if (elements.Current.Value == PathType.DataPath)
		{
			var getFromAlias = scopeData.GetVariable(targetContext, elements.Current.Key);

			if (getFromAlias != null)
			{
				elements = elements.Next();
				targetContext = getFromAlias;
			}
		}

		return targetContext.LoopContextTraversable(elements, scopeData, morestachioExpression);
		//return await targetContext.GetContextForPathInternal(elements, scopeData, morestachioExpression);
	}

	internal ContextObject LoopContextTraversable(
		Traversable elements,
		ScopeData scopeData,
		IMorestachioExpression morestachioExpression
	)
	{
		var context = this;

		while (elements is { HasValue: true })
		{
			context = context.GetContextForPathInternal(elements, scopeData, morestachioExpression);
			elements = elements.Next();
		}

		return context;
	}

	/// <summary>
	///     Determines if the value of this context exists.
	/// </summary>
	/// <returns></returns>
	public virtual bool Exists()
	{
		return DefinitionOfFalse(InternalValue);
	}

#if Span
	/// <summary>
	///     Renders the Current value to a string or if null to the Null placeholder in the Options
	/// </summary>
	/// <param name="scopeData"></param>
	/// <returns></returns>
	public virtual ReadOnlySpan<char> RenderToString(ScopeData scopeData)
	{
		return InternalValue switch
		{
			ReadOnlyMemory<char> roSpan => roSpan.Span,
			string str => str.AsSpan(),
			null => (scopeData.GetVariable(this, "$null")?.InternalValue?.ToString() ?? scopeData._parserOptions.Null)
				.AsSpan(),
			IMorestachioRender renderable => renderable.RenderToString(),
			IMorestachioRenderAsync asyncRenderable => asyncRenderable.RenderToString().GetAwaiter().GetResult(),
			_ => InternalValue.ToString().AsSpan()
		};
	}
#else
	/// <summary>
	///     Renders the Current value to a string or if null to the Null placeholder in the Options
	/// </summary>
	/// <param name="scopeData"></param>
	/// <returns></returns>
	public virtual string RenderToString(ScopeData scopeData)
	{
		return InternalValue switch
		{
			string str => str,
			null => (scopeData.GetVariable(this, "$null")?.InternalValue?.ToString() ?? scopeData._parserOptions.Null),
			IMorestachioRender renderable => renderable.RenderToString(),
			IMorestachioRenderAsync asyncRenderable => asyncRenderable.RenderToString().GetAwaiter().GetResult(),
			_ => InternalValue.ToString()
		};
	}
#endif

	/// <summary>
	///     Gets an FormatterCache from ether the custom formatter or the global one
	/// </summary>
	public virtual FormatterCache PrepareFormatterCall(
		ref object value,
		Type type,
		string name,
		FormatterArgumentType[] arguments,
		ScopeData scopeData
	)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			name = null;
		}

		//call formatters that are given by the Options for this run
		return scopeData._parserOptions.Formatters.PrepareCallMostMatchingFormatter(ref value, type, arguments, name,
			scopeData._parserOptions, scopeData);
	}

	/// <summary>
	///     Clones the ContextObject into a new Detached object
	/// </summary>
	/// <returns></returns>
	public virtual ContextObject CloneForEdit()
	{
		//note: Parent must be the original context so we can traverse up to an unmodified context
		var contextClone = new ContextObject(Key, this, InternalValue)
		{
			_isNaturalContext = false
		};

		return contextClone;
	}

	/// <summary>
	///     Clones the ContextObject into a new Detached object
	/// </summary>
	/// <returns></returns>
	public virtual ContextObject CloneForEdit(object newValue)
	{
		//note: Parent must be the original context so we can traverse up to an unmodified context
		var contextClone = new ContextObject(Key, this, newValue)
		{
			_isNaturalContext = false
		};

		return contextClone;
	}

	/// <summary>
	///     Clones the ContextObject into a new Detached object
	/// </summary>
	/// <returns></returns>
	public virtual ContextObject Copy()
	{
		//note: Parent must be the original context so we can traverse up to an unmodified context
		var contextClone = new ContextObject(Key, _parent, InternalValue)
		{
			_isNaturalContext = true
		};

		return contextClone;
	}
}