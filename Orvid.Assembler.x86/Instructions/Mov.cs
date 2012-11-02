using System;

namespace Orvid.Assembler.x86
{
	public sealed class Mov : x86Instruction
	{
		private uint Imm32;
		private ushort Imm16;
		private byte Imm8;
		private x86Register SourceRegister;
		private x86Register DestinationRegister;
		private x86IndexShift Shift;
		private x86Register IndexRegister;
		private x86Segment Segment;

		//public Mov(x86Assembler parentAssembler, sbyte offset) : base(parentAssembler)
		//{
		//
		//}
		
		public Mov(x86Assembler parentAssembler, x86Register destination, byte imm8) : base(parentAssembler)
		{
			this.InstructionForm = x86InstructionForm.R8_Imm8;
			this.DestinationRegister = destination;
			this.Imm8 = imm8;
		}
		public Mov(x86Assembler parentAssembler, x86Register destination, ushort imm16) : base(parentAssembler)
		{
			this.InstructionForm = x86InstructionForm.R16_Imm16;
			this.DestinationRegister = destination;
			this.Imm16 = imm16;
		}
		public Mov(x86Assembler parentAssembler, x86Register destination, uint imm32) : base(parentAssembler)
		{
			this.InstructionForm = x86InstructionForm.R32_Imm32;
			this.DestinationRegister = destination;
			this.Imm32 = imm32;
		}

		public Mov(x86Assembler parentAssembler, x86Register destination, x86Register source, byte size) : base(parentAssembler)
		{
			SourceRegister = source;
			DestinationRegister = destination;
			switch (size)
			{
				case 1:
					InstructionForm = x86InstructionForm.R8_R8;
					break;
				case 2:
					InstructionForm = x86InstructionForm.R16_R16;
					break;
				case 4:
					InstructionForm = x86InstructionForm.R32_R32;
					break;
				default:
					throw new ArgumentOutOfRangeException("size", "Invalid size!");
			}
		}
		
		/// <summary>
		/// This constructor is used for membase forms of this instruction.
		/// </summary>
		public Mov(x86Assembler parentAssembler, x86Register destination, x86Register source, int sourceOffset, byte size) : base(parentAssembler)
		{
			SourceRegister = source;
			DestinationRegister = destination;
			Imm32 = (uint)sourceOffset;
			switch (size)
			{
				case 1:
					InstructionForm = x86InstructionForm.R8_Membase8;
					break;
				case 2:
					InstructionForm = x86InstructionForm.R16_Membase8;
					break;
				case 4:
					InstructionForm = x86InstructionForm.R32_Membase8;
					break;
				default:
					throw new ArgumentOutOfRangeException("size", "Invalid size!");
			}
		}
		
		/// <summary>
		/// This constructor is used for membase forms of this instruction.
		/// </summary>
		public Mov(x86Assembler parentAssembler, x86Register destination, int destinationOffset, x86Register source, byte size) : base(parentAssembler)
		{
			SourceRegister = source;
			DestinationRegister = destination;
			Imm32 = (uint)destinationOffset;
			switch (size)
			{
				case 1:
					InstructionForm = x86InstructionForm.Membase_R8;
					break;
				case 2:
					InstructionForm = x86InstructionForm.Membase_R16;
					break;
				case 4:
					InstructionForm = x86InstructionForm.Membase_R32;
					break;
				default:
					throw new ArgumentOutOfRangeException("size", "Invalid size!");
			}
		}
		
		public Mov(x86Assembler parentAssembler, x86Register destination, uint address, byte size, x86Segment segment = x86Segment.DS) : base(parentAssembler)
		{
			switch (size)
			{
				case 1:
					InstructionForm = x86InstructionForm.R8_Mem;
					break;
				case 2:
					InstructionForm = x86InstructionForm.R16_Mem;
					break;
				case 4:
					InstructionForm = x86InstructionForm.R32_Mem;
					break;
				default:
					throw new ArgumentOutOfRangeException("size", "Invalid size!");
			}
			Imm32 = address;
			DestinationRegister = destination;
			Segment = segment;
		}
		
		public Mov(x86Assembler parentAssembler, uint address, x86Register source, byte size, x86Segment segment = x86Segment.DS) : base(parentAssembler)
		{
			switch (size)
			{
				case 1:
					InstructionForm = x86InstructionForm.Mem_R8;
					break;
				case 2:
					InstructionForm = x86InstructionForm.Mem_R16;
					break;
				case 4:
					InstructionForm = x86InstructionForm.Mem_R32;
					break;
				default:
					throw new ArgumentOutOfRangeException("size", "Invalid size!");
			}
			Imm32 = address;
			SourceRegister = source;
			Segment = segment;
		}
		
