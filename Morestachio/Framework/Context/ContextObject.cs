using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using Morestachio.Document;
using Morestachio.Formatter.Framework;
using Morestachio.Formatter.Predefined;
using Morestachio.Formatter.Predefined.Accounting;
using Morestachio.Framework.Context.Resolver;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Helper;
using Morestachio.Helper.Logging;
using PathPartElement =
	System.Collections.Generic.KeyValuePair<string, Morestachio.Framework.Expression.Framework.PathType>;
#if ValueTask
using ContextObjectPromise = System.Threading.Tasks.ValueTask<Morestachio.Framework.Context.ContextObject>;
using StringPromise = System.Threading.Tasks.ValueTask<string>;
#else
using ContextObjectPromise = System.Threading.Tasks.Task<Morestachio.Framework.Context.ContextObject>;
using StringPromise = System.Threading.Tasks.Task<string>;

#endif

namespace Morestachio.Framework.Context
{
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
		 private object _value;
		private bool _abortGeneration;

		static ContextObject()
		{
			DefaultFormatter = new MorestachioFormatterService();
			DefaultFormatter.AddFromType(typeof(ObjectStringFormatter));
			DefaultFormatter.AddFromType(typeof(Number));
			DefaultFormatter.AddFromType(typeof(BooleanFormatter));
			DefaultFormatter.AddFromType(typeof(DateFormatter));
			DefaultFormatter.AddFromType(typeof(EqualityFormatter));
			DefaultFormatter.AddFromType(typeof(LinqFormatter));
			DefaultFormatter.AddFromType(typeof(ListExtensions));
			DefaultFormatter.AddFromType(typeof(RegexFormatter));
			DefaultFormatter.AddFromType(typeof(TimeSpanFormatter));
			DefaultFormatter.AddFromType(typeof(StringFormatter));
			DefaultFormatter.AddFromType(typeof(RandomFormatter));
			DefaultFormatter.AddFromType(typeof(Worktime));
			DefaultFormatter.AddFromType(typeof(Money));
			DefaultFormatter.AddFromType(typeof(HtmlFormatter));
			DefaultFormatter.AddFromType(typeof(LoggingFormatter));

			DefaultDefinitionOfFalse = value => value != null &&
												value as bool? != false &&
												// ReSharper disable once CompareOfFloatsByEqualityOperator
												value as double? != 0 &&
												value as int? != 0 &&
												value as string != string.Empty &&
												// We've gotten this far, if it is an object that does NOT cast as enumberable, it exists
												// OR if it IS an enumerable and .Any() returns true, then it exists as well
												(!(value is IEnumerable) || ((IEnumerable)value).Cast<object>().Any()
												);
			DefinitionOfFalse = DefaultDefinitionOfFalse;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ContextObject" /> class.
		/// </summary>
		public ContextObject( ParserOptions options,  string key,  ContextObject parent,
			object value)
		{
			Options = options ?? throw new ArgumentNullException(nameof(options));
			Key = key;
			Parent = parent;
			_value = value;
			if (Parent != null)
			{
				CancellationToken = Parent.CancellationToken;
			}

			IsNaturalContext = Parent?.IsNaturalContext ?? true;
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
		public bool IsNaturalContext { get; protected internal set; }

		/// <summary>
		///     The set of allowed types that may be printed. Complex types (such as arrays and dictionaries)
		///     should not be printed, or their printing should be specialized.
		///     Add an typeof(object) entry as Type to define a Default Output
		/// </summary>
		
		public static IMorestachioFormatterService DefaultFormatter { get; }

		/// <summary>
		///     The parent of the current context or null if its the root context
		/// </summary>
		
		public ContextObject Parent { get; }

		/// <summary>
		///     The evaluated value of the expression
		/// </summary>
		
		public object Value
		{
			get { return _value; }
			set { _value = value; }
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
		///     is an abort currently requested
		/// </summary>
		public bool AbortGeneration
		{
			get
			{
				return Parent?.AbortGeneration ?? _abortGeneration;
			}
			set
			{
				if (Parent == null)
				{
					_abortGeneration = value;
				}
				else
				{
					Parent.AbortGeneration = true;
				}
			}
		}

		/// <summary>
		///     The name of the property or key inside the value or indexer expression for lists
		/// </summary>
		
		public string Key { get; }

		/// <summary>
		///     With what options are the template currently is running
		/// </summary>
		
		public ParserOptions Options { get; }

		/// <summary>
		/// </summary>
		public CancellationToken CancellationToken { get; set; }

		internal ContextObject FindNextNaturalContextObject()
		{
			var context = this;
			while (context != null && !context.IsNaturalContext)
			{
				context = context.Parent;
			}

			return context;
		}

		/// <summary>
		///     Makes this instance natural
		/// </summary>
		/// <returns></returns>
		internal ContextObject MakeNatural()
		{
			IsNaturalContext = true;
			return this;
		}

		/// <summary>
		///     Makes this instance natural
		/// </summary>
		/// <returns></returns>
		internal ContextObject MakeSyntetic()
		{
			IsNaturalContext = false;
			return this;
		}

		/// <summary>
		///     if overwritten by a class it returns a context object for any non standard key or operation.
		///     if non of that
		///     <value>null</value>
		/// </summary>
		/// <returns></returns>
		public virtual ContextObject HandlePathContext(PathPartElement currentElement,
			IMorestachioExpression morestachioExpression, ScopeData scopeData)
		{
			return null;
		}

		private async ContextObjectPromise GetContextForPathInternal(
			Traversable elements,
			ScopeData scopeData,
			IMorestachioExpression morestachioExpression)
		{
			var retval = this;
			if (elements == null)
			{
				return retval;
			}

			var preHandeld = HandlePathContext(elements.Current, morestachioExpression, scopeData);
			if (preHandeld != null)
			{
				return preHandeld;
			}

			var type = Value?.GetType();
			if (elements.Current.Value == PathType.RootSelector) //go the root object
			{
				return ExecuteRootSelector() ?? this;
			}
			else if (elements.Current.Value == PathType.ParentSelector) //go one level up
			{
				if (Parent != null)
				{
					return FindNextNaturalContextObject()?.Parent ?? this.Parent ?? this;
				}

				return this;
			}
			else if (elements.Current.Value == PathType.ObjectSelector) 
				//enumerate ether an IDictionary, an cs object or an IEnumerable to a KeyValuePair array
			{
				//await EnsureValue();
				if (Value is null)
				{
					return Options.CreateContextObject("x:null", CancellationToken, null);
				}

				//ALWAYS return the context, even if the value is null.
				return ExecuteObjectSelector(elements.Current.Key, type);
			}
			else if (elements.Current.Value == PathType.Boolean)
			{
				if (elements.Current.Key == "true" || elements.Current.Key == "false")
				{
					var booleanContext =
						Options.CreateContextObject(".", CancellationToken, elements.Current.Key == "true", this);
					booleanContext.IsNaturalContext = IsNaturalContext;
					return booleanContext;
				}

				return this;
			}
			else if (elements.Current.Value == PathType.Null)
			{
				return Options.CreateContextObject("x:null", CancellationToken, null);
			}
			else if (elements.Current.Value == PathType.DataPath)
			{
				return ExecuteDataPath(elements.Current.Key, morestachioExpression, type);
			}

			return retval;
		}

		internal ContextObject ExecuteObjectSelector(string key, Type type)
		{
			ContextObject innerContext = null;
			if (!Options.HandleDictionaryAsObject && Value is IDictionary<string, object> dictList)
			{
				innerContext = Options.CreateContextObject(key, CancellationToken,
					dictList.Select(e => e), this);
			}
			else
			{
				if (Value != null)
				{
					innerContext = Options.CreateContextObject(key, CancellationToken,
						type
							.GetTypeInfo()
							.GetProperties(BindingFlags.Instance | BindingFlags.Public)
							.Where(e => !e.IsSpecialName && !e.GetIndexParameters().Any())
							.Select(e => new KeyValuePair<string, object>(e.Name, e.GetValue(Value))),
						this);
				}
			}

			return innerContext ?? this;
		}

		internal ContextObject ExecuteDataPath(string key, IMorestachioExpression morestachioExpression, Type type)
		{
			//await EnsureValue();
			if (Value is null)
			{
				return Options.CreateContextObject("x:null", CancellationToken, null);
			}
			//TODO: handle array accessors and maybe "special" keys.
			//ALWAYS return the context, even if the value is null.

			var innerContext = Options.CreateContextObject(key, CancellationToken, null, this);
			if (Options.ValueResolver?.CanResolve(type, Value, key, innerContext) == true)
			{
				innerContext._value = Options.ValueResolver.Resolve(type, Value, key, innerContext);
			}
			else if (!Options.HandleDictionaryAsObject && Value is IDictionary<string, object> ctx)
			{
				if (!ctx.TryGetValue(key, out var o))
				{
					Options.OnUnresolvedPath(new InvalidPathEventArgs(this, morestachioExpression,
						key, Value?.GetType()));
				}

				innerContext._value = o;
			}
			else if (Value is IMorestachioPropertyResolver cResolver)
			{
				if (!cResolver.TryGetValue(key, out var o))
				{
					Options.OnUnresolvedPath(new InvalidPathEventArgs(this, morestachioExpression,
						key, Value?.GetType()));
				}

				innerContext._value = o;
			}
			else if (Value != null)
			{
				if (Value is ICustomTypeDescriptor descriptor)
				{
					var propertyDescriptor = descriptor.GetProperties().Find(key, false);
					if (propertyDescriptor != null)
					{
						innerContext._value = propertyDescriptor.GetValue(Value);
					}
					else
					{
						Options.OnUnresolvedPath(new InvalidPathEventArgs(this, morestachioExpression,
							key, Value?.GetType()));
					}
				}
				else
				{
					var propertyInfo = type.GetTypeInfo().GetProperty(key);
					if (propertyInfo != null)
					{
						innerContext._value = propertyInfo.GetValue(Value);
					}
					else
					{
						Options.OnUnresolvedPath(new InvalidPathEventArgs(this, morestachioExpression,
							key, Value?.GetType()));
					}
				}
			}
			else
			{
				Options.OnUnresolvedPath(new InvalidPathEventArgs(this, morestachioExpression,
					key, Value?.GetType()));
			}

			return innerContext;
		}

		internal ContextObject ExecuteRootSelector()
		{
			var parent = this.Parent ?? this;
			var lastParent = parent;
			while (parent != null)
			{
				parent = parent.Parent;
				if (parent != null)
				{
					lastParent = parent;
				}
			}

			return lastParent;
		}
		
		/// <summary>
		///		Returns a variable that is only present in this context but not below it
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
		internal async ContextObjectPromise GetContextForPath(Traversable elements,
			ScopeData scopeData,
			IMorestachioExpression morestachioExpression)
		{
			if (Key == "x:null" || !elements.HasValue)
			{
				return this;
			}
			//look at the first element if its an alias switch to that alias
			//var peekPathPart = elements.Peek();
			if (elements.Count == 1 && elements.Current.Value == PathType.Null)
			{
				return Options.CreateContextObject("x:null", CancellationToken, null);
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

			return await targetContext.LoopContextTraversable(elements, scopeData, morestachioExpression);
			//return await targetContext.GetContextForPathInternal(elements, scopeData, morestachioExpression);
		}

		internal async ContextObjectPromise LoopContextTraversable(Traversable elements,
			ScopeData scopeData,
			IMorestachioExpression morestachioExpression)
		{
			ContextObject context = this;
			while (elements != null && elements.HasValue)
			{
				context = await context.GetContextForPathInternal(elements, scopeData, morestachioExpression);
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
			return DefinitionOfFalse(Value);
		}

		/// <summary>
		///     Renders the Current value to a string or if null to the Null placeholder in the Options
		/// </summary>
		/// <returns></returns>
		public virtual StringPromise RenderToString()
		{
			return (Value?.ToString() ?? Options.Null).ToPromise();
		}

		/// <summary>
		///     Gets an FormatterCache from ether the custom formatter or the global one
		/// </summary>
		/// <param name="type"></param>
		/// <param name="name"></param>
		/// <param name="arguments"></param>
		/// <param name="scope"></param>
		/// <returns></returns>
		public virtual FormatterCache? PrepareFormatterCall(Type type,
			 string name,
			 FormatterArgumentType[] arguments,
			ScopeData scope)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				name = null;
			}

			//call formatters that are given by the Options for this run
			var cache = Options.Formatters.PrepareCallMostMatchingFormatter(type, arguments, name, Options, scope);
			if (cache != null)
			{
				//one formatter has returned a valid value so use this one.
				return cache;
			}

			//all formatters in the options object have rejected the value so try use the global ones
			return DefaultFormatter.PrepareCallMostMatchingFormatter(type, arguments, name, Options, scope);
		}

		/// <summary>
		///     Clones the ContextObject into a new Detached object
		/// </summary>
		/// <returns></returns>
		public virtual ContextObject CloneForEdit()
		{
			var contextClone = new ContextObject(Options, Key, this, _value) //note: Parent must be the original context so we can traverse up to an unmodified context
			{
				IsNaturalContext = false,
			};

			return contextClone;
		}

		/// <summary>
		///     Clones the ContextObject into a new Detached object
		/// </summary>
		/// <returns></returns>
		public virtual ContextObject CloneForEdit(object newValue)
		{
			var contextClone = new ContextObject(Options, Key, this, newValue) //note: Parent must be the original context so we can traverse up to an unmodified context
			{
				IsNaturalContext = false,
			};

			return contextClone;
		}

		/// <summary>
		///     Clones the ContextObject into a new Detached object
		/// </summary>
		/// <returns></returns>
		public virtual ContextObject Copy()
		{
			var contextClone = new ContextObject(Options, Key, Parent, _value) //note: Parent must be the original context so we can traverse up to an unmodified context
			{
				IsNaturalContext = true,
			};

			return contextClone;
		}
	}
}