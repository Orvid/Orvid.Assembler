using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using Orvid.CodeDom;

namespace Orvid.Assembler.InstructionGen
{
	public sealed class Instruction
	{
		private Dictionary<InstructionArgSet, InstructionForm> mForms = new Dictionary<InstructionArgSet, InstructionForm>();
		public Dictionary<InstructionArgSet, InstructionForm> Forms
		{
			get { return mForms; }
		}

		private Dictionary<InstructionArgSet, InstructionForm> mExactForms = new Dictionary<InstructionArgSet, InstructionForm>();
		public Dictionary<InstructionArgSet, InstructionForm> ExactForms
		{
			get { return mExactForms; }
		}

		private List<InExactInstructionOverrideDescription> mInExactForms = new List<InExactInstructionOverrideDescription>();
		public List<InExactInstructionOverrideDescription> InExactForms
		{
			get { return mInExactForms; }
		}

		private void EvaluateOverrides()
		{
			foreach (var v in mExactForms)
			{
				for (int i = 0; i < FinalForms.Count; i++)
				{
					InstructionForm f = FinalForms[i];
					if (f.GetInstructionCaseString() == v.Value.GetInstructionCaseString())
					{
						FinalForms[i] = v.Value;
						break;
					}
				}
			}
			foreach (var v in mInExactForms)
			{
				List<InExactInstructionOverrideDescription> descs = new List<InExactInstructionOverrideDescription>(1);
				if (NeedsExpand(v))
				{
					descs = ExpandInExactOverride(v);
				}
				else
				{
					descs.Add(v);
				}
				foreach(var v2 in descs)
				{
					string valCaseStr = v2.ArgSetToOverride.GetInstructionCaseString();
					for (int i = 0; i < FinalForms.Count; i++)
					{
						InstructionForm f = FinalForms[i];
						if (f.GetInstructionCaseString() == valCaseStr)
						{
							f.Overrides.Add(v2);
							break;
						}
					}
				}
			}
		}

		private List<InExactInstructionOverrideDescription> ExpandInExactOverride(InExactInstructionOverrideDescription desc)
		{
			var ret = new List<InExactInstructionOverrideDescription>(4);
			for (int i = 0; i < 3; i++)
			{
				InstructionArg newForm = desc.NewForm[i];
				if (newForm.ArgType.ExpandsTo.Count > 0)
				{
					foreach (InstructionArgType exToTp in newForm.ArgType.ExpandsTo)
					{
						var o = desc.DeepCopy();
						var a = o.NewForm[i];
						o.ArgSetToOverride[i] = exToTp;
						a.ArgType = exToTp;
						o.NewForm[i] = a;
						ret.Add(o);
					}
				}
			}
			return ret;
		}

		private bool NeedsExpand(InExactInstructionOverrideDescription desc)
		{
			return 
				desc.NewForm.Arg1.ArgType.ExpandsTo.Count > 0 ||
				desc.NewForm.Arg2.ArgType.ExpandsTo.Count > 0 ||
				desc.NewForm.Arg3.ArgType.ExpandsTo.Count > 0;
		}

		public string OutDirectory { get; private set; }
		public string Name { get; private set; }
		public string FileName { get; private set; }
		public Instruction(string name)
		{
			this.Name = name;
			if (name.Contains("/"))
			{
				int lio = name.LastIndexOf("/");
				OutDirectory = name.Substring(0, lio);
				FileName = name.Substring(lio + 1);
			}
			else
			{
				OutDirectory = "";
				FileName = Name;
			}
		}

		public List<string> LinesOfDocumentation = new List<string>(4);

		public List<InstructionForm> FinalForms = new List<InstructionForm>(32);

		private void ExpandSingleForm(InstructionForm f, int argIdx)
		{
			foreach (InstructionArgType tp in f[argIdx].ArgType.ExpandsTo)
			{
				InstructionForm f2 = f.DeepCopy();
				InstructionArg a = f2[argIdx];
				a.ArgType = tp;
				f2[argIdx] = a;
				FinalForms.Add(f2);
			}
		}

		private void ExpandForms()
		{
			foreach (InstructionForm f in Forms.Values)
			{
				if (f.Arg1.ArgType.ExpandsTo.Count > 0)
				{
					ExpandSingleForm(f, 0);
				}
				else if (f.Arg2.ArgType.ExpandsTo.Count > 0)
				{
					ExpandSingleForm(f, 1);
				}
				else if (f.Arg3.ArgType.ExpandsTo.Count > 0)
				{
					ExpandSingleForm(f, 2);
				}
				else
				{
					FinalForms.Add(f);
				}
			}
		}

