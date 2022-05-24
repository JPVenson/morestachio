using System.Diagnostics;
using System.Xml;
using System.Xml.Schema;
using Morestachio.Document;
using Morestachio.Fluent.Expression;
using Morestachio.Formatter.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Expression.Visitors;

namespace Morestachio.Framework.Expression;

/// <summary>
///		Defines a path with an optional formatting expression including sub expressions
/// </summary>
[DebuggerTypeProxy(typeof(ExpressionDebuggerDisplay))]
[Serializable]
public class MorestachioExpression : IMorestachioExpression
{
	internal MorestachioExpression()
	{
		PathParts = Traversable.Empty;
		Formats = new List<ExpressionArgument>();
	}

	internal MorestachioExpression(in CharacterLocation location) : this()
	{
		Location = location;
	}

	/// <summary>
	///		Serialization constructor 
	/// </summary>
	/// <param name="info"></param>
	/// <param name="context"></param>
	protected MorestachioExpression(SerializationInfo info, StreamingContext context)
	{
		PathParts = new Traversable(info.GetValue(nameof(PathParts), typeof(KeyValuePair<string, PathType>[])) as KeyValuePair<string, PathType>[]);
		Formats = info.GetValue(nameof(Formats), typeof(IList<ExpressionArgument>))
			as IList<ExpressionArgument>;
		FormatterName = info.GetString(nameof(FormatterName));
		Location = CharacterLocation.FromFormatString(info.GetString(nameof(Location)));
		EndsWithDelimiter = info.GetBoolean(nameof(EndsWithDelimiter));
	}

