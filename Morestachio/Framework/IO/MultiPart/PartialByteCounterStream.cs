//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading;
//using Morestachio.Document.Contracts;
//using Morestachio.Framework.IO.SubStream;

//namespace Morestachio.Framework.IO.MultiPart
//{
//	/// <summary>
//	///		Defines an byte counter stream as part of an template that contains multiple parts
//	/// </summary>
//	public class PartialByteCounterStream : IByteCounterStream
//	{
//		/// <summary>
//		/// 
//		/// </summary>
//		public PartialByteCounterStream(ParserOptions options)
//		{
//			Options = options;
//			Partials = new List<IByteCounterStreamPart>();
//			Info = new ByteCounterInfo(options.MaxSize);
//			RootPart = GetNewPart(options.StreamFactory.Output(Options));
//		}

//		/// <summary>
//		///		The Options
//		/// </summary>
//		public ParserOptions Options { get; }

//		/// <summary>
//		///		The partials. Must be in order
//		/// </summary>
//		public IList<IByteCounterStreamPart> Partials { get; private set; }

//		/// <summary>
//		///		The root part all legacy <see cref="IDocumentItem"/> will write to
//		/// </summary>
//		public IByteCounterStreamPart RootPart { get; set; }
//		private ByteCounterInfo Info { get; }

//		/// <summary>
//		///		Gets a new Part
//		/// </summary>
//		public IByteCounterStreamPart GetNewPart(IByteCounterStream withStream)
//		{
//			var partialByteCounterStreamPart = new PartialByteCounterStreamPart(withStream, 2024, Options, this);
//			Partials.Add(partialByteCounterStreamPart);
//			return partialByteCounterStreamPart;
//		}

//		/// <inheritdoc />
//		public void Dispose()
//		{
//			foreach (var byteCounterStream in Partials)
//			{
//				byteCounterStream.Dispose();
//			}
//		}

//		/// <summary>
//		///		Flushes all parts in order
//		/// </summary>
//		public void Flush()
//		{
//			try
//			{
//				if (!Monitor.TryEnter(this))
//				{
//					return;
//				}

//				foreach (var byteCounterStreamPart in Partials.ToArray())
//				{
//					if (byteCounterStreamPart.State == ByteCounterStreamPartType.Closed)
//					{
//						using (var baseWriterBaseStream = byteCounterStreamPart.BaseStream())
//						{
//							baseWriterBaseStream.CopyTo(RootPart.BaseStream());
//						}
//					}
//					Partials.RemoveAt(0);
//				}
//			}
//			finally
//			{
//				if (Monitor.IsEntered(this))
//				{
//					Monitor.Exit(this);
//				}
//			}
//		}

//		/// <inheritdoc />
//		public long BytesWritten
//		{
//			get
//			{
//				return Info.BytesWritten;
//			}
//		}

//		/// <inheritdoc />
//		public bool ReachedLimit
//		{
//			get
//			{
//				return Info.ReachedLimit;
//			}
//		}

//#if Span
//		public void Write(ReadOnlySpan<char> content)
//		{
//			RootPart.Write(content);
//		}
//#endif
///// <inheritdoc />
//		public void Write(string content)
//		{
//			RootPart.Write(content);
//		}

//		/// <inheritdoc />
//		public ISubByteCounterStream GetSubStream()
//		{
//			return new SubByteCounterStream(this, Options);
//		}
//	}
//}

