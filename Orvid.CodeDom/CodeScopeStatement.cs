using System;

namespace Orvid.CodeDom
{
	public sealed class CodeScopeStatement : CodeStatement
	{
		private CodeStatementCollection mStatements = new CodeStatementCollection();
		public CodeStatementCollection Statements
		{
			get { return mStatements; }
		}

		public CodeScopeStatement(params CodeStatement[] statements)
		{
			if (statements.Length > 0)
			{
				mStatements.AddRange(statements);
			}
		}

		internal override void Accept(ICodeDomVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}

