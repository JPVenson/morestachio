using System;

namespace Morestachio.Formatter.Framework
{
	public class FormatterArgumentMap
	{
		public FormatterArgumentMap(int codeIndex, int? argumentIndex)
		{
			CodeParameterIndex = codeIndex;
			ParameterIndex = argumentIndex;
		}

		public int CodeParameterIndex { get; }
		public int? ParameterIndex { get; }
		public Func<object, object> ConverterFunc { get; set; }
		public ObtainValue ObtainValue { get; set; }
	}
}