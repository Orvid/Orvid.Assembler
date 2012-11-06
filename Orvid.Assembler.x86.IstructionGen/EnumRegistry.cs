using System;
using System.Reflection;
using System.Collections.Generic;
using Orvid.CodeDom;

namespace Orvid.Assembler.x86.IstructionGen
{
	public sealed class EnumRegistryEntryMember
	{
		/// <summary>
		/// This is null if there are no docs.
		/// </summary>
		public readonly List<string> Documentation;
		public readonly string Name;
		public readonly string Value;

		public EnumRegistryEntryMember(string name, string value, List<string> docs)
		{
			this.Name = name;
			this.Value = value;
			this.Documentation = docs;
		}
	}

	public sealed class EnumRegistryEntry
	{
		/// <summary>
		/// This is null if there are no docs.
		/// </summary>
		public readonly List<string> Documentation;
		public readonly List<EnumRegistryEntryMember> Members = new List<EnumRegistryEntryMember>(8);
		public readonly string Name;
		public readonly int BaseTypeID;

		public EnumRegistryEntry(string name, int baseTypeID, List<string> docs)
		{
			this.Name = name;
			this.BaseTypeID = baseTypeID;
			this.Documentation = docs;
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
			if (Documentation != null)
			{
				decl.Documentation.Add(new CodeDocumentationSummaryNode(Documentation));
			}

			foreach (var m in Members)
			{
				CodeMemberField fld = new CodeMemberField(decl.Name, m.Name);

				if (m.Documentation != null)
					fld.Documentation.Add(new CodeDocumentationSummaryNode(m.Documentation));

				decl.Members.Add(fld);
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

