/***************************************************************************

Copyright (c) Microsoft Corporation 2008.

This code is licensed using the Microsoft Public License (Ms-PL).  The text of the license can be found here:

http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx

***************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;
using System.IO;
using OpenXml.PowerTools;

namespace OpenXml.PowerTools.Wordprocessing
{
    /// <summary>
    /// Provides access to comment operations
    /// </summary>
    public class CommentAccessor
    {
        private PTWordprocessingDocument parentDocument;

        private static XNamespace ns;

        static CommentAccessor()
        {
            ns = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="document">Document to perform operations on</param>
        public CommentAccessor(PTWordprocessingDocument document)
        {
            this.parentDocument = document;
        }

        /// <summary>
        /// Returns all reference tags from inside the main part of a wordprocessing document
        /// </summary>
        private IEnumerable<XElement> CommentReferences()
        {
            XDocument mainDocument = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart);
            XName run = ns + "r";
            XName startRange = ns + "commentRangeStart";
            XName endRange = ns + "commentRangeEnd";
            XName commentReference = ns + "commentReference";

            IEnumerable<XElement> results =
                mainDocument.Descendants().Where(
                    tag =>
                        tag.Name == startRange ||
                        tag.Name == endRange ||
                        (tag.Name == run && tag.Descendants(commentReference).Count() > 0)
                );
            return results;
        }

        /// <summary>
        /// Returns all comments included into a document
        /// </summary>
        private IEnumerable<XElement> CommentContents()
        {
            IEnumerable<XElement> results = null;
            WordprocessingCommentsPart commentsPart = parentDocument.Document.MainDocumentPart.WordprocessingCommentsPart;
            if (commentsPart != null)
            {
                XDocument commentsPartDocument = parentDocument.GetXDocument(commentsPart);

                results = commentsPartDocument.Element(ns + "comments").Elements(ns + "comment");
            }
            return results;
        }

        /// <summary>
        /// Gets all of the comments from inside a document in a given format
        /// </summary>
        /// <param name="format">Format to return data</param>
        /// <returns>Comments</returns>
        public object GetAll(CommentFormat format)
        {
            IEnumerable<XElement> comments = CommentContents();
            XDocument commentsDocument =
                new XDocument(
                    new XElement(ns + "comments",
                        new XAttribute(XNamespace.Xmlns + "w", ns),
                        comments
                    )
                );
            if (comments != null)
            {
                switch (format)
                {
                    case CommentFormat.PlainText:
                        return commentsDocument.ToString();
                    case CommentFormat.Xml:
                        return commentsDocument;
                    case CommentFormat.Docx:
                        return CreateCommentDocument(comments);
                    default:
                        return null;
                }
            }
            else
                return null;
        }

        /// <summary>
        /// Creates a Wordprocessing document containing all comments form a given Wordprocessing document
        /// </summary>
        /// <param name="contents">Comment contents</param>
        private PTWordprocessingDocument CreateCommentDocument(IEnumerable<XElement> contents)
        {
            string tempPath = Path.GetTempFileName();
            PTWordprocessingDocument doc = new PTWordprocessingDocument(tempPath);
            doc.InnerContent.SetContent(contents);
            return doc;
        }

        /// <summary>
        /// Removes all of the comments existing in the document
        /// </summary>
        public void RemoveAll()
        {
            //Removes comment-related tags inside the main document part
            IEnumerable<XElement> commentReferences = CommentReferences().ToList();
            commentReferences.Remove();

            WordprocessingCommentsPart commentsPart = parentDocument.Document.MainDocumentPart.WordprocessingCommentsPart;
            if (commentsPart != null)
                parentDocument.RemovePart(commentsPart);
        }
    }
}
