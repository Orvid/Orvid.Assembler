//#define DumpTokens
//#define CatchExceptions
using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using Orvid.Assembler.InstructionGen.LanguageProviders;

namespace Orvid.Assembler.InstructionGen
{
	public static class MainClass
	{
		private const string ProfileGenDefine = "ProfileGen";

		public static void Main(string[] args)
		{
			string errMsg = "";
			bool defaultOutDir = true;
			string outDir = Directory.GetCurrentDirectory() + "/out";
			List<string> inFiles = new List<string>();
			List<Language> langs = new List<Language>();
			for (int i = 0; i < args.Length; i++)
			{
				string s = args[i];
				if (s.StartsWith("-o"))
				{
					if (i + 1 >= args.Length)
					{
						errMsg = "Expected an output directory after '-o'!";
						goto Die;
					}
					outDir = Path.GetFullPath(args[i + 1]);
					defaultOutDir = false;
					i++; // Skip the path
				}
				else if (s.StartsWith("-l"))
				{
					if (i + 1 >= args.Length)
					{
						errMsg = "Expected a language after '-l'!";
						goto Die;
					}
					Language l;
					if (!Enum.TryParse(args[i + 1], true, out l))
					{
						errMsg = "Unknown language '" + args[i + 1] + "'!";
						goto Die;
					}
					if (langs.Contains(l))
					{
						errMsg = "Language '" + l.ToString() + "' was already added as an output language!";
						goto Die;
					}
					langs.Add(l);
					i++;
				}
				else
				{
					string fle = Path.GetFullPath(s.Trim());
					if (!File.Exists(fle))
					{
						errMsg = "The input file '" + fle + "' doesn't exist!";
						goto Die;
					}
					inFiles.Add(fle);
				}
			}
			if (langs.Count == 0)
				langs.Add(Language.CSharp);

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
			foreach (string inFile in inFiles)
			{	
				CurrentFileShortName = Path.GetFileName(inFile);
				Token[] toks;
				using (FileStream strm = new FileStream(inFile, FileMode.Open, FileAccess.Read))
				{
					StreamReader rdr = new StreamReader(strm);
					string fileData = rdr.ReadToEnd();
					Stopwatch st = new Stopwatch();
					st.Start();
					toks = CpudTokenizer.Tokenize(fileData);
					st.Stop();
					if (toks.Length > 0)
					{
						ProfilingWrite("Tokenizing took " + st.ElapsedMilliseconds + "MS to generate " + toks.Length.ToString() + " tokens (" + (System.Runtime.InteropServices.Marshal.SizeOf(typeof(Token)) * toks.Length).ToString() + " bytes)");
					}
					DumpTokens(toks);
					fileData = String.Empty;
				}

				// Unfortunately, due to the way things were written,
				// to output multiple languages we have to load it 
				// multiple times.
				foreach(Language l in langs)
				{
					CurrentFileShortName = Path.GetFileName(inFile);
#if CatchExceptions
					try
					{
#endif
						ProfilingStart(stop);
						g = new Generator(toks);
						ProfilingStop(stop);
						ProfilingAdd(stop, ref TotalMSElapsed);
						ProfilingWrite("Parsing for '" + l.ToString() + "' took " + stop.ElapsedMilliseconds.ToString() + "MS.");
						ProfilingReset(stop);
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
						g.WriteInstructions(outDir, l);
						ProfilingStop(stop);
						ProfilingAdd(stop, ref TotalMSElapsed);
						ProfilingWrite("Writing out the instructions for '" + l.ToString() + "' took " + stop.ElapsedMilliseconds.ToString() + "MS.");
						ProfilingReset(stop);
#if CatchExceptions
					}
					catch (Exception e)
					{
						errMsg = "An error occured while writing out the instructions:\r\nErrorType: " + e.GetType().ToString() + "\r\n" + e.Message;
						goto Die;
					}
#endif
					ResetEverything();
				}
			}
			ProfilingWrite("Processing took a total of " + TotalMSElapsed.ToString() + "MS.");
		Die:
			if (errMsg != "")
			{
				Console.WriteLine("An error occured while generating the instructions: " + errMsg);
			}
		}

		public static void ResetEverything()
		{
			StaticTypeReferences.Reset();
			FieldTypeRegistry.Reset();
			SizelessTypeRegistry.Reset();
			InstructionArgTypeRegistry.Reset();
			DocAliasRegistry.Reset();
			PrefixRegistry.Reset();
			EnumRegistry.Reset();
			InstructionFormEnumRegistry.Reset();
			BitPatternRegistry.Reset();
			CurrentFileShortName = null;
		}

		private static string CurrentFileShortName;

		
		[Conditional("DumpTokens")]
		private static void DumpTokens(Token[] toks)
		{
			StreamWriter rtr = new StreamWriter("TokenDump.txt", false);
			rtr.WriteLine("Writing a total of " + toks.Length.ToString() + " tokens.");
			rtr.WriteLine();
			for (int i = 0; i < toks.Length; i++)
			{
				rtr.WriteLine(toks[i].ToString());
			}
			rtr.Flush();
			rtr.Close();
		}

		[Conditional(ProfileGenDefine)]
		public static void ProfilingWrite(string str)
		{
			if (CurrentFileShortName != null)
			{
				Console.WriteLine(CurrentFileShortName + ": " + str);
			}
			else
			{
				Console.WriteLine(str);
			}
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
