namespace Morestachio.Framework
{
	/// <summary>
	///     Can be implemented by an object to provide custom resolving logic
	/// </summary>
	public interface IMorestachioPropertyResolver
	{
		/// <summary>
		///     Gets the value of the property from this object
		/// </summary>
		bool TryGetValue(string name, out object found);
	}
}