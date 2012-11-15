//using System;
//using System.Xml;
//using MonoDevelop.Projects;
//using MonoDevelop.Core;
//
//namespace Orvid.Assembler.Cpud.MonoDevelop
//{
//	public class CpudProject : Project, IProjectBinding, ILanguageBinding
//	{
//		#region ILanguageBinding implementation
//
//		public bool IsSourceCodeFile(FilePath fileName)
//		{
//			throw new NotImplementedException();
//		}
//
//		public FilePath GetFileName(FilePath fileNameWithoutExtension)
//		{
//			throw new NotImplementedException();
//		}
//
//		public string Language
//		{
//			get;
//		}
//
//		public string SingleLineCommentTag
//		{
//			get;
//		}
//
//		public string BlockCommentStartTag
//		{
//			get;
//		}
//
//		public string BlockCommentEndTag
//		{
//			get;
//		}
//
//		#endregion
//
//		public CpudProject()
//		{
//			
//		}
//
//		public override string ProjectType
//		{
//			get
//			{
//				return "CpudProject";
//			}
//		}
//
//		protected override BuildResult DoBuild(IProgressMonitor monitor, ConfigurationSelector configuration)
//		{
//
//			return base.DoBuild(monitor, configuration);
//		}
//
//		public Project CreateProject(ProjectCreateInformation info, XmlElement projectOptions)
//		{
//			return new CpudProject();
//		}
//
//		public Project CreateSingleFileProject(string sourceFile) { return null; }
//		public bool CanCreateSingleFileProject(string sourceFile) { return false; }
//
//	}
//}
//
