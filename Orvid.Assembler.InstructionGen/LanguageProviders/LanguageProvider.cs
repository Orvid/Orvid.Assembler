using System;
using System.IO;
using System.Collections.Generic;
using Orvid.CodeDom;
using Orvid.CodeDom.Compiler;

namespace Orvid.Assembler.InstructionGen.LanguageProviders
{
	public abstract class LanguageProvider
	{
		/// <summary>
		/// Get a language provider for the specified language.
		/// </summary>
		/// <param name='lang'>The language to retrieve a provider for.</param>
		public static LanguageProvider GetProvider(Language lang)
		{
			switch (lang)
			{
				case Language.CSharp:
					return new CSharpLanguageProvider();
				case Language.D:
					return new DLanguageProvider();
				default:
					throw new Exception("Unknown language!");
			}
		}


		public string RootNamespace;
		
		protected List<CodeCompileUnit> CompileUnits = new List<CodeCompileUnit>();
		protected Dictionary<string, CodeNamespace> Namespaces = new Dictionary<string, CodeNamespace>();


		protected abstract void AddDefaultImports(CodeNamespace n);
		public virtual CodeNamespace GetNamespace(string name)
		{
			CodeNamespace nm;
			if (!Namespaces.TryGetValue(name, out nm))
			{
				CodeCompileUnit cu = new CodeCompileUnit();
				CodeNamespace n = new CodeNamespace(name);
				AddDefaultImports(n);
				cu.Namespaces.Add(n);
				CompileUnits.Add(cu);
				Namespaces[name] = n;
				nm = n;
			}
			return nm;
		}

		protected abstract ICodeGenerator CodeGenerator { get; }
		protected abstract string FileExtension { get; }
		public virtual void WriteToFile(string destDirectory)
		{
			var cgO = new CodeGeneratorOptions();
			cgO.IndentString = "\t";
			cgO.BracingStyle = "C";
			cgO.ElseOnClosing = false;
			ICodeGenerator gen = CodeGenerator;
			
			foreach (CodeCompileUnit cu in CompileUnits)
			{
				var tw = GetTargetStream(destDirectory, cu);
				gen.GenerateCodeFromCompileUnit(cu, tw, cgO);
				tw.Flush();
				tw.Close();
			}
		}

		protected virtual StreamWriter GetTargetStream(string destDirectory, CodeCompileUnit cu)
		{
			if (!Directory.Exists(destDirectory))
				Directory.CreateDirectory(destDirectory);
			return new StreamWriter(destDirectory + "/" + cu.Namespaces[0].Name + FileExtension, false);
		}

		public virtual CodeExpression GetPaddedHexToString(CodeExpression obj, int padSize)
		{
			return new CodeBinaryOperatorExpression(
				new CodePrimitiveExpression("0x"),
				CodeBinaryOperatorType.StringConcat,
				new CodeMethodInvokeExpression(
					new CodeMethodInvokeExpression(
						obj,
						"ToString",
						new CodePrimitiveExpression("X")
					),
					"PadLeft",
					new CodePrimitiveExpression(padSize),
					new CodePrimitiveExpression('0')
				)
			);
		}

		public virtual void Finish(string destDirectory)
		{

		}

	}
}

