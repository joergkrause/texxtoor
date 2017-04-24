using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Xml.Linq;
using Texxtoor.BaseLibrary.Core.BaseEntities;

namespace Texxtoor.BaseLibrary.EPub.Model {

  [Table("Container", Schema="Epub")]
  //[ComplexType]
  public class Container : EntityBase {

    public Container() {
      Rootfiles = new List<RootFile>(new RootFile[] { new RootFile { FullPath = "content/content.opf", MediaType = "application/oebps-package+xml" } });
    }

    #region Load and Save support  

    private static XNamespace _containerNameSpace;

    [NotMapped]
    public static XNamespace ContainerNameSpace {
      get {
        if (_containerNameSpace == null) {
          _containerNameSpace = XNamespace.Get("urn:oasis:names:tc:opendocument:xmlns:container");
        }
        return _containerNameSpace;
      }
      internal set {
        _containerNameSpace = value;
      }
    }

    public static Container CreateContainer(XElement cnt) {
      var ns = ContainerNameSpace;
      var containerQuery = cnt
        .Elements(ns + "rootfiles")
        .Select(e => new RootFile {
          FullPath = e.Element(ns + "rootfile").Attribute("full-path").Value,
          MediaType = e.Element(ns + "rootfile").Attribute("media-type").Value
        });
      return new Container() { Rootfiles = containerQuery.ToList() };
    }

    public static XElement CreateXElement(Container cnt) {
      var ns = ContainerNameSpace;
      var xe = new XElement(ns + "container",
          new XElement(ns + "rootfiles",
            cnt.Rootfiles.Select(rf =>
              new XElement(ns + "rootfile",
                new XAttribute("full-path", rf.FullPath),
                new XAttribute("media-type", rf.MediaType))
                )
             ),
          new XAttribute("version", "1.0")
          );
      return xe;
    }

    # endregion

    public IList<RootFile> Rootfiles { get; set; }

  }

  [Table("RootFiles", Schema = "Epub")]
  public class RootFile : EntityBase {

    [Required]
    [StringLength(256)]
    public string FullPath { get; set; }

    [Required]
    [StringLength(64)]
    public string MediaType { get; set; }

    public Container Container { get; set; }

  }
}
