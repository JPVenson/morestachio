using System.Reflection;

namespace Morestachio.Formatter.Framework;

public interface IPrepareFormatterComposingResult
{

	/// <summary>
	///     The list of arguments for the <see cref="MethodInfo" />
	/// </summary>
	IDictionary<MultiFormatterInfo, FormatterArgumentMap> Arguments { get; }

	/// <summary>
	///		Gets an compiled Method info
	/// </summary>
	/// <param name="arguments"></param>
	/// <returns></returns>
	(Func<object, object[], object> method, MethodInfo methodInfo) PrepareInvoke(object[] arguments);
}