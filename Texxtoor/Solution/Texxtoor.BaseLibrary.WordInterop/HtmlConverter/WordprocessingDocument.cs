/***************************************************************************

Copyright (c) Microsoft Corporation 2008.

This code is licensed using the Microsoft Public License (Ms-PL).  The text of the license can be found here:

http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx

***************************************************************************/

using OpenXmlSDK = DocumentFormat.OpenXml.Packaging;
using OpenXml.PowerTools.Wordprocessing;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Globalization;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Xml.Linq;


namespace OpenXml.PowerTools
{
    /// <summary>
    /// Represents a Wordprocessing Document and exposes functionality to manipulate content
    /// </summary>
    public class PTWordprocessingDocument : OpenXmlDocument
    {
        //public WordprocessingDocumentManager InnerContent { get; set; }
        /// <summary>
        /// Comments get/set
        /// </summary>
        public CommentAccessor Comments { get; set; }
        /// <summary>
        /// Changes get/set
        /// </summary>
        public ChangeAccessor Changes { get; set; }
        /// <summary>
        /// Headers get/set
        /// </summary>
        public HeaderAccessor Headers { get; set; }
        /// <summary>
        /// Footer get/set
        /// </summary>
        public FooterAccessor Footer { get; set; }
        /// <summary>
        /// Setting get/set
        /// </summary>
        public SettingAccessor Setting { get; set; }
        /// <summary>
        /// CustomXml get/set
        /// </summary>
        public CustomXmlAccessor CustomXml { get; set; }
        /// <summary>
        /// Background get/set
        /// </summary>
        public BackgroundAccessor Background { get; set; }
        /// <summary>
        /// Style get/set
        /// </summary>
        public StyleAccessor Style { get; set; }
        /// <summary>
        /// Format get/set
        /// </summary>
        public ContentFormatAccessor Format { get; set; }
        /// <summary>
        /// Picture get/set
        /// </summary>
        public PictureAccessor Picture { get; set; }
        /// <summary>
        /// Watermark get/set
        /// </summary>
        public WatermarkAccesor Watermark { get; set; }
        /// <summary>
        /// Theme get/set
        /// </summary>
        public ThemeAccessor Theme { get; set; }
        /// <summary>
        /// TOC get/set
        /// </summary>
        public TOCAccessor TOC { get; set; }
        /// <summary>
        /// TOF get/set
        /// </summary>
        public TOFAccessor TOF { get; set; }
        /// <summary>
        /// TOA get/set
        /// </summary>
        public TOAAccessor TOA { get; set; }
        /// <summary>
        /// Index get/set
        /// </summary>
        public IndexAccessor Index { get; set; }

        /// <summary>
        /// CoreProperties get/set
        /// </summary>
        public CorePropertiesAccesor CoreProperties { get; set; }
        /// <summary>
        /// CustomProperties get/set
        /// </summary>
        public CustomPropertiesAccesor CustomProperties { get; set; }

        private MemoryStream memoryStream;

        /// <summary>
        /// OpenXml SDK Document object
        /// </summary>
        public new OpenXmlSDK.WordprocessingDocument Document
        {
            get { return (OpenXmlSDK.WordprocessingDocument)base.Document; }
            set { base.Document = value; }
        }

        /// <summary>
        /// InnerContent
        /// </summary>
        public new WordprocessingDocumentManager InnerContent
        {
            get { return (WordprocessingDocumentManager)base.InnerContent; }
            set { base.InnerContent = value; }
        }
        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="document">Document to perform operations on</param>
        /// <param name="fullName">Full path name of document</param>
        public PTWordprocessingDocument(OpenXmlSDK.WordprocessingDocument document, string fullName)
            : base()
        {
            Document = document;
            FullName = fullName;
            InnerContent = new WordprocessingDocumentManager(this);
            Comments = new CommentAccessor(this);
            Changes = new ChangeAccessor(this);
            Headers = new HeaderAccessor(this);
            Footer = new FooterAccessor(this);
            Setting = new SettingAccessor(this);
            CustomXml = new CustomXmlAccessor(this);
            Background = new BackgroundAccessor(this);
            Style = new StyleAccessor(this);
            Format = new ContentFormatAccessor(this);
            Picture = new PictureAccessor(this);
            Watermark = new WatermarkAccesor(this);
            Theme = new ThemeAccessor(this);
            TOC = new TOCAccessor(this);
            TOF = new TOFAccessor(this);
            TOA = new TOAAccessor(this);
            Index = new IndexAccessor(this);
            CoreProperties = new CorePropertiesAccesor(this);
            CustomProperties = new CustomPropertiesAccesor(this);
        }

