using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Morestachio.Attributes;
using Morestachio.Formatter.Framework;

namespace Morestachio.Formatter.Linq.Core
{
	public static class EqualityFormatter
	{
		[MorestachioFormatter("equals", "Checks if two objects are equals")]
		public static bool IsEquals([SourceObject]object source, object target)
		{
			return Equals(source, target);
		}
	}
}
