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
		VariableSource = new Dictionary<string, Stack<UsageDataItem>>();
		_scopes = new Stack<UsageDataItem>();
		Root = new UsageDataItem("", UsageDataItemTypes.DataPath, null);
		_scopes.Push(Root);
	}

	/// <summary>
	/// 
	/// </summary>
	public IDictionary<string, Stack<UsageDataItem>> VariableSource { get; set; }

	/// <summary>
	///		Last added path
	/// </summary>
	public UsageDataItem CurrentPath
	{
		get { return _scopes.Count == 0 ? null : _scopes.Peek(); }
	}

	public UsageDataItem Root { get; }

	private Stack<UsageDataItem> _scopes;

	/// <summary>
	///		Pushes a scope into the current path
	/// </summary>
	/// <param name="currentPath"></param>
	/// <returns></returns>
	public UsageData ScopeTo(UsageDataItem currentPath)
	{
		_scopes.Push(currentPath);
		return this;
	}

	/// <summary>
	///		Pushes a scope into the current path
	/// </summary>
	/// <param name="currentPath"></param>
	/// <returns></returns>
	public UsageDataItem AddAndScopeTo(UsageDataItem currentPath)
	{
		currentPath = CurrentPath.AddDependent(currentPath);
		_scopes.Push(currentPath);
		return currentPath;
	}

	/// <summary>
	///		Removes a scope from the current path
	/// </summary>
	/// <param name="currentExpectedPath"></param>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException"></exception>
	public UsageData PopScope(UsageDataItem currentExpectedPath)
	{
		if (_scopes.Pop() != currentExpectedPath)
		{
			throw new InvalidOperationException(
				$"Popped an unexpected scope while evaluating the usage. The document might be malformed or custom document item does not properly implement {nameof(IReportUsage)}");
		}

		return this;
	}

	public UsageDataItem Add(UsageDataItem currentPath)
	{
		if (currentPath is null)
		{
			return null;
		}

		return CurrentPath.AddDependent(currentPath);
	}

	public void PushVariable(string variableName, UsageDataItem value)
	{
		if (!VariableSource.TryGetValue(variableName, out var scope))
		{
			VariableSource[variableName] = scope = new Stack<UsageDataItem>();
		}

		scope.Push(value);
	}

	public void PopVariable(string itemVariableName)
	{
		VariableSource[itemVariableName].Pop();
	}
}