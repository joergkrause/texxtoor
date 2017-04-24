using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;
using Texxtoor.BaseLibrary.Core.Extensions;

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
          guide.ReferenceHref = xe.Attribute("href").NullSafeString();
          guide.ReferenceTitle = xe.Attribute("title").NullSafeString();
          guide.ReferenceType = xe.Attribute("type").NullSafeString();
        }
      }
      return guide;
    }

    internal static XElement CreateXElement(Guide g) {
      var xe = new XElement(OpfPackage.OpfNameSpace + "guide",
        new XElement(OpfPackage.OpfNameSpace + "reference",
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
