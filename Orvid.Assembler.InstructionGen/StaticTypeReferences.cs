using System;
using System.Collections.Generic;
using Orvid.CodeDom;

namespace Orvid.Assembler.InstructionGen
{
	// Always use the typeof(global::) 
	// constructor for references wherever
	// possible.
	public static class StaticTypeReferences
	{

		public static readonly CodeTypeReference String = new CodeTypeReference(typeof(global::System.String));
		public static readonly CodeTypeReference SByte = new CodeTypeReference(typeof(global::System.SByte));
		public static readonly CodeTypeReference Byte = new CodeTypeReference(typeof(global::System.Byte));
		public static readonly CodeTypeReference Short = new CodeTypeReference(typeof(global::System.Int16));
		public static readonly CodeTypeReference UShort = new CodeTypeReference(typeof(global::System.UInt16));
		public static readonly CodeTypeReference Int = new CodeTypeReference(typeof(global::System.Int32));
		public static readonly CodeTypeReference UInt = new CodeTypeReference(typeof(global::System.UInt32));
		public static readonly CodeTypeReference Long = new CodeTypeReference(typeof(global::System.Int64));
		public static readonly CodeTypeReference ULong = new CodeTypeReference(typeof(global::System.UInt64));
		public static readonly CodeTypeReference Void = new CodeTypeReference(typeof(void));

		#region Exceptions
		public static readonly CodeTypeReference ArgumentOutOfRangeException = new CodeTypeReference(typeof(global::System.ArgumentOutOfRangeException));
		public static readonly CodeTypeReference Exception = new CodeTypeReference(typeof(global::System.Exception));
		#endregion

		#region Types
		public static readonly CodeTypeReference NamingHelper = new CodeTypeReference("NamingHelper");
		public static readonly CodeTypeReferenceExpression NamingHelperExpression = new CodeTypeReferenceExpression(NamingHelper);

		// Initialized Types
		public static string StreamClassName;
		public static CodeTypeReference Stream                              { get; private set; }
		public static CodeTypeReferenceExpression StreamExpression          { get; private set; }
		public static string AssemblerClassName;
		public static CodeTypeReference Assembler                           { get; private set; }
		public static CodeTypeReferenceExpression AssemblerExpression       { get; private set; }
		public static string InstructionClassName;
		public static CodeTypeReference Instruction                         { get; private set; }
		public static CodeTypeReferenceExpression InstructionExpression     { get; private set; }
		public static string PrefixClassName;
		public static CodeTypeReference Prefix                              { get; private set; }
		public static CodeTypeReferenceExpression PrefixExpression          { get; private set; }
		public static string AssemblySyntaxClassName;
		public static CodeTypeReference AssemblySyntax                      { get; private set; }
		public static CodeTypeReferenceExpression AssemblySyntaxExpression  { get; private set; }
		public static string InstructionFormClassName;
		public static CodeTypeReference InstructionForm                     { get; private set; }
		public static CodeTypeReferenceExpression InstructionFormExpression { get; private set; }
		public static Dictionary<string, bool> SegmentLookup = new Dictionary<string, bool>(16) 
		{
			{ "NONE", true }
		};
		public static string SegmentClassName;
		public static CodeTypeReference Segment                             { get; private set; }
		public static CodeTypeReferenceExpression SegmentExpression         { get; private set; }

		public static void InitializeTypes()
		{
			StaticTypeReferences.Stream = new CodeTypeReference(StreamClassName);
			StaticTypeReferences.StreamExpression = new CodeTypeReferenceExpression(Stream);
			StaticTypeReferences.Stream_InSByteRange = new CodeMethodReferenceExpression(StreamExpression, "InSByteRange");
			StaticTypeReferences.Stream_InShortRange = new CodeMethodReferenceExpression(StreamExpression, "InShortRange");

			StaticTypeReferences.Assembler = new CodeTypeReference(AssemblerClassName);
			StaticTypeReferences.AssemblerExpression = new CodeTypeReferenceExpression(Assembler);

			StaticTypeReferences.Instruction = new CodeTypeReference(InstructionClassName);
			StaticTypeReferences.InstructionExpression = new CodeTypeReferenceExpression(Instruction);

			StaticTypeReferences.Prefix = new CodeTypeReference(PrefixClassName);
			StaticTypeReferences.PrefixExpression = new CodeTypeReferenceExpression(Prefix);

			StaticTypeReferences.InstructionForm = new CodeTypeReference(InstructionFormClassName);
			StaticTypeReferences.InstructionFormExpression = new CodeTypeReferenceExpression(InstructionForm);

			if (AssemblySyntaxClassName != null)
			{
				StaticTypeReferences.AssemblySyntax = new CodeTypeReference(AssemblySyntaxClassName);
				StaticTypeReferences.AssemblySyntaxExpression = new CodeTypeReferenceExpression(AssemblySyntax);
			}


			StaticTypeReferences.Segment = new CodeTypeReference(SegmentClassName);
			StaticTypeReferences.SegmentExpression = new CodeTypeReferenceExpression(Segment);
			FieldTypeRegistry.Segment = FieldTypeRegistry.Fields[FieldTypeRegistry.RegisterType(SegmentClassName, "seg_")];
		}

