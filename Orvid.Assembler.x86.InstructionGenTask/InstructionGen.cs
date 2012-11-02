using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Orvid.Assembler.x86.InstructionGenTask
{
	public sealed class InstructionGen : Task
	{
		private string cpudPath;
		[Required]
		public string CpudPath
		{
			get { return cpudPath; }
			set { cpudPath = value; }
		}

		private string outDir;
		[Required]
		public string OutputDirectory
		{
			get { return outDir; }
			set { outDir = value; }
		}

		private string architecture;
		[Required]
		public string Architecture
		{
			get { return architecture; }
			set { architecture = value; }
		}

		private string[] outputFiles;
		[Output]
		public string[] OutputFiles
		{
			get { return outputFiles; }
			set { outputFiles = value; }
		}

		public InstructionGen()
		{
		}

		public override bool Execute()
		{
			//throw new Exception();
			Console.WriteLine("Hello?");
			Log.LogMessage("Hello!");
			try
			{
				if (!Directory.Exists(OutputDirectory))
					Directory.CreateDirectory(OutputDirectory);
				string exeName = "Orvid.Assembler." + Architecture + ".InstructionGen.exe";
				if (!File.Exists(exeName))
				{
					Log.LogError("Unsupported architecture '{0}' (The generator could not be found)", Architecture);
					return false;
				}
				Process p = Process.Start(exeName, cpudPath + " -o \"" + OutputDirectory + "\"");
				while (!p.HasExited)
				{
					Thread.Sleep(100);
				}
				if (p.ExitCode != 0)
				{
					Log.LogError("The instruction generator exited with an error code other than 0 '{0}'!", p.ExitCode.ToString());
					return false;
				}
				StreamReader rdr = new StreamReader("instructionFileList.txt");
				List<string> fls = new List<string>();
				while(!rdr.EndOfStream)
				{
					string str = rdr.ReadLine().Trim();
					if (str != "")
					{
						fls.Add(str);
					}
				}
				rdr.Close();
				File.Delete("instructionFileList.txt");
				OutputFiles = fls.ToArray();
				return true;
			}
			catch (Exception e)
			{
				Log.LogErrorFromException(e);
				return false;
			}
		}

	}
}

