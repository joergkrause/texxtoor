/***************************************************************************

Copyright (c) Microsoft Corporation 2008.

This code is licensed using the Microsoft Public License (Ms-PL).  The text of the license can be found here:

http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx

***************************************************************************/

using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DocumentFormat.OpenXml.Packaging;

namespace OpenXml.PowerTools.Wordprocessing
{
    /// <summary>
    /// Available kinds of footer
    /// </summary>
    public enum FooterType
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
    /// Provides access to footer operations
    /// </summary>
    public class FooterAccessor
    {
        private PTWordprocessingDocument parentDocument;

        private static XNamespace ns;
        private static XNamespace relationshipns;

        static FooterAccessor()
        {
            ns = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
            relationshipns = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="document">Document to perform operations on</param>
        public FooterAccessor(PTWordprocessingDocument document)
        {
            this.parentDocument = document;
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
        /// Footer reference nodes inside the document
        /// </summary>
        /// <param name="type">The footer part type</param>
        /// <returns>XElement containing the part reference in the document</returns>
        private XElement GetFooterReference(FooterType type)
        {
            XDocument mainDocument = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart);
            XName footerReferenceTag = ns + "footerReference";
            XName typeTag = ns + "type";
            string typeName = "";

            switch (type)
            {
                case FooterType.First: typeName = "first";
                    break;
                case FooterType.Even: typeName = "even";
                    break;
                case FooterType.Default: typeName = "default";
                    break;
            }

            XElement footerReferenceElement = mainDocument.Descendants().Where(
                tag =>
                    (tag.Name == footerReferenceTag) && (tag.Attribute(typeTag).Value == typeName)
                ).FirstOrDefault();

            return footerReferenceElement;                    
        }

        /// <summary>
        /// Get the specified footer from the document
        /// </summary>
        /// <param name="type">The footer part type</param>
        /// <returns>the XDocument containing the footer</returns>
        public XDocument GetFooter(FooterType type)
        {
            OpenXmlPart footer = GetFooterPart(type);
            if (footer != null)
                return parentDocument.GetXDocument(footer);
            else
                return null;
        }


        /// <summary>
        /// The specified footer part from the document
        /// </summary>
        /// <param name="type">The footer part type</param>
        /// <returns>A OpenXmlPart containing the footer part</returns>
        public OpenXmlPart GetFooterPart(FooterType type)
        {
            // look in the section properties of the main document part, the respective footer
            // needed to extract
            XElement footerReferenceElement = GetFooterReference(type);
            if (footerReferenceElement != null)
            {
                //  get the relation id of the footer part to extract from the document
                string relationshipId = footerReferenceElement.Attribute(relationshipns + "id").Value;
                return parentDocument.Document.MainDocumentPart.GetPartById(relationshipId);
            }
            else
                return null;
        }

        /// <summary>
        /// Removes the specified footer in the document
        /// </summary>
        /// <param name="type">The footer part type</param>
        public void RemoveFooter(FooterType type)
        {
            OpenXmlPart footerPart = GetFooterPart(type);
            parentDocument.RemovePart(footerPart);
        }

        /// <summary>
        /// Set a new footer in a document
        /// </summary>
        /// <param name="footer">XDocument containing the footer to add in the document</param>
        /// <param name="type">The footer part type</param>
        public void SetFooter(XDocument footer, FooterType type)
        {
            //  Removes the reference in the document.xml and the footer part if those already
            //  exist
            XElement footerReferenceElement = GetFooterReference(type);
            if (footerReferenceElement != null)
            {
                RemoveFooter(type);
                footerReferenceElement.Remove();
            }

            //  Add the new footer
            XDocument newFooter;
            FooterPart footerPart = parentDocument.Document.MainDocumentPart.AddNewPart<FooterPart>();
            newFooter = parentDocument.GetXDocument(footerPart);
            newFooter.Add(footer.Root);

            //  If the document does not have a property section a new one must be created
            if (SectionPropertiesElements().Count() == 0)
            {
                AddDefaultSectionProperties();
            }

            //  Creates the relationship of the footer inside the section properties in the document
            string relID = parentDocument.Document.MainDocumentPart.GetIdOfPart(footerPart);
            string kindName = "";
            switch ((FooterType)type)
            {
                case FooterType.First:
                    kindName = "first";
                    break;
                case FooterType.Even:
                    kindName = "even";
                    break;
                case FooterType.Default:
                    kindName = "default";
                    break;
            }

            XElement sectionPropertyElement = SectionPropertiesElements().First();
            sectionPropertyElement.Add(
                new XElement(ns + "footerReference",
                    new XAttribute(ns + "type", kindName),
                    new XAttribute(relationshipns + "id", relID)));

            if (sectionPropertyElement.Element(ns + "titlePg") == null)
                    sectionPropertyElement.Add(
                        new XElement(ns + "titlePg")
                        );

            // add in the settings part the EvendAndOddHeaders. this element
            // allow to see the odd and even footers and headers in the document.
            parentDocument.Setting.AddEvendAndOddHeadersElement();
        }

        /// <summary>
        /// Adds a default sectPr element into the main document
        /// </summary>
        private void AddDefaultSectionProperties()
        {
            XDocument mainDocument = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart);
            mainDocument.Descendants(ns + "body").First().Add(
                new XElement(ns + "sectPr")
            );

        }
    }
}
