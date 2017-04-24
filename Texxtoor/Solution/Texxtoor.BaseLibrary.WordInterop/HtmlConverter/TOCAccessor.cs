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
    /// Provides methods to work with TOC
    /// </summary>
    public class TOCAccessor
    {
        private PTWordprocessingDocument parentDocument;

        private static XNamespace ns;
        private string bookmarkNamePrefix = "_Toc";

        static TOCAccessor()
        {
            ns = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="document">Document to process</param>
        public TOCAccessor(PTWordprocessingDocument document)
        {
            this.parentDocument = document;
        }

        private IEnumerable<XElement> TitleParagraphsElements()
        {
            XDocument mainDocument = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart);
            IEnumerable<XElement> results =
              mainDocument.Descendants().Where
              (
                  tag =>
                      tag.Name == ns + "p" &&
                      tag.Descendants(ns + "t").Count() > 0 &&
                      tag.Descendants().Where
                      (
                        tag2 =>
                            tag2.Name == ns + "pStyle" &&
                            (
                                tag2.Attribute(ns + "val").Value == "Heading1" ||
                                tag2.Attribute(ns + "val").Value == "Heading2" ||
                                tag2.Attribute(ns + "val").Value == "Heading3"
                            )
                      ).Count() > 0
              );

            return results;
        }

        /// <summary>
        /// Generate the Table Of Contents(TOC) of the document
        /// </summary>
        public void Generate()
        {
            int bookMarkIdCounter = 0;

            //  sdtContent, will contain all the paragraphs used in the TOC
            XElement sdtContent = new XElement(ns + "sdtContent");

            //  build the title of the TOC and add it in the sdtContent element, and
            //  some information regarding the attributes of the TOC
            sdtContent.Add(
                new XElement(ns + "p",
                    new XElement(ns + "pPr",
                        new XElement(ns + "pStyle",
                            new XAttribute(ns + "val", "TOCHeading"))),
                    new XElement(ns + "r",
                        new XElement(ns + "t",
                            "Contents"),
                        new XElement(ns + "r",
                            new XElement(ns + "fldChar",
                                    new XAttribute(ns + "fldCharType", "begin"))),
                        new XElement(ns + "r",
                            new XElement(ns + "instrText",
                                new XAttribute(XNamespace.Xml + "space", "preserve"),
                                @" TOC \o ""1-3"" \h \z \u ")),
                        new XElement(ns + "r",
                            new XElement(ns + "fldChar",
                                new XAttribute(ns + "fldCharType", "separate"))))));

            //  for each title found it in the document, we have to wrap the run inside of it,
            //  with a bookmark, this bookmark will have an id which will work as an anchor,
            //  for link references in the TOC
            foreach (XElement titleParagraph in TitleParagraphsElements())
            {
                string bookmarkName = bookmarkNamePrefix + bookMarkIdCounter;
                XElement bookmarkStart =
                    new XElement(ns + "bookmarkStart",
                        new XAttribute(ns + "id", bookMarkIdCounter),
                        new XAttribute(ns + "name", bookmarkName));
                XElement bookmarkEnd =
                    new XElement(ns + "bookmarkEnd",
                        new XAttribute(ns + "id", bookMarkIdCounter));

                //  wrap the run with bookmarkStart and bookmarkEnd
                titleParagraph.AddFirst(bookmarkStart);
                titleParagraph.Add(bookmarkEnd);

                //  get the name of the style of the parapgraph of the title, and for each one,
                //  choose a style to add in the paragraph inside the TOC
                string referenceTitleStyle = "";
                switch (titleParagraph.Descendants(ns + "pStyle").First().Attribute(ns + "val").Value)
                {
                    case "Heading1": referenceTitleStyle = "TOC1";
                        break;
                    case "Heading2": referenceTitleStyle = "TOC2";
                        break;
                    case "Heading3": referenceTitleStyle = "TOC3";
                        break;
                }

                XElement entryElement = titleParagraph.Descendants(ns + "t").FirstOrDefault();
                string entryContent = entryElement == null ? string.Empty : entryElement.Value;

                XElement TOCElement =
                    new XElement(ns + "p",
                        new XElement(ns + "pPr",
                            new XElement(ns + "pStyle",
                                new XAttribute(ns + "val", referenceTitleStyle)),
                            new XElement(ns + "tabs",
                                new XElement(ns + "tab",
                                    new XAttribute(ns + "val", "right"),
                                    new XAttribute(ns + "leader", "dot"),
                                    new XAttribute(ns + "pos", "9350")))),
                        new XElement(ns + "hyperlink",
                            new XAttribute(ns + "anchor", bookmarkName),
                            new XElement(ns + "r",
                                new XElement(ns + "rPr",
                                    new XElement(ns + "rStyle",
                                        new XAttribute(ns + "val", "Hyperlink"))),
                                new XElement(ns + "t", entryContent),
                                new XElement(ns + "tab"),
                                new XElement(ns + "fldChar",
                                    new XAttribute(ns + "fldCharType", "begin")),
                                new XElement(ns + "instrText",
                                    new XAttribute(XNamespace.Xml + "space", "preserve"),
                                    " PAGEREF " + bookmarkName + @" \h "),
                                new XElement(ns + "fldChar",
                                    new XAttribute(ns + "fldCharType", "separate")),
                                new XElement(ns + "fldChar",
                                    new XAttribute(ns + "fldCharType", "end")))));

                sdtContent.Add(TOCElement);
                bookMarkIdCounter++;
            }

            sdtContent.Add(
                new XElement(ns + "p",
                    new XElement(ns + "r",
                        new XElement(ns + "fldChar",
                            new XAttribute(ns + "fldCharType", "end")))));

            //  Finish the xml construction of the TOC
            XElement TOC =
                new XElement(ns + "sdt",
                    new XElement(ns + "sdtPr",
                        new XElement(ns + "docPartObj",
                            new XElement(ns + "docPartGallery",
                                new XAttribute(ns + "val", "Table of Contents")),
                            new XElement(ns + "docPartUnique"))),
                    sdtContent);

            //  add it to the original document
            XDocument xmlMainDocument = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart);
            xmlMainDocument.Root.Element(ns + "body").AddFirst(TOC);
        }
    }
}
