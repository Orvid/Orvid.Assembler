using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using IO = System.IO;
using RE = System.Text.RegularExpressions;
using MonoDevelop.Ide;
using MonoDevelop.Core;
using Mono.TextEditor;
using Mono.TextEditor.Highlighting;
using System.Threading;

namespace Orvid.Assembler.Cpud.MonoDevelop
{
	public sealed class CpudSyntaxMode : SyntaxMode
	{
		private const string Rule_XmlDocumentation = "XmlDocumentation";
		private const string Rule_XmlDocumentation_Tag = "XmlDocumentationTag";
		private const string Rule_Comment = "Comment";
		private const string Rule_NamespaceDeclaration = "NamespaceDeclaration";
		private const string Rule_Info = "Info";
		private const string Rule_String = "String";

		public CpudSyntaxMode()
		{
			#region Attempt at manual creation (not currently possible due to access modifiers in multiple places)

//			this.Delimiter = @"#(){}[] ,	";
//			this.properties = new Dictionary<string, List<string>>()
//			{
//				{ 
//					"LineComment",
//					new List<string>()
//					{
//						"//"
//					}
//				}
//			};
//
//			#region Spans
//			{
//				this.spans = new Span[]
//				{
//					// First up, the EOL spans
//					new Span()
//					{ 
//						Color = "comment.doc",
//						TagColor = "comment.tag.doc",
//						Rule = Rule_XmlDocumentation,
//						Begin = new Regex("///"),
//						StopAtEol = true,
//					},
//					new Span()
//					{
//						Color = "comment.line",
//						TagColor = "comment.tag.line",
//						Rule = Rule_Comment,
//						Begin = new Regex("//"),
//						StopAtEol = true,
//					},
//					new Span()
//					{
//						Color = "text.preprocessor",
//						TagColor = "string",
//						Rule = Rule_NamespaceDeclaration,
//						Begin = new Regex("namespace"),
//						StopAtEol = true,
//					},
//					new Span()
//					{
//						Color = "text",
//						TagColor = "text.preprocessor.keyword",
//						Rule = Rule_Info,
//						Begin = new Regex("#"),
//						StopAtEol = true,
//					},
//					// Then the normal spans
//					new Span()
//					{
//						Color = "string.double",
//						Rule = Rule_String,
//						Escape = @"\",
//						StopAtEol = true,
//						Begin = new Regex("\""),
//						End = new Regex("\""),
//					},
//				};
//			}
//			#endregion
//
//			#region Keywords
//			{
//
//				#region CPU Describing
//				this.keywords.Add(
//					new Keywords()
//					{
//						Color = "keyword",
//						IgnoreCase = false,
//						Words = new List<string>()
//						{
//							"DirectCast",
//							"byte",
//							"int",
//						},
//					}
//				);
//				this.keywords.Add(
//					new Keywords()
//					{
//						Color = "string",
//						IgnoreCase = false,
//						Words = new List<string>()
//						{
//							"AsArg",
//							"Write",
//							"Read",
//							"Size",
//							"Expand",
//							"RequiresSeg",
//						},
//					}
//				);
//				// Built-in Arguments
//				this.keywords.Add(
//					new Keywords()
//					{
//						Color = "keyword.semantic.type",
//						IgnoreCase = false,
//						Words = new List<string>()
//						{
//							"ParentAssembler",
//							"Segment",
//							"Stream",
//						},
//					}
//				);
//				#endregion
//
//				
//				this.keywords.Add(
//					new Keywords()
//					{
//						Color = "text.preprocessor.keyword",
//						IgnoreCase = false,
//						Words = new List<string>()
//						{
//							"cpu",
//						},
//					}
//				);
//				this.keywords.Add(
//					new Keywords()
//					{
//						Color = "text.preprocessor",
//						IgnoreCase = false,
//						Words = new List<string>()
//						{
//							"x86",
//						},
//					}
//				);
//
//				this.keywords.Add(
//					new Keywords()
//					{
//						Color = "string",
//						IgnoreCase = true,
//						Words = new List<string>()
//						{
//							"override",
//							"docalias",
//
//							"!=",
//							"==",
//							"<=",
//							">=",
//							"<",
//							">",
//							"+",
//							"-",
//							"=",
//						},
//					}
//				);
//				this.keywords.Add(
//					new Keywords()
//					{
//						Color = "keyword",
//						IgnoreCase = true,
//						Words = new List<string>()
//						{
//							"imm8",
//							"imm16",
//							"imm32",
//							
//							"arg1",
//							"arg2",
//							"arg3",
//							
//							"evil",
//							
//							"exact",
//							"emitonly",
//							"fits",
//							"comp",
//						},
//					}
//				);
//				this.keywords.Add(
//					new Keywords()
//					{
//						Color = "keyword.semantic.type",
//						IgnoreCase = true,
//						Words = new List<string>()
//						{
//							"none",
//						},
//					}
//				);
//
//
//				// Finally initialize the keyword tables.
//				if (this.keywordTable == null)
//					this.keywordTable = new Dictionary<string, Keywords>();
//				if (this.keywordTableIgnoreCase == null)
//					this.keywordTableIgnoreCase = new Dictionary<string, Keywords>(StringComparer.InvariantCultureIgnoreCase);
//				foreach(Keywords wrds in this.keywords)
//				{
//					if (wrds.IgnoreCase)
//					{
//						foreach(string word in wrds.Words)
//						{
//							this.keywordTableIgnoreCase[word] = wrds;
//						}
//					}
//					else
//					{
//						foreach(string word in wrds.Words)
//						{
//							this.keywordTable[word] = wrds;
//						}
//					}
//				}
//			}
//			#endregion
//
//			#region Matches
//			{
//				this.matches = new Match[]
//				{
//					new CSharpNumberMatch()
//					{
//						Color = "text.markup",
//					}
//				};
//			}
//			#endregion
//
//			#region Rules
//			{
//				#region Comment
//				{
//					this.rules.Add(
//						new Rule(this)
//						{
//							Name = Rule_Comment,
//						}
//					);
//				}
//				#endregion
//
//				#region XmlDocumentation
//				{
//					this.rules.Add(
//						new Rule(this)
//						{
//							Name = Rule_XmlDocumentation,
//							spans = new List<Span>()
//							{
//								new Span()
//								{
//									Color = "comment.tag.doc",
//									Rule = Rule_XmlDocumentation_Tag,
//									StopAtEol = true,
//									Begin = new Regex("<"),
//									End = new Regex(">"),
//								}
//							},
//						}
//					);
//				}
//				#endregion
//
//
//			}
//			#endregion

			#endregion

			var provider = new ResourceXmlProvider(typeof(CpudSyntaxMode).Assembly, "CpudSyntaxMode.xml");
			using (var reader = provider.Open())
			{
				var baseMode = SyntaxMode.Read(reader);
				this.rules = new List<Rule>(baseMode.Rules);
				this.keywords = new List<Keywords>(baseMode.Keywords);
				//this.keywords.Add(wordsSet);

				this.spans = baseMode.Spans;
				this.matches = baseMode.Matches;
				this.prevMarker = baseMode.PrevMarker;
				this.SemanticRules = new List<SemanticRule>(baseMode.SemanticRules);
				this.keywordTable = baseMode.keywordTable;
				this.keywordTableIgnoreCase = baseMode.keywordTableIgnoreCase;
				this.properties = baseMode.Properties;
			}
			if (LogThread == null)
			{
				(LogThread = new Thread(LogThreadMain)).Start();
			}
			else
			{
				// Depending on the exact environment we're running in,
				// this may not actually take, but there's a good chance
				// it will, meaning 1 less comparison in the main log
				// loop.
				ShutdownLogThread = false;
			}
			ID = Interlocked.Increment(ref LastModeID);
			LogWriteLine("Mode Created");
		}

