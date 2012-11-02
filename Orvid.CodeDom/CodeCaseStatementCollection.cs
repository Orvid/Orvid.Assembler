using System;
using System.Collections;

namespace Orvid.CodeDom
{
	[Serializable]
	public sealed class CodeCaseStatementCollection : CollectionBase
	{

		public CodeCaseStatementCollection()
		{
		}
		
		public CodeCaseStatementCollection(CodeCaseStatement[] value)
		{
			this.AddRange(value);
		}
		
		public CodeCaseStatementCollection(CodeCaseStatementCollection value)
		{
			this.AddRange(value);
		}


		public CodeCaseStatement this[int index]
		{
			get
			{
				return (CodeCaseStatement)base.List[index];
			}
			set
			{
				base.List[index] = value;
			}
		}

		
		public int Add(CodeCaseStatement value)
		{
			return base.List.Add(value);
		}
		
		public void AddRange(CodeCaseStatementCollection value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			int count = value.Count;
			for (int i = 0; i < count; i++)
			{
				this.Add(value[i]);
			}
		}
		
		public void AddRange(CodeCaseStatement[] value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			for (int i = 0; i < value.Length; i++)
			{
				this.Add(value[i]);
			}
		}
		
		public bool Contains(CodeCaseStatement value)
		{
			return base.List.Contains(value);
		}
		
		public void CopyTo(CodeCaseStatement[] array, int index)
		{
			base.List.CopyTo(array, index);
		}
		
		public int IndexOf(CodeCaseStatement value)
		{
			return base.List.IndexOf(value);
		}
		
		public void Insert(int index, CodeCaseStatement value)
		{
			base.List.Insert(index, value);
		}
		
		public void Remove(CodeCaseStatement value)
		{
			base.List.Remove(value);
		}
	}
}

