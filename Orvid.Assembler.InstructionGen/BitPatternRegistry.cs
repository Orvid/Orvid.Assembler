using System;
using System.Reflection;
using System.Collections.Generic;
using Orvid.CodeDom;

namespace Orvid.Assembler.InstructionGen
{

	public sealed class BitPatternPieceOperation
	{
		public sealed class BitRange
		{
			public uint Length
			{
				get 
				{
					return EndIdx - StartIdx + 1;
				}
			}
			public readonly uint StartIdx;
			public readonly uint EndIdx;

			public BitRange(uint sIdx, uint eIdx)
			{
				this.StartIdx = sIdx;
				this.EndIdx = eIdx;
			}

			public BitRange(Token[] toks, ref int curIdx)
			{
				Token tok;
				tok = toks[curIdx];
				if (tok.Type != TokenType.Number)
					throw new Exception("Expected a number for the bit range!");
				EndIdx = tok.NumberValue.Value;
				curIdx++;

				tok = toks[curIdx];
				if (tok.Type == TokenType.Dot)
				{
					curIdx++;

					tok = toks[curIdx];
					if (tok.Type != TokenType.Dot)
						throw new Exception("Expected a second dot for the elips statement!");
					curIdx++;

					tok = toks[curIdx];
					if (tok.Type != TokenType.Number)
						throw new Exception("Expected a number for the start of the bit range!");
					StartIdx = tok.NumberValue.Value;
					curIdx++;

					tok = toks[curIdx];
				}
				else
				{
					StartIdx = 0;
					EndIdx--;
				}

				if (tok.Type != TokenType.RSqBracket)
					throw new Exception("Expected the closing of the square bracket!");
				curIdx++;
			}

			public CodeExpression GetExpression(CodeExpression src)
			{
				CodeExpression baseExpression = new CodeBinaryOperatorExpression(
					src,
					CodeBinaryOperatorType.BitwiseAnd,
					new CodePrimitiveExpression((uint)(((1u << (byte)(EndIdx - StartIdx + 1)) - 1) << (byte)StartIdx))
				);
				if (StartIdx != 0)
				{
					baseExpression = new CodeBinaryOperatorExpression(
						baseExpression,
						CodeBinaryOperatorType.ShiftRight,
						new CodePrimitiveExpression((int)StartIdx)
					);
				}
				return Utils.WrapInCast(
					baseExpression,
					(byte)Length
				);
			}
		}

		public sealed class BitPatternInvokeArgumentEntry
		{
			public readonly string Name;
			public readonly BitPatternPieceOperation ArgValue;

			public BitPatternInvokeArgumentEntry(BitPattern parent, Token[] toks, ref int curIdx)
			{
				Token tok;
				tok = toks[curIdx];
				if (tok.Type != TokenType.Identifier)
					throw new Exception("Expected an identifier for the name of the argument to map to!");
				Name = tok.Value;
				curIdx++;

				tok = toks[curIdx];
				if (tok.Type != TokenType.Equal)
					throw new Exception("Expected an equals sign before the value to pass for the argument!");
				curIdx++;

				ArgValue = new BitPatternPieceOperation(parent, toks, ref curIdx);
			}
		}

		public enum OperationType
		{
			BinaryOp,
			Literal,
			Arg,
			PatternInvoke,
		}

		public readonly int ArgIdx;
		public readonly OperationType OpType;
		public readonly BitRange Range;
		public readonly uint Literal;
		public readonly BitPatternPieceOperation LHand;
		public readonly BitPatternPieceOperation RHand;
		public readonly CodeBinaryOperatorType BinaryOp;
		public readonly BitPattern InvokedPattern;
		public readonly List<BitPatternInvokeArgumentEntry> InvokedPatternArgMapping;
		private readonly BitPattern Parent;

		public uint Length
		{
			get { return OpType == OperationType.PatternInvoke ? InvokedPattern.TotalLength : Range.Length; }
		}

