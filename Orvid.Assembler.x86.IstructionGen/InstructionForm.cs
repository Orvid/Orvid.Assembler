using System;
using System.Collections.Generic;
using System.IO;
using Orvid.CodeDom;

namespace Orvid.Assembler.x86.IstructionGen
{
	public class InstructionForm
	{
		public InstructionArg Arg1;
		public InstructionArg Arg2;
		public InstructionArg Arg3;

		public void Init()
		{
			Arg1.ArgType = InstructionArgType.None;
			Arg2.ArgType = InstructionArgType.None;
			Arg3.ArgType = InstructionArgType.None;
		}

		public string Mnemonic;
		public string DefaultSegment;
		public Instruction ParentInstruction;
		public ArgumentExcludeCondition ExcludeCondition;
		public List<WriteOperationArgument> WriteOperationArgs = new List<WriteOperationArgument>();
		public int NextWriteOperationArgIdx = 0;

		public List<WriteOperation> WriteOperations = new List<WriteOperation>();

		public InstructionForm(Instruction parent)
		{
			ParentInstruction = parent;
		}

		public InstructionForm DeepCopy()
		{
			// Because of the way this is designed (awesomely in my opinion),
			// the only thing that has to be deep copied in the WriteOperation list.
			InstructionForm i = new InstructionForm(ParentInstruction);
			i.Arg1 = Arg1;
			i.Arg2 = Arg2;
			i.Arg3 = Arg3;
			i.Mnemonic = Mnemonic;
			i.DefaultSegment = DefaultSegment;
			i.ExcludeCondition = ExcludeCondition != null ? ExcludeCondition.DeepCopy() : null;
			foreach (WriteOperationArgument arg in WriteOperationArgs)
			{
				i.WriteOperationArgs.Add(arg.DeepCopy());
			}
			i.WriteOperations.AddRange(WriteOperations);
			return i;
		}

		public void RequestFields()
		{
			int[] RequiredFields;
			if (RequiredFieldsForArgCache[1] != null)
			{
				RequiredFields = RequiredFieldsForArgCache[1];
			}
			else if (RequiredFieldsForArgCache[0] != null)
			{
				RequiredFields = (int[])RequiredFieldsForArgCache[0].Clone();
				Arg2.ArgType.RequestFields(RequiredFields);
				RequiredFieldsForArgCache[1] = (int[])RequiredFields.Clone();
			}
			else
			{
				RequiredFields = new int[FieldTypeRegistry.MaxFieldID + 1];
				Arg1.ArgType.RequestFields(RequiredFields);
				RequiredFieldsForArgCache[0] = (int[])RequiredFields.Clone();
				Arg2.ArgType.RequestFields(RequiredFields);
				RequiredFieldsForArgCache[1] = (int[])RequiredFields.Clone();
			}
			Arg3.ArgType.RequestFields(RequiredFields);
			if (Arg1.ArgType.RequiresSegment || Arg2.ArgType.RequiresSegment || Arg3.ArgType.RequiresSegment)
			{
				RequiredFields[FieldTypeRegistry.Segment.ID]++;
			}
			for (int i = 0; i < RequiredFields.Length; i++)
			{
				if (RequiredFields[i] > 0)
				{
					ParentInstruction.RequestField(i, RequiredFields[i]);
				}
			}
		}

		private int[][] RequiredFieldsForArgCache = new int[3][];
		
		private struct CachedArgNameKey
		{
			public int Type;
			public short VarNum;
			public byte ArgBaseIndex;
			public override int GetHashCode()
			{
				return (Type << 24) | ((int)ArgBaseIndex << 16) | (VarNum);
			}
		}
		private Dictionary<CachedArgNameKey, string> CachedArgNames = new Dictionary<CachedArgNameKey, string>();