		public Mov(x86Assembler parentAssembler, x86Register destination, x86Register sourceBaseRegister, int sourceDisplacement, x86Register sourceIndexRegister, x86IndexShift sourceShift, byte size, x86Segment segment = x86Segment.DS) : base(parentAssembler)
		{
			switch (size)
			{
				case 1:
					InstructionForm = x86InstructionForm.R8_MemIndex;
					break;
				case 2:
					InstructionForm = x86InstructionForm.R16_MemIndex;
					break;
				case 4:
					InstructionForm = x86InstructionForm.R32_MemIndex;
					break;
				default:
					throw new ArgumentOutOfRangeException("size", "Invalid size!");
			}
			Imm32 = (uint)sourceDisplacement;
			DestinationRegister = destination;
			SourceRegister = sourceBaseRegister;
			IndexRegister = sourceIndexRegister;
			Shift = sourceShift;
			Segment = segment;
		}
		
		public Mov(x86Assembler parentAssembler, x86Register destinationBaseRegister, int destinationDisplacement, x86Register destinationIndexRegister, x86IndexShift destinationShift, x86Register source, byte size, x86Segment segment = x86Segment.DS) : base(parentAssembler)
		{
			switch (size)
			{
				case 1:
					InstructionForm = x86InstructionForm.MemIndex_R8;
					break;
				case 2:
					InstructionForm = x86InstructionForm.MemIndex_R16;
					break;
				case 4:
					InstructionForm = x86InstructionForm.MemIndex_R32;
					break;
				default:
					throw new ArgumentOutOfRangeException("size", "Invalid size!");
			}
			Imm32 = (uint)destinationDisplacement;
			DestinationRegister = destinationBaseRegister;
			SourceRegister = source;
			IndexRegister = destinationIndexRegister;
			Shift = destinationShift;
			Segment = segment;
		}

