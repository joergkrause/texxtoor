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
    /// Available Position to place the TOF
    /// </summary>
    public enum Position
    {
        /// <summary>
        /// Beginning
        /// </summary>
        Beginning,
        /// <summary>
        /// End
        /// </summary>
        End
    }
    /// <summary>
    /// Provides methods to work with TOF
    /// </summary>
    public class TOFAccessor
    {
        private PTWordprocessingDocument parentDocument;

        private static XNamespace ns;
        private static XNamespace vmlns;
        private string bookmarkNamePrefix = "_Tof";

        static TOFAccessor()
        {
            ns = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
            vmlns = "urn:schemas-microsoft-com:vml";
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="document">Document to process</param>
        public TOFAccessor(PTWordprocessingDocument document)
        {
            this.parentDocument = document;
        }

        private IEnumerable<XElement> FigureTitleParagraphsElements()
        {
            XDocument mainDocument = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart);
            IEnumerable<XElement> results =
                mainDocument.Descendants().Where
                (
                    tag =>
                        tag.Name == ns + "p" &&
                        tag.Elements().Where
                        (
                            tag2 =>
                                tag2.Name == ns + "fldSimple" &&
                                tag2.Attribute(ns + "instr").Value.StartsWith(" SEQ Figure")
                        ).Count() > 0
                );

            return results;
        }

        /// <summary>
        /// Retrieve from a FigureTitleParagraphsElement the label text
        /// /// </summary>
        /// <returns></returns>
        private string GetLabelText(XElement FigureTitleParagraphElement)
        {
            string label = "";
            foreach(XElement labelTextPart in FigureTitleParagraphElement.Descendants(ns + "t"))
            {
                label += labelTextPart.Value;
            }
            return label;
        }

        /// <summary>
        /// Generate the TOF (Table Of Figures) of the document
        /// </summary>
        public void Generate(Position pos)
        {
            if (FigureTitleParagraphsElements().Count() > 0)
            {
                int bookMarkIdCounter = 0;

                //  Get the XDocument body, that is the element where TOF are going to be added
                XDocument xmlMainDocument = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart);
                XElement bodyElement = xmlMainDocument.Descendants(ns + "body").First();

                // begin the build of the TOF
                XElement TOF = new XElement("TOF",
                    new XElement(ns + "p",
                        new XElement(ns + "pPr",
                            new XElement(ns + "pStyle",
                                new XAttribute(ns + "val", "TableofFigures")),
                                new XElement(ns + "tabs",
                                    new XElement(ns + "tab",
                                        new XAttribute(ns + "val", "right"),
                                        new XAttribute(ns + "leader", "dot"),
                                        new XAttribute(ns + "pos", "9350")))),
                        new XElement(ns + "r",
                            new XElement(ns + "fldChar",
                                new XAttribute(ns + "fldCharType", "begin"))),
                        new XElement(ns + "r",
                            new XElement(ns + "instrText",
                                new XAttribute(XNamespace.Xml + "space", "preserve"),
                                @" TOC \h \z \c ""Figure"" ")),
                        new XElement(ns + "r",
                            new XElement(ns + "fldChar",
                                new XAttribute(ns + "fldCharType", "separate")))));

                //  for each title found it in the document, we have to wrap the run inside of it,
                //  with a bookmark, this bookmark will have an id which will work as an anchor,
                //  for link references in the TOF
                foreach (XElement figureTitleParagraph in FigureTitleParagraphsElements())
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
                    figureTitleParagraph.Descendants(ns + "r").First().AddBeforeSelf(bookmarkStart);
                    figureTitleParagraph.Descendants(ns + "r").First().AddAfterSelf(bookmarkEnd);
                    TOF.Add(new XElement(ns + "p",
                        new XElement(ns + "pPr",
                            new XElement(ns + "pStyle",
                                new XAttribute(ns + "val", "TableofFigures")),
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
                                new XElement(ns + "t", GetLabelText(figureTitleParagraph)),
                                new XElement(ns + "tab"),
                                new XElement(ns + "fldChar",
                                    new XAttribute(ns + "fldCharType", "begin")),
                                new XElement(ns + "instrText",
                                    new XAttribute(XNamespace.Xml + "space", "preserve"),
                                    " PAGEREF " + bookmarkName + @" \h "),
                                new XElement(ns + "fldChar",
                                    new XAttribute(ns + "fldCharType", "separate")),
                                new XElement(ns + "fldChar",
                                    new XAttribute(ns + "fldCharType", "end"))))));
                    bookMarkIdCounter++;
                }

                TOF.Add(
                    new XElement(ns + "p",
                        new XElement(ns + "r",
                            new XElement(ns + "fldChar",
                                new XAttribute(ns + "fldCharType", "end")))));

                if (pos == Position.End)
                {
                    bodyElement.Add(TOF.Elements());
                }
                else
                {
                    bodyElement.AddFirst(TOF.Elements());
                }
            }
        }
    }
}
