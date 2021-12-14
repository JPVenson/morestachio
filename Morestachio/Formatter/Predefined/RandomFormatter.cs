using System;
using Morestachio.Formatter.Framework;

namespace Morestachio.Formatter.Predefined;
#pragma warning disable CS1591
public static class RandomFormatter
{
	private static Random _random;

	static RandomFormatter()
	{
		_random = new Random();
	}

	[MorestachioGlobalFormatter("Random", "Gets a non-negative random number")]
	public static int Random()
	{
		return _random.Next();
	}

	[MorestachioGlobalFormatter("Random", "Gets a non-negative random number where the number is capped by upperBounds")]
	public static int Random(int upperBounds)
	{
		return _random.Next(upperBounds);
	}

	[MorestachioGlobalFormatter("Random", "Gets a non-negative random number where the number is capped by upperBounds and lowerBounds")]
	public static int Random(int upperBounds, int lowerBounds)
	{
		return _random.Next(lowerBounds, upperBounds);
	}
}