		public BitPatternPieceOperation(BitPattern parent, Token[] toks, ref int curIdx)
		{
			Parent = parent;
			Token tok;
			tok = toks[curIdx];
			if (tok.Type == TokenType.Identifier)
			{
				OpType = OperationType.Arg;
				if (!int.TryParse(tok.Value.Substring(1), out ArgIdx))
					throw new Exception("Unknown argument '" + tok.Value + "'!");
				curIdx++;
				tok = toks[curIdx];
				if (tok.Type != TokenType.LSqBracket)
					throw new Exception("Expected the opening square bracket for the bit pattern operation selector!");
				curIdx++;
				Range = new BitRange(toks, ref curIdx);
				parent.RequestArg(ArgIdx, Range.EndIdx);
			}
			else if (tok.Type == TokenType.Number)
			{
				OpType = OperationType.Literal;
				Literal = tok.NumberValue.Value;
				Number n = tok.NumberValue;
				curIdx++;

				tok = toks[curIdx];
				if (n.Format == NumberFormat.Binary)
				{
					if (tok.Type != TokenType.LSqBracket)
					{
						Range = new BitRange(0, n.LiteralLength - 1);
					}
					else
					{
						curIdx++;
						Range = new BitRange(toks, ref curIdx);
					}
				}
				else if (tok.Type != TokenType.LSqBracket)
				{
					Range = new BitRange(0, Utils.GetHighestBitIndexSet(Literal));
				}
				else
				{
					curIdx++;
					Range = new BitRange(toks, ref curIdx);
				}
			}
			else if (tok.Type == TokenType.LParen)
			{
				curIdx++;
				OpType = OperationType.BinaryOp;

				tok = toks[curIdx];
				switch(tok.Type)
				{
					case TokenType.Star:
						BinaryOp = CodeBinaryOperatorType.Multiply;
						break;
					case TokenType.Plus:
						BinaryOp = CodeBinaryOperatorType.Add;
						break;
					default:
						throw new Exception("Unknown math operation to perform!");
				}
				curIdx++;

				LHand = new BitPatternPieceOperation(parent, toks, ref curIdx);
				RHand = new BitPatternPieceOperation(parent, toks, ref curIdx);

				tok = toks[curIdx];
				if (tok.Type != TokenType.RParen)
					throw new Exception("Expected a closing parenthesis!");
				curIdx++;

				tok = toks[curIdx];
				if (tok.Type != TokenType.LSqBracket)
				{
					Range = new BitRange(0, 32);
				}
				else
				{
					curIdx++;
					Range = new BitRange(toks, ref curIdx);
				}
			}
			else if (tok.Type == TokenType.Cash)
			{
				InvokedPatternArgMapping = new List<BitPatternInvokeArgumentEntry>();
				OpType = OperationType.PatternInvoke;
				curIdx++;

				tok = toks[curIdx];
				if (tok.Type != TokenType.Identifier)
					throw new Exception("Expected an identifier for the bit pattern to invoke!");
				InvokedPattern = BitPatternRegistry.GetPattern(tok.Value);
				curIdx++;

				tok = toks[curIdx];
				if (tok.Type != TokenType.LSqBracket)
					throw new Exception("Expected a left square bracket for the arguments to the bit operation!");
				curIdx++;

				tok = toks[curIdx];
				bool expectsArg = true;
				bool first = true;
				while(tok.Type != TokenType.RSqBracket)
				{
					if (!expectsArg)
						throw new Exception("Expected a closing square bracket for the arguments to the bit operation!");
					InvokedPatternArgMapping.Add(new BitPatternInvokeArgumentEntry(parent, toks, ref curIdx));

					tok = toks[curIdx];
					if (tok.Type == TokenType.Comma)
					{
						curIdx++;
						tok = toks[curIdx];
					}
					else
					{
						expectsArg = false;
					}
					first = false;
				}
				if (expectsArg && !first)
					throw new Exception("Expected another argument!");
				curIdx++;
			}
			else
			{
				throw new Exception("Unsupported bit pattern operation!");
			}
		}