		public override void Emit(x86Stream strm)
		{
			switch (InstructionForm)
			{
				case x86InstructionForm.R8_Imm8:
					strm.WriteByte((byte)(0xB0 + (byte)DestinationRegister));
					strm.WriteImm8(Imm8);
					break;
				case x86InstructionForm.R16_Imm16:
					strm.WritePrefix(x86Prefix.OperandSizeOverride);
					strm.WriteByte((byte)(0xB8 + (byte)DestinationRegister));
					strm.WriteImm16(Imm16);
					break;
				case x86InstructionForm.R32_Imm32:
					strm.WriteByte((byte)(0xB8 + (byte)DestinationRegister));
					strm.WriteImm32(Imm32);
					break;
				case x86InstructionForm.R8_R8:
					strm.WriteByte(0x88);
					strm.WriteModRM(x86AddressingMode.Register, (byte)SourceRegister, (byte)DestinationRegister);
					break;
				case x86InstructionForm.R16_R16:
					strm.WritePrefix(x86Prefix.OperandSizeOverride);
					strm.WriteByte(0x89);
					strm.WriteModRM(x86AddressingMode.Register, (byte)SourceRegister, (byte)DestinationRegister);
					break;
				case x86InstructionForm.R32_R32:
					strm.WriteByte(0x89);
					strm.WriteModRM(x86AddressingMode.Register, (byte)SourceRegister, (byte)DestinationRegister);
					break;
				case x86InstructionForm.R8_Membase:
					strm.WriteByte(0x8A);
					strm.WriteMembase((byte)DestinationRegister, SourceRegister, (int)Imm32);
					break;
				case x86InstructionForm.R16_Membase:
					strm.WritePrefix(x86Prefix.OperandSizeOverride);
					strm.WriteByte(0x8B);
					strm.WriteMembase((byte)DestinationRegister, SourceRegister, (int)Imm32);
					break;
				case x86InstructionForm.R32_Membase:
					strm.WriteByte(0x8B);
					strm.WriteMembase((byte)DestinationRegister, SourceRegister, (int)Imm32);
					break;
				case x86InstructionForm.Membase_R8:
					strm.WriteByte(0x88);
					strm.WriteMembase((byte)SourceRegister, DestinationRegister, (int)Imm32);
					break;
				case x86InstructionForm.Membase_R16:
					strm.WritePrefix(x86Prefix.OperandSizeOverride);
					strm.WriteByte(0x89);
					strm.WriteMembase((byte)SourceRegister, DestinationRegister, (int)Imm32);
					break;
				case x86InstructionForm.Membase_R32:
					strm.WriteByte(0x89);
					strm.WriteMembase((byte)SourceRegister, DestinationRegister, (int)Imm32);
					break;
				case x86InstructionForm.R8_Mem:
					strm.WriteSegmentOverride(Segment, x86Segment.DS);
					strm.WriteByte(0x8A);
					strm.WriteMem((byte)DestinationRegister, Imm32);
					break;
				case x86InstructionForm.R16_Mem:
					strm.WriteSegmentOverride(Segment, x86Segment.DS);
					strm.WritePrefix(x86Prefix.OperandSizeOverride);
					strm.WriteByte(0x8B);
					strm.WriteMem((byte)DestinationRegister, Imm32);
					break;
				case x86InstructionForm.R32_Mem:
					strm.WriteSegmentOverride(Segment, x86Segment.DS);
					strm.WriteByte(0x8B);
					strm.WriteMem((byte)DestinationRegister, Imm32);
					break;
				case x86InstructionForm.Mem_R8:
					strm.WriteSegmentOverride(Segment, x86Segment.DS);
					strm.WriteByte(0x88);
					strm.WriteMem((byte)SourceRegister, Imm32);
					break;
				case x86InstructionForm.Mem_R16:
					strm.WriteSegmentOverride(Segment, x86Segment.DS);
					strm.WritePrefix(x86Prefix.OperandSizeOverride);
					strm.WriteByte(0x89);
					strm.WriteMem((byte)SourceRegister, Imm32);
					break;
				case x86InstructionForm.Mem_R32:
					strm.WriteSegmentOverride(Segment, x86Segment.DS);
					strm.WriteByte(0x89);
					strm.WriteMem((byte)SourceRegister, Imm32);
					break;
				case x86InstructionForm.MemIndex_R8:
					strm.WriteSegmentOverride(Segment, x86Segment.DS);
					strm.WriteByte(0x88);
					strm.WriteMemIndex((byte)SourceRegister, DestinationRegister, (int)Imm32, IndexRegister, (byte)Shift);
					break;
				case x86InstructionForm.MemIndex_R16:
					strm.WriteSegmentOverride(Segment, x86Segment.DS);
					strm.WritePrefix(x86Prefix.OperandSizeOverride);
					strm.WriteByte(0x89);
					strm.WriteMemIndex((byte)SourceRegister, DestinationRegister, (int)Imm32, IndexRegister, (byte)Shift);
					break;
				case x86InstructionForm.MemIndex_R32:
					strm.WriteSegmentOverride(Segment, x86Segment.DS);
					strm.WriteByte(0x89);
					strm.WriteMemIndex((byte)SourceRegister, DestinationRegister, (int)Imm32, IndexRegister, (byte)Shift);
					break;
				case x86InstructionForm.R8_MemIndex:
					strm.WriteSegmentOverride(Segment, x86Segment.DS);
					strm.WriteByte(0x8A);
					strm.WriteMemIndex((byte)DestinationRegister, SourceRegister, (int)Imm32, IndexRegister, (byte)Shift);
					break;
				case x86InstructionForm.R16_MemIndex:
					strm.WriteSegmentOverride(Segment, x86Segment.DS);
					strm.WritePrefix(x86Prefix.OperandSizeOverride);
					strm.WriteByte(0x8B);
					strm.WriteMemIndex((byte)DestinationRegister, SourceRegister, (int)Imm32, IndexRegister, (byte)Shift);
					break;
				case x86InstructionForm.R32_MemIndex:
					strm.WriteSegmentOverride(Segment, x86Segment.DS);
					strm.WriteByte(0x8B);
					strm.WriteMemIndex((byte)DestinationRegister, SourceRegister, (int)Imm32, IndexRegister, (byte)Shift);
					break;
			}
		}

