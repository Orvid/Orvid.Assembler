using System;
using System.Collections.Generic;
using Orvid.CodeDom;

namespace Orvid.Assembler.x86.IstructionGen
{
	public sealed class InExactInstructionOverrideDescription
	{
		private sealed class OverrideCondition
		{
			public enum ConditionType
			{
				Fits,
				Compare_Reg,
				Compare_Imm,
				Compare_Segment,
			}
			public ConditionType Condition;
			public int ArgIndexToCheck;
			public uint ImmValueToCompareTo;
			public string RegisterToCompareTo;

			public OverrideCondition(List<Token> toks)
			{
				Token tok = toks[0];
				if (tok.Type != TokenType.Identifier)
					throw new Exception("Expected an identifier representing the type of condition!");

				if (tok.Value == "fits")
				{
					Condition = ConditionType.Fits;

					if (toks[1].Type != TokenType.LSqBracket)
						throw new Exception("Expected the arg to check if fits!");
					if (toks[3].Type != TokenType.RSqBracket)
						throw new Exception("Expected the close of the arg to check if fits!");
					tok = toks[2];
					if (!tok.Value.StartsWith("arg"))
						throw new Exception("Expected arg declaration!");
					ArgIndexToCheck = Utils.SingleDigitParse(tok.Value[3]) - 1;
				}
				else if (tok.Value == "comp")
				{
					if (toks[1].Type != TokenType.LSqBracket)
						throw new Exception("Expected the arg to compare to!");
					if (toks[5].Type != TokenType.RSqBracket)
						throw new Exception("Expected the close of the arg to compare to!");

					tok = toks[2];
					if (!tok.Value.StartsWith("arg"))
						throw new Exception("Expected arg declaration!");
					ArgIndexToCheck = Utils.SingleDigitParse(tok.Value[3]) - 1;

					if (toks[3].Type != TokenType.Equal)
						throw new Exception("Expected the value to compare to!");

					tok = toks[4];
					if (tok.Type == TokenType.Identifier)
					{
#warning This check needs to be based on the information in the CPUD file.
						switch(tok.Value.ToLower())
						{
							case "cs":
							case "ds":
							case "es":
							case "fs":
							case "gs":
							case "ss":
								Condition = ConditionType.Compare_Segment;
								RegisterToCompareTo = toks[4].Value.ToUpper();
								break;
							case "al":
							case "ax":
							case "eax":
								Condition = ConditionType.Compare_Reg;
								RegisterToCompareTo = "EAX";
								break;
							default:
								throw new Exception("Unknown value to compare against!");
						}
					}
					else if (tok.Type == TokenType.DecimalNumber)
					{
						Condition = ConditionType.Compare_Imm;
						ImmValueToCompareTo = uint.Parse(tok.Value);
					}
					else
					{
						throw new Exception("Expected either an identifier or a decimal number for the value to compare against!");
					}
				}
				else
				{
					throw new Exception("Unknown override condition!");
				}
			}

			public CodeExpression GetConditionExpression(InExactInstructionOverrideDescription ParentDesc, InstructionForm formOverriding)
			{
				switch (Condition)
				{
					case ConditionType.Fits:
						switch(ParentDesc.NewForm[ArgIndexToCheck].ArgType.ID)
						{
							case InstructionArgType.Dis8_ID:
							case InstructionArgType.Imm8_ID:
								return new CodeMethodInvokeExpression(
									StaticTypeReferences.Stream_InSByteRange,
									new CodeCastExpression(
										StaticTypeReferences.Int,
										new CodeFieldReferenceExpression(
											new CodeThisReferenceExpression(),
											formOverriding.GetArgName(FieldTypeRegistry.UInt.ID, 1, ArgIndexToCheck)
										)
									)
								);
							case InstructionArgType.Dis16_ID:
							case InstructionArgType.Imm16_ID:
								return new CodeMethodInvokeExpression(
									StaticTypeReferences.Stream_InShortRange,
									new CodeCastExpression(
										StaticTypeReferences.Int,
										new CodeFieldReferenceExpression(
											new CodeThisReferenceExpression(),
											formOverriding.GetArgName(FieldTypeRegistry.UInt.ID, 1, ArgIndexToCheck)
										)
									)
								);

							default:
								throw new Exception("Unknown arg type!");
						}
					case ConditionType.Compare_Imm:
						if (ParentDesc.NewForm[ArgIndexToCheck].ArgType == InstructionArgType.Imm8)
						{
							return new CodeBinaryOperatorExpression(
								new CodeFieldReferenceExpression(
									new CodeThisReferenceExpression(),
									formOverriding.GetArgName(FieldTypeRegistry.UInt.ID, 1, ArgIndexToCheck)
								),
								CodeBinaryOperatorType.ValueEquality,
								new CodePrimitiveExpression(ImmValueToCompareTo)
							);
						}
						else
						{
							return new CodeBinaryOperatorExpression(
								ParentDesc.NewForm[ArgIndexToCheck].ArgType.AsArgOperation.GetExpression(ParentDesc.NewForm, ArgIndexToCheck, null),
								CodeBinaryOperatorType.ValueEquality,
								new CodePrimitiveExpression(ImmValueToCompareTo)
							);
						}
					case ConditionType.Compare_Reg:
						// Yikes....
						var enu = formOverriding[ArgIndexToCheck].ArgType.Parameters.Values.GetEnumerator();
						enu.MoveNext();
						int argType = enu.Current.ArgType;
						return new CodeBinaryOperatorExpression(
							new CodeFieldReferenceExpression(
								new CodeThisReferenceExpression(),
								formOverriding.GetArgName(argType, 1, ArgIndexToCheck)
							),
							CodeBinaryOperatorType.ValueEquality,
							new CodeFieldReferenceExpression(
								new CodeTypeReferenceExpression(FieldTypeRegistry.Fields[argType].Name),
								RegisterToCompareTo
							)
						);
					case ConditionType.Compare_Segment:
						return new CodeBinaryOperatorExpression(
							new CodeFieldReferenceExpression(
								new CodeThisReferenceExpression(),
								formOverriding.GetArgName(FieldTypeRegistry.Segment.ID, 1, ArgIndexToCheck)
							),
							CodeBinaryOperatorType.ValueEquality,
							new CodeFieldReferenceExpression(StaticTypeReferences.SegmentExpression, RegisterToCompareTo)
						);
					default:
						throw new Exception("Unknown Condition!");
				}
			}
		}
		public InstructionForm NewForm;
		public InstructionArgSet ArgSetToOverride;
		//private bool OverrideEmitOnly = false;
		private List<OverrideCondition> Conditions = new List<OverrideCondition>();

