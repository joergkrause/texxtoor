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
using System.IO;
using System.Linq;
using System.Xml.Linq;
using OpenXmlSDK = DocumentFormat.OpenXml.Packaging;
using System.IO.Packaging;
using System;
using System.Xml;

namespace OpenXml.PowerTools
{
    /// <summary>
    /// Class for managing the Shared Strings part for a SpreadSheetML document
    /// </summary>
    public class SharedStringsAccesor
    {
        private SpreadsheetDocument parentDocument;

        /// <summary>
        /// Public constructor
        /// </summary>
        /// <param name="parent">Open Xml document containing this Shared Strings part</param>
        public SharedStringsAccesor(SpreadsheetDocument parent)
        {
            parentDocument = parent;
        }

        /// <summary>
        /// Returns the value for a specific shared string
        /// </summary>
        /// <param name="index">Index for shared string to be returned</param>
        /// <returns></returns>
        public string GetSharedString(int index)
        {
            XDocument sharedStringsXDocument = XDocument.Load(new XmlTextReader(parentDocument.Document.WorkbookPart.SharedStringTablePart.GetStream()));
            return sharedStringsXDocument.Root.Elements().ElementAt<XElement>(index).Value;
        }

    }
}
