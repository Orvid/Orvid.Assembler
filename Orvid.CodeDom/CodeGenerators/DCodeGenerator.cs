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
	public class DCodeGenerator : CSharpCodeGenerator
	{
		protected override void OutputDocumentation(CodeDocumentationNodeCollection docs)
		{
			if (docs.Count > 0)
			{
				var output = base.Output;

				output.WriteLine("/** ");

				bool firstParam = true;
				bool prevWasSummary = false;
				// Probably need to have this sorted at some point.
				foreach (CodeDocumentationNode node in docs)
				{
					if (prevWasSummary)
					{
						output.WriteLine(" * ");
						prevWasSummary = false;
					}

					if (node is CodeDocumentationSummaryNode)
					{
						var n = (CodeDocumentationSummaryNode)node;
						foreach (string s in n.Lines)
						{
							output.Write(" * ");
							output.WriteLine(s);
						}
						prevWasSummary = true;
					}
					else if (node is CodeDocumentationParameterNode)
					{
						var n = (CodeDocumentationParameterNode)node;
						if (firstParam)
						{
							output.WriteLine(" * Params:");
							firstParam = false;
						}
						output.Write(" *     ");
						output.Write(n.ParamName);
						output.Write(" = ");
						output.WriteLine(n.Summary);
					}
					else if (node is CodeDocumentationReturnNode)
					{
						output.Write(" * Returns: ");
						output.WriteLine(((CodeDocumentationReturnNode)node).Summary);
					}
					else
					{
						throw new Exception("Unknown documentation node type!");
					}
				}

				output.WriteLine(" */");
			}
		}

		protected override void GenerateCastExpression(CodeCastExpression expression)
		{
			TextWriter output = base.Output;
			if (expression.NeedsGrouping)
				output.Write("(");
			output.Write("cast(");
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

		protected override void GenerateBaseReferenceExpression(CodeBaseReferenceExpression expression)
		{
			base.Output.Write("super");
		}

		protected override void OutputMemberScopeModifier(MemberAttributes attributes)
		{
			if ((attributes & MemberAttributes.Sealed) == MemberAttributes.Sealed)
				base.Output.Write("final ");
			base.OutputMemberScopeModifier(attributes);
		}

		protected override void GenerateConstructor(CodeConstructor constructor, CodeTypeDeclaration declaration)
		{	
			if (base.IsCurrentDelegate || base.IsCurrentEnum || base.IsCurrentInterface)
			{
				return;
			}
			this.OutputMemberAccessModifier(constructor.Attributes);
			base.Output.Write("this(");
			this.OutputParameters(constructor.Parameters);
			base.Output.Write(")");
			base.OutputStartBrace();
			base.Indent++;
			if (constructor.BaseConstructorArgs.Count > 0)
			{
				base.Output.Write("super(");
				this.OutputExpressionList(constructor.BaseConstructorArgs);
				base.Output.WriteLine(");");
			}
			if (constructor.ChainedConstructorArgs.Count > 0)
			{
				base.Output.Write("this(");
				this.OutputExpressionList(constructor.ChainedConstructorArgs);
				base.Output.WriteLine(");");
			}
			base.GenerateStatements(constructor.Statements);
			base.Indent--;
			base.Output.WriteLine('}');
		}

		protected override void GenerateNamespaceStart(CodeNamespace ns)
		{
			base.Output.Write("module ");
			base.Output.Write(ns.Name);
			base.Output.WriteLine(";");
			base.Output.WriteLine();
		}
		protected override void GenerateNamespaceEnd(CodeNamespace ns) { }

		protected override void GenerateNamespaceImport(CodeNamespaceImport import)
		{
			base.Output.Write("import ");
			base.Output.Write(import.Namespace);
			base.Output.WriteLine(";");
		}

		protected override void OutputOperator(CodeBinaryOperatorType op)
		{
			if (op == CodeBinaryOperatorType.StringConcat)
				base.Output.Write("~");
			else
				base.OutputOperator(op);
		}

		protected override void GenerateMethodInvokeExpression(CodeMethodInvokeExpression expression)
		{
			if (expression.Parameters.Count == 0 && expression.Method.MethodName == "ToString")
			{
				base.Output.Write("to!string(");
				if (expression.Method.TargetObject != null)
				{
					GenerateExpression(expression.Method.TargetObject);
				}
				base.Output.Write(")");
			}
			else
			{
				base.GenerateMethodInvokeExpression(expression);
			}
		}

		protected override string GetSafeName(string id)
		{
			if (id == "ToString")
				return "toString";
			return base.GetSafeName(id);
		}

		protected override void OutputFieldScopeModifier(MemberAttributes attributes)
		{
			// D Doesn't have a "readonly" modifier.
			if ((attributes & MemberAttributes.ScopeMask) != MemberAttributes.Final)
			{
				base.OutputFieldScopeModifier(attributes);
			}
		}

		protected override string DetermineTypeOutput(CodeTypeReference type)
		{
			switch (type.BaseType.ToLower(CultureInfo.InvariantCulture))
			{
				case "system.byte":
					return "ubyte";
				case "system.sbyte":
					return "byte";
				case "system.exception":
					return "Exception";
				case "system.argumentoutofrangeexception":
					return "Exception";
				default:
					return base.DetermineTypeOutput(type);
			}
		}

		protected override void OutputTypeAttributes(CodeTypeDeclaration declaration)
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
			//         a sealed one was expected. The same goes for static.
			bool wasFinal = false;
			if ((declaration.Attributes & MemberAttributes.ScopeMask) == MemberAttributes.Final)
			{
				wasFinal = true;
				output.Write("final ");
			}
			else if ((declaration.Attributes & MemberAttributes.ScopeMask) == MemberAttributes.Static)
			{
				output.Write("static ");
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
							output.Write("final ");
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

	}
}

