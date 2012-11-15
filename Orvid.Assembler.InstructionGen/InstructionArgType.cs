using System;
using System.Collections.Generic;
using Orvid.CodeDom;
using System.Globalization;

namespace Orvid.Assembler.InstructionGen
{
	public sealed class CustomInstructionArgParameter
	{
		public string Documentation;
		public string ArgNameSuffix = "";
		public int ArgType;
		public bool Valid = true;
		public InstructionArgType Parent;

		public CodeExpression GetLoadExpression(InstructionForm frm, int argIdx)
		{
			int reqFldsPrev = 0;
			foreach (var val in Parent.Parameters.Values)
			{
				if (val == this)
					break;
				if (val.ArgType == this.ArgType)
				{
					reqFldsPrev++;
				}
			}
			return new CodeFieldReferenceExpression(
				new CodeThisReferenceExpression(),
				frm.GetArgName(ArgType, reqFldsPrev + 1, argIdx)
			);
		}

		public override string ToString()
		{
			return FieldTypeRegistry.Fields[ArgType].Name + (Valid ? ": " + ArgNameSuffix : "") + (Documentation != "" ? "Doc: \"" + Documentation + "\"" : "");
		}
	}

	public sealed class CustomArgOperation
	{
		private Token[] toks;
		private InstructionArgType ParentArg;

		public CustomArgOperation(InstructionArgType parent, Token[] tokens)
		{
			this.toks = tokens;
			this.ParentArg = parent;
		}

		public CodeExpression GetExpression(InstructionForm frm, int argIdx, WriteOperation wrOp)
		{
			int curIdx = 0;
			Token tok = toks[curIdx];
			if (tok.Type != TokenType.Identifier)
				throw new Exception("Expected an identifier!");

			if (tok.Value == "ToString")
			{
				curIdx++;

				tok = toks[curIdx];
				if (tok.Type != TokenType.LParen)
					throw new Exception("Expected an opening parenthesis!");
				curIdx++;

				tok = toks[curIdx];
				if (tok.Type != TokenType.Identifier)
					throw new Exception("Expected an identifier for the expression to convert to a string!");
				CodeExpression ldExpr = GetExpressionFromIdentifier(tok.Value, frm, argIdx, wrOp);
				curIdx++;

				tok = toks[curIdx];
				if (tok.Type != TokenType.RParen)
					throw new Exception("Expected the closing parenthesis!");
				curIdx++;

				return new CodeMethodInvokeExpression(
					ldExpr,
					"ToString"
				);
			}
			else if (tok.Value == "DirectCast")
			{
				curIdx++;

				tok = toks[curIdx];
				if (tok.Type != TokenType.LParen)
					throw new Exception("Expected an opening parenthesis!");
				curIdx++;

				tok = toks[curIdx];
				if (tok.Type != TokenType.Identifier)
					throw new Exception("Expected an identifier representing the argument to cast!");
				CustomInstructionArgParameter param;
				if (!ParentArg.Parameters.TryGetValue(tok.Value, out param))
					throw new Exception("Unknown parameter '" + tok.Value + "'!");
				CodeExpression argLoadExpression = param.GetLoadExpression(frm, argIdx);
				curIdx++;

				tok = toks[curIdx];
				if (tok.Type != TokenType.Comma)
					throw new Exception("Expected a comma before the destination type!");
				curIdx++;

				tok = toks[curIdx];
				if (tok.Type != TokenType.Identifier)
					throw new Exception("Expected an identifier representing the type to cast to!");
				CodeTypeReference destType;
				switch(tok.Value.ToLower())
				{
					case "byte":
						destType = StaticTypeReferences.Byte;
						break;
					case "sbyte":
						destType = StaticTypeReferences.SByte;
						break;
					case "ushort":
						destType = StaticTypeReferences.UShort;
						break;
					case "short":
						destType = StaticTypeReferences.Short;
						break;
					case "uint":
						destType = StaticTypeReferences.UInt;
						break;
					case "int":
						destType = StaticTypeReferences.Int;
						break;
					case "ulong":
						destType = StaticTypeReferences.ULong;
						break;
					case "long":
						destType = StaticTypeReferences.Long;
						break;
					default:
						destType = new CodeTypeReference(tok.Value);
						break;
				}
				curIdx++;

				tok = toks[curIdx];
				if (tok.Type != TokenType.RParen)
					throw new Exception("Expected closing parenthesis!");
				curIdx++;

				return new CodeCastExpression(
					destType,
					argLoadExpression
				);
			}
			else
			{
				CodeExpression sourceExpression;
				bool NamedArgNeedsCast = false;
				if (tok.Value == "Stream")
				{
					sourceExpression = StaticTypeReferences.Emit_StreamArg;
				}
				else
				{
					if (tok.Value == "BitPatterns")
						NamedArgNeedsCast = true;
					sourceExpression = new CodeTypeReferenceExpression(tok.Value);
				}
				curIdx++;
			
				tok = toks[curIdx];
				if (tok.Type != TokenType.Dot)
					throw new Exception("Expected a dot!");
				curIdx++;
			
				tok = toks[curIdx];
				if (tok.Type != TokenType.Identifier)
					throw new Exception("Expected an identifier!");
				CodeMethodReferenceExpression methodRef = new CodeMethodReferenceExpression(
					sourceExpression,
					NamedArgNeedsCast ? BitPatternRegistry.GetPattern(tok.Value).Name : tok.Value
				);
				curIdx++;

				tok = toks[curIdx];
				if (tok.Type != TokenType.LParen)
					throw new Exception("Expected an opening parenthesis!");
				curIdx++;

				List<CodeExpression> parms = new List<CodeExpression>(8);
				tok = toks[curIdx];
				bool expectsAnotherArg = true;
				while (tok.Type != TokenType.RParen)
				{
					if (!expectsAnotherArg)
						throw new Exception("Didn't expect another arg!");
					switch (tok.Type)
					{
						case TokenType.Identifier:
							parms.Add(GetExpressionFromIdentifier(tok.Value, frm, argIdx, wrOp, NamedArgNeedsCast));
							break;
						case TokenType.Number:
							if (tok.NumberValue.Format == NumberFormat.Decimal)
							{
								parms.Add(new CodePrimitiveExpression((int)tok.NumberValue.Value));
							}
							else
							{
								parms.Add(new CodePrimitiveExpression(tok.NumberValue.Value));
							}
							break;
						default:
							throw new Exception("Unknown token type!");
					}
					curIdx++;

					tok = toks[curIdx];
					if (tok.Type != TokenType.Comma)
					{
						expectsAnotherArg = false;
					}
					else
					{
						curIdx++;
						tok = toks[curIdx];
					}
				}
				if (expectsAnotherArg)
					throw new Exception("Expected another arg!");

				return new CodeMethodInvokeExpression(
					methodRef,
					parms.ToArray()
				);
			}
		}
		