		public string GetArgName(int fldTp, int varNum, int argBaseIndex)
		{
			CachedArgNameKey key = new CachedArgNameKey()
			{
				Type = fldTp,
				VarNum = (short)varNum,
				ArgBaseIndex = (byte)argBaseIndex
			};
			string retString;
			if (CachedArgNames.TryGetValue(key, out retString))
				return retString;
			int baseVarIdx = 0;
			if (argBaseIndex > 0)
			{
				int[] RequiredFields;
				switch(argBaseIndex)
				{
					case 1:
						RequiredFields = RequiredFieldsForArgCache[0];
						if (RequiredFields == null)
						{
							RequiredFields = new int[FieldTypeRegistry.MaxFieldID + 1];
							Arg1.ArgType.RequestFields(RequiredFields);
							RequiredFieldsForArgCache[0] = RequiredFields;
						}
						break;
					case 2:
						RequiredFields = RequiredFieldsForArgCache[1];
						if (RequiredFields == null)
						{
							if (RequiredFieldsForArgCache[0] == null)
							{
								RequiredFields = new int[FieldTypeRegistry.MaxFieldID + 1];
								Arg1.ArgType.RequestFields(RequiredFields);
								RequiredFieldsForArgCache[0] = (int[])RequiredFields.Clone();
							}
							else
							{
								RequiredFields = (int[])RequiredFieldsForArgCache[0].Clone();
							}
							Arg2.ArgType.RequestFields(RequiredFields);
							RequiredFieldsForArgCache[1] = RequiredFields;
						}
						break;
					case 3:
						RequiredFields = RequiredFieldsForArgCache[2];
						if (RequiredFields == null)
						{
							RequiredFields = RequiredFieldsForArgCache[1];
							if (RequiredFields == null)
							{
								if (RequiredFieldsForArgCache[0] == null)
								{
									RequiredFields = new int[FieldTypeRegistry.MaxFieldID + 1];
									Arg1.ArgType.RequestFields(RequiredFields);
									RequiredFieldsForArgCache[0] = (int[])RequiredFields.Clone();
								}
								else
								{
									RequiredFields = (int[])RequiredFieldsForArgCache[0].Clone();
								}
								Arg2.ArgType.RequestFields(RequiredFields);
								RequiredFieldsForArgCache[1] = RequiredFields;
							}
							else
							{
								RequiredFields = (int[])RequiredFieldsForArgCache[1].Clone();
							}
							Arg3.ArgType.RequestFields(RequiredFields);
							RequiredFieldsForArgCache[2] = RequiredFields;
						}
						break;
					default:
						throw new Exception("Invalid arg base index!");
				}
				if ((int)fldTp > RequiredFields.Length)
					throw new Exception("An error occured while locating the required field!");
				baseVarIdx = RequiredFields[(int)fldTp];
			}
			string fldName = ParentInstruction.GetFieldName(fldTp, baseVarIdx + varNum - 1);
			CachedArgNames[key] = fldName;
			return fldName;
		}