        /// <summary>
        /// WordprocessingDocument (Constructor)
        /// </summary>
        public PTWordprocessingDocument()
            : base()
        {
            memoryStream = new MemoryStream();
            Document = OpenXmlSDK.WordprocessingDocument.Create(memoryStream, DocumentFormat.OpenXml.WordprocessingDocumentType.Document);
            InnerContent = new WordprocessingDocumentManager(this);
            Comments = new CommentAccessor(this);
            Changes = new ChangeAccessor(this);
            Headers = new HeaderAccessor(this);
            Footer = new FooterAccessor(this);
            Setting = new SettingAccessor(this);
            CustomXml = new CustomXmlAccessor(this);
            Background = new BackgroundAccessor(this);
            Style = new StyleAccessor(this);
            Format = new ContentFormatAccessor(this);
            Picture = new PictureAccessor(this);
            Watermark = new WatermarkAccesor(this);
            Theme = new ThemeAccessor(this);
            TOC = new TOCAccessor(this);
            TOF = new TOFAccessor(this);
        }

        // Public operations that are overridden for the Word Processing document type
        /// <summary>
        /// AcceptAllChanges
        /// </summary>
        public override void AcceptAllChanges()
        {
            Changes.AcceptAll();
        }
        /// <summary>
        /// RejectAllChanges
        /// </summary>
        public override void RejectAllChanges()
        {
            Changes.RejectAll();
        }
        /// <summary>
        /// SetBackgroundColor
        /// </summary>
        public override void SetBackgroundColor(string bgColor)
        {
            Background.SetColor(bgColor);
        }
        /// <summary>
        /// SetBackgroundImage
        /// </summary>
        public override void SetBackgroundImage(string bgImagePath)
        {
            Background.SetImage(bgImagePath);
        }
        /// <summary>
        /// GetBackgroundColor
        /// </summary>
        public override string GetBackgroundColor()
        {
            return Background.GetColor();
        }
        /// <summary>
        /// GetBackgroundImage
        /// </summary>
        public override string GetBackgroundImage()
        {
            return Background.GetImage();
        }
        /// <summary>
        /// GetAllComments
        /// </summary>
        public override object GetAllComments(CommentFormat format)
        {
            return Comments.GetAll(format);
        }
        /// <summary>
        /// FindCustomXml
        /// </summary>
        public override XDocument FindCustomXml(string partName)
        {
            return CustomXml.Find(partName);
        }
        /// <summary>
        /// GetHeaders
        /// </summary>
        public override XDocument GetHeaders(HeaderType type)
        {
            return Headers.GetHeader(type);
        }
        /// <summary>
        /// GetFooters
        /// </summary>
        public override XDocument GetFooters(FooterType type)
        {
            return Footer.GetFooter(type);
        }
        /// <summary>
        /// GetStylesDocument
        /// </summary>
        public override XDocument GetStylesDocument()
        {
            return Style.GetStylesDocument();
        }
        /// <summary>
        /// GetTheme
        /// </summary>
        public override Package GetTheme(string outputPath)
        {
            return Theme.GetTheme(outputPath);
        }
        /// <summary>
        /// GetWatermarkText
        /// </summary>
        public override string GetWatermarkText()
        {
            return Watermark.GetWatermarkText();
        }
        /// <summary>
        /// InsertFormat
        /// </summary>
        public override void InsertFormat(string xpathInsertionPoint, string xmlContent)
        {
            Format.InsertFormat(this, xpathInsertionPoint, xmlContent);
        }
        /// <summary>
        /// InsertStyle
        /// </summary>
        public override void InsertStyle(string xpathInsertionPoint, string styleName, XDocument stylesSource)
        {
            Style.InsertStyle(xpathInsertionPoint, styleName, stylesSource);
        }
        /// <summary>
        /// SetCustomXmlDocument
        /// </summary>
        public override void SetCustomXmlDocument(XDocument data, string partName)
        {
            CustomXml.SetDocument(data, partName);
        }
        /// <summary>
        /// SetHeader
        /// </summary>
        public override void SetHeader(XDocument header, HeaderType type)
        {
            Headers.SetHeader(header, type);
        }
        /// <summary>
        /// SetFooter
        /// </summary>
        public override void SetFooter(XDocument footer, FooterType type)
        {
            Footer.SetFooter(footer, type);
        }
        /// <summary>
        /// SetStylePart
        /// </summary>
        public override void SetStylePart(XDocument styleDocument)
        {
            Style.SetStylePart(styleDocument);
        }
        /// <summary>
        /// SetTheme
        /// </summary>
        public override void SetTheme(Package themePackage)
        {
            Theme.SetTheme(themePackage);
        }
        /// <summary>
        /// InsertWatermark
        /// </summary>
        public override void InsertWatermark(string watermarkText, bool diagonalOrientation)
        {
            Watermark.InsertWatermark(watermarkText, diagonalOrientation);
        }
        /// <summary>
        /// InsertPicture
        /// </summary>
        public override void InsertPicture(string insertionPoint, System.Drawing.Image image, string name)
        {
            Picture.Insert(insertionPoint, image, name);
        }
        /// <summary>
        /// GenerateTOA
        /// </summary>
        public override void GenerateTOA(Wordprocessing.Position pos, string stylesSourceFile, bool addDefaultStyles)
        {
            TOA.Generate(pos);
            Style.CreateTOAStyles(stylesSourceFile, addDefaultStyles);
        }
        /// <summary>
        /// GenerateTOC
        /// </summary>
        public override void GenerateTOC(string stylesSourceFile, bool addDefaultStyles)
        {
            TOC.Generate();
            Style.CreateTOCStyles(stylesSourceFile, addDefaultStyles);
        }
        /// <summary>
        /// GenerateTOF
        /// </summary>
        public override void GenerateTOF(Wordprocessing.Position pos, string stylesSourceFile, bool addDefaultStyles)
        {
            TOF.Generate(pos);
            Style.CreateTOFStyles(stylesSourceFile, addDefaultStyles);
        }
        /// <summary>
        /// GenerateIndex
        /// </summary>
        public override void GenerateIndex(string stylesSourceFile, bool addDefaultStyles)
        {
            Index.Generate();
            Style.CreateIndexStyles(stylesSourceFile, addDefaultStyles);
        }
        /// <summary>
        /// JoinDocuments
        /// </summary>
        public override OpenXmlDocument JoinDocuments(IEnumerable<OpenXmlDocument> documents, string outputFilePath)
        {
            return WordprocessingDocumentManager.JoinDocuments(documents, outputFilePath);
        }
        /// <summary>
        /// RemoveAllComments
        /// </summary>
        public override void RemoveAllComments()
        {
            Comments.RemoveAll();
        }

