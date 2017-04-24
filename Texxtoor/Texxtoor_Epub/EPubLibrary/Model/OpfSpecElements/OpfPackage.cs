using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using Texxtoor.Models.BaseEntities.Epub;

namespace Texxtoor.BaseLibrary.EPub.Model
{

    /// <summary>
    /// The package element is the root element of the Package Document.
    /// </summary>
    [Table("Package", Schema = "Epub")]
    public class OpfPackage : EntityBase
    {

        internal static readonly XNamespace OpfNameSpace = XNamespace.Get("http://www.idpf.org/2007/opf");

        internal static OpfPackage CreatePackage(XElement element, ZipStorer gz, string opsFolder, out string ncx)
        {
            var package = new OpfPackage
            {
                Version = element.Attribute("version").Value,
                Identifier = element.Attribute("unique-identifier").Value,
                MetaData = MetaData.CreateMetaData(element.Element(OpfNameSpace + "metadata")),
                Spine = Spine.CreateSpine(element.Element(OpfNameSpace + "spine"))
            };
            var manifestElement = element.Element(OpfNameSpace + "manifest");
            var ncxElement = package.Spine.Toc;
            // we need to pull it directly from XML as this file does not become part of the items
            ncx = manifestElement
              .Elements(OpfNameSpace + "item")
              .First(item => item.Attribute("id").Value == ncxElement)
              .Attribute("href")
              .Value;
            package.Manifest = Manifest.CreateManifest(gz, manifestElement, ncxElement, opsFolder);
            package.Guide = Guide.CreateGuide(element);
            return package;
        }

        internal static XElement CreateXElement(OpfPackage package)
        {
            var xe = new XElement("package",
            new XAttribute("version", package.Version),
            new XAttribute("unique-identifier", package.Identifier)
            );
            return xe;
        }

        /// <summary>
        /// Specifies the EPUB specification version to which the Publication conforms.
        /// </summary>
        [StringLength(12)]
        [Required]
        public string Version { get; set; }

        /// <summary>
        /// An IDREF [XML] that identifies the dc:identifier element that provides the package's preferred, or primary, identifier.
        /// </summary>
        [StringLength(128)]
        public string Identifier { get; set; }

        /// <summary>
        /// Declaration mechanism for prefixes not reserved by this specification.
        /// </summary>
        [StringLength(128)]
        public string Prefix { get; set; }

        /// <summary>
        /// Specifies the language used in the contents and attribute values of the carrying element and its descendants.
        /// </summary>
        [StringLength(10)]
        public string Language { get; set; }

        /// <summary>
        /// Specifies the base text direction of the content and attribute values of the carrying element and its descendants.
        /// </summary>
        public ProgressionDirection Dir { get; set; }

        /// <summary>
        /// The ID [XML] of this element, which must be unique within the document scope.
        /// </summary>
        [StringLength(128)]
        public string PackageId { get; set; }

        /// <summary>
        /// The metadata element encapsulates Publication meta information.
        /// </summary>
        public MetaData MetaData { get; set; }

        /// <summary>
        /// The manifest element provides an exhaustive list of the Publication Resources that constitute the EPUB Publication, each represented by an item element.
        /// </summary>
        public Manifest Manifest { get; set; }

        /// <summary>
        /// The bindings element defines a set of custom handlers for media types not supported by this specification.
        /// </summary>
        public Binding Bindings { get; set; }

        public Spine Spine { get; set; }

        public Guide Guide { get; set; }

    }

}
