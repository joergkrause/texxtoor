using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Tools.Ribbon;
using System.Windows.Forms;
using Microsoft.Office.Interop.Word;
using System.Runtime.InteropServices;
using Microsoft.Office.Tools;
using Microsoft.Office.Core;
using System.IO;
using System.Reflection;
using System.Data;
//using Newtonsoft.Json;
using System.Xml;
using System.Web.Script.Serialization;
//using Newtonsoft.Json;
using System.Collections;

//using System.Reflection;

namespace TEXXTOOR
{
    // Summary:
    //     Represents the selected item in an instance of the System.Web.Mvc.SelectList
    //     class.
 public    enum Semantics
    {
        abbreviation,
        cite, idiom, variable, definition, links , index
    }
    public partial class TexxtoorRibbon
    {
        //Current Document
        public Document _CurrentDocument
        {
            get
            {
                return Globals.ThisAddIn.Application.ActiveDocument;
            }
        }

        //Current Selection Cursor
        public Selection _CurrentSelection
        {
            get
            {
                return Globals.ThisAddIn.Application.Selection;
            }
        }

        //Current Selected Text
        public string _CurrentText
        {
            get
            {
                return Globals.ThisAddIn.Application.Selection.Text;
            }
        }

        //Current Range Of Cursor
        public Range _CurrentRange
        {
            get
            {
                return Globals.ThisAddIn.Application.Selection.Range;
            }
        }

        //Current Horizontal Position of cursor
        public float _Left
        {
            get
            {
                return Globals.ThisAddIn.Application.Selection.get_Information(Microsoft.Office.Interop.Word.WdInformation.wdHorizontalPositionRelativeToPage);
            }
        }

        //Current Vertical Position of Cursor
        public float _Top
        {
            get
            {
                return Globals.ThisAddIn.Application.Selection.get_Information(Microsoft.Office.Interop.Word.WdInformation.wdVerticalPositionRelativeToPage);
            }
        }

        // Ribbon Loading event
        private void TexxtoorRibbon_Load(object sender, RibbonUIEventArgs e)
        {
            //Loading Items To Menus
            
            DDLBinding(1, Semantics.abbreviation);
            DDLBinding(1, Semantics.cite);
            DDLBinding(1, Semantics.definition);
            DDLBinding(1, Semantics.idiom);
            DDLBinding(1, Semantics.links);
            DDLBinding(1, Semantics.variable);
            String strResult= ProxyService.GetAllChapterIds();
            Globals.ThisAddIn.Application.WindowSelectionChange += new ApplicationEvents4_WindowSelectionChangeEventHandler(Application_WindowSelectionChange);
            if (_CurrentSelection.Font.Bold == 0)
            {
                toggleBtnBold.Checked = false;
            }
            else
            {
                toggleBtnBold.Checked = true;
            }
            if (_CurrentSelection.Font.Italic == 0)
            {
                toggleBtnItalic.Checked = false;
            }
            else
            {
                toggleBtnItalic.Checked = true;
            }
            if (_CurrentSelection.Font.Underline == WdUnderline.wdUnderlineNone)
            {
                toggleBtnUnderline.Checked = false;
            }
            else
            {
                toggleBtnUnderline.Checked = true;
            }
            if (_CurrentSelection.Font.StrikeThrough == 0)
            {
                toggleBtnStrikeThrough.Checked = false;
            }
            else
            {
                toggleBtnStrikeThrough.Checked = true;
            }
            if (_CurrentSelection.Paragraphs.Alignment != WdParagraphAlignment.wdAlignParagraphLeft)
            {
                toggleBtnAlignLeft.Checked = false;
            }
            else
            {
                toggleBtnAlignLeft.Checked = true;
            }
            if (_CurrentSelection.Paragraphs.Alignment != WdParagraphAlignment.wdAlignParagraphCenter)
            {
                toggleBtnAlignCenter.Checked = false;
            }
            else
            {
                toggleBtnAlignCenter.Checked = true;
            }
            if (_CurrentSelection.Paragraphs.Alignment != WdParagraphAlignment.wdAlignParagraphRight)
            {
                toggleBtnAlignRight.Checked = false;
            }
            else
            {
                toggleBtnAlignRight.Checked = true;
            }
            //15Oct Added
            if (_CurrentSelection.Range.ListFormat.ListType == WdListType.wdListBullet)
            {
                toggleButtonBullets.Checked = true;
            }
            else
            {
                toggleButtonBullets.Checked = false;
            }
            if (_CurrentSelection.Range.ListFormat.ListType == WdListType.wdListOutlineNumbering)
            {
                toggleButtonNumbering.Checked = true;
            }
            else
            {
                
                toggleButtonNumbering.Checked = false;
            }
        }
        //public class SelectListItem
        //{
        //    // Summary:
        //    //     Initializes a new instance of the System.Web.Mvc.SelectListItem class.
        //    public SelectListItem()
        //    { }

