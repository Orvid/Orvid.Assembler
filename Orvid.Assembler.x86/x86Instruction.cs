using System;
using System.IO;
using Orvid.Assembler;

namespace Orvid.Assembler.x86
{
	public abstract class x86Instruction : Instruction
	{
		public x86Assembler ParentAssembler { get; private set; }
		protected x86InstructionForm InstructionForm { get; set; }

		private x86Instruction() { }
		protected x86Instruction(x86Assembler parentAssembler)
		{
			parentAssembler.Instructions.Add(this);
			ParentAssembler = parentAssembler;
		}

		public override void Emit(Stream strm)
		{
			Emit(new x86Stream(strm));
		}

		public abstract void Emit(x86Stream strm);

		public override string ToString()
		{
			return ToString(x86AssemblySyntax.NASM);
		}

		public abstract string ToString(x86AssemblySyntax syntax);
	}
}