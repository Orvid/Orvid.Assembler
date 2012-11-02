using System;
using System.Reflection;
using System.Collections.Generic;
using Orvid.CodeDom;

namespace Orvid.Assembler.x86.IstructionGen
{
	public sealed class EnumRegistryEntryMember
	{
		public readonly string Name;
		public readonly string Value;

		public EnumRegistryEntryMember(string name, string value)
		{
			this.Name = name;
			this.Value = value;
		}
	}

	public sealed class EnumRegistryEntry
	{
		public readonly List<EnumRegistryEntryMember> Members = new List<EnumRegistryEntryMember>(8);
		public readonly string Name;
		public readonly int BaseTypeID;

		public EnumRegistryEntry(string name, int baseTypeID)
		{
			this.Name = name;
			this.BaseTypeID = baseTypeID;
		}

		public void Write(CodeNamespace n)
		{
			CodeTypeDeclaration decl = new CodeTypeDeclaration(Name);

			decl.TypeAttributes = TypeAttributes.Public;
			decl.Attributes = MemberAttributes.Public;
			decl.IsEnum = true;
			if (BaseTypeID != -1)
			{
				decl.BaseTypes.Add(FieldTypeRegistry.Fields[BaseTypeID].CodeType);
			}

			foreach (var m in Members)
			{
				decl.Members.Add(new CodeMemberField(decl.Name, m.Name));
			}

			n.Types.Add(decl);
		}
	}
	public static class EnumRegistry
	{
		private static List<EnumRegistryEntry> mEntries;
		public static List<EnumRegistryEntry> Entries { get { return mEntries; } }

		public static void WriteEnums(CodeNamespace n)
		{
			foreach (var e in mEntries)
			{
				e.Write(n);
			}
		}

		static EnumRegistry()
		{
			Initialize();
		}

		private static void Initialize()
		{
			mEntries = new List<EnumRegistryEntry>();
		}

		public static void Reset()
		{
			Initialize();
		}

		public static void RegisterEntry(EnumRegistryEntry entry)
		{
			mEntries.Add(entry);
		}

	}
}

