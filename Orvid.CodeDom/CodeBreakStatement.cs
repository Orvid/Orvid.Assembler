using System;

namespace Orvid.CodeDom
{
	public sealed class CodeBreakStatement : CodeStatement
	{
		public CodeBreakStatement()
		{
		}
		
		internal override void Accept(ICodeDomVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}

