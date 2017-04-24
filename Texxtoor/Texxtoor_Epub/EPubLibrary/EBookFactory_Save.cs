using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Texxtoor.BaseLibrary.EPub.Model;
using Texxtoor.Editor.Core.Extensions.Epub;
using System.Collections.Generic;
using System.Web.Hosting;
using Ionic.Zip;

namespace Texxtoor.BaseLibrary.EPub
{

    /// <summary>
    /// Create a stream from internal format.
    /// </summary>
    public static partial class EBookFactory
    {

        private static readonly XDeclaration XmlDeclaration = new XDeclaration("1.0", "UTF-8", "no");

        public static Stream SaveBookToStream(EpubBook book)
        {
            var ms = new MemoryStream(SaveBook(book));
            return ms;
        }

        public static byte[] SaveBook(EpubBook book)
        {
            byte[] data = SaveEpubToDisk(book);
            //byte[] data = SaveBookInternal(book);
            return data;
        }

        private static byte[] SaveEpubToDisk(EpubBook book)
        {
            string FileName = Guid.NewGuid().ToString();
            string PhysicalPath = Path.Combine(Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "data\\"));
            MemoryStream ms = new MemoryStream();

            using (FileStream epubfile = File.Open(PhysicalPath + "\\Epub_" + FileName + ".epub", FileMode.Create, FileAccess.ReadWrite))
            {
                using (var output = new ZipOutputStream(epubfile))
                {
                    var e = output.PutNextEntry("mimetype");
                    e.CompressionLevel = Ionic.Zlib.CompressionLevel.None;


                    Stream mimeType = new MemoryStream();
                    var mtString = "application/epub+zip";

                    output.Write(Encoding.ASCII.GetBytes(mtString), 0, mtString.Length);


                    // OPF File with three Elements: METADATA, MANIFEST, SPINE
                    // container 
                    output.PutNextEntry("META-INF/container.xml");
                    byte[] metaInf = Helper.ReadStreamToEnd(GetContainerData(book.ContainerData));
                    output.Write(metaInf, 0, metaInf.Length);

                    // <item id="ncxtoc" media-type="application/x-dtbncx+xml" href="toc.ncx"/>
                    book.PackageData.Manifest.Items.Add(new ContentFile
                    {
                        Identifier = book.PackageData.Spine.Toc,
                        Href = "toc.ncx",
                        MediaType = "application/x-dtbncx+xml"
                    });


                    output.PutNextEntry("OEBPS/content.opf");
                    byte[] opfData = Helper.ReadStreamToEnd(GetOpfData(book.PackageData));
                    output.Write(opfData, 0, opfData.Length);
                    

                    book.GetTableOfContent().HeadMetaData = new Head();
                    book.GetTableOfContent().NavMap = book.NavigationData.NavMap;
                    book.GetTableOfContent().HeadMetaData.Identifier = "isbn:9780735656680";// Guid.NewGuid().ToString();
                    book.GetTableOfContent().HeadMetaData.Creator = book.PackageData.MetaData.Creator.ToString();
                    book.GetTableOfContent().HeadMetaData.Depth = 2;
                    book.GetTableOfContent().HeadMetaData.TotalPageCount = 0;
                    book.GetTableOfContent().HeadMetaData.MaxPageNumber = 0;


                    output.PutNextEntry("OEBPS/toc.ncx");
                    byte[] ncxData = Helper.ReadStreamToEnd(GetNcxData(book.GetTableOfContent(), book.PackageData.MetaData.Title.Text));
                    output.Write(ncxData, 0, ncxData.Length);

                    // take manifest items and create remaining files
                    foreach (var item in book.GetAllFiles().Where(file => file.Href != "toc.ncx"))
                    {
                        //var msItem = new MemoryStream(item.Data);
                        //msItem.Position = 0;
                        var path = String.Format("{0}/{1}", "OEBPS", item.Href);
                        output.PutNextEntry(path);
                        output.Write(item.Data, 0, item.Data.Length);
                    }
                }
            }
            using (FileStream fileStream = File.OpenRead(PhysicalPath + "\\Epub_" + FileName + ".epub"))
            {
                MemoryStream memStream = new MemoryStream();
                memStream.SetLength(fileStream.Length);
                fileStream.Read(memStream.GetBuffer(), 0, (int)fileStream.Length);
                ms = memStream;
            }

            if (File.Exists(PhysicalPath + "\\Epub_" + FileName + ".epub"))
                File.Delete(PhysicalPath + "\\Epub_" + FileName + ".epub");

            ms.Position = 0;
            return ms.ToArray();
        }