        //    // Summary:
        //    //     Gets or sets a value that indicates whether this System.Web.Mvc.SelectListItem
        //    //     is selected.
        //    //
        //    // Returns:
        //    //     true if the item is selected; otherwise, false.
        //    public bool Selected { get; set; }
        //    //
        //    // Summary:
        //    //     Gets or sets the text of the selected item.
        //    //
        //    // Returns:
        //    //     The text.
        //    public string Text { get; set; }
        //    //
        //    // Summary:
        //    //     Gets or sets the value of the selected item.
        //    //
        //    // Returns:
        //    //     The value.
        //    public string Value { get; set; }
        //}
        void Application_WindowSelectionChange(Selection Sel)
        {
            try
            {
                //if (GetUndoItems() > 0)
                //{
                //    btnUndo.Enabled = true;
                //}
                //else
                //{
                //    btnUndo.Enabled = false;
                //}
                if (Sel.Font.Bold == 0)
                {
                    toggleBtnBold.Checked = false;
                }
                else
                {
                    toggleBtnBold.Checked = true;
                }
                if (Sel.Font.Italic == 0)
                {
                    toggleBtnItalic.Checked = false;
                }
                else
                {
                    toggleBtnItalic.Checked = true;
                }
                if (Sel.Font.Underline == WdUnderline.wdUnderlineNone)
                {
                    toggleBtnUnderline.Checked = false;
                }
                else
                {
                    toggleBtnUnderline.Checked = true;
                }
                if (Sel.Font.StrikeThrough == 0)
                {
                    toggleBtnStrikeThrough.Checked = false;
                }
                else
                {
                    toggleBtnStrikeThrough.Checked = true;
                }
                if (Sel.Paragraphs.Alignment != WdParagraphAlignment.wdAlignParagraphLeft)
                {
                    toggleBtnAlignLeft.Checked = false;
                }
                else
                {
                    toggleBtnAlignLeft.Checked = true;
                }
                if (Sel.Paragraphs.Alignment != WdParagraphAlignment.wdAlignParagraphCenter)
                {
                    toggleBtnAlignCenter.Checked = false;
                }
                else
                {
                    toggleBtnAlignCenter.Checked = true;
                }
                if (Sel.Paragraphs.Alignment != WdParagraphAlignment.wdAlignParagraphRight)
                {
                    toggleBtnAlignRight.Checked = false;
                }
                else
                {
                    toggleBtnAlignRight.Checked = true;
                }
                if (Sel.Range.Text != null)
                {
                    btnConvertTextToTable.Enabled = true;
                }
                else
                {
                    btnConvertTextToTable.Enabled = false;
                }
                //15Oct2013 Added
                if (_CurrentSelection.Range.ListFormat.ListType == WdListType.wdListBullet)
                {
                    toggleButtonBullets.Checked = true;
                }
                else
                {
                    toggleButtonBullets.Checked = false;
                }
                if (_CurrentSelection.Range.ListFormat.ListType == WdListType.wdListOutlineNumbering)
                {
                    toggleButtonNumbering.Checked = true;
                }
                else
                {
                    toggleButtonNumbering.Checked = false;
                }
            }
            catch
            {
                return;
            }

        }
        private const int UndoControlId = 128;
        private int GetUndoItems()
        {


            try
            {
                var commandBars = _CurrentDocument.CommandBars.Cast<CommandBar>();
                var standardCommandBar = commandBars.First(bar => bar.Name.Equals("Standard"));
                var undoControl = standardCommandBar.Controls.Cast<CommandBarControl>().First(control => control.Id == UndoControlId) as CommandBarComboBox;
                //CommandBarComboBox obj = (standardCommandBar.Controls.Cast<CommandBarControl>().First(control => control.Id == UndoControlId) as CommandBarComboBox);

                //var objList = obj.get_List(0);

                return undoControl != null ? undoControl.ListCount : -1;
            }
            catch
            {
                return 0;
            }
        }
        private void OpenExisting_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                _CurrentDocument.CommandBars.ExecuteMso("BlogOpenExisting");
            }
            catch
            {
                return;
            }
        }

        private void Publish_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                _CurrentDocument.CommandBars.ExecuteMso("BlogPublish");
            }
            catch
            {
                return;
            }

        }

        //Basic Formating Event of Text
        private void BasicText_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                Document doc = _CurrentDocument;
                String strContent = _CurrentText;
                Selection selCurrent = _CurrentSelection;
                string strOfficeID = ((RibbonToggleButton)sender).OfficeImageId;
                if (strOfficeID == "Bold")
                {
                    if (selCurrent.Font.Bold == 0)
                    {
                        _CurrentDocument.CommandBars.ExecuteMso("Bold");
                        ((RibbonToggleButton)sender).Checked = true;
                        btnUndo.Enabled = true;
                    }
                    else
                    {
                        _CurrentDocument.CommandBars.ExecuteMso("Bold");
                        ((RibbonToggleButton)sender).Checked = false;
                    }
                    //doc.Content.Sections.Count
                }
                if (strOfficeID == "Italic")
                {
                    if (selCurrent.Font.Italic == 0)
                    {
                        _CurrentDocument.CommandBars.ExecuteMso("Italic");
                        ((RibbonToggleButton)sender).Checked = true;
                        btnUndo.Enabled = true;
                    }
                    else
                    {
                        _CurrentDocument.CommandBars.ExecuteMso("Italic");
                        ((RibbonToggleButton)sender).Checked = false;
                    }
                    //doc.Content.Sections.Count
                }
                if (strOfficeID == "Underline")
                {
                    if (selCurrent.Font.Underline == WdUnderline.wdUnderlineNone)
                    {
                        _CurrentDocument.CommandBars.ExecuteMso("Underline");
                        ((RibbonToggleButton)sender).Checked = true;
                        btnUndo.Enabled = true;
                    }
                    else
                    {
                        _CurrentDocument.CommandBars.ExecuteMso("Underline");
                        //selCurrent.SelectCurrentFont();
                        ((RibbonToggleButton)sender).Checked = false;
                    }

                }
                if (strOfficeID == "Strikethrough")
                {
                    if (selCurrent.Font.StrikeThrough == 0)
                    {
                        _CurrentDocument.CommandBars.ExecuteMso("Strikethrough");
                        ((RibbonToggleButton)sender).Checked = true;
                        btnUndo.Enabled = true;
                    }
                    else
                    {
                        _CurrentDocument.CommandBars.ExecuteMso("Strikethrough");
                        ((RibbonToggleButton)sender).Checked = false;
                    }
                }
                if (strOfficeID == "AlignLeft")
                {
                    if (selCurrent.Tables.Count != 0)
                    {
                        selCurrent.Paragraphs.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
                        //_CurrentDocument.CommandBars.ExecuteMso("AlignLeft");
                        btnUndo.Enabled = true;
                        toggleBtnAlignLeft.Checked = true;
                        toggleBtnAlignRight.Checked = false;
                        toggleBtnAlignCenter.Checked = false;
                    }
                }
                if (strOfficeID == "AlignCenter")
                {
                    if (selCurrent.Tables.Count != 0)
                    {
                        selCurrent.Paragraphs.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                        //_CurrentDocument.CommandBars.ExecuteMso("AlignCenter");
                        btnUndo.Enabled = true;
                        toggleBtnAlignLeft.Checked = false;
                        toggleBtnAlignRight.Checked = false;
                        toggleBtnAlignCenter.Checked = true;
                    }
                }
                if (strOfficeID == "AlignRight")
                {
                    if (selCurrent.Tables.Count != 0)
                    {
                        selCurrent.Paragraphs.Alignment = WdParagraphAlignment.wdAlignParagraphRight;
                        //_CurrentDocument.CommandBars.ExecuteMso("AlignRight");
                        btnUndo.Enabled = true;
                        toggleBtnAlignLeft.Checked = false;
                        toggleBtnAlignRight.Checked = true;
                        toggleBtnAlignCenter.Checked = false;
                    }
                }
                //15Oct2013 Added
                if (strOfficeID == "Bullets")
                {
                    _CurrentDocument.CommandBars.ExecuteMso("Bullets");
                    _CurrentDocument.CommandBars.ExecuteMso("IndentDecreaseWord");
                    btnIncreaseIndent.Enabled = true;
                    btnUndo.Enabled = true;
                    toggleButtonNumbering.Checked = false;
                }
                if (strOfficeID == "Numbering")
                {
                    _CurrentDocument.CommandBars.ExecuteMso("Numbering");
                    _CurrentDocument.CommandBars.ExecuteMso("IndentDecreaseWord");
                    #region old code
                    /*Microsoft.Office.Interop.Word.Application app = Globals.ThisAddIn.Application;
                    Document Currentdoc = _CurrentDocument;
                    Range rng = _CurrentRange;
                    object oTrue = true;
                    object oFalse = false;
                    object oListName = "TreeList";
                    ListTemplate lstTemp = Currentdoc.ListTemplates.Add(ref oTrue, ref oListName);
                    ListGallery lstgly = Globals.ThisAddIn.Application.ListGalleries[WdListGalleryType.wdOutlineNumberGallery];

                    #region
                    //for (int level = 1; level <= 5; level++)
                    //{
                    //    switch (level)
                    //    {
                    //        case 1: lstTemp.ListLevels[level].NumberFormat = "%" + level.ToString() + ".";
                    //            //lstTemp.ListLevels[level].Application.Selection.ParagraphFormat.set_Style(WdBuiltinStyle.wdStyleHeading1);
                    //            break;
                    //        case 2: lstTemp.ListLevels[level].NumberFormat = "%" + (level - 1).ToString() + ".%" + level.ToString() + ".";
                    //            //lstTemp.ListLevels[level].Application.Selection.ParagraphFormat.set_Style(WdBuiltinStyle.wdStyleHeading2);
                    //            break;
                    //        case 3: lstTemp.ListLevels[level].NumberFormat = "%" + (level - 2).ToString() + ".%" + (level - 1).ToString() + ".%" + level.ToString() + ".";
                    //            //lstTemp.ListLevels[level].Application.Selection.ParagraphFormat.set_Style(WdBuiltinStyle.wdStyleHeading3);
                    //            break;
                    //        case 4: lstTemp.ListLevels[level].NumberFormat = "%" + (level - 3).ToString() + ".%" + (level - 2).ToString() + ".%" + (level - 1).ToString() + ".%" + level.ToString() + ".";
                    //            //lstTemp.ListLevels[level].Application.Selection.ParagraphFormat.set_Style(WdBuiltinStyle.wdStyleHeading4);
                    //            break;
                    //        case 5: lstTemp.ListLevels[level].NumberFormat = "%" + (level - 4).ToString() + ".%" + (level - 3).ToString() + ".%" + (level - 2).ToString() + ".%" + (level - 1).ToString() + ".%" + level.ToString() + ".";
                    //            //lstTemp.ListLevels[level].Application.Selection.ParagraphFormat.set_Style(WdBuiltinStyle.wdStyleHeading5);
                    //            break;
                    //    }
                    //    lstTemp.ListLevels[level].NumberStyle = WdListNumberStyle.wdListNumberStyleArabic;
                    //    lstTemp.ListLevels[level].NumberPosition = app.CentimetersToPoints(0.5f * (level - 1));
                    //    lstTemp.ListLevels[level].TextPosition = app.CentimetersToPoints(0.5f * level);
                    //    //lstTemp.ListLevels[level].Application.Selection.ParagraphFormat.set_Style(WdBuiltinStyle.wdStyleHeading1);
                    //}
                    
                    //int level;

                    //level = 1;
                    //lstTemp.ListLevels[level].NumberFormat = "%" + level.ToString() + ".";
                    //lstTemp.ListLevels[level].NumberStyle = WdListNumberStyle.wdListNumberStyleArabic;
                    //lstTemp.ListLevels[level].NumberPosition = app.CentimetersToPoints(0.5f * (level - 1));
                    //lstTemp.ListLevels[level].TextPosition = app.CentimetersToPoints(0.5f * level);
                    //lstTemp.ListLevels[level].Application.Selection.ParagraphFormat.set_Style(WdBuiltinStyle.wdStyleHeading1);
                    //level = 2;
                    //lstTemp.ListLevels[level].NumberFormat = "%" + (level - 1).ToString() + ".%" + level.ToString() + ".";
                    //lstTemp.ListLevels[level].NumberStyle = WdListNumberStyle.wdListNumberStyleArabic;
                    //lstTemp.ListLevels[level].NumberPosition = app.CentimetersToPoints(0.5f * (level - 1));
                    //lstTemp.ListLevels[level].TextPosition = app.CentimetersToPoints(0.5f * level);
                    //lstTemp.ListLevels[level].Application.Selection.ParagraphFormat.set_Style(WdBuiltinStyle.wdStyleHeading2);
                    //level = 3;
                    //lstTemp.ListLevels[level].NumberFormat = "%" + (level - 2).ToString() + ".%" + (level - 1).ToString() + ".%" + level.ToString() + ".";
                    //lstTemp.ListLevels[level].NumberStyle = WdListNumberStyle.wdListNumberStyleArabic;
                    //lstTemp.ListLevels[level].NumberPosition = app.CentimetersToPoints(0.5f * (level - 1));
                    //lstTemp.ListLevels[level].TextPosition = app.CentimetersToPoints(0.5f * level);
                    //lstTemp.ListLevels[level].Application.Selection.ParagraphFormat.set_Style(WdBuiltinStyle.wdStyleHeading3);
                    #endregion
                    object oListApplyTo = WdListApplyTo.wdListApplyToWholeList;
                    object oListBehavior = WdDefaultListBehavior.wdWord10ListBehavior;

                    //rng.ListFormat.ApplyNumberDefault(oListBehavior);
                    lstTemp = lstgly.ListTemplates[5];
                    rng.ListFormat.ApplyListTemplateWithLevel(lstTemp, ref oFalse, ref oListApplyTo, ref oListBehavior);
                    //_CurrentDocument.CommandBars.ExecuteMso("IndentDecreaseWord");*/
                    #endregion
                    btnIncreaseIndent.Enabled = true;
                    btnUndo.Enabled = true;
                    toggleButtonBullets.Checked = false;
                }
            }
            catch
            {
                
            }
        }

        //Paragraph Indent Event of Text
        private void ParagraphIndent_Click(object sender, RibbonControlEventArgs e)
        {
            Selection selCurrent = _CurrentSelection;
            string strOfficeID = ((RibbonButton)sender).OfficeImageId;
            List lst = selCurrent.Range.ListFormat.List;
            if (strOfficeID == "IndentIncreaseWord")
            {
                if (lst != null)
                {
                    _CurrentDocument.CommandBars.ExecuteMso(strOfficeID);
                    btnDecreaseIndent.Enabled = true;
                    btnUndo.Enabled = true;
                }
            }
            if (strOfficeID == "IndentDecreaseWord")
            {
                _CurrentDocument.CommandBars.ExecuteMso(strOfficeID);
                btnUndo.Enabled = true;
            }
        }
        //Cut, copy, paste event
        private void PasteOption_Click(object sender, RibbonControlEventArgs e)
        {
            Document doc = _CurrentDocument;
            Selection selCurrent = _CurrentSelection;
            string strOfficeID = ((RibbonButton)sender).OfficeImageId;
            if (strOfficeID == "Cut")
            {
                _CurrentDocument.CommandBars.ExecuteMso("Cut");
                btnPaste.Enabled = true;
                btnUndo.Enabled = true;
            }
            if (strOfficeID == "Copy")
            {
                _CurrentDocument.CommandBars.ExecuteMso("Copy");
                btnPaste.Enabled = true;
                btnUndo.Enabled = true;
            }
            if (strOfficeID == "Paste")
            {
                _CurrentDocument.CommandBars.ExecuteMso("Paste");
                btnUndo.Enabled = true;
                //btnPaste.Enabled = false;
            }
        }

        private void ManageProject_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                _CurrentDocument.CommandBars.ExecuteMso("BlogManageAccounts");
            }
            catch
            {
                return;
            }

        }

        //Inserting QuickTables
        //15Oct2013 Changed
        private void TableWithGrid_Click(object sender, RibbonControlEventArgs e)
        {
            Document doc = _CurrentDocument;
            Range tableLocation;
            int index = galleryQuickTables.SelectedItemIndex;
            tableLocation = _CurrentRange;
            Table tblCur = tableLocation.Tables.Add(tableLocation, 3, 4);
            switch (index)
            {
                case 0: tblCur.set_Style("Table Grid 1");
                    btnUndo.Enabled = true;
                    break;
                case 1: tblCur.set_Style("Table Grid 2");
                    btnUndo.Enabled = true;
                    break;
                case 2: tblCur.set_Style("Table Grid 3");
                    btnUndo.Enabled = true;
                    break;
                case 3: tblCur.set_Style("Table Grid 8");
                    btnUndo.Enabled = true;
                    break;
            }
        }

        /*InsertTable Event, Which give an option for 
        selecting no.of Rows and Columns*/
        private void InsertTable_Click(object sender, RibbonControlEventArgs e)
        {
            Document doc = _CurrentDocument;
            Dialog dialogBox = Globals.ThisAddIn.Application.Dialogs[WdWordDialog.wdDialogTableInsertTable];
            object missing = 0;
            dialogBox.Show(ref missing);
        }


        /*Convert Text Into Table Event
        */
        private void ConvertTextToTable_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                _CurrentDocument.CommandBars.ExecuteMso("ConvertTextToTable");
            }
            catch
            {
                return;
            }

        }

        private void Undo_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                _CurrentDocument.CommandBars.ExecuteMso("Undo");
            }
            catch
            {
                return;
            }
            btnRedo.Enabled = true;
        }

        private void Redo_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                _CurrentDocument.CommandBars.ExecuteMso("Redo");
            }
            catch
            {
                return;
            }
        }


        private void SpellingGrammar_Check(object sender, RibbonControlEventArgs e)
        {
            try
            {
                _CurrentDocument.CommandBars.ExecuteMso("SpellingAndGrammar");
            }
            catch
            {
                return;
            }
        }

        /*Word Count Event,
         On click You will see
         a Dialog box with all properties 
         of word count*/
        private void Word_Count(object sender, RibbonControlEventArgs e)
        {
            try
            {
                _CurrentDocument.CommandBars.ExecuteMso("WordCount");
            }
            catch
            {
                return;
            }

        }

        /*Insert Picture option at present cursor position
        */
        private void Picture_Insert(object sender, RibbonControlEventArgs e)
        {
            try
            {
                _CurrentDocument.CommandBars.ExecuteMso("PictureInsertFromFile");
            }
            catch
            {
                return;
            }

            btnUndo.Enabled = true;
        }

        /*Insert Normal TextBox at present cursor 
        location with a dimensions of 200X50(WidthXHeight)*/
        private void Insert_Text(object sender, RibbonControlEventArgs e)
        {
            try
            {
                _CurrentDocument.CommandBars.ExecuteMso("TextBoxInsert");
            }
            catch
            {
                return;
            }
            btnUndo.Enabled = true;
        }
        /*This Event Displays the Research TaskPane
         */
        private void Research_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                _CurrentDocument.CommandBars.ExecuteMso("ResearchPane");
            }
            catch
            {
                return;
            }

        }

        /*Comments Event to display Comments task pane 
         on the right side of the document*/
        private void Comments_Click(object sender, RibbonControlEventArgs e)
        {

            // bool bln=Globals.ThisAddIn.CustomTaskPanes.Select<CommentsTaskPane>(x => x.Title.Contains("Comments & Meta Data"))
            if (CommentsTaskPane.Count == 0)
            {
                Microsoft.Office.Tools.CustomTaskPane _taskpane = Globals.ThisAddIn.CustomTaskPanes.Add(new CommentsTaskPane(), "Comments & Meta Data");
                _taskpane.Width = 500;
                object obj = _taskpane.Window;
                _taskpane.Visible = true;
                _taskpane.VisibleChanged += new EventHandler(CommentsTaskPane_VisibleChanged);
                CommentsTaskPane.Count = CommentsTaskPane.Count + 1;
            }
        }

        void CommentsTaskPane_VisibleChanged(object sender, EventArgs e)
        {
            Microsoft.Office.Tools.CustomTaskPane ctpComment = (Microsoft.Office.Tools.CustomTaskPane)sender;
            // Microsoft.Office.Tools.CustomTaskPane
            if (ctpComment != null)
            {
                if (ctpComment.Visible == false)
                {
                    CommentsTaskPane.Count = CommentsTaskPane.Count - 1;
                }
            }
        }

        /*InternalLink Event to display InternalLink task pane 
        on the right side of the document*/
        private void Internal_Link(object sender, RibbonControlEventArgs e)
        {
            if (InternalLinkTaskPane.Count == 0)
            {
                Microsoft.Office.Tools.CustomTaskPane _taskpane = Globals.ThisAddIn.CustomTaskPanes.Add(new InternalLinkTaskPane(), "Internal Links");
                _taskpane.Width = 500;
                _taskpane.Visible = true;
                _taskpane.VisibleChanged += new EventHandler(InternalTaskPane_VisibleChanged);
                InternalLinkTaskPane.Count = InternalLinkTaskPane.Count + 1;
            }


        }
        void InternalTaskPane_VisibleChanged(object sender, EventArgs e)
        {
            Microsoft.Office.Tools.CustomTaskPane ctpComment = (Microsoft.Office.Tools.CustomTaskPane)sender;
            // Microsoft.Office.Tools.CustomTaskPane
            if (ctpComment != null)
            {
                if (ctpComment.Visible == false)
                {
                    InternalLinkTaskPane.Count = InternalLinkTaskPane.Count - 1;
                }
            }
        }

        /*This Event is for ParagraphFormating with built-in WordStyles
         like Normal Heading, Heading1,Heading2
         Heading3, Heading4 and Heading5 
         
        //15Oct2013 Changed
        // Changed the Quick Styles Button into Gallery format and inserted images which can look like normal word document... */
        private void Gallery_Styles(object sender, RibbonControlEventArgs e)
        {
            Selection selCurrent = _CurrentSelection;
            Range rng = this._CurrentDocument.Content.GoToPrevious(WdGoToItem.wdGoToLine);
            Style sty = rng.ParagraphFormat.get_Style();
            switch (QuickStylesgallery.SelectedItemIndex)
            {
                default: selCurrent.ParagraphFormat.set_Style(WdBuiltinStyle.wdStyleNormal);
                         btnUndo.Enabled = true;
                    break;
                case 0: selCurrent.ParagraphFormat.set_Style(WdBuiltinStyle.wdStyleNormal);
                        btnUndo.Enabled = true;
                    break;
                case 1: selCurrent.ParagraphFormat.set_Style(WdBuiltinStyle.wdStyleHeading1);
                        btnUndo.Enabled = true;
                    break;
                case 2: selCurrent.ParagraphFormat.set_Style(WdBuiltinStyle.wdStyleHeading2);
                        btnUndo.Enabled = true;
                    break;
                case 3: selCurrent.ParagraphFormat.set_Style(WdBuiltinStyle.wdStyleHeading3);
                        btnUndo.Enabled = true;
                    break;
                case 4: selCurrent.ParagraphFormat.set_Style(WdBuiltinStyle.wdStyleHeading4);
                        btnUndo.Enabled = true;
                    break;
                case 5: selCurrent.ParagraphFormat.set_Style(WdBuiltinStyle.wdStyleHeading5);
                    btnUndo.Enabled = true;
                    break;
            }
        }
        //Executes styles pane dialog launcher
        private void Styles_DialogLauncher(object sender, RibbonControlEventArgs e)
        {
            _CurrentDocument.CommandBars.ExecuteMso("StylesPane");

        }
        //Executes Basic Text pane dialog launcher
        private void BasicText_DialogLauncher(object sender, RibbonControlEventArgs e)
        {
            _CurrentDocument.CommandBars.ExecuteMso("FontDialog");

        }
        // Executes clipboard pane dialog launcher
        private void Clipboard_DialogLauncher(object sender, RibbonControlEventArgs e)
        {
            _CurrentDocument.CommandBars.ExecuteMso("ShowClipboard");
        }
        // Executs draw table event
        private void DrawTable_Click(object sender, RibbonControlEventArgs e)
        {
            _CurrentDocument.CommandBars.ExecuteMso("TableDrawTable");
        }
        //executes Insert 
        private void ExcelSpreadSheet_Click(object sender, RibbonControlEventArgs e)
        {
            _CurrentDocument.CommandBars.ExecuteMso("TableExcelSpreadsheetInsert");
        }

        private void Thesaurus_Click(object sender, RibbonControlEventArgs e)
        {
            _CurrentDocument.CommandBars.ExecuteMso("Thesaurus");
        }

        private void Translate_Gallery(object sender, RibbonControlEventArgs e)
        {
            int index = TranslateGallery.SelectedItemIndex;
            switch (index)
            {
                case 0: _CurrentDocument.CommandBars.ExecuteMso("TranslateDocument");
                    break;
                case 1: _CurrentDocument.CommandBars.ExecuteMso("TranslateSelected");
                    break;
                case 2: _CurrentDocument.CommandBars.ExecuteMso("MiniTranslator");
                    break;
            }
        }

        private void TranslateLanguage_Options(object sender, RibbonControlEventArgs e)
        {
            _CurrentDocument.CommandBars.ExecuteMso("TranslationLanguageOptions");
        }

        private void SetProofing_Language(object sender, RibbonControlEventArgs e)
        {
            _CurrentDocument.CommandBars.ExecuteMso("SetLanguage");
        }

        //private static DataSet ConvertJsonStringToDataSet(string jsonString)
        //{
        //    try
        //    {
        //        //XmlDocument xd = new XmlDocument();
        //        jsonString = jsonString.Trim().TrimStart('\"').TrimEnd('\"') ;
        //        string xd = JsonConvert.DeserializeXmlNode(jsonString).ToString();
        //        DataSet ds = new DataSet();
        //        //DataSet ds = JsonConvert.DeserializeObject<DataSet>(jsonString);
        //        //ds.ReadXml(new System.Xml.XmlNodeReader(xd));
        //        return ds;



        //        //Newtonsoft.Json.JsonSerializer json = new Newtonsoft.Json.JsonSerializer();

        //        //json.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
        //        //json.ObjectCreationHandling = Newtonsoft.Json.ObjectCreationHandling.Replace;
        //        //json.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
        //        //json.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

        //        //StringReader sr = new StringReader(jsonString);
        //        //Newtonsoft.Json.JsonTextReader reader = new JsonTextReader(sr);
        //        //DataSet result = json.Deserialize<DataSet>(reader);
        //        //reader.Close();

        //        //return result;

        //    }
        //    catch (Exception ex)
        //    {
        //        throw new ArgumentException(ex.Message);
        //    }
        //}


        protected void DDLBinding(int id,Semantics Stype)
        {
            String type = Stype.ToString();
            string strResult = ProxyService.GetSemanticLists(id, type);
            strResult = strResult.Replace("\\", String.Empty);
            strResult = strResult.Trim().TrimStart('\"').TrimEnd('\"');
            var objSerializer = new JavaScriptSerializer();
            var obj = objSerializer.Deserialize<List<SemanticList>>(strResult);
            RibbonGallery rbbnGallary = null;
            switch (Stype)
            {
                case Semantics.abbreviation: rbbnGallary = glyAbbrevations;
                    break;
                case Semantics.cite: rbbnGallary = glyCites;
                    break;
                case Semantics.idiom: rbbnGallary = glyIdioms;
                    break;
                case Semantics.variable: rbbnGallary = glyVariables;
                    break;
                case Semantics.definition: rbbnGallary = glyDefinitions;
                    break;
                case Semantics.links: rbbnGallary = glyLinks;
                    break;
               
                //case "new": rbbnGallary = null;
                //    break;
            }
            if (rbbnGallary != null)
            {
                try
                {

                    for (int loop = 0; loop < obj.Count; loop++)
                    {
                        string strNameOfItem = obj[loop].Text;
                        Microsoft.Office.Tools.Ribbon.RibbonDropDownItem menuItem = Factory.CreateRibbonDropDownItem();
                        menuItem.Tag = obj[loop].Value;
                        //menuItem.Id = obj[loop].Value.ToString();
                        menuItem.Label = obj[loop].Text;

                        rbbnGallary.Items.Add(menuItem);
                        rbbnGallary.Items[loop].Label = strNameOfItem;
                    }
                }
                catch
                {
                }
            }
            
        }

        private void splitBtnVideo_Click(object sender, RibbonControlEventArgs e)
        {

            _CurrentDocument.CommandBars.ExecuteMso("OleObjectInsertMenu");
        }

        private void toggleBtnListing_Click(object sender, RibbonControlEventArgs e)
        {
            _CurrentDocument.CommandBars.ExecuteMso("ListSetNumberingValue");
        }

        private void galleryListing_Click(object sender, RibbonControlEventArgs e)
        {

            _CurrentDocument.CommandBars.ExecuteMso("MultilevelList");
        }

      

        private void toggleButton2_Click(object sender, RibbonControlEventArgs e)
        {
            
        }

        private void btnGetVideoFromLocalDrive_Click(object sender, RibbonControlEventArgs e)
        {
            _CurrentDocument.CommandBars.ExecuteMso("OleObjectctInsert");
        }


        //    protected void DDLBinding()
        //    {
        //        for (int loop = 0; loop < 10; loop++)
        //        {
        //            Microsoft.Office.Tools.Ribbon.RibbonDropDownItem menuItem = Factory.CreateRibbonDropDownItem();
        //            glyAbbrevations.Items.Add(menuItem);
        //            glyAbbrevations.Items[loop].Label = "Item" + loop;
        //        }
        //        glyAbbrevations.Click += new RibbonControlEventHandler(SemanticItems_Click);
        //        for (int loop = 0; loop < 10; loop++)
        //        {
        //            Microsoft.Office.Tools.Ribbon.RibbonDropDownItem menuItem = Factory.CreateRibbonDropDownItem();
        //            glyCites.Items.Add(menuItem);
        //            glyCites.Items[loop].Label = "Item" + loop;
        //        }
        //        glyCites.Click += new RibbonControlEventHandler(SemanticItems_Click);
        //        for (int loop = 0; loop < 10; loop++)
        //        {
        //            Microsoft.Office.Tools.Ribbon.RibbonDropDownItem menuItem = Factory.CreateRibbonDropDownItem();
        //            glyDefinitions.Items.Add(menuItem);
        //            glyDefinitions.Items[loop].Label = "Item" + loop;
        //        }
        //        glyDefinitions.Click += new RibbonControlEventHandler(SemanticItems_Click);
        //        for (int loop = 0; loop < 10; loop++)
        //        {
        //            Microsoft.Office.Tools.Ribbon.RibbonDropDownItem menuItem = Factory.CreateRibbonDropDownItem();
        //            glyIdioms.Items.Add(menuItem);
        //            glyIdioms.Items[loop].Label = "Item" + loop;
        //        }
        //        glyIdioms.Click += new RibbonControlEventHandler(SemanticItems_Click);
        //        for (int loop = 0; loop < 10; loop++)
        //        {
        //            Microsoft.Office.Tools.Ribbon.RibbonDropDownItem menuItem = Factory.CreateRibbonDropDownItem();
        //            glyIndex.Items.Add(menuItem);
        //            glyIndex.Items[loop].Label = "Item" + loop;
        //        }
        //        glyIndex.Click += new RibbonControlEventHandler(SemanticItems_Click);
        //        for (int loop = 0; loop < 10; loop++)
        //        {
        //            Microsoft.Office.Tools.Ribbon.RibbonDropDownItem menuItem = Factory.CreateRibbonDropDownItem();
        //            glyLinks.Items.Add(menuItem);
        //            glyLinks.Items[loop].Label = "Item" + loop;
        //        }
        //        glyLinks.Click += new RibbonControlEventHandler(SemanticItems_Click);
        //        for (int loop = 0; loop < 10; loop++)
        //        {
        //            Microsoft.Office.Tools.Ribbon.RibbonDropDownItem menuItem = Factory.CreateRibbonDropDownItem();
        //            glyVariables.Items.Add(menuItem);
        //            glyVariables.Items[loop].Label = "Item" + loop;
        //        }
        //        glyVariables.Click += new RibbonControlEventHandler(SemanticItems_Click);
        //    }

        //    void SemanticItems_Click(object sender, RibbonControlEventArgs e)
        //    {
        //        string strName = ((RibbonGallery)sender).Label;
        //         strName = strName + " " + ((RibbonGallery)sender).SelectedItem.Label;
        //    }
        //}
    }
}
