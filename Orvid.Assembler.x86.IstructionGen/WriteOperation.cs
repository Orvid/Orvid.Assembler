using System;
using System.IO;
using Orvid.CodeDom;

namespace Orvid.Assembler.x86.IstructionGen
{
	internal enum WriteOperationType
	{
		Byte,
		BytePlusArg,
		Arg,
		Imm8,
		Imm16,
		Imm32,
		Prefix,
		/// <summary>
		/// This causes an exception to be thrown if this point is actually reached. (in the generated code)
		/// </summary>
		Throw,
	}

	public class WriteOperation
	{
		internal WriteOperationArgument WriteArgument;
		private byte ByteValue;
		internal WriteOperationType Type;
		private string SelectedPrefix;
		private byte ArgIdx;
		private string MessageThrown;

		public WriteOperation(Token[] toks, InstructionForm ParentForm)
		{
			// As a note, all operations should be 4 characters,
			// if they are unable to be 4 characters, then they
			// should be sets of 4 characters seperated in
			// some way. (this allows for the write operations to
			// be aligned in the cpud file)
			Token tok = toks[0];
			switch (toks.Length)
			{
				case 1:
					if (tok.Value.StartsWith("arg"))
					{
						this.ArgIdx = (byte)(Utils.SingleDigitParse(tok.Value[3]) - 1);
						if (!ParentForm[ArgIdx].ArgType.HasSize)
						{
							this.Type = WriteOperationType.Arg;
						}
						else
						{
							switch(ParentForm[ArgIdx].ArgType.Size)
							{
								case 1:
									this.Type = WriteOperationType.Imm8;
									break;
								case 2:
									this.Type = WriteOperationType.Imm16;
									break;
								case 4:
									this.Type = WriteOperationType.Imm32;
									break;
								default:
									throw new Exception("Unknown arg type!");
							}
						}
					}
					else if (tok.Value.StartsWith("0x"))
					{
						this.Type = WriteOperationType.Byte;
						this.ByteValue = byte.Parse(tok.Value.Substring(2), System.Globalization.NumberStyles.AllowHexSpecifier);
					}
					else // Prefix
					{
						this.Type = WriteOperationType.Prefix;
						this.SelectedPrefix = PrefixRegistry.GetPrefixName(tok.Value);
						if (this.SelectedPrefix == null)
							throw new Exception("Unknown write operation '" + tok.Value + "'!");
					}
					break;
				case 3:
					this.Type = WriteOperationType.BytePlusArg;
					this.ArgIdx = (byte)(Utils.SingleDigitParse(toks[2].Value[3]) - 1);
					this.ByteValue = byte.Parse(tok.Value.Substring(2), System.Globalization.NumberStyles.AllowHexSpecifier);
					break;
				case 4:
					if (tok.Value == "evil")
					{
						if (toks[1].Type != TokenType.LSqBracket)
							throw new Exception("Expected an opening square bracket before the exception's message!");
						if (toks[3].Type != TokenType.RSqBracket)
							throw new Exception("Expected the closing square bracket after the exception's message!");
						this.Type = WriteOperationType.Throw;
						this.MessageThrown = toks[2].Value;
					}
					else if (tok.Value.StartsWith("arg"))
					{
						if (toks[1].Type != TokenType.LSqBracket)
							throw new Exception("Expected an opening square bracket before the argument write operation!");
						if (toks[3].Type != TokenType.RSqBracket)
							throw new Exception("Expected the closing square bracket after the argument write operation!");
						this.ArgIdx = (byte)(Utils.SingleDigitParse(tok.Value[3]) - 1);
						this.Type = WriteOperationType.Arg;
						this.WriteArgument = new WriteOperationArgument(toks[2]);
					}
					else
					{
						throw new Exception("Unknown write operation type!");
					}
					break;
				default:
					throw new Exception("Unknown number of tokens passed in!");
			}
		}

		public void Write(CodeScopeStatement con, InstructionForm ParentForm)
		{
			switch (Type)
			{
				case WriteOperationType.Arg:
					con.Statements.Add(
						ParentForm[ArgIdx].ArgType.WriteOperation.GetExpression(ParentForm, ArgIdx, this)
					);
					break;
				case WriteOperationType.Prefix:
					con.Statements.Add(
						new CodeMethodInvokeExpression(
							StaticTypeReferences.Emit_Stream_WritePrefix,
							new CodeFieldReferenceExpression(StaticTypeReferences.PrefixExpression, SelectedPrefix)
						)
					);
					break;
				case WriteOperationType.Byte:
					con.Statements.Add(
						new CodeMethodInvokeExpression(
							StaticTypeReferences.Emit_Stream_WriteByte,
							new CodePrimitiveExpression(ByteValue)
						)
					);
					break;
				case WriteOperationType.BytePlusArg:
					con.Statements.Add(
						new CodeMethodInvokeExpression(
							StaticTypeReferences.Emit_Stream_WriteByte,
							new CodeCastExpression(
								StaticTypeReferences.Byte,
								new CodeBinaryOperatorExpression(
									new CodePrimitiveExpression(ByteValue),
									CodeBinaryOperatorType.Add,
									ParentForm[ArgIdx].ArgType.AsArgOperation.GetExpression(ParentForm, ArgIdx, this)
								)
							)
						)
					);
					break;
				case WriteOperationType.Imm8:
					con.Statements.Add(
						new CodeMethodInvokeExpression(
							StaticTypeReferences.Emit_Stream_WriteImm8,
							new CodeCastExpression(
								StaticTypeReferences.Byte,
								new CodeFieldReferenceExpression(
									new CodeThisReferenceExpression(),
									ParentForm.GetArgName(FieldTypeRegistry.UInt.ID, 1, ArgIdx)
								)
							)
						)
					);
					break;
				case WriteOperationType.Imm16:
					con.Statements.Add(
						new CodeMethodInvokeExpression(
							StaticTypeReferences.Emit_Stream_WriteImm16,
							new CodeCastExpression(
								StaticTypeReferences.UShort,
								new CodeFieldReferenceExpression(
									new CodeThisReferenceExpression(),
									ParentForm.GetArgName(FieldTypeRegistry.UInt.ID, 1, ArgIdx)
								)
							)
						)
					);
					break;
				case WriteOperationType.Imm32:
					con.Statements.Add(
						new CodeMethodInvokeExpression(
							StaticTypeReferences.Emit_Stream_WriteImm32,
							new CodeFieldReferenceExpression(
								new CodeThisReferenceExpression(),
								ParentForm.GetArgName(FieldTypeRegistry.UInt.ID, 1, ArgIdx)
							)
						)
					);
					break;
				case WriteOperationType.Throw:
					con.Statements.Add(
						new CodeThrowExceptionStatement(
							new CodeObjectCreateExpression(
								StaticTypeReferences.Exception,
								new CodePrimitiveExpression(MessageThrown)
							)
						)
					);
					break;
				default:
					throw new Exception("Unsupported write operation type!");
			}
		}
	}
}

