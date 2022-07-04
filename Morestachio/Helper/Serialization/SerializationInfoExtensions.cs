using System;
using System.Collections.Generic;
using System.Text;

namespace Morestachio.Helper.Serialization
{
	public class MorestachioSerializationContext
	{
		public MorestachioSerializationContext(IFormatterConverter formatterConverter)
		{
			FormatterConverter = formatterConverter;
		}

		public IFormatterConverter FormatterConverter { get; }
	}

	internal static class SerializationInfoExtensions
	{
		public static T GetValueOrDefault<T>(this SerializationInfo serializationInfo, StreamingContext context, string name)
		{
			return serializationInfo.GetValueOrDefault(context, name, () => default(T));
		}

		public static T GetValueOrDefault<T>(this SerializationInfo serializationInfo, StreamingContext context, string name, Func<T> fallback)
		{
			var item = serializationInfo.GetEnumerable().FirstOrDefault(e => e.Name.Equals(name));

			if (item.Name is null)
			{
				return fallback();
			}

			if (item.Value is not T value)
			{
				return (T)(context.Context as MorestachioSerializationContext)?.FormatterConverter.Convert(item.Value, typeof(T));
			}

			return value;
		}

		public static IEnumerable<T> GetValueOrEmpty<T>(this SerializationInfo serializationInfo, StreamingContext context, string name)
		{
			return serializationInfo.GetValueOrDefault<T[]>(context, name, Array.Empty<T>);
		}

		public static IEnumerable<SerializationEntry> GetEnumerable(this SerializationInfo serializationInfo)
		{
			return new SerializationInfoEnumerable(serializationInfo.GetEnumerator());
		}

		private readonly struct SerializationInfoEnumerable : IEnumerable<SerializationEntry>
		{
			private readonly SerializationInfoEnumerator _enumerator;

			public SerializationInfoEnumerable(SerializationInfoEnumerator enumerator)
			{
				_enumerator = enumerator;
			}

			private readonly struct InnerSerializationInfoEnumerator : IEnumerator<SerializationEntry>
			{
				private readonly SerializationInfoEnumerator _enumerator;

				public InnerSerializationInfoEnumerator(SerializationInfoEnumerator enumerator)
				{
					_enumerator = enumerator;
				}

				/// <inheritdoc />
				public bool MoveNext()
				{
					return _enumerator.MoveNext();
				}

				/// <inheritdoc />
				public void Reset()
				{
					_enumerator.Reset();
				}

				/// <inheritdoc />
				object IEnumerator.Current
				{
					get { return Current; }
				}

				/// <inheritdoc />
				public SerializationEntry Current
				{
					get { return _enumerator.Current; }
				}

				/// <inheritdoc />
				public void Dispose()
				{
					
				}
			}

			/// <inheritdoc />
			public IEnumerator<SerializationEntry> GetEnumerator()
			{
				return new InnerSerializationInfoEnumerator(_enumerator);
			}

			/// <inheritdoc />
			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}
	}
}
