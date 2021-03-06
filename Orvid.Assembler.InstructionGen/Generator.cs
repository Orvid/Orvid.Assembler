using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Orvid.CodeDom;
using Orvid.CodeDom.Compiler;
using Orvid.CodeDom.CodeGenerators;
using System.Globalization;

namespace Orvid.Assembler.InstructionGen
{
	public sealed class Generator
	{
		private string Namespace = "";
		private Dictionary<string, Instruction> Instructions = new Dictionary<string, Instruction>();

		private void ThrowError(string msg, Token tok)
		{
			// Will eventually output line & column information from the token.
			throw new Exception(msg + " @ " + tok.StartLine.ToString() + ":" + tok.StartColumn.ToString()/* + " to " + tok.EndLine.ToString() + ":" + tok.EndColumn.ToString()*/);
		}

		private const int InitialCurDocListSize = 8;
		public unsafe Generator(Token[] toks)
		{
			List<string> curDocList = new List<string>(InitialCurDocListSize);
			int curIdx = 0;
			int endIdx = toks.Length;
			Token tok;
			try
			{
				tok = toks[curIdx];
				if (tok.Type != TokenType.Identifier || tok.Value != "cpu")
					ThrowError("Expecteded the cpu declaration!", tok);
				curIdx++;

				tok = toks[curIdx];
				if (tok.Type != TokenType.Identifier)
					ThrowError("An identifer was expected for the type of CPU!", tok);
				curIdx++;

				tok = toks[curIdx];
				if (tok.Type != TokenType.EOL)
					ThrowError("Expected EOL!", tok);
				curIdx++;

				tok = toks[curIdx];
				if (tok.Type != TokenType.Identifier || tok.Value != "namespace")
					ThrowError("Expected namespace declaration after cpu declaration!", tok);
				curIdx++;

				while (tok.Type != TokenType.EOL)
				{
					tok = toks[curIdx];
					Namespace += tok.Type == TokenType.Dot ? "." : tok.Value;
					curIdx++;
				}

				while (curIdx < endIdx)
				{
					tok = toks[curIdx];

					switch (tok.Type)
					{
						case TokenType.Sharp:
							curIdx++;

							tok = toks[curIdx];
							if (tok.Type != TokenType.Identifier)
								ThrowError("Expected an identifier!", tok);
							if (tok.Value == "define")
							{
								curIdx++;

								tok = toks[curIdx];
								if (tok.Type != TokenType.Identifier)
									ThrowError("Expected an identifier!", tok);
								switch (tok.Value)
								{
									case "StreamClass":
										curIdx++;

										tok = toks[curIdx];
										if (tok.Type != TokenType.Identifier)
											ThrowError("Expected an identifier!", tok);
										StaticTypeReferences.StreamClassName = tok.Value;
										curIdx++;

										tok = toks[curIdx];
										if (tok.Type != TokenType.EOL)
											ThrowError("Expected EOL!", tok);
										curIdx++;
										break;
									case "InstructionClass":
										curIdx++;
										
										tok = toks[curIdx];
										if (tok.Type != TokenType.Identifier)
											ThrowError("Expected an identifier!", tok);
										StaticTypeReferences.InstructionClassName = tok.Value;
										curIdx++;
										
										tok = toks[curIdx];
										if (tok.Type != TokenType.EOL)
											ThrowError("Expected EOL!", tok);
										curIdx++;
										break;
									case "AssemblerClass":
										curIdx++;
										
										tok = toks[curIdx];
										if (tok.Type != TokenType.Identifier)
											ThrowError("Expected an identifier!", tok);
										StaticTypeReferences.AssemblerClassName = tok.Value;
										curIdx++;
										
										tok = toks[curIdx];
										if (tok.Type != TokenType.EOL)
											ThrowError("Expected EOL!", tok);
										curIdx++;
										break;
									case "PrefixClass":
										curIdx++;
										
										tok = toks[curIdx];
										if (tok.Type != TokenType.Identifier)
											ThrowError("Expected an identifier!", tok);
										StaticTypeReferences.PrefixClassName = tok.Value;
										curIdx++;
										
										tok = toks[curIdx];
										if (tok.Type != TokenType.EOL)
											ThrowError("Expected EOL!", tok);
										curIdx++;
										break;
									case "InstructionFormClass":
										curIdx++;
										
										tok = toks[curIdx];
										if (tok.Type != TokenType.Identifier)
											ThrowError("Expected an identifier!", tok);
										StaticTypeReferences.InstructionFormClassName = tok.Value;
										curIdx++;
										
										tok = toks[curIdx];
										if (tok.Type != TokenType.EOL)
											ThrowError("Expected EOL!", tok);
										curIdx++;
										break;
									case "AssemblySyntaxClass":
										curIdx++;
										
										tok = toks[curIdx];
										if (tok.Type != TokenType.Identifier)
											ThrowError("Expected an identifier!", tok);
										StaticTypeReferences.AssemblySyntaxClassName = tok.Value;
										curIdx++;
										
										tok = toks[curIdx];
										if (tok.Type != TokenType.EOL)
											ThrowError("Expected EOL!", tok);
										curIdx++;
										break;
									case "BitPattern":
										curIdx++;
										BitPattern pat = new BitPattern(toks, ref curIdx);
										BitPatternRegistry.RegisterBitPattern(pat);
										break;
									case "BitPatternAlias":
										curIdx++;

										tok = toks[curIdx];
										if (tok.Type != TokenType.Identifier)
											ThrowError("Expected an identifier for the alias to the bit pattern!", tok);
										string bpa = tok.Value;
										curIdx++;

										tok = toks[curIdx];
										if (tok.Type != TokenType.Identifier)
											ThrowError("Expected an identifier for the target of the alias of the bit pattern!", tok);
										BitPatternRegistry.RegisterAlias(bpa, BitPatternRegistry.GetPattern(tok.Value));
										curIdx++;
										
										tok = toks[curIdx];
										if (tok.Type != TokenType.EOL)
											ThrowError("Expected EOL!", tok);
										curIdx++;
										break;
									case "ArgAlias":
										curIdx++;

										tok = toks[curIdx];
										if (tok.Type != TokenType.Identifier)
											ThrowError("Expected an identifier for the alias to use!", tok);
										string alName = tok.Value;
										curIdx++;

										tok = toks[curIdx];
										if (tok.Type != TokenType.Identifier)
											ThrowError("Expected an identifier for the argument type to alias!", tok);
										InstructionArgTypeRegistry.RegisterType(
											new InstructionArgType()
											{
												Name = alName,
												AliasTo = InstructionArgTypeRegistry.GetType(tok.Value),
												IsAlias = true
											}
										);
										curIdx++;
										
										tok = toks[curIdx];
										if (tok.Type != TokenType.EOL)
											ThrowError("Expected EOL!", tok);
										curIdx++;
										break;
									case "Enum":
										curIdx++;

										tok = toks[curIdx];
										if (tok.Type != TokenType.Identifier)
											ThrowError("Expected an identifier representing the name of the enum to generate!", tok);
										string enumName = tok.Value;
										curIdx++;

										tok = toks[curIdx];
										if (tok.Type != TokenType.Identifier)
											ThrowError("Expected an identifier representing the base type of the enum to generate!", tok);
										EnumRegistryEntry ent = new EnumRegistryEntry(enumName, FieldTypeRegistry.GetType(tok.Value), curDocList.Count > 0 ? curDocList : null);
										if (curDocList.Count > 0)
											curDocList = new List<string>(InitialCurDocListSize);
										curIdx++;

										tok = toks[curIdx];
										if (tok.Type == TokenType.EOL)
										{
											curIdx++;
											tok = toks[curIdx];
										}

										if (tok.Type != TokenType.LCurlBracket)
											ThrowError("Expected an opening curly bracket before the values for the enum!", tok);
										curIdx++;

										tok = toks[curIdx];
										while (tok.Type != TokenType.RCurlBracket)
										{
											while (tok.Type == TokenType.DocumentationComment)
											{
												// We're not working from the main list,
												// so we have to do these checks ourself.
												if (tok.Value.Length >= 9 && tok.Value.Length <= 14)
												{
													string tmpS = tok.Value.ToLower();
													if (tmpS == "<summary>" ||
														tmpS == "</summary>"
													    )
													{
														curIdx++;
														tok = toks[curIdx];
														continue;
													}
												}
												curDocList.Add(tok.Value);
												curIdx++;
												tok = toks[curIdx];
											}
											if (tok.Type != TokenType.Identifier)
												ThrowError("Expected an identifier for the enum value!", tok);
											string valueName = tok.Value;
											curIdx++;

											uint? valueValue = null;
											bool showAsHex = false;
											tok = toks[curIdx];
											if (tok.Type == TokenType.Equal)
											{
												curIdx++;

												tok = toks[curIdx];
												if (tok.Type != TokenType.Number)
													ThrowError("Expected a number as the value for the enum value!", tok);
												valueValue = tok.NumberValue.Value;
												showAsHex = tok.NumberValue.Format != NumberFormat.Decimal;
												curIdx++;

												tok = toks[curIdx];
											}
											
											ent.Members.Add(new EnumRegistryEntryMember(valueName, valueValue, showAsHex, curDocList.Count > 0 ? curDocList : null));
											if (curDocList.Count > 0)
												curDocList = new List<string>(InitialCurDocListSize);

											if (tok.Type == TokenType.Comma)
											{
												curIdx++;
												tok = toks[curIdx];
											}
										}
										curIdx++;

										tok = toks[curIdx];
										if (tok.Type != TokenType.EOL)
											ThrowError("Expected EOL!", tok);
										curIdx++;

										EnumRegistry.RegisterEntry(ent);
										break;

									case "Segment":
										curIdx++;
										
										tok = toks[curIdx];
										if (tok.Type != TokenType.Identifier)
											ThrowError("Expected an identifier!", tok);
										StaticTypeReferences.SegmentClassName = tok.Value;
										curIdx++;

										tok = toks[curIdx];
										// EOL before lcurl is optional
										if (tok.Type == TokenType.EOL)
										{
											curIdx++;
											tok = toks[curIdx];
										}

										if (tok.Type != TokenType.LCurlBracket)
											ThrowError("Expected a left curly brace!", tok);
										curIdx++; 

										tok = toks[curIdx];
										if (tok.Type == TokenType.EOL)
										{
											curIdx++;
											tok = toks[curIdx];
										}

										while (tok.Type != TokenType.RCurlBracket)
										{
											if (tok.Type != TokenType.Identifier)
												ThrowError("Expected an identifier!", tok);
											if (StaticTypeReferences.SegmentLookup.ContainsKey(tok.Value.ToUpper()))
												ThrowError("Duplicate segment declared!", tok);
											StaticTypeReferences.SegmentLookup[tok.Value.ToUpper()] = true;
											curIdx++;

											tok = toks[curIdx];
											if (tok.Type == TokenType.Comma)
											{
												curIdx++;
												tok = toks[curIdx];
											}
											else
											{
												break;
											}
										}

										if (tok.Type == TokenType.EOL)
										{
											curIdx++;
											tok = toks[curIdx];
										}

										if (tok.Type != TokenType.RCurlBracket)
											ThrowError("Either your missing a comma after your segment name, or something else is wrong.", tok);
										curIdx++;

										tok = toks[curIdx];
										if (tok.Type == TokenType.EOL)
										{
											curIdx++;
											tok = toks[curIdx];
										}
										break;

									case "arg":
										curIdx++;
										
										InstructionArgType custArg = new InstructionArgType();

										tok = toks[curIdx];
										if (tok.Type != TokenType.LParen)
											ThrowError("Expected opening parenthesis to start the name of the argument!", tok);
										curIdx++;

										tok = toks[curIdx];
										if (tok.Type != TokenType.Identifier)
											ThrowError("Expected an identifier for the name of the argument!", tok);
										custArg.Name = tok.Value;
										curIdx++;

										tok = toks[curIdx];
										if (tok.Type != TokenType.Comma)
										{
											if (tok.Type == TokenType.RParen)
											{
												// It's a based argument
												curIdx++;
												ProcessArgBase(toks, ref curIdx, custArg.Name, null);
												break;
											}
											else
											{
												ThrowError("Expected a comma after the name of the argument!", tok);
											}
										}
										curIdx++;

										tok = toks[curIdx];
										if (tok.Type == TokenType.Comma)
										{
											custArg.TrueName = custArg.Name;
											curIdx++;
										}
										else if (tok.Type == TokenType.Identifier)
										{
											custArg.TrueName = tok.Value;
											curIdx++;

											tok = toks[curIdx];
											if (tok.Type != TokenType.Comma)
											{
												if (tok.Type == TokenType.RParen)
												{
													// It's a based arg.
													curIdx++;
													ProcessArgBase(toks, ref curIdx, custArg.Name, custArg.TrueName);
													break;
												}
												else
												{
													ThrowError("Expected a comma after the true type of the arg!", tok);
												}
											}
											curIdx++;
										}
										else
										{
											ThrowError("Expected either an identifier or a comma!", tok);
										}

										tok = toks[curIdx];
										if (tok.Type != TokenType.Identifier)
											ThrowError("Expected an identifier for the sizeless type of the arg!", tok);
										custArg.SizelessType = SizelessTypeRegistry.GetType(tok.Value);
										curIdx++;

										tok = toks[curIdx];
										if (tok.Type != TokenType.Comma)
											ThrowError("Expected a comma before the true sizeless type of the arg!", tok);
										curIdx++;

										tok = toks[curIdx];
										if (tok.Type == TokenType.RParen)
										{
											custArg.TrueSizelessType = custArg.SizelessType;
										}
										else if (tok.Type == TokenType.Identifier)
										{
											custArg.TrueSizelessType = SizelessTypeRegistry.GetType(tok.Value);
											curIdx++;
										}
										else
										{
											ThrowError("Expected the true sizeless type!", tok);
										}


										tok = toks[curIdx];
										if (tok.Type != TokenType.RParen)
											ThrowError("Expected the closing parenthesis for the name of the argument!", tok);
										curIdx++;

										tok = toks[curIdx];
										if (tok.Type != TokenType.Identifier || tok.Value != "as")
											ThrowError("Expected the 'as' keyword!", tok);
										curIdx++;
										
										tok = toks[curIdx];
										bool expectAnotherParam = true;
										while (tok.Type != TokenType.EOL)
										{
											if (!expectAnotherParam)
												ThrowError("Expected an EOL!", tok);

											CustomInstructionArgParameter param = new CustomInstructionArgParameter();
											if (tok.Type != TokenType.Identifier)
												ThrowError("Expected an identifier representing the type for the parameter!", tok);
											param.ArgType = FieldTypeRegistry.GetType(tok.Value);
											curIdx++;
											if (tok.Value == "Invalid")
											{
												tok = toks[curIdx];
												if (tok.Type != TokenType.EOL)
													ThrowError("Expected EOL after an invalid declaration!", tok);
												param.Valid = false;
												expectAnotherParam = false;
											}
											else
											{
												tok = toks[curIdx];
												if (tok.Type != TokenType.LParen)
													ThrowError("Expected the opening parenthesis for the argument's name suffix!", tok);
												curIdx++;
												
												tok = toks[curIdx];
												if (tok.Type != TokenType.Identifier)
													ThrowError("Expected an identifier representing the argument's name suffix!", tok);
												param.ArgNameSuffix = tok.Value;
												curIdx++;

												tok = toks[curIdx];
												if (tok.Type != TokenType.Comma)
													ThrowError("Expected a comma before the argument's documentation!", tok);
												curIdx++;

												tok = toks[curIdx];
												curIdx++;
												if (tok.Type == TokenType.String)
												{
													param.Documentation = tok.Value;
												}
												else if (tok.Type == TokenType.Identifier)
												{
													if (tok.Value != "DocAlias")
														ThrowError("Expected 'DocAlias'!", tok);

													tok = toks[curIdx];
													if (tok.Type != TokenType.LParen)
														ThrowError("Expected opening parenthesis for the doc alias!", tok);
													curIdx++;

													tok = toks[curIdx];
													if (tok.Type != TokenType.Identifier)
														ThrowError("Expected an identifier representing the doc alias!", tok);
													param.Documentation = DocAliasRegistry.GetDocAliasValue(tok.Value);
													curIdx++;

													tok = toks[curIdx];
													if (tok.Type != TokenType.RParen)
														ThrowError("Expected closing parenthesis for the doc alias!", tok);
													curIdx++;
												}
												else
												{
													ThrowError("Unexpected token!", tok);
												}

												tok = toks[curIdx];
												if (tok.Type != TokenType.RParen)
													ThrowError("Expected the closing parenthesis for the argument's name suffix!", tok);
												curIdx++;
												
												tok = toks[curIdx];
												if (tok.Type == TokenType.Comma)
												{
													curIdx++;
													tok = toks[curIdx];
													expectAnotherParam = true;
												}
												else
												{
													expectAnotherParam = false;
												}
											}
											param.Parent = custArg;
											custArg.Parameters[param.ArgNameSuffix] = param;

											tok = toks[curIdx];
										}
										if (expectAnotherParam)
											ThrowError("Expected another parameter!", tok);
										curIdx++;

										tok = toks[curIdx];
										if (tok.Type != TokenType.LCurlBracket)
											ThrowError("Expected a left curly bracket!", tok);
										curIdx++;

										tok = toks[curIdx];
										while (tok.Type != TokenType.RCurlBracket)
										{
											if (tok.Type != TokenType.Identifier)
												ThrowError("Expected an identifier representing the command!", tok);
											curIdx++;
											int sIdx;
											switch (tok.Value)
											{
												case "Size":
													tok = toks[curIdx];
													if (tok.Type != TokenType.Number)
														ThrowError("Expected a number representing the size!", tok);
													custArg.Size = (int)tok.NumberValue.Value;
													curIdx++;
													break;
												case "RequiresSeg":
													custArg.RequiresSegment = true;
													break;
												case "AsArg":
													sIdx = curIdx;
													tok = toks[curIdx];
													while (tok.Type != TokenType.EOL)
													{
														curIdx++;
														tok = toks[curIdx];
													}
													custArg.AsArgOperation = new CustomArgOperation(custArg, toks.Slice(sIdx, curIdx));
													break;
												case "Write":
													sIdx = curIdx;
													tok = toks[curIdx];
													while (tok.Type != TokenType.EOL)
													{
														curIdx++;
														tok = toks[curIdx];
													}
													custArg.WriteOperation = new CustomArgOperation(custArg, toks.Slice(sIdx, curIdx));
													break;
												case "Read":
													sIdx = curIdx;
													tok = toks[curIdx];
													while (tok.Type != TokenType.EOL)
													{
														curIdx++;
														tok = toks[curIdx];
													}
													custArg.ReadOperation = new CustomArgOperation(custArg, toks.Slice(sIdx, curIdx));
													break;
												case "Expand":
													tok = toks[curIdx];
													if (tok.Type == TokenType.EOL)
													{
														curIdx++;
														tok = toks[curIdx];
													}
													if (tok.Type != TokenType.LCurlBracket)
														ThrowError("Expected an opening curly bracket!", tok);
													curIdx++;

													tok = toks[curIdx];
													if (tok.Type == TokenType.EOL)
													{
														curIdx++;
														tok = toks[curIdx];
													}

													bool expectsAnotherArg = true;
													while (tok.Type != TokenType.RCurlBracket)
													{
														if (!expectsAnotherArg)
															ThrowError("Expected the end of the expand statement!", tok);

														if (tok.Type != TokenType.Identifier)
															ThrowError("Expected an identifier representing the arg to expand to!", tok);
														custArg.ExpandsTo.Add(InstructionArgTypeRegistry.GetType(tok.Value));
														curIdx++;

														tok = toks[curIdx];
														if (tok.Type == TokenType.Comma)
														{
															curIdx++;
															tok = toks[curIdx];
														}
														else
														{
															expectsAnotherArg = false;
														}

														if (tok.Type == TokenType.EOL)
														{
															curIdx++;
															tok = toks[curIdx];
														}
													}
													if (expectsAnotherArg)
														ThrowError("Expected another arg!", tok);
													curIdx++;

													break;
												default:
													ThrowError("Unknown command!", tok);
													break;
											}
											tok = toks[curIdx];
											if (tok.Type != TokenType.EOL)
												ThrowError("Expected EOL after command!", tok);
											curIdx++;

											tok = toks[curIdx];
										}
										InstructionArgTypeRegistry.RegisterType(custArg);
										curIdx += 2;
										break;

									default:
										ThrowError("Unknown compiler directive!", tok);
										break;
								}

							}
							else if (tok.Value == "register")
							{
								curIdx++;

								tok = toks[curIdx];
								if (tok.Type != TokenType.Identifier)
									ThrowError("Expected either Type or SizelessType!", tok);
								curIdx++;
								if (tok.Value == "Type")
								{
									string typeName;
									string prefix = "";

									tok = toks[curIdx];
									if (tok.Type != TokenType.Identifier)
										ThrowError("Expected an identifier for the type!", tok);
									typeName = tok.Value;
									curIdx++;

									tok = toks[curIdx];
									if (tok.Type != TokenType.EOL)
									{
										if (tok.Type == TokenType.Identifier)
										{
											prefix = tok.Value;
											curIdx++;
										}
										else
										{
											ThrowError("Unexpected token!", tok);
										}
									}

									FieldTypeRegistry.RegisterType(typeName, prefix);
								}
								else if (tok.Value == "SizelessType")
								{
									tok = toks[curIdx];
									if (tok.Type != TokenType.Identifier)
										ThrowError("Expected an identifier for the sizeless type!", tok);
									SizelessTypeRegistry.RegisterType(tok.Value);
									curIdx++;
								}
								else if (tok.Value == "Prefix")
								{
									tok = toks[curIdx];
									if (tok.Type != TokenType.Identifier)
										ThrowError("Expected an identifier for the write operation of the prefix!", tok);
									string wop = tok.Value;
									curIdx++;

									tok = toks[curIdx];
									if (tok.Type != TokenType.Identifier)
										ThrowError("Expected an identifier for the full name of the prefix!", tok);
									PrefixRegistry.Register(wop, tok.Value);
									curIdx++;
								}
								else if (tok.Value == "DocAlias")
								{
									tok = toks[curIdx];
									if (tok.Type != TokenType.Identifier)
										ThrowError("Expected an identifier for the name of the doc alias!", tok);
									string docName = tok.Value;
									curIdx++;

									tok = toks[curIdx];
									if (tok.Type != TokenType.String)
										ThrowError("Expected a string!", tok);
									DocAliasRegistry.RegisterDocAlias(docName, tok.Value);
									curIdx++;
								}
								else
								{
									ThrowError("Expected either Type or SizelessType!", tok);
								}

								tok = toks[curIdx];
								if (tok.Type != TokenType.EOL)
									ThrowError("Expected EOL!", tok);
								curIdx++;
							}
							else
							{
								ThrowError("Unknown compiler directive " + tok.Value, tok);
							}
							break;
						case TokenType.DocumentationComment:
							if (tok.Value.Length >= 9 && tok.Value.Length <= 14)
							{
								string tmpS = tok.Value.ToLower();
								if (tmpS == "<summary>" ||
									tmpS == "</summary>"
								    )
								{
									// Ignore it.
									curIdx++;
									break;
								}
							}
							curDocList.Add(tok.Value);
							curIdx++;
							break;
						case TokenType.Identifier:
							string name = tok.Value;
							Instruction instr;
							if (!Instructions.TryGetValue(name, out instr))
							{
								instr = new Instruction(name);
								Instructions[name] = instr;
							}
							curIdx++;

							tok = toks[curIdx];
							if (tok.Type != TokenType.Number)
								ThrowError("Expected a number for the number of arguments!", tok);
							int argCount = (int)tok.NumberValue.Value;
							if (argCount > 3)
							{
								throw new Exception("Invalid arg count '" + argCount.ToString() + "'!");
							}
							curIdx++;
						
							InstructionArgSet argSet = new InstructionArgSet();
							argSet.Init();
							InstructionForm insForm = new InstructionForm(instr);
							insForm.Init();
							for (int i = 0; i < argCount; i++)
							{
								tok = toks[curIdx];
								if (tok.Type != TokenType.Identifier)
									ThrowError("Expected argument!", tok);
								argSet[i] = InstructionArgTypeRegistry.GetType(tok.Value);
								curIdx++;

								tok = toks[curIdx];
								if (tok.Type != TokenType.LParen)
									ThrowError("Expected opening parenthesis for the argument name!", tok);
								curIdx++;

								tok = toks[curIdx];
								if (tok.Type != TokenType.Identifier)
									ThrowError("Expected the name of the argument!", tok);
								string argName = tok.Value;
								curIdx++;

								tok = toks[curIdx];
								int? argDefault = null;
								if (tok.Type == TokenType.Equal)
								{
									curIdx++;

									tok = toks[curIdx];
									if (tok.Type != TokenType.Number)
										ThrowError("Expected the default value to be a number!", tok);
									argDefault = (int)tok.NumberValue.Value;
									curIdx++;

									tok = toks[curIdx];
								}
								if (tok.Type != TokenType.RParen)
									ThrowError("Expected a closing parenthesis after the argument name!", tok);
								curIdx++;

								tok = toks[curIdx];
								ImmNumberFormat numFormat = ImmNumberFormat.Hex;
								if (tok.Type == TokenType.LCurlBracket)
								{
									curIdx++;

									tok = toks[curIdx];
									if (tok.Type != TokenType.Identifier)
										ThrowError("Invalid token for the number format specifier!", tok);
									switch (tok.Value)
									{
										case "d":
											numFormat = ImmNumberFormat.Decimal;
											break;
										case "h":
											numFormat = ImmNumberFormat.Hex;
											break;
										default:
											ThrowError("Unknown number format '" + tok.Value + "'!", tok);
											break;
									}
									curIdx++;

									tok = toks[curIdx];
									if (tok.Type != TokenType.RCurlBracket)
										ThrowError("Expected a closing curly brace!", tok);
									curIdx++;

									tok = toks[curIdx];
								}
								if (i + 1 != argCount)
								{
									if (tok.Type != TokenType.Comma)
										ThrowError("Expected a comma seperator between arguments!", tok);
									curIdx++;
								}
								// otherwise the token isn't ours.

								insForm[i] = new InstructionArg()
								{
									ArgType = argSet[i],
									Name = argName,
									DefaultValue = argDefault,
									NumberFormat = numFormat
								};
							}

							tok = toks[curIdx];
							if (tok.Type == TokenType.Comma)
								ThrowError("Unexpected comma! (The parameter declarations were already complete)", tok);

							if (tok.Type != TokenType.LParen)
								ThrowError("Expected byte encoding!", tok);
							curIdx++;

							tok = toks[curIdx];
							while (tok.Type != TokenType.RParen)
							{
								switch (tok.Type)
								{
									case TokenType.Identifier:
									case TokenType.Number:
										if (toks[curIdx + 1].Type == TokenType.Plus)
										{
											insForm.WriteOperations.Add(new WriteOperation(new Token[] { tok, toks[curIdx + 1], toks[curIdx + 2] }, insForm));
											curIdx += 2;
										}
										else if (toks[curIdx + 1].Type == TokenType.LSqBracket)
										{
											int eIdx = curIdx + 2;
											int sIdx = curIdx;
											while (toks[eIdx].Type != TokenType.RSqBracket)
											{
												eIdx++;
											}
											insForm.WriteOperations.Add(new WriteOperation(toks.Slice(sIdx, eIdx + 1), insForm));
											curIdx = eIdx;
										}
										else
										{
											insForm.WriteOperations.Add(new WriteOperation(new Token[] { tok }, insForm));
										}
										break;
									default:
										ThrowError("Unknown token type for byte encoding!", tok);
										break;
								}

								curIdx++;
								tok = toks[curIdx];
							}
							curIdx++;

							// Only architectures with segments need this operand.
							if (StaticTypeReferences.SegmentLookup.Count > 1)
							{
								tok = toks[curIdx];
								if (tok.Type != TokenType.Identifier)
									ThrowError("Expected the default segment!", tok);
								if (!StaticTypeReferences.IsValidSegment(tok.Value))
								{
									throw new Exception("Unknown segment '" + tok.Value + "'!");
								}
								insForm.DefaultSegment = tok.Value;
								curIdx++;
							}

							tok = toks[curIdx];
							if (tok.Type != TokenType.Identifier)
								ThrowError("Expected the instruction's mnemonic!", tok);
							insForm.Mnemonic = tok.Value;
							curIdx++;

							tok = toks[curIdx];
							if (tok.Type == TokenType.LParen)
							{
								curIdx++;

								List<Token> excludeConditionTokens = new List<Token>(4);
								tok = toks[curIdx];
								while (tok.Type != TokenType.RParen)
								{
									excludeConditionTokens.Add(tok);
									curIdx++;
									tok = toks[curIdx];
								}
								if (excludeConditionTokens.Count == 0)
									throw new Exception("Expected an exclude condition, not a set of empty parenthesis!");
								insForm.ExcludeCondition = new ArgumentExcludeCondition(excludeConditionTokens);
								curIdx++;

								tok = toks[curIdx];
							}

							bool isOverride = false;
							CheckDocAlias:
							if (tok.Type != TokenType.EOL)
							{
								if (tok.Type != TokenType.Identifier)
									ThrowError("Unknown token after instruction mnemonic!", tok);
								switch (tok.Value)
								{
									case "override":
										curIdx++;

										tok = toks[curIdx];
										if (tok.Type != TokenType.LParen)
											ThrowError("Expected opening parenthesis for the override description!", tok);
										curIdx++;

										tok = toks[curIdx];
										if (tok.Type == TokenType.Identifier)
										{
											if (tok.Value != "exact")
												ThrowError("Unknown token, expected either 'exact' or a left square bracket!", tok);
											instr.ExactForms[argSet] = insForm;
											curIdx++;

											tok = toks[curIdx];
											if (tok.Type != TokenType.RParen)
												ThrowError("Expected close of parenthesis!", tok);
											curIdx++;

											tok = toks[curIdx];
										}
										else if (tok.Type == TokenType.LSqBracket)
										{
											curIdx++;

											InExactInstructionOverrideDescription desc = new InExactInstructionOverrideDescription();
											desc.ArgSetToOverride.Init();
											desc.NewForm = insForm;

											tok = toks[curIdx];
											int idx = 0;
											while (tok.Type != TokenType.RSqBracket)
											{
												if (tok.Type != TokenType.Identifier)
													ThrowError("Only identifiers are valid for form descriptions!", tok);
												desc.ArgSetToOverride[idx] = InstructionArgTypeRegistry.GetType(tok.Value);
												idx++;
												curIdx++;
												tok = toks[curIdx];
											}
											curIdx++;

											tok = toks[curIdx];
											while (tok.Type != TokenType.RParen)
											{
												List<Token> condToks = new List<Token>(8);
												if (tok.Type != TokenType.Identifier)
													ThrowError("Expected an identifier as the override condition!", tok);
												condToks.Add(tok);
												curIdx++;

												tok = toks[curIdx];
												if (tok.Type == TokenType.LSqBracket)
												{
													condToks.Add(tok);
													curIdx++;
													tok = toks[curIdx];
													while (tok.Type != TokenType.RSqBracket)
													{
														condToks.Add(tok);
														curIdx++;
														tok = toks[curIdx];
													}
													condToks.Add(tok);
													curIdx++;
													tok = toks[curIdx];
												}
												desc.ParseCondition(condToks);
											}
											curIdx++;
											tok = toks[curIdx];

											instr.InExactForms.Add(desc);
										}
										else
										{
											ThrowError("Unknown token type!", tok);
										}
										isOverride = true;
										goto CheckDocAlias;
									case "docalias":
										curIdx++;

										tok = toks[curIdx];
										if (tok.Type != TokenType.LParen)
											ThrowError("Expected opening left parenthesis!", tok);
										curIdx++;

										tok = toks[curIdx];
										if (tok.Type != TokenType.Identifier)
											ThrowError("Expected an instruction name!", tok);
										Instruction daInstr;
										if (!Instructions.TryGetValue(tok.Value, out daInstr))
											ThrowError("Unable to locate the instruction which contains the documenation! ('" + tok.Value + "')", tok);
										for (int i = 0; i < daInstr.LinesOfDocumentation.Count; i++)
										{
											curDocList.Add(daInstr.LinesOfDocumentation[i]);
										}
										curIdx++;

										tok = toks[curIdx];
										if (tok.Type != TokenType.RParen)
											ThrowError("Expected close of parenthesis!", tok);
										curIdx++;

										tok = toks[curIdx];
										break;
									default:
										ThrowError("Expected either the docalias or override directive!", tok);
										break;
								}
							}

							if (!isOverride)
							{
								if (instr.Forms.ContainsKey(argSet))
									throw new Exception("Duplicate instruction definition!");
								instr.Forms[argSet] = insForm;
							}

							if (curDocList.Count > 0)
							{
								if (instr.LinesOfDocumentation.Count > 0)
									throw new Exception("Attempted to redefine the documentation for " + instr.Name + "!");
								List<string> finalDocList;
								if (curDocList.Count > 2)
								{
									finalDocList = new List<string>(curDocList.Count - 2);
								}
								else
								{
									finalDocList = new List<string>();
								}

								foreach (string s in curDocList)
								{
									finalDocList.Add(s);
								}
								instr.LinesOfDocumentation = finalDocList;
								curDocList = new List<string>(InitialCurDocListSize);
							}
							
							tok = toks[curIdx];
							if (tok.Type != TokenType.EOL)
								ThrowError("Expected EOL!", tok);
							curIdx++;
							break;
						default:
							ThrowError("Unknown token type", tok);
							break;
					}

				}
			}
			catch (IndexOutOfRangeException)
			{
				throw new Exception("Unexpected end of token stream!");
			}
		}

