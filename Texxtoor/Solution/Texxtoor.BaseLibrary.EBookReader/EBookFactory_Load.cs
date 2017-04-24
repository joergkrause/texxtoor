using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using Texxtoor.BaseLibrary.EPub.Model;
using Texxtoor.BaseLibrary.Core.Extensions;
using System.Xml;

namespace Texxtoor.BaseLibrary.EPub {

  /// <summary>
  /// Create EPUB for storage in database.
  /// </summary>
  public static partial class EBookFactory {

    public static EpubBook Create(Stream data) {
      if (data == null) return null;
      byte[] buffer = new byte[data.Length];
      data.Position = 0;
      data.Read(buffer, 0, buffer.Length);
      return Create(buffer);
    }

    public static EpubBook Create(byte[] data) {
      if (data == null) return null;
      return CreateInternal(data);
    }

    private static EpubBook CreateInternal(byte[] data) {
      var b = new EpubBook();
      try {
        using (var ms = new MemoryStream(data)) {
          using (var gz = new ZipArchive(ms)) {
            // ******************** CONTAINER.XML ********************//
            var metainf = gz.Entries.First(z => z.FullName == "META-INF/container.xml");
            using (var mscontainer = new MemoryStream()) {
              metainf.Open().CopyTo(mscontainer);
              mscontainer.Position = 0;
              var containerDoc = XDocument.Load(mscontainer);
              // Container
              b.ContainerData = Container.CreateContainer(containerDoc.Root);
            }
            // OPF (Package)
            using (var msopf = new MemoryStream()) {
              // opf folder
              var opsFolder = Path.GetDirectoryName(b.ContainerData.Rootfiles.First().FullPath).Replace("\\", "/");
              //
              metainf = gz.Entries.First(z => z.FullName == b.ContainerData.Rootfiles.First().FullPath);
              metainf.Open().CopyTo(msopf);
              msopf.Position = 0;
              // ******************** OPF ********************//
              var reader = XmlReader.Create(msopf); 
              var opfDoc = XDocument.Load(reader);
              string ncxHref;
              b.PackageData = OpfPackage.CreatePackage(opfDoc.Root, gz, opsFolder, out ncxHref);
              // metadata head
              // ******************** NCX ********************//
              using (var msncx = new MemoryStream()) {
                metainf = gz.Entries.First(z => z.FullName == opsFolder.CreatePath(ncxHref));
                metainf.Open().CopyTo(msncx);
                msncx.Position = 0;
                var ncxDocument = XDocument.Load(msncx);
                b.NavigationData = Navigation.CreateNavigation(b.PackageData.Manifest, ncxDocument);
              }
            }
            // pull from regular content for faster access using Guide information
            b.CoverDescription = b.PackageData.Guide.ReferenceTitle;
            var coverItem = b.PackageData.Manifest.Items.FirstOrDefault(i => i.Href == b.PackageData.Guide.ReferenceHref);
            if (coverItem != null) {
              b.CoverImage = coverItem.Data;
            }
          }
        }
      } catch (System.Exception) {
        return null;
      }
      return b;
    }

  }
}
