using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Office.Interop.Word;
using TEXXTOOR.Properties;
using TEXXTOOR.Publish;
using TEXXTOOR.Dialogs;
using System.Xml.Linq;
using System.IO;
using TEXXTOOR.TaskPanes;

namespace TEXXTOOR.Services {
  public class CommandService : IService {

    private IList<ICommand> _msoCommands;

    public CommandService() {
      LoadMsoCommands();
    }

    private void LoadMsoCommands() {
      _msoCommands = new List<ICommand>();
      // MSO sends something directly to Word
      _msoCommands.Add(new ActionCommand("splitbtnPublish__btn", PublishCommand));
      _msoCommands.Add(new ActionCommand("btnPublish", PublishCommand));
      _msoCommands.Add(new ActionCommand("btnCheckDocument", PublishCommand));
      _msoCommands.Add(new ActionCommand("btnExport", PublishCommand));

      _msoCommands.Add(new ActionCommand("splitbtnPublishRaw__btn", PublishCommand));
      _msoCommands.Add(new ActionCommand("btnPublishRaw", PublishCommand));
      _msoCommands.Add(new ActionCommand("btnCheckDocumentRaw", PublishCommand));
      _msoCommands.Add(new ActionCommand("btnExportRaw", PublishCommand));

      _msoCommands.Add(new ActionCommand("btnHomePage", GoToHome));
      _msoCommands.Add(new MsoCommand("btnOpenExisting", "FileOpen"));
      _msoCommands.Add(new ActionCommand("btnManageProject", PublishCommand));

      _msoCommands.Add(new ActionCommand("btnActivate", ActivateTemplate));
      _msoCommands.Add(new ActionCommand("btnFullAddIn", ActivateAddIn));

      //_msoCommands.Add(new MsoCommand("btnPaste", "Paste"));
      _msoCommands.Add(new ActionCommand("btnPaste", PlainPaste));
      _msoCommands.Add(new MsoCommand("btnCut", "Cut"));
      _msoCommands.Add(new MsoCommand("btnCopy", "Copy"));
      _msoCommands.Add(new MsoCommand("btnUndo", "Undo"));
      _msoCommands.Add(new MsoCommand("btnRedo", "Redo"));

      _msoCommands.Add(new MsoCommand("toggleBtnBold", "Bold"));
      _msoCommands.Add(new MsoCommand("toggleBtnItalic", "Italic"));
      _msoCommands.Add(new MsoCommand("toggleBtnUnderline", "Underline"));
      _msoCommands.Add(new MsoCommand("toggleBtnAlignLeft", "AlignLeft"));
      _msoCommands.Add(new MsoCommand("toggleBtnAlignCenter", "AlignCenter"));
      _msoCommands.Add(new MsoCommand("toggleBtnAlignRight", "AlignRight"));

      _msoCommands.Add(new MsoCommand("btnBulletList", "Bullets"));
      _msoCommands.Add(new MsoCommand("btnNumberList", "Numbering"));
      _msoCommands.Add(new MsoCommand("btnSubscript", "Subscript"));
      _msoCommands.Add(new MsoCommand("btnSuperscript", "Superscript"));
      _msoCommands.Add(new MsoCommand("btnRightIndent", "IndentIncreaseWord", IndentParagraph));
      _msoCommands.Add(new MsoCommand("btnLeftIndent", "IndentDecreaseWord", IndentParagraph));

      _msoCommands.Add(new MsoCommand("splitBtnSpelling__btn", "SpellingAndGrammar"));
      _msoCommands.Add(new MsoCommand("btnResearch", "ResearchPane"));
      _msoCommands.Add(new MsoCommand("btnThesaurus", "Thesaurus"));
      _msoCommands.Add(new MsoCommand("btnSpellingAndGrammar", "SpellingAndGrammar"));
      _msoCommands.Add(new MsoCommand("splitBtnTranslate__btn", "TranslateDocument"));
      _msoCommands.Add(new MsoCommand("btnSetLanguage", "SetLanguage"));
      _msoCommands.Add(new MsoCommand("btnWordCount", "WordCount"));

      _msoCommands.Add(new ActionCommand("btnInsertText", InsertElement));

      _msoCommands.Add(new ActionCommand("splitBtnPicture__btn", InsertLocalFigure));
      _msoCommands.Add(new ActionCommand("btnGetPicturefromLocal", InsertLocalFigure));
      _msoCommands.Add(new ActionCommand("btnShowServerImage", RetrieveServerImages));

      _msoCommands.Add(new ActionCommand("splitBtnTable__btn", InsertTable));
      _msoCommands.Add(new ActionCommand("btnInsertTable", InsertTable));

      _msoCommands.Add(new ActionCommand("btnInsertVideo", InsertVideo));

      _msoCommands.Add(new ActionCommand("btnInsertCodeSnippet", InsertElement));
      _msoCommands.Add(new ActionCommand("btnInsertSideBar", InsertElement));

      _msoCommands.Add(new ActionCommand("btnDelete", DeleteSnippet));

      _msoCommands.Add(new MsoCommand("btnDrawTable", "TableDrawTable"));
      _msoCommands.Add(new MsoCommand("btnConvertTextToTable", "ConvertTextToTable"));

      _msoCommands.Add(new MsoCommand("grpClipboard__btn", "ShowClipboard"));
      _msoCommands.Add(new MsoCommand("grpBasicText__btn", "FontDialog"));
      _msoCommands.Add(new MsoCommand("btnQuickTables", "TabTableToolsDesign"));

      _msoCommands.Add(new ActionCommand("btnIndex", InsertIndex));
      _msoCommands.Add(new ActionCommand("btnRefreshLists", ResetSemanticList));

      _msoCommands.Add(new ActionCommand("btn_CharacterCode", SemanticFormat));
      _msoCommands.Add(new ActionCommand("btn_CharacterFile", SemanticFormat));
      _msoCommands.Add(new ActionCommand("btn_CharacterKeystroke", SemanticFormat));

      _msoCommands.Add(new ActionCommand("btnSidebarNote", SetSideBarType));
      _msoCommands.Add(new ActionCommand("btnSidebarWarning", SetSideBarType));
      _msoCommands.Add(new ActionCommand("btnSidebarTip", SetSideBarType));
      _msoCommands.Add(new ActionCommand("btnSidebarCustom", SetSideBarType));

      _msoCommands.Add(new ActionCommand("btnHeading1", InsertSection));
      _msoCommands.Add(new ActionCommand("btnHeading2", InsertSection));
      _msoCommands.Add(new ActionCommand("btnHeading3", InsertSection));
      _msoCommands.Add(new ActionCommand("btnHeading4", InsertSection));
      _msoCommands.Add(new ActionCommand("btnHeading5", InsertSection));

      _msoCommands.Add(new ActionCommand("btnMakeText", ConvertSection));
      _msoCommands.Add(new ActionCommand("btnMakeHeading1", ConvertSection));
      _msoCommands.Add(new ActionCommand("btnMakeHeading2", ConvertSection));
      _msoCommands.Add(new ActionCommand("btnMakeHeading3", ConvertSection));
      _msoCommands.Add(new ActionCommand("btnMakeHeading4", ConvertSection));
      _msoCommands.Add(new ActionCommand("btnMakeHeading5", ConvertSection));

      _msoCommands.Add(new ActionCommand("btnAbout", ShowAbout));
    }


