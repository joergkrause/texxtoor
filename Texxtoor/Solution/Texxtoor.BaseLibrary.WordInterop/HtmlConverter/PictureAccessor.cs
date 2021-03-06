/***************************************************************************

Copyright (c) Microsoft Corporation 2008.

This code is licensed using the Microsoft Public License (Ms-PL).  The text of the license can be found here:

http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx

***************************************************************************/

using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using DocumentFormat.OpenXml.Packaging;
using System;
using System.Drawing.Imaging;
using System.Linq;

namespace OpenXml.PowerTools.Wordprocessing
{
    /// <summary>
    /// Provides access to picture operations
    /// </summary>
    public class PictureAccessor
    {
        private PTWordprocessingDocument parentDocument;

        private static XNamespace mainns;
        private MainDocumentPart parentPart;
        private static XNamespace wordprocessingDrawingns;
        private static XNamespace drawingmlMainns;
        private static XNamespace picturens;
        private static XNamespace relationshipns;
        private const int pixelsPerEmu = 9525;

        static PictureAccessor() {
            mainns = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
            wordprocessingDrawingns = "http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing";
            drawingmlMainns = "http://schemas.openxmlformats.org/drawingml/2006/main";
            picturens = "http://schemas.openxmlformats.org/drawingml/2006/picture";
            relationshipns = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="document">Document to perform operations on</param>
        public PictureAccessor(PTWordprocessingDocument document)
        {
            parentDocument = document;
            parentPart = document.Document.MainDocumentPart;
        }

        /// <summary>
        /// Insert a picture into a given xmlpath inside the document part
        /// </summary>
        /// <param name="xpathInsertionPoint">place where we are going to put the picture</param>
        /// <param name="pictureToInsert">picture to insert</param>
        /// <param name="name">name to use for inserted picture</param>
        public void Insert(string xpathInsertionPoint, Image pictureToInsert, string name)
        {
            XDocument xmlMainDocument = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart);
            ImagePart picturePart = null;
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("w", mainns.NamespaceName);
            IEnumerable<XElement> insertionPoints = xmlMainDocument.XPathSelectElements(xpathInsertionPoint, namespaceManager);

            //make the insertion for each insertion point specified in the xpath query
            foreach (XElement insertionPoint in insertionPoints)
            {
                if (picturePart == null)
                {
                    //  Create the picture part in the package
                    picturePart = parentDocument.Document.MainDocumentPart.AddImagePart(GetImagePartType(pictureToInsert.RawFormat));
                }

                //  the pictures in the main document part goes in a very long xml, wich specifies the way the picture
                //  has to be placed using drawingXml.
                insertionPoint.AddAfterSelf(
                    new XElement(mainns + "p",
                        new XElement(mainns + "r",
                            new XElement(mainns + "drawing",
                                new XElement(wordprocessingDrawingns + "inline",
                                    new XElement(wordprocessingDrawingns + "extent",
                                        new XAttribute("cx", pictureToInsert.Width * pixelsPerEmu),
                                        new XAttribute("cy", pictureToInsert.Height * pixelsPerEmu)
                                    ),
                                    new XElement(wordprocessingDrawingns + "docPr",
                                        new XAttribute("name", name),
                                        new XAttribute("id", "1")
                                    ),
                                    new XElement(drawingmlMainns + "graphic",
                                        new XAttribute(XNamespace.Xmlns + "a", drawingmlMainns.NamespaceName),
                                        new XElement(drawingmlMainns + "graphicData",
                                            new XAttribute("uri", picturens.NamespaceName),
                                            new XElement(picturens + "pic",
                                                new XAttribute(XNamespace.Xmlns + "pic", picturens.NamespaceName),
                                                new XElement(picturens + "nvPicPr",
                                                    new XElement(picturens + "cNvPr",
                                                        new XAttribute("id", "0"),
                                                        new XAttribute("name", name)
                                                    ),
                                                    new XElement(picturens + "cNvPicPr")
                                                ),
                                                new XElement(picturens + "blipFill",
                                                    new XElement(drawingmlMainns + "blip",
                                                        new XAttribute(relationshipns + "embed", parentPart.GetIdOfPart(picturePart))
                                                    ),
                                                    new XElement(drawingmlMainns + "stretch",
                                                        new XElement(drawingmlMainns + "fillRect")
                                                    )
                                                ),
                                                new XElement(picturens + "spPr",
                                                    new XElement(drawingmlMainns + "xfrm",
                                                        new XElement(drawingmlMainns + "off",
                                                            new XAttribute("x", "0"),
                                                            new XAttribute("y", "0")
                                                        ),
                                                        new XElement(drawingmlMainns + "ext",
                                                            new XAttribute("cx", pictureToInsert.Width * pixelsPerEmu),
                                                            new XAttribute("cy", pictureToInsert.Height * pixelsPerEmu)
                                                        )
                                                    ),
                                                    new XElement(drawingmlMainns + "prstGeom",
                                                        new XAttribute("prst", "rect")
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                );
            }
            if (picturePart != null)
            {
                Stream partStream = picturePart.GetStream(FileMode.Create, FileAccess.ReadWrite);
                pictureToInsert.Save(partStream, pictureToInsert.RawFormat);
            }
            else
                throw new Exception("The xpath query did not return a valid location.");
        }

        
        /// <summary>
        /// Gets the image type representation for a mimetype
        /// </summary>
        /// <param name="format">Content mimetype</param>
        /// <returns>Image type</returns>
        private ImagePartType GetImagePartType(ImageFormat format)
        {
            if (format.Equals(ImageFormat.Jpeg))
                return ImagePartType.Jpeg;
            else if (format.Equals(ImageFormat.Emf))
                return ImagePartType.Emf;
            else if (format.Equals(ImageFormat.Gif))
                return ImagePartType.Gif;
            else if (format.Equals(ImageFormat.Icon))
                return ImagePartType.Icon;
            else if (format.Equals(ImageFormat.Png))
                return ImagePartType.Png;
            else if (format.Equals(ImageFormat.Tiff))
                return ImagePartType.Tiff;
            else if (format.Equals(ImageFormat.Wmf))
                return ImagePartType.Wmf;
            else
                return ImagePartType.Bmp;
        }
    }
}

