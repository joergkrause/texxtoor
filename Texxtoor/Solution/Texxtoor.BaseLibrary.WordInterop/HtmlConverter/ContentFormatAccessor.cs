/***************************************************************************

Copyright (c) Microsoft Corporation 2008.

This code is licensed using the Microsoft Public License (Ms-PL).  The text of the license can be found here:

http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx

***************************************************************************/

using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using DocumentFormat.OpenXml.Packaging;
using System;

namespace OpenXml.PowerTools.Wordprocessing
{
    /// <summary>
    /// Provides access to content format operations
    /// </summary>
    public class ContentFormatAccessor
    {
        private PTWordprocessingDocument parentDocument;
        private static XNamespace ns;

        static ContentFormatAccessor()
        {
            ns = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="document">Document to perform operations on</param>
        public ContentFormatAccessor(PTWordprocessingDocument document)
        {
            parentDocument = document;
        }

        /// <summary>
        /// Inserts Xml markup representing format attributes inside a specific paragraph or paragraph run
        /// </summary>
        /// <param name="document">Document to insert formatting Xml tags</param>
        /// <param name="xpathInsertionPoint">Paragraph or paragraph run to set format</param>
        /// <param name="content">Formatting tags</param>
        public void InsertFormat(PTWordprocessingDocument document, string xpathInsertionPoint, string content)
        {
            XDocument xDocument = parentDocument.GetXDocument(document.Document.MainDocumentPart);
            XmlDocument xmlMainDocument = OpenXmlDocument.LoadXmlDocumentFromXDocument(xDocument);

            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");

            XmlNodeList insertionPoints = xmlMainDocument.SelectNodes(xpathInsertionPoint, namespaceManager);

            if (insertionPoints.Count == 0)
                throw new Exception("The xpath query did not return a valid location.");

            foreach (XmlNode insertionPoint in insertionPoints)
            {
                XmlNode propertiesElement = insertionPoint.SelectSingleNode(@"w:pPr|w:rPr", namespaceManager);
                if (insertionPoint.Name == "w:p")
                {
                    // Checks if the rPr or pPr element exists
                    if (propertiesElement == null)
                    {
                        propertiesElement = xmlMainDocument.CreateElement("w", "pPr", namespaceManager.LookupNamespace("w"));
                        insertionPoint.PrependChild(propertiesElement);
                    }
                }
                else if (insertionPoint.Name == "w:r")
                {
                    // Checks if the rPr or pPr element exists
                    if (propertiesElement == null)
                    {
                        propertiesElement = xmlMainDocument.CreateElement("w", "rPr", namespaceManager.LookupNamespace("w"));
                        insertionPoint.PrependChild(propertiesElement);
                    }
                }

                if (propertiesElement != null)
                {
                    propertiesElement.InnerXml += content;
                }
                else
                {
                    throw new Exception("Specified xpath query result is not a valid location to place a formatting markup");
                }

            }
            OpenXmlDocument.SaveXmlDocumentIntoXDocument(xmlMainDocument, xDocument);

        }
    }
}