		private void ProcessArgBase(Token[] toks, ref int curIdx, string argName, string argTrueName)
		{
			Token tok;

			tok = toks[curIdx];
			if (tok.Type != TokenType.Colon)
				ThrowError("Expected colon before the start of the arg base!", tok);
			curIdx++;

			tok = toks[curIdx];
			if (tok.Type != TokenType.Identifier)
				ThrowError("Expected an identifier representing the arg type to be the base!", tok);
			InstructionArgType baseType = InstructionArgTypeRegistry.GetType(tok.Value);
			curIdx++;

			InstructionArgType newType = new InstructionArgType()
			{
				Name = argName,
				IsAlias = baseType.IsAlias,
				AliasTo = baseType.AliasTo,
				AsArgOperation = baseType.AsArgOperation,
				ExpandsTo = baseType.ExpandsTo,
				Parameters = baseType.Parameters,
				ReadOperation = baseType.ReadOperation,
				RequiresSegment = baseType.RequiresSegment,
				SizelessType = baseType.SizelessType,
				TrueSizelessType = baseType.TrueSizelessType,
				WriteOperation = baseType.WriteOperation,
			};
			if (argTrueName != null)
				newType.TrueName = argTrueName;
			else 
				newType.TrueName = argName;

			tok = toks[curIdx];
			if (tok.Type == TokenType.LParen)
			{
				curIdx++;

				tok = toks[curIdx];
				if (tok.Type != TokenType.Number)
					ThrowError("Expected a number for the size of the new arg type!", tok);
				newType.Size = (int)tok.NumberValue.Value;
				curIdx++;

				tok = toks[curIdx];
				if (tok.Type != TokenType.RParen)
					ThrowError("Expected a closing parenthesis!", tok);
				curIdx++;

				tok = toks[curIdx];
			}

			if (tok.Type != TokenType.EOL)
				ThrowError("Expected EOL after based arg declaration!", tok);
			curIdx++;

			InstructionArgTypeRegistry.RegisterType(newType);
		}

