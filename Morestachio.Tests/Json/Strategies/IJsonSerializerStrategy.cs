using System;
using System.Collections.Generic;
using System.Text;

namespace Morestachio.Tests.Json.Strategies
{
	public interface IJsonSerializerStrategy
	{
		IParserOptionsBuilder Register(IParserOptionsBuilder options);
		object DeSerialize(string text);
	}
}
