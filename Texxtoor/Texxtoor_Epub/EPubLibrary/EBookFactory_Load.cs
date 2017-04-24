using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using Texxtoor.BaseLibrary.EPub.Model;
using Texxtoor.Editor.Core.Extensions.Epub;
using System.Xml;

namespace Texxtoor.BaseLibrary.EPub
{

    /// <summary>
    /// Create EPUB for storage in database.
    /// </summary>
    public static partial class EBookFactory
    {

        public static EpubBook Create(Stream data)
        {
            if (data == null) return null;
            byte[] buffer = new byte[data.Length];
            data.Position = 0;
            data.Read(buffer, 0, buffer.Length);
            return Create(buffer);
        }

        public static EpubBook Create(byte[] data)
        {
            if (data == null) return null;
            return CreateInternal(data);
        }

        private static EpubBook CreateInternal(byte[] data)
        {
            var b = new EpubBook();
            try
            {
                using (MemoryStream ms = new MemoryStream(data))
                {
                    using (ZipStorer gz = ZipStorer.Open(ms, FileAccess.Read))
                    {
                        ZipStorer.ZipFileEntry metainf;
                        // ******************** CONTAINER.XML ********************//
                        metainf = gz.ReadCentralDir().FirstOrDefault(z => z.FilenameInZip == "META-INF/container.xml");
                        using (MemoryStream mscontainer = new MemoryStream())
                        {
                            gz.ExtractFile(metainf, mscontainer);
                            mscontainer.Position = 0;
                            XDocument containerDoc = XDocument.Load(mscontainer);
                            // Container
                            b.ContainerData = Container.CreateContainer(containerDoc.Root);
                        }
                        // OPF (Package)
                        using (MemoryStream msopf = new MemoryStream())
                        {
                            // opf folder
                            string opsFolder = Path.GetDirectoryName(b.ContainerData.Rootfiles.First().FullPath).Replace("\\", "/");
                            //
                            metainf = gz.ReadCentralDir().FirstOrDefault(z => z.FilenameInZip == b.ContainerData.Rootfiles.First().FullPath);
                            gz.ExtractFile(metainf, msopf);
                            msopf.Position = 0;
                            // ******************** OPF ********************//
                            var reader = XmlTextReader.Create(msopf);
                            XDocument opfDoc = XDocument.Load(reader);
                            string ncxHref;
                            b.PackageData = OpfPackage.CreatePackage(opfDoc.Root, gz, opsFolder, out ncxHref);
                            // metadata head
                            // ******************** NCX ********************//
                            using (MemoryStream msncx = new MemoryStream())
                            {
                                metainf = gz.ReadCentralDir().FirstOrDefault(z => z.FilenameInZip == Helper.CreatePath(opsFolder, ncxHref));
                                gz.ExtractFile(metainf, msncx);
                                msncx.Position = 0;
                                XDocument ncxDocument = XDocument.Load(msncx);
                                b.NavigationData = Navigation.CreateNavigation(b.PackageData.Manifest, ncxDocument);
                            }
                        }
                        // pull from regular content for faster access using Guide information
                        b.CoverDescription = b.PackageData.Guide.ReferenceTitle;
                        var coverItem = b.PackageData.Manifest.Items.FirstOrDefault(i => i.Href == b.PackageData.Guide.ReferenceHref);
                        if (coverItem != null)
                        {
                            b.CoverImage = coverItem.Data;
                        }
                    }
                }
            }
            catch (System.Exception)
            {
                return null;
            }
            return b;
        }

    }
}
