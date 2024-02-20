using System.Reflection;
using Morestachio.Document;

namespace Morestachio.Framework.Context.Resolver;

/// <summary>
///		Can resolve fields from objects in addition to properties
/// </summary>
public class FieldValueResolver : IValueResolver
{
	/// <inheritdoc />
	public bool CanResolve(
		Type type,
		object value,
		string path,
		ContextObject context,
		ScopeData scopeData
	)
	{
		return type.GetField(path, BindingFlags.Public | BindingFlags.Instance) != null;
	}

	/// <inheritdoc />
	public object Resolve(
		Type type,
		object value,
		string path,
		ContextObject context,
		ScopeData scopeData
	)
	{
		return type.GetField(path, BindingFlags.Public | BindingFlags.Instance).GetValue(value);
	}

	/// <inheritdoc />
	public bool IsSealed { get; private set; }

	/// <inheritdoc />
	public void Seal()
	{
		IsSealed = true;
	}
}