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
using System.IO.Packaging;
using System;
using System.Xml;

namespace OpenXml.PowerTools.Wordprocessing
{
    /// <summary>
    /// Provides access to theme operations
    /// </summary>
    public class ThemeAccessor
    {
        private PTWordprocessingDocument parentDocument;

        private static XNamespace ns;
        private static XNamespace drawingns;
        private static XNamespace relationshipns;
        private static string mainDocumentRelationshipType;
        private static string themeRelationshipType;

        static ThemeAccessor()
        {
            ns = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
            drawingns = "http://schemas.openxmlformats.org/drawingml/2006/main";
            relationshipns = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
            mainDocumentRelationshipType = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument";
            themeRelationshipType = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/theme";
        }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="document">Document to perform operations on</param>
        public ThemeAccessor(PTWordprocessingDocument document)
        {
            parentDocument = document;
        }

        /// <summary>
        /// Gets the document theme
        /// </summary>
        public Package GetTheme(string outputPath)
        {
            Package themePackage = null;
            // Loads the theme part main file
            ThemePart theme = parentDocument.Document.MainDocumentPart.ThemePart;
            if (theme != null)
            {
                XDocument themeDocument = parentDocument.GetXDocument(theme);

                // Creates the theme package (thmx file)
                themePackage = Package.Open(outputPath, FileMode.Create);

                // Creates the theme manager part on the new package and loads default content
                PackagePart newThemeManagerPart = themePackage.CreatePart(new Uri("/theme/theme/themeManager.xml", UriKind.RelativeOrAbsolute), "application/vnd.openxmlformats-officedocument.themeManager+xml");
                themePackage.CreateRelationship(newThemeManagerPart.Uri, TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument");
                using (XmlWriter xWriter = XmlWriter.Create(newThemeManagerPart.GetStream(FileMode.Create, FileAccess.Write)))
                {
                    CreateEmptyThemeManager().WriteTo(xWriter);
                    xWriter.Flush();
                }

                // Creates the main theme part
                PackagePart newThemePart = themePackage.CreatePart(new Uri("/theme/theme/" + theme.Uri.OriginalString.Substring(theme.Uri.OriginalString.LastIndexOf('/') + 1), UriKind.RelativeOrAbsolute), theme.ContentType);
                newThemeManagerPart.CreateRelationship(newThemePart.Uri, TargetMode.Internal, theme.RelationshipType);

                // Gets embeded part references
                var embeddedItems =
                    themeDocument
                    .Descendants()
                    .Attributes(relationshipns + "embed");

                foreach (IdPartPair partId in theme.Parts)
                {
                    OpenXmlPart part = partId.OpenXmlPart;

                    // Creates the new media part inside the destination package
                    PackagePart newPart = themePackage.CreatePart(new Uri("/theme/media/" + part.Uri.OriginalString.Substring(part.Uri.OriginalString.LastIndexOf('/') + 1), UriKind.RelativeOrAbsolute), part.ContentType);
                    PackageRelationship relationship =
                        newThemePart.CreateRelationship(newPart.Uri, TargetMode.Internal, part.RelationshipType);

                    // Copies binary content from original part to destination part
                    Stream partStream = part.GetStream(FileMode.Open, FileAccess.Read);
                    Stream newPartStream = newPart.GetStream(FileMode.Create, FileAccess.Write);
                    byte[] fileContent = new byte[partStream.Length];
                    partStream.Read(fileContent, 0, (int)partStream.Length);
                    newPartStream.Write(fileContent, 0, (int)partStream.Length);
                    newPartStream.Flush();

                    // Replaces old embed part reference with the freshly created one
                    XAttribute relationshipAttribute = embeddedItems.FirstOrDefault(e => e.Value == theme.GetIdOfPart(part));
                    if (relationshipAttribute != null)
                        relationshipAttribute.Value = relationship.Id;
                }

                // Writes the updated theme XDocument into the destination package
                using (XmlWriter newThemeWriter = XmlWriter.Create(newThemePart.GetStream(FileMode.Create, FileAccess.Write)))
                    themeDocument.WriteTo(newThemeWriter);

                //themePackage.Flush();
                themePackage.Close();
            }
            return themePackage;
        }

        /// <summary>
        /// Creates an empty theme manager document
        /// </summary>
        private static XDocument CreateEmptyThemeManager() {
            return new XDocument(
                new XElement(drawingns+"themeManager", 
                    new XAttribute(XNamespace.Xmlns + "a", drawingns)
                )
            );
        }

        /// <summary>
        /// Sets the theme part of the document with the package at the given path
        /// </summary>
        /// <param name="themePath">Theme package path</param>
        public void SetTheme(string themePath) {
            try
            {
                Package themePackage = Package.Open(themePath);
                SetTheme(themePackage);
                themePackage.Close();
            }
            catch (Exception) {
                throw;
            }
        }
        
        /// <summary>
        /// Sets the document theme
        /// </summary>
        /// <param name="theme">Theme package</param>
        public void SetTheme(Package theme)
        {
            // Gets the theme manager part
            PackageRelationship themeManagerRelationship =
                theme.GetRelationshipsByType(mainDocumentRelationshipType).FirstOrDefault();
            if (themeManagerRelationship != null)
            {
                PackagePart themeManagerPart = theme.GetPart(themeManagerRelationship.TargetUri);

                // Gets the theme main part
                PackageRelationship themeRelationship =
                    themeManagerPart.GetRelationshipsByType(themeRelationshipType).FirstOrDefault();
                if (themeRelationship != null)
                {
                    PackagePart themePart = theme.GetPart(themeRelationship.TargetUri);
                    XDocument newThemeDocument = XDocument.Load(XmlReader.Create(themePart.GetStream(FileMode.Open, FileAccess.Read)));

                    // Removes existing theme part from document
                    if (parentDocument.Document.MainDocumentPart.ThemePart != null)
                        parentDocument.Document.MainDocumentPart.DeletePart(parentDocument.Document.MainDocumentPart.ThemePart);

                    // Creates a new theme part
                    ThemePart documentThemePart = parentDocument.Document.MainDocumentPart.AddNewPart<ThemePart>();
                    XDocument themeDocument = parentDocument.GetXDocument(documentThemePart);
                    themeDocument.Add(newThemeDocument.Root);

                    var embeddedItems =
                        themeDocument
                        .Descendants()
                        .Attributes(relationshipns + "embed");
                    foreach (PackageRelationship imageRelationship in themePart.GetRelationships())
                    {
                        // Adds an image part to the theme part and stores contents inside
                        PackagePart imagePart = theme.GetPart(imageRelationship.TargetUri);
                        ImagePart newImagePart =
                            documentThemePart.AddImagePart(GetImagePartType(imagePart.ContentType));
                        newImagePart.FeedData(imagePart.GetStream(FileMode.Open, FileAccess.Read));

                        // Updates relationship references into the theme XDocument
                        XAttribute relationshipAttribute = embeddedItems.FirstOrDefault(e => e.Value == imageRelationship.Id);
                        if (relationshipAttribute != null)
                            relationshipAttribute.Value = documentThemePart.GetIdOfPart(newImagePart);
                    }

                    // Updates the theme part with actual contents of the theme XDocument
                    using (XmlWriter newThemeWriter = XmlWriter.Create(documentThemePart.GetStream(FileMode.Create, FileAccess.Write)))
                        themeDocument.WriteTo(newThemeWriter);
                }
            }
        }

        /// <summary>
        /// Gets the image type representation for a mimetype
        /// </summary>
        /// <param name="imageContentType">Content mimetype</param>
        /// <returns>Image type</returns>
        private ImagePartType GetImagePartType(string imageContentType)
        {
            switch (imageContentType) {
                case "image/jpeg":
                    return ImagePartType.Jpeg;
                case "image/emf":
                    return ImagePartType.Emf;
                case "image/gif":
                    return ImagePartType.Gif;
                case "image/ico":
                    return ImagePartType.Icon;
                case "image/pcx":
                    return ImagePartType.Pcx;
                case "image/png":
                    return ImagePartType.Png;
                case "image/tiff":
                    return ImagePartType.Tiff;
                case "image/wmf":
                    return ImagePartType.Wmf;
                default:
                    return ImagePartType.Bmp;
            }
        }
    }
}
