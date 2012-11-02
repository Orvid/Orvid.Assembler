//
// System.CodeDom CodeParameterDeclarationExpression Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
using System;
using System.Runtime.InteropServices;

namespace Orvid.CodeDom
{
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeParameterDeclarationExpression
		: CodeExpression
	{
		private CodeAttributeDeclarationCollection customAttributes;
		private FieldDirection direction;
		private string name;
		private CodeTypeReference type;

		//
		// Constructors
		//
		public CodeParameterDeclarationExpression()
		{
		}

		public CodeParameterDeclarationExpression(CodeTypeReference type, string name)
		{
			this.type = type;
			this.name = name;
		}

		public CodeParameterDeclarationExpression(string type, string name)
		{
			this.type = new CodeTypeReference(type);
			this.name = name;
		}

		public CodeParameterDeclarationExpression(Type type, string name)
		{
			this.type = new CodeTypeReference(type);
			this.name = name;
		}

		//
		// Properties
		//
		public CodeAttributeDeclarationCollection CustomAttributes
		{
			get
			{
				if (customAttributes == null)
					customAttributes = new CodeAttributeDeclarationCollection();
				return customAttributes;
			}
			set
			{
				customAttributes = value;
			}
		}

		public FieldDirection Direction
		{
			get
			{
				return direction;
			}
			set
			{
				direction = value;
			}
		}

		public string Name
		{
			get
			{
				if (name == null)
				{
					return string.Empty;
				}
				return name;
			}
			set
			{
				name = value;
			}
		}

		public CodeTypeReference Type
		{
			get
			{
				if (type == null)
				{
					type = new CodeTypeReference(string.Empty);
				}
				return type;
			}
			set
			{
				type = value;
			}
		}

		public CodeExpression DefaultValueExpression { get; set; }

		//
		// ICodeDomVisitor method
		//
		internal override void Accept(ICodeDomVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