		~CpudSyntaxMode()
		{
			LogWriteLine("Mode Destroyed");
		}

		private sealed class ArgumentDescription
		{
			public sealed class ArgumentDescriptionComparer : IComparer<ArgumentDescription>
			{
				public int Compare(ArgumentDescription x, ArgumentDescription y)
				{
					if (x.EndOffset <= y.StartOffset)
					{
						return -1;
					}
					else if (x.StartOffset >= y.EndOffset)
					{
						return 1;
					}
					else
					{
						// This actually means that the descriptions are overlapping,
						// so we may have to do something about it at some point.
						return 0;
					}
				}
			}
			// These first 2 are the only fields used for actual comparisons,
			// the rest are just for ease of use.
			public int StartOffset;
			public int EndOffset;
			public string Name;

			public override string ToString()
			{
				return Name + " @ " + StartOffset.ToString() + "-" + EndOffset.ToString();
			}
		}

		protected override void OnDocumentSet(EventArgs e)
		{
			base.OnDocumentSet(e);
			LogWriteLine("Document Set: " + (Document != null ? Document.ToString() : "(null)"));
		}

		private object ArgDescLock = new object();
		private SortedSet<ArgumentDescription> ArgumentDescriptions = new SortedSet<ArgumentDescription>(new ArgumentDescription.ArgumentDescriptionComparer());
		private Keywords wordsSet = Mono.TextEditor.Highlighting.Keywords.Read(new System.Xml.XmlTextReader(new IO.StringReader("<Keywords color=\"keyword\" ignorecase=\"true\" />")), true);
		// Yes the regex is long, but it works.
		private static readonly RE.Regex ArgumentDescriptionRegex = new RE.Regex(
			@"^#define[\s]+arg\((((?<Name>[a-zA-Z0-9/_]+)([\s]*,[\s]*[a-zA-Z0-9/_]+)?[\s]*\)[\s]*:[\s]*[a-zA-Z0-9/_]+\([\s]*[0-9]+[\s]*\)$)|((?<Name>[a-zA-Z0-9/_]+)[\s]*,[\s]*[a-zA-Z0-9/_]*[\s]*,[\s]*[a-zA-Z0-9/_]+[\s]*,[\s]*[a-zA-Z0-9/_]*[\s]*\)))",
			RE.RegexOptions.Compiled | RE.RegexOptions.Multiline
		);

