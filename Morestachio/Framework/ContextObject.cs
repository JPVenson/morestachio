using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Morestachio.Attributes;
using Morestachio.Formatter.Framework;
using Morestachio.Formatter.Predefined;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Helper;
using PathPartElement = System.Collections.Generic.KeyValuePair<string, Morestachio.Framework.Expression.Framework.PathType>;

namespace Morestachio.Framework
{
	/// <summary>
	///     The current context for any given expression
	/// </summary>
	public class ContextObject
	{
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
			DefaultDefinitionOfFalse = (value) => value != null &&
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
		///		<para>Gets the Default Definition of false.</para>
		///		This is ether:
		///		<para>- Null</para>
		///		<para>- boolean false</para>
		///		<para>- 0 double or int</para>
		///		<para>- string.Empty (whitespaces are allowed)</para>
		///		<para>- collection not Any().</para>
		///		This field can be used to define your own <see cref="DefinitionOfFalse"/> and then fallback to the default logic
		/// </summary>
		public static readonly Func<object, bool> DefaultDefinitionOfFalse;


		/// <summary>
		///		Gets the Definition of false on your Template.
		/// </summary>
		/// <value>
		///		Must no be null
		/// </value>
		/// <exception cref="InvalidOperationException">If the value is null</exception>
		[NotNull]
		public static Func<object, bool> DefinitionOfFalse
		{
			get { return _definitionOfFalse; }
			set
			{
				_definitionOfFalse = value ?? throw new InvalidOperationException("The value must not be null");
			}
		}

		private static Func<object, bool> _definitionOfFalse;
		[CanBeNull] private object _value;
		[CanBeNull] private readonly ContextObject _parent;

		/// <summary>
		/// Initializes a new instance of the <see cref="ContextObject"/> class.
		/// </summary>
		/// <param name="options">The options.</param>
		/// <param name="key">The key as seen in the Template</param>
		/// <param name="parent">The Logical parent of this ContextObject</param>
		public ContextObject([NotNull]ParserOptions options, [NotNull]string key, [CanBeNull]ContextObject parent, object value)
		{
			Options = options ?? throw new ArgumentNullException(nameof(options));
			Key = key;
			_parent = parent;
			_value = value;
			if (_parent != null)
			{
				CancellationToken = _parent.CancellationToken;
				AbortGeneration = _parent.AbortGeneration;
			}
			IsNaturalContext = _parent?.IsNaturalContext ?? true;
		}

		/// <summary>
		///		Gets a value indicating whether this instance is natural context.
		///		A Natural context is a context outside an Isolated scope
		/// </summary>
		public bool IsNaturalContext { get; protected set; }

		internal ContextObject FindNextNaturalContextObject()
		{
			var context = this;
			while (context != null && !context.IsNaturalContext)
			{
				context = context._parent;
			}

			return context;
		}

		/// <summary>
		///     The set of allowed types that may be printed. Complex types (such as arrays and dictionaries)
		///     should not be printed, or their printing should be specialized.
		///     Add an typeof(object) entry as Type to define a Default Output
		/// </summary>
		[NotNull]
		public static IMorestachioFormatterService DefaultFormatter { get; private set; }

		/// <summary>
		///     The parent of the current context or null if its the root context
		/// </summary>
		[CanBeNull]
		public ContextObject Parent
		{
			get { return _parent; }
		}

		/// <summary>
		///     The evaluated value of the expression
		/// </summary>
		[CanBeNull]
		public object Value
		{
			get { return _value; }
			set { _value = value; }
		}

