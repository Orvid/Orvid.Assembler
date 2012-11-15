using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Orvid.Assembler.InstructionGen
{
	public static class DocAliasRegistry
	{
		private struct DocAlias
		{
			public readonly string Name;
			public readonly string InitialValue;
			public readonly string ExpandedValue;

			public DocAlias(string name, string initValue, string expValue)
			{
				this.Name = name;
				this.InitialValue = initValue;
				this.ExpandedValue = expValue;
			}
		}

		public const string DocAlias_Imm_Value = "The immutable value.";
		public const string DocAlias_Dis_Value = "The displacement from the current location.";
		private static Dictionary<string, DocAlias> DocAliases;

		static DocAliasRegistry()
		{
			Initialize();
		}

		private static void Initialize()
		{
			DocAliases = new Dictionary<string, DocAlias>()
			{
				{ "Imm", new DocAlias("Imm", DocAlias_Imm_Value, DocAlias_Imm_Value) },
				{ "Dis", new DocAlias("Dis", DocAlias_Dis_Value, DocAlias_Dis_Value) }
			};
		}

		public static void Reset()
		{
			Initialize();
		}

		private static readonly Regex DocAliasRegex = new Regex(@"\$\{[a-zA-Z][a-zA-Z0-9_]*\}", RegexOptions.Compiled);

		public static void RegisterDocAlias(string name, string value)
		{
			if (DocAliases.ContainsKey(name))
				throw new Exception("Duplicate doc alias!");
			string initValue = value;
			string expandedValue = value;
			Match m;
			while ((m = DocAliasRegex.Match(expandedValue)).Success)
			{
				string destDocName = m.Value.Substring(2, m.Length - 3);
				DocAlias destDoc;
				if (!DocAliases.TryGetValue(destDocName, out destDoc))
					throw new Exception("Unknown doc alias '" + destDocName + "'!");
				expandedValue = expandedValue.Replace(m.Value, destDoc.ExpandedValue);
			}
			DocAliases[name] = new DocAlias(name, initValue, expandedValue);
		}

		public static string GetDocAliasValue(string name)
		{
			DocAlias doc;
			if (!DocAliases.TryGetValue(name, out doc))
				throw new Exception("Unknown doc alias '" + name + "'!");
			return doc.ExpandedValue;
		}

		public static string ExpandDocAliasValue(string value, string argName)
		{
			return value
				.Replace("{ArgName}", StringCaser.Transform(argName, StringCase.PascalCase))
				.Replace("{argName}", StringCaser.Transform(argName, StringCase.camelCase))
				.Replace("{argname}", StringCaser.Transform(argName, StringCase.lowercase))
				.Replace("{ARGNAME}", StringCaser.Transform(argName, StringCase.UPPERCASE))
			;
		}

	}
}

