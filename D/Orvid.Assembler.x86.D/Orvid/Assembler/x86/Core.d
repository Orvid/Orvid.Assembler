module Orvid.Assembler.x86.Core;

import std.conv;
import std.stream;
import std.string;

import Orvid.Collections;
import Orvid.Assembler.Core;
import Orvid.Assembler.x86.Instructions.Manifest;

public enum x86AddressingMode : ubyte
{
	Membase0 = 0x00,
	Membase1 = 0x01,
	Membase4 = 0x02,
	Register = 0x03,
}

public enum x86Prefix : ubyte
{
	Lock = 0xF0,
	RepNE = 0xF2,
	RepNZ = 0xF2,
	Rep = 0xF3,
	RepE = 0xF3,
	RepZ = 0xF3,
	SegmentOverride_CS = 0x2E,
	SegmentOverride_SS = 0x36,
	SegmentOverride_DS = 0x3E,
	SegmentOverride_ES = 0x26,
	SegmentOverride_FS = 0x64,
	SegmentOverride_GS = 0x65,
	BranchHint_Taken = 0x2E,
	BranchHint_NotTaken = 0x3E,
	OperandSizeOverride = 0x66,
	AddressSizeOverride = 0x67,
}

public final class x86Assembler : Assembler
{
	private static const int DefaultInitialCapacity = 512;
	public List!(x86Instruction) Instructions;
	
	public this(int initialCapacity = DefaultInitialCapacity) @safe pure nothrow
	{
		Instructions = List!(x86Instruction)(initialCapacity);
	}
	
	public final override void Emit(Stream strm)
	{
		x86Stream emissionStream = new x86Stream(strm);
		for (int i = 0; i < this.Instructions.Count; i++)
		{
			Instructions[i].Emit(emissionStream);
		}
	}
}

public abstract class x86Instruction : Instruction
{
	public x86Assembler ParentAssembler;
	protected x86InstructionForm InstructionForm;
	
	protected this(x86Assembler parentAssembler)
	{
		parentAssembler.Instructions.Add(this);
		this.ParentAssembler = parentAssembler;
	}
	
	public abstract void Emit(x86Stream strm);
	
	public final override string toString()
	{
		return this.ToString(x86AssemblySyntax.NASM);
	}
	
	public abstract string ToString(x86AssemblySyntax syntax);
}

