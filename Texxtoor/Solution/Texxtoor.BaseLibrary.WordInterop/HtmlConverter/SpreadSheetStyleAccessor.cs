/***************************************************************************

Copyright (c) Microsoft Corporation 2008.

This code is licensed using the Microsoft Public License (Ms-PL).  The text of the license can be found here:

http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx

***************************************************************************/
/**************************************************************************
Author          : Johann Granados
Company         : Staff DotNet
Email           : johann.granados@staffdotnet.com
Blog            : http://blogs.staffdotnet.com/johanngranados
Creation Date   : 9/2/2008
**************************************************************************/
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml;
using DocumentFormat.OpenXml.Packaging;
using System.Collections.ObjectModel;
using System;

namespace OpenXml.PowerTools.SpreadSheet
{
    /// <summary>
    /// Class to manage the Style Part for an SpreadSheetML document
    /// </summary>
    public class SpreadSheetStyleAccessor
    {
        private SpreadsheetDocument parentDocument;
        private static XNamespace ns;

        /// <summary>
        /// Static constructor
        /// </summary>
        static SpreadSheetStyleAccessor()
        {
            ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        }

        /// <summary>
        /// Public constructor
        /// </summary>
        /// <param name="parent">OpenXml Document containing this style part accesor</param>
        public SpreadSheetStyleAccessor(SpreadsheetDocument parent)
        {
            parentDocument = parent;
        }

        /// <summary>
        /// XDocument containing Xml content of the styles part
        /// </summary>
        public XDocument GetStylesDocument()
        { 
            if (parentDocument.Document.WorkbookPart.WorkbookStylesPart != null)
                return parentDocument.GetXDocument(parentDocument.Document.WorkbookPart.WorkbookStylesPart);
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
                    WorkbookStylesPart stylesPart =
                        parentDocument.Document.WorkbookPart.AddNewPart<WorkbookStylesPart>();
                    stylesDocument = parentDocument.GetXDocument(stylesPart);
                }
                if (stylesDocument.Root == null)
                    stylesDocument.Add(newStylesDocument.Root);
                else
                    stylesDocument.Root.ReplaceWith(newStylesDocument.Root);
            }
            catch (XmlException ex)
            {
                throw new Exception("File specified is not a valid XML file", ex);
            }
        }

        /// <summary>
        /// Returns the index inside the style part for a specific cell style
        /// </summary>
        /// <param name="cellStyle">Name for cell style to return the index of</param>
        /// <returns></returns>
        public int GetCellStyleIndex(string cellStyle)
        {
            XDocument stylesXDocument = GetStylesDocument();
            var cellStyleXElement =
                stylesXDocument.Root
                .Element(ns + "cellStyles")
                .Elements(ns + "cellStyle")
                .Where(c=> c.Attribute("name").Value.ToLower().Equals(cellStyle.ToLower())).FirstOrDefault<XElement>();

            if (cellStyleXElement != null)
            {
                return System.Convert.ToInt32(cellStyleXElement.Attribute("xfId").Value);
            }
            else
            {
                return -1;
            }
        }

    }
}