		public override IEnumerable<Chunk> GetChunks(ColorScheme style, DocumentLine line, int offset, int length)
		{
			SemanticCore(doc.GetText(line.Segment), offset);
			return ProcessChunks(base.GetChunks(style, line, offset, length));
		}

		private void SemanticCore(string txt, int offset)
		{
			RE.MatchCollection mCol = ArgumentDescriptionRegex.Matches(txt);
			foreach (RE.Match m in mCol)
			{
				string matchName = m.Groups["Name"].Value;
				int mo = m.Index + offset;
				int mlo = mo + m.Length;
				lock(ArgDescLock)
				{
					var desc = ArgumentDescriptions.FirstOrDefault(d => d.StartOffset <= mo && d.EndOffset >= mlo);
					if (desc != null)
					{
						if (desc.Name == matchName)
						{
							continue;
						}
						else
						{
							RemoveArg(desc);
						}
					}
					ArgumentDescription argDesc = new ArgumentDescription()
					{
						StartOffset = mo,
						EndOffset = mlo,
						Name = matchName,
					};
					AddArg(argDesc);
				}
			}
		}

		private void AddArg(ArgumentDescription desc)
		{
			LogWriteLine("Adding arg " + desc.ToString());
			//((List<string>)wordsSet.Words).Add(desc.Name);
			//this.keywordTableIgnoreCase[desc.Name] = wordsSet;
			ArgumentDescriptions.Add(desc);
		}

		private void RemoveArg(ArgumentDescription desc)
		{
			LogWriteLine("Removing arg " + desc.ToString());
			//((List<string>)wordsSet.Words).Remove(desc.Name);
			//this.keywordTableIgnoreCase.Remove(desc.Name);
			ArgumentDescriptions.Remove(desc);
		}

		private int ID;
		private void LogWriteLine(string msg)
		{
			Messages.Enqueue(ID.ToString() + ": " + msg);
		}

		private static readonly ConcurrentQueue<string> Messages = new ConcurrentQueue<string>();
		private static Thread LogThread;
		private static int LastModeID = 0;
		private static bool ShutdownLogThread = false;
		
		private static void OnProcessExit(object sender, EventArgs e)
		{
			ShutdownLogThread = true;
		}

		private static void LogThreadMain()
		{
//			string msg;
//			IO.StreamWriter sw = new IO.StreamWriter("/Users/dyke5094/Desktop/lg.txt", true);
//			sw.AutoFlush = false;
//			sw.WriteLine();
//			sw.WriteLine("STARTED @ " + DateTime.Now.ToString());
//			sw.Flush();
//			AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
//			while (true)
//			{
//				if (Messages.Count > 0)
//				{
//					string curTimeString = DateTime.Now.ToShortTimeString();
//					// The write operation here is fast enough, and the input data
//					// sparse enough, that they should all be written in under a
//					// second, which means calculating the current time string every
//					// iteration is just a waste of CPU time.
//					while (Messages.Count > 0)
//					{
//						if (!Messages.TryDequeue(out msg))
//						{
//							throw new Exception("Something is very wrong here!");
//						}
//						sw.WriteLine(curTimeString + ": " + msg);
//						sw.Flush();
//					}
//				}
//				else
//				{
//					// This is here, and not in the main loop so that all pending
//					// messages get written before shutting down the log thread.
//					if (ShutdownLogThread)
//						break;
//					Thread.Sleep(100);
//				}
//			}
//			sw.WriteLine("SHUTDOWN @ " + DateTime.Now.ToString());
//			sw.Flush();
//			sw.Dispose();
		}

		private IEnumerable<Chunk> ProcessChunks(IEnumerable<Chunk> chunks)
		{
//			foreach (Chunk c in chunks)
//			{
//				if (ArgTypeNames.Contains(doc.GetTextAt(c).ToLower()))
//				{
//					c.Style = "keyword";
//				}
//			}

			return chunks;
		}
	}
}

