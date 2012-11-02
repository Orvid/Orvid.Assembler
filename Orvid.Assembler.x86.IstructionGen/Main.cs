//#define CatchExceptions
#define ProfileGen
using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace Orvid.Assembler.x86.IstructionGen
{
	class MainClass
	{
		private const string ProfileGenDefine = "ProfileGen";
//		private static readonly System.Text.RegularExpressions.Regex ArgumentDescriptionRegex = 
//			new System.Text.RegularExpressions.Regex(
//				@"^#define[\s]+arg\(([a-zA-Z0-9/_]+([\s]*,[\s]*[a-zA-Z0-9/_]+)?[\s]*\)[\s]*:[\s]*[a-zA-Z0-9/_]+\([\s]*[0-9]+[\s]*\)$)|([a-zA-Z0-9/_]+[\s]*,[\s]*[a-zA-Z0-9/_]*[\s]*,[\s]*[a-zA-Z0-9/_]+[\s]*,[\s]*[a-zA-Z0-9/_]*[\s]*\))",
//				System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.Multiline);

		public static void Main(string[] args)
		{
//			const string TestStringA = "#define arg(MemIndex16)  : MemIndex8( 2)";
//			const string TestStringB = "#define arg(ModMem16,  Mem16)  : ModMem8( 2)";
//			const string TestStringC = "#define arg(Membase8, , Membase, ) as x86Register(BaseRegister, DocAlias(BaseRegister)), Int(Offset, DocAlias(Offset))";
//			const string TestStringD = "#define arg(Membase32, Membase32, Membase, Membase) as x86Register(BaseRegister, DocAlias(BaseRegister)), Int(Offset, DocAlias(Offset))";
//			bool matchA, matchB, matchC, matchD;
//			matchA = ArgumentDescriptionRegex.Match(TestStringA).Success;
//			matchB = ArgumentDescriptionRegex.Match(TestStringB).Success;
//			matchC = ArgumentDescriptionRegex.Match(TestStringC).Success;
//			matchD = ArgumentDescriptionRegex.Match(TestStringD).Success;
//			int i43 = 0;
//			i43++;
			string errMsg = "";
			bool defaultOutDir = true;
			string outDir = Directory.GetCurrentDirectory() + "/out";
			string inFile = "";
			for (int i = 0; i < args.Length; i++)
			{
				string s = args[i];
				if (s.StartsWith("-o"))
				{
					if (i + 2 > args.Length)
					{
						errMsg = "Expected an output directory after '-o'!";
						goto Die;
					}
					outDir = Path.GetFullPath(args[i + 1]);
					defaultOutDir = false;
				}
				else
				{
					inFile = Path.GetFullPath(s.Trim());
				}
			}
			if (!File.Exists(inFile))
			{
				errMsg = "Input file '" + inFile + "' doesn't exist!";
				goto Die;
			}
			if (!Directory.Exists(outDir))
			{
				if (defaultOutDir)
				{
					Directory.CreateDirectory(outDir);
				}
				else
				{
					errMsg = "Output directory '" + outDir + "' doesn't exit!";
					goto Die;
				}
			}

			Generator g;
			Stopwatch stop = new Stopwatch();
			long TotalMSElapsed = 0;
#if CatchExceptions
			try
			{
#endif
			FileStream strm = new FileStream(inFile, FileMode.Open, FileAccess.Read);
			ProfilingPreCall(strm, outDir);
			ProfilingStart(stop);
			g = new Generator(strm);
			ProfilingStop(stop);
			strm.Close();
			ProfilingAdd(stop, ref TotalMSElapsed);
			ProfilingWrite("Parsing took " + stop.ElapsedMilliseconds.ToString() + "MS.");
			ProfilingReset(stop);
			// Why do we manually trigger a GC? Because we have nearly 16k objects that
			// are now no longer referenced, and will just slow down everything after this.
			GC.Collect();
#if CatchExceptions
			}
			catch (Exception e)
			{
				errMsg = "An error occured while parsing the cpud file:\r\n" + e.Message;
				goto Die;
			}
			try
			{
#endif
			ProfilingStart(stop);
			g.WriteInstructions(outDir);
			ProfilingStop(stop);
			ProfilingAdd(stop, ref TotalMSElapsed);
			ProfilingWrite("Writing out the intstructions took " + stop.ElapsedMilliseconds.ToString() + "MS.");
			ProfilingReset(stop);
#if CatchExceptions
			}
			catch (Exception e)
			{
				errMsg = "An error occured while writing out the instructions:\r\nErrorType: " + e.GetType().ToString() + "\r\n" + e.Message;
				goto Die;
			}
#endif
			ProfilingWrite("Processing took a total of " + TotalMSElapsed.ToString() + "MS.");
			Die:
			if (errMsg != "")
			{
				Console.WriteLine("An error occured while generating the instructions: " + errMsg);
			}
		}

		[Conditional(ProfileGenDefine)]
		private static void ProfilingPreCall(FileStream strm, string outDir)
		{
			strm.Position = 0;
			var a = new Generator(strm);
			a.WriteInstructions(outDir);
			StaticTypeReferences.Reset();
			FieldTypeRegistry.Reset();
			SizelessTypeRegistry.Reset();
			InstructionArgTypeRegistry.Reset();
			DocAliasRegistry.Reset();
			PrefixRegistry.Reset();
			EnumRegistry.Reset();
			strm.Position = 0;
			GC.Collect();
		}

		[Conditional(ProfileGenDefine)]
		private static void ProfilingWrite(string str)
		{
			Console.WriteLine(str);
		}

		[Conditional(ProfileGenDefine)]
		private static void ProfilingReset(Stopwatch s)
		{
			s.Reset();
		}
		
		[Conditional(ProfileGenDefine)]
		private static void ProfilingStart(Stopwatch s)
		{
			s.Start();
		}
		
		[Conditional(ProfileGenDefine)]
		private static void ProfilingStop(Stopwatch s)
		{
			s.Stop();
		}
		
		[Conditional(ProfileGenDefine)]
		private static void ProfilingAdd(Stopwatch s, ref long totalMS)
		{
			totalMS += s.ElapsedMilliseconds;
		}
	}
}
