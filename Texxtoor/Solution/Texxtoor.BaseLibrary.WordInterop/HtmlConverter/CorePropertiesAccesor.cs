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

namespace OpenXml.PowerTools
{
    /// <summary>
    /// Provides access to the custom properties part
    /// </summary>
    public class CorePropertiesAccesor
    {
        OpenXmlDocument parentDocument;

        /// <summary>
        /// Class constructor  
        /// </summary>
        /// <param name="document">Document to perform operations on</param>
        public CorePropertiesAccesor(OpenXmlDocument document)
        {
            parentDocument = document;
        }


        internal void RemoveAll()
        {
            IEnumerable<CoreFilePropertiesPart> corePropertiesParts = parentDocument.Document.GetPartsOfType<CoreFilePropertiesPart>();
            if (corePropertiesParts != null && corePropertiesParts.Count()>0)
                parentDocument.RemovePart(corePropertiesParts.First());
        }
    }
}