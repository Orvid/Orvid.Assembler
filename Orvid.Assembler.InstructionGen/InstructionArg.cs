using System;

namespace Orvid.Assembler.InstructionGen
{
	public struct InstructionArg
	{
		// This is always assigned when
		// the object is created.
		public InstructionArgType ArgType;
		public string Name;
		public int? DefaultValue;
		public ImmNumberFormat NumberFormat;
		
		public override string ToString()
		{
			return (Name != "" ? Name + ": " : "") + "Type: " + ArgType.ToString() + (DefaultValue.HasValue ? " Default: " + DefaultValue.Value.ToString() : "");
		}
	}
}

