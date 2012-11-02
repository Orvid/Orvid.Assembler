using System;

namespace Orvid.Assembler.x86.IstructionGen
{
	public struct InstructionArg
	{
		// This is always assigned when
		// the object is created.
		public InstructionArgType ArgType;
		public string Name;
		public string DefaultValue;
		public ImmNumberFormat NumberFormat;
		
		public override string ToString()
		{
			return (Name != "" ? Name + ": " : "") + "Type: " + ArgType.ToString() + (DefaultValue != "" ? " Default: " + DefaultValue : "");
		}
	}
}

