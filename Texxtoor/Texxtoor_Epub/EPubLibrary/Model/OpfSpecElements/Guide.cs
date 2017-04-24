using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;
using Texxtoor.Editor.Core.Extensions.Epub;

namespace Texxtoor.BaseLibrary.EPub.Model {


  /// <summary>
  /// reference href="cover.html" type="cover" title="Cover"
  /// </summary>
  [ComplexType]
  public class Guide {

    internal static Guide CreateGuide(XElement element) {
      var xguide = element.Element(OpfPackage.OpfNameSpace + "guide");
      var guide = new Guide();
      if (xguide != null) {
        var xe = xguide.Element(OpfPackage.OpfNameSpace + "reference");        
        if (xe != null) {
          guide.ReferenceHref = Helper.NullSaveString(xe.Attribute("href"));
          guide.ReferenceTitle = Helper.NullSaveString(xe.Attribute("title"));
          guide.ReferenceType = Helper.NullSaveString(xe.Attribute("type"));
        }
      }
      return guide;
    }

    internal static XElement CreateXElement(Guide g) {
      var xe = new XElement("guide",
        new XElement("reference",
          new XAttribute("href", g.ReferenceHref ?? String.Empty),
          new XAttribute("type", g.ReferenceType ?? String.Empty),
          new XAttribute("title", g.ReferenceTitle ?? String.Empty)
          )
      );
      return xe;
    }

    [StringLength(255)]
    public string ReferenceHref { get; set; }
    [StringLength(128)]
    public string ReferenceType { get; set; }
    [StringLength(255)]
    public string ReferenceTitle { get; set; }

  }

}
