using System;
using System.Collections.Generic;

namespace Orvid.Assembler.InstructionGen
{
	public static class PrefixRegistry
	{
		private struct Prefix
		{
			public readonly string WriteOperationName;
			public readonly string Name;

			public Prefix(string writeOperationName, string name)
			{
				this.WriteOperationName = writeOperationName;
				this.Name = name;
			}
		}

		private static Dictionary<string, Prefix> Prefixes;

		static PrefixRegistry()
		{
			Initialize();
		}

		private static void Initialize()
		{
			Prefixes = new Dictionary<string, Prefix>();
		}

		public static void Reset()
		{
			Initialize();
		}

		public static void Register(string writeOperationName, string name)
		{
			if (Prefixes.ContainsKey(writeOperationName))
				throw new Exception("Duplicate prefix operation '" + writeOperationName + "'!");
			Prefixes[writeOperationName] = new Prefix(writeOperationName, name);
		}

		public static string GetPrefixName(string writeOperationName)
		{
			Prefix p;
			if (!Prefixes.TryGetValue(writeOperationName, out p))
				return null;
			return p.Name;
		}

	}
}

