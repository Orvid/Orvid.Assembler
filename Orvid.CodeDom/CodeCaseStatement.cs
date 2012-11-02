using System;

namespace Orvid.CodeDom
{
	public class CodeCaseStatement : CodeStatement
	{
		public CodeExpression CaseExpression { get; set; }

		private readonly CodeStatementCollection mStatements = new CodeStatementCollection();
		public CodeStatementCollection Statements
		{
			get { return mStatements; }
		}

		protected CodeCaseStatement()
		{
		}

		public CodeCaseStatement(CodeExpression caseExpression)
		{
			this.CaseExpression = caseExpression;
		}
	}
}