    public IList<ICommand> MsoCommands {
      get {
        return _msoCommands;
      }
    }

    internal void ExecuteMso(string command, object parameter = null) {
      MsoCommands.Single(c => c.CommandId == command).Execute(parameter);
    }


    internal bool ExecuteToggleMso(string command, bool pressed) {
      MsoCommands.Single(c => c.CommandId == command).Execute(pressed);
      return GetButtonPressed(command);
    }

    internal void Execute(string command, object parameter = null) {
      MsoCommands.Single(c => c.CommandId == command).Execute(parameter);
    }

    internal bool GetButtonPressed(string controlId) {
      try {
        var currentSelection = ServicePool.Instance.GetService<DocumentService>().CurrentSelection;
        if (currentSelection.Paragraphs.Alignment == WdParagraphAlignment.wdAlignParagraphLeft && controlId == "toggleBtnAlignLeft") {
          return true;
        }
        if (currentSelection.Paragraphs.Alignment == WdParagraphAlignment.wdAlignParagraphCenter && controlId == "toggleBtnAlignCenter") {
          return true;
        }
        if (currentSelection.Paragraphs.Alignment == WdParagraphAlignment.wdAlignParagraphRight && controlId == "toggleBtnAlignRight") {
          return true;
        }
        if (currentSelection.Font.Bold != 0 && controlId == "toggleBtnBold") {
          return true;
        }
        if (currentSelection.Font.Italic != 0 && controlId == "toggleBtnItalic") {
          return true;
        }
        if (currentSelection.Font.Underline != 0 && controlId == "toggleBtnUnderline") {
          return true;
        }
        if (ServicePool.Instance.GetService<DocumentService>().CheckLocalizedStyleName(currentSelection.get_Style(), WdBuiltinStyle.wdStyleListBullet)) {
          return true;
        }
        if (ServicePool.Instance.GetService<DocumentService>().CheckLocalizedStyleName(currentSelection.get_Style(), WdBuiltinStyle.wdStyleListNumber)) {
          return true;
        }
      } catch (Exception ex) {
        Trace.TraceError(ex.Message, "GetButtonPressed");
      }
      return false;
    }

