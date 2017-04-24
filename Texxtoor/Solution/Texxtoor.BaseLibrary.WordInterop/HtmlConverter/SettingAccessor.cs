/***************************************************************************

Copyright (c) Microsoft Corporation 2008.

This code is licensed using the Microsoft Public License (Ms-PL).  The text of the license can be found here:

http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx

***************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;

namespace OpenXml.PowerTools.Wordprocessing
{
    /// <summary>
    /// Provides access to setting operations
    /// </summary>
    public class SettingAccessor
    {
        private PTWordprocessingDocument parentDocument;

        private static XNamespace ns;
        private static XNamespace settingsns;
        private static XNamespace relationshipns;

        static SettingAccessor()
        {
            ns = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
            settingsns = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/settings";
            relationshipns = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="document">Document to perform operations on</param>
        public SettingAccessor(PTWordprocessingDocument document)
        {
            this.parentDocument = document;
        }

        /// <summary>
        /// Nodes list with displayBackgroundShape elements
        /// </summary>
        public XElement DisplayBackgroundShapeElements()
        {
            XDocument settingsDocument = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart.DocumentSettingsPart);
            return settingsDocument.Descendants(settingsns + "displayBackgroundShape").FirstOrDefault();
        }

        /// <summary>
        /// Adds a displayBackgroundShape element to the settings file
        /// </summary>
        public void AddBackgroundShapeElement()
        {
            XDocument settingsDocument = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart.DocumentSettingsPart);
            settingsDocument.Root.Add(
                new XElement(ns + "displayBackgroundShape")
            );
        }

        /// <summary>
        /// Adds a the evenAndOddHeaders element, which allows to define distinct headers and footers for odd and even pages
        /// </summary>
        public void AddEvendAndOddHeadersElement()
        {
            XDocument settingsDocument = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart.DocumentSettingsPart);
            if (settingsDocument.Descendants(ns + "evenAndOddHeaders").FirstOrDefault() == null)
                settingsDocument.Root.Add(
                    new XElement(ns + "evenAndOddHeaders"));
        }

        /// <summary>
        /// Adds the documentProtection element to a document
        /// </summary>
        public void AddDocumentProtectionElement()
        {
            //Finds the settings part
            XDocument settingsDocument;
            XElement documentProtectionElement = null;
            if (parentDocument.Document.MainDocumentPart.DocumentSettingsPart == null)
            {
                //If settings part does not exist creates a new one
                DocumentSettingsPart settingsPart = parentDocument.Document.MainDocumentPart.AddNewPart<DocumentSettingsPart>();
                settingsDocument = parentDocument.GetXDocument(settingsPart);
                settingsDocument.Add(CreateEmptySettings().Root);
            }
            else
            {
                //If the settings part does exist looks if documentProtection has been included
                settingsDocument = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart.DocumentSettingsPart);
                documentProtectionElement = settingsDocument.Element(ns + "settings").Element(ns + "documentProtection");
            }

            //Creates the documentProtection element, or edits it if it exists
            if (documentProtectionElement == null)
            {
                settingsDocument
                    .Element(ns + "settings")
                    .Add(
                        new XElement(ns + "documentProtection",
                            new XAttribute(ns + "edit", "readOnly")
                        )
                    );
            }
            else
                documentProtectionElement.SetAttributeValue(ns + "edit", "readOnly");
        }
        /// <summary>
        /// Creates an empty base structure for a settings part
        /// </summary>
        /// <returns></returns>
        private static XDocument CreateEmptySettings()
        {
            XDocument document =
                new XDocument(
                    new XElement(ns + "settings",
                        new XAttribute(XNamespace.Xmlns + "w", ns),
                        new XAttribute(XNamespace.Xmlns + "r", relationshipns)
                    )
                );
            return document;
        }
    }
}
