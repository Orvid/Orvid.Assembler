using System;

namespace Orvid.Assembler.x86.IstructionGen
{
	public sealed class SizelessType
	{
		public static readonly SizelessType None = new SizelessType(0, "None");
		public static readonly SizelessType Imm = new SizelessType(1, "Imm");
		public static readonly SizelessType Dis = new SizelessType(2, "Dis");

		public int ID { get; private set; }
		public string Name { get; private set; }

		public SizelessType(int id, string name)
		{
			this.ID = id;
			this.Name = name;
		}
		
		public override int GetHashCode()
		{
			return ID;
		}

	}
}

