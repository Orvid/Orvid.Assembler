using System;
using System.Collections.Generic;

namespace Orvid.Assembler.x86.IstructionGen
{
	public static class SizelessTypeRegistry
	{
		private static Dictionary<string, SizelessType> RegisteredTypes;

		static SizelessTypeRegistry()
		{
			Initialize();
		}

		private static void Initialize()
		{
			RegisteredTypes = new Dictionary<string, SizelessType>()
			{
				// These IDs are hard-coded
				{ "None", SizelessType.None },
				{ "Imm",  SizelessType.Imm  },
				{ "Dis",  SizelessType.Dis  }
			};
		}

		public static void Reset()
		{
			Initialize();
		}

		public static SizelessType GetType(string name)
		{
			SizelessType tp;
			if (!RegisteredTypes.TryGetValue(name, out tp))
				throw new Exception("Unknown sizeless type '" + name + "'!");
			return tp;
		}

		public static void RegisterType(string name)
		{
			RegisteredTypes.Add(name, new SizelessType(RegisteredTypes.Count, name));
		}

	}
}

