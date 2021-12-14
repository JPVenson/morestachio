namespace Morestachio.Formatter.Framework;

/// <summary>
///		Delegate for using with <see cref="FormatterArgumentMap"/>
/// </summary>
/// <param name="sourceObject"></param>
/// <param name="arguments"></param>
/// <returns></returns>
public delegate object ObtainValue(object sourceObject, FormatterArgumentType[] arguments);