		public InExactInstructionOverrideDescription DeepCopy()
		{
			var v = new InExactInstructionOverrideDescription();
			v.NewForm = NewForm.DeepCopy();
			v.ArgSetToOverride = ArgSetToOverride;
			v.Conditions = Conditions;
			return v;
		}

		public void ParseCondition(List<Token> toks)
		{
			if (toks[0].Type == TokenType.Identifier && toks[0].Value == "emitonly")
			{
				// This is here just for clarity on the description file side.
				//OverrideEmitOnly = true;
			}
			else
			{
				Conditions.Add(new OverrideCondition(toks));
			}
		}

		private bool CanGenSegmentSwitch(InstructionForm overridenForm)
		{
			if (overridenForm.Overrides.Count < 2)
				return false;
			for (int i = 0; i < overridenForm.Overrides.Count; i++)
			{
				var l = overridenForm.Overrides[i].Conditions;
				for (int i2 = 0; i2 < l.Count; i2++)
				{
					if (l[i2].Condition != OverrideCondition.ConditionType.Compare_Segment)
						return false;
				}
			}
			return true;
		}

		public void WriteConditionalEmit(CodeScopeStatement con, InstructionForm overridenForm, bool ignoreOtherOverrides = false)
		{
			if (Conditions.Count == 0)
				throw new Exception("There are no conditions to the override!");
			if (!ignoreOtherOverrides && CanGenSegmentSwitch(overridenForm))
			{
				CodeSwitchStatement sw = new CodeSwitchStatement(
					new CodeFieldReferenceExpression(
						new CodeThisReferenceExpression(),
						overridenForm.GetArgName(FieldTypeRegistry.Segment.ID, 1, Conditions[0].ArgIndexToCheck)
					)
				);
				for (int i = 0; i < overridenForm.Overrides.Count; i++)
				{
					CodeCaseStatement cs = new CodeCaseStatement(
						new CodeFieldReferenceExpression(
							StaticTypeReferences.SegmentExpression,
							overridenForm.Overrides[i].Conditions[0].RegisterToCompareTo
						)
					);
					CodeScopeStatement cst = new CodeScopeStatement();
					overridenForm.Overrides[i].NewForm.WriteEmit(cst, true);
					cst.Statements.Add(new CodeBreakStatement());
					cs.Statements.Add(cst);
					sw.Cases.Add(cs);
				}
				CodeDefaultCaseStatement defStat = new CodeDefaultCaseStatement();
				CodeScopeStatement defCont = new CodeScopeStatement();
				overridenForm.WriteEmit(defCont, true);
				defStat.Statements.Add(defCont);
				if (overridenForm.WriteOperations[0].Type != WriteOperationType.Throw)
				{
					defStat.Statements.Add(new CodeBreakStatement());
				}
				sw.Cases.Add(defStat);
				con.Statements.Add(sw);
			}
			else
			{
				CodeScopeStatement tCont = new CodeScopeStatement();
				CodeScopeStatement fCont = new CodeScopeStatement();
				CodeExpression condition = Conditions[0].GetConditionExpression(this, overridenForm);
				for (int i = 1; i < Conditions.Count; i++)
				{
					condition = new CodeBinaryOperatorExpression(
						condition, 
						CodeBinaryOperatorType.BooleanAnd, 
						Conditions[i].GetConditionExpression(this, overridenForm)
					);
				}
				CodeConditionStatement condStat = new CodeConditionStatement(condition, new CodeStatement[] { tCont });
				NewForm.WriteEmit(tCont, true);
				if (!ignoreOtherOverrides)
				{
					condStat.FalseStatements.Add(fCont);
					if (overridenForm.Overrides.Count > 1)
					{
						// If we've gotten here, we know that we are the override at index 0.
						for (int i = 1; i < overridenForm.Overrides.Count; i++)
						{
							CodeScopeStatement eICont = new CodeScopeStatement();
							overridenForm.Overrides[i].WriteConditionalEmit(eICont, overridenForm, true);
							fCont.Statements.Add(eICont);
							fCont = new CodeScopeStatement();
							((CodeConditionStatement)eICont.Statements[0]).FalseStatements.Add(fCont);
						}
					}
					overridenForm.WriteEmit(fCont, true);
				}
				con.Statements.Add(condStat);
			}
		}

	}
}

