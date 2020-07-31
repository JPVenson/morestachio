using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;
using Morestachio.Formatter.Framework;
using Morestachio.Formatter.Predefined;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Helper;
using PathPartElement =
	System.Collections.Generic.KeyValuePair<string, Morestachio.Framework.Expression.Framework.PathType>;
#if ValueTask
using Promise = System.Threading.Tasks.ValueTask;
using ContextObjectPromise = System.Threading.Tasks.ValueTask<Morestachio.Framework.ContextObject>;
using StringPromise = System.Threading.Tasks.ValueTask<string>;
using ObjectPromise = System.Threading.Tasks.ValueTask<object>;
#else
using Promise = System.Threading.Tasks.Task;
using ContextObjectPromise = System.Threading.Tasks.Task<Morestachio.Framework.ContextObject>;
using StringPromise = System.Threading.Tasks.Task<string>;
using ObjectPromise = System.Threading.Tasks.Task<object>;

#endif

namespace Morestachio.Framework
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
		[CanBeNull] private object _value;

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
		public ContextObject([NotNull] ParserOptions options, [NotNull] string key, [CanBeNull] ContextObject parent,
			object value)
		{
			Options = options ?? throw new ArgumentNullException(nameof(options));
			Key = key;
			Parent = parent;
			Value = value;
			if (Parent != null)
			{
				CancellationToken = Parent.CancellationToken;
				AbortGeneration = Parent.AbortGeneration;
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
		[NotNull]
		public static Func<object, bool> DefinitionOfFalse
		{
			get { return _definitionOfFalse; }
			set { _definitionOfFalse = value ?? throw new InvalidOperationException("The value must not be null"); }
		}

		/// <summary>
		///     Gets a value indicating whether this instance is natural context.
		///     A Natural context is a context outside an Isolated scope
		/// </summary>
		public bool IsNaturalContext { get; protected set; }

		/// <summary>
		///     The set of allowed types that may be printed. Complex types (such as arrays and dictionaries)
		///     should not be printed, or their printing should be specialized.
		///     Add an typeof(object) entry as Type to define a Default Output
		/// </summary>
		[NotNull]
		public static IMorestachioFormatterService DefaultFormatter { get; }

		/// <summary>
		///     The parent of the current context or null if its the root context
		/// </summary>
		[CanBeNull]
		public ContextObject Parent { get; }

		/// <summary>
		///     The evaluated value of the expression
		/// </summary>
		[CanBeNull]
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
		public bool AbortGeneration { get; set; }

		/// <summary>
		///     The name of the property or key inside the value or indexer expression for lists
		/// </summary>
		[NotNull]
		public string Key { get; }

		/// <summary>
		///     With what options are the template currently is running
		/// </summary>
		[NotNull]
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
		protected virtual ContextObject HandlePathContext(Traversable elements,
			PathPartElement currentElement,
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

			var preHandeld = HandlePathContext(elements, elements.Current, morestachioExpression, scopeData);
			if (preHandeld != null)
			{
				return preHandeld;
			}

			var type = Value?.GetType();

			if (elements.Current.Value == PathType.RootSelector) //go the root object
			{
				var parent = Parent ?? this;
				var lastParent = parent;
				while (parent != null)
				{
					parent = parent.Parent;
					if (parent != null)
					{
						lastParent = parent;
					}
				}

				if (lastParent != null)
				{
					retval = await lastParent.GetContextForPathInternal(elements.Dequeue(), scopeData,
						morestachioExpression);
				}
			}
			else if (elements.Current.Value == PathType.ParentSelector) //go one level up
			{
				if (Parent != null)
				{
					var parent = FindNextNaturalContextObject();
					ContextObject parentsRetVal = null;
					if (parent != null && parent.Parent != null)
					{
						parentsRetVal = await parent.Parent
							.GetContextForPathInternal(elements.Dequeue(), scopeData, morestachioExpression);
					}

					if (parentsRetVal != null)
					{
						retval = parentsRetVal;
					}
					else
					{
						retval = await GetContextForPathInternal(elements.Dequeue(), scopeData, morestachioExpression);
					}
				}
				else
				{
					retval = await GetContextForPathInternal(elements.Dequeue(), scopeData, morestachioExpression);
				}
			}
			else if (elements.Current.Value == PathType.ObjectSelector
			) //enumerate ether an IDictionary, an cs object or an IEnumerable to a KeyValuePair array
			{
				//await EnsureValue();
				if (Value is null)
				{
					return Options.CreateContextObject("x:null", CancellationToken, null);
				}

				//ALWAYS return the context, even if the value is null.
				ContextObject innerContext = null;
				switch (Value)
				{
					case IDictionary<string, object> dictList:
						innerContext = Options.CreateContextObject(elements.Current.Key, CancellationToken,
							dictList.Select(e => e), this);
						break;
					default:
						{
							if (Value != null)
							{
								innerContext = Options.CreateContextObject(elements.Current.Key, CancellationToken,
									type
										.GetTypeInfo()
										.GetProperties(BindingFlags.Instance | BindingFlags.Public)
										.Where(e => !e.IsSpecialName && !e.GetIndexParameters().Any())
										.Select(e => new KeyValuePair<string, object>(e.Name, e.GetValue(Value))),
									this);
							}

							break;
						}
				}

				retval = await innerContext.GetContextForPathInternal(elements.Dequeue(), scopeData,
					morestachioExpression);
			}
			else if (elements.Current.Value == PathType.Boolean)
			{
				if (elements.Current.Key == "true" || elements.Current.Key == "false")
				{
					var booleanContext =
						Options.CreateContextObject(".", CancellationToken, elements.Current.Key == "true", this);
					booleanContext.IsNaturalContext = IsNaturalContext;
					return await booleanContext.GetContextForPathInternal(elements.Dequeue(), scopeData,
						morestachioExpression);
				}
			}
			else if (elements.Current.Value == PathType.Null)
			{
				return Options.CreateContextObject("x:null", CancellationToken, null, null);
			}
			else if (elements.Current.Value == PathType.DataPath)
			{
				//await EnsureValue();
				if (Value is null)
				{
					return Options.CreateContextObject("x:null", CancellationToken, null);
				}
				//TODO: handle array accessors and maybe "special" keys.
				//ALWAYS return the context, even if the value is null.

				var innerContext = Options.CreateContextObject(elements.Current.Key, CancellationToken, null, this);
				if (Options.ValueResolver?.CanResolve(type, Value, elements.Current.Key, innerContext) == true)
				{
					innerContext.Value = Options.ValueResolver.Resolve(type, Value, elements.Current.Key, innerContext);
				}
				else if (Value is IDictionary<string, object> ctx)
				{
					if (!ctx.TryGetValue(elements.Current.Key, out var o))
					{
						Options.OnUnresolvedPath(new InvalidPathEventArgs(this, morestachioExpression,
							elements.Current.Key, Value?.GetType()));
					}

					innerContext.Value = o;
				}
				else if (Value is IMorestachioPropertyResolver cResolver)
				{
					if (!cResolver.TryGetValue(elements.Current.Key, out var o))
					{
						Options.OnUnresolvedPath(new InvalidPathEventArgs(this, morestachioExpression,
							elements.Current.Key, Value?.GetType()));
					}

					innerContext.Value = o;
				}
				else if (Value != null)
				{
					var propertyInfo = type.GetTypeInfo().GetProperty(elements.Current.Key);
					if (propertyInfo != null)
					{
						innerContext.Value = propertyInfo.GetValue(Value);
					}
					else
					{
						Options.OnUnresolvedPath(new InvalidPathEventArgs(this, morestachioExpression,
							elements.Current.Key, Value?.GetType()));
					}
				}

				retval = await innerContext.GetContextForPathInternal(elements.Dequeue(), scopeData,
					morestachioExpression);
			}

			return retval;
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

			if (elements.Current.Value == PathType.DataPath)
			{
				var getFromAlias = scopeData.GetVariable(elements.Current.Key);
				if (getFromAlias != null)
				{
					elements = elements.Dequeue();
					return await getFromAlias.GetContextForPathInternal(elements, scopeData, morestachioExpression);
				}
			}

			return await GetContextForPathInternal(elements, scopeData, morestachioExpression);
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
			[CanBeNull] string name,
			[NotNull] FormatterArgumentType[] arguments,
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
				AbortGeneration = AbortGeneration
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
				AbortGeneration = AbortGeneration
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
				AbortGeneration = AbortGeneration
			};

			return contextClone;
		}
	}

	/// <summary>
	///     Can be implemented by an object to provide custom resolving logic
	/// </summary>
	public interface IMorestachioPropertyResolver
	{
		/// <summary>
		///     Gets the value of the property from this object
		/// </summary>
		bool TryGetValue(string name, out object found);
	}
}