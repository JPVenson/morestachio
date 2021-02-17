using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using Morestachio.Document;
using Morestachio.Fluent.Expression;
using Morestachio.Formatter.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Expression.Visitors;
using Morestachio.Framework.Tokenizing;
using Morestachio.Helper;
using Morestachio.Parsing.ParserErrors;
#if ValueTask
using ContextObjectPromise = System.Threading.Tasks.ValueTask<Morestachio.Framework.Context.ContextObject>;
#else
using ContextObjectPromise = System.Threading.Tasks.Task<Morestachio.Framework.Context.ContextObject>;
#endif

namespace Morestachio.Framework.Expression
{
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

		internal MorestachioExpression(CharacterLocation location) : this()
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
		public FormatterCache? Cache { get; private set; }

		/// <summary>
		///		Gets whenever this expression was explicitly closed
		/// </summary>
		public bool EndsWithDelimiter { get; private set; }

		/// <inheritdoc />
		public CompiledExpression Compile()
		{
			if (!PathParts.HasValue && Formats.Count == 0 && FormatterName == null)
			{
				return (contextObject, data) => contextObject.ToPromise();
			}
			if (!PathParts.HasValue && Formats.Count == 1 && FormatterName == "")
			{
				//this enables the usage of brackets. A formatter that has no name and one argument e.g ".(data + 1)" or "(data + 1)" should be considered a bracket
				return (contextObject, data) => Formats[0].GetValue(contextObject, data);
			}

			if (PathParts.Count == 1 && PathParts.Current.Value == PathType.Null)
			{
				return (contextObject, data) => contextObject.Options
					.CreateContextObject("x:null", contextObject.CancellationToken, null).ToPromise();
			}

			var pathQueue = new List<Func<ContextObject, ScopeData, IMorestachioExpression, ContextObjectPromise>>();
			var pathParts = PathParts.ToArray();

			if (pathParts.Length > 0 && pathParts.First().Value == PathType.DataPath)
			{
				var firstItem = pathParts.First();

				pathQueue.Add((context, scopeData, expression) =>
				{
					var variable = scopeData.GetVariable(context, firstItem.Key);
					if (variable != null)
					{
						return variable.ToPromise();
					}

					return context.ExecuteDataPath(firstItem.Key, expression, context.Value?.GetType()).ToPromise();
				});
				pathParts = pathParts.Skip(1).ToArray();
			}

			foreach (var pathPart in pathParts)
			{
				switch (pathPart.Value)
				{
					case PathType.DataPath:
						pathQueue.Add((contextObject, scopeData, expression) =>
						{
							return contextObject.ExecuteDataPath(pathPart.Key, expression, contextObject.Value?.GetType()).ToPromise();
						});
						break;
					case PathType.RootSelector:
						pathQueue.Add((contextObject, scopeData, expression) =>
						{
							return contextObject.ExecuteRootSelector().ToPromise();
						});
						break;
					case PathType.ParentSelector:
						pathQueue.Add((contextObject, scopeData, expression) =>
						{
							var natContext = contextObject.FindNextNaturalContextObject();
							return (natContext?.Parent ?? contextObject).ToPromise();
						});
						break;
					case PathType.ObjectSelector:
						pathQueue.Add((contextObject, scopeData, expression) =>
						{
							return contextObject.ExecuteObjectSelector(pathPart.Key, contextObject.Value?.GetType())
								.ToPromise();
						});
						break;
					case PathType.Null:
						pathQueue.Add((contextObject, scopeData, expression) =>
						{
							return contextObject.Options.CreateContextObject("x:null", contextObject.CancellationToken, null)
								.ToPromise();
						});
						break;
					case PathType.Boolean:
						pathQueue.Add((contextObject, scopeData, expression) =>
						{
							var booleanContext =
								contextObject.Options.CreateContextObject(".", contextObject.CancellationToken,
									pathPart.Key == "true", contextObject);
							booleanContext.IsNaturalContext = contextObject.IsNaturalContext;
							return booleanContext.ToPromise();
						});
						break;
					case PathType.SelfAssignment:
						pathQueue.Add((contextObject, scopeDate, expression) => contextObject.ToPromise());
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			Func<ContextObject, ScopeData, IMorestachioExpression, ContextObjectPromise> getContext;

			if (pathQueue.Count != 0)
			{
				getContext =
					async (contextObject, data, expression) =>
					{
						foreach (var func in pathQueue)
						{
							contextObject = await func(contextObject, data, expression);
						}

						return contextObject;
					};
			}
			else
			{
				getContext = (context, scopeData, expression) => context.ToPromise();
			}


			if (!Formats.Any() && FormatterName == null)
			{
				return (contextObject, data) => getContext(contextObject, data, this);
			}

			var formatsCompiled = Formats.ToDictionary(f => f, f => f.Compile()).ToArray();

			FormatterCache? cache = null;
			return async (contextObject, scopeData) =>
			{
				var ctx = await getContext(contextObject, scopeData, this);

				if (ctx == contextObject)
				{
					ctx = contextObject.CloneForEdit();
				}

				var arguments = new FormatterArgumentType[formatsCompiled.Length];
				var naturalValue = contextObject.FindNextNaturalContextObject();
				for (var index = 0; index < formatsCompiled.Length; index++)
				{
					var formatterArgument = formatsCompiled[index];
					var value = await formatterArgument.Value(naturalValue, scopeData);
					arguments[index] = new FormatterArgumentType(index, formatterArgument.Key.Name, value?.Value);
				}

				if (cache == null)
				{
					cache = ctx.PrepareFormatterCall(
						ctx.Value?.GetType() ?? typeof(object),
						FormatterName,
						arguments,
						scopeData);
				}

				if (cache != null && !Equals(cache.Value, default(FormatterCache)))
				{
					ctx.Value = await contextObject.Options.Formatters.Execute(cache.Value, ctx.Value, contextObject.Options, arguments);
					ctx.MakeSyntetic();
				}

				return ctx;
			};
		}

		/// <inheritdoc />
		public async ContextObjectPromise GetValue(ContextObject contextObject, ScopeData scopeData)
		{
			if (!PathParts.Any() && Formats.Count == 1 && FormatterName == "")
			{
				//indicates the usage of brackets
				return await Formats[0].GetValue(contextObject, scopeData);
			}

			var contextForPath = await contextObject.GetContextForPath(PathParts, scopeData, this);
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
				var value = await formatterArgument.MorestachioExpression.GetValue(naturalValue, scopeData);
				arguments[index] = new FormatterArgumentType(index, formatterArgument.Name, value?.Value);
			}
			//contextForPath.Value = await contextForPath.Format(FormatterName, argList, scopeData);

			if (Cache == null)
			{
				Cache = contextForPath.PrepareFormatterCall(
					contextForPath.Value?.GetType() ?? typeof(object),
					FormatterName,
					arguments,
					scopeData);
			}

			if (Cache != null && !Equals(Cache.Value, default(FormatterCache)))
			{
				contextForPath.Value = await contextObject.Options.Formatters.Execute(Cache.Value, contextForPath.Value, contextObject.Options, arguments);
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

		/// <inheritdoc />
		public override string ToString()
		{
			var visitor = new DebuggerViewExpressionVisitor();
			Accept(visitor);
			return visitor.StringBuilder.ToString();
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
				get { return _exp.ToString(); }
			}
		}
	}
}