		public static LanguageProviders.LanguageProvider LanguageProvider;

		public void WriteInstructions(string destDirectory, LanguageProviders.Language lang)
		{
			LanguageProvider = LanguageProviders.LanguageProvider.GetProvider(lang);
			LanguageProvider.RootNamespace = Namespace;

			StaticTypeReferences.InitializeTypes();
			EnumRegistry.WriteEnums(LanguageProvider.GetNamespace(Namespace));
			BitPatternRegistry.WritePatterns(LanguageProvider.GetNamespace(Namespace));

			string prevNamespaceName = null;
			CodeNamespace prevNamespace = null;
			foreach (Instruction i in Instructions.Values)
			{
				string newNamespaceName = Namespace + (i.GetNamespaceExtension() == "" ? "" : "." + i.GetNamespaceExtension());
				CodeNamespace n = prevNamespaceName == newNamespaceName ? prevNamespace : LanguageProvider.GetNamespace(newNamespaceName);
				i.Write(destDirectory, n);
				prevNamespace = n;
				prevNamespaceName = newNamespaceName;
			}

			// This has to be done after the instructions are written,
			// because otherwise they haven't requested their forms yet.
			InstructionFormEnumRegistry.WriteFormEnum(LanguageProvider.GetNamespace(Namespace));

			LanguageProvider.WriteToFile(destDirectory);
			LanguageProvider.Finish(destDirectory);
		}
	}
}

