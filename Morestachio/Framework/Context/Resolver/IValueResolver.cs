using System;

namespace Morestachio.Framework.Context.Resolver
{
	/// <summary>
	///		Can be used to extract a value from an object that is not natively supported such as IDictionary{string, object} or object.
	///		for example: This could be used to resolve Newtonesofts JObject
	/// </summary>
	public interface IValueResolver
	{
		///  <summary>
		///		Should return ether NULL or an object as result.
		///		this CAN return a Task that has a result. The task will be awaited if needed.
		///  </summary>
		///  <param name="type"></param>
		///  <param name="value"></param>
		///  <param name="path"></param>
		///  <param name="context"></param>
		///  <returns></returns>
		object Resolve(Type type, object value, string path, ContextObject context);

		///  <summary>
		/// 		Will be called for each lookup. If returns false the default logic will kick in otherwise morestachio will call <see cref="Resolve"/> to obtain the value from path
		///  </summary>
		///  <param name="type"></param>
		///  <param name="value"></param>
		///  <param name="path"></param>
		///  <param name="context"></param>
		///  <returns></returns>
		bool CanResolve(Type type, object value, string path, ContextObject context);
	}
}