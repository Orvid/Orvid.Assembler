using System;
using System.IO;

namespace Orvid.Assembler
{
	public enum AssemblerEndianness
	{
		BigEndian,
		LittleEndian,
	}

	public class AssemblerStream : Stream
	{
		protected Stream ParentStream;
		protected AssemblerEndianness Endianness;

		public AssemblerStream(Stream parentStream, AssemblerEndianness endianness)
		{
			this.ParentStream = parentStream;
			this.Endianness = endianness;
		}
		
		/// <summary>
		/// Writes an 8-bit immutable value to the stream.
		/// </summary>
		/// <param name='imm'>The immutable value to write.</param>
		public void WriteImm8(byte imm)
		{
			WriteByte(imm);
		}


		// General Note:
		//    We cannot use BitConverter here because it uses the
		//    endianness of the machine doing the converting.


		/// <summary>
		/// Writes a 16-bit immutable value to the stream.
		/// </summary>
		/// <param name='imm'>The immutable value to write.</param>
		public void WriteImm16(ushort imm)
		{
			switch (Endianness)
			{
				case AssemblerEndianness.LittleEndian:
					WriteByte((byte)(imm & 0xFF));
					WriteByte((byte)((imm >> 8) & 0xFF));
					break;
				case AssemblerEndianness.BigEndian:
					WriteByte((byte)((imm >> 8) & 0xFF));
					WriteByte((byte)(imm & 0xFF));
					break;
				default:
					throw new Exception("Unknown endianness!");
			}
		}
		
		/// <summary>
		/// Writes a 32-bit immutable value to the stream.
		/// </summary>
		/// <param name='imm'>The immutable value to write.</param>
		public void WriteImm32(uint imm)
		{
			switch (Endianness)
			{
				case AssemblerEndianness.LittleEndian:
					WriteByte((byte)(imm & 0xFF));
					WriteByte((byte)((imm >> 8) & 0xFF));
					WriteByte((byte)((imm >> 16) & 0xFF));
					WriteByte((byte)((imm >> 24) & 0xFF));
					break;
				case AssemblerEndianness.BigEndian:
					WriteByte((byte)((imm >> 24) & 0xFF));
					WriteByte((byte)((imm >> 16) & 0xFF));
					WriteByte((byte)((imm >> 8) & 0xFF));
					WriteByte((byte)(imm & 0xFF));
					break;
				default:
					throw new Exception("Unknown endianness!");
			}
		}
		
		/// <summary>
		/// Slightly non-traditional way of checking this,
		/// but it is fast. (no branching if cmov is supported)
		/// </summary>
		public static bool InSByteRange(int val)
		{
			return (val == (int)(sbyte)(byte)val);
		}
		
		/// <summary>
		/// Slightly non-traditional way of checking this,
		/// but it is fast. (no branching if cmov is supported)
		/// </summary>
		public static bool InShortRange(int val)
		{
			return (val == (int)(short)(ushort)val);
		}
		
		public override void Flush()
		{
			ParentStream.Flush();
		}
		public override int Read(byte[] buffer, int offset, int count)
		{
			return ParentStream.Read(buffer, offset, count);
		}
		public override long Seek(long offset, SeekOrigin origin)
		{
			return ParentStream.Seek(offset, origin);
		}
		public override void SetLength(long value)
		{
			ParentStream.SetLength(value);
		}
		public override void Write(byte[] buffer, int offset, int count)
		{
			ParentStream.Write(buffer, offset, count);
		}
		public override bool CanRead
		{
			get { return ParentStream.CanRead; }
		}
		public override bool CanSeek
		{
			get { return ParentStream.CanSeek; }
		}
		public override bool CanWrite
		{
			get { return ParentStream.CanWrite; }
		}
		public override long Length
		{
			get { return ParentStream.Length; }
		}
		public override long Position
		{
			get { return ParentStream.Position; }
			set { ParentStream.Position = value; }
		}
	}
}