    internal void PlainPaste(string strOfficeId, object parameter) {
      try {
        ServicePool.Instance.GetService<DocumentService>().CurrentSelection
            .PasteSpecial(Type.Missing, Type.Missing, Type.Missing, Type.Missing, WdPasteDataType.wdPasteText, Type.Missing, Type.Missing);
      } catch (Exception ex) {

      }
    }

    internal void IndentParagraph(string strOfficeId = null) {
      var currentSelection = ServicePool.Instance.GetService<DocumentService>().CurrentSelection;
      var lst = currentSelection.Range.ListFormat.List;
      if (lst != null) {
        new MsoCommand("", strOfficeId).Execute(null);
      }
    }

    internal void ConvertSection(string strOfficeId, object parameter) {
      switch (strOfficeId) {
        case "btnMakeText":
          ServicePool.Instance.GetService<DocumentService>().ConvertSection(0);
          break;
        case "btnMakeHeading1":
          ServicePool.Instance.GetService<DocumentService>().ConvertSection(1);
          break;
        case "btnMakeHeading2":
          ServicePool.Instance.GetService<DocumentService>().ConvertSection(2);
          break;
        case "btnMakeHeading3":
          ServicePool.Instance.GetService<DocumentService>().ConvertSection(3);
          break;
        case "btnMakeHeading4":
          ServicePool.Instance.GetService<DocumentService>().ConvertSection(4);
          break;
        case "btnMakeHeading5":
          ServicePool.Instance.GetService<DocumentService>().ConvertSection(5);
          break;
      }
    }

    internal void InsertSection(string strOfficeId, object parameter) {
      switch (strOfficeId) {
        case "btnHeading1":
          ServicePool.Instance.GetService<DocumentService>().InsertSection(1);
          break;
        case "btnHeading2":
          ServicePool.Instance.GetService<DocumentService>().InsertSection(2);
          break;
        case "btnHeading3":
          ServicePool.Instance.GetService<DocumentService>().InsertSection(3);
          break;
        case "btnHeading4":
          ServicePool.Instance.GetService<DocumentService>().InsertSection(4);
          break;
        case "btnHeading5":
          ServicePool.Instance.GetService<DocumentService>().InsertSection(5);
          break;
      }
    }

    internal void InsertLocalFigure() {
      ServicePool.Instance.GetService<DocumentService>().InsertFigure();
    }

    internal void SetRange() {
      var currentRange = ServicePool.Instance.GetService<DocumentService>().CurrentRange;
      var currentSelection = ServicePool.Instance.GetService<DocumentService>().CurrentSelection;
      currentRange.SetRange(currentSelection.Text.Length, currentSelection.Text.Length);
    }

    internal void GoToHome() {
      var documentId = ServicePool.Instance.GetService<ServerService>().GetDocumentId(false);
      var target = Resources.CommandService_GoToHome;
      if (documentId != 0) {
        target = Resources.CommandService_GoToHome_Opus_Edit_ + documentId;
      }
      try {
        Process.Start(target);
      } catch (Win32Exception noBrowser) {
        if (noBrowser.ErrorCode == -2147467259)
          MessageBox.Show(noBrowser.Message);
      } catch (Exception other) {
        MessageBox.Show(other.Message);
      }
    }

    internal void InsertElement(string controlId, object parameter = null) {

      switch (controlId) {
        case "btnInsertText":
          ServicePool.Instance.GetService<DocumentService>().InsertText();
          break;
        case "btnInsertCodeSnippet": //Code Snippet (Listing)
          ServicePool.Instance.GetService<DocumentService>().InsertListing();
          break;
        case "btnInsertSideBar":
          ServicePool.Instance.GetService<DocumentService>().InsertSidebar();
          break;
      }
    }

    internal void ActivateTemplate(string strOfficeId, object parameter = null) {
      ServicePool.Instance.GetService<DocumentService>().ApplyTemplate();
    }

    internal void ActivateAddIn(string strOfficeId, object parameter = null) {
      ServicePool.Instance.GetService<DocumentService>().JustChecking = false;
      ServicePool.Instance.GetService<DocumentService>().ApplyTemplate();
      ServicePool.Instance.GetService<AddInService>().AddInEnabled = true;
    }