        /// <summary>
        /// SaveAs
        /// </summary>
        public void SaveAs(string path)
        {
            Document.Package.Flush();
            FlushParts();
            using (BinaryWriter w = new BinaryWriter(new FileStream(@path, FileMode.Create)))
            {
                w.Write(memoryStream.GetBuffer());
                w.Close();
            }
        }
        
        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="filePath">Path of document to load</param>
        public PTWordprocessingDocument(string filePath) : this(filePath, false) { }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="filePath">Path of document to load</param>
        /// <param name="createNew">Whether create a new document or load contents from existing one</param>
        public PTWordprocessingDocument(string filePath, bool createNew)
            : base()
        {
            try
            {
                if (createNew)
                    Document = OpenXmlSDK.WordprocessingDocument.Create(filePath, DocumentFormat.OpenXml.WordprocessingDocumentType.Document);
                else
                    Document = OpenXmlSDK.WordprocessingDocument.Open(filePath, true);
                FullName = filePath;

                InnerContent = new WordprocessingDocumentManager(this);
                Comments = new CommentAccessor(this);
                Changes = new ChangeAccessor(this);
                Headers = new HeaderAccessor(this);
                Footer = new FooterAccessor(this);
                Setting = new SettingAccessor(this);
                CustomXml = new CustomXmlAccessor(this);
                Background = new BackgroundAccessor(this);
                Style = new StyleAccessor(this);
                Format = new ContentFormatAccessor(this);
                Picture = new PictureAccessor(this);
                Watermark = new WatermarkAccesor(this);
                Theme = new ThemeAccessor(this);
                TOC = new TOCAccessor(this);
                TOF = new TOFAccessor(this);
                TOA = new TOAAccessor(this);
                Index = new IndexAccessor(this);
            }
            catch (OpenXmlSDK.OpenXmlPackageException ope)
            {
                throw new Exception("Bad formed OpenXml package", ope);
            }
            catch (FileFormatException ffe) {
                throw new Exception("File contains corrupted data", ffe);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
