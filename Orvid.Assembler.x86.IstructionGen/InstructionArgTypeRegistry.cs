using System;
using System.Collections.Generic;

namespace Orvid.Assembler.x86.IstructionGen
{
	public static class InstructionArgTypeRegistry
	{
		// The key here is in all capitol letters.
		private static Dictionary<string, InstructionArgType> TypeLookup;

		static InstructionArgTypeRegistry()
		{
			Initialize();
		}

		private static void Initialize()
		{
			TypeLookup = new Dictionary<string, InstructionArgType>()
			{
				// The IDs of these are hard-coded.
				{ "NONE",  InstructionArgType.None  }, // 0
				{ "IMM8",  InstructionArgType.Imm8  }, // 1
				{ "IMM16", InstructionArgType.Imm16 }, // 2
				{ "IMM32", InstructionArgType.Imm32 }, // 3
				{ "DIS8",  InstructionArgType.Dis8  }, // 4
				{ "DIS16", InstructionArgType.Dis16 }, // 5
				{ "DIS32", InstructionArgType.Dis32 }, // 6
			};
		}

		public static void Reset()
		{
			Initialize();
		}

		public static void RegisterType(InstructionArgType tp)
		{
			if (TypeLookup.ContainsKey(tp.Name))
				throw new Exception("Duplicate arg type definition!");
			tp.ID = TypeLookup.Count;
			TypeLookup[tp.Name.ToUpper()] = tp;
		}

		public static InstructionArgType GetType(string name)
		{
			InstructionArgType tp;
			if (!TypeLookup.TryGetValue(name.ToUpper(), out tp))
				throw new Exception("Unknown arg type '" + name + "'!");
			if (tp.IsAlias)
				return tp.AliasTo;
			return tp;
		}

	}
}

