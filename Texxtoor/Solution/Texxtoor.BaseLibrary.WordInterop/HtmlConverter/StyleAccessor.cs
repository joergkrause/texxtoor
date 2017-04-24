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
    /// Provides access to style operations
    /// </summary>
    public class StyleAccessor
    {
        private PTWordprocessingDocument parentDocument;
        //private XDocument xmlStylesDefinitionDocument;
        private static XNamespace ns;
        /// <summary>
        /// newStyleNameSuffic variable
        /// </summary>
        public static readonly string newStyleNameSuffix = "_1";

        static StyleAccessor()
        {
            ns = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="document">Document to perform operations on</param>
        public StyleAccessor(PTWordprocessingDocument document)
        {
            parentDocument = document;
        }

        /// <summary>
        /// Gets the style hierarchy (link styles, next styles and basedOn styles) associated
        /// to the specified style, in a XElement collection
        /// </summary>
        /// <param name="styleName">Name of the style from where to get the hierarchy</param>
        /// <param name="stylesFile">File from where styles are taken </param>
        /// <param name="styleNameSuffix">Suffix of style name.</param>
        /// <returns>a collection containing the specified style and all the styles associated with it</returns>
        private Collection<XElement> GetStyleHierarchy(string styleName, XDocument stylesFile, string styleNameSuffix)
        {
            try
            {
                Collection<XElement> stylesCollection = new Collection<XElement>();
                GetStyleHierarchyProcess(styleName, stylesFile, stylesCollection);

                return stylesCollection;
            }
            catch (XmlException ex)
            {
                throw new Exception("File specified is not a valid XML file", ex);
            }

        }

        /// <summary>
        /// Gets the xml of a specific style definition
        /// </summary>
        /// <param name="styleName">Name of the style to get from the styles file</param>
        /// <param name="xmlStyleDefinitions">Style definitions</param>
        /// <param name="stylesCollection">Collection of styles</param>
        private void GetStyleHierarchyProcess(string styleName, XDocument xmlStyleDefinitions, Collection<XElement> stylesCollection)
        {
            XName style = ns + "style";
            XName styleId = ns + "styleId";

            //  the style name can come empty, because the given style could not have a link, basedOn or next style.
            //  In those cases the stylename will come empty
            if (styleName != "")
            {
                // Creates a copy of the xmlStyleDefinition variable so the original xml will not be altered
                XElement actualStyle = new XElement(xmlStyleDefinitions.Root);
                actualStyle = actualStyle.Descendants().Where
                    (
                        tag =>
                        (tag.Name == style) && (tag.Attribute(styleId).Value.ToUpper() == styleName.ToUpper())
                     ).ToList().FirstOrDefault();

                if (actualStyle != null)
                {
                    // Looks in the stylesCollection if the style has already been added
                    IEnumerable<XElement> insertedStyles =
                        stylesCollection.Where
                        (
                            tag =>
                                (tag.Name == style) && (tag.Attribute(styleId).Value.ToUpper() == styleName.ToUpper())
                        );

                    // If the style has not been inserted
                    if (insertedStyles.Count() == 0)
                    {
                        stylesCollection.Add(actualStyle);
                        GetStyleHierarchyProcess(GetLinkStyleId(actualStyle), xmlStyleDefinitions, stylesCollection);
                        GetStyleHierarchyProcess(GetNextStyleId(actualStyle), xmlStyleDefinitions, stylesCollection);
                        GetStyleHierarchyProcess(GetBasedOnStyleId(actualStyle), xmlStyleDefinitions, stylesCollection);

                    }
                    // Changes the name of the style, so there would be no conflict with the original styles definition
                    actualStyle.Attribute(styleId).Value = actualStyle.Attribute(styleId).Value + newStyleNameSuffix;
                }
                else
                    throw new Exception("Style or dependencies not found in the given style library.");
            }
        }

        /// <summary>
        /// Gets the name of the 'link' style associated to the given style
        /// </summary>
        /// <param name="xmlStyle">Xml to search for linked style</param>
        /// <returns>Name of the style</returns>
        private string GetLinkStyleId(XElement xmlStyle)
        {
            XName val = ns + "val";
            string linkStyleId = "";
            XElement linkStyle = xmlStyle.Descendants(ns + "link").FirstOrDefault();
            if (linkStyle != null)
            {
                linkStyleId = linkStyle.Attribute(val).Value;
                //  Changes the name of the attribute, because the new added style is being renamed
                linkStyle.Attribute(val).Value = linkStyle.Attribute(val).Value + newStyleNameSuffix;
            }
            return linkStyleId;
        }

        /// <summary>
        /// Gets the name of the style tagged as 'next' associated to a given style
        /// </summary>
        /// <param name="xmlStyle">Xml to search for next style</param>
        /// <returns>Name of the style</returns>
        private string GetNextStyleId(XElement xmlStyle)
        {
            XName val = ns + "val";
            string nextStyleId = "";
            XElement nextStyle = xmlStyle.Descendants(ns + "next").FirstOrDefault();
            if (nextStyle != null)
            {
                nextStyleId = nextStyle.Attribute(val).Value;
                // Changes the name of the attribute, because the new added style is being renamed
                nextStyle.Attribute(val).Value = nextStyle.Attribute(val).Value + newStyleNameSuffix;
            }
            return nextStyleId;
        }

        /// <summary>
        /// Gets the name of the style tagged as 'basedOn' associated to a given style
        /// </summary>
        /// <param name="xmlStyle">Xml to search for basedOn style</param>
        /// <returns>Name of the style</returns>
        private string GetBasedOnStyleId(XElement xmlStyle)
        {
            XName val = ns + "val";
            string basedOnStyleId = "";
            XElement basedOnStyle = xmlStyle.Descendants(ns + "basedOn").FirstOrDefault();
            if (basedOnStyle != null)
            {
                basedOnStyleId = basedOnStyle.Attribute(val).Value;
                // Change the name of the attribute, because the new added style is being renamed
                basedOnStyle.Attribute(val).Value = basedOnStyle.Attribute(val).Value + newStyleNameSuffix;
            }
            return basedOnStyleId;
        }

        /// <summary>
        /// XDocument containing Xml content of the styles part
        /// </summary>
        public XDocument GetStylesDocument()
        {
            if (parentDocument.Document.MainDocumentPart.StyleDefinitionsPart != null)
                return parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart.StyleDefinitionsPart);
            else
                return null;
        }

        /// <summary>
        /// Sets a new styles part inside the document
        /// </summary>
        /// <param name="newStylesDocument">Path of styles definition file</param>
        public void SetStylePart(XDocument newStylesDocument)
        {
            try
            {
                // Replaces XDocument with the style file to transfer
                XDocument stylesDocument = GetStylesDocument();
                if (stylesDocument == null)
                {
                    StyleDefinitionsPart stylesPart =
                        parentDocument.Document.MainDocumentPart.AddNewPart<StyleDefinitionsPart>();
                    stylesDocument = parentDocument.GetXDocument(stylesPart);
                }
                if (stylesDocument.Root == null)
                    stylesDocument.Add(newStylesDocument.Root);
                else
                    stylesDocument.Root.ReplaceWith(newStylesDocument.Root);
            }
            catch (XmlException ex) {
                throw new Exception("File specified is not a valid XML file", ex);
            }
        }

        /// <summary>
        /// Adds a new style definition
        /// </summary>
        /// <param name="xmlStyleDefinition">Style definition</param>
        public void AddStyleDefinition(XElement xmlStyleDefinition)
        {
            // Inserts the new style
            XDocument stylesPart = GetStylesDocument();
            stylesPart.Root.Add(xmlStyleDefinition);
        }

        /// <summary>
        /// Adds a set of styles in the styles.xml file
        /// </summary>
        /// <param name="xmlStyleDefinitions">Collection of style definitions</param>
        public void AddStyleDefinition(IEnumerable<XElement> xmlStyleDefinitions)
        {
            XDocument stylesPart = GetStylesDocument();
            foreach (XElement xmlStyleDefinition in xmlStyleDefinitions)
            {
                stylesPart.Root.Add(xmlStyleDefinition);
            }
        }

        /// <summary>
        /// Adds inside the styles.xml file the necesary TOC styles
        /// </summary>
        public void CreateTOCStyles(string stylesSourceFile, bool addDefaultStyles)
        {
            if (stylesSourceFile == "")
            {
                if (addDefaultStyles)
                {
                    //  the styles need it are: TOCHeading, TOC1, TOC2, TOC3 and hyperlink

                    XElement TOCHeading = new XElement(ns + "style",
                        new XAttribute(ns + "type", "paragraph"),
                        new XAttribute(ns + "styleId", "TOCHeading"),
                        new XElement(ns + "name",
                            new XAttribute(ns + "val", "TOC  Heading")),
                        new XElement(ns + "basedOn",
                            new XAttribute(ns + "val", "Heading1")),
                        new XElement(ns + "next",
                            new XAttribute(ns + "val", "Normal")),
                        new XElement(ns + "unhideWhenUsed"),
                        new XElement(ns + "qFormat"),
                        new XElement(ns + "pPr",
                            new XElement(ns + "outlineLvl",
                                new XAttribute(ns + "val", "9"))));
                    AddStyleDefinition(TOCHeading);

                    XElement TOC1 = new XElement(ns + "style",
                        new XAttribute(ns + "type", "paragraph"),
                        new XAttribute(ns + "styleId", "TOC1"),
                        new XElement(ns + "name",
                            new XAttribute(ns + "val", "toc 1")),
                        new XElement(ns + "basedOn",
                            new XAttribute(ns + "val", "Normal")),
                        new XElement(ns + "next",
                            new XAttribute(ns + "val", "Normal")),
                        new XElement(ns + "autoRedefine"),
                        new XElement(ns + "unhideWhenUsed"),
                        new XElement(ns + "pPr",
                            new XElement(ns + "spacing",
                                new XAttribute(ns + "after", "100"))));

                    AddStyleDefinition(TOC1);

                    XElement TOC2 = new XElement(ns + "style",
                        new XAttribute(ns + "type", "paragraph"),
                        new XAttribute(ns + "styleId", "TOC2"),
                        new XElement(ns + "name",
                            new XAttribute(ns + "val", "toc 2")),
                        new XElement(ns + "basedOn",
                            new XAttribute(ns + "val", "Normal")),
                        new XElement(ns + "next",
                            new XAttribute(ns + "val", "Normal")),
                            new XElement(ns + "autoRedefine"),
                            new XElement(ns + "unhideWhenUsed"),
                            new XElement(ns + "pPr",
                                new XElement(ns + "spacing",
                                    new XAttribute(ns + "after", "100")),
                                new XElement(ns + "ind",
                                    new XAttribute(ns + "left", "220"))));

                    AddStyleDefinition(TOC2);

                    XElement TOC3 = new XElement(ns + "style",
                        new XAttribute(ns + "type", "paragraph"),
                        new XAttribute(ns + "styleId", "TOC3"),
                        new XElement(ns + "name",
                            new XAttribute(ns + "val", "toc 3")),
                        new XElement(ns + "basedOn",
                            new XAttribute(ns + "val", "Normal")),
                        new XElement(ns + "next",
                            new XAttribute(ns + "val", "Normal")),
                        new XElement(ns + "autoRedefine"),
                        new XElement(ns + "unhideWhenUsed"),
                        new XElement(ns + "pPr",
                            new XElement(ns + "spacing",
                                new XAttribute(ns + "after", "100")),
                            new XElement(ns + "ind",
                                new XAttribute(ns + "left", "440"))));

                    AddStyleDefinition(TOC3);

                    XElement Hyperlink = new XElement(ns + "style",
                        new XAttribute(ns + "type", "character"),
                        new XAttribute(ns + "styleId", "Hyperlink"),
                        new XElement(ns + "name",
                            new XAttribute(ns + "val", "toc 3")),
                        new XElement(ns + "basedOn",
                            new XAttribute(ns + "val", "DefaultParagraphFont")),
                        new XElement(ns + "unhideWhenUsed"),
                        new XElement(ns + "pPr",
                            new XElement(ns + "color",
                                new XAttribute(ns + "val", "0000FF"),
                                new XAttribute(ns + "themeColor", "hyperlink")),
                            new XElement(ns + "u",
                                new XAttribute(ns + "val", "single"))));

                    AddStyleDefinition(Hyperlink);

                    XElement Heading1 = new XElement(ns + "style",
                        new XAttribute(ns + "type", "paragraph"),
                        new XAttribute(ns + "styleId", "Heading1"),
                        new XElement(ns + "name",
                           new XAttribute(ns + "val", "heading 1")),
                        new XElement(ns + "basedOn",
                           new XAttribute(ns + "val", "DefaultParagraphFont")),
                        new XElement(ns + "next",
                            new XAttribute(ns + "val", "Normal")),
                        new XElement(ns + "link",
                            new XAttribute(ns + "val", "Heading1Char")),
                        new XElement(ns + "unhideWhenUsed"),
                        new XElement(ns + "qFormat"),
                        new XElement(ns + "pPr",
                            new XElement(ns + "keepNext"),
                            new XElement(ns + "keepLines"),
                            new XElement(ns + "spacing",
                               new XAttribute(ns + "before", "480"),
                               new XAttribute(ns + "after", "0")),
                            new XElement(ns + "outlineLvl",
                               new XAttribute(ns + "val", "0"))),
                        new XElement(ns + "rPr",
                            new XElement(ns + "rFonts",
                               new XAttribute(ns + "asciiTheme", "majorHAnsi"),
                               new XAttribute(ns + "eastAsiaTheme", "majorEastAsia"),
                               new XAttribute(ns + "hAnsiTheme", "majorHAnsi"),
                               new XAttribute(ns + "cstheme", "majorBidi")),
                            new XElement(ns + "b"),
                            new XElement(ns + "bCs"),
                            new XElement(ns + "color",
                               new XAttribute(ns + "val", "365F91"),
                               new XAttribute(ns + "themeColor", "accent1"),
                               new XAttribute(ns + "themeShade", "BF")),
                            new XElement(ns + "sz",
                                new XAttribute(ns + "val", 28)),
                            new XElement(ns + "szCs",
                                new XAttribute(ns + "val", 28))));

                    AddStyleDefinition(Heading1);

                    XElement Heading1Char = new XElement(ns + "style",
                        new XAttribute(ns + "type", "paragraph"),
                        new XAttribute(ns + "styleId", "Heading1"),
                        new XElement(ns + "name",
                           new XAttribute(ns + "val", "heading 1")),
                        new XElement(ns + "basedOn",
                           new XAttribute(ns + "val", "DefaultParagraphFont")),
                        new XElement(ns + "link",
                            new XAttribute(ns + "val", "Heading1")),
                        new XElement(ns + "rPr",
                            new XElement(ns + "rFonts",
                               new XAttribute(ns + "asciiTheme", "majorHAnsi"),
                               new XAttribute(ns + "eastAsiaTheme", "majorEastAsia"),
                               new XAttribute(ns + "hAnsiTheme", "majorHAnsi"),
                               new XAttribute(ns + "cstheme", "majorBidi")),
                            new XElement(ns + "b"),
                            new XElement(ns + "bCs"),
                            new XElement(ns + "color",
                               new XAttribute(ns + "val", "365F91"),
                               new XAttribute(ns + "themeColor", "accent1"),
                               new XAttribute(ns + "themeShade", "BF")),
                            new XElement(ns + "sz",
                                new XAttribute(ns + "val", 28)),
                            new XElement(ns + "szCs",
                                new XAttribute(ns + "val", 28))));

                    AddStyleDefinition(Heading1Char);

                }
            }
            else
            {
                //  add the styles from the styles source file
                XDocument StyleXmlPart = GetStylesDocument();
                //  the preffix must be empty, because the styles need to be recognized by the TOC
                XDocument stylesSource = XDocument.Load(stylesSourceFile);
                IEnumerable<XElement> TOCStyles = GetStyleHierarchy("TOCHeading", stylesSource, string.Empty);
                TOCStyles = TOCStyles.Concat(GetStyleHierarchy("Hyperlink", stylesSource, string.Empty));
                TOCStyles = TOCStyles.Concat(GetStyleHierarchy("TOC1", stylesSource, string.Empty));
                TOCStyles = TOCStyles.Concat(GetStyleHierarchy("TOC2", stylesSource, string.Empty));
                TOCStyles = TOCStyles.Concat(GetStyleHierarchy("TOC3", stylesSource, string.Empty));
                AddStyleDefinition(TOCStyles);
            }
        }

        /// <summary>
        /// add inside the styles.xml file the necesary TOC styles
        /// </summary>
        public void CreateTOFStyles(string stylesSourceFile, bool addDefaultStyles)
        {
            if (stylesSourceFile == "")
            {
                if (addDefaultStyles)
                {
                    XElement Caption = new XElement(ns + "style",
                        new XAttribute(ns + "type", "paragraph"),
                        new XAttribute(ns + "styleId", "Caption"),
                        new XElement(ns + "name",
                           new XAttribute(ns + "val", "caption")),
                        new XElement(ns + "basedOn",
                           new XAttribute(ns + "val", "Normal")),
                        new XElement(ns + "next",
                            new XAttribute(ns + "val", "Normal")),
                        new XElement(ns + "qFormat"),
                        new XElement(ns + "pPr",
                            new XElement(ns + "spacing",
                                new XAttribute(ns + "line", "240"),
                                new XAttribute(ns + "lineRule", "auto"))),
                        new XElement(ns + "rPr",
                            new XElement(ns + "b"),
                            new XElement(ns + "bCs"),
                            new XElement(ns + "color",
                               new XAttribute(ns + "val", "4F81BD"),
                               new XAttribute(ns + "themeColor", "accent1")),
                            new XElement(ns + "sz",
                                new XAttribute(ns + "val", 18)),
                            new XElement(ns + "szCs",
                                new XAttribute(ns + "val", 18))));

                    AddStyleDefinition(Caption);

                    XElement TableOfFigures = new XElement(ns + "style",
                        new XAttribute(ns + "type", "paragraph"),
                        new XAttribute(ns + "styleId", "TableofFigures"),
                        new XElement(ns + "name",
                            new XAttribute(ns + "val", "toc 3")),
                        new XElement(ns + "basedOn",
                            new XAttribute(ns + "val", "DefaultParagraphFont")),
                        new XElement(ns + "next",
                            new XAttribute(ns + "val", "Normal")),
                        new XElement(ns + "unhideWhenUsed"),
                        new XElement(ns + "pPr",
                            new XElement(ns + "color",
                                new XAttribute(ns + "spacing", "0000FF"),
                                new XAttribute(ns + "after", "0"))));

                    AddStyleDefinition(TableOfFigures);

                    XElement Hyperlink = new XElement(ns + "style",
                        new XAttribute(ns + "type", "character"),
                        new XAttribute(ns + "styleId", "Hyperlink"),
                        new XElement(ns + "name",
                            new XAttribute(ns + "val", "toc 3")),
                            new XElement(ns + "basedOn",
                                new XAttribute(ns + "val", "DefaultParagraphFont")),
                        new XElement(ns + "unhideWhenUsed"),
                        new XElement(ns + "pPr",
                            new XElement(ns + "color",
                                new XAttribute(ns + "val", "0000FF"),
                                new XAttribute(ns + "themeColor", "hyperlink")),
                            new XElement(ns + "u",
                                new XAttribute(ns + "val", "single"))));

                    AddStyleDefinition(Hyperlink);
                }
            }
            else
            {
                //  add the styles from the styles source file
                XDocument StyleXmlPart = GetStylesDocument();
                //  the preffix must be empty, because the styles need to be recognized by the TOC
                XDocument stylesSource = XDocument.Load(stylesSourceFile);
                IEnumerable<XElement> TOFStyles = GetStyleHierarchy("TableofFigures", stylesSource, string.Empty);
                TOFStyles = TOFStyles.Concat(GetStyleHierarchy("Hyperlink", stylesSource, string.Empty));
                TOFStyles = TOFStyles.Concat(GetStyleHierarchy("Caption", stylesSource, string.Empty));
                AddStyleDefinition(TOFStyles);
            }
        }

        /// <summary>
        /// add inside the styles.xml file the necesary TOC styles
        /// </summary>
        public void CreateTOAStyles(string stylesSourceFile, bool addDefaultStyles)
        {
            if (stylesSourceFile == "")
            {
                if (addDefaultStyles)
                {
                    XElement TOAHeading = new XElement(ns + "style",
                        new XAttribute(ns + "type", "paragraph"),
                        new XAttribute(ns + "styleId", "TOAHeading"),
                        new XElement(ns + "name",
                           new XAttribute(ns + "val", "toa heading")),
                        new XElement(ns + "basedOn",
                           new XAttribute(ns + "val", "Normal")),
                        new XElement(ns + "next",
                            new XAttribute(ns + "val", "Normal")),
                        new XElement(ns + "semiHidden"),
                        new XElement(ns + "unhideWhenUsed"),
                        new XElement(ns + "pPr",
                            new XElement(ns + "spacing",
                                new XAttribute(ns + "before", "120"))),
                        new XElement(ns + "rPr",
                            new XElement(ns + "b"),
                            new XElement(ns + "bCs"),
                            new XElement(ns + "sz",
                                new XAttribute(ns + "val", 24)),
                            new XElement(ns + "szCs",
                                new XAttribute(ns + "val", 24))));

                    AddStyleDefinition(TOAHeading);

                    XElement tableOfAuthorities = new XElement(ns + "style",
                        new XAttribute(ns + "type", "paragraph"),
                        new XAttribute(ns + "styleId", "TableofAuthorities"),
                        new XElement(ns + "name",
                            new XAttribute(ns + "val", "table of authorities")),
                        new XElement(ns + "basedOn",
                            new XAttribute(ns + "val", "Normal")),
                        new XElement(ns + "next",
                            new XAttribute(ns + "val", "Normal")),
                        new XElement(ns + "semiHidden"),
                        new XElement(ns + "unhideWhenUsed"),
                        new XElement(ns + "pPr",
                            new XElement(ns + "spacing",
                                new XAttribute(ns + "after", "0")),
                            new XElement(ns + "ind", 
                                new XAttribute(ns + "left", "220"),
                                new XAttribute(ns + "hanging", "220"))));

                    AddStyleDefinition(tableOfAuthorities);
                }
            }
            else
            {
                //  add the styles from the styles source file
                XDocument StyleXmlPart = GetStylesDocument();
                //  the prefix must be empty, because the styles need to be recognized by the TOC
                XDocument stylesSource = XDocument.Load(stylesSourceFile);
                IEnumerable<XElement> TOAStyles = GetStyleHierarchy("TOAHeading", stylesSource, string.Empty);
                TOAStyles = TOAStyles.Concat(GetStyleHierarchy("TableofAuthorities", stylesSource, string.Empty));
                AddStyleDefinition(TOAStyles);
            }
        }

        /// <summary>
        /// add inside the styles.xml file the necesary TOC styles
        /// </summary>
        public void CreateIndexStyles(string stylesSourceFile, bool addDefaultStyles)
        {
            if (stylesSourceFile == "")
            {
                if (addDefaultStyles)
                {
                    XElement Index1 = new XElement(ns + "style",
                        new XAttribute(ns + "type", "paragraph"),
                        new XAttribute(ns + "styleId", "Index1"),
                        new XElement(ns + "name",
                           new XAttribute(ns + "val", "index 1")),
                        new XElement(ns + "basedOn",
                           new XAttribute(ns + "val", "Normal")),
                        new XElement(ns + "next",
                            new XAttribute(ns + "val", "Normal")),
                        new XElement(ns + "autoRedefine"),
                        new XElement(ns + "semiHidden"),
                        new XElement(ns + "unhideWhenUsed"),
                        new XElement(ns + "pPr",
                            new XElement(ns + "spacing",
                                new XAttribute(ns + "after", "0"),
                                new XAttribute(ns + "line", "240"),
                                new XAttribute(ns + "lineRule", "auto")),
                            new XElement(ns + "ind",
                                new XAttribute(ns + "left", "220"),
                                new XAttribute(ns + "hanging", "220"))));

                    AddStyleDefinition(Index1);
                }
            }
            else
            {
                //  add the styles from the styles source file
                XDocument StyleXmlPart = GetStylesDocument();
                //  the preffix must be empty, because the styles need to be recognized by the TOC
                XDocument stylesSource = XDocument.Load(stylesSourceFile);
                IEnumerable<XElement> IndexStyles = GetStyleHierarchy("Index1", stylesSource, string.Empty);
                AddStyleDefinition(IndexStyles);
            }
        }

        /// <summary>
        /// Insert a style into a given xmlpath inside the document part
        /// </summary>
        /// <param name="xpathInsertionPoint">place where we are going to put the style</param>
        /// <param name="styleValue">name of the style</param>
        /// <param name="stylesSource">XDocument containing styles</param>
        public void InsertStyle(string xpathInsertionPoint, string styleValue, XDocument stylesSource)
        {
            XDocument xDocument = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart);
            XmlDocument xmlMainDocument = OpenXmlDocument.LoadXmlDocumentFromXDocument(xDocument);

            //  create the style element to add in the document, based upon the style name.
            //  this is an example of an style element

            //  <w:pStyle w:val="Heading1" /> 

            //  so, in order to construct this, we have to know already if the style will be placed inside
            //  a run or inside a paragraph. to know this we have to verify against the xpath, and know if
            //  the query want to access a 'run' or a paragraph

            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(xmlMainDocument.NameTable);
            namespaceManager.AddNamespace("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");

            XmlNodeList insertionPoints = xmlMainDocument.SelectNodes(xpathInsertionPoint, namespaceManager);

            if (insertionPoints.Count == 0)
                throw new Exception("The xpath query did not return a valid location.");

            foreach (XmlNode insertionPoint in insertionPoints)
            {
                XmlElement xmlStyle = null;

                if (insertionPoint.LocalName == "r" || insertionPoint.LocalName == "p")
                {
                    XmlNode propertiesElement = insertionPoint.SelectSingleNode(@"w:pPr|w:rPr", namespaceManager);

                    //if (propertiesElement != null)
                    //{
                        if (insertionPoint.Name == "w:p")
                        {
                            xmlStyle = xmlMainDocument.CreateElement("w", "pStyle", namespaceManager.LookupNamespace("w"));

                            //  retrieve the suffix from the styleAccesor class
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
                    //}
                }
                else
                {
                    throw new Exception("The xpath query did not return a valid location.");                
                }
            }

            OpenXmlDocument.SaveXmlDocumentIntoXDocument(xmlMainDocument, xDocument);

            //  the style has been added in the main document part, but now we have to add the
            //  style definition in the styles definitions part. the style definition need to be
            //  extracted from the given inputStyle file.

            //  Because a style can be linked with other styles and
            //  can also be based on other styles,  all the complete hierarchy of styles has
            //  to be added
            Collection<XElement> styleHierarchy = parentDocument.Style.GetStyleHierarchy(styleValue, stylesSource, newStyleNameSuffix);

            //  open the styles file in the document
            XDocument xmlStylesDefinitionDocument = parentDocument.Style.GetStylesDocument();

            XDocument xElem = new XDocument();
            xElem.Add(xmlStylesDefinitionDocument.Root);
            //insert the new style
            foreach (XElement xmlStyleDefinition in styleHierarchy)
            {
                xElem.Root.Add(xmlStyleDefinition);
            }
            xmlStylesDefinitionDocument.Root.ReplaceWith(xElem.Root);
        }

    }
}
