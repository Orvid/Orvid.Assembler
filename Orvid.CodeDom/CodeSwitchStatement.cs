using System;

namespace Orvid.CodeDom
{
	/// <summary>
	/// Represents a switch statement.
	/// </summary>
	public sealed class CodeSwitchStatement : CodeStatement
	{
		/// <summary>
		/// The value this statement is switched
		/// on.
		/// </summary>
		public CodeExpression SwitchedOn { get; set; }

		private readonly CodeCaseStatementCollection mCases = new CodeCaseStatementCollection();
		public CodeCaseStatementCollection Cases
		{
			get { return mCases; }
		}

		public CodeSwitchStatement(CodeExpression switchedOn)
		{
			this.SwitchedOn = switchedOn;
		}

		internal override void Accept(ICodeDomVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}