		/// <summary>
		///		Makes this instance natural
		/// </summary>
		/// <returns></returns>
		internal ContextObject MakeNatural()
		{
			this.IsNaturalContext = true;
			return this;
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

		/// <summary>
		///     if overwritten by a class it returns a context object for any non standard key or operation.
		///     if non of that
		///     <value>null</value>
		/// </summary>
		/// <returns></returns>
		protected virtual ContextObject HandlePathContext(Traversable elements,
			PathPartElement currentElement, 
			IMorestachioExpression morestachioExpression)
		{
			return null;
		}

		private async Task<ContextObject> GetContextForPath(
			Traversable elements,
			ScopeData scopeData, 
			IMorestachioExpression morestachioExpression)
		{
			var retval = this;
			if (elements.Any())
			{
				var path = elements.Dequeue();
				var preHandeld = HandlePathContext(elements, path, morestachioExpression);
				if (preHandeld != null)
				{
					return preHandeld;
				}
				var type = _value?.GetType();

				if (path.Value == PathType.RootSelector) //go the root object
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

					if (lastParent != null)
					{
						retval = await lastParent.GetContextForPath(elements, scopeData, morestachioExpression);
					}
				}
				else if (path.Value == PathType.ParentSelector) //go one level up
				{
					if (_parent != null)
					{
						var parentsRetVal = (await (FindNextNaturalContextObject()?._parent?.GetContextForPath(elements, scopeData, morestachioExpression) ?? Task.FromResult((ContextObject)null)));
						if (parentsRetVal != null)
						{
							retval = parentsRetVal;
						}
						else
						{
							retval = await GetContextForPath(elements, scopeData, morestachioExpression);
						}
					}
					else
					{
						retval = await GetContextForPath(elements, scopeData, morestachioExpression);
					}
				}
				else if (path.Value == PathType.ObjectSelector) //enumerate ether an IDictionary, an cs object or an IEnumerable to a KeyValuePair array
				{
					//await EnsureValue();
					if (_value is null)
					{
						return Options.CreateContextObject("x:null", CancellationToken, null);
					}
					//ALWAYS return the context, even if the value is null.
					ContextObject innerContext = null;
					switch (_value)
					{
						case IDictionary<string, object> dictList:
							innerContext = Options.CreateContextObject(path.Key, CancellationToken,
								dictList.Select(e => e), this);
							break;
						default:
							{
								if (_value != null)
								{
									innerContext = Options.CreateContextObject(path.Key, CancellationToken,
										type
											.GetTypeInfo()
											.GetProperties(BindingFlags.Instance | BindingFlags.Public)
											.Where(e => !e.IsSpecialName && !e.GetIndexParameters().Any())
											.Select(e => new KeyValuePair<string, object>(e.Name, e.GetValue(_value))),
										this);
								}

								break;
							}
					}

					retval = await innerContext.GetContextForPath(elements, scopeData, morestachioExpression);
				}
				//else if (path.Value == PathType.Number)
				//{
				//	//check if this part of the path can be seen as an number
				//	if (Number.TryParse(path.Key, Options.CultureInfo, out var isNumber))
				//	{
				//		var contextObject = Options.CreateContextObject(".", CancellationToken, isNumber, this);
				//		contextObject.IsNaturalContext = IsNaturalContext;
				//		return await contextObject.GetContextForPath(elements, scopeData, morestachioExpression);
				//	}
				//}
				else if (path.Value == PathType.Boolean)
				{
					if (path.Key == "true" || path.Key == "false")
					{
						var booleanContext = Options.CreateContextObject(".", CancellationToken, path.Key == "true", this);
						booleanContext.IsNaturalContext = IsNaturalContext;
						return await booleanContext.GetContextForPath(elements, scopeData, morestachioExpression);
					}
				}
				else if (path.Value == PathType.DataPath)
				{
					if (path.Key.Equals("$recursion")) //go the root object
					{
						retval = Options.CreateContextObject(path.Key, CancellationToken, scopeData.PartialDepth.Count, this);
					}
					else
					{
						//await EnsureValue();
						if (_value is null)
						{
							return Options.CreateContextObject("x:null", CancellationToken, null);
						}
						//TODO: handle array accessors and maybe "special" keys.
						else
						{
							//ALWAYS return the context, even if the value is null.

							var innerContext = Options.CreateContextObject(path.Key, CancellationToken, null, this);
							if (Options.ValueResolver?.CanResolve(type, _value, path.Key, innerContext) == true)
							{
								innerContext._value = Options.ValueResolver.Resolve(type, _value, path.Key, innerContext);
							}
							else if (_value is IDictionary<string, object> ctx)
							{
								if (!ctx.TryGetValue(path.Key, out var o))
								{
									Options.OnUnresolvedPath(new InvalidPathEventArgs(this, morestachioExpression, path.Key, _value?.GetType()));
								}
								innerContext._value = o;
							}
							else if (_value != null)
							{
								var propertyInfo = type.GetTypeInfo().GetProperty(path.Key);
								if (propertyInfo != null)
								{
									innerContext._value = propertyInfo.GetValue(_value);
								}
								else
								{
									Options.OnUnresolvedPath(new InvalidPathEventArgs(this, morestachioExpression, path.Key, _value?.GetType()));
								}
							}

							retval = await innerContext.GetContextForPath(elements, scopeData, morestachioExpression);
						}
					}
				}
			}

			return retval;
		}

