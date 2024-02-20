namespace Morestachio.Formatter.Framework;

/// <summary>
///		Aggregates two dictionaries
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TItem"></typeparam>
public class AggregateDictionary<TKey, TItem> : AggregateCollection<KeyValuePair<TKey, TItem>>, IDictionary<TKey, TItem>
{
	/// <inheritdoc />
	public AggregateDictionary(IDictionary<TKey, TItem> self, IDictionary<TKey, TItem> parent)
		: base(self, parent)
	{
		_self = self;
		_parent = parent;
	}

	private readonly IDictionary<TKey, TItem> _self;
	private readonly IDictionary<TKey, TItem> _parent;

	/// <inheritdoc />
	public void Add(TKey key, TItem value)
	{
		_self.Add(key, value);
	}

	/// <inheritdoc />
	public bool ContainsKey(TKey key)
	{
		return _self.ContainsKey(key) || _parent.ContainsKey(key);
	}

	/// <inheritdoc />
	public bool Remove(TKey key)
	{
		return _self.Remove(key);
	}

	/// <inheritdoc />
	public bool TryGetValue(TKey key, out TItem value)
	{
		return _self.TryGetValue(key, out value) || _parent.TryGetValue(key, out value);
	}

	/// <inheritdoc />
	public TItem this[TKey key]
	{
		get
		{
			if (ContainsKey(key))
			{
				return _self[key];
			}

			return _parent[key];
		}
		set { _self[key] = value; }
	}

	/// <inheritdoc />
	public ICollection<TKey> Keys
	{
		get { return new AggregateCollection<TKey>(_self.Keys, _parent.Keys); }
	}

	/// <inheritdoc />
	public ICollection<TItem> Values
	{
		get { return new AggregateCollection<TItem>(_self.Values, _parent.Values); }
	}
}