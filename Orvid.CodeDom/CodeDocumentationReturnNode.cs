using System;

namespace Orvid.CodeDom
{
	public sealed class CodeDocumentationReturnNode : CodeDocumentationNode
	{
		public string Summary { get; set; }

		public CodeDocumentationReturnNode() { }
		public CodeDocumentationReturnNode(string summary)
		{
			this.Summary = summary;
		}

	}
}