		/// <summary>
		///     Will walk the path by using the path seperator "." and evaluate the object at the end
		/// </summary>
		/// <param name="pathParts"></param>
		/// <param name="scopeData"></param>
		/// <param name="morestachioExpression"></param>
		/// <returns></returns>
		internal async Task<ContextObject> GetContextForPath(IList<PathPartElement> pathParts,
			ScopeData scopeData, 
			IMorestachioExpression morestachioExpression)
		{
			if (Key == "x:null")
			{
				return this;
			}

			var elements = new Traversable(pathParts);
			//foreach (var m in PathFinder.Matches(path).OfType<Match>())
			//{
			//	elements.Enqueue(m.Value);
			//}

			if (elements.Any())
			{
				//look at the first element if its an alias switch to that alias
				var peekPathPart = elements.Peek();
				if (elements.Count == 1 && peekPathPart.Value == PathType.Null)
				{
					return Options.CreateContextObject("x:null", CancellationToken, null);
				}

				if (peekPathPart.Value == PathType.DataPath)
				{
					var getFromAlias = scopeData.GetVariable(peekPathPart.Key);
					if (getFromAlias != null)
					{
						elements.Dequeue();
						return await getFromAlias.GetContextForPath(elements, scopeData, morestachioExpression);
					}
				}
			}

			return await GetContextForPath(elements, scopeData, morestachioExpression);
		}
		/// <summary>
		///     Determines if the value of this context exists.
		/// </summary>
		/// <returns></returns>
		public virtual async Task<bool> Exists()
		{
			//await EnsureValue();
			return DefinitionOfFalse(_value);
		}

		/// <summary>
		///		Renders the Current value to a string or if null to the Null placeholder in the Options
		/// </summary>
		/// <returns></returns>
		public virtual async Task<string> RenderToString()
		{
			//await EnsureValue();
			return _value?.ToString() ?? (Options.Null?.ToString());
		}

		/// <summary>
		///     Parses the current object by using the given argument
		/// </summary>
		public virtual async Task<object> Format([CanBeNull] string name, [NotNull] List<Tuple<string, object>> argument)
		{
			//await EnsureValue();
			var retval = _value;
			if (_value == null)
			{
				return retval;
			}

			if (string.IsNullOrWhiteSpace(name))
			{
				name = null;
			}

			//call formatters that are given by the Options for this run
			retval = await Options.Formatters.CallMostMatchingFormatter(_value.GetType(), argument, _value, name, Options);
			if (!Equals(retval, MorestachioFormatterService.FormatterFlow.Skip))
			{
				//one formatter has returned a valid value so use this one.
				return retval;
			}

			//all formatters in the options object have rejected the value so try use the global ones
			retval = await DefaultFormatter.CallMostMatchingFormatter(_value.GetType(), argument, _value, name, Options);
			if (!Equals(retval, MorestachioFormatterService.FormatterFlow.Skip))
			{
				return retval;
			}
			return _value;
		}

		/// <summary>
		///     Parses the current object by using the given argument
		/// </summary>
		public virtual async Task<object> Operator([CanBeNull] string name, [CanBeNull] ContextObject other)
		{
			//await EnsureValue();
			var retval = _value;
			if (_value == null)
			{
				return retval;
			}

			if (string.IsNullOrWhiteSpace(name))
			{
				return Value;
			}

			//name =  name;

			//call formatters that are given by the Options for this run
			var values = new List<Tuple<string, object>>();
			values.Add(Tuple.Create((string)null, other.Value));
			retval = await Options.Formatters.CallMostMatchingFormatter(_value.GetType(), values, _value, name, Options);
			if (!Equals(retval, MorestachioFormatterService.FormatterFlow.Skip))
			{
				//one formatter has returned a valid value so use this one.
				return retval;
			}

			//all formatters in the options object have rejected the value so try use the global ones
			retval = await DefaultFormatter.CallMostMatchingFormatter(_value.GetType(), values, _value, name, Options);
			if (!Equals(retval, MorestachioFormatterService.FormatterFlow.Skip))
			{
				return retval;
			}
			return _value;
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
			var contextClone = new ContextObject(Options, Key, _parent, _value) //note: Parent must be the original context so we can traverse up to an unmodified context
			{
				IsNaturalContext = true,
				AbortGeneration = AbortGeneration
			};

			return contextClone;
		}
	}
}