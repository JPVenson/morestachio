﻿using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Morestachio.Framework.Tokenizing
{
	/// <summary>
	///		Defines an option declared inline with the keyword that is bound to a DocumentItem
	/// </summary>
	[Serializable]
	public readonly struct PersistantTokenOption : ITokenOption, ISerializable
	{
		/// <inheritdoc />
		public PersistantTokenOption(string name, bool value)
		{
			Name = name;
			Value = value;
			Persistent = true;
		}
		/// <inheritdoc />
		public PersistantTokenOption(string name, string value)
		{
			Name = name;
			Value = value;
			Persistent = true;
		}
		
		/// <inheritdoc />
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		private PersistantTokenOption(SerializationInfo info, StreamingContext c)
		{
			Name = info.GetString(nameof(Name));
			var valueType = info.GetValue("ValueType", typeof(Type)) as Type;
			if (valueType == null)
			{
				Value = null;
			}
			else
			{
				Value = info.GetValue(nameof(Value), valueType);	
			}
			Persistent = true;
		}

		/// <inheritdoc />
		public string Name { get; }
		/// <inheritdoc />
		public object Value { get; }
		/// <inheritdoc />
		public bool Persistent { get; }
		
		/// <inheritdoc />
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(nameof(Name), Name);
			info.AddValue("ValueType", Value?.GetType());
			info.AddValue(nameof(Value), Value);
		}

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