public final override class x86Stream : AssemblerStream
{
	public this(Stream parentStream) nothrow
	{
		super(parentStream, AssemblerEndianness.LittleEndian);
	}
	
	public void WritePrefix(x86Prefix prefix)
	{
		WriteByte(cast(ubyte)prefix);
	}

	public void WriteModRM(x86AddressingMode mod, ubyte opCode, ubyte rm)
	{
		WriteAddressByte(cast(ubyte)mod, opCode, rm);
	}
	
	public void WriteRegister(ubyte o, x86Register r)
	{
		WriteModRM(x86AddressingMode.Register, o, cast(ubyte)r);
	}
	
	public void WriteRegister(ubyte o, x86DebugRegister r)
	{
		WriteModRM(x86AddressingMode.Register, o, cast(ubyte)r);
	}
	
	public void WriteRegister(ubyte o, x86ControlRegister r)
	{
		WriteModRM(x86AddressingMode.Register, o, cast(ubyte)r);
	}

	public void WriteRegister(ubyte o, x86Segment r)
	{
		WriteModRM(x86AddressingMode.Register, o, cast(ubyte)r);
	}

	private void WriteAddressByte(ubyte m, ubyte o, ubyte r)
	{
		WriteByte(cast(ubyte)(((m & 0x03) << 6) | ((o & 0x07) << 3) | ((r & 0x07))));
	}

	public void WriteSegmentOverride(x86Segment selected, x86Segment defaultSeg)
	{
		if (selected != defaultSeg)
		{
			switch(selected)
			{
				case x86Segment.ES:
					WriteByte(0x26);
					break;
				case x86Segment.CS:
					WriteByte(0x2E);
					break;
				case x86Segment.SS:
					WriteByte(0x36);
					break;
				case x86Segment.DS:
					WriteByte(0x3E);
					break;
				case x86Segment.FS:
					WriteByte(0x64);
					break;
				case x86Segment.GS:
					WriteByte(0x65);
					break;
				default:
					throw new Exception("Unknown x86 Segment!");
			}
		}
	}

	public void WriteMem(ubyte r, uint address)
	{
		WriteAddressByte(0, r, cast(ubyte)x86Register.EBP);
		WriteImm32(address);
	}

	public void WriteMembase(ubyte r, x86Register baseReg, int displacement)
	{
		if (baseReg == x86Register.ESP)
		{
			if (displacement == 0)
			{
				WriteAddressByte(0, r, cast(ubyte)x86Register.ESP);
				WriteAddressByte(0, cast(ubyte)x86Register.ESP, cast(ubyte)x86Register.ESP);
			}
			else if (InSByteRange(displacement))
			{
				WriteAddressByte(1, r, cast(ubyte)x86Register.ESP);
				WriteAddressByte(0, cast(ubyte)x86Register.ESP, cast(ubyte)x86Register.ESP);
				WriteImm8(cast(ubyte)displacement);
			}
			else
			{
				WriteAddressByte(2, r, cast(ubyte)x86Register.ESP);
				WriteAddressByte(0, cast(ubyte)x86Register.ESP, cast(ubyte)x86Register.ESP);
				WriteImm32(cast(uint)displacement);
			}
		}
		else if (displacement == 0 && baseReg != x86Register.EBP)
		{
			WriteAddressByte(0, r, cast(ubyte)baseReg);
		}
		else if (InSByteRange(displacement))
		{
			WriteAddressByte(1, r, cast(ubyte)baseReg);
			WriteImm8(cast(ubyte)displacement);
		}
		else
		{
			WriteAddressByte(2, r, cast(ubyte)baseReg);
			WriteImm32(cast(uint)displacement);
		}
	}

	public void WriteMemIndex(ubyte r, x86Register baseReg, int displacement, x86Register indexReg, x86IndexShift iShift)
	{
		ubyte shift = cast(ubyte)iShift;
		switch (shift)
		{
			case 0:
			case 1:
			case 2:
			case 3:
				break;
			default:
				throw new Exception("Invalid shift!");
		}
		if (baseReg == x86Register.None)
		{
			WriteAddressByte(0, r, 4);
			WriteAddressByte(shift, cast(ubyte)indexReg, 5);
			WriteImm32(cast(uint)displacement);
		}
		else if (displacement == 0 && baseReg != x86Register.EBP)
		{
			WriteAddressByte(0, r, 4);
			WriteAddressByte(shift, cast(ubyte)indexReg, cast(ubyte)baseReg);
		}
		else if (InSByteRange(displacement))
		{
			WriteAddressByte(1, r, 4);
			WriteAddressByte(shift, cast(ubyte)indexReg, cast(ubyte)baseReg);
			WriteImm8(cast(ubyte)displacement);
		}
		else
		{
			WriteAddressByte(2, r, 4);
			WriteAddressByte(shift, cast(ubyte)indexReg, cast(ubyte)baseReg);
			WriteImm32(cast(uint)displacement);
		}
	}
}

public static class NamingHelper
{
	public static string NameFPURegister(x86FPURegister reg)
	{
		switch (reg)
		{
			case x86FPURegister.ST0:
				return "ST(0)";
			case x86FPURegister.ST1:
				return "ST(1)";
			case x86FPURegister.ST2:
				return "ST(2)";
			case x86FPURegister.ST3:
				return "ST(3)";
			case x86FPURegister.ST4:
				return "ST(4)";
			case x86FPURegister.ST5:
				return "ST(5)";
			case x86FPURegister.ST6:
				return "ST(6)";
			case x86FPURegister.ST7:
				return "ST(7)";
			default:
				throw new Exception("Invalid FPU Register!");
		}
	}

	public static string NameSegment(x86Segment seg, bool upper = false)
	{
		switch (seg)
		{
			case x86Segment.CS:
				return upper ? "CS" : "cs";
			case x86Segment.DS:
				return upper ? "DS" : "ds";
			case x86Segment.ES:
				return upper ? "ES" : "es";
			case x86Segment.FS:
				return upper ? "FS" : "fs";
			case x86Segment.GS:
				return upper ? "GS" : "gs";
			case x86Segment.SS:
				return upper ? "SS" : "ss";
			default:
				throw new Exception("Invalid segment!");
		}
	}
	
	public static string NameControlRegister(x86ControlRegister reg)
	{
		switch (reg)
		{
			case x86ControlRegister.CR0:
				return "CR0";
			case x86ControlRegister.CR1:
				return "CR1";
			case x86ControlRegister.CR2:
				return "CR2";
			case x86ControlRegister.CR3:
				return "CR3";
			case x86ControlRegister.CR4:
				return "CR4";
			case x86ControlRegister.CR5:
				return "CR5";
			case x86ControlRegister.CR6:
				return "CR6";
			case x86ControlRegister.CR7:
				return "CR7";
			default:
				throw new Exception("Invalid Control Register!");
		}
	}
	