		private CodeExpression GetExpressionFromIdentifier(string val, InstructionForm frm, int argIdx, WriteOperation wrOp, bool namedParamNeedsCast = false)
		{
			CustomInstructionArgParameter param;
			if (ParentArg.Parameters.TryGetValue(val, out param))
			{
				if (namedParamNeedsCast)
				{
					return frm[argIdx].ArgType.AsArgOperation.GetExpression(frm, argIdx, wrOp);
				}
				else
				{
					return param.GetLoadExpression(frm, argIdx);
				}
			}
			else if (val == "Segment")
			{
				return new CodeFieldReferenceExpression(
					new CodeThisReferenceExpression(),
					frm.GetArgName(FieldTypeRegistry.Segment.ID, 1, InstructionArgSet.TotalArgs_Max)
				);
			}
			else if (val == "ParentAssembler")
			{
				return StaticTypeReferences.ParentAssemblerExpression;
			}
			else if (val == "Stream")
			{
				return StaticTypeReferences.Emit_StreamArg;
			}
			else if (val.StartsWith("arg"))
			{
				int fArg = Utils.SingleDigitParse(val[3]) - 1;
				return wrOp.WriteArguments[fArg].GetLoadExpression(frm);
			}
			else
			{
				return new CodeTypeReferenceExpression(val);
			}
		}

	}

	public sealed class InstructionArgType
	{
		public const string DisArgSuffix = "Displacement";
		public const string ImmArgSuffix = "Value";

		public const int None_ID = 0;
		public static readonly InstructionArgType None = new InstructionArgType()
		{
			ID = None_ID,
			Name = "None",
			TrueName = "None",
			SizelessType = SizelessType.None,
			TrueSizelessType = SizelessType.None,
			Size = 0,
		};

