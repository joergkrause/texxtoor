/***************************************************************************

Copyright (c) Microsoft Corporation 2008.

This code is licensed using the Microsoft Public License (Ms-PL).  The text of the license can be found here:

http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx

***************************************************************************/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;


namespace OpenXml.PowerTools.Wordprocessing
{
    /// <summary>
    /// Provides access to watermark operations
    /// </summary>
    public class WatermarkAccesor
    {
        private PTWordprocessingDocument parentDocument;

        private static XNamespace ns;
        private static XNamespace officens;
        private static XNamespace vmlns;
        private static XNamespace relationshipsns;
        private string diagonalWatermarkStyle = "position:absolute;margin-left:0;margin-top:0;width:527.85pt;height:131.95pt;rotation:315;z-index:-251656192;mso-position-horizontal:center;mso-position-horizontal-relative:margin;mso-position-vertical:center;mso-position-vertical-relative:margin";
        private string defaultWatermarkStyle = "position:absolute;margin-left:0;margin-top:0;width:468pt;height:117pt;z-index:-251652096;mso-position-horizontal:center;mso-position-horizontal-relative:margin;mso-position-vertical:center;mso-position-vertical-relative:margin";

        static WatermarkAccesor()
        {
            ns = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
            officens = "urn:schemas-microsoft-com:office:office";
            vmlns = "urn:schemas-microsoft-com:vml";
            relationshipsns = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="document">Document to perform operations on</param>
        public WatermarkAccesor(PTWordprocessingDocument document)
        {
            parentDocument = document;
        }

        /// <summary>
        /// Inserts a watermark text inside a document
        /// </summary>
        /// <param name="watermarkText">text to show in the watermark</param>
        /// <param name="diagonalOrientation">specify that the text orientation will be in a diagonal way</param>
        public void InsertWatermark(string watermarkText, bool diagonalOrientation)
        {
            Collection<XDocument> headers = new Collection<XDocument>();

            if (parentDocument.Headers.GetHeaderReference(HeaderType.First) == null)
                headers.Add(parentDocument.Headers.AddNewHeader(HeaderType.First));
            else
                headers.Add(parentDocument.Headers.GetHeader(HeaderType.First));

            if (parentDocument.Headers.GetHeaderReference(HeaderType.Even) == null)
                headers.Add(parentDocument.Headers.AddNewHeader(HeaderType.Even));
            else
                headers.Add(parentDocument.Headers.GetHeader(HeaderType.Even));

            if (parentDocument.Headers.GetHeaderReference(HeaderType.Default) == null)
                headers.Add(parentDocument.Headers.AddNewHeader(HeaderType.Default));
            else
                headers.Add(parentDocument.Headers.GetHeader(HeaderType.Default));

            foreach (XDocument header in headers) {
                var runElement = header.Descendants(ns + "r").FirstOrDefault();
                if (runElement == null) {
                    header.Root.Add(
                        new XElement(ns + "sdt",
                            new XElement(ns + "sdtContent",
                                new XElement(ns + "p",
                                    new XElement(ns + "pPr",
                                        new XElement(ns + "pStyle",
                                            new XAttribute(ns + "val", "Header")
                                        )
                                    ),
                                    runElement = new XElement(ns + "r")
                                )
                            )
                        )
                    );

                }
                runElement.AddBeforeSelf(CreateWatermarkVml(watermarkText, diagonalOrientation));
            }
        }

        /// <summary>
        /// Creates the markup for watermark displaying
        /// </summary>
        /// <param name="watermarkText">Text to include in markup</param>
        /// <param name="diagonalOrientation">Orientation of text</param>
        /// <returns>Watermark markup</returns>
        private XElement CreateWatermarkVml(string watermarkText, bool diagonalOrientation)
        {
                return new XElement(ns + "r",
                    new XElement(ns + "pict",
                        new XElement(vmlns + "shapetype",
                            new XAttribute("id", "_x0000_t136"),
                            new XAttribute("coordsize", "21600,21600"),
                            new XAttribute(officens + "spt", "136"),
                            new XAttribute("adj", "10800"),
                            new XAttribute("path", "m@7,l@8,m@5,21600l@6,21600e"),
                            new XElement(vmlns + "formulas",
                                new XElement(vmlns + "f",
                                    new XAttribute("eqn", "sum #0 0 10800")
                                ),
                                new XElement(vmlns + "f",
                                    new XAttribute("eqn", "prod #0 2 1")
                                ),
                                new XElement(vmlns + "f",
                                    new XAttribute("eqn", "sum 21600 0 @1")
                                ),
                                new XElement(vmlns + "f",
                                    new XAttribute("eqn", "sum 0 0 @2")
                                ),
                                new XElement(vmlns + "f",
                                    new XAttribute("eqn", "sum 21600 0 @3")
                                ),
                                new XElement(vmlns + "f",
                                    new XAttribute("eqn", "if @0 @3 0")
                                ),
                                new XElement(vmlns + "f",
                                    new XAttribute("eqn", "if @0 21600 @1")
                                ),
                                new XElement(vmlns + "f",
                                    new XAttribute("eqn", "if @0 0 @2")
                                ),
                                new XElement(vmlns + "f",
                                    new XAttribute("eqn", "if @0 @4 21600")
                                ),
                                new XElement(vmlns + "f",
                                    new XAttribute("eqn", "mid @5 @6")
                                ),
                                new XElement(vmlns + "f",
                                    new XAttribute("eqn", "mid @8 @5")
                                ),
                                new XElement(vmlns + "f",
                                    new XAttribute("eqn", "mid @7 @8")
                                ),
                                new XElement(vmlns + "f",
                                    new XAttribute("eqn", "mid @6 @7")
                                ),
                                new XElement(vmlns + "f",
                                    new XAttribute("eqn", "sum @6 0 @5")
                                )
                            ),
                            new XElement(vmlns + "path",
                                new XAttribute("textpathok", "t"),
                                new XAttribute(officens + "connecttype", "custom"),
                                new XAttribute(officens + "connectlocs", "@9,0;@10,10800;@11,21600;@12,10800"),
                                new XAttribute(officens + "connectangles", "270,180,90,0")
                            ),
                            new XElement(vmlns + "textpath",
                                new XAttribute("on", "t"),
                                new XAttribute("fitshape", "t")
                            ),
                            new XElement(vmlns + "handles",
                                new XElement(vmlns + "h",
                                    new XAttribute("position", "#0,bottomRight"),
                                    new XAttribute("xrange", "6629,14971")
                                )
                            ),
                            new XElement(officens + "lock",
                                new XAttribute(vmlns + "ext", "edit"),
                                new XAttribute("text", "t"),
                                new XAttribute("shapetype", "t")
                            )
                        ),
                        new XElement(vmlns + "shape",
                            new XAttribute("id", "PowerPlusWaterMarkObject98078923"),
                            new XAttribute(officens + "spid", "_x0000_s2055"),
                            new XAttribute("type", "#_x0000_t136"),
                            new XAttribute("style", diagonalOrientation ? diagonalWatermarkStyle : defaultWatermarkStyle),
                            new XAttribute(officens + "allowincell", "f"),
                            new XAttribute("fillcolor", "silver"),
                            new XAttribute("stroked", "f"),
                            new XElement(vmlns + "fill",
                                new XAttribute("opacity", ".5")
                            ),
                            new XElement(vmlns + "textpath",
                                new XAttribute("style", "font-family:&quot;Calibri&quot;;font-size:1pt"),
                                new XAttribute("string", watermarkText)
                            )
                        )
                    )
                );
        }

        /// <summary>
        /// Gets the text related to watermark from a document
        /// </summary>
        /// <returns>Watermark text</returns>
        public string GetWatermarkText()
        {
            IEnumerable<XElement> watermarkDescription = GetWatermark();
            if (watermarkDescription != null)
                return
                    watermarkDescription
                    .Descendants(vmlns + "shape")
                    .Descendants(vmlns + "textpath")
                    .First()
                    .Attribute("string")
                    .Value;
            else
                return string.Empty;
        }

        /// <summary>
        /// Gets the document structure related to watermark description
        /// </summary>
        /// <returns>Document structure related to watermark description</returns>
        public IEnumerable<XElement> GetWatermark()
        {
            //  to get the watermark text, we have to look inside the document
            //  get the default header reference and get the header reference id part
            XElement defaultHeaderReference = parentDocument.Headers.GetHeaderReference(HeaderType.Default);
            if (defaultHeaderReference != null)
            {
                string headerReferenceId = defaultHeaderReference.Attribute(relationshipsns + "id").Value;
                OpenXmlPart headerPart = parentDocument.Document.MainDocumentPart.GetPartById(headerReferenceId);
                if (headerPart != null)
                {
                    XDocument headerPartXml = parentDocument.GetXDocument(headerPart);
                    return headerPartXml.Descendants(ns + "pict");
                }
                else
                    return null;
            }
            else
                return null;
        }
    }
}
