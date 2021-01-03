namespace Morestachio.Framework.Tokenizing
{
	/// <summary>
	///		Defines an option declared inline with the keyword that is bound to a DocumentItem
	/// </summary>
	public readonly struct PersistantTokenOption : ITokenOption
	{
		public PersistantTokenOption(string name, bool value)
		{
			Name = name;
			Value = value;
			Persistent = true;
		}
		public PersistantTokenOption(string name, string value)
		{
			Name = name;
			Value = value;
			Persistent = true;
		}

		/// <inheritdoc />
		public string Name { get; }
		/// <inheritdoc />
		public object Value { get; }
		/// <inheritdoc />
		public bool Persistent { get; }

		public bool Equals(ITokenOption other)
		{
			return Name == other.Name && Equals(Value, other.Value) && Persistent == other.Persistent;
		}

		public override bool Equals(object obj)
		{
			return obj is PersistantTokenOption other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (Name != null ? Name.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Value != null ? Value.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ Persistent.GetHashCode();
				return hashCode;
			}
		}
	}
}