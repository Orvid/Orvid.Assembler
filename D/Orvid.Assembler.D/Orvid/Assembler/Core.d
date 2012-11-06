module Orvid.Assembler.Core;

import std.stream;

public abstract class Assembler
{
	public this() @safe pure nothrow
	{
	}
	
	public abstract void Emit(Stream strm);
}

public abstract class Instruction
{
	public this() @safe pure nothrow
	{
	}
}

public enum AssemblerEndianness
{
	BigEndian,
	LittleEndian,
}

public abstract class AssemblerStream : Stream
{
	protected Stream ParentStream;
	protected AssemblerEndianness Endianness;
	
	public this(Stream parentStream, AssemblerEndianness endianness) nothrow
	{
		assert(parentStream.writeable);
		this.writeable = true;
		this.ParentStream = parentStream;
		this.Endianness = endianness;
		super();
	}
	
	public void WriteByte(ubyte val)
	{
		write(val);
	}
	
	public void WriteImm8(ubyte imm)
	{
		write(imm);
	}
	
	public void WriteImm16(ushort imm)
	{
		switch(Endianness)
		{
			case AssemblerEndianness.LittleEndian:
				write(cast(ubyte)(imm & 0xFF));
				write(cast(ubyte)((imm >> 8) & 0xFF));
				break;
			case AssemblerEndianness.BigEndian:
				write(cast(ubyte)((imm >> 8) & 0xFF));
				write(cast(ubyte)(imm & 0xFF));
				break;
			default:
				assert(0);
		}
	}
	
	public void WriteImm32(uint imm)
	{
		switch(Endianness)
		{
			case AssemblerEndianness.LittleEndian:
				write(cast(ubyte)(imm & 0xFF));
				write(cast(ubyte)((imm >> 8) & 0xFF));
				write(cast(ubyte)((imm >> 16) & 0xFF));
				write(cast(ubyte)((imm >> 24) & 0xFF));
				break;
			case AssemblerEndianness.BigEndian:
				write(cast(ubyte)((imm >> 24) & 0xFF));
				write(cast(ubyte)((imm >> 16) & 0xFF));
				write(cast(ubyte)((imm >> 8) & 0xFF));
				write(cast(ubyte)(imm & 0xFF));
				break;
			default:
				assert(0);	
		}
	}
	
	public static bool InSByteRange(int val)
	{
		return val == cast(int)cast(byte)cast(ubyte)val;
	}
	
	public static bool InShortRange(int val)
	{
		return val == cast(int)cast(short)cast(ushort)val;
	}
	
	public override size_t writeBlock(const void* buffer, size_t size)
	{
		return ParentStream.writeBlock(buffer, size);
	}
	
	public override size_t readBlock(void* buffer, size_t size)
	{
		return ParentStream.readBlock(buffer, size);
	}
	
	public override ulong seek(long offset, SeekPos whence)
	{
		return ParentStream.seek(offset, whence);
	}
}