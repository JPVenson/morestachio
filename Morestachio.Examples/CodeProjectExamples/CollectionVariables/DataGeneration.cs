using System.Linq;
using Morestachio.Example.Base;

namespace Morestachio.Examples.CodeProjectExamples.CollectionVariables
{
	public class DataGeneration : MorestachioExampleBase
	{
		//there must be always a method in the Program class that will be called to obtain the data
		public override object GetData()
		{
			var list = Enumerable.Range(0, 10).Select(f => (char)('a' + f));
			return list;
		}
	}
}