		public const int Imm8_ID = 1;
		public static readonly InstructionArgType Imm8 = new InstructionArgType()
		{
			ID = Imm8_ID,
			Name = "Imm8",
			TrueName = "Imm8",
			SizelessType = SizelessType.Imm,
			TrueSizelessType = SizelessType.Imm,
			Size = 1,
			Parameters = new Dictionary<string, CustomInstructionArgParameter>()
			{
				{ 
					ImmArgSuffix, new CustomInstructionArgParameter()
					{
						ArgNameSuffix = ImmArgSuffix,
						ArgType = FieldTypeRegistry.UInt.ID,
						Documentation = DocAliasRegistry.DocAlias_Imm_Value,
						Parent = Imm8
					}
				}
			},
		};

		public const int Imm16_ID = 2;
		public static readonly InstructionArgType Imm16 = new InstructionArgType()
		{
			ID = Imm16_ID,
			Name = "Imm16",
			TrueName = "Imm16",
			SizelessType = SizelessType.Imm,
			TrueSizelessType = SizelessType.Imm,
			Size = 2,
			Parameters = new Dictionary<string, CustomInstructionArgParameter>()
			{
				{ 
					ImmArgSuffix, new CustomInstructionArgParameter()
					{
						ArgNameSuffix = ImmArgSuffix,
						ArgType = FieldTypeRegistry.UInt.ID,
						Documentation = DocAliasRegistry.DocAlias_Imm_Value,
						Parent = Imm16
					}
				}
			},
		};

		public const int Imm32_ID = 3;
		public static readonly InstructionArgType Imm32 = new InstructionArgType()
		{
			ID = Imm32_ID,
			Name = "Imm32",
			TrueName = "Imm32",
			SizelessType = SizelessType.Imm,
			TrueSizelessType = SizelessType.Imm,
			Size = 4,
			Parameters = new Dictionary<string, CustomInstructionArgParameter>()
			{
				{ 
					ImmArgSuffix, new CustomInstructionArgParameter()
					{
						ArgNameSuffix = ImmArgSuffix,
						ArgType = FieldTypeRegistry.UInt.ID,
						Documentation = DocAliasRegistry.DocAlias_Imm_Value,
						Parent = Imm32
					}
				}
			},
		};
		
		public const int Dis8_ID = 4;
		public static readonly InstructionArgType Dis8 = new InstructionArgType()
		{
			ID = Dis8_ID,
			Name = "Dis8",
			TrueName = "Dis8",
			SizelessType = SizelessType.Dis,
			TrueSizelessType = SizelessType.Dis,
			Size = 1,
			Parameters = new Dictionary<string, CustomInstructionArgParameter>()
			{
				{ 
					DisArgSuffix, new CustomInstructionArgParameter()
					{
						ArgNameSuffix = DisArgSuffix,
						ArgType = FieldTypeRegistry.UInt.ID,
						Documentation = DocAliasRegistry.DocAlias_Dis_Value,
						Parent = Dis8
					}
				}
			},
		};
		
		public const int Dis16_ID = 5;
		public static readonly InstructionArgType Dis16 = new InstructionArgType()
		{
			ID = Dis16_ID,
			Name = "Dis16",
			TrueName = "Dis16",
			SizelessType = SizelessType.Dis,
			TrueSizelessType = SizelessType.Dis,
			Size = 2,
			Parameters = new Dictionary<string, CustomInstructionArgParameter>()
			{
				{ 
					DisArgSuffix, new CustomInstructionArgParameter()
					{
						ArgNameSuffix = DisArgSuffix,
						ArgType = FieldTypeRegistry.UInt.ID,
						Documentation = DocAliasRegistry.DocAlias_Dis_Value,
						Parent = Dis16
					}
				}
			},
		};
		
		public const int Dis32_ID = 6;
		public static readonly InstructionArgType Dis32 = new InstructionArgType()
		{
			ID = Dis32_ID,
			Name = "Dis32",
			TrueName = "Dis32",
			SizelessType = SizelessType.Dis,
			TrueSizelessType = SizelessType.Dis,
			Size = 4,
			Parameters = new Dictionary<string, CustomInstructionArgParameter>()
			{
				{ 
					DisArgSuffix, new CustomInstructionArgParameter()
					{
						ArgNameSuffix = DisArgSuffix,
						ArgType = FieldTypeRegistry.UInt.ID,
						Documentation = DocAliasRegistry.DocAlias_Dis_Value,
						Parent = Dis32
					}
				}
			},
		};

