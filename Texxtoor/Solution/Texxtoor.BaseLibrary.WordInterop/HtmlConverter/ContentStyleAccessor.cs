/***************************************************************************

Copyright (c) Microsoft Corporation 2008.

This code is licensed using the Microsoft Public License (Ms-PL).  The text of the license can be found here:

http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx

***************************************************************************/

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace OpenXml.PowerTools.Wordprocessing
{
    /// <summary>
    /// Provides access to content style operations
    /// </summary>
    public class ContentStyleAccessor
    {
        private PTWordprocessingDocument parentDocument;
        private static XNamespace ns;
        private const string newStyleNameSuffix = "_1";

        static ContentStyleAccessor()
        {
            ns = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="document">Document to perform operations on</param>
        public ContentStyleAccessor(PTWordprocessingDocument document)
        {
            parentDocument = document;
        }

        /// <summary>
        /// Gets the tree representing all style hierarchy for a given style
        /// </summary>
        /// <param name="styleName">Name of style</param>
        /// <param name="stylesFile">Styles library</param>
        /// <returns>Style tree</returns>
        private Collection<XElement> GetStyleHierarchy(string styleName, string stylesFile)
        {
            Collection<XElement> stylesCollection = new Collection<XElement>();
            XDocument xml = new XDocument();
            xml = XDocument.Load(stylesFile);
            GetStyleDefinition(styleName, xml, stylesCollection);

            return stylesCollection;
        }

        /// <summary>
        /// Gets the style definition for the document
        /// </summary>
        /// <param name="styleName">Style name</param>
        /// <param name="xmlStyleDefinitions">Style library</param>
        /// <param name="stylesCollection">Styles</param>
        public void GetStyleDefinition(string styleName, XDocument xmlStyleDefinitions, Collection<XElement> stylesCollection)
        {
            XName style = ns + "style";
            XName styleId = ns + "styleId";

            //  create a copy of the xmlstyleDefinition variable so the 
            //  original xml don't be altered
            XElement actualStyle = 
                new XElement(
                    xmlStyleDefinitions
                    .Descendants()
                    .Where(
                        tag =>
                            (tag.Name == style) && (tag.Attribute(styleId).Value == styleName)
                    )
                    .ToList()
                    .FirstOrDefault()
                 );

            if (actualStyle != null)
            {
                //  look in the stylesCollection if the style has already been added
                IEnumerable<XElement> insertedStyles =
                    stylesCollection.Where
                    (
                        tag =>
                            (tag.Name == style) && (tag.Attribute(styleId).Value == styleName)
                    );

                //  if the style has not been inserted
                if (!(insertedStyles.Count() > 0))
                {
                    stylesCollection.Add(actualStyle);
                    GetStyleDefinition(getLinkStyleId(actualStyle), xmlStyleDefinitions, stylesCollection);
                    GetStyleDefinition(getNextStyleId(actualStyle), xmlStyleDefinitions, stylesCollection);
                    GetStyleDefinition(getBasedOnStyleId(actualStyle), xmlStyleDefinitions, stylesCollection);

                }
                //  change the name of the style, so there would be no conflict 
                //  with the original styles definition
                actualStyle.Attribute(styleId).Value = actualStyle.Attribute(styleId).Value + newStyleNameSuffix;
            }
        }

        /// <summary>
        /// Gets the name of the linked style associated to the given style
        /// </summary>
        /// <param name="xmlStyle">Style to find link</param>
        /// <returns>Linked style name</returns>
        public string getLinkStyleId(XElement xmlStyle)
        {
            XName val = ns + "val";
            string linkStyleId = "";
            XElement linkStyle = xmlStyle.Descendants(ns + "link").FirstOrDefault();
            if (linkStyle != null)
            {
                linkStyleId = linkStyle.Attribute(val).Value;
                //  change the name of the attribute, because the new added style is being renamed
                linkStyle.Attribute(val).Value = linkStyle.Attribute(val).Value + newStyleNameSuffix;
            }
            return linkStyleId;
        }

        /// <summary>
        /// Gets the name of the style tagged as 'next' associated to the given style
        /// </summary>
        /// <param name="xmlStyle">Style to find 'next' element</param>
        /// <returns>Name of style tagged as 'next</returns>
        public string getNextStyleId(XElement xmlStyle)
        {
            XName val = ns + "val";
            string nextStyleId = "";
            XElement nextStyle = xmlStyle.Descendants(ns + "next").FirstOrDefault();
            if (nextStyle != null)
            {
                nextStyleId = nextStyle.Attribute(val).Value;
                //  change the name of the attribute, because the new added style is being renamed
                nextStyle.Attribute(val).Value = nextStyle.Attribute(val).Value + newStyleNameSuffix;
            }
            return nextStyleId;
        }

        /// <summary>
        /// Get the name of the style tagged as 'basedOn' associated to the given style
        /// </summary>
        /// <param name="xmlStyle">Style to find 'basedOn' element</param>
        /// <returns>Name of style tagged as 'basedOn'</returns>
        public string getBasedOnStyleId(XElement xmlStyle)
        {
            XName val = ns + "val";
            string basedOnStyleId = "";
            XElement basedOnStyle = xmlStyle.Descendants(ns + "basedOn").FirstOrDefault();
            if (basedOnStyle != null)
            {
                basedOnStyleId = basedOnStyle.Attribute(val).Value;
                //  change the name of the attribute, because the new added style is being renamed
                basedOnStyle.Attribute(val).Value = basedOnStyle.Attribute(val).Value + newStyleNameSuffix;
            }
            return basedOnStyleId;
        }

        /// <summary>
        /// Sets the style to a given location inside the document part
        /// </summary>
        /// <param name="xpathInsertionPoint">Document fragment to set style</param>
        /// <param name="styleValue">Name of style</param>
        /// <param name="stylesSourceFilePath">Styles library</param>
        public void InsertStyle(string xpathInsertionPoint, string styleValue, string stylesSourceFilePath)
        {
            XDocument xDocument = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart);
            XmlDocument xmlMainDocument = OpenXmlDocument.LoadXmlDocumentFromXDocument(xDocument);

            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(xmlMainDocument.NameTable);
            namespaceManager.AddNamespace("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");

            XmlNodeList insertionPoints = xmlMainDocument.SelectNodes(xpathInsertionPoint, namespaceManager);


            foreach (XmlNode insertionPoint in insertionPoints)
            {
                XmlElement xmlStyle = null;

                XmlNode propertiesElement = insertionPoint.SelectSingleNode(@"w:pPr|w:rPr", namespaceManager);
                if (insertionPoint.Name == "w:p")
                {
                    xmlStyle = xmlMainDocument.CreateElement("w", "pStyle", namespaceManager.LookupNamespace("w"));
                    xmlStyle.SetAttribute("val", namespaceManager.LookupNamespace("w"), styleValue + newStyleNameSuffix);

                    //  check if the rPr or pPr element exist, if so, then add the style xml element
                    //  inside, if not, then add a new rPr or pPr element
                    if (propertiesElement != null)
                    {
                        //  check if there is already a style node and remove it
                        XmlNodeList xmlStyleList = propertiesElement.SelectNodes("w:pStyle", namespaceManager);
                        for (int i = 0; i < xmlStyleList.Count; i++)
                        {
                            propertiesElement.RemoveChild(xmlStyleList[i]);
                        }
                        propertiesElement.PrependChild(xmlStyle);
                    }
                    else
                    {
                        propertiesElement = xmlMainDocument.CreateElement("w", "pPr", namespaceManager.LookupNamespace("w"));
                        propertiesElement.PrependChild(xmlStyle);
                        insertionPoint.PrependChild(propertiesElement);
                    }
                }

                if (insertionPoint.Name == "w:r")
                {
                    xmlStyle = xmlMainDocument.CreateElement("w", "rStyle", namespaceManager.LookupNamespace("w"));
                    xmlStyle.SetAttribute("val", namespaceManager.LookupNamespace("w"), styleValue + newStyleNameSuffix);
                    if (propertiesElement != null)
                    {
                        // check if there is already a style node and remove it
                        XmlNodeList xmlStyleList = propertiesElement.SelectNodes("w:rStyle", namespaceManager);
                        for (int i = 0; i < xmlStyleList.Count; i++)
                        {
                            propertiesElement.RemoveChild(xmlStyleList[i]);
                        }
                        propertiesElement.PrependChild(xmlStyle);
                    }
                    else
                    {
                        propertiesElement = xmlMainDocument.CreateElement("w", "rPr", namespaceManager.LookupNamespace("w"));
                        propertiesElement.PrependChild(xmlStyle);
                        insertionPoint.PrependChild(propertiesElement);
                    }
                }
            }

            OpenXmlDocument.SaveXmlDocumentIntoXDocument(xmlMainDocument, xDocument);

            //  Adds the style definition in the styles definitions part. The style definition need to be
            //  extracted from the given styles library file.
            Collection<XElement> styleHierarchy = GetStyleHierarchy(styleValue, stylesSourceFilePath);

            //  Opens the styles file in the document
            XDocument xmlStylesDefinitionDocument = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart.StyleDefinitionsPart);
            XDocument xElem = new XDocument();
            xElem.Add(xmlStylesDefinitionDocument.Root);
            
            //Inserts the new style
            foreach (XElement xmlStyleDefinition in styleHierarchy)
            {
                xElem.Root.Add(xmlStyleDefinition);
            }
            xmlStylesDefinitionDocument.Root.ReplaceWith(xElem.Root);
        }
    }
}