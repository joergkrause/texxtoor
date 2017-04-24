/***************************************************************************

Copyright (c) Microsoft Corporation 2008.

This code is licensed using the Microsoft Public License (Ms-PL).  The text of the license can be found here:

http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx

***************************************************************************/

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;

namespace OpenXml.PowerTools.Wordprocessing
{
    /// <summary>
    /// Provides access to background operations
    /// </summary>
    public class BackgroundAccessor
    {
        private PTWordprocessingDocument parentDocument;

        private const string defaultBackgroundColor = "FFFFFF";
        private const string defaultBWMode = "white";
        private const string defaultTargetScreenSize = "800,600";
        private const string defaultVmlBackgroundImageId = "_x0000_s1025";
        private const string defaultImageRecolor = "t";
        private const string defaultImageType = "frame";

        private static XNamespace ns;
        private static XNamespace vmlns;
        private static XNamespace officens;
        private static XNamespace relationshipsns;

        static BackgroundAccessor()
        {
            ns = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
            vmlns = "urn:schemas-microsoft-com:vml";
            officens = "urn:schemas-microsoft-com:office:office";
            relationshipsns = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="document">Document to perform operations on</param>
        public BackgroundAccessor(PTWordprocessingDocument document)
        {
            parentDocument = document;
        }

        /// <summary>
        /// Nodes list with background elements
        /// </summary>
        private XElement BackgroundElement()
        {
            XDocument mainDocument = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart);
            XElement result = mainDocument.Descendants(ns + "background").FirstOrDefault();
            return result;
        }

        /// <summary>
        /// Nodes list with background elements
        /// </summary>
        private XElement BackgroundFillElement()
        {
            XDocument mainDocument = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart);
            XElement result = mainDocument.Descendants(ns + "background").Descendants(vmlns + "fill").FirstOrDefault();
            return result;
        }

        /// <summary>
        /// Sets the document background color
        /// </summary>
        /// <param name="colorValue">String representation of the hexadecimal RGB color</param>
        public void SetColor(string colorValue)
        {
            XDocument mainDocument = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart);

            // If the background element already exists, deletes it
            XElement backgroundElement = BackgroundElement();
            if (backgroundElement != null)
                backgroundElement.Remove();

            mainDocument.Root.AddFirst(
                new XElement(ns + "background",
                    new XAttribute(ns + "color", colorValue)
                )
            );

            // Enables background displaying by adding "displayBackgroundShape" tag
            if (parentDocument.Setting.DisplayBackgroundShapeElements() == null)
                parentDocument.Setting.AddBackgroundShapeElement();
        }

        /// <summary>
        /// Sets the document background image
        /// </summary>
        /// <param name="imagePath">Path of the background image</param>
        public void SetImage(string imagePath)
        {
            XDocument mainDocument = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart);

            // Adds the image to the package
            ImagePart imagePart = parentDocument.Document.MainDocumentPart.AddImagePart(ImagePartType.Bmp);
            Stream imageStream = new StreamReader(imagePath).BaseStream;
            byte[] imageBytes = new byte[imageStream.Length];
            imageStream.Read(imageBytes, 0, imageBytes.Length);
            imagePart.GetStream().Write(imageBytes, 0, imageBytes.Length);

            // Creates a "background" element relating the image and the document

            // If the background element already exists, deletes it
            XElement backgroundElement = BackgroundElement();
            if (backgroundElement != null)
                backgroundElement.Remove();

            // Background element construction
            mainDocument.Root.Add(
                new XElement(ns + "background",
                    new XAttribute(ns + "color", defaultBackgroundColor),
                    new XElement(vmlns + "background",
                        new XAttribute(vmlns + "id", defaultVmlBackgroundImageId),
                        new XAttribute(officens + "bwmode", defaultBWMode),
                        new XAttribute(officens + "targetscreensize", defaultTargetScreenSize),
                        new XElement(vmlns + "fill",
                            new XAttribute(relationshipsns + "id", parentDocument.Document.MainDocumentPart.GetIdOfPart(imagePart)),
                            new XAttribute("recolor", defaultImageRecolor),
                            new XAttribute("type", defaultImageType)
                        )
                    )
                )
            );


            // Enables background displaying by adding "displayBackgroundShape" tag
            if (parentDocument.Setting.DisplayBackgroundShapeElements() == null)
                parentDocument.Setting.AddBackgroundShapeElement();
        }

        /// <summary>
        /// Gets the document background color
        /// </summary>
        /// <returns>string representation of the hexadecimal rgb color</returns>
        public string GetColor()
        {
            XElement backgroundElement = BackgroundElement();
            return (backgroundElement == null) ?
                string.Empty :
                backgroundElement.Attribute(ns + "color").Value;
        }

        /// <summary>
        /// Gets the document background image
        /// </summary>
        /// <returns>string path of the background image</returns>
        public string GetImage()
        {
            string imageName;
            string imageRelationshipId;
            OpenXmlPart imagePart;
            XElement fillElement = BackgroundFillElement();

            if (fillElement != null)
            {
                imageRelationshipId = fillElement.Attribute(relationshipsns + "id").Value;
                imagePart = parentDocument.Document.MainDocumentPart.GetPartById(imageRelationshipId);

                // Gets the image name (path stripped)
                string imagePath = imagePart.Uri.OriginalString;
                imageName = imagePath.Substring(imagePath.LastIndexOf('/')+1);

                // Writes the image outside the package
                OpenXmlDocument.SavePartAs(imagePart, imageName);

                // Returns the path of the place where we wrote the image.
                return Path.GetFullPath(imageName);
            }
            else
                return string.Empty;
        }
    }
}