        private static byte[] SaveBookInternal(EpubBook book)
        {
            var ms = new MemoryStream();
            using (ZipStorer gz = ZipStorer.Create(ms, ""))
            {
                // mimetype File
                Stream mimeType = new MemoryStream();
                var mtString = "application/epub+zip";
                mimeType.Write(Encoding.ASCII.GetBytes(mtString), 0, mtString.Length);
                mimeType.Position = 0;
                gz.AddStream(ZipStorer.Compression.Deflate, "mimetype", mimeType, DateTime.Now, "");

                // OPF File with three Elements: METADATA, MANIFEST, SPINE
                // container 
                Stream metaInf = GetContainerData(book.ContainerData);
                gz.AddStream(ZipStorer.Compression.Deflate, "META-INF/container.xml", metaInf, DateTime.Now, "");

                // opf        
                var opfPath = book.ContainerData.Rootfiles.First().FullPath;

                // <item id="ncxtoc" media-type="application/x-dtbncx+xml" href="toc.ncx"/>
                book.PackageData.Manifest.Items.Add(new ContentFile
                {

                    Identifier = book.PackageData.Spine.Toc,
                    Href = "toc.ncx",
                    MediaType = "application/x-dtbncx+xml"
                });

                Stream opfData = GetOpfData(book.PackageData);
                gz.AddStream(ZipStorer.Compression.Deflate, opfPath, opfData, DateTime.Now, "");

                // ncx
                //List<NavPoint> navmaplist = new List<NavPoint>();

                //for (int i = 0; i < book.PackageData.Manifest.Items.Count; i++)
                //{
                //    if (book.PackageData.Manifest.Items[i].MediaType == "application/xhtml+xml")
                //    {
                //        navmaplist.Add(new NavPoint { Identifier = book.PackageData.Manifest.Items[i].Identifier, PlayOrder = (i + 1), LabelText = System.Text.Encoding.UTF8.GetString(book.PackageData.Manifest.Items[i].Data), Content = book.PackageData.Manifest.Items[i].Href });
                //    }
                //}

                book.GetTableOfContent().HeadMetaData = new Head();
                book.GetTableOfContent().NavMap = book.NavigationData.NavMap;
                book.GetTableOfContent().HeadMetaData.Identifier = "isbn:9780735656680";// Guid.NewGuid().ToString();
                book.GetTableOfContent().HeadMetaData.Creator = book.PackageData.MetaData.Creator.ToString();
                book.GetTableOfContent().HeadMetaData.Depth = 2;
                book.GetTableOfContent().HeadMetaData.TotalPageCount = 0;
                book.GetTableOfContent().HeadMetaData.MaxPageNumber = 0;

                Stream ncxData = GetNcxData(book.GetTableOfContent(), book.PackageData.MetaData.Title.Text);
                string ncxElement = Helper.CreatePath(Path.GetDirectoryName(opfPath), "toc.ncx");
                gz.AddStream(ZipStorer.Compression.Deflate, ncxElement, ncxData, DateTime.Now, "");

                // take manifest items and create remaining files
                var oepbsPath = Path.GetDirectoryName(opfPath);
                foreach (var item in book.GetAllFiles().Where(file => file.Href != "toc.ncx"))
                {
                    var msItem = new MemoryStream(item.Data);
                    msItem.Position = 0;
                    var path = String.Format("{0}/{1}", oepbsPath, item.Href);
                    gz.AddStream(ZipStorer.Compression.Deflate, path, msItem, DateTime.Now, "");
                }
                ms.Position = 0;
                return ms.ToArray();
            }
        }

        // container.xml
        private static MemoryStream GetContainerData(Container cnt)
        {
            var opfDoc = new XDocument();
            opfDoc.Declaration = XmlDeclaration;
            opfDoc.Add(Container.CreateXElement(cnt));
            var ms = new MemoryStream();
            opfDoc.Save(ms);
            ms.Position = 0;
            return ms;
        }
        
        // opfdata.opf
        private static MemoryStream GetOpfData(OpfPackage package)
        {
            var opfDoc = new XDocument();
            opfDoc.Declaration = XmlDeclaration;
            var xe = OpfPackage.CreateXElement(package);
            xe.Add(MetaData.CreateXElement(package.MetaData));
            xe.Add(Manifest.CreateXElement(package.Manifest));
            xe.Add(Spine.CreateXElement(package.Spine));
            xe.Add(Guide.CreateXElement(package.Guide));
            opfDoc.Add(xe);
            var ms = new MemoryStream();
            opfDoc.Save(ms);
            ms.Position = 0;
            return ms;
        }
        
        // ncx.toc
        private static MemoryStream GetNcxData(Texxtoor.BaseLibrary.EPub.Model.Navigation nav, string title)
        {
            var ncxDoc = new XDocument();
            ncxDoc.Declaration = XmlDeclaration;
            var ncx = Texxtoor.BaseLibrary.EPub.Model.Navigation.CreateXElement(nav, title);
            ncxDoc.Add(ncx);
            var ms = new MemoryStream();
            ncxDoc.Save(ms);
            ms.Position = 0;
            return ms;
        }
    }
}