		private const int InitialFieldCollectionCapacity = 32;
		private Dictionary<FieldTypeNumPair, int> Fields = new Dictionary<FieldTypeNumPair, int>(InitialFieldCollectionCapacity);
		private List<CodeMemberField> FieldDeclarations = new List<CodeMemberField>(InitialFieldCollectionCapacity);
		private List<string> FieldNames = new List<string>(InitialFieldCollectionCapacity);

		private struct FieldTypeNumPair
		{
			public int TypeID;
			public short FieldNumber;
			public FieldTypeNumPair(int tpID, int fldNum)
			{
				TypeID = tpID;
				FieldNumber = (short)fldNum;
			}
			public override int GetHashCode()
			{
				return ((int)TypeID << 16) | (FieldNumber);
			}
		}
		
		public string GetFieldName(int tpID, int fldNum)
		{
			int foundNum;
			if (!Fields.TryGetValue(new FieldTypeNumPair(tpID, fldNum), out foundNum))
				throw new Exception("Unknown field!");
			return FieldNames[foundNum];
		}

        public void RequestField(int tp, int count)
		{
			for (int i = 0; i < count; i++)
			{
				FieldTypeNumPair key = new FieldTypeNumPair(tp, i);
				if (!Fields.ContainsKey(key))
				{
					string fieldPrefix = FieldTypeRegistry.Fields[tp].Prefix != "" ? FieldTypeRegistry.Fields[tp].Prefix : "local_";
					string fieldName = fieldPrefix + FieldDeclarations.Count.ToString();
					CodeMemberField fld = new CodeMemberField(FieldTypeRegistry.Fields[tp].Name, fieldName);
					fld.Attributes = MemberAttributes.Private;
					// A field's value is only ever assigned in
					// the constructor, and that only occurs on
					// the current thread, so we make this information
					// available to whatever language's compiler 
					// we're using. (readonly in C#)
					fld.Attributes |= MemberAttributes.Final;
					FieldDeclarations.Add(fld);
					FieldNames.Add(fieldName);
					Fields[key] = FieldDeclarations.Count - 1;
				}
			}
		}

		private struct SizelessArgSet
		{
			public SizelessType Arg1;
			public SizelessType Arg2;
			public SizelessType Arg3;
			public override int GetHashCode()
			{
				return (Arg1.ID << 16) | (Arg2.ID << 8) | (Arg3.ID);
			}
		}

		private Dictionary<InstructionArgSet, bool> FormsEmitted = new Dictionary<InstructionArgSet, bool>();
		private Dictionary<SizelessArgSet, bool> ConstructorsEmitted = new Dictionary<SizelessArgSet, bool>();

		public bool NeedsEmission(InstructionForm frm)
		{
			InstructionArgSet key = new InstructionArgSet()
			{
				Arg1 = InstructionArgTypeRegistry.GetType(frm.Arg1.ArgType.TrueName),
				Arg2 = InstructionArgTypeRegistry.GetType(frm.Arg2.ArgType.TrueName),
				Arg3 = InstructionArgTypeRegistry.GetType(frm.Arg3.ArgType.TrueName)
			};
			if (!FormsEmitted.ContainsKey(key))
			{
				FormsEmitted[key] = true;
				return true;
			}
			return false;
		}

