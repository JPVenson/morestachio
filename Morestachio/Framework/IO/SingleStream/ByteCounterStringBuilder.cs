using System;
using System.Text;
using Morestachio.Framework.IO.SubStream;

namespace Morestachio.Framework.IO.SingleStream
{
	/// <summary>
	///		Uses a <see cref="System.Text.StringBuilder"/> to write the template
	/// </summary>
	public class ByteCounterStringBuilder : IByteCounterStream
	{
		private readonly StringBuilder _sb;

		/// <summary>
		///		The <see cref="System.Text.StringBuilder"/> used to write the template to
		/// </summary>
		public StringBuilder StringBuilder
		{
			get { return _sb; }
		}

		/// <summary>
		///		The Parser options used to the Template
		/// </summary>
		protected ParserOptions Options { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sb"></param>
		/// <param name="options"></param>
		public ByteCounterStringBuilder(StringBuilder sb, ParserOptions options)
		{
			_sb = sb;
			Options = options;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sb"></param>
		/// <param name="options"></param>
		public ByteCounterStringBuilder(ParserOptions options)
		{
			_sb = new StringBuilder();
			Options = options;
		}

		/// <inheritdoc />
		public void Dispose()
		{

		}

		/// <inheritdoc />
		public override string ToString()
		{
			return _sb.ToString();
		}

		/// <inheritdoc />
		public long BytesWritten { get; private set; }
		/// <inheritdoc />
		public bool ReachedLimit { get; private set; }

#if Span
		/// <inheritdoc />
		public void Write(ReadOnlySpan<char> content)
		{

			if (Options.MaxSize == 0)
			{
				_sb.Append(content);
				return;
			}

			var sourceCount = BytesWritten;
			if (sourceCount >= Options.MaxSize - 1)
			{
				ReachedLimit = true;
				return;
			}

			//TODO this is a performance critical operation. As we might deal with variable-length encodings this cannot be compute initial
			var cl = Options.Encoding.GetByteCount(content);

			var overflow = sourceCount + cl - Options.MaxSize;
			if (overflow <= 0)
			{
				BytesWritten += cl;
				_sb.Append(content);
				return;
			}

			if (overflow < content.Length)
			{
				BytesWritten += cl - overflow;
				//BaseWriter.Write(content.ToCharArray(0, (int)(cl - overflow)));
				_sb.Append(content[0..((int)(cl - overflow))]);
			}
			else
			{
				BytesWritten += cl;
				_sb.Append(content);
			}
		}
#endif
		/// <inheritdoc />
		public void Write(string content)
		{
			content = content ?? Options.Null?.ToString();

			var sourceCount = BytesWritten;

			if (Options.MaxSize == 0)
			{
				_sb.Append(content);
				return;
			}

			if (sourceCount >= Options.MaxSize - 1)
			{
				ReachedLimit = true;
				return;
			}

			//TODO this is a performance critical operation. As we might deal with variable-length encodings this cannot be compute initial
			var cl = Options.Encoding.GetByteCount(content);

			var overflow = sourceCount + cl - Options.MaxSize;
			if (overflow <= 0)
			{
				BytesWritten += cl;
				_sb.Append(content);
				return;
			}

			if (overflow < content.Length)
			{
				BytesWritten += cl - overflow;
				_sb.Append(content.ToCharArray(0, (int)(cl - overflow)));
			}
			else
			{
				BytesWritten += cl;
				_sb.Append(content);
			}
		}

		/// <inheritdoc />
		public ISubByteCounterStream GetSubStream()
		{
			return new SubByteCounterStream(this, Options);
		}
	}
}