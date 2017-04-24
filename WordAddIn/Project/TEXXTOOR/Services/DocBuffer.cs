using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Word=Microsoft.Office.Interop.Word;

namespace TEXXTOOR.Services {
	public class DocBuffer {
		//Word API Objects
		private Word._Document HiddenDoc;
		private Word.Selection curSel;
		private Word.Template template;

		//ref parameters
		private object missing = System.Type.Missing;
		private object FalseObj = false; //flip this for docbuffer troubleshooting
		private object templateObj;

		//Is docbuffer running?
		public Boolean started { get; private set; }

		//Open document on new object
		public DocBuffer() {
			//Clear out unused buffer bookmarks
			Word.Bookmarks bookmarks = Globals.ThisAddIn.Application.ActiveDocument.Bookmarks;
			bookmarks.ShowHidden = true;

			foreach (Word.Bookmark mark in bookmarks) {
				if (mark.Name.Contains("_buf")) {
					mark.Delete();
				}
			}

			//Remove trail of undo's for clearing out the bookmarks
			Globals.ThisAddIn.Application.ActiveDocument.UndoClear();

			//Set up template
			template = Globals.ThisAddIn.Application.NormalTemplate;
			templateObj = template;

			//Open Blank document, then attach styles *and update
			HiddenDoc = Globals.ThisAddIn.Application.Documents.Add(ref missing, ref missing, ref missing, ref FalseObj);
			HiddenDoc.set_AttachedTemplate(ref templateObj);
			HiddenDoc.UpdateStyles();

			//Tell hidden document it has been saved to remove rare prompt to save document
			HiddenDoc.Saved = true;

			//Make primary document active
			Globals.ThisAddIn.Application.ActiveDocument.Activate();

		}

		~DocBuffer() {
			try {
				HiddenDoc.Close(ref FalseObj, ref missing, ref missing);
			}
			catch {
			}
		}

		public void Close() {
			try {
				HiddenDoc.Close(ref FalseObj, ref missing, ref missing);
			}
			catch {
			}
		}

		public void Start() {
			try {
				//Make hidden document active to receive selection
				HiddenDoc.Activate(); //results in a slight application focus loss
			}
			catch (System.Runtime.InteropServices.COMException ex) {
				if (ex.Message == "Object has been deleted.") {
					//Open Blank document, then attach styles *and update
					HiddenDoc = Globals.ThisAddIn.Application.Documents.Add(ref missing, ref missing, ref missing, ref FalseObj);
					HiddenDoc.set_AttachedTemplate(ref templateObj);
					HiddenDoc.UpdateStyles();
					HiddenDoc.Activate();
				}
				else
					throw;
			}

			//Remove Continue Bookmark, if exists
			Word.Bookmarks hiddenDocBookmarks = Globals.ThisAddIn.Application.ActiveDocument.Bookmarks;
			if (hiddenDocBookmarks.Exists("Continue")) {
				object deleteMarkObj = "Continue";
				Word.Bookmark deleteMark = hiddenDocBookmarks.get_Item(ref deleteMarkObj);
				deleteMark.Select();
				deleteMark.Delete();
			}

			//Tell hidden document it has been saved to remove rare prompt to save document
			HiddenDoc.Saved = true;

			//Keep track when started
			started = true;
		}

		//Used for non-modal dialogs to bring active document back up between text insertion
		public void Continue() {
			//Exit quietly if buffer hasn't started
			if (!started) return;

			//Verify hidden document is active
			if ((HiddenDoc as Word.Document) != Globals.ThisAddIn.Application.ActiveDocument) {
				HiddenDoc.Activate();
			}

			//Hidden doc selection
			curSel = Globals.ThisAddIn.Application.Selection;

			//Hidden doc range
			Word.Range bufDocRange;

			//Select entire doc, save range
			curSel.WholeStory();
			bufDocRange = curSel.Range;

			//Find end, put a bookmark there
			bufDocRange.SetRange(curSel.End, curSel.End);
			object bookmarkObj = bufDocRange;

			//Generate "Continue" hidden bookmark
			Word.Bookmark mark = Globals.ThisAddIn.Application.ActiveDocument.Bookmarks.Add("Continue", ref bookmarkObj);
			mark.Select();

			//Tell hidden document it has been saved to remove rare prompt to save document
			HiddenDoc.Saved = true;

			//Make primary document active
			Globals.ThisAddIn.Application.ActiveDocument.Activate();
		}