		public CodeExpression GetExpression()
		{
			switch (OpType)
			{
				case OperationType.BinaryOp:
					return new CodeBinaryOperatorExpression(
						LHand.GetExpression(),
						BinaryOp,
						RHand.GetExpression()
					);
				case OperationType.Arg:
					return Range.GetExpression(new CodeArgumentReferenceExpression(Parent.GetArg(ArgIdx).Name));
				case OperationType.Literal:
					return new CodePrimitiveExpression(Literal);
				case OperationType.PatternInvoke:
					List<CodeExpression> args = new List<CodeExpression>();
					args.Add(StaticTypeReferences.Emit_StreamArg);
					for (int i = 0; i < this.InvokedPattern.Args.Count; i++)
					{
						BitPatternInvokeArgumentEntry arg = null;
						bool found = false;
						for (int i2 = 0; i2 < this.InvokedPatternArgMapping.Count; i2++)
						{
							if (InvokedPatternArgMapping[i2].Name == this.InvokedPattern.Args[i].Name)
							{
								arg = InvokedPatternArgMapping[i2];
								found = true;
								break;
							}
						}
						if (!found)
						{
							args.Add(new CodeArgumentReferenceExpression(Parent.GetArg(Parent.CurrentArgBase).Name));
							Parent.CurrentArgBase++;
						}
						else
						{
							args.Add(Utils.WrapInCast(arg.ArgValue.GetExpression(), (byte)arg.ArgValue.Length, true));
						}
					}
					return new CodeMethodInvokeExpression(
						new CodeTypeReferenceExpression("BitPatterns"),
						InvokedPattern.Name,
						args.ToArray()
					);
				default:
					throw new Exception("Unknown operation type!");
			}
		}
	}

	public sealed class BitPatternPiece
	{
		public readonly List<BitPatternPieceOperation> Operations = new List<BitPatternPieceOperation>();
		public readonly uint TotalLength;

		public BitPatternPiece(BitPattern parent, Token[] toks, ref int curIdx)
		{
			Token tok;
			tok = toks[curIdx];
			while (tok.Type != TokenType.RParen)
			{
				var bpp = new BitPatternPieceOperation(parent, toks, ref curIdx);
				TotalLength += bpp.Length;
				Operations.Add(bpp);
				tok = toks[curIdx];
			}
			curIdx++;
		}

		public void Write(CodeMemberMethod mthd)
		{
			if (Operations.Count == 1 && Operations[0].OpType == BitPatternPieceOperation.OperationType.PatternInvoke)
			{
				mthd.Statements.Add(Operations[0].GetExpression());
			}
			else
			{    
				CodeExpression expr = null;
				uint curLength = 0;

				for(int i = Operations.Count - 1; i >= 0; i--)
				{
					if (expr == null)
					{
						expr = Utils.WrapInCast(Operations[i].GetExpression(), (byte)TotalLength);
					}
					else
					{
						expr = new CodeBinaryOperatorExpression(
							Utils.WrapInCast(
								new CodeBinaryOperatorExpression(
									Utils.WrapInCast(Operations[i].GetExpression(), (byte)TotalLength),
									CodeBinaryOperatorType.ShiftLeft,
									new CodePrimitiveExpression((int)curLength)
								),
								(byte)TotalLength
							),
							CodeBinaryOperatorType.BitwiseOr,
							expr
						);
					}
					curLength += Operations[i].Length;
				}

				expr = Utils.WrapInCast(expr, (byte)TotalLength);

				CodeExpression stat = null;
				switch (TotalLength)
				{
					case 0:
						stat = null;
						break;
					case 8:
						stat = new CodeMethodInvokeExpression(
							StaticTypeReferences.Emit_StreamArg,
							"WriteImm8",
							Utils.WrapInCast(expr, 8)
						);
						break;
					case 16:
						stat = new CodeMethodInvokeExpression(
							StaticTypeReferences.Emit_StreamArg,
							"WriteImm16",
							Utils.WrapInCast(expr, 16)
						);
						break;
					case 32:
						stat = new CodeMethodInvokeExpression(
							StaticTypeReferences.Emit_StreamArg,
							"WriteImm32",
							Utils.WrapInCast(expr, 32)
						);
						break;
					default:
						throw new Exception("Unknown total size for a bit pattern piece! The total number of bits must be 8, 16 or 32!");
				}
				if (stat != null)
				{
					mthd.Statements.Add(stat);
				}
			}
		}
	}

	public sealed class BitPatternArgument
	{
		// This is in bits.
		public uint Size;
		public readonly string Name;

		public BitPatternArgument(string name, uint size)
		{
			this.Name = name;
			this.Size = size;
		}
	}

	public sealed class BitPattern
	{
		public string Name { get; private set; }
		public List<BitPatternPiece> Pieces = new List<BitPatternPiece>();
		public List<BitPatternArgument> Args = new List<BitPatternArgument>();
		public readonly uint TotalLength;
		public int CurrentArgBase = 1;

