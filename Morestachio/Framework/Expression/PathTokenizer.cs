using System;
using System.Collections.Generic;
using System.Linq;
using Morestachio.ParserErrors;

namespace Morestachio.Framework.Expression
{
	public readonly struct PathPart
	{
		public PathPart(PathType pathType, string value)
		{
			PathType = pathType;
			Value = value;
		}

		public PathType PathType { get; }
		public string Value { get; }
	}

	public enum PathType
	{
		DataPath,
		RootSelector,
		ParentSelector,
		SelfAssignment,
		ObjectSelector,
		Number
	}

#pragma warning disable 1591

	/// <summary>
	///		This path is internal and should not be used in your code
	/// </summary>
	internal class PathTokenizer
	{
		public PathTokenizer()
		{
			PathParts = new List<KeyValuePair<string, PathType>>();
			CurrentPart = "";
		}

		IList<KeyValuePair<string, PathType>> PathParts { get; set; }

		public string CurrentPart { get; set; }
		public bool LastCharWasDelimiter { get; set; }
		public bool CurrentPartIsNumber { get; set; }

		public bool Add(char c)
		{
			if (!Tokenizer.IsExpressionChar(c))
			{
				return false;
			}

			LastCharWasDelimiter = c == '.';

			if (c == '/')
			{
				if (CurrentPart == "..")
				{
					if (PathParts.Any() && PathParts.Any(e => e.Value != PathType.ParentSelector && e.Value != PathType.RootSelector))
					{
						return false;
					}
					PathParts.Add(new KeyValuePair<string, PathType>(null, PathType.ParentSelector));
					CurrentPart = "";
					return true;
				}
				//add error
				return false;
			}

			if (c == '~')
			{
				if (CurrentPart != string.Empty || PathParts.Any())
				{
					return false;
				}

				PathParts.Add(new KeyValuePair<string, PathType>(null, PathType.RootSelector));
				CurrentPart = "";
				return true;
			}

			if (c == '?')
			{
				PathParts.Add(new KeyValuePair<string, PathType>(null, PathType.ObjectSelector));
				CurrentPart = "";
				return true;
			}

			if (c != '.' && CurrentPart == "." && PathParts.Count == 0)
			{
				//in this case somebody wrote .data
				//so ignore the dot
				CurrentPart = "";
			}

			if (CurrentPart != "" && CurrentPart != "." && c == '.')
			{
				if (CurrentPartIsNumber)
				{
					if (CurrentPart.Contains("."))
					{
						PathParts.Add(new KeyValuePair<string, PathType>(CurrentPart, PathType.Number));
						CurrentPart = "";
					}
					else
					{
						CurrentPart += c;
					}

					return true;
				}
				
				if (CheckPathPart() != -1)
				{
					return false;
				}
				PathParts.Add(new KeyValuePair<string, PathType>(CurrentPart, PathType.DataPath));
				CurrentPart = "";
			}
			else
			{
				if (CurrentPart == string.Empty && char.IsDigit(c))
				{
					CurrentPartIsNumber = true;
					if (PathParts.Any())
					{
						return false;
					}
				}

				CurrentPart += c;
			}

			return true;
		}

		private int CheckPathPart()
		{
			var part = CurrentPart;
			if (part.StartsWith("$") && part != "$")
			{
				part = part.Substring(1, part.Length - 1);
			}

			var match = Tokenizer.NegativeCharRegex.Match(part);
			if (match.Success)
			{
				return match.Index;
			}

			return -1;
		}

		public string GetFormatterName()
		{
			string lastPartIsFormatterName;//PathParts.LastOrDefault().Key;
			if (LastCharWasDelimiter)
			{
				lastPartIsFormatterName = CurrentPart;
			}
			else
			{
				lastPartIsFormatterName = PathParts.LastOrDefault().Key;
				PathParts.RemoveAt(PathParts.Count - 1);
			}

			CurrentPart = "";
			return lastPartIsFormatterName;
		}

		public IList<KeyValuePair<string, PathType>> Compile(out int hasError)
		{
			if (CurrentPart == ".")
			{
				PathParts.Add(new KeyValuePair<string, PathType>(null, PathType.SelfAssignment));
			}
			else if (CurrentPart == "../")
			{
				PathParts.Add(new KeyValuePair<string, PathType>(null, PathType.ParentSelector));
			}
			else if (CurrentPart == "~")
			{
				PathParts.Add(new KeyValuePair<string, PathType>(null, PathType.RootSelector));
			}
			else if (CurrentPart == "?")
			{
				PathParts.Add(new KeyValuePair<string, PathType>(null, PathType.ObjectSelector));
			}
			else if (CurrentPart.Trim() != "")
			{
				if (CurrentPartIsNumber)
				{
					PathParts.Add(new KeyValuePair<string, PathType>(CurrentPart, PathType.Number));
				}
				else
				{
					hasError = CheckPathPart();
					if (hasError != -1)
					{
						return new List<KeyValuePair<string, PathType>>();
					}
					PathParts.Add(new KeyValuePair<string, PathType>(CurrentPart, PathType.DataPath));	
				}
			}

			if (PathParts.Count > 1 && PathParts.Last().Value == PathType.SelfAssignment)
			{
				PathParts.Remove(PathParts.Last());
			}

			//if (!PathParts.Any())
			//{
			//	hasError = 0;
			//	return PathParts;
			//}

			CurrentPart = "";
			
			hasError = -1;
			return PathParts;
		}
	}
}