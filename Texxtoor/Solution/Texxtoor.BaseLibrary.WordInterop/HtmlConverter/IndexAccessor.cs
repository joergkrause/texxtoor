/***************************************************************************

Copyright (c) Microsoft Corporation 2008.

This code is licensed using the Microsoft Public License (Ms-PL).  The text of the license can be found here:

http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx

***************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;

namespace OpenXml.PowerTools.Wordprocessing
{
    /// <summary>
    /// process available index references
    /// </summary>
    public class IndexAccessor
    {
        private PTWordprocessingDocument parentDocument;
        private static XNamespace ns;

        static IndexAccessor()
        {
            ns = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="document">Document to process</param>
        public IndexAccessor(PTWordprocessingDocument document)
        {
            this.parentDocument = document;
        }

        private IEnumerable<XElement> IndexReferences()
        {
            XDocument mainDocument = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart);
            IEnumerable<XElement> results =
                mainDocument
                .Descendants(ns + "p")
                .Elements(ns + "r")
                .Where(
                    r =>
                        r.Elements(ns + "instrText").Count() > 0 && 
                        r.ElementsBeforeSelf().Last().Element(ns + "instrText")!= null &&
                        r.ElementsBeforeSelf().Last().Element(ns + "instrText").Value.EndsWith("\"") && 
                        r.ElementsAfterSelf().First().Element(ns + "instrText") != null &&
                        r.ElementsAfterSelf().First().Element(ns + "instrText").Value.StartsWith("\"")
                );

            return results;
        }

        //private IEnumerable<XElement> IndexReferences()
        //{
        //    XDocument mainDocument = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart);
        //    IEnumerable<XElement> results =
        //      mainDocument
        //      .Descendants(ns + "p")
        //      .Descendants(ns + "instrText")
        //      .Where(
        //        t => !t.Value.StartsWith("\"") && !t.Value.EndsWith("\"")
        //      );

        //    return results;
        //}

        /// <summary>
        /// Retrieves from a string containing the field-specific-switches of an index, its
        /// category
        /// </summary>
        /// <param name="reference"></param>
        /// <returns></returns>
        private string GetFieldCode(XElement reference)
        {
            //  One example of a Index reference field code could be:
            //  XE "Tres:tres"

            //  Build the complete text of the reference field
            string fieldCode = "";
            foreach (XElement partialFieldCode in reference.Descendants(ns + "instrText"))
            {
                fieldCode += partialFieldCode.Value;
            }

            return fieldCode;
        }

        /// <summary>
        /// Get the reference category of a field code string
        /// </summary>
        /// <param name="fieldCode">the field code string</param>
        /// <returns>The number of the category</returns>
        private string GetIndexMainEntry(string fieldCode)
        {

            //  Retrieve the main entry
            string mainEntry = "";
            Regex expReg = new Regex(@""".*?""");
            Match match = expReg.Match(fieldCode);
            if (match.Success)
            {
                mainEntry = match.Value;
            }

            //  remove the " letters from the main entry
            mainEntry = mainEntry.Remove(0, 1);
            mainEntry = mainEntry.Remove(mainEntry.Length - 1, 1);

            return mainEntry;
        }

        /// <summary>
        /// Generate the Index of the document
        /// </summary>
        public void Generate()
        {
            XElement Index = new XElement("Index");

            XElement IndexFirstPart =
                        new XElement(ns + "p",
                            new XElement(ns + "r",
                                new XElement(ns + "fldChar",
                                    new XAttribute(ns + "fldCharType", "begin")),
                                new XElement(ns + "instrText",
                                    new XAttribute(XNamespace.Xml + "space", "preserve"),
                                    @" INDEX \h ""A"" \c ""2"" \z ""1033"" "),
                                new XElement(ns + "fldChar",
                                    new XAttribute(ns + "fldCharType", "separate"))));

            Index.Add(IndexFirstPart);

            //  Build the index with the IndexReferences
            foreach (XElement reference in IndexReferences())
            {
                //string fieldCode = GetFieldCode(reference);
                string mainEntry = reference.Value;//GetIndexMainEntry(reference.Value);

                //  Build the XElement containing the index reference
                XElement IndexElement =
                    new XElement(ns + "p",
                        new XElement(ns + "pPr",
                            new XElement(ns + "pStyle",
                                new XAttribute(ns + "val", "Index1")),
                                new XElement(ns + "tabs",
                                    new XElement(ns + "tab",
                                        new XAttribute(ns + "val", "right"),
                                        new XAttribute(ns + "leader", "dot"),
                                        new XAttribute(ns + "pos", "9350")))),
                        new XElement(ns + "r",
                            new XElement(ns + "t", mainEntry)));

                Index.Add(IndexElement);
            }

            //  Close the open character field
            Index.Add(
                new XElement(ns + "p",
                    new XElement(ns + "r",
                        new XElement(ns + "fldChar",
                            new XAttribute(ns + "fldCharType", "end")))));

            XDocument mainDocumentPart = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart);
            foreach (XElement IndexElement in Index.Elements())
            {
                mainDocumentPart.Descendants(ns + "body").First().Add(IndexElement);
            }
        }
    }
}
