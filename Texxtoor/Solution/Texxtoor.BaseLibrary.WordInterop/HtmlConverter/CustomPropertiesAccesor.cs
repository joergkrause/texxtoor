/***************************************************************************

Copyright (c) Microsoft Corporation 2008.

This code is licensed using the Microsoft Public License (Ms-PL).  The text of the license can be found here:

http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx

***************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml;
using OpenXmlSDK = DocumentFormat.OpenXml.Packaging;
using System.Collections.ObjectModel;
using System;

namespace OpenXml.PowerTools
{
    /// <summary>
    /// Provides access to the custom properties part
    /// </summary>
    public class CustomPropertiesAccesor
    {
        OpenXmlDocument parentDocument;

        private static XNamespace ns;
        private static XNamespace relationshipsns;
        private static XNamespace customPropertiesns;
        private static XNamespace vTypesns;

        static CustomPropertiesAccesor()
        {
            ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            relationshipsns = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
            customPropertiesns = "http://schemas.openxmlformats.org/officeDocument/2006/custom-properties";
            vTypesns = "http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes";
        }
        /// <summary>
        /// Class constructor  
        /// </summary>
        /// <param name="document">Document to perform operations on</param>
        public CustomPropertiesAccesor(OpenXmlDocument document)
        {
            parentDocument = document;
        }

        /// <summary>
        /// Creates a new custom properties parts
        /// </summary>
        public XDocument CreateCustomPropertiesPart()
        {
            //  create the custom properties part
            OpenXmlSDK.CustomFilePropertiesPart customPropertiesPart = null;
            OpenXmlDocumentType type = OpenXmlDocument.GetDocumentType(parentDocument.Document.Package);
            switch (type)
            {
                case OpenXmlDocumentType.WordprocessingML:
                    customPropertiesPart = ((OpenXmlSDK.WordprocessingDocument)parentDocument.Document).AddCustomFilePropertiesPart();
                    break;
                case OpenXmlDocumentType.SpreadsheetML:
                    customPropertiesPart = ((OpenXmlSDK.SpreadsheetDocument)parentDocument.Document).AddCustomFilePropertiesPart();
                    break;
                case OpenXmlDocumentType.PresentationML:
                    customPropertiesPart = ((OpenXmlSDK.PresentationDocument)parentDocument.Document).AddCustomFilePropertiesPart();
                    break;
            }
            XDocument customPropertiesXDocument = parentDocument.GetXDocument(customPropertiesPart);
            customPropertiesXDocument.Add(
                new XElement(customPropertiesns + "Properties",
                    new XAttribute(XNamespace.Xmlns + "c", customPropertiesns),
                    new XAttribute(XNamespace.Xmlns + "vt", vTypesns)));
            return customPropertiesXDocument;
        }

/*        /// <summary>
        /// Adds the documentProtection element to a document
        /// </summary>
        public void AddDocumentProtection()
        {
            //Finds the custom properties part
            XDocument customPropertiesDocument = null;
            XElement documentProtectionElement = null;

            if (parentDocument.Document.WorkbookPart.CustomPropertyPart == null)
            {
                //If settings part does not exist creates a new one
                customPropertiesDocument = CreateCustomPropertiesPart();
            }
            else
            {
                //If the custom properties part does exist, look if the document protection already exists
                customPropertiesDocument = parentDocument.GetXDocument(parentDocument.Document.WorkbookPart.CustomPropertyPart);
                XName propertyTagName = customPropertiesns + "property";
                XName propertyAttributeName = customPropertiesns + "name";
                XName propertyName = "_MarkAsFinal";
                documentProtectionElement = customPropertiesDocument.Descendants().Where(
                    tag =>
                        (tag.Name == propertyTagName) && (tag.Attribute(propertyAttributeName).Value == propertyName)
                    ).First();
            }

            //  create a document protection element if it doesn't exist
            if (documentProtectionElement == null)
            {
                customPropertiesDocument.Root.Add(
                    new XElement(customPropertiesns + "property",
                        new XAttribute("fmtid", "{D5CDD505-2E9C-101B-9397-08002B2CF9AE}"),
                        new XAttribute("pid", "456"),
                        new XAttribute("name", "_MarkAsFinal"),
                        new XElement(vTypesns + "bool", "true")));
            }
        }*/

        internal void RemoveAll()
        {
            IEnumerable<OpenXmlSDK.CustomFilePropertiesPart> customPropertiesParts = parentDocument.Document.GetPartsOfType<OpenXmlSDK.CustomFilePropertiesPart>();
            if (customPropertiesParts != null && customPropertiesParts.Count() > 0)
                parentDocument.RemovePart(customPropertiesParts.First());
        }
    }
}