/***************************************************************************

Copyright (c) Microsoft Corporation 2008.

This code is licensed using the Microsoft Public License (Ms-PL).  The text of the license can be found here:

http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx

***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using OpenXmlSDK = DocumentFormat.OpenXml.Packaging;
using System.IO.Packaging;

namespace OpenXml.PowerTools
{
    /// <summary>
    /// Base class for OpenXml document format types
    /// </summary>
    public enum OpenXmlDocumentType
    {
        /// <summary>
        /// SpreadsheetML
        /// </summary>
        SpreadsheetML,
        /// <summary>
        /// WordprocessingML
        /// </summary>
        WordprocessingML,
        /// <summary>
        /// PresentationML
        /// </summary>
        PresentationML
    }

    /// <summary>
    /// Format to export comment contents
    /// </summary>
    public enum CommentFormat
    {
        /// <summary>
        /// PlainText
        /// </summary>
        PlainText,
        /// <summary>
        /// Xml
        /// </summary>
        Xml,
        /// <summary>
        /// Docx
        /// </summary>
        Docx
    }

    /// <summary>
    /// Specify the OpenXmlDocument class
    /// </summary>
    public abstract class OpenXmlDocument
    {
        /*
         * XDocument functionality has been moved to ProcessingExtensions.cs as extension functions to OpenXmlSDK classes.
         */

        /// <summary>
        /// Package to perform operations on
        /// </summary>
        public OpenXmlSDK.OpenXmlPackage Document { get; set; }
        /// <summary>
        /// Full file name of document
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Digital signature manager
        /// </summary>
        /// <remarks>
        /// As Digital Signatures are a feature of Open Packaging Convention rather than OpenXml standard, 
        /// Digital Signature access is performed on a general class, not in particular classes
        /// </remarks>
        public DigitalSignatureAccessor DigitalSignatures { get; set; }

        /// <summary>
        /// InnerContent
        /// </summary>
        //  the same goes for the innercontent of each document type (wordprosessing and spreadsheet)
        public DocumentManager InnerContent { get; set; }

        /// <summary>
        /// OpenXmlDocument (static)
        /// </summary>
        // Static "factory" creation functions
        public static OpenXmlDocument FromFile(string filename, FileAccess access)
        {
            Package package = Package.Open(filename, FileMode.Open, access);
            return FromPackage(package, filename);
        }

        /// <summary>
        /// FromPackage (static)
        /// </summary>
        public static OpenXmlDocument FromPackage(OpenXmlSDK.OpenXmlPackage package)
        {
            return FromPackage(package.Package, null);
        }

        /// <summary>
        /// FromPackage (static)
        /// </summary>
        public static OpenXmlDocument FromPackage(Package package, string fullName)
        {
            OpenXmlDocumentType type = GetDocumentType(package);
            switch (type)
            {
                case OpenXmlDocumentType.WordprocessingML:
                    return new PTWordprocessingDocument(OpenXmlSDK.WordprocessingDocument.Open(package), fullName);
                case OpenXmlDocumentType.SpreadsheetML:
                    return new SpreadsheetDocument(OpenXmlSDK.SpreadsheetDocument.Open(package), fullName);
                case OpenXmlDocumentType.PresentationML:
                    //return new PresentationDocument(OpenXmlSDK.PresentationDocument.Open(package));
                    return null;
            }
            return null;
        }

        // Public operations, will be overridden if specific to a document type
        // If operation is invalid on some documents, these functions will throw an exception (InvalidOperation)
        /// <summary>
        /// AcceptAllChanges
        /// </summary>
        public virtual void AcceptAllChanges()
        {
            throw new InvalidOperationException("Document does not support tracking of changes.");
        }
        /// <summary>
        /// AcceptAllChanges
        /// </summary>
        public virtual void RejectAllChanges()
        {
            throw new InvalidOperationException("Document does not support tracking of changes.");
        }
        /// <summary>
        /// AcceptAllChanges
        /// </summary>
        public virtual void SetBackgroundColor(string bgColor)
        {
            throw new InvalidOperationException("Document does not support a background color.");
        }
        /// <summary>
        /// AcceptAllChanges
        /// </summary>
        public virtual void SetBackgroundImage(string bgImagePath)
        {
            throw new InvalidOperationException("Document does not support a background image.");
        }
        /// <summary>
        /// AcceptAllChanges
        /// </summary>
        public virtual string GetBackgroundColor()
        {
            throw new InvalidOperationException("Document does not support a background color.");
        }
        /// <summary>
        /// AcceptAllChanges
        /// </summary>
        public virtual string GetBackgroundImage()
        {
            throw new InvalidOperationException("Document does not support a background image.");
        }
        /// <summary>
        /// AcceptAllChanges
        /// </summary>
        public virtual object GetAllComments(CommentFormat format)
        {
            throw new InvalidOperationException("Document does not support comments.");
        }
        /// <summary>
        /// AcceptAllChanges
        /// </summary>
        public virtual XDocument FindCustomXml(string partName)
        {
            throw new InvalidOperationException("Document does not support custom XML data.");
        }
        /// <summary>
        /// AcceptAllChanges
        /// </summary>
        public virtual XDocument GetHeaders(Wordprocessing.HeaderType type)
        {
            throw new InvalidOperationException("Document does not support headers.");
        }
        /// <summary>
        /// AcceptAllChanges
        /// </summary>
        public virtual XDocument GetFooters(Wordprocessing.FooterType type)
        {
            throw new InvalidOperationException("Document does not support footers.");
        }
        /// <summary>
        /// AcceptAllChanges
        /// </summary>
        public virtual XDocument GetStylesDocument()
        {
            throw new InvalidOperationException("Document does not support styles.");
        }
        /// <summary>
        /// AcceptAllChanges
        /// </summary>
        public virtual Package GetTheme(string outputPath)
        {
            throw new InvalidOperationException("Document does not support themes.");
        }
        /// <summary>
        /// AcceptAllChanges
        /// </summary>
        public virtual string GetWatermarkText()
        {
            throw new InvalidOperationException("Document does not support watermarks.");
        }
        /// <summary>
        /// AcceptAllChanges
        /// </summary>
        public virtual void InsertFormat(string xpathInsertionPoint, string xmlContent)
        {
            throw new InvalidOperationException("Document does not support formatted content.");
        }
        /// <summary>
        /// AcceptAllChanges
        /// </summary>
        public virtual void InsertStyle(string xpathInsertionPoint, string styleName, XDocument stylesSource)
        {
            throw new InvalidOperationException("Document does not support formatted content.");
        }
        /// <summary>
        /// AcceptAllChanges
        /// </summary>
        public virtual void SetCustomXmlDocument(XDocument data, string partName)
        {
            throw new InvalidOperationException("Document does not support custom XML data.");
        }
        /// <summary>
        /// AcceptAllChanges
        /// </summary>
        public virtual void SetHeader(XDocument header, Wordprocessing.HeaderType type)
        {
            throw new InvalidOperationException("Document does not support headers.");
        }
        /// <summary>
        /// SetFooter
        /// </summary>
        public virtual void SetFooter(XDocument footer, Wordprocessing.FooterType type)
        {
            throw new InvalidOperationException("Document does not support footers.");
        }
        /// <summary>
        /// SetStylePart
        /// </summary>
        public virtual void SetStylePart(XDocument styleDocument)
        {
            throw new InvalidOperationException("Document does not support footers.");
        }

        /// <summary>
        /// AddTable
        /// </summary>
        public virtual void AddTable(string worksheetName, string tableStyle, bool useHeaders, short fromColum, short toColum, int fromRow, int toRow)
        {
            throw new InvalidOperationException("Document does not support table styles.");
        }

        /// <summary>
        /// SetTheme
        /// </summary>
        public virtual void SetTheme(Package themePackage)
        {
            throw new InvalidOperationException("Document does not support footers.");
        }
        /// <summary>
        /// InsertWatermark
        /// </summary>
        public virtual void InsertWatermark(string watermarkText, bool diagonalOrientation)
        {
            throw new InvalidOperationException("Document does not support footers.");
        }
        /// <summary>
        /// InsertPicture
        /// </summary>
        public virtual void InsertPicture(string insertionPoint, System.Drawing.Image image, string name)
        {
            throw new InvalidOperationException("Document does not support pictures.");
        }
        /// <summary>
        /// GenerateTOA
        /// </summary>
        public virtual void GenerateTOA(Wordprocessing.Position pos, string stylesSourcefile, bool addDefaultStyles)
        {
            throw new InvalidOperationException("Document does not support a Table of Authorities.");
        }
        /// <summary>
        /// GenerateTOC
        /// </summary>
        public virtual void GenerateTOC(string stylesSourcefile, bool addDefaultStyles)
        {
            throw new InvalidOperationException("Document does not support a Table of Contents.");
        }
        /// <summary>
        /// GenerateTOF
        /// </summary>
        public virtual void GenerateTOF(Wordprocessing.Position pos, string stylesSourcefile, bool addDefaultStyles)
        {
            throw new InvalidOperationException("Document does not support a Table of Figures.");
        }
        /// <summary>
        /// GenerateIndex
        /// </summary>
        public virtual void GenerateIndex(string stylesSourcefile, bool addDefaultStyles)
        {
            throw new InvalidOperationException("Document does not support an Index.");
        }
        /// <summary>
        /// JoinDocuments
        /// </summary>
        public virtual OpenXmlDocument JoinDocuments(IEnumerable<OpenXmlDocument> documents, string outputFilePath)
        {
            throw new InvalidOperationException("Joins not yet supported for this document.");
        }
        /// <summary>
        /// RemoveAllComments
        /// </summary>
        public virtual void RemoveAllComments()
        {
            throw new InvalidOperationException("Document does not support comments.");
        }

        /// <summary>
        /// Setting value for a spreadsheet cell
        /// <remarks>Author:Johann Granados Company: Staff DotNet Creation Date: 8/30/2008</remarks>
        /// </summary>
        public virtual void SetCellValue(string worksheetName, int fromRow, int toRow, short fromColumn, short toColumn, string value)
        {
            throw new InvalidOperationException("Document does not support this operation");
        }

        /// <summary>
        /// Setting style for a spreadsheet cell
        /// <remarks>Author:Johann Granados Company: Staff DotNet Creation Date: 8/30/2008</remarks>
        /// </summary>
        public virtual void SetCellStyle(string worksheetName, int fromRow, int toRow, short fromColumn, short toColumn, string cellStyle)
        {
            throw new InvalidOperationException("Document does not support this operation");
        }

        /// <summary>
        /// Setting width for a range of columns
        /// <remarks>Author:Johann Granados Company: Staff DotNet Creation Date: 8/30/2008</remarks>
        /// </summary>
        public virtual void SetColumnWidth(string worksheetName, short fromColumn, short tocolumn, int width)
        {
            throw new InvalidOperationException("Document does not support this operation");
        }
        /// <summary>
        /// Class constructor
        /// </summary>
        public OpenXmlDocument()
        {
            DigitalSignatures = new DigitalSignatureAccessor(this);
            InnerContent = new DocumentManager(this);

        }

        /// <summary>
        /// Loads an XDocument with contents of a given part
        /// </summary>
        /// <param name="part">Part to load contents from</param>
        /// <returns>XDocument with contents of part</returns>
        protected internal XDocument GetXDocument(OpenXmlSDK.OpenXmlPart part)
        {
            return part.GetXDocument();
        }

        /// <summary>
        /// Cleanly removes a part from a package
        /// </summary>
        /// <param name="part"></param>
        internal void RemovePart(OpenXmlSDK.OpenXmlPart part)
        {
            var parentParts = part.GetParentParts().ToList();
            foreach (var parentPart in parentParts)
                parentPart.DeletePart(part);
        }

        /// <summary>
        /// Updates package contents to reflect document alterations
        /// </summary>
        internal void FlushParts()
        {
            Document.FlushParts();
        }

        /// <summary>
        /// Closes properly the document
        /// </summary>
        public void Close()
        {
            Document.Close();
        }

        #region Miscellaneous (XmlDoc to XDoc, XDoc to XmlDoc, SavePartAs)

        internal static OpenXmlDocumentType GetDocumentType(Package package)
        {
            PackageRelationship relationship = package.GetRelationshipsByType("http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument").FirstOrDefault();

            if (relationship == null)
                throw new InvalidOperationException("Package is not an OpenXml Document.");

            PackagePart part = package.GetPart(PackUriHelper.ResolvePartUri(relationship.SourceUri, relationship.TargetUri));
            switch (part.ContentType)
            {
                case "application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml":
                    return OpenXmlDocumentType.WordprocessingML;
                case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml":
                    return OpenXmlDocumentType.SpreadsheetML;
                case "application/vnd.openxmlformats-officedocument.presentationml.presentation.main+xml":
                    return OpenXmlDocumentType.PresentationML;
                default:
                    throw new InvalidOperationException("Package is not an OpenXml Document.");
            }
        }

        /// <summary>
        /// Saves the contents of an XDocument object into a XmlDocument object
        /// </summary>
        /// <param name="xDocument">Source document</param>
        /// <returns>Destination document</returns>
        internal static XmlDocument LoadXmlDocumentFromXDocument(XDocument xDocument)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(xDocument.CreateReader());
            return xmlDocument;
        }

        /// <summary>
        /// Saves the contents of an XmlDocument object into a XDocument object
        /// </summary>
        /// <param name="xmlDocument">Source document</param>
        /// <param name="xDocument">Destination document</param>
        internal static void SaveXmlDocumentIntoXDocument(XmlDocument xmlDocument, XDocument xDocument)
        {
            XDocument xNewDocument = new XDocument();
            using (XmlWriter xWriter = xNewDocument.CreateWriter())
                xmlDocument.WriteTo(xWriter);
            xDocument.Root.ReplaceWith(xNewDocument.Root);
        }

        /// <summary>
        /// Writes the contents of a part into a file
        /// </summary>
        /// <param name="part">OpenXml part to read contents from</param>
        /// <param name="filePath">Full path of file to write part contents to</param>
        internal static void SavePartAs(OpenXmlSDK.OpenXmlPart part, string filePath)
        {
            Stream partStream = part.GetStream(FileMode.Open, FileAccess.Read);
            byte[] partContent = new byte[partStream.Length];
            partStream.Read(partContent, 0, (int)partStream.Length);

            File.WriteAllBytes(filePath, partContent);
        }
        #endregion
    }
}
