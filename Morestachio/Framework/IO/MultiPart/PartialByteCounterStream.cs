using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Morestachio.Framework
{
	public class PartialByteCounterStream : IByteCounterStream
	{
		public PartialByteCounterStream(ParserOptions options)
		{
			Options = options;
			Partials = new List<IByteCounterStreamPart>();
			Info = new ByteCounterInfo(options.MaxSize);
			RootPart = GetNewPart(options.StreamFactory.Output());
		}
		
		public ParserOptions Options { get; }
		public IList<IByteCounterStreamPart> Partials { get; private set; }

		public IByteCounterStreamPart RootPart { get; set; }
		private ByteCounterInfo Info { get; }
		
		public IByteCounterStreamPart GetNewPart(Stream withStream)
		{
			var partialByteCounterStreamPart = new PartialByteCounterStreamPart(withStream, 2024, Options, this);
			Partials.Add(partialByteCounterStreamPart);
			return partialByteCounterStreamPart;
		}

		public IByteCounterStreamPart GetNewPart()
		{
			return GetNewPart(Options.StreamFactory.TempStream());
		}

		public void Dispose()
		{
			foreach (var byteCounterStream in Partials)
			{
				byteCounterStream.Dispose();
			}
		}

		public void Flush()
		{
			try
			{
				if (!Monitor.TryEnter(this))
				{
					return;
				}

				foreach (var byteCounterStreamPart in Partials.ToArray())
				{
					if (byteCounterStreamPart.State == ByteCounterStreamPartType.Closed)
					{
						using (var baseWriterBaseStream = byteCounterStreamPart.BaseStream())
						{
							baseWriterBaseStream.CopyTo(RootPart.BaseStream());
						}
					}
					Partials.RemoveAt(0);
				}
			}
			finally
			{
				if (Monitor.IsEntered(this))
				{
					Monitor.Exit(this);
				}
			}
		}

		public long BytesWritten
		{
			get
			{
				return Info.BytesWritten;
			}
		}

		public bool ReachedLimit
		{
			get
			{
				return Info.ReachedLimit;
			}
		}

		public void Write(string content)
		{
			RootPart.Write(content);
		}

		public ISubByteCounterStream GetSubStream()
		{
			return new SubByteCounterStream(this, Options);
		}

		public Stream Stream
		{
			get
			{
				return RootPart.BaseStream();
			}
		}
	}
}