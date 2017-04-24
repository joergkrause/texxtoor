using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Xml.Linq;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.BaseLibrary.Core.Extensions;

namespace Texxtoor.BaseLibrary.EPub.Model {

  /// <summary>
  /// The spine element defines the default reading order of the EPUB Publication content by 
  /// defining an ordered list of manifest item references.
  /// </summary>
  [Table("Spine", Schema = "Epub")]
  public class Spine : EntityBase {

    internal static Spine CreateSpine(XElement e) {
      var spine = new Spine{
        Identifier = (e.Attribute(XmlHelper.GetAttributeName<Spine, EPubAttribute>(m => m.Identifier))).NullSafeString(),
        Toc = (e.Attribute(XmlHelper.GetAttributeName<Spine, EPubAttribute>(m => m.Toc))).NullSafeString()
      };
      var dir = (e.Attribute(XmlHelper.GetAttributeName<Spine, EPubAttribute>(m => m.PageProgressionDirection))).NullSafeString();
      if (!String.IsNullOrEmpty(dir)) {
        spine.PageProgressionDirection = (ProgressionDirection)Enum.Parse(typeof(ProgressionDirection), dir, true);
      }
      var ns = e.Document.Root.GetDefaultNamespace();
      spine.ItemRefs = e.Descendants(ns + "itemref")
          .Select(itemref => new ItemRef {
            Identifier = itemref.Attribute(XmlHelper.GetAttributeName<ItemRef, EPubAttribute>(m => m.Identifier)).NullSafeString(),
            IdRef = itemref.Attribute(XmlHelper.GetAttributeName<ItemRef, EPubAttribute>(m => m.IdRef)).NullSafeString(),
            Linear = itemref.Attribute(XmlHelper.GetAttributeName<ItemRef, EPubAttribute>(m => m.Linear)).NullSafeBool(),
            Properties = itemref.Attribute(XmlHelper.GetAttributeName<ItemRef, EPubAttribute>(m => m.Properties)).NullSafeString()
          }).ToList();
      return spine;
    }

    internal static XElement CreateXElement(Spine spine) {
      var xe = new XElement(OpfPackage.OpfNameSpace + "spine");
      foreach (var iref in spine.ItemRefs) {
        var ielement = new XElement(OpfPackage.OpfNameSpace + "itemref", new XAttribute(XmlHelper.GetAttributeName<ItemRef, EPubAttribute>(m => m.IdRef), iref.IdRef ?? String.Empty));
        if (iref.Linear.HasValue) {
          ielement.Add(new XAttribute(XmlHelper.GetAttributeName<ItemRef, EPubAttribute>(m => m.Linear), iref.Linear.Value ? "yes" : "no"));
        }
        if (!String.IsNullOrEmpty(iref.Identifier)) {
          ielement.Add(new XAttribute(XmlHelper.GetAttributeName<ItemRef, EPubAttribute>(m => m.Id), iref.Identifier));
        }
        if (!String.IsNullOrEmpty(iref.Properties)) {
          ielement.Add(new XAttribute(XmlHelper.GetAttributeName<ItemRef, EPubAttribute>(m => m.Properties), iref.Properties));
        }
        xe.Add(ielement);
      }
      xe.Add(new XAttribute("toc", spine.Toc));
      return xe;
    }

    /// <summary>
    /// The ID [XML] of this element, which must be unique within the document scope.
    /// </summary>
    [EPubAttribute("id")]
    [StringLength(128)]
    public string Identifier { get; set; }

    /// <summary>
    /// Multiple itemref elements.
    /// </summary>
    public virtual IList<ItemRef> ItemRefs { get; set; }

    /// <summary>
    /// An IDREF [XML] that identifies the manifest item that represents the superseded NCX.
    /// </summary>
    [EPubAttribute("toc")]
    [StringLength(256)]
    public string Toc { get; set; }

    /// <summary>
    /// The global direction in which the Publication content flows.
    /// </summary>
    [EPubAttribute("page-progression-direction")]
    public ProgressionDirection PageProgressionDirection { get; set; }
  }

  [Table("Spine_ItemRef", Schema = "Epub")]
  [EPubElement("itemref")]
  public class ItemRef : EntityBase {
    /// <summary>
    /// An IDREF [XML] that identifies a manifest item.
    /// </summary>
    [EPubAttribute("idref")]
    public string IdRef { get; set; }
    /// <summary>
    /// Specifies whether the referenced content is primary.
    /// </summary>
    [EPubAttribute("linear")]
    public bool? Linear { get; set; }
    /// <summary>
    /// The ID [XML] of this element, which must be unique within the document scope.
    /// </summary>
    [EPubAttribute("id")]
    public string Identifier { get; set; }
    /// <summary>
    /// A space-separated list of property values.
    /// </summary>
    [EPubAttribute("properties")]
    public string Properties { get; set; }

  }



}
