using System;
using Mono.TextEditor;
using MonoDevelop.Core;
using MonoDevelop.Projects.Text;
using MonoDevelop.Ide.Gui.Content;

namespace Orvid.Assembler.Cpud.MonoDevelop
{
	public class CpudTextEditorExtension : TextEditorExtension
	{
		private TextEditorData tDat;

		public override void Initialize()
		{
			base.Initialize();
			tDat = Document.Editor;
		}

		private const string EmptyDocumentation = 
			"/ <summary>" + ExtraLineDoc +
			ExtraLineDoc +
			EmptyDocEnd;
		private const string EmptyDocEnd = "</summary>";
		private const string ExtraLineDoc = "\r\n/// ";

		public override bool KeyPress(Gdk.Key key, char keyChar, Gdk.ModifierType modifier)
		{
			if (tDat.Document.MimeType != "text/orvid-cpud")
				return base.KeyPress(key, keyChar, modifier);
			if (keyChar != '/' && key != Gdk.Key.Return)
				return base.KeyPress(key, keyChar, modifier);
			
			var line = tDat.Document.GetLine(tDat.Caret.Line);
			if (key == Gdk.Key.Return && keyChar != '\n')
			{
				if (!tDat.GetTextAt(line).TrimStart().StartsWith("///"))
					return base.KeyPress(key, keyChar, modifier);
				int offset = tDat.Caret.Offset;
				using (var undo = tDat.OpenUndoGroup ())
				{
					tDat.Insert(offset, ExtraLineDoc);
				}
				return false;
			}
			else
			{
				string text = tDat.Document.GetTextAt(line.Offset, line.Length);
			
				if (!text.EndsWith("//"))
					return base.KeyPress(key, keyChar, modifier);
			
				// check if there is doc comment above or below.
				var l = line.PreviousLine;
				while (l != null && l.Length == 0)
					l = l.PreviousLine;
				if (l != null && tDat.GetTextAt(l).TrimStart().StartsWith("///"))
					return base.KeyPress(key, keyChar, modifier);
			
				l = line.NextLine;
				while (l != null && l.Length == 0)
					l = l.NextLine;
				if (l != null && tDat.GetTextAt(l).TrimStart().StartsWith("///"))
					return base.KeyPress(key, keyChar, modifier);

			
				int offset = tDat.Caret.Offset;
			
				int insertedLength;
			
				// Insert key (3rd undo step)
				tDat.Insert(offset, "/");
			
				using (var undo = tDat.OpenUndoGroup ())
				{
					insertedLength = tDat.Replace(offset, 1, EmptyDocumentation);
					// important to set the caret position here for the undo step
					tDat.Caret.Offset = offset + insertedLength - EmptyDocEnd.Length - ExtraLineDoc.Length + 1;
				}

				return false;
			}
		}
	}
}

