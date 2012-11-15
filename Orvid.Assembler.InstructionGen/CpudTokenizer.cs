using System;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Orvid.Assembler.InstructionGen
{
	public enum TokenType
	{
		// First are the token type who's value
		// field is null.
		LCurlBracket,
		RCurlBracket,
		LSqBracket,
		RSqBracket,
		LParen,
		RParen,
		Dot,
		Plus,
		Minus,
		Equal,
		LThan,
		GThan,
		Exclaim,
		Comma,
		Sharp,
		Colon,
		Cash,
		Star,
		EOL,

		Identifier,
		DocumentationComment,
		Number,
		String,
	}

	/// <summary>
	/// The different formats a number can be in.
	/// </summary>
	public enum NumberFormat : int
	{
		// Keep these ordered by their bases.
		// (aka. decimal is base 10, and hexadecimal is base 16)
		/// <summary>
		/// Represents an unsigned base 2 number.
		/// </summary>
		Binary,
		/// <summary>
		/// Represents an unsigned base 8 number.
		/// </summary>
		Octal,
		/// <summary>
		/// Represents a signed base 10 number.
		/// </summary>
		Decimal,
		/// <summary>
		/// Represents an unsigned base 16 number.
		/// </summary>
		Hexadecimal,
	}

	/// <summary>
	/// Represents a number which can be in one
	/// of many formats.
	/// </summary>
	public sealed class Number
	{
		/// <summary>
		/// The format that this number was in.
		/// </summary>
		public readonly NumberFormat Format;
		/// <summary>
		/// A <see cref="uint"/> representing the
		/// value that the number represents. This
		/// value should be casted to an <see cref="int"/>
		/// if the <see cref="Format"/> is <see cref="Format.Decimal"/>.
		/// </summary>
		public readonly uint Value;
		/// <summary>
		/// The length of this number when it was written.
		/// Only valid if the <see cref="Format"/> is <see cref="Format.Binary"/>.
		/// </summary>
		public readonly uint LiteralLength;

		public Number(NumberFormat format, uint val, uint len = 0)
		{
			this.Format = format;
			this.Value = val;
			this.LiteralLength = len;
		}
	}

	// The reason this is a struct is because
	// there can easily be 16k tokens that get
	// generated, and that adds quite a bit of
	// GC pressure, increasing generation time
	// by a surprising ammount.
	[StructLayout(LayoutKind.Sequential)]
	public struct Token
	{
		// We have the ability to track the end
		// of the token as well, (the code is actually
		// all still there) but that extra
		// 8 bytes actually manages to increase
		// parse time by 1/3rd
		public readonly TokenType Type;
		public readonly string Value;
		public readonly Number NumberValue;
		public readonly int StartLine;
		public readonly int StartColumn;
		public Token(string value, TokenType tp, int sLine, int sColumn, Number numVal = null)
		{
			this.Value = value;
			this.Type = tp;
			this.StartLine = sLine;
			this.StartColumn = sColumn;
			this.NumberValue = numVal;
		}
		
		public override string ToString()
		{
			return Type.ToString() + ": " + Value + " @ " + StartLine.ToString() + ":" + StartColumn.ToString()/* + " to " + EndLine.ToString() + ":" + EndColumn.ToString()*/;
		}
	}

	public static class CpudTokenizer
	{

		private enum TokenizerState
		{
			Unknown,
			Identifier,
			UnknownNumber,
			DecimalNumber,
			HexadecimalNumber,
			BinaryNumber,
			MaybeComment,
			UnknownComment,
			SingleLineComment,
			DocumentationComment,
			DocumentationCommentLeadingSpace,
			ProbablyDocumentationComment,
			String,
		}

		private static bool ShouldCreateEOLToken(TokenType prevTokenType)
		{
			switch (prevTokenType)
			{
				case TokenType.Comma:
				case TokenType.LCurlBracket:
				//case TokenType.RCurlBracket:
				case TokenType.EOL:
				case TokenType.DocumentationComment:
					return false;
				default:
					return true;
			}
		}

		private const int InitialTokenListCapacity = 1024 * 32; // 32Kb
		public static unsafe Token[] Tokenize(string value)
		{
			List<Token> toks = new List<Token>(InitialTokenListCapacity);
			fixed(char* vl2 = value)
			{
				char* val = vl2;
				char* end = &val[value.Length];
				char* tokenStart = val;
				int tokStartLine = 0;
				int tokStartColumn = 0;
				const int StartLine = 1;
				const int StartColumn = 1;
				int curLine = StartLine;
				int curColumn = StartColumn;
				TokenizerState state = TokenizerState.Unknown;
				while (val < end)
				{
					switch(state)
					{
						case TokenizerState.Unknown:
							switch(*val)
							{
								case ' ':
								case '\t':
									// These characters are ignored.
									break;
								
								// Only 1 EOL token will ever be be generated in a row,
								// no matter how many actual newlines are between tokens.
								case '\r':
									if (val + 1 < end && val[1] == '\n')
									{
										// Skip the second character if it is a
										// Windows-style line ending. (CRLF)
										val++;
									}

									if (toks.Count > 0 && ShouldCreateEOLToken(toks[toks.Count - 1].Type))
									{
										toks.Add(new Token(null, TokenType.EOL, curLine, curColumn));
									}
									curLine++;
									// We set the current column to StartColumn - 1 because
									// the code after this switch will add 1 to the current
									// column, meaning the final value will be StartColumn.
									curColumn = StartColumn - 1;
									break;
								case '\n':
									if (toks.Count > 0 && ShouldCreateEOLToken(toks[toks.Count - 1].Type))
									{
										toks.Add(new Token(null, TokenType.EOL, curLine, curColumn));
									}
									curLine++;
									// See above
									curColumn = StartColumn - 1;
									break;

								#region Single Character Tokens

								case '[':
									toks.Add(new Token(null, TokenType.LSqBracket, curLine, curColumn));
									break;
								case ']':
									toks.Add(new Token(null, TokenType.RSqBracket, curLine, curColumn));
									break;
								case '(':
									toks.Add(new Token(null, TokenType.LParen, curLine, curColumn));
									break;
								case ')':
									toks.Add(new Token(null, TokenType.RParen, curLine, curColumn));
									break;
								case '{':
									toks.Add(new Token(null, TokenType.LCurlBracket, curLine, curColumn));
									break;
								case '}':
									toks.Add(new Token(null, TokenType.RCurlBracket, curLine, curColumn));
									break;
								case '.':
									toks.Add(new Token(null, TokenType.Dot, curLine, curColumn));
									break;
								case '+':
									toks.Add(new Token(null, TokenType.Plus, curLine, curColumn));
									break;
								case '-':
									toks.Add(new Token(null, TokenType.Minus, curLine, curColumn));
									break;
								// SPEED: Need to eventually create ==, <=, >=, -=, and += tokens, that
								//        way fewer tokens are needed.
								case '=':
									toks.Add(new Token(null, TokenType.Equal, curLine, curColumn));
									break;
								case '<':
									toks.Add(new Token(null, TokenType.LThan, curLine, curColumn));
									break;
								case '>':
									toks.Add(new Token(null, TokenType.GThan, curLine, curColumn));
									break;
								case '!':
									toks.Add(new Token(null, TokenType.Exclaim, curLine, curColumn));
									break;
								case ',':
									toks.Add(new Token(null, TokenType.Comma, curLine, curColumn));
									break;
								case '#':
									toks.Add(new Token(null, TokenType.Sharp, curLine, curColumn));
									break;
								case '$':
									toks.Add(new Token(null, TokenType.Cash, curLine, curColumn));
									break;
								case '*':
									toks.Add(new Token(null, TokenType.Star, curLine, curColumn));
									break;
								case ':':
									toks.Add(new Token(null, TokenType.Colon, curLine, curColumn));
									break;

								#endregion

								case '/':
									state = TokenizerState.MaybeComment;
									break;
								case '0':
								case '1':
								case '2':
								case '3':
								case '4':
								case '5':
								case '6':
								case '7':
								case '8':
								case '9':
									state = TokenizerState.UnknownNumber;
									break;
								case '"':
									state = TokenizerState.String;
									break;
								default:
									if (IsIdentifierCharacter(*val))
									{
										state = TokenizerState.Identifier;
									}
									else
									{
										throw new Exception("Unknown token!");
									}
									break;
							}
							tokenStart = val;
							tokStartColumn = curColumn;
							tokStartLine = curLine;
							curColumn++;
							val++;
							break;
						case TokenizerState.String:
							if (*val == '"')
							{
								toks.Add(new Token(new String(tokenStart, 1, (int)(val - tokenStart) - 1), TokenType.String, tokStartLine, tokStartColumn));
								state = TokenizerState.Unknown;
							}
							curColumn++;
							val++;
							break;
						case TokenizerState.UnknownNumber:
							if (*val == 'x')
							{
								state = TokenizerState.HexadecimalNumber;
								curColumn++;
								val++;
							}
							else if (*val == 'b')
							{
								state = TokenizerState.BinaryNumber;
								curColumn++;
								val++;
							}
							else
							{
								state = TokenizerState.DecimalNumber;
							}
							break;
						case TokenizerState.BinaryNumber:
							switch(*val)
							{
								case '0':
								case '1':
									curColumn++;
									val++;
									break;
								default:
									uint pVal = Utils.ParseBinary(new String(tokenStart + 2, 0, (int)(val - tokenStart - 2)));
									toks.Add(new Token(null, TokenType.Number, tokStartLine, tokStartColumn, new Number(NumberFormat.Binary, pVal, (uint)(val - tokenStart - 2))));
									state = TokenizerState.Unknown;
									break;
							}
							break;
						case TokenizerState.DecimalNumber:
							switch(*val)
							{
								case '0':
								case '1':
								case '2':
								case '3':
								case '4':
								case '5':
								case '6':
								case '7':
								case '8':
								case '9':
									curColumn++;
									val++;
									break;
								default:
									int pVal = int.Parse(new String(tokenStart, 0, (int)(val - tokenStart)));
									toks.Add(new Token(null, TokenType.Number, tokStartLine, tokStartColumn, new Number(NumberFormat.Decimal, (uint)pVal)));
									state = TokenizerState.Unknown;
									break;
							}
							break;
						case TokenizerState.HexadecimalNumber:
							switch(*val)
							{
								case '0':
								case '1':
								case '2':
								case '3':
								case '4':
								case '5':
								case '6':
								case '7':
								case '8':
								case '9':
								case 'A':
								case 'B':
								case 'C':
								case 'D':
								case 'E':
								case 'F':
									curColumn++;
									val++;
									break;
								default:
									uint pVal = uint.Parse(new String(tokenStart + 2, 0, (int)(val - tokenStart - 2)), NumberStyles.AllowHexSpecifier);
									toks.Add(new Token(null, TokenType.Number, tokStartLine, tokStartColumn, new Number(NumberFormat.Hexadecimal, pVal)));
									state = TokenizerState.Unknown;
									break;
							}
							break;
						case TokenizerState.Identifier:
							if (!IsIdentifierCharacter(*val))
							{
								toks.Add(new Token(new String(tokenStart, 0, (int)(val - tokenStart)), TokenType.Identifier, tokStartLine, tokStartColumn));
								state = TokenizerState.Unknown;
							}
							else
							{
								curColumn++;
								val++;
							}
							break;
						case TokenizerState.MaybeComment:
							switch(*val)
							{
								case '/':
									state = TokenizerState.UnknownComment;
									break;
								default:
									throw new Exception("Unexpected '/'!");
							}
							curColumn++;
							val++;
							break;
						case TokenizerState.UnknownComment:
							switch(*val)
							{
								case '/':
									state = TokenizerState.ProbablyDocumentationComment;
									break;
								default:
									state = TokenizerState.SingleLineComment;
									break;
							}
							curColumn++;
							val++;
							break;
						case TokenizerState.SingleLineComment:
							switch(*val)
							{
								case '\r':
								case '\n':
									// Discard single-line comments
									state = TokenizerState.Unknown;
									break;
								default:
									curColumn++;
									val++;
									break;
							}
							break;
						case TokenizerState.ProbablyDocumentationComment:
							switch(*val)
							{
								case '/':
									// Well what do you know! We're actually a commented out documentation comment! (or a commented out comment)
									state = TokenizerState.SingleLineComment;
									break;
								default:
									// Alas, we're just a lowly documentation comment :(
									state = TokenizerState.DocumentationCommentLeadingSpace;
									break;
							}
							// We would end up removing the 3 leading forward slashes
							// anyways, so we do it here, before the initial string object is created.
							tokenStart = val;
							break;
						case TokenizerState.DocumentationCommentLeadingSpace:
							switch(*val)
							{
								case ' ':
								case '\t':
									val++;
									tokenStart = val;
									break;
								default:
									state = TokenizerState.DocumentationComment;
									break;
							}
							break;
						case TokenizerState.DocumentationComment:
							switch(*val)
							{
								case '\r':
								case '\n':
									toks.Add(new Token(new String(tokenStart, 0, (int)(val - tokenStart)), TokenType.DocumentationComment, tokStartLine, tokStartColumn));
									state = TokenizerState.Unknown;
									break;
								default:
									curColumn++;
									val++;
									break;
							}
							break;
						default:
							throw new Exception("Unknown parser state!");
					}
				}
				switch(state)
				{
					case TokenizerState.DecimalNumber:
					case TokenizerState.UnknownNumber:
						int pVal = int.Parse(new String(tokenStart, 0, (int)(val - tokenStart)));
						toks.Add(new Token(null, TokenType.Number, tokStartLine, tokStartColumn, new Number(NumberFormat.Decimal, (uint)pVal)));
						break;
					case TokenizerState.HexadecimalNumber:
						uint pVal2 = uint.Parse(new String(tokenStart + 2, 0, (int)(val - tokenStart - 2)), NumberStyles.AllowHexSpecifier);
						toks.Add(new Token(null, TokenType.Number, tokStartLine, tokStartColumn, new Number(NumberFormat.Hexadecimal, pVal2)));
						break;
					case TokenizerState.DocumentationComment:
						toks.Add(new Token(new String(tokenStart, 0, (int)(val - tokenStart)), TokenType.DocumentationComment, tokStartLine, tokStartColumn));
						break;
					case TokenizerState.Identifier:
						toks.Add(new Token(new String(tokenStart, 0, (int)(val - tokenStart)), TokenType.Identifier, tokStartLine, tokStartColumn));
						break;
					case TokenizerState.MaybeComment:
						throw new Exception("Unexpected '/'!");

					case TokenizerState.SingleLineComment:
					case TokenizerState.UnknownComment:
					case TokenizerState.Unknown:
						break;
					default:
						throw new Exception("Unknown state!");
				}
			}
			return toks.ToArray();
		}

		private static bool IsIdentifierCharacter(char c)
		{
			switch (c)
			{
				// Yes, a forward slash is valid
				// as part of an identifier in a 
				// cpud file.
				case '/':
				case '_':
				case 'a':
				case 'b':
				case 'c':
				case 'd':
				case 'e':
				case 'f':
				case 'g':
				case 'h':
				case 'i':
				case 'j':
				case 'k':
				case 'l':
				case 'm':
				case 'n':
				case 'o':
				case 'p':
				case 'q':
				case 'r':
				case 's':
				case 't':
				case 'u':
				case 'v':
				case 'w':
				case 'x':
				case 'y':
				case 'z':
				case 'A':
				case 'B':
				case 'C':
				case 'D':
				case 'E':
				case 'F':
				case 'G':
				case 'H':
				case 'I':
				case 'J':
				case 'K':
				case 'L':
				case 'M':
				case 'N':
				case 'O':
				case 'P':
				case 'Q':
				case 'R':
				case 'S':
				case 'T':
				case 'U':
				case 'V':
				case 'W':
				case 'X':
				case 'Y':
				case 'Z':
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					return true;
			}
			return false;
		}

	}
}

