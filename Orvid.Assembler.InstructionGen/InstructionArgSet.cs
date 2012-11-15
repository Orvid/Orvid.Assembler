using System;

namespace Orvid.Assembler.InstructionGen
{
	/// <summary>
	/// This represents an instruction's set of args,
	/// it MUST stay a structure otherwise the dictionary
	/// lookups done using it would fail.
	/// </summary>
	public struct InstructionArgSet
	{
		public const int TotalArgs_Max = 3;

		public InstructionArgType Arg1;
		public InstructionArgType Arg2;
		public InstructionArgType Arg3;

		/// <summary>
		/// This should be called on all values
		/// of this type. It initializes the class
		/// fields.
		/// </summary>
		public void Init()
		{
			Arg1 = InstructionArgType.None;
			Arg2 = InstructionArgType.None;
			Arg3 = InstructionArgType.None;
		}

		public override int GetHashCode()
		{
			return (Arg1.ID << 16) | (Arg2.ID << 8) | Arg3.ID;
		}

		public string GetInstructionCaseString()
		{
			if (Arg3 != InstructionArgType.None)
			{
				return Arg1.TrueName + "_" + Arg2.TrueName + "_" + Arg3.TrueName;
			}
			else if (Arg2 != InstructionArgType.None)
			{
				return Arg1.TrueName + "_" + Arg2.TrueName;
			}
			else
			{
				return Arg1.TrueName;
			}
		}

		public InstructionArgType this[int idx]
		{
			get
			{
				switch(idx)
				{
					case 0:
						return Arg1;
					case 1:
						return Arg2;
					case 2:
						return Arg3;
					default:
						throw new Exception("Invalid arg index!");
				}
			}
			set
			{
				switch(idx)
				{
					case 0:
						Arg1 = value;
						break;
					case 1:
						Arg2 = value;
						break;
					case 2:
						Arg3 = value;
						break;
					default:
						throw new Exception("Invalid arg index!");
				}
			}
		}
		
		public override string ToString()
		{
			if (Arg3 != InstructionArgType.None)
			{
				return "Arg1: " + Arg1.ToString() + ", Arg2: " + Arg2.ToString() + ", Arg3: " + Arg3.ToString();
			}
			else if (Arg2 != InstructionArgType.None)
			{
				return "Arg1: " + Arg1.ToString() + ", Arg2: " + Arg2.ToString();
			}
			else if (Arg1 != InstructionArgType.None)
			{
				return "Arg1: " + Arg1.ToString();
			}
			else
			{
				return "No Args";
			}
		}
	}
}

