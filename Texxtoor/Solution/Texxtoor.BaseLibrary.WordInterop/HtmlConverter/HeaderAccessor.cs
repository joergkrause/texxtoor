/***************************************************************************

Copyright (c) Microsoft Corporation 2008.

This code is licensed using the Microsoft Public License (Ms-PL).  The text of the license can be found here:

http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx

***************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml;
using DocumentFormat.OpenXml.Packaging;
using System.Collections.ObjectModel;
using System;

namespace OpenXml.PowerTools.Wordprocessing
{
    /// <summary>
    /// Available kinds of header
    /// </summary>
    public enum HeaderType
    {
        /// <summary>
        /// First
        /// </summary>
        First,
        /// <summary>
        /// Even
        /// </summary>
        Even,
        /// <summary>
        /// Default
        /// </summary>
        Default
    }

    /// <summary>
    /// Provides access to header operations
    /// </summary>
    public class HeaderAccessor
    {
        private PTWordprocessingDocument parentDocument;

        private const string defaultHeaderType = "default";
        private static XNamespace ns;
        private static XNamespace relationshipns;
        private static XNamespace officens;
        private static XNamespace vmlns;
        private static XNamespace wordns;

        static HeaderAccessor() {
            ns = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";        
            relationshipns =  "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
            officens = "urn:schemas-microsoft-com:office:office";
            vmlns = "urn:schemas-microsoft-com:vml";
            wordns = "urn:schemas-microsoft-com:office:word";
        }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="document">Document to perform operations on</param>
        public HeaderAccessor(PTWordprocessingDocument document)
        {
			parentDocument = document;
		}

        /// <summary>
        /// Elements tagged as section properties
        /// </summary>
        /// <returns>IEnumerable&lt;XElement&gt; containing all the section properties elements found it in the document</returns>
        private IEnumerable<XElement> SectionPropertiesElements()
        {
            XDocument mainDocument = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart);
            IEnumerable<XElement> results =
                mainDocument
                .Descendants(ns + "p")
                .Elements(ns + "pPr")
                .Elements(ns + "sectPr");
            if (results.Count() == 0)
                results = mainDocument.Root.Elements(ns + "body").Elements(ns + "sectPr");
            return results;
        }

        /// <summary>
        /// Adds a new header reference in the section properties
        /// </summary>
        /// <param name="type">The header part type</param>
        /// <param name="headerPartId">the header part id</param>
        public void AddHeaderReference(HeaderType type, string headerPartId)
        {
            //  If the document does not have a property section a new one must be created
            if (SectionPropertiesElements().Count() == 0)
            {
                AddDefaultSectionProperties();
            }

            string typeName = "";
            switch ((HeaderType)type)
            {
                case HeaderType.First:
                    typeName = "first";
                    break;
                case HeaderType.Even:
                    typeName = "even";
                    break;
                case HeaderType.Default:
                    typeName = "default";
                    break;
            }

            XElement sectionPropertyElement = SectionPropertiesElements().First();
            sectionPropertyElement.Add(
                new XElement(ns + "headerReference",
                    new XAttribute(ns + "type", typeName),
                    new XAttribute(relationshipns + "id", headerPartId)));

            if (sectionPropertyElement.Element(ns + "titlePg") == null)
                sectionPropertyElement.Add(
                    new XElement(ns + "titlePg")
                    );
        }

        /// <summary>
        /// Adds a new header part in the document
        /// </summary>
        /// <param name="type">The footer part type</param>
        /// <returns>A XDocument contaning the added header</returns>
        public XDocument AddNewHeader(HeaderType type)
        {
            // Creates the new header part
            HeaderPart newHeaderPart = parentDocument.Document.MainDocumentPart.AddNewPart<HeaderPart>();

            XDocument headerContent = parentDocument.GetXDocument(newHeaderPart);
            XDocument emptyHeader = CreateEmptyHeaderDocument();
            if (headerContent.Root == null)
            {
                headerContent.Add(emptyHeader.Root);
            }
            else
                headerContent.Root.ReplaceWith(emptyHeader.Root);

            string newHeaderPartId = parentDocument.Document.MainDocumentPart.GetIdOfPart(newHeaderPart);
            AddHeaderReference(type, newHeaderPartId);

            return headerContent;
        }

        /// <summary>
        /// Creates an empty header document
        /// </summary>
        /// <returns>A XDocument containing the xml of an empty header part </returns>
        private static XDocument CreateEmptyHeaderDocument()
        {
            return new XDocument(
                new XElement(ns + "hdr",
                    new XAttribute(XNamespace.Xmlns + "w", ns),
                    new XAttribute(XNamespace.Xmlns + "r", relationshipns),
                    new XAttribute(XNamespace.Xmlns + "o", officens),
                    new XAttribute(XNamespace.Xmlns + "v", vmlns),
                    new XAttribute(XNamespace.Xmlns + "w10", wordns)
                )
            );
        }

        /// <summary>
        /// Header reference nodes inside the document
        /// </summary>
        /// <param name="type">The header part type</param>
        /// <returns>XElement containing the part reference in the document</returns>
        public XElement GetHeaderReference(HeaderType type)
        {
            XDocument mainDocument = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart);
            XName headerReferenceTag = ns + "headerReference";
            XName typeTag = ns + "type";
            string typeName = "";

            switch (type)
            {
                case HeaderType.First: typeName = "first";
                    break;
                case HeaderType.Even: typeName = "even";
                    break;
                case HeaderType.Default: typeName = "default";
                    break;
            }

            XElement headerReferenceElement = mainDocument.Descendants().Where(
                tag =>
                    (tag.Name == headerReferenceTag) && (tag.Attribute(typeTag).Value == typeName)
                ).FirstOrDefault();

            return headerReferenceElement;
        }

        /// <summary>
        /// Get the specified header from the document
        /// </summary>
        /// <param name="type">The header part type</param>
        /// <returns>A XDocument containing the header</returns>
        public XDocument GetHeader(HeaderType type)
        {
            OpenXmlPart header = GetHeaderPart(type);
            if (header != null)
                return parentDocument.GetXDocument(header);
            else
                return null;
        }


        /// <summary>
        /// The specified header part from the document
        /// </summary>
        /// <param name="type">The header part type</param>
        /// <returns>A OpenXmlPart containing the header part</returns>
        public OpenXmlPart GetHeaderPart(HeaderType type)
        {
            // look in the section properties of the main document part, the respective Header
            // needed to extract
            XElement headerReferenceElement = GetHeaderReference(type);
            if (headerReferenceElement != null)
            {
                //  get the relation id of the Header part to extract from the document
                string relationId = headerReferenceElement.Attribute(relationshipns + "id").Value;
                return parentDocument.Document.MainDocumentPart.GetPartById(relationId);
            }
            else
                return null;
        }

        /// <summary>
        /// Removes the specified header in the document
        /// </summary>
        /// <param name="type">The header part type</param>
        public void RemoveHeader(HeaderType type)
        {
            OpenXmlPart headerPart = GetHeaderPart(type);
            parentDocument.RemovePart(headerPart);
        }

        /// <summary>
        /// Set a new header in a document
        /// </summary>
        /// <param name="header">XDocument containing the header to add in the document</param>
        /// <param name="type">The header part type</param>
        public void SetHeader(XDocument header, HeaderType type)
        {
            //  Removes the reference in the document.xml and the header part if those already
            //  exist
            XElement headerReferenceElement = GetHeaderReference(type);
            if (headerReferenceElement != null)
            {
                RemoveHeader(type);
                headerReferenceElement.Remove();
            }

            //  Add the new header
            XDocument newHeader;
            HeaderPart headerPart = parentDocument.Document.MainDocumentPart.AddNewPart<HeaderPart>();
            newHeader = parentDocument.GetXDocument(headerPart);
            newHeader.Add(header.Root);

            //  Creates the relationship of the header inside the section properties in the document
            string relID = parentDocument.Document.MainDocumentPart.GetIdOfPart(headerPart);
            AddHeaderReference(type, relID);

            // add in the settings part the EvendAndOddHeaders. this element
            // allow to see the odd and even headers and headers in the document.
            parentDocument.Setting.AddEvendAndOddHeadersElement();
        }

        /// <summary>
        /// Adds a default sectPr element into the main document
        /// </summary>
        private void AddDefaultSectionProperties()
        {
            XDocument mainDocument = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart);
            mainDocument.Element(ns + "body").Add(
                new XElement(ns + "sectPr")
            );
        }
    }
}