		public bool IsAlias { get; set; }
		public InstructionArgType AliasTo { get; set; }
		public string Name { get; set; }
		public string TrueName { get; set; }
		public SizelessType SizelessType { get; set; }
		public SizelessType TrueSizelessType { get; set; }

		public bool RequiresSegment { get; set; }

		private Dictionary<string, CustomInstructionArgParameter> mParameters = new Dictionary<string, CustomInstructionArgParameter>();
		public Dictionary<string, CustomInstructionArgParameter> Parameters
		{
			get { return mParameters; }
			set { mParameters = value; }
		}

		private int? mSize;
		public bool HasSize
		{
			get { return mSize.HasValue; }
		}
		public int Size
		{
			get
			{
				if (mSize == null)
					throw new Exception("The size wasn't specified!");
				return mSize.Value;
			}
			set
			{
				mSize = value;
			}
		}

		private List<InstructionArgType> mExpandsTo = new List<InstructionArgType>();
		public List<InstructionArgType> ExpandsTo
		{
			get { return mExpandsTo; }
			set { mExpandsTo = value; }
		}
		
		public CustomArgOperation WriteOperation { get; set; }
		public CustomArgOperation ReadOperation { get; set; }
		public CustomArgOperation AsArgOperation { get; set; }

		public int ID { get; set; }

		public void RequestFields(int[] RequiredFields)
		{
			if (this.ExpandsTo.Count > 0)
				throw new Exception("This should not be happening!");
			foreach (CustomInstructionArgParameter param in mParameters.Values)
			{
				RequiredFields[param.ArgType]++;
			}
		}

		public void AddDocumentationLines(string argBaseName, CodeDocumentationNodeCollection LinesOfDocumentation)
		{
			foreach (CustomInstructionArgParameter param in mParameters.Values)
			{
				LinesOfDocumentation.Add(new CodeDocumentationParameterNode(argBaseName + param.ArgNameSuffix, DocAliasRegistry.ExpandDocAliasValue(param.Documentation, argBaseName)));
			}
		}

		public void RequestParameters(InstructionArg arg, bool hasSizeArg, CodeParameterDeclarationExpressionCollection prs)
		{
			if (hasSizeArg)
			{
				switch (ID)
				{
					case None_ID:
						return;
					case Imm8_ID:
					case Imm16_ID:
					case Imm32_ID:
						prs.Add(new CodeParameterDeclarationExpression(StaticTypeReferences.UInt, arg.Name + ImmArgSuffix) 
							{
								DefaultValueExpression = (arg.DefaultValue.HasValue ? new CodePrimitiveExpression(arg.DefaultValue.Value) : null) 
							}
						);
						return;
						
					default:
						break;
				}
			}
			switch (ID)
			{
				case None_ID:
					break;
				case Dis8_ID:
					prs.Add(new CodeParameterDeclarationExpression(StaticTypeReferences.SByte, arg.Name + DisArgSuffix));
					break;
				case Dis16_ID:
					prs.Add(new CodeParameterDeclarationExpression(StaticTypeReferences.Short, arg.Name + DisArgSuffix));
					break;
				case Dis32_ID:
					prs.Add(new CodeParameterDeclarationExpression(StaticTypeReferences.Int, arg.Name + DisArgSuffix));
					break;

				case Imm8_ID:
					prs.Add(new CodeParameterDeclarationExpression(StaticTypeReferences.Byte, arg.Name + ImmArgSuffix) 
						{
							DefaultValueExpression = (arg.DefaultValue.HasValue ? new CodePrimitiveExpression(arg.DefaultValue.Value) : null) 
						}
					);
					return;
				case Imm16_ID:
					prs.Add(new CodeParameterDeclarationExpression(StaticTypeReferences.UShort, arg.Name + ImmArgSuffix) 
						{
							DefaultValueExpression = (arg.DefaultValue.HasValue ? new CodePrimitiveExpression(arg.DefaultValue.Value) : null) 
						}
					);
					return;
				case Imm32_ID:
					prs.Add(new CodeParameterDeclarationExpression(StaticTypeReferences.UInt, arg.Name + ImmArgSuffix) 
						{
							DefaultValueExpression = (arg.DefaultValue.HasValue ? new CodePrimitiveExpression(arg.DefaultValue.Value) : null) 
						}
					);
					return;

				default:
					foreach (CustomInstructionArgParameter p in mParameters.Values)
					{
						prs.Add(new CodeParameterDeclarationExpression(FieldTypeRegistry.Fields[p.ArgType].CodeType, arg.Name + p.ArgNameSuffix));
					}
					return;
			}
		}

