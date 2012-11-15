using System;

namespace Orvid.CodeDom
{
	public sealed class CodeDocumentationParameterNode : CodeDocumentationNode
	{
		public string ParamName { get; set; }
		public string Summary { get; set; }

		public CodeDocumentationParameterNode() { }
		public CodeDocumentationParameterNode(string paramName, string summary)
		{
			this.ParamName = paramName;
			this.Summary = summary;
		}
	}
}

