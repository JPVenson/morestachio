//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.CompilerServices;
//using Morestachio.Framework.Expression.Framework;

//namespace Morestachio.Framework
//{
//	internal static class TagTokenizer
//	{
//		public static TagDefinition Declare { get; } =
//			TagDefinition.ParseDefinition("#DECLARE (string:name=@add(PartialDeclarationOpen)) @push(declare)");

//		public static void Tokenize(string text, TagDefinition definition)
//		{
//		}
//	}

//	internal delegate IEnumerable<TokenPair> Generator(
//		Stack<Tuple<string, int>> scopeStack,
//		string token,
//		TokenzierContext context);

//	internal readonly struct TagDefinition
//	{
//		private TagDefinition(string definition,
//			string tag,
//			TagParameterGroupDefinition[] parameters,
//			Tuple<ScopeActionTypes, string>[] scopeActionType,
//			TokenType[] tokenType)
//			: this()
//		{
//			Definition = definition;
//			Tag = tag;
//			Parameters = parameters;
//			ScopeActionType = scopeActionType;
//			TokenType = tokenType;

//			Generator = GetGenerator();
//		}

//		private Generator GetGenerator()
//		{
//			var that = this;
//			return (
//				Stack<Tuple<string, int>> scopeStack,
//				string token,
//				TokenzierContext context) =>
//			{
//				if (that.Parameters.Length == 0)
//				{
//					if (!token.Equals("{{" + that.Tag + "}}"))
//					{
//						return Enumerable.Empty<TokenPair>();
//					}
//				}

//				var trimmedToken = token.Trim('{', '}').Trim().Substring(that.Tag.TrimStart('#').Length);
//				var index = 0;
//				var tokens = new List<TokenPair>();
//				foreach (var tagParameterDefinition in that.Parameters)
//				{
//					foreach (var parameterDefinition in tagParameterDefinition.Parameters)
//					{
//						bool isOptional = parameterDefinition.Type.HasFlagFast(TagParameterDefinitionTypes.Optional);

//						if (parameterDefinition.Type == TagParameterDefinitionTypes.String)
//						{

//						}
//					}

//					switch (tagParameterDefinition.Type)
//					{
//						case TagParameterDefinitionTypes.None:
//						case TagParameterDefinitionTypes.Flag:
//							break;
//						case TagParameterDefinitionTypes.Expression:
//							{

//							}
//							break;
//						case TagParameterDefinitionTypes.String:
//							var value = "";
//							for (int i = index; i < trimmedToken.Length; i++)
//							{
//								var c = trimmedToken[i];
//								if (char.IsWhiteSpace(c))
//								{
//									break;
//								}

//								value += c;
//							}
//							tokens.Add(new TokenPair(that.TokenType));
//							break;
//						case TagParameterDefinitionTypes.Optional:
//							break;
//						case TagParameterDefinitionTypes.Alias:
//							break;
//						default:
//							throw new ArgumentOutOfRangeException();
//					}
//				}
//			};
//		}

//		internal static TagDefinition ParseDefinition(string definition)
//		{
//			var parts = definition.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
//			var tag = parts[0];
//			var scopeActions = new List<Tuple<ScopeActionTypes, string>>();
//			var parameter = new List<TagParameterGroupDefinition>();
//			var tokenTypes = new List<TokenType>();
//			foreach (var part in parts.Skip(1))
//			{
//				if (part.StartsWith("@"))
//				{
//					if (part == "@push")
//					{
//						scopeActions.Add(Tuple.Create(ScopeActionTypes.Push,
//							part.Remove(0, "@push".Length).Trim('(', ')')));
//					}
//					else if (part == "@pop")
//					{
//						scopeActions.Add(Tuple.Create(ScopeActionTypes.Pop,
//							part.Remove(0, "@pop".Length).Trim('(', ')')));
//					}
//				}
//				else if (part.StartsWith("("))
//				{
//					if (!part.EndsWith(")"))
//					{
//						throw new InvalidOperationException(
//							$"The definitions '{definition}' argument '{part}' is invalid as it does not end with an )");
//					}

//					var conditionAndResult = part.Remove(part.Length - 1, 1).Remove(0, 1).Split('=');
//					var textCondition = conditionAndResult[0];
//					var textAction = conditionAndResult[1];

//					var conditions = new List<TagParameterDefinition>();
//					var actions = new List<Tuple<ScopeActionTypes, string>>();
//					foreach (var condition in textCondition.Split('&'))
//					{
//						var argumentParts = condition.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
//						if (argumentParts.Length != 2)
//						{
//							throw new InvalidOperationException(
//								$"The definitions '{definition}' argument '{part}' is invalid as it contains more then one :");
//						}

//						var argType = argumentParts[0];
//						var parsedArgType =
//							(TagParameterDefinitionTypes)Enum.Parse(typeof(TagParameterDefinitionTypes),
//								condition.TrimEnd('?'));
//						if (argType.EndsWith("?"))
//						{
//							parsedArgType |= TagParameterDefinitionTypes.Optional;
//						}
//						var argName = argumentParts[1];
//						conditions.Add(new TagParameterDefinition(argName, parsedArgType));
//					}

//					foreach (var action in textAction.Split('&'))
//					{
//						if (action.StartsWith("@add("))
//						{
//							actions.Add(Tuple.Create(ScopeActionTypes.Add,
//								part.Remove(0, "@add".Length).Trim('(', ')')));
//						}
//					}

//					parameter.Add(new TagParameterGroupDefinition(conditions.ToArray(), actions.ToArray()));
//				}
//			}

//			return new TagDefinition(definition,
//				tag,
//				parameter.ToArray(),
//				scopeActions.ToArray(),
//				tokenTypes.ToArray());
//		}

//		public string Definition { get; }
//		public Tuple<ScopeActionTypes, string>[] ScopeActionType { get; }
//		public string Tag { get; }
//		public TagParameterGroupDefinition[] Parameters { get; }
//		public TokenType[] TokenType { get; }

//		public Generator Generator { get; }
//	}

//	internal readonly struct TagParameterGroupDefinition
//	{
//		public TagParameterGroupDefinition(TagParameterDefinition[] parameters, Tuple<ScopeActionTypes, string>[] tokenType)
//		{
//			Parameters = parameters;
//			TokenType = tokenType;
//		}

//		public TagParameterDefinition[] Parameters { get; }
//		public Tuple<ScopeActionTypes, string>[] TokenType { get; }
//	}


//	internal readonly struct TagParameterDefinition
//	{
//		public TagParameterDefinition(string name, TagParameterDefinitionTypes type)
//		{
//			Name = name;
//			Type = type;
//		}

//		public string Name { get; }
//		public TagParameterDefinitionTypes Type { get; }
//	}

//	internal enum ScopeActionTypes
//	{
//		None,
//		Push,
//		Pop,
//		Add
//	}

//	[Flags]
//	internal enum TagParameterDefinitionTypes
//	{
//		None = 1 << 0,
//		Flag = 1 << 1,
//		Expression = 1 << 2,
//		String = 1 << 3,
//		Optional = 1 << 4,
//		Alias = 1 << 5
//	}

//	internal static class TagParameterDefinitionTypesExtensions
//	{
//		public static bool HasFlagFast(this TagParameterDefinitionTypes value, TagParameterDefinitionTypes flag)
//		{
//			return (value & flag) != 0;
//		}
//	}
//}