		public static void Reset()
		{
			StaticTypeReferences.SegmentLookup = new Dictionary<string, bool>(16) 
			{
				{ "NONE", true }
			};
			StaticTypeReferences.StreamClassName = null;
			StaticTypeReferences.Stream = null;
			StaticTypeReferences.StreamExpression = null;
			StaticTypeReferences.Stream_InSByteRange = null;
			StaticTypeReferences.Stream_InShortRange = null;
			
			StaticTypeReferences.AssemblerClassName = null;
			StaticTypeReferences.Assembler = null;
			StaticTypeReferences.AssemblerExpression = null;

			StaticTypeReferences.InstructionClassName = null;
			StaticTypeReferences.Instruction = null;
			StaticTypeReferences.InstructionExpression = null;

			StaticTypeReferences.PrefixClassName = null;
			StaticTypeReferences.Prefix = null;
			StaticTypeReferences.PrefixExpression = null;

			StaticTypeReferences.InstructionFormClassName = null;
			StaticTypeReferences.InstructionForm = null;
			StaticTypeReferences.InstructionFormExpression = null;

			StaticTypeReferences.AssemblySyntaxClassName = null;
			StaticTypeReferences.AssemblySyntax = null;
			StaticTypeReferences.AssemblySyntaxExpression = null;

			StaticTypeReferences.SegmentClassName = null;
			StaticTypeReferences.Segment = null;
			StaticTypeReferences.SegmentExpression = null;
		}

		public static bool IsValidSegment(string seg)
		{
			return SegmentLookup.ContainsKey(seg.ToUpper());
		}
		#endregion

		#region Fields
		public static readonly CodeFieldReferenceExpression ParentAssemblerExpression = new CodeFieldReferenceExpression(
			new CodeThisReferenceExpression(),
			"ParentAssembler"
		);
		#endregion
		
		#region Arguments
		public const string Emit_StreamArgName = "strm";
		public static readonly CodeArgumentReferenceExpression Emit_StreamArg = new CodeArgumentReferenceExpression(Emit_StreamArgName);

		public const string ToString_SyntaxArgName = "syntax";
		public static readonly CodeArgumentReferenceExpression ToString_SyntaxArg = new CodeArgumentReferenceExpression(ToString_SyntaxArgName);
		#endregion

		#region Methods
		public static readonly CodeMethodReferenceExpression Emit_Stream_WritePrefix =
			new CodeMethodReferenceExpression(Emit_StreamArg, "WritePrefix");
		public static readonly CodeMethodReferenceExpression Emit_Stream_WriteByte = 
			new CodeMethodReferenceExpression(Emit_StreamArg, "WriteByte");
		public static readonly CodeMethodReferenceExpression Emit_Stream_WriteImm8 =
			new CodeMethodReferenceExpression(Emit_StreamArg, "WriteImm8");
		public static readonly CodeMethodReferenceExpression Emit_Stream_WriteImm16 =
			new CodeMethodReferenceExpression(Emit_StreamArg, "WriteImm16");
		public static readonly CodeMethodReferenceExpression Emit_Stream_WriteImm32 = 
			new CodeMethodReferenceExpression(Emit_StreamArg, "WriteImm32");
		public static readonly CodeMethodReferenceExpression Emit_Stream_WriteSegmentOverride =
			new CodeMethodReferenceExpression(Emit_StreamArg, "WriteSegmentOverride");

		// Static methods
		public static CodeMethodReferenceExpression Stream_InSByteRange  { get; private set; }
		public static CodeMethodReferenceExpression Stream_InShortRange  { get; private set; }
		
		public static readonly CodeMethodReferenceExpression NamingHelper_NameDisplacement =
			new CodeMethodReferenceExpression(NamingHelperExpression, "NameDisplacement");
		
		#endregion

	}
}

