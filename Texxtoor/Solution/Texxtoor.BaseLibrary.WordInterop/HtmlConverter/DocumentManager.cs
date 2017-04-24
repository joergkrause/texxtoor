/***************************************************************************

Copyright (c) Microsoft Corporation 2008.

This code is licensed using the Microsoft Public License (Ms-PL).  The text of the license can be found here:

http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx

***************************************************************************/

using System;
using System.IO;
using System.IO.Packaging;
using System.Xml;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System.Xml.Xsl;

namespace OpenXml.PowerTools
{
    /// <summary>
    /// Provides generic functions to work with documents
    /// </summary>
    public class DocumentManager
    {
        /// <summary>
        /// Document to perform queries or apply changes on
        /// </summary>
        protected internal OpenXmlDocument parentDocument;

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="document">Document to perform operations on</param>
        public DocumentManager(OpenXmlDocument document)
        {
            parentDocument = document;
        }

        /// <summary>
        /// Inserts an Xml fragment into a given file (part) inside a document
        /// </summary>
        /// <param name="xmlPartPath">Path of file part</param>
        /// <param name="xpathInsertionPoint">Point after which xml will be injected</param>
        /// <param name="xmlContent">Xml to insert</param>
        public virtual void InsertXml(string xmlPartPath, string xpathInsertionPoint, string xmlContent)
        {
            try
            {
                Uri xmlPartUri;
                XmlDocument xmlDocument;
                PackagePart xmlPart=null;

                // Searches for the given part
                xmlPartUri = new Uri(xmlPartPath, UriKind.Relative);
                if (parentDocument.Document.Package.PartExists(xmlPartUri))
                {
                    // Loads part contents into an XmlDocument
                    xmlPart = parentDocument.Document.Package.GetPart(xmlPartUri);
                    using (XmlReader xmlReader = XmlReader.Create(xmlPart.GetStream(FileMode.Open, FileAccess.Read)))
                    {
                        try
                        {
                            xmlDocument = new XmlDocument();
                            xmlDocument.Load(xmlReader);
                        }
                        catch (XmlException)
                        {
                            xmlDocument = new XmlDocument();
                        }
                    }

                    // Looks into the XmlDocument for nodes at the specified path
                    XmlNamespaceManager namespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
                    namespaceManager.AddNamespace("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
                    XmlNode insertionPoint =
                        xmlDocument.SelectSingleNode(xpathInsertionPoint, namespaceManager);

                    if (insertionPoint != null)
                    {

                        StringReader r = new StringReader("<w:node xmlns:w='http://schemas.openxmlformats.org/wordprocessingml/2006/main'>" + xmlContent + "</w:node>");

                        XmlNode nodoid = xmlDocument.ReadNode(XmlReader.Create(r));
                        //doc.LoadXml("<w:node xmlns:w='http://schemas.openxmlformats.org/wordprocessingml/2006/main'>" + xmlContent + "</w:node>");


                        //// Inserts new contents into the given part
                        XmlNode xmlNodeToInsert =
                            nodoid;// doc.FirstChild;
                        //    xmlDocument.CreateElement("w","node", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");


                        //xmlNodeToInsert. .InnerXml = xmlContent;
                        XmlNodeList nodes = xmlNodeToInsert.ChildNodes;
                        if (nodes.Count > 0)
                            for (int i = nodes.Count - 1; i >= 0; i--)
                            {
                                XmlNode node = nodes[i];
                                insertionPoint.ParentNode.InsertAfter(node, insertionPoint);
                            }

                        // Writes the XmlDocument back to the part
                        using (XmlWriter writer = XmlWriter.Create(xmlPart.GetStream(FileMode.Create, FileAccess.Write)))
                        {
                            xmlDocument.WriteTo(writer);
                        }
                    }
                    else
                        throw new Exception("Insertion point does not exist");
                }
                else
                    throw new Exception("Specified part does not exist");
            }
            catch (XmlException)
            {

                throw new Exception("Bad formed XML file or fragment");
            }
            catch (IOException)
            {
                throw new Exception("Error accessing file");
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Locks the document from editing
        /// </summary>
        public virtual void Lock() { }

        /// <summary>
        /// Splits the document into multiple output files
        /// </summary>
        public virtual Collection<OpenXmlDocument> Split(string outputPath, string prefix)
        {
            throw new InvalidOperationException("Split not yet supported for this document type.");
        }

        /// <summary>
        /// Transform a word document in a html file.
        /// </summary>
        /// <param name="packing">indicates if the output must be saved in a package</param>
        /// <param name="resourcesPackageName">the name of the output package</param>
        /// <param name="htmlOutputName">the name of the html output file</param>
        /// <param name="outputPath">the path where are going to be placed the output files</param>
        /// <param name="xslFilePath">the xsl file to use for the transformation, insted of the default xsl</param>
        public virtual void TransformToHtml(bool packing, string resourcesPackageName, string htmlOutputName, string outputPath, string xslFilePath, XsltArgumentList arguments = null){}
    }
}