		public bool NeedsSizeArgument(InstructionArgType arg1Tp, InstructionArgType arg2Tp, InstructionArgType arg3Tp, ref List<byte> sizesNeeded, out bool constructorNeeded, ref List<string> formsNeeded)
		{
			constructorNeeded = true;
			if (arg1Tp == InstructionArgType.None)
				return false;
			int cnt = 0;
			int argChecking = -1;
			SizelessArgSet argSet = new SizelessArgSet()
			{
				Arg1 = arg1Tp.TrueSizelessType,
				Arg2 = arg2Tp.TrueSizelessType,
				Arg3 = arg3Tp.TrueSizelessType
			};
			if (ConstructorsEmitted.ContainsKey(argSet))
			{
				constructorNeeded = false;
				return false;
			}
			else
			{
				ConstructorsEmitted[argSet] = true;
				constructorNeeded = true;
			}
			InstructionForm curForm = null;
			foreach (InstructionForm f in FinalForms)
			{
				if (curForm == null && f.Arg1.ArgType == arg1Tp && f.Arg2.ArgType == arg2Tp && f.Arg3.ArgType == arg3Tp)
				{
					curForm = f;
				}
				else if (f.Arg1.ArgType.SizelessType == arg1Tp.SizelessType && f.Arg2.ArgType.SizelessType == arg2Tp.SizelessType && f.Arg3.ArgType.SizelessType == arg3Tp.SizelessType)
				{
					if (argChecking < 0)
					{
						if (f.Arg1.ArgType.Size != arg1Tp.Size)
						{
							argChecking = 0;
						}
						else if (f.Arg2.ArgType.Size != arg2Tp.Size)
						{
							argChecking = 1;
						}
						else if (f.Arg3.ArgType.Size != arg3Tp.Size)
						{
							argChecking = 2;
						}
						else
						{
							throw new Exception("Duplicate size!");
						}
					}
					cnt++;
					sizesNeeded.Add((byte)f[argChecking].ArgType.Size);
					formsNeeded.Add(f.GetInstructionCaseString());
				}
			}
			if (curForm == null)
				throw new Exception("Unable to locate this form!");
			if (argChecking >= 0)
			{
				cnt++;
				sizesNeeded.Add((byte)curForm[argChecking].ArgType.Size);
				formsNeeded.Add(curForm.GetInstructionCaseString());
			}
			return cnt > 1;
		}

		public string GetNamespaceExtension()
		{
			if (OutDirectory != "")
				return OutDirectory;
			return "";
		}

		public void Write(string outDir, CodeNamespace nmspc)
		{
			ExpandForms();
			EvaluateOverrides();
			bool lonely = FinalForms.Count == 1;
			CodeTypeDeclaration td = new CodeTypeDeclaration(FileName);
			td.Attributes = MemberAttributes.Final;
			td.Attributes |= MemberAttributes.Public;
			td.IsClass = true;
			td.BaseTypes.Add(StaticTypeReferences.Instruction);
			td.Documentation.Add(new CodeDocumentationSummaryNode(LinesOfDocumentation));

			foreach (InstructionForm f in FinalForms)
			{
				f.RequestFields();
			}
			foreach (var v in FieldDeclarations)
			{
				td.Members.Add(v);
			}

			foreach (InstructionForm f in FinalForms)
			{
				f.WriteConstructors(td, FileName);
			}

			CodeMemberMethod mth = new CodeMemberMethod();
			mth.Documentation.AddRange(
				new CodeDocumentationSummaryNode("Write this instruction to a stream."),
				new CodeDocumentationParameterNode(StaticTypeReferences.Emit_StreamArgName, "The stream to write to.")
			);
			mth.Name = "Emit";
			mth.Parameters.Add(new CodeParameterDeclarationExpression(StaticTypeReferences.Stream, StaticTypeReferences.Emit_StreamArgName));
			mth.Attributes = MemberAttributes.Public;
			mth.Attributes |= MemberAttributes.Override;
			mth.Attributes |= MemberAttributes.Sealed;
			mth.ReturnType = StaticTypeReferences.Void;
			CodeSwitchStatement instructionFormSwitch = null;
			if (!lonely)
			{
				instructionFormSwitch = new CodeSwitchStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "InstructionForm"));
				mth.Statements.Add(instructionFormSwitch);
			}
			foreach (InstructionForm f in FinalForms)
			{
				if (NeedsEmission(f))
				{
					CodeScopeStatement con = new CodeScopeStatement();
					if (lonely)
					{
						mth.Statements.Add(con);
					}
					else
					{
						CodeCaseStatement cas = new CodeCaseStatement(
							new CodeFieldReferenceExpression(StaticTypeReferences.InstructionFormExpression, f.GetInstructionCaseString())
						);
						cas.Statements.Add(con);
						cas.Statements.Add(new CodeBreakStatement());
						instructionFormSwitch.Cases.Add(cas);
					}
					f.WriteEmit(con, false);
				}
			}
			if (!lonely)
			{
				CodeDefaultCaseStatement defStat = new CodeDefaultCaseStatement();
				defStat.Statements.Add(
					new CodeThrowExceptionStatement(
						new CodeObjectCreateExpression(
							StaticTypeReferences.Exception,
							new CodePrimitiveExpression("Unknown Instruction Form!")
						)
					)
				);
				instructionFormSwitch.Cases.Add(defStat);
			}
			td.Members.Add(mth);
			instructionFormSwitch = null;

			FormsEmitted = new Dictionary<InstructionArgSet, bool>();

