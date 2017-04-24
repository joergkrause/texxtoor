/***************************************************************************

Copyright (c) Microsoft Corporation 2008.

This code is licensed using the Microsoft Public License (Ms-PL).  The text of the license can be found here:

http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx

***************************************************************************/

using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;
using System;

namespace OpenXml.PowerTools.Wordprocessing {
  /// <summary>
  /// Performs operations on document custom xml
  /// </summary>
  public class CustomXmlAccessor {
    private PTWordprocessingDocument parentDocument;

    /// <summary>
    /// Class constructor
    /// </summary>
    /// <param name="document">Document to perform operations on</param>
    public CustomXmlAccessor(PTWordprocessingDocument document) {
      parentDocument = document;
    }

    /// <summary>
    /// Searches for a custom Xml part with a given name
    /// </summary>
    /// <param name="xmlPartName">Name of custom Xml part</param>
    /// <returns>XDocument with customXml part loaded</returns>
    public XDocument Find(string xmlPartName) {
      string partName = "/" + xmlPartName;
      var customXmlPart =
          parentDocument.Document.MainDocumentPart.CustomXmlParts.Where(
              t => t.Uri.OriginalString.EndsWith(partName, System.StringComparison.OrdinalIgnoreCase)
          ).FirstOrDefault();
      if (customXmlPart == null)
        throw new ArgumentException("Part name '" + xmlPartName + "' not found.");
      return parentDocument.GetXDocument(customXmlPart);
    }

    /// <summary>
    /// Replaces a previously existing customXml part by another one
    /// </summary>
    /// <param name="customXmlDocument">XDocument of part to replace inside the document package</param>
    /// <param name="partNameOnly">Name of the part.</param>
    public void SetDocument(XDocument customXmlDocument, string partNameOnly) {
      string partName = "/" + partNameOnly;
      var customXmlPart =
          parentDocument.Document.MainDocumentPart.CustomXmlParts.Where(
              t => t.Uri.OriginalString.EndsWith(partName, System.StringComparison.OrdinalIgnoreCase)
          ).FirstOrDefault();

      if (customXmlPart == null)
        customXmlPart = parentDocument.Document.MainDocumentPart.AddCustomXmlPart(CustomXmlPartType.CustomXml);

      XDocument newCustomXmlDocument = parentDocument.GetXDocument(customXmlPart);
      if (newCustomXmlDocument.Root != null)
        newCustomXmlDocument.Root.ReplaceWith(customXmlDocument.Root);
      else
        newCustomXmlDocument.Add(customXmlDocument.Root);
    }
  }
}
