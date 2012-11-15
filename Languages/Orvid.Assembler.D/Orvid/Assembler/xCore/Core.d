module Orvid.Assembler.xCore.Core;

import std.stream;

import Orvid.Collections;
import Orvid.Assembler.Core;
import Orvid.Assembler.xCore.Instructions.Manifest;

public final class xCoreAssembler : Assembler
{
	private static const int DefaultInitialCapacity = 512;
	public List!(xCoreInstruction) Instructions;
	
	public this(int initialCapacity = DefaultInitialCapacity) @safe pure nothrow
	{
		Instructions = List!(xCoreInstruction)(initialCapacity);
	}
	
	public final override void Emit(Stream strm)
	{
		xCoreStream emissionStream = new xCoreStream(strm);
		for (int i = 0; i < this.Instructions.Count; i++)
		{
			Instructions[i].Emit(emissionStream);
		}
	}
}

public abstract class xCoreInstruction : Instruction
{
	public xCoreAssembler ParentAssembler;
	
	protected this(xCoreAssembler parentAssembler)
	{
		parentAssembler.Instructions.Add(this);
		this.ParentAssembler = parentAssembler;
	}
	
	public abstract void Emit(xCoreStream strm);
}

public final class xCoreStream : AssemblerStream
{
	public this(Stream parentStream) nothrow
	{
		super(parentStream, AssemblerEndianness.LittleEndian);
	}
}