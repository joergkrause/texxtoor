/***************************************************************************

Copyright (c) Microsoft Corporation 2008.

This code is licensed using the Microsoft Public License (Ms-PL).  The text of the license can be found here:

http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx

***************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;

namespace OpenXml.PowerTools.Wordprocessing
{
    /// <summary>
    /// Provides access to change tracking operations
    /// </summary>
    public class ChangeAccessor
    {
        private PTWordprocessingDocument parentDocument;

        private static XNamespace ns;

        static ChangeAccessor()
        {
            ns = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="document">Document to perform operations on</param>
        public ChangeAccessor(PTWordprocessingDocument document)
        {
            this.parentDocument = document;
        }

        /// <summary>
        /// Nodes list with elements that need to be deleted
        /// </summary>
        private IEnumerable<XElement> ChangeTrackingElements()
        {
            XDocument mainDocument = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart);
            XName del = ns + "del";
            XName moveFromRangeStart = ns + "moveFromRangeStart";
            XName moveFrom = ns + "moveFrom";
            XName moveFromRangeEnd = ns + "moveFromRangeEnd";
            XName moveToRangeStart = ns + "moveToRangeStart";
            XName moveToRangeEnd = ns + "moveToRangeEnd";

            IEnumerable<XElement> results =
                mainDocument.Descendants().Where(
                    tag =>
                        tag.Name == del ||
                        tag.Name == moveFromRangeStart ||
                        tag.Name == moveFrom ||
                        tag.Name == moveFromRangeEnd ||
                        tag.Name == moveToRangeStart ||
                        tag.Name == moveToRangeEnd
                );
            return results;
        }

        /// <summary>
        /// Elements tagged as insertions inside the document
        /// </summary>
        private IEnumerable<XElement> InsElements()
        {
            XDocument mainDocument = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart);

            IEnumerable<XElement> results = mainDocument.Descendants(ns + "ins");
            return results;
        }

        /// <summary>
        /// Elements tagged as deletions inside the document
        /// </summary>
        private IEnumerable<XElement> DelElements()
        {
            XDocument mainDocument = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart);

            IEnumerable<XElement> results = mainDocument.Descendants(ns + "del");
            return results;
        }

        /// <summary>
        /// Elements tagged as text deletion elements inside the document
        /// </summary>
        private IEnumerable<XElement> DelTextElements()
        {
            XDocument mainDocument = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart);

            IEnumerable<XElement> results = mainDocument.Descendants(ns + "del").Descendants(ns + "delText");
            return results;
        }

        /// <summary>
        /// Attributes identifying deletions inside paragraphs
        /// </summary>
        private IEnumerable<XAttribute> DelAttributes()
        {
            XDocument mainDocument = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart);

            IEnumerable<XAttribute> results =
                mainDocument.Descendants().Attributes(ns + "rsidDel");
            return results;
        }

        /// <summary>
        /// Nodes list with elements that need to be reallocated.
        /// </summary>
        private IEnumerable<XElement> AcceptedTrackingElements()
        {
            XDocument mainDocument = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart);
            XName ins = ns + "ins";
            XName moveTo = ns + "moveTo";

            IEnumerable<XElement> results =
                mainDocument.Descendants().Where(
                    tag => tag.Name == ins || tag.Name == moveTo
                );
            return results;
        }

        /// <summary>
        /// Accepts all text change revisions for one part
        /// </summary>
        private void AcceptRevisionsForPart(OpenXmlPart part)
        {
            XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
            XNamespace m = "http://schemas.openxmlformats.org/officeDocument/2006/math";

            XDocument xDoc = parentDocument.GetXDocument(part);

            // Accept inserted text, run properties for paragraph marks, etc.
            // =============================================================================

            // Find all w:ins elements, remove the w:ins element and move its children nodes
            // up one level.
            //
            // Before:
            //
            //<w:p w:rsidR="005C743F"
            //     w:rsidRDefault="005C743F">
            //    <w:ins w:id="1"
            //           w:author="Eric White"
            //           w:date="2008-04-27T08:33:00Z">
            //        <w:r>
            //            <w:t xml:space="preserve">Text </w:t>
            //        </w:r>
            //    </w:ins>
            //    <w:r>
            //        <w:t>inserted at the beginning of the paragraph.</w:t>
            //    </w:r>
            //</w:p>
            //
            // After:
            //
            //<w:p w:rsidR="005C743F"
            //     w:rsidRDefault="005C743F">
            //    <w:r>
            //        <w:t xml:space="preserve">Text </w:t>
            //    </w:r>
            //    <w:r>
            //        <w:t>inserted at the beginning of the paragraph.</w:t>
            //    </w:r>
            //</w:p>
            //
            // Some of the w:ins elements have no children, for instance a run property that
            // indicates that a paragraph has been inserted looks like this:
            //
            //<w:p w:rsidR="005C743F"
            //     w:rsidRDefault="005C743F">
            //    <w:pPr>
            //        <w:rPr>
            //            <w:ins w:id="2"
            //                   w:author="Eric White"
            //                   w:date="2008-04-27T08:34:00Z"/>
            //        </w:rPr>
            //    </w:pPr>
            //    <w:r>
            //        <w:t xml:space="preserve">Text inserted at the end of the </w:t>
            //    </w:r>
            //    <w:r>
            //        <w:t>paragraph.</w:t>
            //    </w:r>
            //</w:p>
            //
            // and we want it to look like this:
            //
            //<w:p w:rsidR="005C743F"
            //     w:rsidRDefault="005C743F">
            //    <w:pPr>
            //        <w:rPr>
            //        </w:rPr>
            //    </w:pPr>
            //    <w:r>
            //        <w:t xml:space="preserve">Text inserted at the end of the </w:t>
            //    </w:r>
            //    <w:r>
            //        <w:t>paragraph.</w:t>
            //    </w:r>
            //</w:p>
            //
            // There are no child nodes of that w:ins element, so the following code works properly
            // to accept this revision too.

            foreach (var x in xDoc.Descendants(w + "ins").ToList())
                x.ReplaceWith(x.Nodes());

            // Accept deleted paragraphs.
            // =============================================================================
            //
            // Find all w:p/w:pPr/w:rPr/w:del nodes, append all child nodes of the following paragraph
            // to the paragraph containing the w:p/w:pPr/w:rPr/w:del node, delete the following paragraph,
            // and delete the w:p/w:pPr/w:rPr/w:del node.
            //
            // Before:
            //
            //<w:p w:rsidR="004A3F21"
            //     w:rsidDel="004A3F21"
            //     w:rsidRDefault="004A3F21"
            //     w:rsidP="008A4F80">
            //    <w:pPr>
            //        <w:rPr>
            //            <w:del w:id="17"
            //                   w:author="Eric White"
            //                   w:date="2008-04-27T13:02:00Z" />
            //        </w:rPr>
            //    </w:pPr>
            //    <w:r>
            //        <w:t xml:space="preserve">This paragraph is joined </w:t>
            //    </w:r>
            //</w:p>
            //<w:p w:rsidR="004A3F21"
            //     w:rsidRDefault="004A3F21"
            //     w:rsidP="008A4F80">
            //    <w:r>
            //        <w:t>with this paragraph.</w:t>
            //    </w:r>
            //</w:p>
            //
            // After:
            //
            //<w:p w:rsidR="004A3F21"
            //     w:rsidDel="004A3F21"
            //     w:rsidRDefault="004A3F21"
            //     w:rsidP="008A4F80">
            //    <w:pPr>
            //        <w:rPr>
            //        </w:rPr>
            //    </w:pPr>
            //    <w:r>
            //        <w:t xml:space="preserve">This paragraph is joined </w:t>
            //    </w:r>
            //    <w:r>
            //        <w:t>with this paragraph.</w:t>
            //    </w:r>
            //</w:p>

            foreach (var x in xDoc.Descendants(w + "p")
                                  .Elements(w + "pPr")
                                  .Elements(w + "rPr")
                                  .Elements(w + "del")
                                  .Reverse()
                                  .ToList())
            {
                // find the w:p element
                XElement p = x.Ancestors(w + "p").First();

                // add the elements of the paragraph following.  This code will work even if there
                // is no following paragraph.
                p.Add(p.ElementsAfterSelf(w + "p").Take(1).Elements());

                // Remove the next paragraph if there is one.
                p.ElementsAfterSelf(w + "p").Take(1).Remove();

                // remove the w:p/w:pPr/w:rPr/w:del node
                x.Remove();
            }

            // Accept changes for changes in formatting on paragraphs.
            // Accept changes for changes in formatting on runs.
            // Accept changes for applied styles to a table.
            // Accept changes for grid changes to a table.
            // Accept changes for column properties.
            // Accept changes for row properties.
            // Accept revisions for table level property exceptions.
            // Accept revisions for section properties.
            var pPrChange = w + "pPrChange";
            var rPrChange = w + "rPrChange";
            var tblPrChange = w + "tblPrChange";
            var tblGridChange = w + "tblGridChange";
            var tcPrChange = w + "tcPrChange";
            var trPrChange = w + "trPrChange";
            var tblPrExChange = w + "tblPrExChange";
            var sectPrChange = w + "sectPrChange";
            xDoc.Descendants()
                .Where(x =>
                        x.Name == pPrChange ||
                        x.Name == rPrChange ||
                        x.Name == tblPrChange ||
                        x.Name == tblGridChange ||
                        x.Name == tcPrChange ||
                        x.Name == trPrChange ||
                        x.Name == tblPrExChange ||
                        x.Name == sectPrChange)
                .Remove();

            // Accept changes for deleted rows in tables.
            // Find all w:tr/w:trPr/w:del elements, and remove the w:tr elements.
            foreach (var x in xDoc.Descendants(w + "tr")
                                  .Elements(w + "trPr")
                                  .Elements(w + "del")
                                  .ToList())
                x.Parent.Parent.Remove();

            // Accept deleted text in paragraphs.
            // =============================================================================
            //
            // Remove all w:p/w:del nodes.
            //
            // Before:
            //
            //<w:p w:rsidR="005C743F"
            //     w:rsidRDefault="005C743F"
            //     w:rsidP="005C743F">
            //    <w:r>
            //        <w:t xml:space="preserve">This line contains </w:t>
            //    </w:r>
            //    <w:del w:id="8"
            //           w:author="Eric White"
            //           w:date="2008-04-27T08:37:00Z">
            //        <w:r w:rsidDel="005C743F">
            //            <w:delText xml:space="preserve">deleted </w:delText>
            //        </w:r>
            //    </w:del>
            //    <w:r>
            //        <w:t>text.</w:t>
            //    </w:r>
            //</w:p>
            //
            // After:
            //
            //<w:p w:rsidR="005C743F"
            //     w:rsidRDefault="005C743F"
            //     w:rsidP="005C743F">
            //    <w:r>
            //        <w:t xml:space="preserve">This line contains </w:t>
            //    </w:r>
            //    <w:r>
            //        <w:t>text.</w:t>
            //    </w:r>
            //</w:p>

            // The Remove extension method uses snapshot semantics.
            //xDoc.Descendants(w + "p")
            //    .Elements(w + "del")
            //    .Remove();
            xDoc.Descendants(w + "del")
                .Remove();

            // Currently this code doesn't handle:
            // w:tblStylePr/w:trPr/w:del
            // w:style/w:trPr/w:del
            // MathML
            // Smart tags
            // Custom XML

            // I don't believe that the following is strictly necessary, but not sure.  I notice that if
            // you remove all rows from a table, and open and save using Word 2007, tables with no rows are deleted.
            // In any case, remove the tables that no longer have rows.
            xDoc.Descendants(w + "tbl")
                .Where(x => !x.Elements(w + "tr").Any())
                .Remove();

            // Accept moved paragraphs.
            // Find all w:p/w:moveFrom elements, and remove the w:p element.
            // Remove all w:moveFromRangeEnd elements
            // Find all w:p/w:moveTo elements, remove the w:moveTo elements, and promote their children to
            // be children of the w:p element
            // Remove all w:moveToRangeStart and w:moveToRangeEnd elements
            foreach (var x in xDoc.Descendants(w + "p").Elements(w + "moveFrom").ToList())
            {
                var p = x.Ancestors(w + "p").First();
                p.Remove();
            }
            xDoc.Descendants(w + "moveFromRangeEnd").Remove();
            foreach (var x in xDoc.Descendants(w + "p")
                                  .Elements(w + "moveTo")
                                  .ToList())
                x.ReplaceWith(x.Nodes());
            xDoc.Descendants(w + "moveToRangeStart").Remove();
            xDoc.Descendants(w + "moveToRangeEnd").Remove();

            //using (XmlWriter xw = XmlWriter.Create(part.GetStream(FileMode.Create, FileAccess.Write)))
            //{
            //    xDoc.Save(xw);
            //}
            //xDoc.Save("Office Open XML Part 1.xml", SaveOptions.None);
        }

        /// <summary>
        /// Accepts all text change revisions in the document
        /// </summary>
        public void AcceptAll()
        {
            MainDocumentPart mainPart = parentDocument.Document.MainDocumentPart;
            AcceptRevisionsForPart(mainPart);
            foreach (var p in mainPart.HeaderParts)
                AcceptRevisionsForPart(p);
            foreach (var p in mainPart.FooterParts)
                AcceptRevisionsForPart(p);
        }

        /// <summary>
        /// Rejects all text change revisions in the document
        /// </summary>
        public void RejectAll()
        {
            // Removes insertion tracking elements
            InsElements().Remove();

            // Removes text deletion tracking elements
            var delTextElements = DelTextElements().ToList();
            foreach (var delTextElement in delTextElements)
            {
                delTextElement.AddAfterSelf(
                    new XElement(ns + "t", delTextElement.Value)
                );
            }
            delTextElements.Remove();

            // Removes deletion tracking elements
            var delElements = DelElements().ToList();
            foreach (var delElement in delElements)
            {
                delElement.AddAfterSelf(
                    delElement.Nodes()
                );
            }
            delElements.Remove();

            // Removes deletion tracking attributes
            DelAttributes().Remove();
        }
    }
}