    internal void PublishCommand(string strOfficeId, object parameter = null) {
      var loggedOn = false;
      string cleanUpPath;
      try {
        GetDocument getDocumentDlg = null;
        switch (strOfficeId) {
          case "btnManageProject":
            loggedOn = ServicePool.Instance.GetService<ServerService>().LogOn();
            if (!loggedOn) return;
            getDocumentDlg = new GetDocument();
            getDocumentDlg.ShowDialog();
            break;
          case "btnCheckDocumentRaw": // check conversion and upload
            ServicePool.Instance.GetService<DocumentService>().JustChecking = true;
            ServicePool.Instance.GetService<DocumentService>().CheckDocument();
            break;
          case "btnCheckDocument": // check conversion and upload
            ServicePool.Instance.GetService<DocumentService>().CheckDocument();
            break;
          case "splitbtnPublishRaw__btn":
            ServicePool.Instance.GetService<DocumentService>().JustChecking = true;
            goto case "splitbtnPublish__btn";
          case "btnPublishRaw": // check conversion (not loaded)
            ServicePool.Instance.GetService<DocumentService>().JustChecking = true;
            goto case "btnPublish";
          case "splitbtnPublish__btn":
          case "btnPublish": // check conversion
            loggedOn = ServicePool.Instance.GetService<ServerService>().LogOn();
            if (!loggedOn) return;
            string export;
            try {
              var name = ServicePool.Instance.GetService<DocumentService>().CurrentDocument.Name;
              var content = ServicePool.Instance.GetService<DocumentService>().GetDocumentContent(out cleanUpPath);              
              export = Convertor.Export(content, name, cleanUpPath);
            } catch (Exception ex) {
              MessageBox.Show(ex.Message, Resources.CommandService_PublishCommand_Error_Checking, MessageBoxButtons.OK, MessageBoxIcon.Error);
              break;
            }
            ServicePool.Instance.GetService<ServerService>().PublishDocument(export);
            break;
          case "btnExportRaw": // check conversion
          case "btnExport": // check conversion
            if (ServicePool.Instance.GetService<DocumentService>().ConsistencyChecker()) {
              try {
                var name = ServicePool.Instance.GetService<DocumentService>().CurrentDocument.Name;
                var content = ServicePool.Instance.GetService<DocumentService>().GetDocumentContent(out cleanUpPath);
                export = Convertor.Export(content, name, cleanUpPath);
                if (export != null) {
                  var sfd = new SaveFileDialog {
                    Filter = Resources.CommandService_PublishCommand_Texxtoor_Backup, 
                    InitialDirectory = RibbonTexxtoor.LOCAL_APPN_DATA_PATH, 
                    SupportMultiDottedExtensions = true, 
                    OverwritePrompt = true
                  };
                  if (sfd.ShowDialog() == DialogResult.OK) {
                    File.WriteAllText(sfd.FileName, export);
                  }
                } else {
                  throw new InvalidOperationException(Resources.CommandService_PublishCommand_Cannot_convert_document);
                }
              } catch (Exception ex) {
                MessageBox.Show(ex.Message, Resources.CommandService_PublishCommand_Error_Checking, MessageBoxButtons.OK, MessageBoxIcon.Error);
              }
            } else {
              MessageBox.Show(Resources.CommandService_PublishCommand_Cannot_export_because, Resources.CommandService_PublishCommand_Error_Checking, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            break;
          default:
            ExecuteMso(MsoCommands.Single(m => m.CommandId == strOfficeId).CommandId);
            break;
        }
      } catch {
        return;
      }
    }

    internal void InsertTable(string strOfficeId, object parameter = null) {
      ServicePool.Instance.GetService<DocumentService>().InsertTable();
    }

    internal void InsertVideo(string strOfficeId, object parameter = null) {
      ServicePool.Instance.GetService<DocumentService>().InsertVideo();
    }

    internal void DeleteSnippet(string strOfficeId, object parameter = null) {
      ServicePool.Instance.GetService<DocumentService>().DeleteSnippet();
    }

    internal void InsertServerPicture(int id) {
      // 1. connect to server and request resources assigned to project
      // 2. show figure selection dialog with server images
      // 3. load image from server and embed with back reference
    }

    internal void RetrieveServerImages(string strOfficeId, object parameter = null) {
      if (ServicePool.Instance.GetService<ServerService>().LogOn()) {
        ServicePool.Instance.GetService<ServerService>().GetDocumentId();
      }
    }

    internal void ResetSemanticList(string strOfficeId, object parameter = null) {
      ServicePool.Instance.GetService<AddInService>().ResetSemanticList();
    }

    internal void InsertIndex(string strOfficeId, object parameter = null) {
      ServicePool.Instance.GetService<DocumentService>().InsertIndex();
    }

    internal void SemanticFormat(string strOfficeId, object parameter = null) {
      ServicePool.Instance.GetService<DocumentService>().InsertSemanticElement(strOfficeId);
    }

    internal void SetSideBarType(string strOfficeId, object parameter = null) {
      ServicePool.Instance.GetService<DocumentService>().SetSidebarType(strOfficeId);
    }

    internal void ShowAbout(string strOfficeId, object parameter = null) {
      new AboutBox().ShowDialog();
    }

  }
}