		public void WriteConstructors(CodeTypeDeclaration tdecl, string className)
		{
			if (ParentInstruction.LinesOfDocumentation.Count == 0)
			{
				ParentInstruction.LinesOfDocumentation.Add("Creates a new instance of the <see cref=\"" + ParentInstruction.FileName + "\" /> class.");
			}
			if (Arg1.ArgType == InstructionArgType.None)
			{
				CodeConstructor c = new CodeConstructor();
				c.Attributes = MemberAttributes.Public;

				c.Documentation.AddRange(
					new CodeDocumentationSummaryNode(ParentInstruction.LinesOfDocumentation),
					new CodeDocumentationParameterNode("parentAssembler", "The assembler to add this instruction to.")
				);
				c.Parameters.Add(new CodeParameterDeclarationExpression(StaticTypeReferences.Assembler, "parentAssembler"));
				c.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("parentAssembler"));
				tdecl.Members.Add(c);
			}
			else
			{
				List<byte> sizesNeeded = new List<byte>(4);
				List<string> formsNeeded = new List<string>(4);
				bool constructorNeeded;
				bool needsSizeArg = ParentInstruction.NeedsSizeArgument(Arg1.ArgType, Arg2.ArgType, Arg3.ArgType, ref sizesNeeded, out constructorNeeded, ref formsNeeded);
				if (constructorNeeded)
				{
					int segArgIdx;
					bool needsSegArg = NeedsSegment(out segArgIdx);
					CodeConstructor c = new CodeConstructor();
					c.Attributes = MemberAttributes.Public;
					c.Parameters.Add(new CodeParameterDeclarationExpression(StaticTypeReferences.Assembler, "parentAssembler"));
					c.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("parentAssembler"));
					Arg1.ArgType.RequestParameters(Arg1, needsSizeArg, c.Parameters);
					Arg2.ArgType.RequestParameters(Arg2, needsSizeArg, c.Parameters);
					Arg3.ArgType.RequestParameters(Arg3, needsSizeArg, c.Parameters);
					if (needsSizeArg)
						c.Parameters.Add(new CodeParameterDeclarationExpression(StaticTypeReferences.Byte, "size"));
					if (needsSegArg)
					{
						CodeParameterDeclarationExpression cpd = new CodeParameterDeclarationExpression(StaticTypeReferences.Segment, "segment");
						cpd.DefaultValueExpression = new CodeFieldReferenceExpression(StaticTypeReferences.SegmentExpression, DefaultSegment.ToString());
						c.Parameters.Add(cpd);
					}
						
					CodeDocumentationNodeCollection docs = new CodeDocumentationNodeCollection();
					docs.Add(new CodeDocumentationParameterNode("parentAssembler", "The assembler to add this instruction to."));
					Arg1.ArgType.AddDocumentationLines(Arg1.Name, docs);
					Arg2.ArgType.AddDocumentationLines(Arg2.Name, docs);
					Arg3.ArgType.AddDocumentationLines(Arg3.Name, docs);
					if (needsSizeArg)
						docs.Add(new CodeDocumentationParameterNode("size", "The size of the operands for this instruction."));
					if (needsSegArg)
						docs.Add(new CodeDocumentationParameterNode("segment", "The segment to use for memory access within this instruction."));
					c.Documentation.Add(new CodeDocumentationSummaryNode(ParentInstruction.LinesOfDocumentation));
					c.Documentation.AddRange(docs);
					WriteConstructorBodyInstructionFormDetermination(c, sizesNeeded, formsNeeded);
					Arg1.ArgType.WriteConstructorBodyArgProcessing(this, c, 0);
					Arg2.ArgType.WriteConstructorBodyArgProcessing(this, c, 1);
					Arg3.ArgType.WriteConstructorBodyArgProcessing(this, c, 2);
					if (needsSegArg)
					{
						c.Statements.Add(
							new CodeAssignStatement(
								new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), GetArgName(FieldTypeRegistry.Segment.ID, 1, segArgIdx)),
								new CodeArgumentReferenceExpression("segment")
							)
						);
					}

