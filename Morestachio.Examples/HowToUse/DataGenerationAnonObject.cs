using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.IO;

using Morestachio;

namespace Morestachio.Examples.HowToUse
{
	public static class DataGenerationAnonObject
	{
		//there must be always a method in the Program class that will be called to obtain the data
		public static object GetData()
		{
			var model = new
			{
				name = "John", 
				sender = "Sally"
			};
			return model;
		}
	}
}