	/// <inheritdoc />
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue(nameof(PathParts), PathParts.ToArray());
		info.AddValue(nameof(Formats), Formats);
		info.AddValue(nameof(FormatterName), FormatterName);
		info.AddValue(nameof(Location), Location.ToFormatString());
		info.AddValue(nameof(EndsWithDelimiter), EndsWithDelimiter);
	}

	/// <inheritdoc />
	public XmlSchema GetSchema()
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	public void ReadXml(XmlReader reader)
	{
		Location = CharacterLocation.FromFormatString(reader.GetAttribute(nameof(Location)));
		EndsWithDelimiter = reader.GetAttribute(nameof(EndsWithDelimiter)) == bool.TrueString;
		var pathParts = new List<KeyValuePair<string, PathType>>();
		reader.ReadStartElement();//Path

		if (reader.Name == "Path")
		{
			reader.ReadStartElement();//Any SubPath

			while (reader.Name != "Path" && reader.NodeType != XmlNodeType.EndElement)
			{
				var partName = reader.Name;
				string partValue = null;
				if (reader.IsEmptyElement)
				{
					reader.ReadStartElement();
				}
				else
				{
					partValue = reader.ReadElementContentAsString();
				}
				pathParts.Add(new KeyValuePair<string, PathType>(partValue, (PathType)Enum.Parse(typeof(PathType), partName)));
			}
			reader.ReadEndElement();//</Path>
		}
		PathParts = new Traversable(pathParts);
		if (reader.Name == "Format" && reader.NodeType == XmlNodeType.Element)
		{
			FormatterName = reader.GetAttribute(nameof(FormatterName));
			if (reader.IsEmptyElement)
			{
				reader.ReadStartElement();
			}
			else
			{
				reader.ReadStartElement(); //<Argument>
				while (reader.Name == "Argument" && reader.NodeType != XmlNodeType.EndElement)
				{
					var formatSubTree = reader.ReadSubtree();
					formatSubTree.Read();

					var expressionArgument = new ExpressionArgument();
					Formats.Add(expressionArgument);

					expressionArgument.ReadXml(formatSubTree);

					reader.Skip();
					reader.ReadEndElement();
				}
				reader.ReadEndElement();//</Format>
			}
		}
		reader.ReadEndElement();
	}

	/// <inheritdoc />
	public void WriteXml(XmlWriter writer)
	{
		writer.WriteAttributeString(nameof(Location), Location.ToFormatString());
		if (EndsWithDelimiter)
		{
			writer.WriteAttributeString(nameof(EndsWithDelimiter), bool.TrueString);
		}

		if (PathParts.Any())
		{
			writer.WriteStartElement("Path");
			foreach (var pathPart in PathParts.ToArray())
			{
				writer.WriteElementString(pathPart.Value.ToString(), pathPart.Key);
			}
			writer.WriteEndElement();//</Path>
		}
		if (FormatterName != null)
		{
			writer.WriteStartElement("Format");
			writer.WriteAttributeString(nameof(FormatterName), FormatterName);
			foreach (var expressionArgument in Formats)
			{
				writer.WriteStartElement("Argument");
				expressionArgument.WriteXml(writer);
				writer.WriteEndElement();//</Argument>
			}
			writer.WriteEndElement();//</Format>
		}
	}

	/// <summary>
	///		Contains all parts of the path
	/// </summary>
	public Traversable PathParts { get; internal set; }

	/// <summary>
	///		If filled contains the arguments to be used to format the value located at PathParts
	/// </summary>
	public IList<ExpressionArgument> Formats { get; internal set; }

	/// <summary>
	///		If set the formatter name to be used to format the value located at PathParts
	/// </summary>
	public string FormatterName { get; internal set; }

	/// <inheritdoc />
	public CharacterLocation Location { get; private set; }

	/// <summary>
	///		The prepared call for an formatter
	/// </summary>
	public FormatterCache Cache { get; private set; }

	/// <summary>
	///		Gets whenever this expression was explicitly closed
	/// </summary>
	public bool EndsWithDelimiter { get; private set; }

	/// <param name="parserOptions"></param>
	/// <inheritdoc />
	public CompiledExpression Compile(ParserOptions parserOptions)
	{
		if (!PathParts.HasValue && Formats.Count == 0 && FormatterName == null)
		{
			return (contextObject, _) => contextObject.ToPromise();
		}
		if (!PathParts.HasValue && Formats.Count == 1 && FormatterName == "")
		{
			//this enables the usage of brackets. A formatter that has no name and one argument e.g ".(data + 1)" or "(data + 1)" should be considered a bracket
			return (contextObject, data) => Formats[0].GetValue(contextObject, data);
		}

		if (PathParts.Count == 1 && PathParts.Current.Value == PathType.Null)
		{
			var nullValue = parserOptions.CreateContextObject("x:null", null).ToPromise();
			return (_, _) => nullValue;
		}

		var pathParts = PathParts.ToArray();

		Func<ContextObject, ScopeData, IMorestachioExpression, ContextObject> getContext = null;

		if (pathParts.Length != 0)
		{
			var pathQueue = new Func<ContextObject, ScopeData, IMorestachioExpression, ContextObject>[pathParts.Length];
			var idx = 0;
			if (pathParts.Length > 0 && pathParts.First().Value == PathType.DataPath)
			{
				var key = pathParts[0].Key;
				pathQueue[idx++] = ((context, scopeData, expression) =>
				{
					return scopeData.GetVariable(context, key) ?? context.ExecuteDataPath(key, expression, scopeData);
				});
			}

			for (; idx < pathQueue.Length;)
			{
				var pathPart = pathParts[idx];
				var key = pathPart.Key;
				switch (pathPart.Value)
				{
					case PathType.DataPath:
						pathQueue[idx++] = ((contextObject, scopeData, expression) =>
						{
							return contextObject.ExecuteDataPath(key, expression, scopeData);
						});
						break;
					case PathType.RootSelector:
						pathQueue[idx++] = ((contextObject, _, _) =>
						{
							return contextObject.ExecuteRootSelector();
						});
						break;
					case PathType.ParentSelector:
						pathQueue[idx++] = ((contextObject, _, _) =>
						{
							var natContext = contextObject.FindNextNaturalContextObject();
							return (natContext?.Parent ?? contextObject);
						});
						break;
					case PathType.ObjectSelector:
						pathQueue[idx++] = ((contextObject, scopeData, _) =>
						{
							return contextObject.ExecuteObjectSelector(key, scopeData);
						});
						break;
					case PathType.Null:
						var nullValue = parserOptions.CreateContextObject("x:null", null);
						pathQueue[idx++] = ((_, _, _) => nullValue);
						break;
					case PathType.Boolean:
						var booleanValue = key == "true";
						var booleanContext = parserOptions.CreateContextObject(".", booleanValue);
						booleanContext.IsNaturalContext = true;
						pathQueue[idx++] = ((_, _, _) => booleanContext);
						break;
					case PathType.SelfAssignment:
					case PathType.ThisPath:
						pathQueue[idx++] = ((contextObject, _, _) => contextObject);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			if (pathQueue.Length == 1)
			{
				getContext = pathQueue[0];
			}
			else
			{
				getContext =
					(contextObject, data, expression) =>
					{
						for (var index = 0; index < pathQueue.Length; index++)
						{
							var func = pathQueue[index];
							contextObject = func(contextObject, data, expression);
						}

						return contextObject;
					};
			}
		}

		if (!Formats.Any() && FormatterName == null)
		{
			if (getContext == null)
			{
				return (contextObject, _) => contextObject.ToPromise();
			}

			return (contextObject, data) => getContext(contextObject, data, this).ToPromise();
		}

		var formatsCompiled = Formats.ToDictionary(f => f, f =>
		{
			if (f.IsCompileTimeEval())
			{
				return f.GetCompileTimeValue();
			}

			return f.Compile(parserOptions);
		}).ToArray();

		var arguments = new FormatterArgumentType[formatsCompiled.Length];
		var allConstants = formatsCompiled.All(e => e.Key.IsCompileTimeEval());

		if (allConstants)
		{
			for (var index = 0; index < formatsCompiled.Length; index++)
			{
				var keyValuePair = formatsCompiled[index];
				arguments[index] = new FormatterArgumentType(index, keyValuePair.Key.Name, keyValuePair.Value, keyValuePair.Key.MorestachioExpression);
			}
		}

		FormatterCache cache = null;

		async Promise CallFormatter(
			ContextObject naturalContext,
			ContextObject outputContext,
			ScopeData scopeData)
		{
			if (!allConstants)
			{
				for (var index = 0; index < formatsCompiled.Length; index++)
				{
					var formatterArgument = formatsCompiled[index];

					object value;
					if (formatterArgument.Value is CompiledExpression cex)
					{
						value = (await cex(naturalContext, scopeData).ConfigureAwait(false))?.Value;
					}
					else
					{
						value = formatterArgument.Value;
					}

					arguments[index] = new FormatterArgumentType(index, formatterArgument.Key.Name, value, formatterArgument.Key.MorestachioExpression);
				}
			}

			if (cache == null)
			{
				cache = outputContext.PrepareFormatterCall(
					outputContext.Value?.GetType() ?? typeof(object),
					FormatterName,
					arguments,
					scopeData);
			}

			if (cache != null)
			{
				outputContext._value = await scopeData.ParserOptions.Formatters.Execute(cache, outputContext.Value, scopeData.ParserOptions, arguments).ConfigureAwait(false);
				outputContext.MakeSyntetic();
			}
		}

		if (getContext == null)
		{
			return async (contextObject, scopeData) =>
			{
				var ctx = scopeData.ParserOptions.CreateContextObject("", contextObject.Value,
					contextObject);
				contextObject = contextObject.FindNextNaturalContextObject();
				await CallFormatter(contextObject, ctx, scopeData).ConfigureAwait(false);
				return ctx;
			};
		}

		return async (contextObject, scopeData) =>
		{
			var ctx = getContext(contextObject, scopeData, this);
			await CallFormatter(contextObject, ctx, scopeData).ConfigureAwait(false);
			return ctx;
		};
	}

	/// <inheritdoc />
	public async ContextObjectPromise GetValue(ContextObject contextObject, ScopeData scopeData)
	{
		if (!PathParts.Any() && Formats.Count == 1 && FormatterName == "")
		{
			//indicates the usage of brackets
			return await Formats[0].GetValue(contextObject, scopeData).ConfigureAwait(false);
		}

		var contextForPath = contextObject.GetContextForPath(PathParts, scopeData, this);
		if (!Formats.Any() && FormatterName == null)
		{
			return contextForPath;
		}

		if (contextForPath == contextObject)
		{
			contextForPath = contextObject.CloneForEdit();
		}

		var arguments = new FormatterArgumentType[Formats.Count];
		var naturalValue = contextObject.FindNextNaturalContextObject();
		for (var index = 0; index < Formats.Count; index++)
		{
			var formatterArgument = Formats[index];
			var value = await formatterArgument.MorestachioExpression.GetValue(naturalValue, scopeData).ConfigureAwait(false);
			arguments[index] = new FormatterArgumentType(index, formatterArgument.Name, value?.Value, formatterArgument.MorestachioExpression);
		}
		//contextForPath.Value = await contextForPath.Format(FormatterName, argList, scopeData);

		Cache ??= contextForPath.PrepareFormatterCall(
			contextForPath.Value?.GetType() ?? typeof(object),
			FormatterName,
			arguments,
			scopeData);

		if (Cache != null)
		{
			contextForPath.Value = await scopeData.ParserOptions.Formatters.Execute(Cache, contextForPath.Value, scopeData.ParserOptions, arguments).ConfigureAwait(false);
			contextForPath.MakeSyntetic();
		}
		return contextForPath;
	}

	/// <inheritdoc />
	public void Accept(IMorestachioExpressionVisitor visitor)
	{
		visitor.Visit(this);
	}

	/// <inheritdoc />
	public bool IsCompileTimeEval()
	{
		if (!PathParts.HasValue)
		{
			return true;
		}

		if (PathParts.Count == 1)
		{
			if (PathParts.Current.Value == PathType.Boolean)
			{
				return true;
			}

			if (PathParts.Current.Value == PathType.Null)
			{
				return true;
			}
		}

		return false;
	}

	/// <inheritdoc />
	public object GetCompileTimeValue()
	{
		if (!PathParts.HasValue)
		{
			return null;
		}
		if (PathParts.Count == 1)
		{
			if (PathParts.Current.Value == PathType.Boolean)
			{
				return PathParts.Current.Key == "true";
			}

			if (PathParts.Current.Value == PathType.Null)
			{
				return null;
			}
		}

		return null;
	}

	/// <inheritdoc />
	protected bool Equals(MorestachioExpression other)
	{
		if (other.PathParts.Count != PathParts.Count)
		{
			return false;
		}
		if (other.Formats.Count != Formats.Count)
		{
			return false;
		}

		if (other.FormatterName != FormatterName)
		{
			return false;
		}

		if (!other.Location.Equals(Location))
		{
			return false;
		}

		var parts = PathParts.ToArray();
		var otherParts = other.PathParts.ToArray();
		if (parts.Length != otherParts.Length || Formats.Count != other.Formats.Count)
		{
			return false;
		}

		for (var index = 0; index < parts.Length; index++)
		{
			var thisPart = parts[index];
			var thatPart = otherParts[index];
			if (thatPart.Value != thisPart.Value || thatPart.Key != thisPart.Key)
			{
				return false;
			}
		}

		for (var index = 0; index < Formats.Count; index++)
		{
			var thisArgument = Formats[index];
			var thatArgument = other.Formats[index];
			if (!thisArgument.Equals(thatArgument))
			{
				return false;
			}
		}

		return true;
	}

	/// <inheritdoc />
	public bool Equals(IMorestachioExpression other)
	{
		return Equals((object)other);
	}

	/// <inheritdoc />
	public override bool Equals(object obj)
	{
		if (ReferenceEquals(null, obj))
		{
			return false;
		}

		if (ReferenceEquals(this, obj))
		{
			return true;
		}

		if (obj.GetType() != this.GetType())
		{
			return false;
		}

		return Equals((MorestachioExpression)obj);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		unchecked
		{
			var hashCode = (PathParts != null ? PathParts.GetHashCode() : 0);
			hashCode = (hashCode * 397) ^ (Formats != null ? Formats.GetHashCode() : 0);
			hashCode = (hashCode * 397) ^ (FormatterName != null ? FormatterName.GetHashCode() : 0);
			hashCode = (hashCode * 397) ^ (Location.GetHashCode());
			return hashCode;
		}
	}

	private class ExpressionDebuggerDisplay
	{
		private readonly MorestachioExpression _exp;

		public ExpressionDebuggerDisplay(MorestachioExpression exp)
		{
			_exp = exp;
		}

		public string Path
		{
			get { return string.Join(".", _exp.PathParts); }
		}

		public string FormatterName
		{
			get { return _exp.FormatterName; }
		}

		public string Expression
		{
			get
			{
				return _exp.AsStringExpression();
			}
		}

		public string DbgView
		{
			get
			{
				return _exp.AsDebugExpression();
			}
		}

		/// <inheritdoc />
		public override string ToString()
		{
			var visitor = new DebuggerViewExpressionVisitor();
			_exp.Accept(visitor);
			return visitor.StringBuilder.ToString();
		}
	}
}