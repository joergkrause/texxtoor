namespace TEXXTOOR
{
    partial class TexxtoorRibbon : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public TexxtoorRibbon()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Microsoft.Office.Tools.Ribbon.RibbonDialogLauncher ribbonDialogLauncherImpl1 = this.Factory.CreateRibbonDialogLauncher();
            Microsoft.Office.Tools.Ribbon.RibbonDialogLauncher ribbonDialogLauncherImpl2 = this.Factory.CreateRibbonDialogLauncher();
            Microsoft.Office.Tools.Ribbon.RibbonDialogLauncher ribbonDialogLauncherImpl3 = this.Factory.CreateRibbonDialogLauncher();
            Microsoft.Office.Tools.Ribbon.RibbonDropDownItem ribbonDropDownItemImpl1 = this.Factory.CreateRibbonDropDownItem();
            Microsoft.Office.Tools.Ribbon.RibbonDropDownItem ribbonDropDownItemImpl2 = this.Factory.CreateRibbonDropDownItem();
            Microsoft.Office.Tools.Ribbon.RibbonDropDownItem ribbonDropDownItemImpl3 = this.Factory.CreateRibbonDropDownItem();
            Microsoft.Office.Tools.Ribbon.RibbonDropDownItem ribbonDropDownItemImpl4 = this.Factory.CreateRibbonDropDownItem();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TexxtoorRibbon));
            Microsoft.Office.Tools.Ribbon.RibbonDropDownItem ribbonDropDownItemImpl5 = this.Factory.CreateRibbonDropDownItem();
            Microsoft.Office.Tools.Ribbon.RibbonDropDownItem ribbonDropDownItemImpl6 = this.Factory.CreateRibbonDropDownItem();
            Microsoft.Office.Tools.Ribbon.RibbonDropDownItem ribbonDropDownItemImpl7 = this.Factory.CreateRibbonDropDownItem();
            Microsoft.Office.Tools.Ribbon.RibbonDropDownItem ribbonDropDownItemImpl8 = this.Factory.CreateRibbonDropDownItem();
            Microsoft.Office.Tools.Ribbon.RibbonDropDownItem ribbonDropDownItemImpl9 = this.Factory.CreateRibbonDropDownItem();
            Microsoft.Office.Tools.Ribbon.RibbonDropDownItem ribbonDropDownItemImpl10 = this.Factory.CreateRibbonDropDownItem();
            Microsoft.Office.Tools.Ribbon.RibbonDropDownItem ribbonDropDownItemImpl11 = this.Factory.CreateRibbonDropDownItem();
            Microsoft.Office.Tools.Ribbon.RibbonDropDownItem ribbonDropDownItemImpl12 = this.Factory.CreateRibbonDropDownItem();
            Microsoft.Office.Tools.Ribbon.RibbonDropDownItem ribbonDropDownItemImpl13 = this.Factory.CreateRibbonDropDownItem();
            this.tabTEXXTOOR = this.Factory.CreateRibbonTab();
            this.grpTextoorText = this.Factory.CreateRibbonGroup();
            this.grpClipboard = this.Factory.CreateRibbonGroup();
            this.separator2 = this.Factory.CreateRibbonSeparator();
            this.buttonGroup1 = this.Factory.CreateRibbonButtonGroup();
            this.grpBasicText = this.Factory.CreateRibbonGroup();
            this.btnGrpFormat = this.Factory.CreateRibbonButtonGroup();
            this.btnGrpAlignment = this.Factory.CreateRibbonButtonGroup();
            this.separator1 = this.Factory.CreateRibbonSeparator();
            this.buttonGroup2 = this.Factory.CreateRibbonButtonGroup();
            this.buttonGroup3 = this.Factory.CreateRibbonButtonGroup();
            this.grpProofing = this.Factory.CreateRibbonGroup();
            this.groupStyles = this.Factory.CreateRibbonGroup();
            this.tabInsert = this.Factory.CreateRibbonTab();
            this.groupInsertText = this.Factory.CreateRibbonGroup();
            this.groupInsertFigure = this.Factory.CreateRibbonGroup();
            this.groupInsertTable = this.Factory.CreateRibbonGroup();
            this.tabSemantics = this.Factory.CreateRibbonTab();
            this.grpSemanticElements = this.Factory.CreateRibbonGroup();
            this.grpSocialInteraction = this.Factory.CreateRibbonGroup();
            this.grpSideBar = this.Factory.CreateRibbonGroup();
            this.grpLinks = this.Factory.CreateRibbonGroup();
            this.grpSemanticText = this.Factory.CreateRibbonGroup();
            this.groupInsertVideo = this.Factory.CreateRibbonGroup();
            this.splitbtnPublish = this.Factory.CreateRibbonSplitButton();
            this.btnPublish = this.Factory.CreateRibbonButton();
            this.btnPublishAsDraft = this.Factory.CreateRibbonButton();
            this.btnHomePage = this.Factory.CreateRibbonButton();
            this.btnOpenExisting = this.Factory.CreateRibbonButton();
            this.btnManageProject = this.Factory.CreateRibbonButton();
            this.btnPaste = this.Factory.CreateRibbonButton();
            this.btnCut = this.Factory.CreateRibbonButton();
            this.btnCopy = this.Factory.CreateRibbonButton();
            this.btnUndo = this.Factory.CreateRibbonButton();
            this.btnRedo = this.Factory.CreateRibbonButton();
            this.toggleBtnBold = this.Factory.CreateRibbonToggleButton();
            this.toggleBtnItalic = this.Factory.CreateRibbonToggleButton();
            this.toggleBtnUnderline = this.Factory.CreateRibbonToggleButton();
            this.toggleBtnStrikeThrough = this.Factory.CreateRibbonToggleButton();
            this.toggleBtnAlignLeft = this.Factory.CreateRibbonToggleButton();
            this.toggleBtnAlignCenter = this.Factory.CreateRibbonToggleButton();
            this.toggleBtnAlignRight = this.Factory.CreateRibbonToggleButton();
            this.toggleButtonBullets = this.Factory.CreateRibbonToggleButton();
            this.toggleButtonNumbering = this.Factory.CreateRibbonToggleButton();
            this.btnDecreaseIndent = this.Factory.CreateRibbonButton();
            this.btnIncreaseIndent = this.Factory.CreateRibbonButton();
            this.splitBtnSpelling = this.Factory.CreateRibbonSplitButton();
            this.btnSpellingAndGrammar = this.Factory.CreateRibbonButton();
            this.btnResearch = this.Factory.CreateRibbonButton();
            this.btnThesaurus = this.Factory.CreateRibbonButton();
            this.TranslateGallery = this.Factory.CreateRibbonGallery();
            this.btnChooseTranslationLanguage = this.Factory.CreateRibbonButton();
            this.btnSetLanguage = this.Factory.CreateRibbonButton();
            this.btnWordCount = this.Factory.CreateRibbonButton();
            this.QuickStylesgallery = this.Factory.CreateRibbonGallery();
            this.btnInsertText = this.Factory.CreateRibbonButton();
            this.splitBtnPicture = this.Factory.CreateRibbonSplitButton();
            this.btnGetPicturefromLocal = this.Factory.CreateRibbonButton();
            this.btnGetPicturefromServer = this.Factory.CreateRibbonButton();
            this.splitBtnTable = this.Factory.CreateRibbonSplitButton();
            this.btnInsertTable = this.Factory.CreateRibbonButton();
            this.toggleButton1 = this.Factory.CreateRibbonToggleButton();
            this.btnConvertTextToTable = this.Factory.CreateRibbonButton();
            this.btnExcelSpreadSheet = this.Factory.CreateRibbonButton();
            this.galleryQuickTables = this.Factory.CreateRibbonGallery();
            this.splitBtnVideo = this.Factory.CreateRibbonSplitButton();
            this.btnGetVideoFromLocalDrive = this.Factory.CreateRibbonButton();
            this.btnGetVideoFromServer = this.Factory.CreateRibbonButton();
            this.glyAbbrevations = this.Factory.CreateRibbonGallery();
            this.glyCites = this.Factory.CreateRibbonGallery();
            this.glyLinks = this.Factory.CreateRibbonGallery();
            this.glyIdioms = this.Factory.CreateRibbonGallery();
            this.glyVariables = this.Factory.CreateRibbonGallery();
            this.glyDefinitions = this.Factory.CreateRibbonGallery();
            this.glyIndex = this.Factory.CreateRibbonGallery();
            this.btnComments = this.Factory.CreateRibbonButton();
            this.menuType = this.Factory.CreateRibbonMenu();
            this.btnInternalLink = this.Factory.CreateRibbonButton();
            this.btnRemoveLink = this.Factory.CreateRibbonButton();
            this.splitBtnCode = this.Factory.CreateRibbonSplitButton();
            this.splitBtnFile = this.Factory.CreateRibbonSplitButton();
            this.splitBtnKeystroke = this.Factory.CreateRibbonSplitButton();
            this.tabTEXXTOOR.SuspendLayout();
            this.grpTextoorText.SuspendLayout();
            this.grpClipboard.SuspendLayout();
            this.buttonGroup1.SuspendLayout();
            this.grpBasicText.SuspendLayout();
            this.btnGrpFormat.SuspendLayout();
            this.btnGrpAlignment.SuspendLayout();
            this.buttonGroup2.SuspendLayout();
            this.buttonGroup3.SuspendLayout();
            this.grpProofing.SuspendLayout();
            this.groupStyles.SuspendLayout();
            this.tabInsert.SuspendLayout();
            this.groupInsertText.SuspendLayout();
            this.groupInsertFigure.SuspendLayout();
            this.groupInsertTable.SuspendLayout();
            this.tabSemantics.SuspendLayout();
            this.grpSemanticElements.SuspendLayout();
            this.grpSocialInteraction.SuspendLayout();
            this.grpSideBar.SuspendLayout();
            this.grpLinks.SuspendLayout();
            this.grpSemanticText.SuspendLayout();
            this.groupInsertVideo.SuspendLayout();
            // 
            // tabTEXXTOOR
            // 
            this.tabTEXXTOOR.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.tabTEXXTOOR.Groups.Add(this.grpTextoorText);
            this.tabTEXXTOOR.Groups.Add(this.grpClipboard);
            this.tabTEXXTOOR.Groups.Add(this.grpBasicText);
            this.tabTEXXTOOR.Groups.Add(this.grpProofing);
            this.tabTEXXTOOR.Groups.Add(this.groupStyles);
            this.tabTEXXTOOR.Label = "TEXXTOOR";
            this.tabTEXXTOOR.Name = "tabTEXXTOOR";
            // 
            // grpTextoorText
            // 
            this.grpTextoorText.Items.Add(this.splitbtnPublish);
            this.grpTextoorText.Items.Add(this.btnHomePage);
            this.grpTextoorText.Items.Add(this.btnOpenExisting);
            this.grpTextoorText.Items.Add(this.btnManageProject);
            this.grpTextoorText.Label = "Texxtoor Text";
            this.grpTextoorText.Name = "grpTextoorText";
            // 
            // grpClipboard
            // 
            this.grpClipboard.DialogLauncher = ribbonDialogLauncherImpl1;
            this.grpClipboard.Items.Add(this.btnPaste);
            this.grpClipboard.Items.Add(this.btnCut);
            this.grpClipboard.Items.Add(this.btnCopy);
            this.grpClipboard.Items.Add(this.separator2);
            this.grpClipboard.Items.Add(this.buttonGroup1);
            this.grpClipboard.Label = "Clipboard";
            this.grpClipboard.Name = "grpClipboard";
            this.grpClipboard.DialogLauncherClick += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.Clipboard_DialogLauncher);
            // 
            // separator2
            // 
            this.separator2.Name = "separator2";
            // 
            // buttonGroup1
            // 
            this.buttonGroup1.Items.Add(this.btnUndo);
            this.buttonGroup1.Items.Add(this.btnRedo);
            this.buttonGroup1.Name = "buttonGroup1";
            // 
            // grpBasicText
            // 
            this.grpBasicText.DialogLauncher = ribbonDialogLauncherImpl2;
            this.grpBasicText.Items.Add(this.btnGrpFormat);
            this.grpBasicText.Items.Add(this.btnGrpAlignment);
            this.grpBasicText.Items.Add(this.separator1);
            this.grpBasicText.Items.Add(this.buttonGroup2);
            this.grpBasicText.Items.Add(this.buttonGroup3);
            this.grpBasicText.Label = "Basic Text";
            this.grpBasicText.Name = "grpBasicText";
            this.grpBasicText.DialogLauncherClick += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.BasicText_DialogLauncher);
            // 
            // btnGrpFormat
            // 
            this.btnGrpFormat.Items.Add(this.toggleBtnBold);
            this.btnGrpFormat.Items.Add(this.toggleBtnItalic);
            this.btnGrpFormat.Items.Add(this.toggleBtnUnderline);
            this.btnGrpFormat.Items.Add(this.toggleBtnStrikeThrough);
            this.btnGrpFormat.Name = "btnGrpFormat";
            // 
            // btnGrpAlignment
            // 
            this.btnGrpAlignment.Items.Add(this.toggleBtnAlignLeft);
            this.btnGrpAlignment.Items.Add(this.toggleBtnAlignCenter);
            this.btnGrpAlignment.Items.Add(this.toggleBtnAlignRight);
            this.btnGrpAlignment.Name = "btnGrpAlignment";
            // 
            // separator1
            // 
            this.separator1.Name = "separator1";
            // 
            // buttonGroup2
            // 
            this.buttonGroup2.Items.Add(this.toggleButtonBullets);
            this.buttonGroup2.Items.Add(this.toggleButtonNumbering);
            this.buttonGroup2.Name = "buttonGroup2";
            // 
            // buttonGroup3
            // 
            this.buttonGroup3.Items.Add(this.btnDecreaseIndent);
            this.buttonGroup3.Items.Add(this.btnIncreaseIndent);
            this.buttonGroup3.Name = "buttonGroup3";
            // 
            // grpProofing
            // 
            this.grpProofing.Items.Add(this.splitBtnSpelling);
            this.grpProofing.Label = "Proofing";
            this.grpProofing.Name = "grpProofing";
            // 
            // groupStyles
            // 
            this.groupStyles.DialogLauncher = ribbonDialogLauncherImpl3;
            this.groupStyles.Items.Add(this.QuickStylesgallery);
            this.groupStyles.Label = "Styles";
            this.groupStyles.Name = "groupStyles";
            this.groupStyles.DialogLauncherClick += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.Styles_DialogLauncher);
            // 
            // tabInsert
            // 
            this.tabInsert.Groups.Add(this.groupInsertText);
            this.tabInsert.Groups.Add(this.groupInsertFigure);
            this.tabInsert.Groups.Add(this.groupInsertTable);
            this.tabInsert.Groups.Add(this.groupInsertVideo);
            this.tabInsert.Label = "Insert";
            this.tabInsert.Name = "tabInsert";
            // 
            // groupInsertText
            // 
            this.groupInsertText.Items.Add(this.btnInsertText);
            this.groupInsertText.Label = "Text";
            this.groupInsertText.Name = "groupInsertText";
            // 
            // groupInsertFigure
            // 
            this.groupInsertFigure.Items.Add(this.splitBtnPicture);
            this.groupInsertFigure.Label = "Figure";
            this.groupInsertFigure.Name = "groupInsertFigure";
            // 
            // groupInsertTable
            // 
            this.groupInsertTable.Items.Add(this.splitBtnTable);
            this.groupInsertTable.Label = "Table";
            this.groupInsertTable.Name = "groupInsertTable";
            // 
            // tabSemantics
            // 
            this.tabSemantics.Groups.Add(this.grpSemanticElements);
            this.tabSemantics.Groups.Add(this.grpSocialInteraction);
            this.tabSemantics.Groups.Add(this.grpSideBar);
            this.tabSemantics.Groups.Add(this.grpLinks);
            this.tabSemantics.Groups.Add(this.grpSemanticText);
            this.tabSemantics.Label = "Semantic";
            this.tabSemantics.Name = "tabSemantics";
            // 
            // grpSemanticElements
            // 
            this.grpSemanticElements.Items.Add(this.glyAbbrevations);
            this.grpSemanticElements.Items.Add(this.glyCites);
            this.grpSemanticElements.Items.Add(this.glyLinks);
            this.grpSemanticElements.Items.Add(this.glyIdioms);
            this.grpSemanticElements.Items.Add(this.glyVariables);
            this.grpSemanticElements.Items.Add(this.glyDefinitions);
            this.grpSemanticElements.Items.Add(this.glyIndex);
            this.grpSemanticElements.Label = "Semantic Elements";
            this.grpSemanticElements.Name = "grpSemanticElements";
            // 
            // grpSocialInteraction
            // 
            this.grpSocialInteraction.Items.Add(this.btnComments);
            this.grpSocialInteraction.Label = "Social Interaction";
            this.grpSocialInteraction.Name = "grpSocialInteraction";
            // 
            // grpSideBar
            // 
            this.grpSideBar.Items.Add(this.menuType);
            this.grpSideBar.Label = "Side Bar";
            this.grpSideBar.Name = "grpSideBar";
            // 
            // grpLinks
            // 
            this.grpLinks.Items.Add(this.btnInternalLink);
            this.grpLinks.Items.Add(this.btnRemoveLink);
            this.grpLinks.Label = "Links";
            this.grpLinks.Name = "grpLinks";
            // 
            // grpSemanticText
            // 
            this.grpSemanticText.Items.Add(this.splitBtnCode);
            this.grpSemanticText.Items.Add(this.splitBtnFile);
            this.grpSemanticText.Items.Add(this.splitBtnKeystroke);
            this.grpSemanticText.Label = "Semantic Text";
            this.grpSemanticText.Name = "grpSemanticText";
            // 
            // groupInsertVideo
            // 
            this.groupInsertVideo.Items.Add(this.splitBtnVideo);
            this.groupInsertVideo.Label = "Video";
            this.groupInsertVideo.Name = "groupInsertVideo";
            // 
            // splitbtnPublish
            // 
            this.splitbtnPublish.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.splitbtnPublish.Items.Add(this.btnPublish);
            this.splitbtnPublish.Items.Add(this.btnPublishAsDraft);
            this.splitbtnPublish.Label = "Publish";
            this.splitbtnPublish.Name = "splitbtnPublish";
            this.splitbtnPublish.OfficeImageId = "BlogPublishMenu";
            this.splitbtnPublish.SuperTip = "Publish the post to the blog account so that other people can read it.";
            this.splitbtnPublish.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.Publish_Click);
            // 
            // btnPublish
            // 
            this.btnPublish.Label = "Publish";
            this.btnPublish.Name = "btnPublish";
            this.btnPublish.OfficeImageId = "BlogPublish";
            this.btnPublish.ShowImage = true;
            this.btnPublish.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.Publish_Click);
            // 
            // btnPublishAsDraft
            // 
            this.btnPublishAsDraft.Label = "Publish As Draft";
            this.btnPublishAsDraft.Name = "btnPublishAsDraft";
            this.btnPublishAsDraft.OfficeImageId = "BlogPublishDraft";
            this.btnPublishAsDraft.ShowImage = true;
            this.btnPublishAsDraft.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.Publish_Click);
            // 
            // btnHomePage
            // 
            this.btnHomePage.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnHomePage.Enabled = false;
            this.btnHomePage.Label = "Home Page";
            this.btnHomePage.Name = "btnHomePage";
            this.btnHomePage.OfficeImageId = "BlogHomePage";
            this.btnHomePage.ShowImage = true;
            // 
            // btnOpenExisting
            // 
            this.btnOpenExisting.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnOpenExisting.Label = "Open Existing";
            this.btnOpenExisting.Name = "btnOpenExisting";
            this.btnOpenExisting.OfficeImageId = "BlogOpenExisting";
            this.btnOpenExisting.ShowImage = true;
            this.btnOpenExisting.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.OpenExisting_Click);
            // 
            // btnManageProject
            // 
            this.btnManageProject.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnManageProject.Label = "Manage Projects";
            this.btnManageProject.Name = "btnManageProject";
            this.btnManageProject.OfficeImageId = "BlogManageAccounts";
            this.btnManageProject.ShowImage = true;
            this.btnManageProject.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.ManageProject_Click);
            // 
            // btnPaste
            // 
            this.btnPaste.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnPaste.Enabled = false;
            this.btnPaste.Label = "Paste";
            this.btnPaste.Name = "btnPaste";
            this.btnPaste.OfficeImageId = "Paste";
            this.btnPaste.ShowImage = true;
            this.btnPaste.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.PasteOption_Click);
            // 
            // btnCut
            // 
            this.btnCut.Label = "Cut";
            this.btnCut.Name = "btnCut";
            this.btnCut.OfficeImageId = "Cut";
            this.btnCut.ShowImage = true;
            this.btnCut.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.PasteOption_Click);
            // 
            // btnCopy
            // 
            this.btnCopy.Label = "Copy";
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.OfficeImageId = "Copy";
            this.btnCopy.ShowImage = true;
            this.btnCopy.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.PasteOption_Click);
            // 
            // btnUndo
            // 
            this.btnUndo.Enabled = false;
            this.btnUndo.Label = "button1";
            this.btnUndo.Name = "btnUndo";
            this.btnUndo.OfficeImageId = "Undo";
            this.btnUndo.ShowImage = true;
            this.btnUndo.ShowLabel = false;
            this.btnUndo.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.Undo_Click);
            // 
            // btnRedo
            // 
            this.btnRedo.Enabled = false;
            this.btnRedo.Label = "button2";
            this.btnRedo.Name = "btnRedo";
            this.btnRedo.OfficeImageId = "Redo";
            this.btnRedo.ShowImage = true;
            this.btnRedo.ShowLabel = false;
            this.btnRedo.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.Redo_Click);
            // 
            // toggleBtnBold
            // 
            this.toggleBtnBold.Label = "toggleButton1";
            this.toggleBtnBold.Name = "toggleBtnBold";
            this.toggleBtnBold.OfficeImageId = "Bold";
            this.toggleBtnBold.ShowImage = true;
            this.toggleBtnBold.ShowLabel = false;
            this.toggleBtnBold.SuperTip = "Bold";
            this.toggleBtnBold.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.BasicText_Click);
            // 
            // toggleBtnItalic
            // 
            this.toggleBtnItalic.Label = "toggleButton1";
            this.toggleBtnItalic.Name = "toggleBtnItalic";
            this.toggleBtnItalic.OfficeImageId = "Italic";
            this.toggleBtnItalic.ShowImage = true;
            this.toggleBtnItalic.ShowLabel = false;
            this.toggleBtnItalic.SuperTip = "Italic";
            this.toggleBtnItalic.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.BasicText_Click);
            // 
            // toggleBtnUnderline
            // 
            this.toggleBtnUnderline.Label = "toggleButton2";
            this.toggleBtnUnderline.Name = "toggleBtnUnderline";
            this.toggleBtnUnderline.OfficeImageId = "Underline";
            this.toggleBtnUnderline.ShowImage = true;
            this.toggleBtnUnderline.ShowLabel = false;
            this.toggleBtnUnderline.SuperTip = "Underline";
            this.toggleBtnUnderline.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.BasicText_Click);
            // 
            // toggleBtnStrikeThrough
            // 
            this.toggleBtnStrikeThrough.Label = "toggleButton1";
            this.toggleBtnStrikeThrough.Name = "toggleBtnStrikeThrough";
            this.toggleBtnStrikeThrough.OfficeImageId = "Strikethrough";
            this.toggleBtnStrikeThrough.ShowImage = true;
            this.toggleBtnStrikeThrough.ShowLabel = false;
            this.toggleBtnStrikeThrough.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.BasicText_Click);
            // 
            // toggleBtnAlignLeft
            // 
            this.toggleBtnAlignLeft.Label = "toggleButton3";
            this.toggleBtnAlignLeft.Name = "toggleBtnAlignLeft";
            this.toggleBtnAlignLeft.OfficeImageId = "AlignLeft";
            this.toggleBtnAlignLeft.ShowImage = true;
            this.toggleBtnAlignLeft.ShowLabel = false;
            this.toggleBtnAlignLeft.SuperTip = "Align left";
            this.toggleBtnAlignLeft.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.BasicText_Click);
            // 
            // toggleBtnAlignCenter
            // 
            this.toggleBtnAlignCenter.Label = "toggleButton4";
            this.toggleBtnAlignCenter.Name = "toggleBtnAlignCenter";
            this.toggleBtnAlignCenter.OfficeImageId = "AlignCenter";
            this.toggleBtnAlignCenter.ShowImage = true;
            this.toggleBtnAlignCenter.ShowLabel = false;
            this.toggleBtnAlignCenter.SuperTip = "Align Center";
            this.toggleBtnAlignCenter.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.BasicText_Click);
            // 
            // toggleBtnAlignRight
            // 
            this.toggleBtnAlignRight.Label = "toggleButton5";
            this.toggleBtnAlignRight.Name = "toggleBtnAlignRight";
            this.toggleBtnAlignRight.OfficeImageId = "AlignRight";
            this.toggleBtnAlignRight.ShowImage = true;
            this.toggleBtnAlignRight.ShowLabel = false;
            this.toggleBtnAlignRight.SuperTip = "Align Right";
            this.toggleBtnAlignRight.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.BasicText_Click);
            // 
            // toggleButtonBullets
            // 
            this.toggleButtonBullets.Label = "toggleButton2";
            this.toggleButtonBullets.Name = "toggleButtonBullets";
            this.toggleButtonBullets.OfficeImageId = "Bullets";
            this.toggleButtonBullets.ShowImage = true;
            this.toggleButtonBullets.ShowLabel = false;
            this.toggleButtonBullets.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.BasicText_Click);
            // 
            // toggleButtonNumbering
            // 
            this.toggleButtonNumbering.Label = "toggleButton1";
            this.toggleButtonNumbering.Name = "toggleButtonNumbering";
            this.toggleButtonNumbering.OfficeImageId = "Numbering";
            this.toggleButtonNumbering.ShowImage = true;
            this.toggleButtonNumbering.ShowLabel = false;
            this.toggleButtonNumbering.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.BasicText_Click);
            // 
            // btnDecreaseIndent
            // 
            this.btnDecreaseIndent.Enabled = false;
            this.btnDecreaseIndent.Label = "button1";
            this.btnDecreaseIndent.Name = "btnDecreaseIndent";
            this.btnDecreaseIndent.OfficeImageId = "IndentDecreaseWord";
            this.btnDecreaseIndent.ShowImage = true;
            this.btnDecreaseIndent.ShowLabel = false;
            this.btnDecreaseIndent.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.ParagraphIndent_Click);
            // 
            // btnIncreaseIndent
            // 
            this.btnIncreaseIndent.Enabled = false;
            this.btnIncreaseIndent.Label = "button2";
            this.btnIncreaseIndent.Name = "btnIncreaseIndent";
            this.btnIncreaseIndent.OfficeImageId = "IndentIncreaseWord";
            this.btnIncreaseIndent.ShowImage = true;
            this.btnIncreaseIndent.ShowLabel = false;
            this.btnIncreaseIndent.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.ParagraphIndent_Click);
            // 
            // splitBtnSpelling
            // 
            this.splitBtnSpelling.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.splitBtnSpelling.Items.Add(this.btnSpellingAndGrammar);
            this.splitBtnSpelling.Items.Add(this.btnResearch);
            this.splitBtnSpelling.Items.Add(this.btnThesaurus);
            this.splitBtnSpelling.Items.Add(this.TranslateGallery);
            this.splitBtnSpelling.Items.Add(this.btnSetLanguage);
            this.splitBtnSpelling.Items.Add(this.btnWordCount);
            this.splitBtnSpelling.Label = "Spelling";
            this.splitBtnSpelling.Name = "splitBtnSpelling";
            this.splitBtnSpelling.OfficeImageId = "SpellingMenu";
            this.splitBtnSpelling.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.SpellingGrammar_Check);
            // 
            // btnSpellingAndGrammar
            // 
            this.btnSpellingAndGrammar.Label = "Spelling & Grammar";
            this.btnSpellingAndGrammar.Name = "btnSpellingAndGrammar";
            this.btnSpellingAndGrammar.OfficeImageId = "SpellingAndGrammar";
            this.btnSpellingAndGrammar.ShowImage = true;
            this.btnSpellingAndGrammar.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.SpellingGrammar_Check);
            // 
            // btnResearch
            // 
            this.btnResearch.Label = "Research";
            this.btnResearch.Name = "btnResearch";
            this.btnResearch.OfficeImageId = "ResearchPane";
            this.btnResearch.ShowImage = true;
            this.btnResearch.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.Research_Click);
            // 
            // btnThesaurus
            // 
            this.btnThesaurus.Label = "Thesaurus";
            this.btnThesaurus.Name = "btnThesaurus";
            this.btnThesaurus.OfficeImageId = "Thesaurus";
            this.btnThesaurus.ShowImage = true;
            this.btnThesaurus.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.Thesaurus_Click);
            // 
            // TranslateGallery
            // 
            this.TranslateGallery.Buttons.Add(this.btnChooseTranslationLanguage);
            this.TranslateGallery.ColumnCount = 1;
            this.TranslateGallery.ItemImageSize = new System.Drawing.Size(75, 50);
            ribbonDropDownItemImpl1.Label = "Translate Document [English (U.S.) to Arabic] ";
            ribbonDropDownItemImpl1.OfficeImageId = "TranslateDocument";
            ribbonDropDownItemImpl1.Tag = "Show a machine tanslation in a web browser.";
            ribbonDropDownItemImpl2.Label = "Translate Selected text";
            ribbonDropDownItemImpl2.OfficeImageId = "TranslateSelected";
            ribbonDropDownItemImpl2.Tag = "Show a translation from local and online services in the Reasearch Pane.";
            ribbonDropDownItemImpl3.Label = "Mini Translator [Catalan]";
            ribbonDropDownItemImpl3.OfficeImageId = "MiniTranslator";
            ribbonDropDownItemImpl3.Tag = "Point to a word or select a phrase to view a quick translation.";
            this.TranslateGallery.Items.Add(ribbonDropDownItemImpl1);
            this.TranslateGallery.Items.Add(ribbonDropDownItemImpl2);
            this.TranslateGallery.Items.Add(ribbonDropDownItemImpl3);
            this.TranslateGallery.Label = "Translate";
            this.TranslateGallery.Name = "TranslateGallery";
            this.TranslateGallery.OfficeImageId = "Translate";
            this.TranslateGallery.ShowImage = true;
            this.TranslateGallery.ButtonClick += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.TranslateLanguage_Options);
            this.TranslateGallery.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.Translate_Gallery);
            // 
            // btnChooseTranslationLanguage
            // 
            this.btnChooseTranslationLanguage.Label = "Choose a Translation Language...";
            this.btnChooseTranslationLanguage.Name = "btnChooseTranslationLanguage";
            this.btnChooseTranslationLanguage.ShowImage = true;
            // 
            // btnSetLanguage
            // 
            this.btnSetLanguage.Label = "Set Proofing Language";
            this.btnSetLanguage.Name = "btnSetLanguage";
            this.btnSetLanguage.OfficeImageId = "SetLanguage";
            this.btnSetLanguage.ShowImage = true;
            this.btnSetLanguage.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.SetProofing_Language);
            // 
            // btnWordCount
            // 
            this.btnWordCount.Label = "Word Count";
            this.btnWordCount.Name = "btnWordCount";
            this.btnWordCount.OfficeImageId = "WordCount";
            this.btnWordCount.ShowImage = true;
            this.btnWordCount.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.Word_Count);
            // 
            // QuickStylesgallery
            // 
            this.QuickStylesgallery.ColumnCount = 3;
            this.QuickStylesgallery.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            ribbonDropDownItemImpl4.Image = ((System.Drawing.Image)(resources.GetObject("ribbonDropDownItemImpl4.Image")));
            ribbonDropDownItemImpl4.Label = "Normal";
            ribbonDropDownItemImpl4.OfficeImageId = "StyleNormal";
            ribbonDropDownItemImpl4.ScreenTip = "Normal";
            ribbonDropDownItemImpl5.Image = ((System.Drawing.Image)(resources.GetObject("ribbonDropDownItemImpl5.Image")));
            ribbonDropDownItemImpl5.Label = "Heading1";
            ribbonDropDownItemImpl6.Image = ((System.Drawing.Image)(resources.GetObject("ribbonDropDownItemImpl6.Image")));
            ribbonDropDownItemImpl6.Label = "Heading2";
            ribbonDropDownItemImpl7.Image = ((System.Drawing.Image)(resources.GetObject("ribbonDropDownItemImpl7.Image")));
            ribbonDropDownItemImpl7.Label = "Heading3";
            ribbonDropDownItemImpl8.Image = ((System.Drawing.Image)(resources.GetObject("ribbonDropDownItemImpl8.Image")));
            ribbonDropDownItemImpl8.Label = "Heading4";
            ribbonDropDownItemImpl9.Image = ((System.Drawing.Image)(resources.GetObject("ribbonDropDownItemImpl9.Image")));
            ribbonDropDownItemImpl9.Label = "Heading5";
            this.QuickStylesgallery.Items.Add(ribbonDropDownItemImpl4);
            this.QuickStylesgallery.Items.Add(ribbonDropDownItemImpl5);
            this.QuickStylesgallery.Items.Add(ribbonDropDownItemImpl6);
            this.QuickStylesgallery.Items.Add(ribbonDropDownItemImpl7);
            this.QuickStylesgallery.Items.Add(ribbonDropDownItemImpl8);
            this.QuickStylesgallery.Items.Add(ribbonDropDownItemImpl9);
            this.QuickStylesgallery.Label = "Quick Styles";
            this.QuickStylesgallery.Name = "QuickStylesgallery";
            this.QuickStylesgallery.OfficeImageId = "QuickStylesGallery";
            this.QuickStylesgallery.RowCount = 1;
            this.QuickStylesgallery.ShowImage = true;
            this.QuickStylesgallery.ShowItemLabel = false;
            this.QuickStylesgallery.ShowItemSelection = true;
            this.QuickStylesgallery.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.Gallery_Styles);
            // 
            // btnInsertText
            // 
            this.btnInsertText.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnInsertText.Label = "Insert Text";
            this.btnInsertText.Name = "btnInsertText";
            this.btnInsertText.OfficeImageId = "TextBoxInsert";
            this.btnInsertText.ShowImage = true;
            this.btnInsertText.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.Insert_Text);
            // 
            // splitBtnPicture
            // 
            this.splitBtnPicture.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.splitBtnPicture.Items.Add(this.btnGetPicturefromLocal);
            this.splitBtnPicture.Items.Add(this.btnGetPicturefromServer);
            this.splitBtnPicture.Label = "Picture";
            this.splitBtnPicture.Name = "splitBtnPicture";
            this.splitBtnPicture.OfficeImageId = "PictureInsertMenu";
            this.splitBtnPicture.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.Picture_Insert);
            // 
            // btnGetPicturefromLocal
            // 
            this.btnGetPicturefromLocal.Label = "Get Picture from Local ";
            this.btnGetPicturefromLocal.Name = "btnGetPicturefromLocal";
            this.btnGetPicturefromLocal.ShowImage = true;
            this.btnGetPicturefromLocal.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.Picture_Insert);
            // 
            // btnGetPicturefromServer
            // 
            this.btnGetPicturefromServer.Label = "Get Picture from Server";
            this.btnGetPicturefromServer.Name = "btnGetPicturefromServer";
            this.btnGetPicturefromServer.ShowImage = true;
            // 
            // splitBtnTable
            // 
            this.splitBtnTable.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.splitBtnTable.Items.Add(this.btnInsertTable);
            this.splitBtnTable.Items.Add(this.toggleButton1);
            this.splitBtnTable.Items.Add(this.btnConvertTextToTable);
            this.splitBtnTable.Items.Add(this.btnExcelSpreadSheet);
            this.splitBtnTable.Items.Add(this.galleryQuickTables);
            this.splitBtnTable.Label = "Table";
            this.splitBtnTable.Name = "splitBtnTable";
            this.splitBtnTable.OfficeImageId = "InsertTable";
            this.splitBtnTable.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.InsertTable_Click);
            // 
            // btnInsertTable
            // 
            this.btnInsertTable.Label = "Insert Table";
            this.btnInsertTable.Name = "btnInsertTable";
            this.btnInsertTable.OfficeImageId = "InsertTable";
            this.btnInsertTable.ShowImage = true;
            this.btnInsertTable.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.InsertTable_Click);
            // 
            // toggleButton1
            // 
            this.toggleButton1.Label = "Draw Table";
            this.toggleButton1.Name = "toggleButton1";
            this.toggleButton1.OfficeImageId = "TableDrawTable";
            this.toggleButton1.ShowImage = true;
            this.toggleButton1.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.DrawTable_Click);
            // 
            // btnConvertTextToTable
            // 
            this.btnConvertTextToTable.Enabled = false;
            this.btnConvertTextToTable.Label = "Convert Text to Table";
            this.btnConvertTextToTable.Name = "btnConvertTextToTable";
            this.btnConvertTextToTable.OfficeImageId = "ConvertTextToTable";
            this.btnConvertTextToTable.ShowImage = true;
            this.btnConvertTextToTable.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.ConvertTextToTable_Click);
            // 
            // btnExcelSpreadSheet
            // 
            this.btnExcelSpreadSheet.Label = "Excel Spreadsheet";
            this.btnExcelSpreadSheet.Name = "btnExcelSpreadSheet";
            this.btnExcelSpreadSheet.OfficeImageId = "TableExcelSpreadsheetInsert";
            this.btnExcelSpreadSheet.ShowImage = true;
            this.btnExcelSpreadSheet.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.ExcelSpreadSheet_Click);
            // 
            // galleryQuickTables
            // 
            ribbonDropDownItemImpl10.Label = "Table with Grid 1";
            ribbonDropDownItemImpl11.Label = "Table with Grid 2";
            ribbonDropDownItemImpl12.Label = "Table with Grid 3";
            ribbonDropDownItemImpl13.Label = "Table with Grid 8";
            this.galleryQuickTables.Items.Add(ribbonDropDownItemImpl10);
            this.galleryQuickTables.Items.Add(ribbonDropDownItemImpl11);
            this.galleryQuickTables.Items.Add(ribbonDropDownItemImpl12);
            this.galleryQuickTables.Items.Add(ribbonDropDownItemImpl13);
            this.galleryQuickTables.Label = "QuickTables";
            this.galleryQuickTables.Name = "galleryQuickTables";
            this.galleryQuickTables.OfficeImageId = "QuickTablesInsertGallery";
            this.galleryQuickTables.ShowImage = true;
            this.galleryQuickTables.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.TableWithGrid_Click);
            // 
            // splitBtnVideo
            // 
            this.splitBtnVideo.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.splitBtnVideo.Items.Add(this.btnGetVideoFromLocalDrive);
            this.splitBtnVideo.Items.Add(this.btnGetVideoFromServer);
            this.splitBtnVideo.Label = "Video";
            this.splitBtnVideo.Name = "splitBtnVideo";
            this.splitBtnVideo.OfficeImageId = "PictureInsertMenu";
            this.splitBtnVideo.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.splitBtnVideo_Click);
            // 
            // btnGetVideoFromLocalDrive
            // 
            this.btnGetVideoFromLocalDrive.Label = "Get Video from the Local Drive";
            this.btnGetVideoFromLocalDrive.Name = "btnGetVideoFromLocalDrive";
            this.btnGetVideoFromLocalDrive.ShowImage = true;
            this.btnGetVideoFromLocalDrive.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnGetVideoFromLocalDrive_Click);
            // 
            // btnGetVideoFromServer
            // 
            this.btnGetVideoFromServer.Label = "GetVideoFromServer";
            this.btnGetVideoFromServer.Name = "btnGetVideoFromServer";
            this.btnGetVideoFromServer.ShowImage = true;
            // 
            // glyAbbrevations
            // 
            this.glyAbbrevations.ColumnCount = 1;
            this.glyAbbrevations.Label = "Abbrevations";
            this.glyAbbrevations.Name = "glyAbbrevations";
            // 
            // glyCites
            // 
            this.glyCites.ColumnCount = 1;
            this.glyCites.Label = "Cites";
            this.glyCites.Name = "glyCites";
            // 
            // glyLinks
            // 
            this.glyLinks.ColumnCount = 1;
            this.glyLinks.Label = "Links";
            this.glyLinks.Name = "glyLinks";
            // 
            // glyIdioms
            // 
            this.glyIdioms.ColumnCount = 1;
            this.glyIdioms.Label = "Idioms";
            this.glyIdioms.Name = "glyIdioms";
            // 
            // glyVariables
            // 
            this.glyVariables.ColumnCount = 1;
            this.glyVariables.Label = "Variables";
            this.glyVariables.Name = "glyVariables";
            // 
            // glyDefinitions
            // 
            this.glyDefinitions.ColumnCount = 1;
            this.glyDefinitions.Label = "Definitions";
            this.glyDefinitions.Name = "glyDefinitions";
            // 
            // glyIndex
            // 
            this.glyIndex.ColumnCount = 1;
            this.glyIndex.Label = "Index";
            this.glyIndex.Name = "glyIndex";
            // 
            // btnComments
            // 
            this.btnComments.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnComments.Image = global::TEXXTOOR.Properties.Resources.Commentare;
            this.btnComments.Label = "Comments";
            this.btnComments.Name = "btnComments";
            this.btnComments.ShowImage = true;
            this.btnComments.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.Comments_Click);
            // 
            // menuType
            // 
            this.menuType.Dynamic = true;
            this.menuType.Label = "Type";
            this.menuType.Name = "menuType";
            // 
            // btnInternalLink
            // 
            this.btnInternalLink.Label = "Internal Link";
            this.btnInternalLink.Name = "btnInternalLink";
            this.btnInternalLink.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.Internal_Link);
            // 
            // btnRemoveLink
            // 
            this.btnRemoveLink.Label = "Remove Link";
            this.btnRemoveLink.Name = "btnRemoveLink";
            // 
            // splitBtnCode
            // 
            this.splitBtnCode.Label = "Code";
            this.splitBtnCode.Name = "splitBtnCode";
            // 
            // splitBtnFile
            // 
            this.splitBtnFile.Label = "File";
            this.splitBtnFile.Name = "splitBtnFile";
            // 
            // splitBtnKeystroke
            // 
            this.splitBtnKeystroke.Label = "Keystroke";
            this.splitBtnKeystroke.Name = "splitBtnKeystroke";
            // 
            // TexxtoorRibbon
            // 
            this.Name = "TexxtoorRibbon";
            this.RibbonType = "Microsoft.Word.Document";
            this.Tabs.Add(this.tabTEXXTOOR);
            this.Tabs.Add(this.tabInsert);
            this.Tabs.Add(this.tabSemantics);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.TexxtoorRibbon_Load);
            this.tabTEXXTOOR.ResumeLayout(false);
            this.tabTEXXTOOR.PerformLayout();
            this.grpTextoorText.ResumeLayout(false);
            this.grpTextoorText.PerformLayout();
            this.grpClipboard.ResumeLayout(false);
            this.grpClipboard.PerformLayout();
            this.buttonGroup1.ResumeLayout(false);
            this.buttonGroup1.PerformLayout();
            this.grpBasicText.ResumeLayout(false);
            this.grpBasicText.PerformLayout();
            this.btnGrpFormat.ResumeLayout(false);
            this.btnGrpFormat.PerformLayout();
            this.btnGrpAlignment.ResumeLayout(false);
            this.btnGrpAlignment.PerformLayout();
            this.buttonGroup2.ResumeLayout(false);
            this.buttonGroup2.PerformLayout();
            this.buttonGroup3.ResumeLayout(false);
            this.buttonGroup3.PerformLayout();
            this.grpProofing.ResumeLayout(false);
            this.grpProofing.PerformLayout();
            this.groupStyles.ResumeLayout(false);
            this.groupStyles.PerformLayout();
            this.tabInsert.ResumeLayout(false);
            this.tabInsert.PerformLayout();
            this.groupInsertText.ResumeLayout(false);
            this.groupInsertText.PerformLayout();
            this.groupInsertFigure.ResumeLayout(false);
            this.groupInsertFigure.PerformLayout();
            this.groupInsertTable.ResumeLayout(false);
            this.groupInsertTable.PerformLayout();
            this.tabSemantics.ResumeLayout(false);
            this.tabSemantics.PerformLayout();
            this.grpSemanticElements.ResumeLayout(false);
            this.grpSemanticElements.PerformLayout();
            this.grpSocialInteraction.ResumeLayout(false);
            this.grpSocialInteraction.PerformLayout();
            this.grpSideBar.ResumeLayout(false);
            this.grpSideBar.PerformLayout();
            this.grpLinks.ResumeLayout(false);
            this.grpLinks.PerformLayout();
            this.grpSemanticText.ResumeLayout(false);
            this.grpSemanticText.PerformLayout();
            this.groupInsertVideo.ResumeLayout(false);
            this.groupInsertVideo.PerformLayout();

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab tabTEXXTOOR;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup grpTextoorText;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnHomePage;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnOpenExisting;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnManageProject;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup grpClipboard;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnCut;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnCopy;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup grpBasicText;
        internal Microsoft.Office.Tools.Ribbon.RibbonButtonGroup btnGrpFormat;
        internal Microsoft.Office.Tools.Ribbon.RibbonButtonGroup btnGrpAlignment;
        internal Microsoft.Office.Tools.Ribbon.RibbonSplitButton splitbtnPublish;
        internal Microsoft.Office.Tools.Ribbon.RibbonSeparator separator1;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup grpProofing;
        internal Microsoft.Office.Tools.Ribbon.RibbonSplitButton splitBtnSpelling;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup groupStyles;
        private Microsoft.Office.Tools.Ribbon.RibbonTab tabInsert;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup groupInsertText;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnInsertText;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup groupInsertFigure;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup groupInsertTable;
        private Microsoft.Office.Tools.Ribbon.RibbonTab tabSemantics;
        internal Microsoft.Office.Tools.Ribbon.RibbonSplitButton splitBtnPicture;
        internal Microsoft.Office.Tools.Ribbon.RibbonSplitButton splitBtnTable;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup grpSemanticElements;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup grpSocialInteraction;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup grpSideBar;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup grpLinks;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup grpSemanticText;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnPublish;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnPublishAsDraft;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnSpellingAndGrammar;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnResearch;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnThesaurus;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnSetLanguage;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnWordCount;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnComments;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnInternalLink;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnRemoveLink;
        internal Microsoft.Office.Tools.Ribbon.RibbonSplitButton splitBtnCode;
        internal Microsoft.Office.Tools.Ribbon.RibbonSplitButton splitBtnFile;
        internal Microsoft.Office.Tools.Ribbon.RibbonSplitButton splitBtnKeystroke;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnInsertTable;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnConvertTextToTable;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnGetPicturefromLocal;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnGetPicturefromServer;
        internal Microsoft.Office.Tools.Ribbon.RibbonToggleButton toggleBtnBold;
        internal Microsoft.Office.Tools.Ribbon.RibbonToggleButton toggleBtnItalic;
        internal Microsoft.Office.Tools.Ribbon.RibbonToggleButton toggleBtnUnderline;
        internal Microsoft.Office.Tools.Ribbon.RibbonToggleButton toggleBtnAlignLeft;
        internal Microsoft.Office.Tools.Ribbon.RibbonToggleButton toggleBtnAlignCenter;
        internal Microsoft.Office.Tools.Ribbon.RibbonToggleButton toggleBtnAlignRight;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnPaste;
        internal Microsoft.Office.Tools.Ribbon.RibbonToggleButton toggleBtnStrikeThrough;
        internal Microsoft.Office.Tools.Ribbon.RibbonGallery galleryQuickTables;
        public Microsoft.Office.Tools.Ribbon.RibbonGallery QuickStylesgallery;
        internal Microsoft.Office.Tools.Ribbon.RibbonToggleButton toggleButtonBullets;
        internal Microsoft.Office.Tools.Ribbon.RibbonToggleButton toggleButtonNumbering;
        internal Microsoft.Office.Tools.Ribbon.RibbonSeparator separator2;
        internal Microsoft.Office.Tools.Ribbon.RibbonButtonGroup buttonGroup1;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnRedo;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnExcelSpreadSheet;
        internal Microsoft.Office.Tools.Ribbon.RibbonGallery TranslateGallery;
        private Microsoft.Office.Tools.Ribbon.RibbonButton btnChooseTranslationLanguage;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnUndo;
        internal Microsoft.Office.Tools.Ribbon.RibbonToggleButton toggleButton1;
        internal Microsoft.Office.Tools.Ribbon.RibbonButtonGroup buttonGroup2;
        internal Microsoft.Office.Tools.Ribbon.RibbonButtonGroup buttonGroup3;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnDecreaseIndent;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnIncreaseIndent;
        internal Microsoft.Office.Tools.Ribbon.RibbonMenu menuType;
        internal Microsoft.Office.Tools.Ribbon.RibbonGallery glyAbbrevations;
        internal Microsoft.Office.Tools.Ribbon.RibbonGallery glyCites;
        internal Microsoft.Office.Tools.Ribbon.RibbonGallery glyLinks;
        internal Microsoft.Office.Tools.Ribbon.RibbonGallery glyIdioms;
        internal Microsoft.Office.Tools.Ribbon.RibbonGallery glyVariables;
        internal Microsoft.Office.Tools.Ribbon.RibbonGallery glyDefinitions;
        internal Microsoft.Office.Tools.Ribbon.RibbonGallery glyIndex;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup groupInsertVideo;
        internal Microsoft.Office.Tools.Ribbon.RibbonSplitButton splitBtnVideo;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnGetVideoFromLocalDrive;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnGetVideoFromServer;
    }

    partial class ThisRibbonCollection
    {
        internal TexxtoorRibbon TexxtoorRibbon
        {
            get { return this.GetRibbon<TexxtoorRibbon>(); }
        }
    }
}
