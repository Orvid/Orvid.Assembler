using System;
using System.IO;
using System.Collections.Generic;
using Orvid.Assembler;

namespace Orvid.Assembler.x86
{
	/// <summary>
	/// Represents a patchable location in the emission stream.
	/// </summary>
	public sealed class PatchableLocationDescription
	{
		/// <summary>
		/// The target label.
		/// </summary>
		public Label TargetLabel;
		/// <summary>
		/// The location in the emission stream where the
		/// patching needs to occur.
		/// </summary>
		public uint PatchLocation;
		/// <summary>
		/// True if the emitted value should be a relative
		/// address rather than an absolute one.
		/// </summary>
		public bool Relative;
	}

	public abstract class x86Instruction : Instruction
	{
		public List<PatchableLocationDescription> PatchableLocations = new List<PatchableLocationDescription>();

		public x86Assembler ParentAssembler { get; private set; }
		protected x86InstructionForm InstructionForm { get; set; }

		private x86Instruction() { }
		protected x86Instruction(x86Assembler parentAssembler, bool addToParent = true)
		{
			// This is so that a label doesn't get added when it's created.
			if (addToParent) 
			{
				parentAssembler.Instructions.Add(this);
			}
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