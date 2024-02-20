using System.Collections;
using System.Collections.Generic;
using System.Text;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Framework.Expression.Framework;

/// <inheritdoc />
public class MorestachioErrorCollection : ICollection<IMorestachioError>
{
	private ICollection<IMorestachioError> _base;

	/// <summary>
	/// 
	/// </summary>
	public MorestachioErrorCollection()
	{
		_base = new List<IMorestachioError>();
	}

	/// <summary>
	///		Concatenates all errors into one single text
	/// </summary>
	/// <returns></returns>
	public string GetErrorText()
	{
		var sb = new StringBuilder();

		foreach (var err in this)
		{
			err.Format(sb);
			sb.AppendLine();
		}

		return sb.ToString();
	}

	/// <inheritdoc />
	public IEnumerator<IMorestachioError> GetEnumerator()
	{
		return _base.GetEnumerator();
	}

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)_base).GetEnumerator();
	}

	/// <inheritdoc />
	public void Add(IMorestachioError item)
	{
		//#if DEBUG
		//throw item.GetException();
		//#endif
		_base.Add(item);
	}

	/// <inheritdoc />
	public void Clear()
	{
		_base.Clear();
	}

	/// <inheritdoc />
	public bool Contains(IMorestachioError item)
	{
		return _base.Contains(item);
	}

	/// <inheritdoc />
	public void CopyTo(IMorestachioError[] array, int arrayIndex)
	{
		_base.CopyTo(array, arrayIndex);
	}

	/// <inheritdoc />
	public bool Remove(IMorestachioError item)
	{
		return _base.Remove(item);
	}

	/// <inheritdoc />
	public int Count
	{
		get { return _base.Count; }
	}

	/// <inheritdoc />
	public bool IsReadOnly
	{
		get { return _base.IsReadOnly; }
	}
}