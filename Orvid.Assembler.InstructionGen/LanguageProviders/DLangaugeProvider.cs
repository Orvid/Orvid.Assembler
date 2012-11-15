using System;
using System.IO;
using System.Collections.Generic;
using Orvid.CodeDom;
using Orvid.CodeDom.Compiler;
using Orvid.CodeDom.CodeGenerators;

namespace Orvid.Assembler.InstructionGen.LanguageProviders
{
	public sealed class DLanguageProvider : LanguageProvider
	{
		protected override ICodeGenerator CodeGenerator { get { return new DCodeGenerator(); } }
		protected override string FileExtension { get { return ".d"; } }
		private List<string> GeneratedNamespaces = new List<string>(16);
		
		protected override void AddDefaultImports(CodeNamespace n)
		{
			n.Imports.Add(new CodeNamespaceImport("std.conv"));
			n.Imports.Add(new CodeNamespaceImport("std.string"));
			n.Imports.Add(new CodeNamespaceImport(RootNamespace + ".Core"));
			if (n.Name != RootNamespace)
			{
				n.Imports.Add(new CodeNamespaceImport(RootNamespace + ".Instructions.Core"));
			}
			string cun = n.Name;
			string filNam;
			if (cun != RootNamespace)
			{
				int li = cun.LastIndexOf('.');
				filNam = cun.Substring(li + 1);
				cun = cun.Substring(0, li) + ".Instructions";
			}
			else
			{
				cun += ".Instructions";
				filNam = "Core";
			}
			n.Name = cun + "." + filNam;
			GeneratedNamespaces.Add(n.Name);
		}

		protected override StreamWriter GetTargetStream(string destDirectory, CodeCompileUnit cu)
		{
			string cun = cu.Namespaces[0].Name;
			int li = cun.LastIndexOf('.');
			string filNam = cun.Substring(li + 1);
			cun = cun.Substring(0, li);

			string packagedDirectory = destDirectory + "/" + cun.Replace('.', '/');

			if (!Directory.Exists(packagedDirectory))
				Directory.CreateDirectory(packagedDirectory);

			return new StreamWriter(packagedDirectory + "/" + filNam + FileExtension, false);
		}

		public override CodeExpression GetPaddedHexToString(CodeExpression obj, int padSize)
		{
			return new CodeBinaryOperatorExpression(
				new CodePrimitiveExpression("0x"),
				CodeBinaryOperatorType.StringConcat,
				new CodeMethodInvokeExpression(
					new CodeMethodReferenceExpression()
					{
						MethodName = "format"
					},
					new CodePrimitiveExpression("%0" + padSize.ToString() + "X"),
					obj
				)
			);
		}

		public override void Finish(string destDirectory)
		{
			string outNmspc = RootNamespace + ".Instructions";
			string outDir = outNmspc.Replace('.', '/');
			var sw = new StreamWriter(destDirectory + "/" + outDir + "/Manifest.d", false);

			sw.Write("module ");
			sw.Write(outNmspc);
			sw.WriteLine(".Manifest;");
			sw.WriteLine();
			foreach (string n in GeneratedNamespaces)
			{
				sw.Write("public import ");
				sw.Write(n);
				sw.WriteLine(";");
			}

			sw.Flush();
			sw.Close();
		}
	}
}