	public static string NameDebugRegister(x86DebugRegister reg)
	{
		switch (reg)
		{
			case x86DebugRegister.DR0:
				return "DR0";
			case x86DebugRegister.DR1:
				return "DR1";
			case x86DebugRegister.DR2:
				return "DR2";
			case x86DebugRegister.DR3:
				return "DR3";
			case x86DebugRegister.DR4:
				return "DR4";
			case x86DebugRegister.DR5:
				return "DR5";
			case x86DebugRegister.DR6:
				return "DR6";
			case x86DebugRegister.DR7:
				return "DR7";
			default:
				throw new Exception("Invalid Debug Register!");
		}
	}

	public static string NameRegister(x86Register reg, ubyte size)
	{
		if (reg == x86Register.None)
		{
			throw new Exception("This should never happen!");
		}
		switch (size)
		{
			case 1:
				switch(reg)
				{
					case x86Register.EAX:
						return "AL";
					case x86Register.EBP:
						return "CH";
					case x86Register.EBX:
						return "BL";
					case x86Register.ECX:
						return "CL";
					case x86Register.EDI:
						return "BH";
					case x86Register.EDX:
						return "DL";
					case x86Register.ESI:
						return "DH";
					case x86Register.ESP:
						return "AH";
					default:
						throw new Exception("Unknown register!");
				}
			case 2:
				switch(reg)
				{
					case x86Register.EAX:
						return "AX";
					case x86Register.EBP:
						return "BP";
					case x86Register.EBX:
						return "BX";
					case x86Register.ECX:
						return "CX";
					case x86Register.EDI:
						return "DI";
					case x86Register.EDX:
						return "DX";
					case x86Register.ESI:
						return "SI";
					case x86Register.ESP:
						return "SP";
					default:
						throw new Exception("Unknown register!");
				}
			case 4:
				switch(reg)
				{
					case x86Register.EAX:
						return "EAX";
					case x86Register.EBP:
						return "EBP";
					case x86Register.EBX:
						return "EBX";
					case x86Register.ECX:
						return "ECX";
					case x86Register.EDI:
						return "EDI";
					case x86Register.EDX:
						return "EDX";
					case x86Register.ESI:
						return "ESI";
					case x86Register.ESP:
						return "ESP";
					default:
						throw new Exception("Unknown register!");
				}
			default:
				throw new Exception("Unknown size!");
		}
	}

	public static string NameMembase(x86Register reg, int displacement)
	{
		int sVal = displacement;
		if (displacement == 0)
		{
			return "[" ~ NameRegister(reg, 4) ~ "]";
		}
		else if (x86Stream.InSByteRange(sVal))
		{
			return "[" ~ NameRegister(reg, 4) ~ " " ~ (sVal < 0 ? "-" : "+") ~ " " ~ to!string(sVal < 0 ? -sVal : sVal) ~ "]";
		}
		else
		{
			return "[" ~ NameRegister(reg, 4) ~ " " ~ (sVal < 0 ? "-" : "+") ~ " " ~ to!string(sVal < 0 ? -sVal : sVal) ~ "]";
		}
	}

	public static string NameDisplacement(x86Assembler a, x86Instruction baseInstr, int displacement)
	{
		return "[rel " ~ (displacement < 0 ? " - " : " + ") ~ to!string(displacement & 0x7FFFFFFF) ~ "]";
	}

	public static string NameMem(x86Assembler a, uint address, x86Segment segment)
	{
		// Assembler gets passed in here because we will eventually do symbol lookup.
		return "[" ~ NameSegment(segment) ~ ":0x" ~ format("%08X", address) ~ "]";
	}

	public static string NameMemIndex(x86Assembler a, x86Register baseRegister, int displacement, x86Register indexRegister, x86IndexShift shift, x86Segment segment)
	{
		string ret = "";
		bool form2 = baseRegister == x86Register.None;
		if (form2)
		{
			ret ~= NameSegment(segment) ~ ":0x" ~ format("%08X", displacement);
		}
		ret ~= "[" ~ NameRegister(indexRegister, 4);
		switch (shift)
		{
			case x86IndexShift.Mul1:
				break;
			case x86IndexShift.Mul2:
				ret ~= " * 2";
				break;
			case x86IndexShift.Mul4:
				ret ~= " * 4";
				break;
			case x86IndexShift.Mul8:
				ret ~= " * 8";
				break;
			default:
				throw new Exception("Invalid index shift!");
		}
		if (baseRegister != x86Register.None)
		{
			ret ~= " + " ~ NameRegister(baseRegister, 4);
		}
		if (displacement != 0 && !form2)
		{
			ret ~= " " ~ (displacement < 0 ? "-" : "+") ~ " " ~ to!string(displacement < 0 ? -displacement : displacement);
		}
		return ret ~ "]";
	}
}