		public BitPattern(Token[] toks, ref int curIdx)
		{
			Token tok;
			tok = toks[curIdx];
			if (tok.Type != TokenType.Identifier)
				throw new Exception("Expected an identifier for the name of the bit pattern!");
			this.Name = tok.Value;
			curIdx++;

			tok = toks[curIdx];
			while (tok.Type == TokenType.LParen)
			{
				curIdx++;
				BitPatternPiece pc = new BitPatternPiece(this, toks, ref curIdx);
				TotalLength += pc.TotalLength;
				Pieces.Add(pc);
				tok = toks[curIdx];
			}

			if (tok.Type != TokenType.EOL)
				throw new Exception("Expected an EOL at the end of the bit pattern!");
			curIdx++;
		}

		public BitPatternArgument GetArg(int idx)
		{
			return Args[idx - 1];
		}

		public BitPatternArgument RequestArg(int idx, uint maxBitIdx)
		{
			idx--;
			while (Args.Count <= idx)
			{
				Args.Add(new BitPatternArgument("i" + (Args.Count + 1).ToString(), 0));
			}
			BitPatternArgument arg = Args[idx];
			if (arg.Size < maxBitIdx + 1)
				arg.Size = maxBitIdx + 1;
			return arg;
		}

		public void Write(CodeTypeDeclaration td)
		{
			CodeMemberMethod mthd = new CodeMemberMethod();
			mthd.Name = Name;
			mthd.Attributes = MemberAttributes.Public;
			mthd.Attributes |= MemberAttributes.Static;
			mthd.Parameters.Add(new CodeParameterDeclarationExpression(StaticTypeReferences.Stream, StaticTypeReferences.Emit_StreamArgName));
			foreach (BitPatternArgument arg in Args)
			{
				CodeTypeReference pt = null;
				if (arg.Size <= 8)
					pt = StaticTypeReferences.Byte;
				else if (arg.Size <= 16)
					pt = StaticTypeReferences.UShort;
				else if (arg.Size <= 32)
					pt = StaticTypeReferences.UInt;
				else if (arg.Size <= 64)
					pt = StaticTypeReferences.ULong;
				else
					throw new Exception("Geeze, that's a giant argument!");
				mthd.Parameters.Add(new CodeParameterDeclarationExpression(pt, arg.Name));
			}

			foreach (BitPatternPiece pc in Pieces)
			{
				pc.Write(mthd);
			}

			td.Members.Add(mthd);
		}
	}

	public static class BitPatternRegistry
	{
		private static Dictionary<string, BitPattern> Patterns;

		static BitPatternRegistry()
		{
			Initialize();
		}

		private static void Initialize()
		{
			Patterns = new Dictionary<string, BitPattern>();
		}

		public static void Reset()
		{
			Initialize();
		}

		public static void RegisterBitPattern(BitPattern pat)
		{
			if (Patterns.ContainsKey(pat.Name))
				throw new Exception("The BitPattern '" + pat.Name + "' was already defined!");
			Patterns[pat.Name] = pat;
		}

		public static void RegisterAlias(string aliasName, BitPattern pat)
		{
			if (Patterns.ContainsKey(aliasName))
				throw new Exception("The BitPattern '" + aliasName + "' was already defined!");
			Patterns[aliasName] = pat;
		}

		public static BitPattern GetPattern(string name)
		{
			BitPattern pat;
			if (!Patterns.TryGetValue(name, out pat))
				throw new Exception("Unknown bit pattern '" + name + "'!");
			return pat;
		}

		public static void WritePatterns(CodeNamespace nmspc)
		{
			if (Patterns.Count > 0)
			{
				CodeTypeDeclaration td = new CodeTypeDeclaration("BitPatterns");
				td.Attributes = MemberAttributes.Static;
				td.Attributes |= MemberAttributes.Public;
				td.TypeAttributes = TypeAttributes.Class;
				td.TypeAttributes |= TypeAttributes.Public;

				// Deal with aliases.
				Dictionary<BitPattern, bool> pats = new Dictionary<BitPattern, bool>();

				foreach (BitPattern pat in Patterns.Values)
				{
					if (!pats.ContainsKey(pat))
					{
						pat.Write(td);
						pats[pat] = true;
					}
				}
				
				nmspc.Types.Add(td);
			}
		}
	}
}

