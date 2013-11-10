using System;
using System.IO;
namespace InOutTools
{
	public class HandleQuotesStream : Stream
	{
		protected Stream source;
		protected string quotes;
		protected char[] search;
		protected char replace;
		
		protected bool inquotes;
		
		public HandleQuotesStream(string filename, string quotes, string search, char replace)
		{
			source = new FileStream(filename, FileMode.Open);
			this.quotes = quotes;
			this.search = search.ToCharArray();
			this.replace = replace;
		}
				
		public override int ReadByte()
		{
			while (true) {
				int bb = source.ReadByte();
				if (bb < 0)
					return bb;
				
				// drop escaped characters (e.g., other quotes)
				if (inquotes && bb == '\\') {
					source.ReadByte();
					return replace;
				}
				
				char cc = (char) bb;
				if (quotes.Contains(cc.ToString()))
					inquotes = !inquotes;
				else if (inquotes && Array.IndexOf<char>(search, cc) != -1)
					return (int) replace;
				else
					return bb;
			}
		}
		
		public override bool CanRead {
			get {
				return true;
			}
		}
		
		public override bool CanSeek {
			get {
				return false;
			}
		}
		
		public override bool CanWrite {
			get {
				return false;
			}
		}
		
		public override bool CanTimeout {
			get {
				return source.CanTimeout;
			}
		}
		
		public override long Length {
			get {
				return source.Length;
			}
		}
		
		public override long Position {
			get {
				return source.Position;
			}
			set {
				source.Position = value;
			}
		}
		
		public override void Flush()
		{
			source.Flush();
		}
		
		public override long Seek(long offset, SeekOrigin origin)
		{
			return source.Seek(offset, origin);
		}
		
		public override void SetLength(long value)
		{
			source.SetLength(value);
		}
		
		public override void Write(byte[] buffer, int offset, int count)
		{
			source.Write(buffer, offset, count);
		}
		
		public override int Read(byte[] buffer, int offset, int count)
		{
			for (int ii = 0; ii < count; ii++) {
				int bb = ReadByte();
				buffer[ii + offset] = (byte) bb;
				if (bb < 0)
					return ii;
			}
			
			return count;
		}
	}
}