					tdecl.Members.Add(c);
				}
			}
		}

		private void WriteConstructorBodyInstructionFormDetermination(CodeConstructor c, List<byte> sizesNeeded, List<string> formsNeeded)
		{
			if (sizesNeeded.Count > 1)
			{
				CodeSwitchStatement s = new CodeSwitchStatement(new CodeArgumentReferenceExpression("size"));

				// If your willing to accept the sizes being out of order, then this isn't
				// needed, but I don't like them out of order so it is.
				List<KeyValuePair<byte, string>> sizeFormPairs = new List<KeyValuePair<byte, string>>();
				for (int i = 0; i < sizesNeeded.Count; i++)
				{
					sizeFormPairs.Add(new KeyValuePair<byte, string>(sizesNeeded[i], formsNeeded[i]));
				}
				sizeFormPairs.Sort(
					delegate(KeyValuePair<byte, string> x, KeyValuePair<byte, string> y)
					{
						return x.Key.CompareTo(y.Key);
					}
				);
				sizesNeeded.Sort();
				for (int i = 0; i < sizeFormPairs.Count; i++)
				{
					// The cast is needed so it doesn't write the value out as hex.
					CodeCaseStatement cas = new CodeCaseStatement(new CodePrimitiveExpression((int)sizeFormPairs[i].Key));
					cas.Statements.Add(
						new CodeAssignStatement(
							new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "InstructionForm"),
							new CodeFieldReferenceExpression(StaticTypeReferences.InstructionFormExpression, sizeFormPairs[i].Value)
						)
					);
					cas.Statements.Add(new CodeBreakStatement());
					s.Cases.Add(cas);
					InstructionFormEnumRegistry.RequestForm(sizeFormPairs[i].Value);
				}

				string exSzs = "";
				if (sizesNeeded.Count == 2)
				{
					exSzs = "either " + sizesNeeded[0].ToString() + " or " + sizesNeeded[1].ToString();
				}
				else
				{
					exSzs = sizesNeeded[0].ToString();
					int szndc = sizesNeeded.Count - 1;
					for (int i = 1; i < szndc; i++)
					{
						exSzs += ", " + sizesNeeded[i].ToString();
					}
					exSzs += " or " + sizesNeeded[szndc];
				}
				CodeDefaultCaseStatement defStat = new CodeDefaultCaseStatement();
				defStat.Statements.Add(
					new CodeThrowExceptionStatement(
						new CodeObjectCreateExpression(
							StaticTypeReferences.ArgumentOutOfRangeException, 
							new CodePrimitiveExpression("size"),
							new CodePrimitiveExpression("Invalid size! Expected " + exSzs + "!")
						)
					)
				);
				s.Cases.Add(defStat);

				c.Statements.Add(s);
			}
			else
			{
				c.Statements.Add(
					new CodeAssignStatement(
						new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "InstructionForm"),
						new CodeFieldReferenceExpression(StaticTypeReferences.InstructionFormExpression, GetInstructionCaseString())
					)
				);
				InstructionFormEnumRegistry.RequestForm(GetInstructionCaseString());
			}
		}

		private int? CachedSegArgIdx = null;
		public bool NeedsSegment(out int segArgIdx)
		{
			if (CachedSegArgIdx != null)
			{
				segArgIdx = CachedSegArgIdx.Value;
				return segArgIdx != -1;
			}
			if (Arg1.ArgType.RequiresSegment)
			{
				segArgIdx = 0;
				CachedSegArgIdx = 0;
				return true;
			}
			else if (Arg2.ArgType.RequiresSegment)
			{
				segArgIdx = 1;
				CachedSegArgIdx = 1;
				return true;
			}
			else if (Arg3.ArgType.RequiresSegment)
			{
				segArgIdx = 2;
				CachedSegArgIdx = 2;
				return true;
			}
			else
			{
				segArgIdx = -1;
				CachedSegArgIdx = -1;
				return false;
			}
		}

		public List<InExactInstructionOverrideDescription> Overrides = new List<InExactInstructionOverrideDescription>();

		public void WriteEmit(CodeScopeStatement con, bool ignoreOverrides/* = false*/)
		{
			if (!ignoreOverrides && Overrides.Count > 0)
			{
				Overrides[0].WriteConditionalEmit(con, this);
			}
			else
			{
				int segArgIdx;
				if (NeedsSegment(out segArgIdx))
				{
					con.Statements.Add(
						new CodeMethodInvokeExpression(
							StaticTypeReferences.Emit_Stream_WriteSegmentOverride,
							new CodeFieldReferenceExpression(
									new CodeThisReferenceExpression(),
									GetArgName(FieldTypeRegistry.Segment.ID, 1, segArgIdx)
							),
							new CodeFieldReferenceExpression(StaticTypeReferences.SegmentExpression, DefaultSegment)
						)
					);
				}
				for (int i = 0; i < WriteOperations.Count; i++)
				{
					WriteOperations[i].Write(con, this);
				}
			}
		}

		private string cachedInstructionCaseString = null;
		public string GetInstructionCaseString()
		{
			if (cachedInstructionCaseString != null)
				return cachedInstructionCaseString;
			InstructionArgType a1 = InstructionArgTypeRegistry.GetType(Arg1.ArgType.TrueName);
			InstructionArgType a2 = InstructionArgTypeRegistry.GetType(Arg2.ArgType.TrueName);
			InstructionArgType a3 = InstructionArgTypeRegistry.GetType(Arg3.ArgType.TrueName);
			string retStr;
			if (a3 != InstructionArgType.None)
			{
				retStr = a1.TrueName + "_" + a2.TrueName + "_" + a3.TrueName;
			}
			else if (a2 != InstructionArgType.None)
			{
				retStr = a1.TrueName + "_" + a2.TrueName;
			}
			else
			{
				retStr = a1.TrueName;
			}
			cachedInstructionCaseString = retStr;
			return retStr;
		}

		public void WriteToString(CodeScopeStatement con)
		{
			if (Arg1.ArgType == InstructionArgType.None)
			{
				con.Statements.Add(
					new CodeMethodReturnStatement(
						new CodePrimitiveExpression(Mnemonic)
					)
				);
			}
			else if (Arg2.ArgType == InstructionArgType.None)
			{
				if (ExcludeCondition != null)
				{
					CodeConditionStatement f = new CodeConditionStatement(ExcludeCondition.GetConditionExpression(this));
					f.TrueStatements.Add(
						new CodeMethodReturnStatement(
							new CodeBinaryOperatorExpression(
								new CodePrimitiveExpression(Mnemonic + " "),
								CodeBinaryOperatorType.StringConcat,
								Arg1.ArgType.GetToStringForArg(this, 0)
							)
						)
					);
					f.FalseStatements.Add(
						new CodeMethodReturnStatement(
							new CodePrimitiveExpression(Mnemonic)
						)
					);
					con.Statements.Add(f);
				}
				else
				{
					
					con.Statements.Add(
						new CodeMethodReturnStatement(
							new CodeBinaryOperatorExpression(
								new CodePrimitiveExpression(Mnemonic + " "),
								CodeBinaryOperatorType.StringConcat,
								Arg1.ArgType.GetToStringForArg(this, 0)
							)
						)
					);
				}
			}
			else if (Arg3.ArgType == InstructionArgType.None)
			{
				con.Statements.Add(
					new CodeMethodReturnStatement(
						new CodeBinaryOperatorExpression(
							new CodePrimitiveExpression(Mnemonic + " "),
							CodeBinaryOperatorType.StringConcat,
							new CodeBinaryOperatorExpression(
								Arg1.ArgType.GetToStringForArg(this, 0),
								CodeBinaryOperatorType.StringConcat,
								new CodeBinaryOperatorExpression(
									new CodePrimitiveExpression(", "),
									CodeBinaryOperatorType.StringConcat,
									Arg2.ArgType.GetToStringForArg(this, 1)
								)
							)
						)
					)
				);
			}
			else
			{
				con.Statements.Add(
					new CodeMethodReturnStatement(
						new CodeBinaryOperatorExpression(
							new CodePrimitiveExpression(Mnemonic + " "),
							CodeBinaryOperatorType.StringConcat,
							new CodeBinaryOperatorExpression(
								Arg1.ArgType.GetToStringForArg(this, 0),
								CodeBinaryOperatorType.StringConcat,
								new CodeBinaryOperatorExpression(
									new CodeBinaryOperatorExpression(
										new CodePrimitiveExpression(", "),
										CodeBinaryOperatorType.StringConcat,
										Arg2.ArgType.GetToStringForArg(this, 1)
									),
									CodeBinaryOperatorType.StringConcat,
									new CodeBinaryOperatorExpression(
										new CodePrimitiveExpression(", "),
										CodeBinaryOperatorType.StringConcat,
										Arg3.ArgType.GetToStringForArg(this, 2)
									)
								)
							)
						)
					)
				);
			}
		}

		private CodeExpression GetToStringForArg(int argIdx)
		{
			InstructionArg arg = this[argIdx];
			int padSize = 0;
			switch (arg.ArgType.ID)
			{
				case InstructionArgType.Imm8_ID:
					padSize = 2;
					goto ImmCommon;
				case InstructionArgType.Imm16_ID:
					padSize = 4;
					goto ImmCommon;
				case InstructionArgType.Imm32_ID:
					padSize = 8;
					goto ImmCommon;
				default:
					goto NonImm;
			}
		ImmCommon:
			if (arg.NumberFormat == ImmNumberFormat.Decimal)
			{
				return new CodeMethodInvokeExpression(
					new CodeFieldReferenceExpression(
						new CodeThisReferenceExpression(),
						GetArgName(FieldTypeRegistry.UInt.ID, 1, argIdx)
					),
					"ToString"
				);
			}
			else
			{
				return Generator.LanguageProvider.GetPaddedHexToString(
					new CodeFieldReferenceExpression(
						new CodeThisReferenceExpression(),
						GetArgName(FieldTypeRegistry.UInt.ID, 1, argIdx)
					),
					padSize
				);
			}
		NonImm:
			return arg.ArgType.ReadOperation.GetExpression(this, argIdx, null);
		}

		public InstructionArg this[int idx]
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
			if (Arg3.ArgType != InstructionArgType.None)
			{
				return "Arg1: " + Arg1.ToString() + ", Arg2: " + Arg2.ToString() + ", Arg3: " + Arg3.ToString();
			}
			else if (Arg2.ArgType != InstructionArgType.None)
			{
				return "Arg1: " + Arg1.ToString() + ", Arg2: " + Arg2.ToString();
			}
			else if (Arg1.ArgType != InstructionArgType.None)
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