		public override string ToString(x86AssemblySyntax syntax)
		{
			switch (syntax)
			{
				case x86AssemblySyntax.NASM:
					switch(InstructionForm)
					{
						case x86InstructionForm.R8_R8:
							return "mov " + NamingHelper.NameRegister(DestinationRegister, 1) + ", " + NamingHelper.NameRegister(SourceRegister, 1);
						case x86InstructionForm.R8_Imm8:
							return "mov " + NamingHelper.NameRegister(DestinationRegister, 1) + ", 0x" + Imm8.ToString("X").PadLeft(2, '0');
						case x86InstructionForm.R8_Mem:
							return "mov " + NamingHelper.NameRegister(DestinationRegister, 1) + ", " + NamingHelper.NameMem(ParentAssembler, Imm32, Segment);
						case x86InstructionForm.R8_Membase:
							return "mov " + NamingHelper.NameRegister(DestinationRegister, 1) + ", " + NamingHelper.NameMembase(SourceRegister, Imm32);
						case x86InstructionForm.R8_MemIndex:
							return "mov " + NamingHelper.NameRegister(DestinationRegister, 1) + ", " + NamingHelper.NameMemindex(ParentAssembler, SourceRegister, (int)Imm32, IndexRegister, Shift, Segment);
						case x86InstructionForm.R16_R16:
							return "mov " + NamingHelper.NameRegister(DestinationRegister, 2) + ", " + NamingHelper.NameRegister(SourceRegister, 2);
						case x86InstructionForm.R16_Imm16:
							return "mov " + NamingHelper.NameRegister(DestinationRegister, 2) + ", 0x" + Imm16.ToString("X").PadLeft(4, '0');
						case x86InstructionForm.R16_Mem:
							return "mov " + NamingHelper.NameRegister(DestinationRegister, 2) + ", " + NamingHelper.NameMem(ParentAssembler, Imm32, Segment);
						case x86InstructionForm.R16_Membase:
							return "mov " + NamingHelper.NameRegister(DestinationRegister, 2) + ", " + NamingHelper.NameMembase(SourceRegister, Imm32);
						case x86InstructionForm.R16_MemIndex:
							return "mov " + NamingHelper.NameRegister(DestinationRegister, 2) + ", " + NamingHelper.NameMemindex(ParentAssembler, SourceRegister, (int)Imm32, IndexRegister, Shift, Segment);
						case x86InstructionForm.R32_R32:
							return "mov " + NamingHelper.NameRegister(DestinationRegister, 4) + ", " + NamingHelper.NameRegister(SourceRegister, 4);
						case x86InstructionForm.R32_Imm32:
							return "mov " + NamingHelper.NameRegister(DestinationRegister, 4) + ", 0x" + Imm32.ToString("X").PadLeft(8, '0');
						case x86InstructionForm.R32_Mem:
							return "mov " + NamingHelper.NameRegister(DestinationRegister, 4) + ", " + NamingHelper.NameMem(ParentAssembler, Imm32, Segment);
						case x86InstructionForm.R32_Membase:
							return "mov " + NamingHelper.NameRegister(DestinationRegister, 4) + ", " + NamingHelper.NameMembase(SourceRegister, Imm32);
						case x86InstructionForm.R32_MemIndex:
							return "mov " + NamingHelper.NameRegister(DestinationRegister, 4) + ", " + NamingHelper.NameMemindex(ParentAssembler, SourceRegister, (int)Imm32, IndexRegister, Shift, Segment);
						case x86InstructionForm.Mem_R8:
							return "mov " + NamingHelper.NameMem(ParentAssembler, Imm32, Segment) + ", " + NamingHelper.NameRegister(SourceRegister, 1);
						case x86InstructionForm.Mem_R16:
							return "mov " + NamingHelper.NameMem(ParentAssembler, Imm32, Segment) + ", " + NamingHelper.NameRegister(SourceRegister, 2);
						case x86InstructionForm.Mem_R32:
							return "mov " + NamingHelper.NameMem(ParentAssembler, Imm32, Segment) + ", " + NamingHelper.NameRegister(SourceRegister, 4);
						case x86InstructionForm.Membase_R32:
							return "mov " + NamingHelper.NameMembase(DestinationRegister, Imm32) + ", " + NamingHelper.NameRegister(SourceRegister, 4);
						case x86InstructionForm.Membase_R16:
							return "mov " + NamingHelper.NameMembase(DestinationRegister, Imm32) + ", " + NamingHelper.NameRegister(SourceRegister, 2);
						case x86InstructionForm.Membase_R8:
							return "mov " + NamingHelper.NameMembase(DestinationRegister, Imm32) + ", " + NamingHelper.NameRegister(SourceRegister, 1);
						case x86InstructionForm.MemIndex_R8:
							return "mov " + NamingHelper.NameMemindex(ParentAssembler, DestinationRegister, (int)Imm32, IndexRegister, Shift, Segment) + ", " + NamingHelper.NameRegister(SourceRegister, 1);
						case x86InstructionForm.MemIndex_R16:
							return "mov " + NamingHelper.NameMemindex(ParentAssembler, DestinationRegister, (int)Imm32, IndexRegister, Shift, Segment) + ", " + NamingHelper.NameRegister(SourceRegister, 2);
						case x86InstructionForm.MemIndex_R32:
							return "mov " + NamingHelper.NameMemindex(ParentAssembler, DestinationRegister, (int)Imm32, IndexRegister, Shift, Segment) + ", " + NamingHelper.NameRegister(SourceRegister, 4);

						default:
							throw new Exception("Unsupported instruction form!");
					}
				case x86AssemblySyntax.GAS:
					throw new Exception("GAS syntax isn't currently supported!");
				default:
					throw new Exception("Unknown assembly syntax!");
			}
		}

	}
}