		public void WriteConstructorBodyArgProcessing(InstructionForm frm, CodeConstructor c, int argIdx)
		{
			InstructionArg arg = frm[argIdx];
			switch (ID)
			{
				case None_ID:
					break;
				// Dis8 & Dis16 have to be sign-extended before being stored.
				// (this way they can be compared using the exact same code as
				// dis32 can)
				case Dis8_ID:
				case Dis16_ID:
					c.Statements.Add(
						new CodeAssignStatement(
							new CodeFieldReferenceExpression(
								new CodeThisReferenceExpression(),
								frm.GetArgName(FieldTypeRegistry.UInt.ID, 1, argIdx)
							),
							new CodeCastExpression(
								StaticTypeReferences.UInt,
								new CodeCastExpression(
									StaticTypeReferences.Int,
									new CodeArgumentReferenceExpression(arg.Name + DisArgSuffix)
								)
							)
						)
					);
					break;
				// No sign extension needed.
				case Dis32_ID:
					c.Statements.Add(
						new CodeAssignStatement(
							new CodeFieldReferenceExpression(
								new CodeThisReferenceExpression(),
								frm.GetArgName(FieldTypeRegistry.UInt.ID, 1, argIdx)
							),
							new CodeCastExpression(
								StaticTypeReferences.UInt,
								new CodeArgumentReferenceExpression(arg.Name + DisArgSuffix)
							)
						)
					);
					break;
				case Imm8_ID:
				case Imm16_ID:
				case Imm32_ID:
					c.Statements.Add(
						new CodeAssignStatement(
							new CodeFieldReferenceExpression(
								new CodeThisReferenceExpression(),
								frm.GetArgName(FieldTypeRegistry.UInt.ID, 1, argIdx)
							),
							new CodeCastExpression(
								StaticTypeReferences.UInt,
								new CodeArgumentReferenceExpression(arg.Name + ImmArgSuffix)
							)
						)
					);
					break;

				default:
					int[] reqs = new int[FieldTypeRegistry.MaxFieldID + 1];
					foreach(CustomInstructionArgParameter p in mParameters.Values)
					{
						c.Statements.Add(
							new CodeAssignStatement(
								new CodeFieldReferenceExpression(
									new CodeThisReferenceExpression(),
									frm.GetArgName(p.ArgType, reqs[p.ArgType] + 1, argIdx)
								),
								new CodeArgumentReferenceExpression(arg.Name + p.ArgNameSuffix)
							)
						);
						reqs[p.ArgType]++;
					}
					break;
			}
		}

		public CodeExpression GetToStringForArg(InstructionForm frm, int argIdx)
		{
			InstructionArg arg = frm[argIdx];
			int padSize = 0;
			switch (arg.ArgType.ID)
			{
				case InstructionArgType.Dis8_ID:
				case InstructionArgType.Dis16_ID:
				case InstructionArgType.Dis32_ID:
					return new CodeMethodInvokeExpression(
						StaticTypeReferences.NamingHelper_NameDisplacement,
						StaticTypeReferences.ParentAssemblerExpression,
						new CodeThisReferenceExpression(),
						new CodeCastExpression(
							StaticTypeReferences.Int,
							new CodeFieldReferenceExpression(
								new CodeThisReferenceExpression(),
								frm.GetArgName(FieldTypeRegistry.UInt.ID, 1, argIdx)
							)
						)
					);

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
					return arg.ArgType.ReadOperation.GetExpression(frm, argIdx, null);
			}
		ImmCommon:
			if (arg.NumberFormat == ImmNumberFormat.Decimal)
			{
				return new CodeMethodInvokeExpression(
					new CodeFieldReferenceExpression(
						new CodeThisReferenceExpression(),
						frm.GetArgName(FieldTypeRegistry.UInt.ID, 1, argIdx)
					),
					"ToString"
				);
			}
			else
			{
				return Generator.LanguageProvider.GetPaddedHexToString(
					new CodeFieldReferenceExpression(
						new CodeThisReferenceExpression(),
						frm.GetArgName(FieldTypeRegistry.UInt.ID, 1, argIdx)
					),
					padSize
				);
			}
		}


		public override int GetHashCode()
		{
			return ID;
		}

	}
}

