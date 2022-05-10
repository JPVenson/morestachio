using System;
using System.Collections.Generic;

namespace Morestachio.Configuration.Transform
{
	/// <summary>
	///		Options for Morestachio config generation
	/// </summary>
	public class MorestachioConfigOptions
	{
		public MorestachioConfigOptions()
		{
			ParserOptions = ParserOptionsBuilder.New;
			TransformCondition = pair => pair.Value?.StartsWith("mex{{") == true && pair.Value.EndsWith("}}");
			PreTransform = pair => new KeyValuePair<string, string>(pair.Key,
				pair.Value
					.Remove(pair.Value.Length - "}}".Length)
					.Remove(0, "mex{{".Length));
			PostTransform = pair => pair;
			Values = new Dictionary<string, IDictionary<string, object>>();
		}

		public Func<IParserOptionsBuilder> ParserOptions { get; set; }
		public Func<KeyValuePair<string, string>, bool> TransformCondition { get; set; }
		public Func<KeyValuePair<string, string>, KeyValuePair<string, string>> PreTransform { get; set; }
		public Func<KeyValuePair<string, string>, KeyValuePair<string, string>> PostTransform { get; set; }
		public IDictionary<string, IDictionary<string, object>> Values { get; set; }
	}
}