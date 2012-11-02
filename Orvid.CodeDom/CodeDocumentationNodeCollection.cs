using System;
using System.Collections;

namespace Orvid.CodeDom
{
	public sealed class CodeDocumentationNodeCollection : CollectionBase
	{
		//
		// Constructors
		//
		public CodeDocumentationNodeCollection()
		{
		}
		
		public CodeDocumentationNodeCollection(params CodeDocumentationNode[] value)
		{
			AddRange(value);
		}

		public CodeDocumentationNodeCollection(CodeDocumentationNodeCollection value)
		{
			AddRange(value);
		}

		//
		// Properties
		//
		public CodeDocumentationNode this[int index]
		{
			get
			{
				return (CodeDocumentationNode)List[index];
			}
			set
			{
				List[index] = value;
			}
		}

		//
		// Methods
		//
		public int Add(CodeDocumentationNode value)
		{
			return List.Add(value);
		}

		public void AddRange(params CodeDocumentationNode[] value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}

			for (int i = 0; i < value.Length; i++)
			{
				Add(value[i]);
			}
		}
		
		public void AddRange(CodeDocumentationNodeCollection value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}

			int count = value.Count;
			for (int i = 0; i < count; i++)
			{
				Add(value[i]);
			}
		}

		public bool Contains(CodeDocumentationNode value)
		{
			return List.Contains(value);
		}
		
		public void CopyTo(CodeDocumentationNode[] array, int index)
		{
			List.CopyTo(array, index);
		}

		public int IndexOf(CodeDocumentationNode value)
		{
			return List.IndexOf(value);
		}

		public void Insert(int index, CodeDocumentationNode value)
		{
			List.Insert(index, value);
		}

		public void Remove(CodeDocumentationNode value)
		{
			List.Remove(value);
		}
	}
}
