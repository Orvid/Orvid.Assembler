using System;
using System.Collections.Generic;

namespace Orvid.CodeDom
{
	public sealed class CodeDocumentationSummaryNode : CodeDocumentationNode
	{
		private List<string> mLines = new List<string>(10);
		public List<string> Lines { get { return mLines; } }

		public CodeDocumentationSummaryNode() { }
		public CodeDocumentationSummaryNode(params string[] linesOfDocumentation)
		{
			this.mLines.AddRange(linesOfDocumentation);
		}

		public CodeDocumentationSummaryNode(IEnumerable<string> linesOfDocumentation)
		{
			this.mLines.AddRange(linesOfDocumentation);
		}
	}
}

