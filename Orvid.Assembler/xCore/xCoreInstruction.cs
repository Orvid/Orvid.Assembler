using System;
using System.IO;

namespace Orvid.Assembler.xCore
{
	public abstract class xCoreInstruction : Instruction
	{
		public xCoreAssembler ParentAssembler { get; private set; }

		private xCoreInstruction() { }
		protected xCoreInstruction(xCoreAssembler parentAssembler)
		{
			parentAssembler.Instructions.Add(this);
			this.ParentAssembler = parentAssembler;
		}

		public override void Emit(Stream strm)
		{
			Emit(new xCoreStream(strm));
		}

		public abstract void Emit(xCoreStream strm);

	}
}

