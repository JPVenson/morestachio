using System.Collections;
using System.Collections.Generic;

namespace Morestachio.Framework.Expression;

internal class OperatorList : IReadOnlyDictionary<OperatorTypes, MorestachioOperator>
{
	public OperatorList()
	{
		Operators = new Dictionary<OperatorTypes, MorestachioOperator>();
	}

	private IDictionary<OperatorTypes, MorestachioOperator> Operators { get; }

	public void Add(MorestachioOperator mOperator)
	{
		Operators[mOperator.OperatorType] = mOperator;
	}

	public IEnumerator<KeyValuePair<OperatorTypes, MorestachioOperator>> GetEnumerator()
	{
		return Operators.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)Operators).GetEnumerator();
	}

	public int Count
	{
		get
		{
			return Operators.Count;
		}
	}

	public bool ContainsKey(OperatorTypes key)
	{
		return Operators.ContainsKey(key);
	}

	public bool TryGetValue(OperatorTypes key, out MorestachioOperator value)
	{
		return Operators.TryGetValue(key, out value);
	}

	public MorestachioOperator this[OperatorTypes key]
	{
		get { return Operators[key]; }
	}

	public IEnumerable<OperatorTypes> Keys
	{
		get
		{
			return Operators.Keys;
		}
	}

	public IEnumerable<MorestachioOperator> Values
	{
		get
		{
			return Operators.Values;
		}
	}
}