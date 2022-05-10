namespace Morestachio.Document.Contracts;

/// <summary>
///		Contains a list of all evaluated property paths. Experimental
/// </summary>
public class UsageData
{
	/// <summary>
	/// 
	/// </summary>
	public UsageData()
	{
		VariableSource = new Dictionary<string, string>();
		_scopes = new Stack<string>();
	}

	/// <summary>
	/// 
	/// </summary>
	public IDictionary<string, string> VariableSource { get; set; }

	/// <summary>
	///		Last added path
	/// </summary>
	public string CurrentPath
	{
		get { return _scopes.Count == 0 ? null : _scopes.Peek(); }
	}

	private Stack<string> _scopes;

	/// <summary>
	///		Pushes a scope into the current path
	/// </summary>
	/// <param name="currentPath"></param>
	/// <returns></returns>
	public UsageData ScopeTo(string currentPath)
	{
		_scopes.Push(currentPath);
		return this;
	}

	/// <summary>
	///		Removes a scope from the current path
	/// </summary>
	/// <param name="currentExpectedPath"></param>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException"></exception>
	public UsageData PopScope(string currentExpectedPath)
	{
		if (_scopes.Pop() != currentExpectedPath)
		{
			throw new InvalidOperationException($"Popped an unexpected scope while evaluating the usage. The document might be malformed or custom document item does not properly implement {nameof(IReportUsage)}");
		}
		return this;
	}
}