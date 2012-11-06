using System;
using System.Reflection;
using System.Collections.Generic;
using Orvid.CodeDom;

namespace Orvid.Assembler.x86.IstructionGen
{
	public static class InstructionFormEnumRegistry
	{
		private static Dictionary<string, bool> RegisteredForms;

		static InstructionFormEnumRegistry()
		{
			Initialize();
		}

		private static void Initialize()
		{
			RegisteredForms = new Dictionary<string, bool>();
		}

		public static void Reset()
		{
			Initialize();
		}

		public static void RequestForm(string form)
		{
			if (!RegisteredForms.ContainsKey(form))
				RegisteredForms[form] = true;
		}

		public static void WriteFormEnum(CodeNamespace n)
		{
			CodeTypeDeclaration decl = new CodeTypeDeclaration(StaticTypeReferences.InstructionFormClassName);
			decl.Attributes = MemberAttributes.Assembly;
			decl.IsEnum = true;
			CodeTypeReference enumType = null;
			if (RegisteredForms.Count < ushort.MaxValue)
			{
				if (RegisteredForms.Count < byte.MaxValue)
				{
					enumType = StaticTypeReferences.Byte;
				}
				else
				{
					enumType = StaticTypeReferences.UShort;
				}
			}
			else
			{
				throw new Exception("Yikes that IS a lot of forms!");
			}
			decl.BaseTypes.Add(enumType);

			foreach (string f in RegisteredForms.Keys)
			{
				decl.Members.Add(new CodeMemberField(enumType, f));
			}

			n.Types.Add(decl);
		}

	}
}