			mth = new CodeMemberMethod();
			bool hasSyntax = StaticTypeReferences.AssemblySyntaxClassName != null;
			if (hasSyntax)
			{
				mth.Documentation.AddRange(
					new CodeDocumentationSummaryNode(
						"Get a string representation of this instruction in the",
						"specified assembly syntax."
					),
					new CodeDocumentationParameterNode(StaticTypeReferences.ToString_SyntaxArgName, "The syntax to get the string representation in.")
				);
			}
			else
			{
				mth.Documentation.Add(new CodeDocumentationSummaryNode("Get a string representation of this instruction."));
			}
			mth.Name = "ToString";
			mth.Attributes = MemberAttributes.Public;
			mth.Attributes |= MemberAttributes.Override;
			mth.Attributes |= MemberAttributes.Sealed;
			mth.ReturnType = StaticTypeReferences.String;
			if (!hasSyntax)
			{
				if (!lonely)
				{
					instructionFormSwitch = new CodeSwitchStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "InstructionForm"));
					mth.Statements.Add(instructionFormSwitch);
				}
				foreach (InstructionForm f in FinalForms)
				{
					if (NeedsEmission(f))
					{
						CodeScopeStatement con = new CodeScopeStatement();
						if (lonely)
						{
							mth.Statements.Add(con);
						}
						else
						{
							CodeCaseStatement cas = new CodeCaseStatement(
								new CodeFieldReferenceExpression(
									StaticTypeReferences.InstructionFormExpression, 
									f.GetInstructionCaseString()
								)
							);
							cas.Statements.Add(con);
							instructionFormSwitch.Cases.Add(cas);
						}
						f.WriteToString(con);
					}
				}
				if (!lonely)
				{
					CodeDefaultCaseStatement defStat = new CodeDefaultCaseStatement();
					defStat.Statements.Add(
						new CodeThrowExceptionStatement(
							new CodeObjectCreateExpression(
								StaticTypeReferences.Exception,
								new CodePrimitiveExpression("Unknown Instruction Form!")
							)
						)
					);
					instructionFormSwitch.Cases.Add(defStat);
				}
			}
			else
			{
				mth.Parameters.Add(new CodeParameterDeclarationExpression(StaticTypeReferences.AssemblySyntax, StaticTypeReferences.ToString_SyntaxArgName));
				CodeSwitchStatement sw = new CodeSwitchStatement(StaticTypeReferences.ToString_SyntaxArg);
				mth.Statements.Add(sw);

				CodeCaseStatement cs = new CodeCaseStatement(
					new CodeFieldReferenceExpression(
						StaticTypeReferences.AssemblySyntaxExpression,
						"NASM"
					)
				);
				if (!lonely)
				{
					instructionFormSwitch = new CodeSwitchStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "InstructionForm"));
					cs.Statements.Add(instructionFormSwitch);
				}
				foreach (InstructionForm f in FinalForms)
				{
					if (NeedsEmission(f))
					{
						CodeScopeStatement con = new CodeScopeStatement();
						if (lonely)
						{
							cs.Statements.Add(con);
						}
						else
						{
							CodeCaseStatement cas = new CodeCaseStatement(
								new CodeFieldReferenceExpression(StaticTypeReferences.InstructionFormExpression, f.GetInstructionCaseString())
							);
							cas.Statements.Add(con);
							instructionFormSwitch.Cases.Add(cas);
						}
						f.WriteToString(con);
					}
				}
				if (!lonely)
				{
					CodeDefaultCaseStatement defStat = new CodeDefaultCaseStatement();
					defStat.Statements.Add(
						new CodeThrowExceptionStatement(
							new CodeObjectCreateExpression(
								StaticTypeReferences.Exception,
								new CodePrimitiveExpression("Unknown Instruction Form!")
							)
						)
					);
					instructionFormSwitch.Cases.Add(defStat);
				}
				sw.Cases.Add(cs);

				cs = new CodeCaseStatement(
					new CodeFieldReferenceExpression(
						StaticTypeReferences.AssemblySyntaxExpression,
						"GAS"
					)
				);
				sw.Cases.Add(cs);
				CodeDefaultCaseStatement def = new CodeDefaultCaseStatement();
				def.Statements.Add(
					new CodeThrowExceptionStatement(
						new CodeObjectCreateExpression(
							StaticTypeReferences.Exception,
							new CodePrimitiveExpression("Unknown Instruction Form!")
						)
					)
				);
				sw.Cases.Add(def);
			}

			td.Members.Add(mth);


			nmspc.Types.Add(td);
		}
	}
}

