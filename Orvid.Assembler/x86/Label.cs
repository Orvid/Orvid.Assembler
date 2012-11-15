using System;
using System.Collections.Generic;

namespace Orvid.Assembler.x86
{
	public sealed class TestInstruction : x86Instruction
	{
		private Label target;

		public TestInstruction(x86Assembler parentAssembler, Label targetLabel) : base(parentAssembler)
		{
			if (!targetLabel.Marked)
			{
				targetLabel.PreMarkingReferencedInstructions.Add(this);
			}
			target = targetLabel;
		}

		public override void Emit(x86Stream strm)
		{
			strm.WriteByte(0x8B);
			strm.WriteMem(0, 0);
			this.PatchableLocations.Add(
				new PatchableLocationDescription() 
				{
					TargetLabel = target, 
					PatchLocation = (uint)strm.Position - 4,
				}
			);
		}

		public override string ToString(x86AssemblySyntax syntax)
		{
			return "TestInstruction";
		}
	}

	public sealed class Label : x86Instruction
	{
		public bool Global { get; set; }
		public string Name { get; set; }

		private int markedIdx = -1;
		public bool Marked { get { return markedIdx != -1; } }

		public uint EmittedAddress { get; private set; }
		public bool Emitted { get; private set; }

		internal List<x86Instruction> PreMarkingReferencedInstructions = new List<x86Instruction>(4);

		public Label(x86Assembler parentAssembler, string name = "", bool global = false) : base(parentAssembler, false)
		{
			this.Name = name;
			this.Global = global;
		}

		public void Mark()
		{
			markedIdx = ParentAssembler.Instructions.Count;
			ParentAssembler.Instructions.Add(this);
		}

		public override void Emit(x86Stream strm)
		{
			EmittedAddress = (uint)strm.Position;
			Emitted = true;
			bool movedInStream = false;
			// Patch the instructions that have already been
			// emitted, the rest will use our address without
			// patching.
			foreach (var instr in PreMarkingReferencedInstructions)
			{
				foreach (PatchableLocationDescription pldesc in instr.PatchableLocations)
				{
					if (pldesc.TargetLabel == this)
					{
						movedInStream = true;
						strm.Position = pldesc.PatchLocation;
						if (pldesc.Relative)
						{
#warning Need to support relative patches
							throw new Exception("This isn't currently supported! :(");
						}
						else
						{
							strm.WriteAddressLiteral(EmittedAddress);
						}
					}
				}
			}
			if (movedInStream)
			{
				strm.Position = EmittedAddress;
			}
		}

		public override string ToString(x86AssemblySyntax syntax)
		{
			switch (syntax)
			{
				case x86AssemblySyntax.NASM:
					return (Global ? "" : ".") + (Name != "" ? Name : "lbl_" + EmittedAddress.ToString("X")) + ":";
				case x86AssemblySyntax.GAS:
				default:
					throw new Exception("This isn't currently supported!");
			}
		}
	}
}

