using System;
using System.IO;

namespace Morestachio.TemplateContainers
{
	public class StringTemplateResource : TemplateResource
	{
		private readonly string _template;

		public StringTemplateResource(string template)
		{
			_template = template;
		}

		public override int IndexOf(string token, int startIndex)
		{
			return _template.IndexOf(token, startIndex, StringComparison.Ordinal);
		}

		public override string Substring(int index, int length)
		{
			return _template.Substring(index, length);
		}

		public override string Substring(int index)
		{
			return _template.Substring(index);
		}

		public override char this[int index]
		{
			get { return _template[index]; }
		}

		public override int Length()
		{
			return _template.Length;
		}

		public override string ToString()
		{
			return _template;
		}
	}
}