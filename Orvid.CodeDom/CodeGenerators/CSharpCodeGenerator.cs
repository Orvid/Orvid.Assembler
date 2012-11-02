using System;
using Orvid.CodeDom;
using Orvid.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace Orvid.CodeDom.CodeGenerators
{
	public class CSharpCodeGenerator : CodeGenerator
	{
		
		#region Extensions
		
		protected override void GenerateSwitch(CodeSwitchStatement e)
		{
			TextWriter output = base.Output;
			output.Write("switch(");
			GenerateExpression(e.SwitchedOn);
			output.Write(")");
			OutputStartBrace();
			Indent++;
			foreach (CodeCaseStatement st in e.Cases)
			{
				if (st is CodeDefaultCaseStatement)
				{
					GenerateDefaultCaseStatement((CodeDefaultCaseStatement)st);
				}
				else
				{
					GenerateCaseStatement(st);
				}
			}
			Indent--;
			output.Write("}");
			output.WriteLine();
		}
		
		protected override void GenerateCaseStatement(CodeCaseStatement e)
		{
			TextWriter output = base.Output;
			output.Write("case ");
			GenerateExpression(e.CaseExpression);
			output.Write(":");
			output.WriteLine();
			Indent++;
			GenerateStatements(e.Statements);
			Indent--;
		}
		
		protected override void GenerateDefaultCaseStatement(CodeDefaultCaseStatement e)
		{
			TextWriter output = base.Output;
			output.Write("default:");
			output.WriteLine();
			Indent++;
			GenerateStatements(e.Statements);
			Indent--;
		}
		
		protected override void GenerateBreakStatement(CodeBreakStatement e)
		{
			TextWriter output = base.Output;
			output.Write("break");
			if (this.dont_write_semicolon)
			{
				return;
			}
			output.WriteLine(';');
		}

		protected override void GenerateDocumentationSummaryNode(CodeDocumentationSummaryNode n)
		{
			TextWriter output = base.Output;
			output.WriteLine("/// <summary>");
			foreach (string s in n.Lines)
			{
				output.Write("/// ");
				output.WriteLine(s);
			}
			output.WriteLine("/// </summary>");
		}

		protected override void GenerateDocumentationParameterNode(CodeDocumentationParameterNode n)
		{
			base.Output.Write("/// <param name=\"");
			base.Output.Write(n.ParamName);
			base.Output.Write("\">");
			base.Output.Write(n.Summary);
			base.Output.WriteLine("</param>");
		}

		protected override void GenerateDocumentationReturnNode(CodeDocumentationReturnNode n)
		{
			base.Output.Write("/// <returns>");
			base.Output.Write(n.Summary);
			base.Output.WriteLine("</returns>");
		}
		
		#endregion

		//
		// Static Fields
		//
		
		private static Hashtable keywordsTable;
		
		private static readonly string[] keywords = new string[]
		{
			"abstract",
			"event",
			"new",
			"struct",
			"as",
			"explicit",
			"null",
			"switch",
			"base",
			"extern",
			"this",
			"false",
			"operator",
			"throw",
			"break",
			"finally",
			"out",
			"true",
			"fixed",
			"override",
			"try",
			"case",
			"params",
			"typeof",
			"catch",
			"for",
			"private",
			"foreach",
			"protected",
			"checked",
			"goto",
			"public",
			"unchecked",
			"class",
			"if",
			"readonly",
			"unsafe",
			"const",
			"implicit",
			"ref",
			"continue",
			"in",
			"return",
			"using",
			"virtual",
			"default",
			"interface",
			"sealed",
			"volatile",
			"delegate",
			"internal",
			"do",
			"is",
			"sizeof",
			"while",
			"lock",
			"stackalloc",
			"else",
			"static",
			"enum",
			"namespace",
			"object",
			"bool",
			"byte",
			"float",
			"uint",
			"char",
			"ulong",
			"ushort",
			"decimal",
			"int",
			"sbyte",
			"short",
			"double",
			"long",
			"string",
			"void",
			"partial",
			"yield",
			"where"
		};
		
		//
		// Fields
		//
		
		private IDictionary<string, string> providerOptions;
		
		private bool dont_write_semicolon;
		
		//
		// Properties
		//
		
		protected override string NullToken
		{
			get
			{
				return "null";
			}
		}
		
		protected IDictionary<string, string> ProviderOptions
		{
			get
			{
				return this.providerOptions;
			}
		}
		
		//
		// Constructors
		//
		
		public CSharpCodeGenerator()
		{
			this.dont_write_semicolon = false;
		}
		
		public CSharpCodeGenerator(IDictionary<string, string> providerOptions)
		{
			this.providerOptions = providerOptions;
		}
		
		//
		// Static Methods
		//
		
		private static void FillKeywordTable()
		{
			CSharpCodeGenerator.keywordsTable = new Hashtable();
			string[] array = CSharpCodeGenerator.keywords;
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i];
				CSharpCodeGenerator.keywordsTable.Add(text, text);
			}
		}
		
		private static bool is_identifier_part_character(char c)
		{
			return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_' || (c >= '0' && c <= '9') || char.IsLetter(c);
		}
		
		private static bool is_identifier_start_character(char c)
		{
			return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_' || c == '@' || char.IsLetter(c);
		}
		
		private static bool IsAbstract(MemberAttributes attributes)
		{
			return (attributes & MemberAttributes.ScopeMask) == MemberAttributes.Abstract;
		}
		
		//
		// Methods
		//
		
		protected override string CreateEscapedIdentifier(string value)
		{
			if (value == null)
			{
				throw new NullReferenceException("Argument identifier is null.");
			}
			return this.GetSafeName(value);
		}
		
		protected override string CreateValidIdentifier(string value)
		{
			if (value == null)
			{
				throw new NullReferenceException();
			}
			if (CSharpCodeGenerator.keywordsTable == null)
			{
				CSharpCodeGenerator.FillKeywordTable();
			}
			if (CSharpCodeGenerator.keywordsTable.Contains(value))
			{
				return "_" + value;
			}
			return value;
		}
		
		private string DetermineTypeOutput(CodeTypeReference type)
		{
			string baseType = type.BaseType;
			string text = baseType.ToLower(CultureInfo.InvariantCulture);
			string result;
			switch (text)
			{
				case "system.int32":
					result = "int";
					return result;
				case "system.int64":
					result = "long";
					return result;
				case "system.int16":
					result = "short";
					return result;
				case "system.boolean":
					result = "bool";
					return result;
				case "system.char":
					result = "char";
					return result;
				case "system.string":
					result = "string";
					return result;
				case "system.object":
					result = "object";
					return result;
				case "system.void":
					result = "void";
					return result;
				case "system.byte":
					result = "byte";
					return result;
				case "system.sbyte":
					result = "sbyte";
					return result;
				case "system.decimal":
					result = "decimal";
					return result;
				case "system.double":
					result = "double";
					return result;
				case "system.single":
					result = "float";
					return result;
				case "system.uint16":
					result = "ushort";
					return result;
				case "system.uint32":
					result = "uint";
					return result;
				case "system.uint64":
					result = "ulong";
					return result;
			}
			StringBuilder stringBuilder = new StringBuilder(baseType.Length);
			if ((type.Options & CodeTypeReferenceOptions.GlobalReference) != (CodeTypeReferenceOptions)0)
			{
				stringBuilder.Append("global::");
			}
			int num2 = 0;
			for (int i = 0; i < baseType.Length; i++)
			{
				char c = baseType[i];
				if (c != '+' && c != '.')
				{
					if (c == '`')
					{
						stringBuilder.Append(this.CreateEscapedIdentifier(baseType.Substring(num2, i - num2)));
						i++;
						int num3 = i;
						while (num3 < baseType.Length && char.IsDigit(baseType[num3]))
						{
							num3++;
						}
						int count = int.Parse(baseType.Substring(i, num3 - i));
						this.OutputTypeArguments(type.TypeArguments, stringBuilder, count);
						i = num3;
						if (i < baseType.Length && (baseType[i] == '+' || baseType[i] == '.'))
						{
							stringBuilder.Append('.');
							i++;
						}
						num2 = i;
					}
				}
				else
				{
					stringBuilder.Append(this.CreateEscapedIdentifier(baseType.Substring(num2, i - num2)));
					stringBuilder.Append('.');
					i++;
					num2 = i;
				}
			}
			if (num2 < baseType.Length)
			{
				stringBuilder.Append(this.CreateEscapedIdentifier(baseType.Substring(num2)));
			}
			result = stringBuilder.ToString();
			return result;
		}
		
		protected override void GenerateArgumentReferenceExpression(CodeArgumentReferenceExpression expression)
		{
			base.Output.Write(this.GetSafeName(expression.ParameterName));
		}
		
		protected override void GenerateArrayCreateExpression(CodeArrayCreateExpression expression)
		{
			TextWriter output = base.Output;
			output.Write("new ");
			CodeExpressionCollection initializers = expression.Initializers;
			CodeTypeReference codeTypeReference = expression.CreateType;
			if (initializers.Count > 0)
			{
				this.OutputType(codeTypeReference);
				if (expression.CreateType.ArrayRank == 0)
				{
					output.Write("[]");
				}
				this.OutputStartBrace();
				base.Indent++;
				this.OutputExpressionList(initializers, true);
				base.Indent--;
				output.Write("}");
			}
			else
			{
				for (CodeTypeReference arrayElementType = codeTypeReference.ArrayElementType; arrayElementType != null; arrayElementType = arrayElementType.ArrayElementType)
				{
					codeTypeReference = arrayElementType;
				}
				this.OutputType(codeTypeReference);
				output.Write('[');
				CodeExpression sizeExpression = expression.SizeExpression;
				if (sizeExpression != null)
				{
					base.GenerateExpression(sizeExpression);
				}
				else
				{
					output.Write(expression.Size);
				}
				output.Write(']');
			}
		}
		
		protected override void GenerateArrayIndexerExpression(CodeArrayIndexerExpression expression)
		{
			TextWriter output = base.Output;
			base.GenerateExpression(expression.TargetObject);
			output.Write('[');
			this.OutputExpressionList(expression.Indices);
			output.Write(']');
		}
		
		protected override void GenerateAssignStatement(CodeAssignStatement statement)
		{
			TextWriter output = base.Output;
			base.GenerateExpression(statement.Left);
			output.Write(" = ");
			if (statement.Right is CodeCastExpression)
				((CodeCastExpression)statement.Right).NeedsGrouping = false;
			base.GenerateExpression(statement.Right);
			if (this.dont_write_semicolon)
			{
				return;
			}
			output.WriteLine(';');
		}
		
		protected override void GenerateAttachEventStatement(CodeAttachEventStatement statement)
		{
			TextWriter output = base.Output;
			this.GenerateEventReferenceExpression(statement.Event);
			output.Write(" += ");
			base.GenerateExpression(statement.Listener);
			output.WriteLine(';');
		}
		
		protected override void GenerateAttributeDeclarationsEnd(CodeAttributeDeclarationCollection attributes)
		{
			base.Output.Write(']');
		}
		
		protected override void GenerateAttributeDeclarationsStart(CodeAttributeDeclarationCollection attributes)
		{
			base.Output.Write('[');
		}
		
		protected override void GenerateBaseReferenceExpression(CodeBaseReferenceExpression expression)
		{
			base.Output.Write("base");
		}
		
		protected override void GenerateCastExpression(CodeCastExpression expression)
		{
			// CLEANUP: This has been cleaned up so that the code it generates looks better.
			TextWriter output = base.Output;
			if (expression.NeedsGrouping)
				output.Write("(");
			output.Write("(");
			this.OutputType(expression.TargetType);
			output.Write(")");
			// Casts can be chained.
			if (expression.Expression is CodeCastExpression)
				((CodeCastExpression)expression.Expression).NeedsGrouping = false;
			// CodeBinaryOperatorExpressions already generate their own parenthesis surrounding themself.
			base.GenerateExpression(expression.Expression);
			if (expression.NeedsGrouping)
				output.Write(")");
		}
		
		private void GenerateCharValue(char c)
		{
			base.Output.Write('\'');
			switch (c)
			{
				case '\t':
					base.Output.Write("\\t");
					break;
				case '\n':
					base.Output.Write("\\n");
					break;
				case '\r':
					base.Output.Write("\\r");
					break;
				case '\v':
				case '\f':
				default:
					if (c == '\u2028')
					{
						base.Output.Write("\\u");
						TextWriter arg_11F_0 = base.Output;
						int num = (int)c;
						arg_11F_0.Write(num.ToString("X4", CultureInfo.InvariantCulture));
						break;
					}
					if (c == '\u2029')
					{
						base.Output.Write("\\u");
						TextWriter arg_152_0 = base.Output;
						int num2 = (int)c;
						arg_152_0.Write(num2.ToString("X4", CultureInfo.InvariantCulture));
						break;
					}
					if (c == '\0')
					{
						base.Output.Write("\\0");
						break;
					}
					if (c == '"')
					{
						base.Output.Write("\\\"");
						break;
					}
					if (c == '\'')
					{
						base.Output.Write("\\'");
						break;
					}
					if (c != '\\')
					{
						base.Output.Write(c);
						break;
					}
					base.Output.Write("\\\\");
					break;
			}
			base.Output.Write('\'');
		}
		
		private void GenerateCodeChecksumPragma(CodeChecksumPragma pragma)
		{
			base.Output.Write("#pragma checksum ");
			base.Output.Write(this.QuoteSnippetString(pragma.FileName));
			base.Output.Write(" \"");
			base.Output.Write(pragma.ChecksumAlgorithmId.ToString("B"));
			base.Output.Write("\" \"");
			if (pragma.ChecksumData != null)
			{
				byte[] checksumData = pragma.ChecksumData;
				for (int i = 0; i < checksumData.Length; i++)
				{
					byte b = checksumData[i];
					base.Output.Write(b.ToString("X2"));
				}
			}
			base.Output.WriteLine("\"");
		}
		
		private void GenerateCodeRegionDirective(CodeRegionDirective region)
		{
			CodeRegionMode regionMode = region.RegionMode;
			if (regionMode == CodeRegionMode.Start)
			{
				base.Output.Write("#region ");
				base.Output.WriteLine(region.RegionText);
				return;
			}
			if (regionMode == CodeRegionMode.End)
			{
				base.Output.WriteLine("#endregion");
				return;
			}
		}
		
		protected override void GenerateComment(CodeComment comment)
		{
			TextWriter output = base.Output;
			string value;
			if (comment.DocComment)
			{
				value = "///";
			}
			else
			{
				value = "//";
			}
			output.Write(value);
			output.Write(' ');
			string text = comment.Text;
			for (int i = 0; i < text.Length; i++)
			{
				output.Write(text[i]);
				if (text[i] == '\r')
				{
					if (i >= text.Length - 1 || text[i + 1] != '\n')
					{
						output.Write(value);
					}
				}
				else
				{
					if (text[i] == '\n')
					{
						output.Write(value);
					}
				}
			}
			output.WriteLine();
		}
		
		protected override void GenerateCompileUnit(CodeCompileUnit compileUnit)
		{
			this.GenerateCompileUnitStart(compileUnit);
			this.GenerateGlobalNamespace(compileUnit);
			if (compileUnit.AssemblyCustomAttributes.Count > 0)
			{
				this.OutputAttributes(compileUnit.AssemblyCustomAttributes, "assembly: ", false);
				base.Output.WriteLine(string.Empty);
			}
			this.GenerateLocalNamespaces(compileUnit);
			this.GenerateCompileUnitEnd(compileUnit);
		}
		
		protected override void GenerateCompileUnitStart(CodeCompileUnit compileUnit)
		{
			this.GenerateComment(new CodeComment("------------------------------------------------------------------------------"));
			this.GenerateComment(new CodeComment(" <autogenerated>"));
			this.GenerateComment(new CodeComment("     This code was generated by a tool."));
			this.GenerateComment(new CodeComment("     Mono Runtime Version: " + Environment.Version));
			this.GenerateComment(new CodeComment(string.Empty));
			this.GenerateComment(new CodeComment("     Changes to this file may cause incorrect behavior and will be lost if "));
			this.GenerateComment(new CodeComment("     the code is regenerated."));
			this.GenerateComment(new CodeComment(" </autogenerated>"));
			this.GenerateComment(new CodeComment("------------------------------------------------------------------------------"));
			base.Output.WriteLine();
			base.GenerateCompileUnitStart(compileUnit);
		}
		
		protected override void GenerateConditionStatement(CodeConditionStatement statement)
		{
			TextWriter output = base.Output;
			output.Write("if (");
			base.GenerateExpression(statement.Condition);
			output.Write(")");
			this.OutputStartBrace();
			base.Indent++;
			base.GenerateStatements(statement.TrueStatements);
			base.Indent--;
			CodeStatementCollection falseStatements = statement.FalseStatements;
			bool genned = false;
			if (falseStatements.Count > 0)
			{
				output.Write('}');
				if (base.Options.ElseOnClosing)
				{
					output.Write(' ');
				}
				else
				{
					output.WriteLine();
				}
				output.Write("else ");
				if (falseStatements.Count == 1)
				{
					CodeStatement stat = falseStatements[0];
					CheckSingleCondition:
					if (stat is CodeScopeStatement)
					{
						if (((CodeScopeStatement)stat).Statements.Count == 1)
						{
							stat = ((CodeScopeStatement)stat).Statements[0];
							goto CheckSingleCondition;
						}
					}
					if (stat is CodeConditionStatement)
					{
						genned = true;
						GenerateStatement(stat);
					}
				}
				if (!genned)
				{
					this.OutputStartBrace();
					base.Indent++;
					base.GenerateStatements(falseStatements);
					base.Indent--;
				}
			}
			if (!genned)
			{
				output.WriteLine('}');
			}
		}
		
		protected override void GenerateConstructor(CodeConstructor constructor, CodeTypeDeclaration declaration)
		{
			if (base.IsCurrentDelegate || base.IsCurrentEnum || base.IsCurrentInterface)
			{
				return;
			}
			this.OutputAttributes(constructor.CustomAttributes, null, false);
			this.OutputMemberAccessModifier(constructor.Attributes);
			base.Output.Write(this.GetSafeName(base.CurrentTypeName) + "(");
			this.OutputParameters(constructor.Parameters);
			base.Output.Write(")");
			if (constructor.BaseConstructorArgs.Count > 0)
			{
				base.Output.Write(" : ");
				base.Output.Write("base(");
				this.OutputExpressionList(constructor.BaseConstructorArgs);
				base.Output.Write(')');
			}
			if (constructor.ChainedConstructorArgs.Count > 0)
			{
				base.Output.Write(" : ");
				base.Output.Write("this(");
				this.OutputExpressionList(constructor.ChainedConstructorArgs);
				base.Output.Write(')');
			}
			this.OutputStartBrace();
			base.Indent++;
			base.GenerateStatements(constructor.Statements);
			base.Indent--;
			base.Output.WriteLine('}');
		}
		
		protected override void GenerateDecimalValue(decimal d)
		{
			base.GenerateDecimalValue(d);
			base.Output.Write('m');
		}
		
		protected override void GenerateDefaultValueExpression(CodeDefaultValueExpression e)
		{
			base.Output.Write("default(");
			this.OutputType(e.Type);
			base.Output.Write(')');
		}
		
		protected override void GenerateDelegateCreateExpression(CodeDelegateCreateExpression expression)
		{
			TextWriter output = base.Output;
			output.Write("new ");
			this.OutputType(expression.DelegateType);
			output.Write('(');
			CodeExpression targetObject = expression.TargetObject;
			if (targetObject != null)
			{
				base.GenerateExpression(targetObject);
				base.Output.Write('.');
			}
			output.Write(this.GetSafeName(expression.MethodName));
			output.Write(')');
		}
		
		protected override void GenerateDelegateInvokeExpression(CodeDelegateInvokeExpression expression)
		{
			if (expression.TargetObject != null)
			{
				base.GenerateExpression(expression.TargetObject);
			}
			base.Output.Write('(');
			this.OutputExpressionList(expression.Parameters);
			base.Output.Write(')');
		}
		
		protected override void GenerateDirectives(CodeDirectiveCollection directives)
		{
			IEnumerator enumerator = directives.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					CodeDirective codeDirective = (CodeDirective)enumerator.Current;
					if (codeDirective is CodeChecksumPragma)
					{
						this.GenerateCodeChecksumPragma((CodeChecksumPragma)codeDirective);
					}
					else
					{
						if (!(codeDirective is CodeRegionDirective))
						{
							throw new NotImplementedException("Unknown CodeDirective");
						}
						this.GenerateCodeRegionDirective((CodeRegionDirective)codeDirective);
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
		}
		
		protected override void GenerateEntryPointMethod(CodeEntryPointMethod method, CodeTypeDeclaration declaration)
		{
			this.OutputAttributes(method.CustomAttributes, null, false);
			base.Output.Write("public static ");
			this.OutputType(method.ReturnType);
			base.Output.Write(" Main()");
			this.OutputStartBrace();
			base.Indent++;
			base.GenerateStatements(method.Statements);
			base.Indent--;
			base.Output.WriteLine("}");
		}
		
		protected override void GenerateEvent(CodeMemberEvent eventRef, CodeTypeDeclaration declaration)
		{
			if (base.IsCurrentDelegate || base.IsCurrentEnum)
			{
				return;
			}
			this.OutputAttributes(eventRef.CustomAttributes, null, false);
			if (eventRef.PrivateImplementationType == null)
			{
				this.OutputMemberAccessModifier(eventRef.Attributes);
			}
			base.Output.Write("event ");
			if (eventRef.PrivateImplementationType != null)
			{
				this.OutputTypeNamePair(eventRef.Type, eventRef.PrivateImplementationType.BaseType + "." + eventRef.Name);
			}
			else
			{
				this.OutputTypeNamePair(eventRef.Type, this.GetSafeName(eventRef.Name));
			}
			base.Output.WriteLine(';');
		}
		
		protected override void GenerateEventReferenceExpression(CodeEventReferenceExpression expression)
		{
			if (expression.TargetObject != null)
			{
				base.GenerateExpression(expression.TargetObject);
				base.Output.Write('.');
			}
			base.Output.Write(this.GetSafeName(expression.EventName));
		}
		
		protected override void GenerateExpressionStatement(CodeExpressionStatement statement)
		{
			base.GenerateExpression(statement.Expression);
			if (this.dont_write_semicolon)
			{
				return;
			}
			base.Output.WriteLine(';');
		}
		
		protected override void GenerateField(CodeMemberField field)
		{
			if (base.IsCurrentDelegate || base.IsCurrentInterface)
			{
				return;
			}
			TextWriter output = base.Output;
			this.OutputAttributes(field.CustomAttributes, null, false);
			if (base.IsCurrentEnum)
			{
				base.Output.Write(this.GetSafeName(field.Name));
			}
			else
			{
				MemberAttributes attributes = field.Attributes;
				this.OutputMemberAccessModifier(attributes);
				this.OutputVTableModifier(attributes);
				this.OutputFieldScopeModifier(attributes);
				this.OutputTypeNamePair(field.Type, this.GetSafeName(field.Name));
			}
			CodeExpression initExpression = field.InitExpression;
			if (initExpression != null)
			{
				output.Write(" = ");
				base.GenerateExpression(initExpression);
			}
			if (base.IsCurrentEnum)
			{
				output.WriteLine(',');
			}
			else
			{
				output.WriteLine(';');
			}
		}
		
		protected override void GenerateFieldReferenceExpression(CodeFieldReferenceExpression expression)
		{
			CodeExpression targetObject = expression.TargetObject;
			if (targetObject != null)
			{
				base.GenerateExpression(targetObject);
				base.Output.Write('.');
			}
			base.Output.Write(this.GetSafeName(expression.FieldName));
		}
		
		private void GenerateGenericsConstraints(CodeTypeParameterCollection parameters)
		{
			int count = parameters.Count;
			if (count == 0)
			{
				return;
			}
			bool flag = false;
			for (int i = 0; i < count; i++)
			{
				CodeTypeParameter codeTypeParameter = parameters[i];
				bool flag2 = codeTypeParameter.Constraints.Count != 0;
				base.Output.WriteLine();
				if (flag2 || codeTypeParameter.HasConstructorConstraint)
				{
					if (!flag)
					{
						base.Indent++;
						flag = true;
					}
					base.Output.Write("where ");
					base.Output.Write(codeTypeParameter.Name);
					base.Output.Write(" : ");
					for (int j = 0; j < codeTypeParameter.Constraints.Count; j++)
					{
						if (j > 0)
						{
							base.Output.Write(", ");
						}
						this.OutputType(codeTypeParameter.Constraints[j]);
					}
					if (codeTypeParameter.HasConstructorConstraint)
					{
						if (flag2)
						{
							base.Output.Write(", ");
						}
						base.Output.Write("new");
						if (flag2)
						{
							base.Output.Write(" ");
						}
						base.Output.Write("()");
					}
				}
			}
			if (flag)
			{
				base.Indent--;
			}
		}
		
		private void GenerateGenericsParameters(CodeTypeParameterCollection parameters)
		{
			int count = parameters.Count;
			if (count == 0)
			{
				return;
			}
			base.Output.Write('<');
			for (int i = 0; i < count - 1; i++)
			{
				base.Output.Write(parameters[i].Name);
				base.Output.Write(", ");
			}
			base.Output.Write(parameters[count - 1].Name);
			base.Output.Write('>');
		}
		
		private void GenerateGlobalNamespace(CodeCompileUnit compileUnit)
		{
			CodeNamespace codeNamespace = null;
			IEnumerator enumerator = compileUnit.Namespaces.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					CodeNamespace codeNamespace2 = (CodeNamespace)enumerator.Current;
					if (string.IsNullOrEmpty(codeNamespace2.Name))
					{
						codeNamespace = codeNamespace2;
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
			if (codeNamespace != null)
			{
				this.GenerateNamespace(codeNamespace);
			}
		}
		
		protected override void GenerateGotoStatement(CodeGotoStatement statement)
		{
			TextWriter output = base.Output;
			output.Write("goto ");
			output.Write(this.GetSafeName(statement.Label));
			output.WriteLine(";");
		}
		
		protected override void GenerateIndexerExpression(CodeIndexerExpression expression)
		{
			TextWriter output = base.Output;
			base.GenerateExpression(expression.TargetObject);
			output.Write('[');
			this.OutputExpressionList(expression.Indices);
			output.Write(']');
		}
		
		protected override void GenerateIterationStatement(CodeIterationStatement statement)
		{
			TextWriter output = base.Output;
			this.dont_write_semicolon = true;
			output.Write("for (");
			base.GenerateStatement(statement.InitStatement);
			output.Write("; ");
			base.GenerateExpression(statement.TestExpression);
			output.Write("; ");
			base.GenerateStatement(statement.IncrementStatement);
			output.Write(")");
			this.dont_write_semicolon = false;
			this.OutputStartBrace();
			base.Indent++;
			base.GenerateStatements(statement.Statements);
			base.Indent--;
			output.WriteLine('}');
		}
		
		protected override void GenerateLabeledStatement(CodeLabeledStatement statement)
		{
			base.Indent--;
			base.Output.Write(statement.Label);
			base.Output.WriteLine(":");
			base.Indent++;
			if (statement.Statement != null)
			{
				base.GenerateStatement(statement.Statement);
			}
		}
		
		protected override void GenerateLinePragmaEnd(CodeLinePragma linePragma)
		{
			base.Output.WriteLine();
			base.Output.WriteLine("#line default");
			base.Output.WriteLine("#line hidden");
		}
		
		protected override void GenerateLinePragmaStart(CodeLinePragma linePragma)
		{
			base.Output.WriteLine();
			base.Output.Write("#line ");
			base.Output.Write(linePragma.LineNumber);
			base.Output.Write(" \"");
			base.Output.Write(linePragma.FileName);
			base.Output.Write("\"");
			base.Output.WriteLine();
		}
		
		private void GenerateLocalNamespaces(CodeCompileUnit compileUnit)
		{
			IEnumerator enumerator = compileUnit.Namespaces.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					CodeNamespace codeNamespace = (CodeNamespace)enumerator.Current;
					if (!string.IsNullOrEmpty(codeNamespace.Name))
					{
						this.GenerateNamespace(codeNamespace);
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
		}
		
		protected override void GenerateMethod(CodeMemberMethod method, CodeTypeDeclaration declaration)
		{
			if (base.IsCurrentDelegate || base.IsCurrentEnum)
			{
				return;
			}
			TextWriter output = base.Output;
			this.OutputAttributes(method.CustomAttributes, null, false);
			this.OutputAttributes(method.ReturnTypeCustomAttributes, "return: ", false);
			MemberAttributes attributes = method.Attributes;
			if (!base.IsCurrentInterface)
			{
				if (method.PrivateImplementationType == null)
				{
					this.OutputMemberAccessModifier(attributes);
					this.OutputVTableModifier(attributes);
					this.OutputMemberScopeModifier(attributes);
				}
			}
			else
			{
				this.OutputVTableModifier(attributes);
			}
			this.OutputType(method.ReturnType);
			output.Write(' ');
			CodeTypeReference privateImplementationType = method.PrivateImplementationType;
			if (privateImplementationType != null)
			{
				output.Write(privateImplementationType.BaseType);
				output.Write('.');
			}
			output.Write(this.GetSafeName(method.Name));
			this.GenerateGenericsParameters(method.TypeParameters);
			output.Write('(');
			this.OutputParameters(method.Parameters);
			output.Write(')');
			this.GenerateGenericsConstraints(method.TypeParameters);
			if (CSharpCodeGenerator.IsAbstract(attributes) || declaration.IsInterface)
			{
				output.WriteLine(';');
			}
			else
			{
				this.OutputStartBrace();
				base.Indent++;
				base.GenerateStatements(method.Statements);
				base.Indent--;
				output.WriteLine('}');
			}
		}
		
		protected override void GenerateMethodInvokeExpression(CodeMethodInvokeExpression expression)
		{
			TextWriter output = base.Output;
			this.GenerateMethodReferenceExpression(expression.Method);
			output.Write('(');
			this.OutputExpressionList(expression.Parameters);
			output.Write(')');
		}
		
		protected override void GenerateMethodReferenceExpression(CodeMethodReferenceExpression expression)
		{
			if (expression.TargetObject != null)
			{
				base.GenerateExpression(expression.TargetObject);
				base.Output.Write('.');
			}
			base.Output.Write(this.GetSafeName(expression.MethodName));
			if (expression.TypeArguments.Count > 0)
			{
				base.Output.Write(this.GetTypeArguments(expression.TypeArguments));
			}
		}
		
		protected override void GenerateMethodReturnStatement(CodeMethodReturnStatement statement)
		{
			// CLEANUP: This method has been modified so it generates code better.
			TextWriter output = base.Output;
			if (statement.Expression != null)
			{
				output.Write("return ");
				if (statement.Expression is CodeBinaryOperatorExpression)
					((CodeBinaryOperatorExpression)statement.Expression).NeedsGrouping = false;
				else if (statement.Expression is CodeCastExpression)
					((CodeCastExpression)statement.Expression).NeedsGrouping = false;
				base.GenerateExpression(statement.Expression);
				output.WriteLine(";");
			}
			else
			{
				output.WriteLine("return;");
			}
		}
		
		protected override void GenerateNamespaceEnd(CodeNamespace ns)
		{
			string name = ns.Name;
			if (name != null && name.Length != 0)
			{
				base.Indent--;
				base.Output.WriteLine("}");
			}
		}
		
		protected override void GenerateNamespaceImport(CodeNamespaceImport import)
		{
			TextWriter output = base.Output;
			output.Write("using ");
			output.Write(this.GetSafeName(import.Namespace));
			output.WriteLine(';');
		}
		
		protected override void GenerateNamespaceStart(CodeNamespace ns)
		{
			TextWriter output = base.Output;
			string name = ns.Name;
			if (name != null && name.Length != 0)
			{
				output.Write("namespace ");
				output.Write(this.GetSafeName(name));
				this.OutputStartBrace();
				base.Indent++;
			}
		}
		
		protected override void GenerateObjectCreateExpression(CodeObjectCreateExpression expression)
		{
			base.Output.Write("new ");
			this.OutputType(expression.CreateType);
			base.Output.Write('(');
			this.OutputExpressionList(expression.Parameters);
			base.Output.Write(')');
		}
		
		protected override void GenerateParameterDeclarationExpression(CodeParameterDeclarationExpression e)
		{
			this.OutputAttributes(e.CustomAttributes, null, true);
			this.OutputDirection(e.Direction);
			this.OutputType(e.Type);
			base.Output.Write(' ');
			base.Output.Write(this.GetSafeName(e.Name));
		}
		
		protected override void GeneratePrimitiveExpression(CodePrimitiveExpression e)
		{
			if (e.Value is byte)
			{
				byte val = (byte)e.Value;
				base.Output.Write("0x" + val.ToString("X").PadLeft(2, '0'));
			}
			else if (e.Value is char)
			{
				this.GenerateCharValue((char)e.Value);
			}
			else if (e.Value is ushort)
			{
				ushort num = (ushort)e.Value;
				base.Output.Write(num.ToString(CultureInfo.InvariantCulture));
			}
			else if (e.Value is uint)
			{
				uint num2 = (uint)e.Value;
				base.Output.Write(num2.ToString(CultureInfo.InvariantCulture));
				base.Output.Write("u");
			}
			else if (e.Value is ulong)
			{
				ulong num3 = (ulong)e.Value;
				base.Output.Write(num3.ToString(CultureInfo.InvariantCulture));
				base.Output.Write("ul");
			}
			else if (e.Value is sbyte)
			{
				sbyte b = (sbyte)e.Value;
				base.Output.Write(b.ToString(CultureInfo.InvariantCulture));
			}
			else if (e.Value is float)
			{
				float f = (float)e.Value;
				base.Output.Write(f.ToString(CultureInfo.InvariantCulture));
				base.Output.Write("f");
			}
			else if (e.Value is double)
			{
				double d = (double)e.Value;
				base.Output.Write(d.ToString(CultureInfo.InvariantCulture));
				base.Output.Write("d");
			}
			else if (e.Value is decimal)
			{
				decimal d = (decimal)e.Value;
				base.Output.Write(d.ToString(CultureInfo.InvariantCulture));
				base.Output.Write("X");
			}
			else
			{
				base.GeneratePrimitiveExpression(e);
			}
		}
		
		protected override void GenerateProperty(CodeMemberProperty property, CodeTypeDeclaration declaration)
		{
			if (base.IsCurrentDelegate || base.IsCurrentEnum)
			{
				return;
			}
			TextWriter output = base.Output;
			this.OutputAttributes(property.CustomAttributes, null, false);
			MemberAttributes attributes = property.Attributes;
			if (!base.IsCurrentInterface)
			{
				if (property.PrivateImplementationType == null)
				{
					this.OutputMemberAccessModifier(attributes);
					this.OutputVTableModifier(attributes);
					this.OutputMemberScopeModifier(attributes);
				}
			}
			else
			{
				this.OutputVTableModifier(attributes);
			}
			this.OutputType(property.Type);
			output.Write(' ');
			if (!base.IsCurrentInterface && property.PrivateImplementationType != null)
			{
				output.Write(property.PrivateImplementationType.BaseType);
				output.Write('.');
			}
			if (string.Compare(property.Name, "Item", true, CultureInfo.InvariantCulture) == 0 && property.Parameters.Count > 0)
			{
				output.Write("this[");
				this.OutputParameters(property.Parameters);
				output.Write(']');
			}
			else
			{
				output.Write(this.GetSafeName(property.Name));
			}
			this.OutputStartBrace();
			base.Indent++;
			if (declaration.IsInterface || CSharpCodeGenerator.IsAbstract(property.Attributes))
			{
				if (property.HasGet)
				{
					output.WriteLine("get;");
				}
				if (property.HasSet)
				{
					output.WriteLine("set;");
				}
			}
			else
			{
				if (property.HasGet)
				{
					output.Write("get");
					this.OutputStartBrace();
					base.Indent++;
					base.GenerateStatements(property.GetStatements);
					base.Indent--;
					output.WriteLine('}');
				}
				if (property.HasSet)
				{
					output.Write("set");
					this.OutputStartBrace();
					base.Indent++;
					base.GenerateStatements(property.SetStatements);
					base.Indent--;
					output.WriteLine('}');
				}
			}
			base.Indent--;
			output.WriteLine('}');
		}
		
		protected override void GeneratePropertyReferenceExpression(CodePropertyReferenceExpression expression)
		{
			CodeExpression targetObject = expression.TargetObject;
			if (targetObject != null)
			{
				base.GenerateExpression(targetObject);
				base.Output.Write('.');
			}
			base.Output.Write(this.GetSafeName(expression.PropertyName));
		}
		
		protected override void GeneratePropertySetValueReferenceExpression(CodePropertySetValueReferenceExpression expression)
		{
			base.Output.Write("value");
		}
		
		protected override void GenerateRemoveEventStatement(CodeRemoveEventStatement statement)
		{
			TextWriter output = base.Output;
			this.GenerateEventReferenceExpression(statement.Event);
			output.Write(" -= ");
			base.GenerateExpression(statement.Listener);
			output.WriteLine(';');
		}
		
		protected override void GenerateSingleFloatValue(float f)
		{
			base.GenerateSingleFloatValue(f);
			base.Output.Write('F');
		}
		
		protected override void GenerateSnippetExpression(CodeSnippetExpression expression)
		{
			base.Output.Write(expression.Value);
		}
		
		protected override void GenerateSnippetMember(CodeSnippetTypeMember member)
		{
			base.Output.Write(member.Text);
		}
		
		protected override void GenerateThisReferenceExpression(CodeThisReferenceExpression expression)
		{
			base.Output.Write("this");
		}
		
		protected override void GenerateThrowExceptionStatement(CodeThrowExceptionStatement statement)
		{
			base.Output.Write("throw");
			if (statement.ToThrow != null)
			{
				base.Output.Write(' ');
				base.GenerateExpression(statement.ToThrow);
			}
			base.Output.WriteLine(";");
		}
		
		protected override void GenerateTryCatchFinallyStatement(CodeTryCatchFinallyStatement statement)
		{
			TextWriter output = base.Output;
			CodeGeneratorOptions options = base.Options;
			output.Write("try");
			this.OutputStartBrace();
			base.Indent++;
			base.GenerateStatements(statement.TryStatements);
			base.Indent--;
			IEnumerator enumerator = statement.CatchClauses.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					CodeCatchClause codeCatchClause = (CodeCatchClause)enumerator.Current;
					output.Write('}');
					if (options.ElseOnClosing)
					{
						output.Write(' ');
					}
					else
					{
						output.WriteLine();
					}
					output.Write("catch (");
					this.OutputTypeNamePair(codeCatchClause.CatchExceptionType, this.GetSafeName(codeCatchClause.LocalName));
					output.Write(")");
					this.OutputStartBrace();
					base.Indent++;
					base.GenerateStatements(codeCatchClause.Statements);
					base.Indent--;
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
			CodeStatementCollection finallyStatements = statement.FinallyStatements;
			if (finallyStatements.Count > 0)
			{
				output.Write('}');
				if (options.ElseOnClosing)
				{
					output.Write(' ');
				}
				else
				{
					output.WriteLine();
				}
				output.Write("finally");
				this.OutputStartBrace();
				base.Indent++;
				base.GenerateStatements(finallyStatements);
				base.Indent--;
			}
			output.WriteLine('}');
		}
		
		protected override void GenerateTypeConstructor(CodeTypeConstructor constructor)
		{
			if (base.IsCurrentDelegate || base.IsCurrentEnum || base.IsCurrentInterface)
			{
				return;
			}
			this.OutputAttributes(constructor.CustomAttributes, null, false);
			base.Output.Write("static " + this.GetSafeName(base.CurrentTypeName) + "()");
			this.OutputStartBrace();
			base.Indent++;
			base.GenerateStatements(constructor.Statements);
			base.Indent--;
			base.Output.WriteLine('}');
		}
		
		protected override void GenerateTypeEnd(CodeTypeDeclaration declaration)
		{
			if (!base.IsCurrentDelegate)
			{
				base.Indent--;
				base.Output.WriteLine("}");
			}
		}
		
		protected override void GenerateTypeOfExpression(CodeTypeOfExpression e)
		{
			base.Output.Write("typeof(");
			this.OutputType(e.Type);
			base.Output.Write(")");
		}
		
		protected override void GenerateTypeStart(CodeTypeDeclaration declaration)
		{
			TextWriter output = base.Output;
			this.OutputAttributes(declaration.CustomAttributes, null, false);
			if (!base.IsCurrentDelegate)
			{
				this.OutputTypeAttributes(declaration);
				output.Write(this.GetSafeName(declaration.Name));
				this.GenerateGenericsParameters(declaration.TypeParameters);
				IEnumerator enumerator = declaration.BaseTypes.GetEnumerator();
				if (enumerator.MoveNext())
				{
					CodeTypeReference t = (CodeTypeReference)enumerator.Current;
					output.Write(" : ");
					this.OutputType(t);
					while (enumerator.MoveNext())
					{
						t = (CodeTypeReference)enumerator.Current;
						output.Write(", ");
						this.OutputType(t);
					}
				}
				this.GenerateGenericsConstraints(declaration.TypeParameters);
				this.OutputStartBrace();
				base.Indent++;
			}
			else
			{
				if ((declaration.TypeAttributes & TypeAttributes.VisibilityMask) == TypeAttributes.Public)
				{
					output.Write("public ");
				}
				CodeTypeDelegate codeTypeDelegate = (CodeTypeDelegate)declaration;
				output.Write("delegate ");
				this.OutputType(codeTypeDelegate.ReturnType);
				output.Write(" ");
				output.Write(this.GetSafeName(declaration.Name));
				output.Write("(");
				this.OutputParameters(codeTypeDelegate.Parameters);
				output.WriteLine(");");
			}
		}
		
		protected override void GenerateVariableDeclarationStatement(CodeVariableDeclarationStatement statement)
		{
			TextWriter output = base.Output;
			this.OutputTypeNamePair(statement.Type, this.GetSafeName(statement.Name));
			CodeExpression initExpression = statement.InitExpression;
			if (initExpression != null)
			{
				output.Write(" = ");
				base.GenerateExpression(initExpression);
			}
			if (!this.dont_write_semicolon)
			{
				output.WriteLine(';');
			}
		}
		
		protected override void GenerateVariableReferenceExpression(CodeVariableReferenceExpression expression)
		{
			base.Output.Write(this.GetSafeName(expression.VariableName));
		}
		
		protected string GetSafeName(string id)
		{
			if (CSharpCodeGenerator.keywordsTable == null)
			{
				CSharpCodeGenerator.FillKeywordTable();
			}
			if (CSharpCodeGenerator.keywordsTable.Contains(id))
			{
				return "@" + id;
			}
			return id;
		}
		
		private string GetTypeArguments(CodeTypeReferenceCollection collection)
		{
			StringBuilder stringBuilder = new StringBuilder(" <");
			IEnumerator enumerator = collection.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					CodeTypeReference type = (CodeTypeReference)enumerator.Current;
					stringBuilder.Append(this.GetTypeOutput(type));
					stringBuilder.Append(", ");
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
			stringBuilder.Length--;
			stringBuilder[stringBuilder.Length - 1] = '>';
			return stringBuilder.ToString();
		}
		
		protected override string GetTypeOutput(CodeTypeReference type)
		{
			if ((type.Options & CodeTypeReferenceOptions.GenericTypeParameter) != (CodeTypeReferenceOptions)0)
			{
				return type.BaseType;
			}
			string text;
			if (type.ArrayElementType != null)
			{
				text = this.GetTypeOutput(type.ArrayElementType);
			}
			else
			{
				text = this.DetermineTypeOutput(type);
			}
			int i = type.ArrayRank;
			if (i > 0)
			{
				text += '[';
				for (i--; i > 0; i--)
				{
					text += ',';
				}
				text += ']';
			}
			return text;
		}
		
		protected override bool IsValidIdentifier(string identifier)
		{
			if (identifier == null || identifier.Length == 0)
			{
				return false;
			}
			if (CSharpCodeGenerator.keywordsTable == null)
			{
				CSharpCodeGenerator.FillKeywordTable();
			}
			if (CSharpCodeGenerator.keywordsTable.Contains(identifier))
			{
				return false;
			}
			if (!CSharpCodeGenerator.is_identifier_start_character(identifier[0]))
			{
				return false;
			}
			for (int i = 1; i < identifier.Length; i++)
			{
				if (!CSharpCodeGenerator.is_identifier_part_character(identifier[i]))
				{
					return false;
				}
			}
			return true;
		}
		
		private void OutputAttributeDeclaration(CodeAttributeDeclaration attribute)
		{
			base.Output.Write(attribute.Name.Replace('+', '.'));
			base.Output.Write('(');
			IEnumerator enumerator = attribute.Arguments.GetEnumerator();
			if (enumerator.MoveNext())
			{
				CodeAttributeArgument argument = (CodeAttributeArgument)enumerator.Current;
				this.OutputAttributeArgument(argument);
				while (enumerator.MoveNext())
				{
					base.Output.Write(", ");
					argument = (CodeAttributeArgument)enumerator.Current;
					this.OutputAttributeArgument(argument);
				}
			}
			base.Output.Write(')');
		}
		
		protected void OutputAttributes(CodeAttributeDeclarationCollection attributes, string prefix, bool inline)
		{
			bool flag = false;
			IEnumerator enumerator = attributes.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					CodeAttributeDeclaration codeAttributeDeclaration = (CodeAttributeDeclaration)enumerator.Current;
					if (codeAttributeDeclaration.Name == "System.ParamArrayAttribute")
					{
						flag = true;
					}
					else
					{
						this.GenerateAttributeDeclarationsStart(attributes);
						if (prefix != null)
						{
							base.Output.Write(prefix);
						}
						this.OutputAttributeDeclaration(codeAttributeDeclaration);
						this.GenerateAttributeDeclarationsEnd(attributes);
						if (inline)
						{
							base.Output.Write(" ");
						}
						else
						{
							base.Output.WriteLine();
						}
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
			if (flag)
			{
				if (prefix != null)
				{
					base.Output.Write(prefix);
				}
				base.Output.Write("params");
				if (inline)
				{
					base.Output.Write(" ");
				}
				else
				{
					base.Output.WriteLine();
				}
			}
		}
		
		protected override void OutputFieldScopeModifier(MemberAttributes attributes)
		{
			MemberAttributes memberAttributes = attributes & MemberAttributes.ScopeMask;
			if (memberAttributes == MemberAttributes.Static)
			{
				base.Output.Write("static ");
			}
			else
			{
				if (memberAttributes == MemberAttributes.Const)
				{
					base.Output.Write("const ");
				}
			}
		}
		
		protected override void OutputMemberAccessModifier(MemberAttributes attributes)
		{
			MemberAttributes memberAttributes = attributes & MemberAttributes.AccessMask;
			if (memberAttributes != MemberAttributes.Assembly && memberAttributes != MemberAttributes.FamilyAndAssembly)
			{
				if (memberAttributes != MemberAttributes.Family)
				{
					if (memberAttributes != MemberAttributes.FamilyOrAssembly)
					{
						if (memberAttributes != MemberAttributes.Private)
						{
							if (memberAttributes == MemberAttributes.Public)
							{
								base.Output.Write("public ");
							}
						}
						else
						{
							base.Output.Write("private ");
						}
					}
					else
					{
						base.Output.Write("protected internal ");
					}
				}
				else
				{
					base.Output.Write("protected ");
				}
			}
			else
			{
				base.Output.Write("internal ");
			}
		}
		
		protected override void OutputMemberScopeModifier(MemberAttributes attributes)
		{
			switch (attributes & MemberAttributes.ScopeMask)
			{
				case MemberAttributes.Abstract:
					base.Output.Write("abstract ");
					break;
				case MemberAttributes.Final:
					break;
				case MemberAttributes.Static:
					base.Output.Write("static ");
					break;
				case MemberAttributes.Override:
					base.Output.Write("override ");
					break;
				default:
				{
					MemberAttributes memberAttributes = attributes & MemberAttributes.AccessMask;
					if (memberAttributes == MemberAttributes.Assembly || memberAttributes == MemberAttributes.Family || memberAttributes == MemberAttributes.Public)
					{
						base.Output.Write("virtual ");
					}
					break;
				}
			}
		}
		
		protected void OutputStartBrace()
		{
			if (base.Options.BracingStyle == "C")
			{
				base.Output.WriteLine(string.Empty);
				base.Output.WriteLine("{");
			}
			else
			{
				base.Output.WriteLine(" {");
			}
		}
		
		protected override void OutputType(CodeTypeReference type)
		{
			base.Output.Write(this.GetTypeOutput(type));
		}
		
		private void OutputTypeArguments(CodeTypeReferenceCollection typeArguments, StringBuilder sb, int count)
		{
			if (count == 0)
			{
				return;
			}
			if (typeArguments.Count == 0)
			{
				sb.Append("<>");
				return;
			}
			sb.Append('<');
			sb.Append(this.GetTypeOutput(typeArguments[0]));
			for (int i = 1; i < count; i++)
			{
				sb.Append(", ");
				sb.Append(this.GetTypeOutput(typeArguments[i]));
			}
			sb.Append('>');
		}
		
		protected virtual void OutputTypeAttributes(CodeTypeDeclaration declaration)
		{
			TextWriter output = base.Output;
			TypeAttributes typeAttributes = declaration.TypeAttributes;
			switch (typeAttributes & TypeAttributes.VisibilityMask)
			{
				case TypeAttributes.NotPublic:
				case TypeAttributes.NestedAssembly:
				case TypeAttributes.NestedFamANDAssem:
					output.Write("internal ");
					break;
				case TypeAttributes.Public:
				case TypeAttributes.NestedPublic:
					output.Write("public ");
					break;
				case TypeAttributes.NestedPrivate:
					output.Write("private ");
					break;
				case TypeAttributes.NestedFamily:
					output.Write("protected ");
					break;
				case TypeAttributes.VisibilityMask:
					output.Write("protected internal ");
					break;
			}
			if ((declaration.Attributes & MemberAttributes.New) != (MemberAttributes)0)
			{
				output.Write("new ");
			}
			// BUGFIX: This previously was not checked, producing a non-sealed class when
			//         a sealed one was expected.
			bool wasFinal = false;
			if ((declaration.Attributes & MemberAttributes.Final) == MemberAttributes.Final)
			{
				wasFinal = true;
				output.Write("sealed ");
			}
			if (declaration.IsStruct)
			{
				if (declaration.IsPartial)
				{
					output.Write("partial ");
				}
				output.Write("struct ");
			}
			else
			{
				if (declaration.IsEnum)
				{
					output.Write("enum ");
				}
				else
				{
					if ((typeAttributes & TypeAttributes.ClassSemanticsMask) != TypeAttributes.NotPublic)
					{
						if (declaration.IsPartial)
						{
							output.Write("partial ");
						}
						output.Write("interface ");
					}
					else
					{
						if ((typeAttributes & TypeAttributes.Sealed) == TypeAttributes.Sealed && !wasFinal)
						{
							output.Write("sealed ");
						}
						if ((typeAttributes & TypeAttributes.Abstract) != TypeAttributes.NotPublic)
						{
							output.Write("abstract ");
						}
						if (declaration.IsPartial)
						{
							output.Write("partial ");
						}
						output.Write("class ");
					}
				}
			}
		}
		
		private void OutputVTableModifier(MemberAttributes attributes)
		{
			if ((attributes & MemberAttributes.VTableMask) == MemberAttributes.New)
			{
				base.Output.Write("new ");
			}
		}

		protected override string QuoteSnippetString(string value)
		{
			string text = value.Replace("\\", "\\\\");
			text = text.Replace("\"", "\\\"");
			text = text.Replace("\t", "\\t");
			text = text.Replace("\r", "\\r");
			text = text.Replace("\n", "\\n");
			return "\"" + text + "\"";
		}
		
		protected override bool Supports(GeneratorSupport supports)
		{
			return true;
		}
	}
}
