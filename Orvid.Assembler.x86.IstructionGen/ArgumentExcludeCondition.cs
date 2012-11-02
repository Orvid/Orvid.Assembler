using System;
using System.Collections.Generic;
using Orvid.CodeDom;

namespace Orvid.Assembler.x86.IstructionGen
{
	public class ArgumentExcludeCondition
	{
		private enum ConditionType
		{
			NotEqual,
			Equal,
			Greater,
			GreaterOrEqual,
			Less,
			LessOrEqual
		}
		private int ArgToExclude;
		private ConditionType Condition;
		private uint ConditionArg;

		/// <summary>
		/// Creates a deep copy of this object.
		/// </summary>
		public ArgumentExcludeCondition DeepCopy()
		{
			ArgumentExcludeCondition e = new ArgumentExcludeCondition();
			e.ArgToExclude = ArgToExclude;
			e.Condition = Condition;
			e.ConditionArg = ConditionArg;
			return e;
		}

		private ArgumentExcludeCondition() { }

		public ArgumentExcludeCondition(List<Token> toks)
		{
			Token tok = toks[0];
			if (tok.Type != TokenType.Identifier)
				throw new Exception("Unknown token for argument to exclude condition!");
			ArgToExclude = Utils.SingleDigitParse(tok.Value[3]) - 1;
			if (ArgToExclude != 0)
				throw new Exception("Cannot exclude anything but the first argument!");
			tok = toks[1];
			int nextTokIdx = 2;
			switch (tok.Type)
			{
				case TokenType.LThan:
					if (toks[2].Type == TokenType.Equal)
					{
						nextTokIdx++;
						Condition = ConditionType.LessOrEqual;
					}
					else
					{
						Condition = ConditionType.Less;
					}
					break;
				case TokenType.GThan:
					if (toks[2].Type == TokenType.Equal)
					{
						nextTokIdx++;
						Condition = ConditionType.GreaterOrEqual;
					}
					else
					{
						Condition = ConditionType.Greater;
					}
					break;
				case TokenType.Equal:
					if (toks[2].Type != TokenType.Equal)
						throw new Exception("Unknown condition for an argument exclude!");
					nextTokIdx++;
					Condition = ConditionType.Equal;
					break;
				case TokenType.Exclaim:
					if (toks[2].Type != TokenType.Equal)
						throw new Exception("Unknown condition for an argument exclude!");
					nextTokIdx++;
					Condition = ConditionType.NotEqual;
					break;

				default:
					throw new Exception("Unknown condition for an argument exclude!");
			}
			tok = toks[nextTokIdx];
			if (tok.Type != TokenType.DecimalNumber)
				throw new Exception("The value being compared to in an argument exclude condition must be a decimal number!");
			ConditionArg = uint.Parse(tok.Value);
		}

		public CodeExpression GetConditionExpression(InstructionForm parent)
		{
			CodeBinaryOperatorType cond;
			switch (Condition)
			{
				case ConditionType.Less:
					cond = CodeBinaryOperatorType.LessThan;
					break;
				case ConditionType.LessOrEqual:
					cond = CodeBinaryOperatorType.LessThanOrEqual;
					break;
				case ConditionType.Greater:
					cond = CodeBinaryOperatorType.GreaterThan;
					break;
				case ConditionType.GreaterOrEqual:
					cond = CodeBinaryOperatorType.GreaterThanOrEqual;
					break;
				case ConditionType.Equal:
					cond = CodeBinaryOperatorType.ValueEquality;
					break;
				case ConditionType.NotEqual:
					cond = CodeBinaryOperatorType.IdentityInequality;
					break;
				default:
					throw new Exception("Unknown condition!");
			}
			return new CodeBinaryOperatorExpression(
				new CodeFieldReferenceExpression(
					new CodeThisReferenceExpression(),
					parent.GetArgName(FieldTypeRegistry.UInt.ID, 1, ArgToExclude)
				),
				cond,
				new CodePrimitiveExpression(ConditionArg)
			);
		}
	}
}