		public void End() {
			//Exit quietly if buffer hasn't started
			if (!started) return;

			//Turn off buffer started flag
			started = false;

			//Verify hidden document is active
			if ((HiddenDoc as Word.Document) != Globals.ThisAddIn.Application.ActiveDocument) {
				HiddenDoc.Activate();
			}

			//Remove Continue Bookmark, if exists
			Word.Bookmarks hiddenDocBookmarks = Globals.ThisAddIn.Application.ActiveDocument.Bookmarks;
			hiddenDocBookmarks.ShowHidden = true;
			if (hiddenDocBookmarks.Exists("Continue")) {
				object deleteMarkObj = "Continue";
				Word.Bookmark deleteMark = hiddenDocBookmarks.get_Item(ref deleteMarkObj);
				deleteMark.Delete();
			}

			//Hidden doc selection
			curSel = Globals.ThisAddIn.Application.Selection;

			//Hidden doc range
			Word.Range hiddenDocRange;
			Word.Range bufDocRange;

			//Select entire doc, save range
			curSel.WholeStory();
			bufDocRange = curSel.Range;

			//If cursor bookmark placed in, move there, else find end of text, put a bookmark there
			Boolean cursorFound = false;
			if (hiddenDocBookmarks.Exists("_cursor")) {
				object cursorBookmarkObj = "_cursor";
				Word.Bookmark cursorBookmark = hiddenDocBookmarks.get_Item(ref cursorBookmarkObj);
				bufDocRange.SetRange(cursorBookmark.Range.End, cursorBookmark.Range.End);
				cursorBookmark.Delete();
				cursorFound = true;
			}
			else {
				//The -2 is done because [range object].WordOpenXML likes to drop bookmarks at the end of the range
				bufDocRange.SetRange(curSel.End - 2, curSel.End - 2);
			}

			object bookmarkObj = bufDocRange;

			//Generate GUID for hidden bookmark
			System.Guid guid = System.Guid.NewGuid();
			String id = "_buf" + guid.ToString().Replace("-", string.Empty);
			Word.Bookmark mark = Globals.ThisAddIn.Application.ActiveDocument.Bookmarks.Add(id, ref bookmarkObj);

			//Get OpenXML Text (Text with formatting)
			curSel.WholeStory();
			hiddenDocRange = curSel.Range;
			string XMLText = hiddenDocRange.WordOpenXML;

			//Clear out contents of buffer
			hiddenDocRange.Delete(ref missing, ref missing); //comment this for docbuffer troubleshooting

			//Tell hidden document it has been saved to remove rare prompt to save document
			HiddenDoc.Saved = true;

			//Make primary document active
			Globals.ThisAddIn.Application.ActiveDocument.Activate();

			//Get selection from new active document
			curSel = Globals.ThisAddIn.Application.Selection;

			//insert buffered formatted text into main document
			curSel.InsertXML(XMLText, ref missing);

			//Place cursor at bookmark+1 (this is done due to WordOpenXML ignoring bookmarks at the end of the selection)
			Word.Bookmarks bookmarks = Globals.ThisAddIn.Application.ActiveDocument.Bookmarks;
			bookmarks.ShowHidden = true;

			object stringObj = id;
			Word.Bookmark get_mark = bookmarks.get_Item(ref stringObj);
			bufDocRange = get_mark.Range;

			if (cursorFound) //Canned language actively placed cursor
				bufDocRange.SetRange(get_mark.Range.End, get_mark.Range.End);
			else //default cursor at the end of text
				bufDocRange.SetRange(get_mark.Range.End + 1, get_mark.Range.End + 1);
			bufDocRange.Select();
		}
	}
}
