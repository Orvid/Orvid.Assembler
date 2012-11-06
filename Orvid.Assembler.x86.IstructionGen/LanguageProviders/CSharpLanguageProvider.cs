using System;
using Orvid.CodeDom;
using Orvid.CodeDom.Compiler;
using Orvid.CodeDom.CodeGenerators;

namespace Orvid.Assembler.x86.IstructionGen.LanguageProviders
{
	public sealed class CSharpLanguageProvider : LanguageProvider
	{
		protected override ICodeGenerator CodeGenerator { get { return new CSharpCodeGenerator(); } }
		protected override string FileExtension { get { return ".cs"; } }

		protected override void AddDefaultImports(CodeNamespace n)
		{
			n.Imports.Add(new CodeNamespaceImport("System"));
			if (n.Name != RootNamespace)
			{
				n.Imports.Add(new CodeNamespaceImport(RootNamespace));
			}
		}
	}
}

