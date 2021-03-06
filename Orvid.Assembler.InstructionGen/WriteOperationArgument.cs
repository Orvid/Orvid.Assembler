using System;
using Orvid.CodeDom;

namespace Orvid.Assembler.InstructionGen
{
	public sealed class WriteOperationArgument
	{
		private enum OperationSourceType
		{
			Constant,
			Argument,
		}
		private OperationSourceType Type;
		private byte Constant;
		private byte ParentArgIdx;

		public WriteOperationArgument(Token tok)
		{
			if (tok.Type == TokenType.Identifier)
			{
				if (tok.Value.StartsWith("arg"))
				{
					Type = OperationSourceType.Argument;
					ParentArgIdx = (byte)(Utils.SingleDigitParse(tok.Value[3]) - 1);
				}
				else
				{
					throw new Exception("Unknown identifier for a write operation argument!");
				}
			}
			else
			{
				Type = OperationSourceType.Constant;
				if (tok.Type != TokenType.Number)
					throw new Exception("Expected either an argument or a number!");
				Constant = (byte)tok.NumberValue.Value;
			}
		}
		private WriteOperationArgument() { }

		public WriteOperationArgument DeepCopy()
		{
			WriteOperationArgument e = new WriteOperationArgument();
			e.Type = Type;
			e.Constant = Constant;
			e.ParentArgIdx = ParentArgIdx;
			return e;
		}

		public CodeExpression GetLoadExpression(InstructionForm frm)
		{
			if (Type == OperationSourceType.Constant)
			{
				return new CodePrimitiveExpression(Constant);
			}
			else
			{
				switch(frm[ParentArgIdx].ArgType.ID)
				{
					case InstructionArgType.Imm32_ID:
						return new CodeFieldReferenceExpression(
							new CodeThisReferenceExpression(),
							frm.GetArgName(FieldTypeRegistry.UInt.ID, 1, ParentArgIdx)
						);
					case InstructionArgType.Imm16_ID:
						return new CodeCastExpression(
							StaticTypeReferences.UShort,
							new CodeFieldReferenceExpression(
								new CodeThisReferenceExpression(),
								frm.GetArgName(FieldTypeRegistry.UInt.ID, 1, ParentArgIdx)
							)
						);
					case InstructionArgType.Imm8_ID:
						return new CodeCastExpression(
							StaticTypeReferences.Byte,
							new CodeFieldReferenceExpression(
								new CodeThisReferenceExpression(),
								frm.GetArgName(FieldTypeRegistry.UInt.ID, 1, ParentArgIdx)
							)
						);

					default:
						if (frm[ParentArgIdx].ArgType.AsArgOperation == null)
							throw new Exception("The specified arg isn't valid as an argument to a write operation!");
						return frm[ParentArgIdx].ArgType.AsArgOperation.GetExpression(frm, ParentArgIdx, null);
				}
			}
		}
	}
}

