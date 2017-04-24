/***************************************************************************

Copyright (c) Microsoft Corporation 2009.

This code is licensed using the Microsoft Public License (Ms-PL).  The text of the license can be found here:

http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx

***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenXml.PowerTools
{
    /// <summary>
    /// Contains extension methods to modify Open XML Documents
    /// </summary>
    public static class PowerToolsExtensions
    {
        /// <summary>
        /// Removes personal information from the document.
        /// </summary>
        /// <param name="document"></param>
        public static void RemovePersonalInformation(this OpenXmlPackage document)
        {
            WordprocessingDocument doc = document as WordprocessingDocument;
            if (doc == null)
                throw new InvalidOperationException("Removal of personal information only supported for Wordprocessing documents.");

            XNamespace x = "http://schemas.openxmlformats.org/officeDocument/2006/extended-properties";
            XDocument extendedFileProperties = doc.ExtendedFilePropertiesPart.GetXDocument();
            extendedFileProperties.Elements(x + "Properties").Elements(x + "Company").Remove();
            XElement totalTime = extendedFileProperties.Elements(x + "Properties").Elements(x + "TotalTime").FirstOrDefault();
            if (totalTime != null)
                totalTime.Value = "0";

            XNamespace dc = "http://purl.org/dc/elements/1.1/";
            XNamespace cp = "http://schemas.openxmlformats.org/package/2006/metadata/core-properties";
            XDocument coreFileProperties = doc.CoreFilePropertiesPart.GetXDocument();
            foreach (var textNode in coreFileProperties.Elements(cp + "coreProperties")
                                                       .Elements(dc + "creator")
                                                       .Nodes()
                                                       .OfType<XText>())
                textNode.Value = "";
            foreach (var textNode in coreFileProperties.Elements(cp + "coreProperties")
                                                       .Elements(cp + "lastModifiedBy")
                                                       .Nodes()
                                                       .OfType<XText>())
                textNode.Value = "";
            XElement revision = coreFileProperties.Elements(cp + "coreProperties").Elements(cp + "revision").FirstOrDefault();
            if (revision != null)
                revision.Value = "1";

            // add w:removePersonalInformation, w:removeDateAndTime to DocumentSettingsPart
            XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
            XDocument documentSettings = doc.MainDocumentPart.DocumentSettingsPart.GetXDocument();
            // add the new elements in the right position.  Add them after the following three elements
            // (which may or may not exist in the xml document).
            XElement settings = documentSettings.Root;
            XElement lastOfTop3 = settings.Elements()
                .Where(e => e.Name == w + "writeProtection" ||
                    e.Name == w + "view" ||
                    e.Name == w + "zoom")
                .InDocumentOrder()
                .LastOrDefault();
            if (lastOfTop3 == null)
            {
                // none of those three exist, so add as first children of the root element
                settings.AddFirst(
                    settings.Elements(w + "removePersonalInformation").Any() ?
                        null :
                        new XElement(w + "removePersonalInformation"),
                    settings.Elements(w + "removeDateAndTime").Any() ?
                        null :
                        new XElement(w + "removeDateAndTime")
                );
            }
            else
            {
                // one of those three exist, so add after the last one
                lastOfTop3.AddAfterSelf(
                    settings.Elements(w + "removePersonalInformation").Any() ?
                        null :
                        new XElement(w + "removePersonalInformation"),
                    settings.Elements(w + "removeDateAndTime").Any() ?
                        null :
                        new XElement(w + "removeDateAndTime")
                );
            }
        }

        private static string StringConcatenate<T>(this IEnumerable<T> source, Func<T, string> func)
        {
            StringBuilder sb = new StringBuilder();
            foreach (T item in source)
                sb.Append(func(item));
            return sb.ToString();
        }

        private static string StringConcatenate<T>(this IEnumerable<T> source, Func<T, string> func, string separator)
        {
            StringBuilder sb = new StringBuilder();
            foreach (T item in source)
                sb.Append(func(item)).Append(separator);
            if (sb.Length > separator.Length)
                sb.Length -= separator.Length;
            return sb.ToString();
        }

        static string ContainsAnyStyles(IEnumerable<string> stylesToSearch, IEnumerable<string> searchStrings)
        {
            if (searchStrings == null)
                return null;
            foreach (var style in stylesToSearch)
                foreach (var s in searchStrings)
                    if (String.Compare(style, s, true) == 0)
                        return s;
            return null;
        }

        static string ContainsAnyContent(string stringToSearch, IEnumerable<string> searchStrings,
            IEnumerable<Regex> regularExpressions, bool isRegularExpression, bool caseInsensitive)
        {
            if (searchStrings == null)
                return null;
            if (isRegularExpression)
            {
                foreach (var r in regularExpressions)
                    if (r.IsMatch(stringToSearch))
                        return r.ToString();
            }
            else
                if (caseInsensitive)
                {
                    foreach (var s in searchStrings)
                        if (stringToSearch.ToLower().Contains(s.ToLower()))
                            return s;
                }
                else
                {
                    foreach (var s in searchStrings)
                        if (stringToSearch.Contains(s))
                            return s;
                }

            return null;
        }

        static IEnumerable<string> GetAllStyleIdsAndNames(WordprocessingDocument doc, string styleId)
        {
            string localStyleId = styleId;
            XNamespace w =
              "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

            yield return styleId;

            string styleNameForFirstStyle = (string)doc
                .MainDocumentPart
                .StyleDefinitionsPart
                .GetXDocument()
                .Root
                .Elements(w + "style")
                .Where(e => (string)e.Attribute(w + "type") == "paragraph" &&
                    (string)e.Attribute(w + "styleId") == styleId)
                .Elements(w + "name")
                .Attributes(w + "val")
                .FirstOrDefault();

            if (styleNameForFirstStyle != null)
                yield return styleNameForFirstStyle;

            while (true)
            {
                XElement style = doc
                    .MainDocumentPart
                    .StyleDefinitionsPart
                    .GetXDocument()
                    .Root
                    .Elements(w + "style")
                    .Where(e => (string)e.Attribute(w + "type") == "paragraph" &&
                        (string)e.Attribute(w + "styleId") == localStyleId)
                    .FirstOrDefault();

                if (style == null)
                    yield break;

                var basedOn = (string)style
                    .Elements(w + "basedOn")
                    .Attributes(w + "val")
                    .FirstOrDefault();

                if (basedOn == null)
                    yield break;

                yield return basedOn;

                XElement basedOnStyle = doc
                    .MainDocumentPart
                    .StyleDefinitionsPart
                    .GetXDocument()
                    .Root
                    .Elements(w + "style")
                    .Where(e => (string)e.Attribute(w + "type") == "paragraph" &&
                        (string)e.Attribute(w + "styleId") == basedOn)
                    .FirstOrDefault();

                string basedOnStyleName = (string)basedOnStyle
                    .Elements(w + "name")
                    .Attributes(w + "val")
                    .FirstOrDefault();


                if (basedOnStyleName != null)
                    yield return basedOnStyleName;

                localStyleId = basedOn;
            }
        }
        private static IEnumerable<string> GetInheritedStyles(WordprocessingDocument doc, string styleName)
        {
            string localStyleName = styleName;
            XNamespace w =
              "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

            yield return styleName;
            while (true)
            {
                XElement style = doc
                    .MainDocumentPart
                    .StyleDefinitionsPart
                    .GetXDocument()
                    .Root
                    .Elements(w + "style")
                    .Where(e => (string)e.Attribute(w + "type") == "paragraph" &&
                        (string)e.Element(w + "name").Attribute(w + "val") == localStyleName)
                    .FirstOrDefault();

                if (style == null)
                    yield break;

                var basedOn = (string)style
                    .Elements(w + "basedOn")
                    .Attributes(w + "val")
                    .FirstOrDefault();

                if (basedOn == null)
                    yield break;

                yield return basedOn;
                localStyleName = basedOn;
            }
        }

        /// <summary>
        /// Search document for paragraphs using a particular style or containing particular text.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="styleSearchString"></param>
        /// <param name="contentSearchString"></param>
        /// <param name="isRegularExpression"></param>
        /// <param name="caseInsensitive"></param>
        /// <returns></returns>
        static public MatchInfo[] SearchInDocument(this OpenXmlPackage document,
            IEnumerable<string> styleSearchString, IEnumerable<string> contentSearchString,
            bool isRegularExpression, bool caseInsensitive)
        {
            WordprocessingDocument doc = document as WordprocessingDocument;
            if (doc == null)
                throw new InvalidOperationException("Searching only supported for Wordprocessing documents.");

            XNamespace w =
              "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
            XName r = w + "r";
            XName ins = w + "ins";

            RegexOptions options;
            Regex[] regularExpressions = null;
            if (isRegularExpression && contentSearchString != null)
            {
                if (caseInsensitive)
                    options = RegexOptions.IgnoreCase | RegexOptions.Compiled;
                else
                    options = RegexOptions.Compiled;
                regularExpressions = contentSearchString
                    .Select(s => new Regex(s, options)).ToArray();
            }

            var defaultStyleName = (string)doc
                .MainDocumentPart
                .StyleDefinitionsPart
                .GetXDocument()
                .Root
                .Elements(w + "style")
                .Where(style =>
                    (string)style.Attribute(w + "type") == "paragraph" &&
                    (string)style.Attribute(w + "default") == "1")
                .First()
                .Attribute(w + "styleId");

            var q1 = doc
                .MainDocumentPart
                .GetXDocument()
                .Root
                .Element(w + "body")
                .Elements()
                .Select((p, i) =>
                {
                    var styleNode = p
                        .Elements(w + "pPr")
                        .Elements(w + "pStyle")
                        .FirstOrDefault();
                    var styleName = styleNode != null ?
                        (string)styleNode.Attribute(w + "val") :
                        defaultStyleName;
                    return new
                    {
                        Element = p,
                        Index = i,
                        StyleName = styleName
                    };
                }
                );

            var q2 = q1
                .Select(i =>
                {
                    string text = null;
                    if (i.Element.Name == w + "p")
                        text = i.Element.Elements()
                            .Where(z => z.Name == r || z.Name == ins)
                            .Descendants(w + "t")
                            .StringConcatenate(element => (string)element);
                    else
                        text = i.Element
                            .Descendants(w + "p")
                            .StringConcatenate(p => p
                                .Elements()
                                .Where(z => z.Name == r || z.Name == ins)
                                .Descendants(w + "t")
                                .StringConcatenate(element => (string)element),
                                Environment.NewLine
                            );

                    return new
                    {
                        Element = i.Element,
                        StyleName = i.StyleName,
                        Index = i.Index,
                        Text = text
                    };
                }
                );

            var q3 = q2
                .Select(i =>
                    new MatchInfo
                    {
                        ElementNumber = i.Index + 1,
                        Content = i.Text,
                        Style = ContainsAnyStyles(GetAllStyleIdsAndNames(doc, i.StyleName).Distinct(), styleSearchString),
                        Pattern = ContainsAnyContent(i.Text, contentSearchString, regularExpressions, isRegularExpression, caseInsensitive),
                        IgnoreCase = caseInsensitive
                    }
                )
                .Where(i => (styleSearchString == null || i.Style != null) && (contentSearchString == null || i.Pattern != null));
            return q3.ToArray();
        }
    }

    /// <summary>
    /// Match information for Select-OpenXmlString
    /// </summary>
    public class MatchInfo {
      /// <summary>
      /// Full path of file that matched
      /// </summary>
      public string Path { get; set; }
      /// <summary>
      /// Filename of file that matched
      /// </summary>
      public string Filename {
        get {
          if (Path == null)
            return null;
          FileInfo info = new FileInfo(Path);
          return info.Name;
        }
      }
      /// <summary>
      /// Element number of element whose style or content matched
      /// </summary>
      public int ElementNumber { get; set; }
      /// <summary>
      /// Full contents, without formatting, of matched element
      /// </summary>
      public string Content { get; set; }
      /// <summary>
      /// The first style that matched the element
      /// </summary>
      public string Style { get; set; }
      /// <summary>
      /// The first pattern that matched the content of the element
      /// </summary>
      public string Pattern { get; set; }
      /// <summary>
      /// Indicates if case was ignored on the pattern match
      /// </summary>
      public bool IgnoreCase { get; set; }

      /// <summary>
      /// Simple constructor
      /// </summary>
      public MatchInfo() {
      }
    }
}